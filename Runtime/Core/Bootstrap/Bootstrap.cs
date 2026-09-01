using System;
using UnityEngine;

namespace VADE.DevTools.Bootstrap
{

    public abstract class Bootstrap : MonoBehaviour
    {
        private static Bootstrap _instance;
        public static Bootstrap Instance => _instance;

        [SerializeField] private bool dontDestroyOnLoad = true;

        public bool IsInitialized { get; private set; }

        public event Action Initialized;

        protected virtual void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Debug.LogWarning("[Bootstrap] На сцене уже есть активный Bootstrap — дублирующийся объект будет уничтожен.", this);
                Destroy(gameObject);
                return;
            }

            _instance = this;

            if (dontDestroyOnLoad)
                DontDestroyOnLoad(gameObject);

            RegisterDependencies();
            Initialize(OnInitializeComplete);
        }

        protected virtual void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }

        protected abstract void RegisterDependencies();

        protected virtual void Initialize() { }

        protected virtual void Initialize(Action onComplete)
        {
            Initialize();
            onComplete?.Invoke();
        }

        private void OnInitializeComplete()
        {
            IsInitialized = true;
            Initialized?.Invoke();
        }
    }
}
