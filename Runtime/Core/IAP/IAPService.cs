#if VADE_IAP
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Security;

namespace VADE.DevTools.IAP
{
    public class IAPService
    {
        private readonly ProductData[] products;

        private StoreController storeController;
        private Dictionary<string, ProductData> productsDictionary;
        private bool isInitializing;

        public bool IsInitialized => storeController != null;

        public IAPService(IEnumerable<ProductData> products)
        {
            this.products = products.ToArray();
        }

        public async Task Initialize()
        {
            if (isInitializing) return;
            isInitializing = true;

            try
            {
                await UnityServices.InitializeAsync();

                storeController = UnityIAPServices.StoreController();
                SubscribeStoreEvents();

                await storeController.Connect();

                productsDictionary = new Dictionary<string, ProductData>();
                var productDefinitions = new List<ProductDefinition>();

                foreach (var product in products)
                {
                    product.Bind(this);
                    productsDictionary[product.StoreId] = product;
                    productDefinitions.Add(new ProductDefinition(product.StoreId, product.type));
                }

                storeController.FetchProducts(productDefinitions);
            }
            finally
            {
                isInitializing = false;
            }
        }

        private void SubscribeStoreEvents()
        {
            storeController.OnStoreConnected += OnStoreConnected;
            storeController.OnStoreDisconnected += OnStoreDisconnected;
            storeController.OnProductsFetched += OnProductsFetched;
            storeController.OnProductsFetchFailed += OnProductsFetchFailed;
            storeController.OnPurchasesFetched += OnPurchasesFetched;
            storeController.OnPurchasesFetchFailed += OnPurchasesFetchFailed;
            storeController.OnPurchasePending += OnPurchasePending;
            storeController.OnPurchaseFailed += OnPurchaseFailed;
            storeController.OnPurchaseConfirmed += OnPurchaseConfirmed;
            storeController.OnPurchaseDeferred += OnPurchaseDeferred;
        }

        private void OnStoreConnected() => Debug.Log("[IAPService] Store connected");

        private void OnStoreDisconnected(StoreConnectionFailureDescription description) =>
            Debug.LogWarning("[IAPService] Store disconnected: " + description);

        private void OnProductsFetched(List<Product> fetched)
        {
            foreach (var product in fetched)
            {
                var productData = GetProductDataById(product.definition.id);
                if (productData == null) continue;

                productData.UpdateProductView(new ProductLocalizedInfo
                {
                    title = product.metadata.localizedTitle,
                    description = product.metadata.localizedDescription,
                    price = (float)product.metadata.localizedPrice,
                    currency = product.metadata.isoCurrencyCode
                });
            }

            storeController.FetchPurchases();
        }

        private void OnProductsFetchFailed(ProductFetchFailed failure) =>
            Debug.LogWarning("[IAPService] Products fetch failed: " + failure.FailureReason);

        private void OnPurchasesFetched(Orders orders)
        {
            foreach (var order in orders.ConfirmedOrders)
                ApplyEntitlements(order);
        }

        private void OnPurchasesFetchFailed(PurchasesFetchFailureDescription failure) =>
            Debug.LogWarning("[IAPService] Purchases fetch failed: " + failure.FailureReason);

        private void ApplyEntitlements(Order order)
        {
            var items = order.CartOrdered?.Items();
            if (items == null) return;

            foreach (var item in items)
            {
                var product = item.Product;
                if (product == null) continue;

                var productData = GetProductDataById(product.definition.id);
                if (productData == null) continue;

                switch (product.definition.type)
                {
                    case ProductType.NonConsumable:
                        productData.Apply();
                        break;

                    case ProductType.Subscription:
                        var subInfo = order.Info?.PurchasedProductInfo?.FirstOrDefault()?.subscriptionInfo;
                        if (subInfo != null && subInfo.IsSubscribed() == Result.True)
                            productData.Apply();
                        break;
                }
            }
        }

        private void OnPurchasePending(PendingOrder order)
        {
            var item = order.CartOrdered?.Items()?.FirstOrDefault();
            if (item?.Product == null)
            {
                storeController.ConfirmPurchase(order);
                return;
            }

            var product = item.Product;
            var productData = GetProductDataById(product.definition.id);

            if (productData == null)
            {
                Debug.LogWarning("[IAPService] Pending purchase for unknown product: " + product.definition.id);
                storeController.ConfirmPurchase(order);
                return;
            }

            string receipt = order.Info?.Receipt;
            if (!string.IsNullOrEmpty(receipt) && !ValidatePurchase(receipt))
            {
                Debug.LogWarning("[IAPService] Receipt validation failed for: " + product.definition.id);
                return;
            }

            if (productData.type == ProductType.Consumable)
                productData.Apply(ResolveConsumableQuantity(item, receipt));
            else
                productData.Apply();

            storeController.ConfirmPurchase(order);
        }

