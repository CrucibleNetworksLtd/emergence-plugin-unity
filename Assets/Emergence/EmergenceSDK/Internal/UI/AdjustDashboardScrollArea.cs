using UnityEngine;
using UnityEngine.UI;

namespace EmergenceSDK.Internal.UI
{
    public class AdjustDashboardScrollArea : MonoBehaviour
    {
        public RectTransform scrollViewRT;
        public RectTransform addPersonaButtonRT;

        public HorizontalLayoutGroup mainHLG;
        public HorizontalLayoutGroup scrollHLG;

        private RectTransform mainRT;
        private RectTransform scrollContentsRT;

        private void Awake()
        {
            mainRT = mainHLG.GetComponent<RectTransform>();
            scrollContentsRT = scrollHLG.GetComponent<RectTransform>();
        }

        private void Update()
        {
            Vector2 size = new Vector2(0.0f, scrollViewRT.sizeDelta.y);
            mainHLG.spacing = scrollHLG.spacing;

            if (scrollContentsRT.childCount > 0)
            {
                RectTransform firstChild = scrollContentsRT.GetChild(0).GetComponent<RectTransform>();
                size.x = scrollContentsRT.childCount * (firstChild.sizeDelta.x + scrollHLG.spacing);
                size.x = Mathf.Clamp(size.x, 0.0f, mainRT.rect.width - addPersonaButtonRT.rect.width - mainHLG.spacing);
            }

            scrollViewRT.sizeDelta = size;
        }
    }
}
