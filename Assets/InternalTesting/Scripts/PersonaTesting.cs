#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Services;
using EmergenceSDK.Types;
using UnityEditor;
using UnityEngine;

namespace EmergenceSDK.InternalTesting
{
    public class PersonaTesting : BaseTestWindow
    {
        private bool accessTokenRetrieved = false;
        private List<Persona> personas = new List<Persona>();
        private IPersonaService personaService;
        
        private Persona currentPersona;
        private Persona testPersona;

        private void OnGUI()
        {
            if (!ReadyToTest(out var msg))
            {
                EditorGUILayout.LabelField(msg);
                return;
            }
            needsCleanUp = true;

            personaService ??= EmergenceServices.GetService<IPersonaService>();
            
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
                EditorGUILayout.LabelField("PersonaBio: " + persona.bio);
            }

            if(currentPersona == null)
                return;

            if (GUILayout.Button("Create Test Persona"))
            {
                Persona newPersona = new Persona();
                newPersona.name = "TestPersona";
                newPersona.bio = "TestBio";
                newPersona.avatar = currentPersona.avatar;
                personaService.CreatePersona(newPersona, () => GetPersonaPressed(), EmergenceLogger.LogError);
            }

            if(testPersona == null)
                return;
            
            if (GUILayout.Button("Update Test Persona"))
            {
                testPersona.bio = "UpdatedBio";
                personaService.EditPersona(testPersona, () => GetPersonaPressed(), EmergenceLogger.LogError);
            }
            
            if (GUILayout.Button("Delete Test Persona"))
            {
                personaService.DeletePersona(testPersona, () => GetPersonaPressed(), EmergenceLogger.LogError);
            }
        }

        private void GetAccessTokenPressed()
        {
            personaService.GetAccessToken((accessToken) =>
            {
                accessTokenRetrieved = !String.IsNullOrEmpty(accessToken);
                Repaint();
            }, EmergenceLogger.LogError);
        }

        private void GetPersonaPressed()
        {
            personaService.GetPersonas((personasIn, currentPersonaIn) =>
            {
                currentPersona = currentPersonaIn ?? personasIn.FirstOrDefault();
                personas = personasIn;
                TryStoreTestPersona();
                Repaint();
            }, EmergenceLogger.LogError);
        }

        private void TryStoreTestPersona()
        {
            testPersona = personas.FirstOrDefault(persona => persona.name == "TestPersona");
        }

        protected override void CleanUp()
        {
            personas.Clear();
            personaService = null;
            currentPersona = null;
            accessTokenRetrieved = false;
            testPersona = null;
        }
    }
}

#endif