        private static int ResolveConsumableQuantity(CartItem item, string receipt)
        {
            if (item.Quantity > 0)
                return item.Quantity;

#if UNITY_ANDROID
            if (!string.IsNullOrEmpty(receipt))
            {
                try
                {
                    var data = new GooglePurchaseData(receipt);
                    if (data.json.quantity > 0)
                        return data.json.quantity;
                }
                catch { }
            }
#endif
            return 1;
        }

        private void OnPurchaseFailed(FailedOrder order)
        {
            var productId = order.CartOrdered?.Items()?.FirstOrDefault()?.Product?.definition.id;
            Debug.LogWarning("[IAPService] Purchase of " + productId + " failed: " + order.FailureReason);
        }

        private void OnPurchaseConfirmed(Order order) { }

        private void OnPurchaseDeferred(DeferredOrder order) { }

        public void BuyProduct(ProductData productData)
        {
            if (!IsInitialized || productData == null || !productsDictionary.ContainsKey(productData.StoreId))
                return;

            storeController.PurchaseProduct(productData.StoreId);
        }

        public void RestorePurchases()
        {
            if (!IsInitialized) return;

            if (Application.platform != RuntimePlatform.IPhonePlayer && Application.platform != RuntimePlatform.OSXPlayer)
                return;

            storeController.RestoreTransactions((success, error) =>
            {
                if (!success)
                    Debug.LogWarning("[IAPService] RestorePurchases failed: " + error);
            });
        }

        private bool ValidatePurchase(string receipt)
        {
            if (string.IsNullOrEmpty(receipt)) return false;

            try
            {
#if UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE_OSX
                byte[] googleTangle;
                try
                {
                    googleTangle = GooglePlayTangle.Data();
                }
                catch (NotImplementedException)
                {
                    Debug.LogWarning("[IAPService] GooglePlayTangle не настроен — Window > Unity IAP > IAP Receipt Validation Obfuscator");
                    return true;
                }

                byte[] appleTangle = null;
#if UNITY_IOS || UNITY_STANDALONE_OSX
                try { appleTangle = AppleTangle.Data(); }
                catch (NotImplementedException) { }
#endif
                var validator = new CrossPlatformValidator(googleTangle, appleTangle, Application.identifier);
                var result = validator.Validate(receipt);
                return result != null && result.Length > 0;
#else
                return true;
#endif
            }
            catch (IAPSecurityException e)
            {
                Debug.LogWarning("[IAPService] Receipt validation exception: " + e.Message);
                return false;
            }
            catch (Exception e)
            {
                Debug.LogWarning("[IAPService] Receipt validation error: " + e.Message);
                return false;
            }
        }

        private ProductData GetProductDataById(string productId)
        {
            if (productsDictionary != null && productsDictionary.TryGetValue(productId, out var product))
                return product;

            Debug.LogWarning("[IAPService] Product not found: " + productId);
            return null;
        }
    }

    internal class GooglePurchaseData
    {
        public string inAppPurchaseData;
        public string inAppDataSignature;
        public GooglePurchaseJson json;

        [Serializable] private struct GooglePurchaseReceipt { public string Payload; }
        [Serializable] private struct GooglePurchasePayload { public string json; public string signature; }

        [Serializable]
        public struct GooglePurchaseJson
        {
            public string autoRenewing;
            public string orderId;
            public string packageName;
            public string productId;
            public string purchaseTime;
            public string purchaseState;
            public string developerPayload;
            public string purchaseToken;
            public int quantity;
        }

        public GooglePurchaseData(string receipt)
        {
            try
            {
                var purchaseReceipt = JsonUtility.FromJson<GooglePurchaseReceipt>(receipt);
                var purchasePayload = JsonUtility.FromJson<GooglePurchasePayload>(purchaseReceipt.Payload);
                json = JsonUtility.FromJson<GooglePurchaseJson>(purchasePayload.json);
                inAppPurchaseData = purchasePayload.json;
                inAppDataSignature = purchasePayload.signature;
            }
            catch
            {
                inAppPurchaseData = "";
                inAppDataSignature = "";
            }
        }
    }
}
#endif
