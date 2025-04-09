using UnityEngine;

namespace TestProject.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaRect : MonoBehaviour
    {
        private RectTransform rectTransform;
        private Rect lastSafeArea;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            Refresh();
        }

        private void LateUpdate()
        {
            if (lastSafeArea != Screen.safeArea)
            {
                lastSafeArea = Screen.safeArea;
                Refresh();
            }
        }

        public void Refresh()
        {
            var anchorMin = lastSafeArea.position;
            var anchorMax = lastSafeArea.position + lastSafeArea.size;
            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
        }
    }
}
