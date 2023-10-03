using System;
using System.Collections.Generic;
using System.Linq;
using EmergenceSDK.Types;
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
        internal Persona SelectedPersona => itemPositions.First(pair => pair.Value == 0).Key.Persona;

        private Material[] itemMaterials;
        
        private int count => Items.Count; // Total items
        private int selected = 0; // Target item
        private const int previousSelected = 0; // Previous target item
        private float originalItemWidth = 0;
        
        private bool refreshing = true; // For spreading FX

        private const float MAX_BLUR = 30.0f;
        private const float MAX_SIZE = 4.0f;

        public Action<Persona> ArrowClicked;

        internal PersonaScrollItemStore Items;

        private Dictionary<PersonaScrollItem, int> itemPositions = new();

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

        private void OnArrowRightClicked() => HandleArrowClick(1);
        private void OnArrowLeftClicked() => HandleArrowClick(-1);
        
        private void HandleArrowClick(int position)
        {
            if (itemPositions.Values.Contains(position))
            {
                var keyPersona = itemPositions.First(pair => pair.Value == position).Key.Persona;
                ArrowClicked?.Invoke(keyPersona);
                GoToPosition(position);
            }
        }


        public void GoToPosition(Persona persona)
        {
            if(Items == null)
            {
                GoToPosition(0);
                return;
            }

            var personaItem = Items.FirstOrDefault(item => item.Persona.id == persona.id);
            var position = itemPositions[personaItem];
            GoToPosition(position);
        }
        
        internal void GoToPosition(int position)
        {
            selected = position;
            SetAllZOrders();

            var spacesToMove = -selected;
            itemPositions = UpdatedDesiredPositions(spacesToMove);

            foreach (var item in Items)
            {
                var newX = itemPositions[item]//The index in the list
                                * originalItemWidth //The width of the item
                                * GetDistancePerPosition(Math.Abs(itemPositions[item])); //Distance scaling here

                // Create the new anchored position tween
                var goToPosTween = new AnchoredPositionXTween()
                {
                    to = newX,
                    duration = duration // Assuming 'duration' is a predefined animation duration
                };
                var scale = GetScalePerPosition(Math.Abs(itemPositions[item]));
                var refreshScaleTween = new LocalScaleTween()
                {
                    from = Vector3.one * GetScalePerPosition(),
                    to = new Vector3(scale, scale, scale),
                    duration = duration,
                };

                // Add the tween to the item
                item.gameObject.AddTween(goToPosTween);
                item.gameObject.AddTween(refreshScaleTween);
                ApplyVisualTweens(scale, item);
            }
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
                Items[index].transform.SetSiblingIndex(order);
            }
        }

        public void Refresh()
        {
            if (originalItemWidth == 0 && count > 0)
            {
                originalItemWidth = scrollItemsRoot.GetChild(0).GetComponent<RectTransform>().rect.width;
            }

            var childCount = scrollItemsRoot.childCount;
            Items.SetPersonas(new PersonaScrollItem[childCount]);
            itemPositions.Clear();
            itemMaterials = new Material[childCount];
            for (int i = 0; i < scrollItemsRoot.childCount; i++)
            {
                Items[i] = scrollItemsRoot.GetChild(i).GetComponent<PersonaScrollItem>();
                itemMaterials[i] = Items[i].Material;
                itemPositions.Add(Items[i], i);
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

            GoToPosition(Items.GetCurrentPersonaIndex());
        }
        
        public Dictionary<PersonaScrollItem, int> UpdatedDesiredPositions(PersonaScrollItem targetItem)
        {
            // Make a copy of the original dictionary if you don't want to modify it directly.
            Dictionary<PersonaScrollItem, int> updatedItemPositions = new Dictionary<PersonaScrollItem, int>(itemPositions);

            int offset = -updatedItemPositions[targetItem];

            ShiftDesiredPositions(offset, updatedItemPositions);
            return updatedItemPositions;
        }
        
        public Dictionary<PersonaScrollItem, int> UpdatedDesiredPositions(int shiftAmount)
        {
            // Make a copy of the original dictionary if you don't want to modify it directly.
            Dictionary<PersonaScrollItem, int> updatedItemPositions = new Dictionary<PersonaScrollItem, int>(itemPositions);

            ShiftDesiredPositions(shiftAmount, updatedItemPositions);

            return updatedItemPositions;
        }

        private static void ShiftDesiredPositions(int shiftAmount, Dictionary<PersonaScrollItem, int> updatedItemPositions)
        {
            // Create a list of keys because we can't directly modify keys while iterating the dictionary.
            List<PersonaScrollItem> keys = new List<PersonaScrollItem>(updatedItemPositions.Keys);

            foreach (var key in keys)
            {
                updatedItemPositions[key] += shiftAmount;
                var updatedItemPosition = updatedItemPositions[key];
                var isInBounds = updatedItemPosition is > 2 or < -2;
                key.gameObject.SetActive(!isInBounds);
            }
        }

        private void PlayRefreshAnimation()
        {
            ResetAllItems();
            var otherItems = Items.GetNonActiveItems();
            var newPos = UpdatedDesiredPositions(Items.GetCurrentPersonaScrollItem());
            foreach (var scrollItem in otherItems)
            {
                //Get the distance between the current persona and the scroll item based on preset values using the indices
                var positionsFromCentre = newPos[scrollItem] - itemPositions[scrollItem];
                var dist = GetDistancePerPosition(Math.Abs(positionsFromCentre)) //Get dist multiplier
                            * positionsFromCentre //Get positions to move
                            * originalItemWidth; //Get the distance in pixels
                var refreshPosTween = new AnchoredPositionXTween()
                {
                    from = scrollItem.transform.localPosition.x,
                    to = dist,
                    duration = duration,
                };
                
                //Get the scale based on preset values using the indices
                var scale = GetScalePerPosition(Math.Abs(Items.GetCurrentPersonaIndex() - Items.GetIndex(scrollItem)));
                var refreshScaleTween = new LocalScaleTween()
                {
                    from = Vector3.one * GetScalePerPosition(),
                    to = new Vector3(scale, scale, scale),
                    duration = duration,
                };
                
                //Add the tweens to the scroll items
                scrollItem.gameObject.AddTween(refreshPosTween);
                scrollItem.gameObject.AddTween(refreshScaleTween);
                
                ApplyVisualTweens(scale, scrollItem);
                itemPositions = newPos;
            }
        }

        private void ResetAllItems()
        {
            SetAllZOrders();
            foreach (var item in Items)
            {
                var t = item.transform;
                t.localScale = Vector3.one;
                t.localPosition = Vector3.zero;
            }
        }

        private void ApplyVisualTweens(float scale, PersonaScrollItem scrollItem)
        {
            var blurTween = new FloatTween()
            {
                to = MAX_BLUR - MAX_BLUR * scale,
                duration = duration,
                onUpdate = (_, value) => { scrollItem.Material.SetFloat("_BlurAmount", value); }
            };

            var sizeTween = new FloatTween()
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
            recalculateMaskingTween.onEnd += (_) => refreshing = false;
            recalculateMaskingTween.onCancel += (_) => refreshing = false;
            scrollItem.gameObject.AddTween(blurTween);
            scrollItem.gameObject.AddTween(sizeTween);
            scrollItem.gameObject.AddTween(recalculateMaskingTween);
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