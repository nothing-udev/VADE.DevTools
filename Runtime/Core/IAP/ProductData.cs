#if VADE_IAP
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Purchasing;
using VADE.DevTools.Reactive;

namespace VADE.DevTools.IAP
{
    [Serializable]
    public struct ProductLocalizedInfo
    {
        public Sprite icon;
        public string title;
        public string description;
        public float price;
        public string currency;
        public string TextPrice => $"{price} {currency}";
    }

    [CreateAssetMenu(fileName = "New Product", menuName = "Configs/VADE/IAP/Product")]
    public class ProductData : ScriptableObject
    {
        public string id;
        public ProductType type;
        public Sprite icon;
        public UnityEvent onProductPurchased;

        private ProductLocalizedInfo localizedInfo;
        private IAPService iapService;
        private readonly Reactive<bool> productPurchased = new(false);

        public string StoreId { get; private set; }

        public bool IsPurchased => type != ProductType.Consumable && productPurchased.value;

        public void Bind(IAPService service)
        {
            iapService = service;
            StoreId = Application.identifier + "." + id;
        }

        public void UpdateProductView(ProductLocalizedInfo info)
        {
            info.icon = icon;
            localizedInfo = info;
        }

        public void Apply(int quantity = 1)
        {
            for (int i = 0; i < quantity; i++)
                onProductPurchased?.Invoke();

            productPurchased.value = true;
        }

        public void Purchase() => iapService.BuyProduct(this);

        public ProductLocalizedInfo GetLocalizedInfo() => localizedInfo;

        public IDisposable Subscribe(Action action) => onProductPurchased.Subscribe(action);

        public IDisposable SubscribeOnce(Action action)
        {
            if (productPurchased.value)
            {
                action.Invoke();
                return null;
            }

            return productPurchased.Subscribe(purchased =>
            {
                if (purchased) action.Invoke();
            });
        }
    }
}
#endif
