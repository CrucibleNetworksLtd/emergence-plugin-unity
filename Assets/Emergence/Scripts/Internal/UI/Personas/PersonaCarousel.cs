using System;
using Cysharp.Threading.Tasks;
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
        
        private async UniTaskVoid StartAnimationAsync()
        {
            float t = 0.0f;

            while (t < 1.0f)
            {
                if (refreshing)
                {
                    timeCounter += Time.deltaTime;
                }
                else
                {
                    timeCounter += Time.deltaTime / duration;
                }

                if (timeCounter >= 1.0f)
                {
                    timeCounter = 1.0f;
                }

                t = Mathf.SmoothStep(0.0f, 1.0f, timeCounter);

                for (int i = 0; i < count; i++)
                {
                    PositionAndScaleItem(i, t);
                }

                if (timeCounter >= 1.0f)
                {
                    refreshing = false;
                    timeCounter = 0.0f;
                    break;
                }

                await UniTask.Yield(PlayerLoopTiming.Update);
            }
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

            SetZOrder(selected + 1, count - 2);
            SetZOrder(selected - 1, count - 3);
            SetZOrder(selected + 2, count - 4);
            SetZOrder(selected - 2, count - 5);
            SetZOrder(selected + 3, count - 6);
            SetZOrder(selected - 3, count - 7);
            StartAnimationAsync().Forget();
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
            StartAnimationAsync().Forget();
        }

        public void GoToActivePersona()
        {
            if (refreshing)
            {
                return;
            }

            GoToPosition(items.GetCurrentPersonaIndex());
        }

        private void PositionAndScaleItem(int position, float t)
        {
            Transform itemTransform = items[position].transform;

            int startPosition = position - previousSelected;
            int endPosition = position - selected;

            float startScale = GetScalePerPosition(startPosition);
            float endScale = GetScalePerPosition(endPosition);
            float scale = Mathf.Lerp(startScale, endScale, t);

            float startSeparation = GetDistancePerPosition(startPosition);
            float endSeparation = GetDistancePerPosition(endPosition);
            float separation = Mathf.Lerp(startSeparation, endSeparation, t) * originalItemWidth;

            if (refreshing)
            {
                // Spreading effect
                itemTransform.localPosition = Vector2.right * (t * (diff + position - selected) * separation);
            }
            else
            {
                // Carousel
                itemTransform.localPosition = Vector2.right * (Mathf.Lerp(startPosition, endPosition, t) * separation);
            }

            itemTransform.localScale = Vector3.one * scale;
            itemMaterials[position].SetFloat("_BlurAmount", MAX_BLUR - MAX_BLUR * scale);
            itemMaterials[position].SetFloat("_Size", 1.0f + (MAX_SIZE - MAX_SIZE * scale));
            items[position].RecalculateMasking();
        }

        private float GetScalePerPosition(int position)
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