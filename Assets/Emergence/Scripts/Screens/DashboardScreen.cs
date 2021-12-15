using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Emergence
{
    public class DashboardScreen : MonoBehaviour
    {
        [Header("UI References")]
        public Transform personaScrollContents;
        public Button addPersonaButton;

        [Header("Utilities")]
        [SerializeField]
        private Pool personaButtonPool;

        private void Start()
        {
            // TODO Delete this
            // Pool usage example
            GameObject go = personaButtonPool.GetNewObject();

            go.transform.SetParent(personaScrollContents);
            go.transform.localScale = Vector3.one; // Sometimes unity breaks the size when reparenting

            // Fill with persona info
            PersonaScrollItem psi = go.GetComponent<PersonaScrollItem>();

            Persona persona = new Persona();
            bool selected = true;
            psi.Refresh(persona, selected);

            // On some awake register to this event
            PersonaScrollItem.OnSelected += PersonaScrollItem_OnSelected;

            // On the equivalent OnDestroy unregister to this event
            PersonaScrollItem.OnSelected -= PersonaScrollItem_OnSelected;

            // When done
            personaButtonPool.ReturnUsedObject(go);

            // Loading images
            RequestImage.Instance.AskForImage("url", (url, imageTexture2D) =>
                {
                    // success callback
                },
                (url, error, errorCode) =>
                {
                    // error callback
                });
        }

        private void PersonaScrollItem_OnSelected(Persona persona)
        {
            // Send to the server the selected persona id, then refresh again
        }
    }
}
