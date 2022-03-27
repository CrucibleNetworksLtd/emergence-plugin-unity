using UnityEngine;
using UnityEngine.UI;

namespace EmergenceSDK
{
    public class PersonaCarousel : MonoBehaviour
    {
        [Header("Configuration")]
        public float duration = 0.5f;

        [Header("UI References")]
        public Button arrowLeftButton;
        public Button arrowRightButton;
        public Transform scrollItemsRoot;

        public static PersonaCarousel Instance;

        private Transform[] items;
        private float timeCounter = 0.0f;
        private bool started = false;
        private int count = 0; // Total items
        private int selected = 0; // Target item
        private int previousSelected; // Previous target item
        private float originalItemWidth = 0; // To avoid hardcoding the item width
        private bool refreshing = true; // For spreading FX
        private int diff; // Cached movement amount
        private int activePersonaIndex = 0;

        public delegate void ArrowClicked(int index);
        public static event ArrowClicked OnArrowClicked;

        private void Awake()
        {
            Instance = this;
            arrowLeftButton.onClick.AddListener(OnArrowLeftClicked);
            arrowRightButton.onClick.AddListener(OnArrowRightClicked);
            PersonaScrollItem.OnSelected += PersonaScrollItem_OnSelected;
        }

        private void OnDestroy()
        {
            arrowLeftButton.onClick.RemoveListener(OnArrowLeftClicked);
            arrowRightButton.onClick.RemoveListener(OnArrowRightClicked);
            PersonaScrollItem.OnSelected -= PersonaScrollItem_OnSelected;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                GoToPosition(0);
            }

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                GoToPosition(count - 1);
            }
            if (!started)
            {
                return;
            }

            if (refreshing)
            {
                timeCounter += Time.deltaTime / 1.0f;
            }
            else
            {
                timeCounter += Time.deltaTime / duration;
            }

            if (timeCounter >= 1.0f)
            {
                timeCounter = 1.0f;
                started = false;
            }

            float t = Mathf.SmoothStep(0.0f, 1.0f, timeCounter);

            for (int i = 0; i < count; i++)
            {
                PositionAndScaleItem(i, t);
            }

            if (!started)
            {
                refreshing = false;
                timeCounter = 0.0f;
            }
        }

        private void OnArrowRightClicked()
        {
            int position = selected + 1;
            if (position < count)
            {
                GoToPosition(position);
                OnArrowClicked?.Invoke(position);
            }
        }

        private void OnArrowLeftClicked()
        {
            int position = selected - 1;
            if (position >= 0)
            {
                GoToPosition(position);
                OnArrowClicked?.Invoke(position);
            }
        }

        private void PersonaScrollItem_OnSelected(Persona persona, int index)
        {
            GoToPosition(index);
        }

        private void GoToPosition(int position, bool instant = false)
        {
            started = true;
            previousSelected = selected;
            diff = selected - position;
            selected = position;
            timeCounter = 0.0f;

            for (int i = 0; i < count; i++)
            {
                items[i].SetAsFirstSibling();
            }

            items[selected].SetAsLastSibling();

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
                items[index].SetSiblingIndex(order);
            }
        }

        public void Refresh(int selectedIndex)
        {
            count = scrollItemsRoot.childCount;

            if (originalItemWidth == 0 && count > 0)
            {
                originalItemWidth = scrollItemsRoot.GetChild(0).GetComponent<RectTransform>().rect.width;
            }

            items = new Transform[scrollItemsRoot.childCount];

            for (int i = 0; i < scrollItemsRoot.childCount; i++)
            {
                items[i] = scrollItemsRoot.GetChild(i);
            }

            selected = selectedIndex;
            previousSelected = selected;
            activePersonaIndex = selected;
            refreshing = true;
            GoToPosition(activePersonaIndex);
        }

        public void GoToActivePersona()
        {
            if (refreshing)
            {
                return;
            }

            GoToPosition(activePersonaIndex);
        }

        private void PositionAndScaleItem(int position, float t)
        {
            Transform item = items[position];

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
                item.localPosition = Vector2.right * t * (diff + position - selected) * separation;
            }
            else
            {
                // Carousel
                item.localPosition = Vector2.right * Mathf.Lerp(startPosition, endPosition, t) * separation;
            }

            item.localScale = Vector3.one * scale;
        }

        private float GetScalePerPosition(int position)
        {
            // These values were picked to match the reference design, can be replaced by a proper formula
            // to be used for an infinite number of items, instead of 5
            float result = 0.0f;
            switch (Mathf.Abs(position))
            {
                case 0:
                    result = 1.0f;
                    break;
                case 1:
                    result = 0.75f;
                    break;
                case 2:
                    result = 0.5f;
                    break;
                case 3:
                    result = 0.0f;
                    break;
            }
            return result;
        }

        private float GetDistancePerPosition(int position)
        {
            // These values were picked to match the reference design, can be replaced by a proper formula
            // to be used for an infinite number of items, instead of 5
            float result = 0.0f;
            switch (Mathf.Abs(position))
            {
                case 0:
                    result = 1.0f;
                    break;
                case 1:
                    result = 0.85f;
                    break;
                case 2:
                    result = 0.75f;
                    break;
                case 3:
                    result = 0.45f;
                    break;
                default:
                    result = 0.45f;
                    break;
            }
            return result;
        }
    }
}