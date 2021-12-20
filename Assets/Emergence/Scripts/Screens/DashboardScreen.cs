using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
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

        NetworkManager networkManager = new NetworkManager();
        private string currentAccessToken;

        private List<Persona> personas;

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

        IEnumerator GetBalance()
        {
            UnityEngine.Debug.Log("GetBalance request started");
            string uri = networkManager.APIBase + "getbalance";

            using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
            {
                yield return webRequest.SendWebRequest();
                if (webRequest.responseCode == 200)
                {
                    //parse json and get the balance from the result
                    StartCoroutine(GetAccessToken());
                }

            }
        }

        IEnumerator GetAccessToken()
        {
            UnityEngine.Debug.Log("GetAccessToken request started");
            string uri = networkManager.APIBase + "get-access-token";
            ///save access token
            using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
            {
                yield return webRequest.SendWebRequest();
                if (webRequest.responseCode == 200) 
                {
                    //parse json and get access token value
                    currentAccessToken = webRequest.GetResponseHeader("accessToken");
                    StartCoroutine(GetPersonas());

                }
            }
        }

        IEnumerator GetPersonas()
        {
            //throw new NotImplementedException();
            UnityEngine.Debug.Log("GetPersonas request started");
            string uri = networkManager.DatabaseAPIPrivate + "personas";

            using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
            {
                webRequest.SetRequestHeader("Authorization", currentAccessToken);
                yield return webRequest.SendWebRequest();
                if (webRequest.responseCode == 200)
                {
                    //parse json and get personas
                    //personas JsonUtility.FromJson
                }

            }
        }

    }
}
