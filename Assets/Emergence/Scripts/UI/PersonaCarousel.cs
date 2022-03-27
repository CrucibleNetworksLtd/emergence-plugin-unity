using UnityEngine;
using UnityEngine.UI;

namespace EmergenceSDK
{
    public class PersonaCarousel : MonoBehaviour
    {
        [Header("Configuration")]
        public float duration = 0.5f;
        public float spacing = 30.0f;

        [Header("UI References")]
        public Button arrowLeftButton;
        public Button arrowRightButton;
        public Transform scrollItemsRoot;

        private float timeCounter = 0.0f;
        private bool started = false;
        private int count = 0;
        private int selected = 0;
        private float originalItemWidth = 0;
        private bool refreshing = true;
        private int diff;
        private int previousSelected;

        public static PersonaCarousel Instance;
        private Transform[] items;


        // 1 - scroll normal
        // 2 - centrado en selected
        // 3 - escala segun position
        // 4 - distancia segun position
        // 5 - maximo visible segun ancho

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
                GoToPosition(1);
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                GoToPosition(2);
            }

            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                GoToPosition(3);
            }

            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                GoToPosition(4);
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
            }
        }

        private void OnArrowLeftClicked()
        {
            int position = selected - 1;
            if (position >= 0)
            {
                GoToPosition(position);
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
            refreshing = true;
            GoToPosition(selectedIndex);
        }

        private void PositionAndScaleItem(int position, float t)
        {
            Transform item = items[position];

            int startPosition = position - previousSelected;
            int endPosition = position - selected;

            float startScale = GetScaleForPosition(startPosition);
            float endScale = GetScaleForPosition(endPosition);
            float scale = Mathf.Lerp(startScale, endScale, t);

            float separation = scale * originalItemWidth + spacing;

            
            switch (Mathf.Abs(startPosition))
            {
                case 1:
                    /*startScale = GetScaleForPosition(startPosition + 1);
                    endScale = GetScaleForPosition(endPosition + 1);
                    separation += Mathf.Lerp(startScale, endScale, t) * originalItemWidth + spacing;*/
                    separation += originalItemWidth * dist0;
                    break;
                case 2:
                    /*startScale = GetScaleForPosition(startPosition + 1);
                    endScale = GetScaleForPosition(endPosition + 1);
                    separation += Mathf.Lerp(startScale, endScale, t) * originalItemWidth + spacing;

                    startScale = GetScaleForPosition(startPosition + 2);
                    endScale = GetScaleForPosition(endPosition + 2);
                    separation += Mathf.Lerp(startScale, endScale, t) * originalItemWidth + spacing;*/
                    separation += originalItemWidth * dist1;
                    break;
                case 3:
                    separation += originalItemWidth * dist2;
                    break;
            }
            
            if (refreshing)
            {
                item.localPosition = Vector2.right * t * (diff + position - selected) * separation;
            }
            else
            {
                item.localPosition = Vector2.right * Mathf.Lerp(startPosition, endPosition, t) * separation; //* (originalItemWidth + spacing);
            }

            item.localScale = Vector3.one * scale;
        }

        private float GetScaleForPosition(int position)
        {
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
        [Range(0.0f, 4.0f)]
        public float dist0 = 1.0f;
        [Range(0.0f, 4.0f)]
        public float dist1 = 0.8f;
        [Range(0.0f, 4.0f)]
        public float dist2 = 0.7f;
        [Range(0.0f, 4.0f)]
        public float dist3 = 0.7f;
    }
}