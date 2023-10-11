using UnityEngine;

namespace EmergenceSDK.Internal.UI.Personas
{
    public class PersonaCreationEditingStatusWidget : MonoBehaviour
    {
        [SerializeField]
        private GameObject createStepOne;
        [SerializeField]
        private GameObject createStepTwo;
        
        [SerializeField]
        private GameObject editStepOne;
        [SerializeField]
        private GameObject editStepTwo;
        
        public void SetStepVisible(bool stepOne, bool creating)
        {
            if (stepOne && creating)
            {
                createStepOne.SetActive(true);
                HideAllBut(createStepOne);
            }
            else if (!stepOne && creating)
            {
                createStepTwo.SetActive(true);
                HideAllBut(createStepTwo);
            }
            else if (stepOne)
            {
                editStepOne.SetActive(true);
                HideAllBut(editStepOne);
            }
            else
            {
                editStepTwo.SetActive(true);
                HideAllBut(editStepTwo);
            }
        }
        
        private void HideAllBut(GameObject obj)
        {
            createStepOne.SetActive(createStepOne == obj);
            createStepTwo.SetActive(createStepTwo == obj);
            editStepOne.SetActive(editStepOne == obj);
            editStepTwo.SetActive(editStepTwo == obj);
        }
    }
}