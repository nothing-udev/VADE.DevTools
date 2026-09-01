using System;
using System.Collections;
using UnityEngine;
using VADE.DevTools.Reactive;
#if VADE_DOTWEEN
using DG.Tweening;
#endif

namespace VADE.DevTools.UI
{

    public abstract class Window : MonoBehaviour
    {
        public bool IsVisible { get; private set; }
        protected readonly Connectable Connections = new();

        [Header("Animation Settings")]
        [SerializeField] protected bool animate = false;
        [SerializeField] protected Transform animTarget;
        [SerializeField] protected float showDuration = 0.3f;
        [SerializeField] protected float hideDuration = 0.2f;
#if VADE_DOTWEEN
        [SerializeField] protected Ease showEase = Ease.OutBack;
        [SerializeField] protected Ease hideEase = Ease.InBack;
#endif

        private Coroutine _animationRoutine;

        protected virtual void Awake()
        {
            if (animate && animTarget == null)
                animTarget = transform;
        }

        public virtual void Show(object data)
        {
            gameObject.SetActive(true);
            IsVisible = true;
            OnShow(data);

            if (animate)
                PlayShowAnimation();
        }

        public virtual void Hide()
        {
            PlayHideAnimation(() =>
            {
                OnHide();
                Connections.Dispose();
                gameObject.SetActive(false);
                IsVisible = false;
            });
        }

        protected virtual void OnDestroy()
        {
            Connections.Dispose();
        }

        public void PreloadAndDisable()
        {
            Connections.Dispose();
            gameObject.SetActive(false);
            IsVisible = false;
        }

        protected abstract void OnShow(object data);
        protected abstract void OnHide();

        protected virtual void PlayShowAnimation()
        {
            if (!animate) return;
#if VADE_DOTWEEN
            animTarget.localScale = Vector3.zero;
            animTarget.DOScale(Vector3.one, showDuration).SetEase(showEase);
#else
            RestartAnimation(ScaleRoutine(Vector3.zero, Vector3.one, showDuration, null));
#endif
        }

        protected virtual void PlayHideAnimation(Action onComplete)
        {
            if (!animate)
            {
                onComplete?.Invoke();
                return;
            }
#if VADE_DOTWEEN
            animTarget.DOScale(Vector3.zero, hideDuration).SetEase(hideEase).OnComplete(() => onComplete?.Invoke());
#else
            RestartAnimation(ScaleRoutine(animTarget.localScale, Vector3.zero, hideDuration, onComplete));
#endif
        }

#if !VADE_DOTWEEN
        private void RestartAnimation(IEnumerator routine)
        {
            if (_animationRoutine != null)
                StopCoroutine(_animationRoutine);
            _animationRoutine = StartCoroutine(routine);
        }

        private IEnumerator ScaleRoutine(Vector3 from, Vector3 to, float duration, Action onComplete)
        {
            float t = 0f;
            animTarget.localScale = from;
            while (t < duration)
            {
                t += Time.deltaTime;
                animTarget.localScale = Vector3.Lerp(from, to, duration <= 0f ? 1f : Mathf.Clamp01(t / duration));
                yield return null;
            }
            animTarget.localScale = to;
            _animationRoutine = null;
            onComplete?.Invoke();
        }
#endif
    }
}
