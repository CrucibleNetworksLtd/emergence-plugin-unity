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
        private bool isEdit;

        private void Start()
        {
            for (int i = 0; i < EmergenceState.Personas.Count; i++)
            {
                GameObject go = personaButtonPool.GetNewObject();

                go.transform.SetParent(personaScrollContents);
                go.transform.localScale = Vector3.one; // Sometimes unity breaks the size when reparenting

                // Fill with persona info
                PersonaScrollItem psi = go.GetComponent<PersonaScrollItem>();
                
                Persona persona = EmergenceState.Personas[i];
                bool selected = EmergenceState.CurrentPersona.id.Equals(persona.id);
                psi.Refresh(persona, selected);


                // Loading images
                RequestImage.Instance.AskForImage(persona.avatar.url, (url, imageTexture2D) =>
                {
                    // success callback
                    persona.AvatarImage = imageTexture2D;
                    psi.Refresh(persona, selected);
                },
                (url, error, errorCode) =>
                {
                    // error callback
                    Debug.LogError("[" + url + "] " + error + " " + errorCode);
                });
            }

            // On some awake register to this event
            PersonaScrollItem.OnSelected += PersonaScrollItem_OnSelected;

            // On the equivalent OnDestroy unregister to this event
            PersonaScrollItem.OnSelected -= PersonaScrollItem_OnSelected;

            // When done
            //personaButtonPool.ReturnUsedObject(go);


        }

        private void Awake()
        {
            addPersonaButton.onClick.AddListener(OnEditPersona);
        }

        private void OnEditPersona()
        {
            isEdit = true;
            if (isEdit)
            {
                EmergenceManager.Instance.ShowEditPersona();
            }

        }

        private void PersonaScrollItem_OnSelected(Persona persona)
        {
            // Send to the server the selected persona id, then refresh again
        }

        IEnumerator GetBalance()
        {
            Debug.Log("GetBalance request started");
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
            Debug.Log("GetAccessToken request started");
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
            Debug.Log("GetPersonas request started");
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
