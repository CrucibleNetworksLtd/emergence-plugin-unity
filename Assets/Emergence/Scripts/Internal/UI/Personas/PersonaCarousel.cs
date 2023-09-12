using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Tweens;
using UnityEngine;
using UnityEngine.UI;

namespace EmergenceSDK.Internal.UI.Personas
{
    public class PersonaCarousel : MonoBehaviour
    {
        [Header("Configuration")]
        [Range(0.05f,10f)]
        public float duration = 0.5f;

        [Header("UI References")]
        public Button arrowLeftButton;
        public Button arrowRightButton;
        public Transform scrollItemsRoot;

        public static PersonaCarousel Instance;
        internal int SelectedIndex => selected;

        private Material[] itemMaterials;

        private float timeCounter = 0.0f;
        private int count => items.Count; // Total items
        private int selected = 0; // Target item
        private int previousSelected; // Previous target item
        private float originalItemWidth = 0;
        
        private bool refreshing = true; // For spreading FX
        private int diff; // Cached movement amount

        private const float MAX_BLUR = 30.0f;
        private const float MAX_SIZE = 4.0f;

        public Action<int> ArrowClicked;

        internal PersonaScrollItemStore items;
        
        private List<TweenInstance> tweens = new List<TweenInstance>();

        private void Awake()
        {
            Instance = this;
            arrowLeftButton.onClick.AddListener(OnArrowLeftClicked);
            arrowRightButton.onClick.AddListener(OnArrowRightClicked);
        }

        private void OnDestroy()
        {
            arrowLeftButton.onClick.RemoveListener(OnArrowLeftClicked);
            arrowRightButton.onClick.RemoveListener(OnArrowRightClicked);
        }

        private void OnArrowRightClicked()
        {
            int position = selected + 1;
            if (position < count)
            {
                selected = position;
                ArrowClicked?.Invoke(position);
            }
        }

        private void OnArrowLeftClicked()
        {
            int position = selected - 1;
            if (position >= 0)
            {
                selected = position;
                ArrowClicked?.Invoke(position);
            }
        }

        internal void GoToPosition(int position)
        { 
            previousSelected = selected;
            diff = selected - position;
            selected = position;
            timeCounter = 0.0f;

            for (int i = 0; i < count; i++)
            {
                items[i].transform.SetAsFirstSibling();
            }

            if (count > 0)
            {
                items[selected].transform.SetAsLastSibling();
            }

            SetAllZOrders();
            //StartAnimationAsync().Forget();
        }

        private void SetAllZOrders()
        {
            SetZOrder(selected + 1, count - 2);
            SetZOrder(selected - 1, count - 3);
            SetZOrder(selected + 2, count - 4);
            SetZOrder(selected - 2, count - 5);
            SetZOrder(selected + 3, count - 6);
            SetZOrder(selected - 3, count - 7);
        }

        private void SetZOrder(int index, int order)
        {
            if (index < count && index > 0)
            {
                items[index].transform.SetSiblingIndex(order);
            }
        }

        public void Refresh()
        {
            if (originalItemWidth == 0 && count > 0)
            {
                originalItemWidth = scrollItemsRoot.GetChild(0).GetComponent<RectTransform>().rect.width;
            }

            var childCount = scrollItemsRoot.childCount;
            items.SetPersonas(new PersonaScrollItem[childCount]);
            itemMaterials = new Material[childCount];
            for (int i = 0; i < scrollItemsRoot.childCount; i++)
            {
                items[i] = scrollItemsRoot.GetChild(i).GetComponent<PersonaScrollItem>();
                itemMaterials[i] = items[i].Material;
            }

            refreshing = true;
            GoToActivePersona();
            PlayRefreshAnimation();
        }

        public void GoToActivePersona()
        {
            if (refreshing)
            {
                return;
            }

            GoToPosition(items.GetCurrentPersonaIndex());
        }
        
        private void PlayRefreshAnimation()
        {
            SetAllZOrders();
            var otherItems = items.GetNonActiveItems();
            foreach (var scrollItem in otherItems)
            {
                //Get the distance between the current persona and the scroll item based on preset values using the indices
                var dist = GetDistancePerPosition(Math.Abs(items.GetCurrentPersonaIndex()-items.GetIndex(scrollItem))) //Get dist multiplier
                            * items.GetCurrentPersonaIndex() - items.GetIndex(scrollItem) > 0 ? 1 : -1 //Get direction
                            * originalItemWidth; //Get the distance in pixels
                var refreshPosTween = new AnchoredPositionXTween()
                {
                    from = scrollItem.transform.localPosition.x,
                    to = dist,
                    duration = duration,
                };
                
                //Get the scale based on preset values using the indices
                var scale = GetScalePerPosition(Math.Abs(items.GetCurrentPersonaIndex() - items.GetIndex(scrollItem)));
                var refreshScaleTween = new LocalScaleTween()
                {
                    from = Vector3.one * GetScalePerPosition(),
                    to = new Vector3(scale, scale, scale),
                    duration = duration,
                };
                
                var refreshBlurTween = new FloatTween()
                {
                    to = MAX_BLUR - MAX_BLUR * scale,
                    duration = duration,
                    onUpdate = (_, value) => { scrollItem.Material.SetFloat("_BlurAmount", value); }
                };
                
                var refreshSizeTween = new FloatTween()
                {
                    to = 1.0f + (MAX_SIZE - MAX_SIZE * scale),
                    duration = duration,
                    onUpdate = (_, value) => { scrollItem.Material.SetFloat("_Size", value); }
                };
                
                var recalculateMaskingTween = new FloatTween()
                {
                    duration = duration,
                    onUpdate = (_, __) => { scrollItem.RecalculateMasking(); }
                };
                
                //Add the tweens to the scroll items
                tweens.Add(scrollItem.gameObject.AddTween(refreshPosTween));
                tweens.Add(scrollItem.gameObject.AddTween(refreshScaleTween));
                tweens.Add(scrollItem.gameObject.AddTween(refreshBlurTween));
                tweens.Add(scrollItem.gameObject.AddTween(refreshSizeTween));
                tweens.Add(scrollItem.gameObject.AddTween(recalculateMaskingTween));
            }
        }

        private void PlayGoToAnimation()
        {
            
        }
        
        //Default values should guarantee that the smallest value is returns
        private float GetScalePerPosition(int position = -1)
        {
            // These values were picked to match the reference design
            switch (Mathf.Abs(position))
            {
                case 0: return 1.0f;
                case 1: return 0.75f;
                case 2: return 0.5f;
                default: return 0.45f;
            }
        }

        private float GetDistancePerPosition(int position)
        {
            // These values were picked to match the reference design
            switch (Mathf.Abs(position))
            {
                case 0: return 1.0f;
                case 1: return 0.85f;
                case 2: return 0.75f;
                default: return 0.45f;
            }
        }
    }
}