using UnityEngine;

namespace EmergenceSDK.Internal.UI.Personas
{
    public class PersonaCreationEditingStatusWidget : MonoBehaviour
    {
        public GameObject[] EditGOs;
        public GameObject[] CreateGOs;
        
        public void HideAll()
        {
            foreach (var go in EditGOs)
            {
                go.SetActive(false);
            }
            
            foreach (var go in CreateGOs)
            {
                go.SetActive(false);
            }
        }
        
        public void SetVisible(bool creating)
        {
            foreach (var go in CreateGOs)
            {
                go.SetActive(creating);
            }
            foreach (var go in EditGOs)
            {
                go.SetActive(!creating);
            }
        }
    }
}