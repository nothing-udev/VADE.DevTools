using System;
using UnityEngine;

namespace VADE.DevTools.Onboarding
{
    public class WorldArrowPointer : MonoBehaviour
    {
        [SerializeField] private bool flatRotation = true;
        [SerializeField] private float hideDistance = 2f;
        [SerializeField] private Transform arrowView;

        public event Action<Transform> TargetChanged;

        private Camera cam;
        private bool resetWhenNear;
        private float maxShowDistance = Mathf.Infinity;
        private Transform target;
        private bool active;

        private void Awake()
        {
            cam = Camera.main;
            if (OnboardingService.Instance != null)
                OnboardingService.Instance.SetupWorldArrow(this);
            Hide();
        }

        public bool IsTargeting(Transform t) => active && target == t;

        public void Show(Transform newTarget)
        {
            active = true;
            SetTarget(newTarget);
            SetArrowVisible(true);
        }

        public void Hide()
        {
            active = false;
            SetArrowVisible(false);
        }

        public void DisableArrow()
        {
            SetTarget(null);
            Hide();
        }

        private void LateUpdate()
        {
            if (!active || target == null || cam == null) return;

            float distance = Vector3.Distance(transform.position, target.position);
            if (ShouldHideArrow(distance)) return;

            UpdateArrowRotation();
        }

        private bool ShouldHideArrow(float distance)
        {
            if (distance > maxShowDistance)
            {
                SetArrowVisible(false);
                return true;
            }

            if (distance <= hideDistance)
            {
                if (resetWhenNear) SetTarget(null);
                else SetArrowVisible(false);
                return true;
            }

            SetArrowVisible(true);
            return false;
        }

        private void UpdateArrowRotation()
        {
            Vector3 direction = flatRotation
                ? new Vector3(target.position.x, transform.position.y, target.position.z) - transform.position
                : target.position - transform.position;

            if (direction.sqrMagnitude <= 0.0001f) return;
            transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        }

        public void SetTarget(Transform newTarget, bool resetNear = false, float maxDistance = Mathf.Infinity)
        {
            if (target != newTarget)
                TargetChanged?.Invoke(newTarget);

            target = newTarget;
            resetWhenNear = resetNear;
            maxShowDistance = maxDistance;

            if (target == null)
                SetArrowVisible(false);
        }

        private void SetArrowVisible(bool visible)
        {
            if (arrowView != null && arrowView.gameObject.activeSelf != visible)
                arrowView.gameObject.SetActive(visible);
        }
    }
}
