#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Services;
using EmergenceSDK.Types;
using UnityEditor;
using UnityEngine;

namespace EmergenceSDK
{
    public class PersonaTesting : BaseTestWindow
    {
        private bool accessTokenRetrieved;
        
        private List<Persona> personas = new List<Persona>();

        private void OnGUI()
        {
            if (!ReadyToTest(out var msg))
            {
                EditorGUILayout.LabelField(msg);
                return;
            }
            
            EditorGUILayout.LabelField("Test Persona Service");

            if (!accessTokenRetrieved)
            {
                if(GUILayout.Button("GetAccessToken")) 
                    GetAccessTokenPressed();
                
                return;
            }
            
            
            if (GUILayout.Button("GetPersona")) 
                GetPersonaPressed();

            foreach (var persona in personas)
            {
                EditorGUILayout.LabelField("Persona: " + persona.name);
                EditorGUILayout.LabelField("Bio: " + persona.bio);
            }
        }

        private void GetAccessTokenPressed()
        {
            var personaService = EmergenceServices.GetService<IPersonaService>();
            personaService.GetAccessToken((accessToken) => accessTokenRetrieved = !String.IsNullOrEmpty(accessToken), EmergenceLogger.LogError);
        }

        private void GetPersonaPressed()
        {
            var personaService = EmergenceServices.GetService<IPersonaService>();
            personaService.GetPersonas((personasIn, currentPersona) => personas = personasIn, EmergenceLogger.LogError);
        }
    }
}

#endif