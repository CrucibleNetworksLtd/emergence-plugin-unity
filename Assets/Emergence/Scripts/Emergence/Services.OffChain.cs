using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;

namespace EmergenceSDK
{
    public partial class Services
    {
        #region AWS API

        #region GetPersonas

        public delegate void SuccessPersonas(List<Persona> personas, Persona currentPersona);
        public void GetPersonas(SuccessPersonas success, GenericError error)
        {
            if (!CheckEnv()) { return; }
            StartCoroutine(CoroutineGetPersonas(success, error));
        }

        private IEnumerator CoroutineGetPersonas(SuccessPersonas success, GenericError error)
        {
            Debug.Log("GetPersonas request started");
            string url = envValues.databaseAPIPrivate + "personas";

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.SetRequestHeader("Authorization", currentAccessToken);
                yield return request.SendWebRequest();
                PrintRequestResult("GetPersonas", request);

                if (RequestError(request))
                {
                    error?.Invoke(request.error, request.responseCode);
                }
                else
                {
                    PersonasResponse personasResponse = SerializationHelper.Deserialize<PersonasResponse>(request.downloadHandler.text);
                    success?.Invoke(personasResponse.personas, personasResponse.personas.FirstOrDefault(p => p.id == personasResponse.selected));
                }
            }
        }

        #endregion GetPersonas

        #region CreatePersona

        public delegate void SuccessCreatePersona();
        public void CreatePersona(Persona persona, SuccessCreatePersona success, GenericError error)
        {
            if (!CheckEnv()) { return; }
            StartCoroutine(CoroutineCreatePersona(persona, success, error));
        }

        private IEnumerator CoroutineCreatePersona(Persona persona, SuccessCreatePersona success, GenericError error)
        {
            Debug.Log("CreatePersona request started");
            string jsonPersona = SerializationHelper.Serialize(persona);
            Debug.Log("Json Persona: " + jsonPersona);
            Debug.Log("currentAccessToken: " + currentAccessToken);

            string url = envValues.databaseAPIPrivate + "persona";

            using (UnityWebRequest request = UnityWebRequest.Post(url, string.Empty))
            {
                request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonPersona));
                request.uploadHandler.contentType = "application/json";

                request.SetRequestHeader("Authorization", currentAccessToken);

                yield return request.SendWebRequest();
                PrintRequestResult("Save Persona", request);

                if (RequestError(request))
                {
                    error?.Invoke(request.error, request.responseCode);
                }
                else
                {
                    success?.Invoke();
                }
            }
        }

        #endregion CreatePersona

        #region EditPersona

        public delegate void SuccessEditPersona();
        public void EditPersona(Persona persona, SuccessEditPersona success, GenericError error)
        {
            if (!CheckEnv()) { return; }
            StartCoroutine(CoroutineEditPersona(persona, success, error));
        }

        private IEnumerator CoroutineEditPersona(Persona persona, SuccessEditPersona success, GenericError error)
        {
            Debug.Log("Edit Persona request started");
            string jsonPersona = SerializationHelper.Serialize(persona);
            Debug.Log("Json Persona: " + jsonPersona);
            Debug.Log("currentAccessToken: " + currentAccessToken);

            string url = envValues.databaseAPIPrivate + "persona";

            using (UnityWebRequest request = UnityWebRequest.Post(url, string.Empty))
            {
                request.method = "PATCH";
                request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonPersona));
                request.uploadHandler.contentType = "application/json";

                request.SetRequestHeader("Authorization", currentAccessToken);

                yield return request.SendWebRequest();
                PrintRequestResult("Save Persona", request);

                if (RequestError(request))
                {
                    error?.Invoke(request.error, request.responseCode);
                }
                else
                {
                    success?.Invoke();
                }
            }
        }

        #endregion EditPersona

        #region DeletePersona

        public delegate void SuccessDeletePersona();
        public void DeletePersona(Persona persona, SuccessDeletePersona success, GenericError error)
        {
            if (!CheckEnv()) { return; }
            StartCoroutine(CoroutineDeletePersona(persona, success, error));
        }

        private IEnumerator CoroutineDeletePersona(Persona persona, SuccessDeletePersona success, GenericError error)
        {
            Debug.Log("DeletePersona request started");
            string url = envValues.databaseAPIPrivate + "persona/" + persona.id;

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.method = "DELETE";
                request.SetRequestHeader("Authorization", currentAccessToken);
                yield return request.SendWebRequest();
                PrintRequestResult("Delete Persona Persona", request);

                if (RequestError(request))
                {
                    error?.Invoke(request.error, request.responseCode);
                }
                else
                {
                    success?.Invoke();
                }
            }
        }

        #endregion DeletePersona

        #region SetCurrentPersona

        public delegate void SuccessSetCurrentPersona();
        public void SetCurrentPersona(Persona persona, SuccessSetCurrentPersona success, GenericError error)
        {
            if (!CheckEnv()) { return; }
            StartCoroutine(CoroutineSetCurrentPersona(persona, success, error));
        }

        private IEnumerator CoroutineSetCurrentPersona(Persona persona, SuccessSetCurrentPersona success, GenericError error)
        {
            Debug.Log("Set Current Persona request started");
            string url = envValues.databaseAPIPrivate + "setActivePersona/" + persona.id;

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.method = "PATCH";
                request.SetRequestHeader("Authorization", currentAccessToken);
                yield return request.SendWebRequest();
                PrintRequestResult("Set Current Persona", request);

                if (RequestError(request))
                {
                    error?.Invoke(request.error, request.responseCode);
                }
                else
                {
                    success?.Invoke();
                }
            }
        }

        #endregion SetCurrentPersona

        #region GetAvatars

        public delegate void SuccessAvatars(List<Persona.Avatar> avatar);
        public void GetAvatars(SuccessAvatars success, GenericError error)
        {
            if (!CheckEnv()) { return; }
            StartCoroutine(CoroutineGetAvatars(success, error));
        }

        private IEnumerator CoroutineGetAvatars(SuccessAvatars success, GenericError error)
        {
            Debug.Log("Get Avatars request started");
            string url = envValues.databaseAPIPrivate + "userUnlockedAvatars?id=" + gameId;

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.SetRequestHeader("Authorization", currentAccessToken);
                yield return request.SendWebRequest();
                PrintRequestResult("Get Avatars", request);

                if (RequestError(request))
                {
                    error?.Invoke(request.error, request.responseCode);
                }
                else
                {
                    GetAvatarsResponse response = SerializationHelper.Deserialize<GetAvatarsResponse>(request.downloadHandler.text);
                    success?.Invoke(response.avatars);
                }
            }
        }

        #endregion GetAvatars

        #endregion AWS API
    }
}