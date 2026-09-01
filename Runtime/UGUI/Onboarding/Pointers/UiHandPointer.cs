using System.Collections;
using UnityEngine;
using UnityEngine.UI;
#if VADE_DOTWEEN
using DG.Tweening;
#endif

namespace VADE.DevTools.Onboarding
{
    public enum HandAnimation { None, Click, SlideVertical, SlideHorizontal }

    [RequireComponent(typeof(RectTransform))]
    public class UiHandPointer : MonoBehaviour
    {
        [SerializeField] private Canvas targetCanvas;

        private RectTransform hand;
        private Image img;
        private RectTransform target;
        private Transform parentTransform;
        private HandAnimation animationType;
        private RectTransform lastTarget;
        private Vector3 baseLocalPos;
        private Vector3 baseScale;

#if VADE_DOTWEEN
        private Tween currentTween;
#else
        private Coroutine currentAnimRoutine;
#endif

        private void Awake()
        {
            hand = GetComponent<RectTransform>();
            img = GetComponent<Image>();
            parentTransform = transform.parent;
            Hide();
        }

        private void Start()
        {
            if (OnboardingService.Instance != null)
                OnboardingService.Instance.SetupUIHand(this);
        }

        public void Show(RectTransform targetRect, HandAnimation animation = HandAnimation.None)
        {
            if (!gameObject.activeInHierarchy) return;
            StartCoroutine(DelayedShow(targetRect, animation));
        }

        private IEnumerator DelayedShow(RectTransform targetRect, HandAnimation animation)
        {
            yield return null;

            target = targetRect;
            lastTarget = targetRect;

            transform.SetParent(target);
            img.enabled = true;
            hand.position = target.position;

            animationType = animation;
            baseLocalPos = hand.localPosition;
            baseScale = hand.localScale;

            PlayAnimation(animation);
        }

        public void VisibilityState(bool state)
        {
            if (state)
            {
                if (lastTarget != null) ShowLast();
            }
            else
            {
                Hide();
            }
        }

        public void ShowLast()
        {
            img.enabled = true;
            hand.position = lastTarget.position;
            baseLocalPos = hand.localPosition;
            baseScale = hand.localScale;
            PlayAnimation(animationType);
        }

        public void Hide()
        {
            target = null;
            img.enabled = false;
            transform.SetParent(parentTransform);

            StopCurrentAnimation();

            hand.localScale = Vector3.one;
            hand.localPosition = Vector3.zero;
        }

        private void StopCurrentAnimation()
        {
#if VADE_DOTWEEN
            if (currentTween != null && currentTween.IsActive())
            {
                currentTween.Kill();
                currentTween = null;
            }
#else
            if (currentAnimRoutine != null)
            {
                StopCoroutine(currentAnimRoutine);
                currentAnimRoutine = null;
            }
#endif
        }

        private void PlayAnimation(HandAnimation type)
        {
            StopCurrentAnimation();

            hand.localPosition = baseLocalPos;
            hand.localScale = baseScale;

            switch (type)
            {
                case HandAnimation.None:
                    break;

                case HandAnimation.Click:
                    hand.localPosition += new Vector3(15f, -15f, 0f);
#if VADE_DOTWEEN
                    currentTween = hand.DOScale(0.8f, 0.2f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutQuad);
#else
                    currentAnimRoutine = StartCoroutine(YoyoScale(baseScale, 0.8f, 0.2f));
#endif
                    break;

                case HandAnimation.SlideVertical:
#if VADE_DOTWEEN
                    currentTween = hand.DOLocalMoveY(baseLocalPos.y + 200f, 1f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutQuad);
#else
                    currentAnimRoutine = StartCoroutine(YoyoLocalPosition(baseLocalPos, baseLocalPos + new Vector3(0f, 200f, 0f), 1f));
#endif
                    break;

                case HandAnimation.SlideHorizontal:
#if VADE_DOTWEEN
                    currentTween = hand.DOLocalMoveX(baseLocalPos.x + 250f, 1f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutQuad);
#else
                    currentAnimRoutine = StartCoroutine(YoyoLocalPosition(baseLocalPos, baseLocalPos + new Vector3(250f, 0f, 0f), 1f));
#endif
                    break;
            }
        }

#if !VADE_DOTWEEN
        private IEnumerator YoyoLocalPosition(Vector3 from, Vector3 to, float duration)
        {
            while (true)
            {
                yield return Lerp(from, to, duration, false);
                yield return Lerp(to, from, duration, false);
            }
        }

        private IEnumerator YoyoScale(Vector3 baseScaleValue, float multiplier, float duration)
        {
            Vector3 targetScale = baseScaleValue * multiplier;
            while (true)
            {
                yield return Lerp(baseScaleValue, targetScale, duration, true);
                yield return Lerp(targetScale, baseScaleValue, duration, true);
            }
        }

        private IEnumerator Lerp(Vector3 from, Vector3 to, float duration, bool isScale)
        {
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / duration));
                if (isScale) hand.localScale = Vector3.Lerp(from, to, k);
                else hand.localPosition = Vector3.Lerp(from, to, k);
                yield return null;
            }
        }
#endif
    }
}
