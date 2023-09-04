using Cysharp.Threading.Tasks;
using EmergenceSDK.Types;
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

        private PersonaScrollItemStore items;
        private Material[] itemMaterials;

        private float timeCounter = 0.0f;
        private int count = 0; // Total items
        private int selected = 0; // Target item
        private int previousSelected; // Previous target item
        private float originalItemWidth = 0; // To avoid hardcoding the item width
        private bool refreshing = true; // For spreading FX
        private int diff; // Cached movement amount
        private int activePersonaIndex = 0;

        private const float MAX_BLUR = 30.0f;
        private const float MAX_SIZE = 4.0f;

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

        public void Refresh(int selectedIndex)
        {
            count = scrollItemsRoot.childCount;

            if (originalItemWidth == 0 && count > 0)
            {
                originalItemWidth = scrollItemsRoot.GetChild(0).GetComponent<RectTransform>().rect.width;
            }

            items = new PersonaScrollItem[scrollItemsRoot.childCount];
            itemMaterials = new Material[scrollItemsRoot.childCount];
            for (int i = 0; i < scrollItemsRoot.childCount; i++)
            {
                items[i] = scrollItemsRoot.GetChild(i).GetComponent<PersonaScrollItem>();
                itemMaterials[i] = items[i].Material;
            }

            selected = selectedIndex;
            previousSelected = selected;
            activePersonaIndex = selected;
            refreshing = true;
            GoToPosition(activePersonaIndex);
            StartAnimationAsync().Forget();
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
            items[position].FixUnityStencilBug();
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