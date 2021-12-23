using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Emergence
{
    public class TooltipHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("UI References")]
        public RectTransform tooltipPosition;

        [Header("Tooltips")]
        [SerializeField]
        private string tooltipMessage;

        private void Awake()
        {
            if (tooltipPosition == null)
            {
                tooltipPosition = GetComponent<RectTransform>();
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            Tooltip.Instance.Show(tooltipPosition, tooltipMessage);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Tooltip.Instance.Hide();
        }
    }
}