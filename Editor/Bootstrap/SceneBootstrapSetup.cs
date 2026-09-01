using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VADE.DevTools.Editor.Utils;
using VADE.DevTools.UI;

namespace VADE.DevTools.Editor.Bootstrap
{

    [InitializeOnLoad]
    public static class SceneBootstrapSetup
    {
        static SceneBootstrapSetup()
        {
            EditorSceneManager.newSceneCreated += OnNewSceneCreated;
        }

        private static void OnNewSceneCreated(Scene scene, NewSceneSetup setup, NewSceneMode mode)
        {
            if (setup == NewSceneSetup.EmptyScene) return;
            SetupScene(scene);
        }

        [MenuItem("Tools/VADE/Setup Scene")]
        public static void SetupCurrentScene()
        {
            SetupScene(EditorSceneManager.GetActiveScene());
        }

        private static void SetupScene(Scene scene)
        {
            EnsureBootstrapPlaceholder(scene);
            EnsureWindowService(scene);
            EnsureEventSystem(scene);
        }

        private static void EnsureBootstrapPlaceholder(Scene scene)
        {
            if (FindAny<VADE.DevTools.Bootstrap.Bootstrap>() != null) return;

            var go = new GameObject("Bootstrap");
            MoveToScene(go, scene);

            Debug.Log("[VADE.DevTools] Добавлен объект 'Bootstrap'. Повесьте на него свой класс-наследник Bootstrap.");
            Selection.activeGameObject = go;
        }

        private static void EnsureWindowService(Scene scene)
        {
            if (FindAny<WindowService>() != null) return;

            var canvasGo = new GameObject("UI Root", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            MoveToScene(canvasGo, scene);

            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            var screensRoot = CreateFullStretchChild(canvasGo.transform, "ScreensRoot");
            var popupsRoot = CreateFullStretchChild(canvasGo.transform, "PopupsRoot");

            var serviceGo = new GameObject("WindowService", typeof(WindowService));
            serviceGo.transform.SetParent(canvasGo.transform, false);

            var service = serviceGo.GetComponent<WindowService>();
            var so = new SerializedObject(service);
            so.FindProperty("screensRoot").objectReferenceValue = screensRoot;
            so.FindProperty("popupsRoot").objectReferenceValue = popupsRoot;
            so.ApplyModifiedPropertiesWithoutUndo();

            Debug.Log("[VADE.DevTools] Добавлены 'UI Root' (Canvas/ScreensRoot/PopupsRoot) и 'WindowService' с уже проставленными ссылками.");
        }

        private static void EnsureEventSystem(Scene scene)
        {
            if (FindAny<EventSystem>() != null) return;

            var go = new GameObject("EventSystem", typeof(EventSystem));
            MoveToScene(go, scene);

            var inputSystemModuleType = OptionalTypeUtility.FindType("UnityEngine.InputSystem.UI.InputSystemUIInputModule");
            if (inputSystemModuleType != null)
                go.AddComponent(inputSystemModuleType);
            else
                go.AddComponent<StandaloneInputModule>();

            Debug.Log("[VADE.DevTools] Добавлен EventSystem — без него UI не получает клики/наведение.");
        }

        private static RectTransform CreateFullStretchChild(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return rt;
        }

        private static void MoveToScene(GameObject go, Scene scene)
        {
            if (go.scene != scene)
                EditorSceneManager.MoveGameObjectToScene(go, scene);
        }

        private static T FindAny<T>() where T : Object
        {
#if UNITY_2022_2_OR_NEWER
            return Object.FindAnyObjectByType<T>();
#else
            return Object.FindObjectOfType<T>();
#endif
        }
    }
}
