using UnityEngine;
using UnityEngine.UI;

namespace VADE.DevTools.Extensions
{
    public static class ScrollRectExtensions
    {
        public static void ScrollToItemHorizontal(this ScrollRect scrollRect, RectTransform targetItem)
        {
            Canvas.ForceUpdateCanvases();

            RectTransform content = scrollRect.content;
            RectTransform viewport = scrollRect.viewport;

            Vector2 itemLocalPos = (Vector2)content.InverseTransformPoint(content.position) -
                                (Vector2)content.InverseTransformPoint(targetItem.position);

            float contentWidth = content.rect.width;
            float viewportWidth = viewport.rect.width;

            float normalizedPos = Mathf.Clamp01((-itemLocalPos.x - (viewportWidth * .5f)) / (contentWidth - viewportWidth));
            scrollRect.horizontalNormalizedPosition = normalizedPos;
        }
    }
}
