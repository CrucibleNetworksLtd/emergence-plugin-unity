using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Internal.UI.Screens;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Services;
using EmergenceSDK.Types;
using UnityEngine;
using Avatar = EmergenceSDK.Types.Avatar;

namespace EmergenceSDK.Internal.UI.Personas
{
    public class PersonaUIManager
    {
        private readonly DashboardScreen dashboardScreen;
        private readonly Pool personaButtonPool;
        private readonly PersonaCarousel personaCarousel;
        private readonly Transform personaScrollContents;
        private IPersonaService personaService;
        internal readonly PersonaScrollItemStore PersonaScrollItemStore = new PersonaScrollItemStore();

        private HashSet<string> imagesRefreshing = new HashSet<string>();
        
        private Persona SelectedPersona => PersonaCarousel.Instance.SelectedPersona;
        private Persona ActivePersona => PersonaScrollItemStore.GetCurrentPersonaScrollItem()?.Persona;
        
        private bool requestingInProgress = false;
        
        private Texture2D defaultAvatarThumbnail;
        private Texture2D DefaultAvatarThumbnail =>  defaultAvatarThumbnail ??= Resources.Load<Texture2D>("DefaultAvatarThumbnail")  ?? new Texture2D(100, 100);

        public PersonaUIManager(DashboardScreen dashboardScreen, Pool personaButtonPool,
            PersonaCarousel personaCarousel, Transform personaScrollContents)
        {
            this.dashboardScreen = dashboardScreen;
            this.personaButtonPool = personaButtonPool;
            this.personaCarousel = personaCarousel;
            personaCarousel.Items = PersonaScrollItemStore;
            this.personaScrollContents = personaScrollContents;

            dashboardScreen.CreatePersonaClicked += OnCreatePersona;
            dashboardScreen.ActivePersonaClicked += OnActivePersonaClicked;
            dashboardScreen.UsePersonaAsCurrentClicked += OnUsePersonaAsCurrent;
            dashboardScreen.EditPersonaClicked += OnEditPersona;
            dashboardScreen.DeletePersonaClicked += OnDeletePersona;

            PersonaScrollItem.OnSelected += PersonaScrollItem_OnSelected;
            PersonaScrollItem.OnImageCompleted += PersonaScrollItem_OnImageCompleted;
            
            personaCarousel.ArrowClicked += PersonaCarousel_OnArrowClicked;
        }
        
        
        private void OnCreatePersona()
        {
            Persona persona = new Persona()
            {
                id = string.Empty,
                name = string.Empty,
                bio = string.Empty,
                avatar = new Avatar()
                {
                    avatarId = string.Empty,
                },
                AvatarImage = null,
            };

            EditPersonaScreen.Instance.Refresh(persona, true, true);
            ScreenManager.Instance.ShowEditPersona();
        }
        
        private void OnActivePersonaClicked()
        {
            if (ActivePersona == null)
            {
                return;
            }

            PersonaScrollItem_OnSelected(ActivePersona, -1);
            PersonaCarousel.Instance.GoToActivePersona();
        }
        
        private async void OnUsePersonaAsCurrent()
        {
            Modal.Instance.Show("Loading Personas...");
            var setPersonaResponse = await personaService.SetCurrentPersonaAsync(SelectedPersona);
            if (setPersonaResponse.Success)
            {
                Refresh().Forget();
            }
            Modal.Instance.Hide();
            if(!setPersonaResponse.Success)
            {
                ModalPromptOK.Instance.Show("Error setting current persona");
            }
        }
        
        private void OnEditPersona()
        {
            EditPersonaScreen.Instance.Refresh(SelectedPersona, ActivePersona.id == SelectedPersona.id);
            ScreenManager.Instance.ShowEditPersona();
        }

        private void OnDeletePersona() => ModalPromptYESNO.Instance.Show($"Deleting persona: {SelectedPersona.name}", "Are you sure?", DeleteSelectedPersona);

        private async void DeleteSelectedPersona()
        {
            Modal.Instance.Show("Deleting Persona...");
            var personaServiceResult = await personaService.DeletePersonaAsync(SelectedPersona);
            if (personaServiceResult.Success)
            {
                Refresh().Forget();
            }
            Modal.Instance.Hide();
        }

        public async UniTask Refresh()
        {
            personaService = EmergenceServices.GetService<IPersonaService>();
            
            dashboardScreen.HideUI();
            dashboardScreen.HideDetailsPanel();
            HeaderScreen.Instance.Show();
            while (personaScrollContents.childCount > 0)
            {
                personaButtonPool.ReturnUsedObject(personaScrollContents.GetChild(0).gameObject);
            }

            Modal.Instance.Show("Loading Personas...");
            
            var personas = await personaService.GetPersonasAsync();
            if (personas.Success)
            {
                GetPersonasSuccess(personas.Result0, personas.Result1);
            }
            else
            {
                ModalPromptOK.Instance.Show("Error retrieving personas");
            }
            personaCarousel.GoToActivePersona();
        }

        private void GetPersonasSuccess(List<Persona> personas, Persona currentPersona)
        {
            Modal.Instance.Show("Retrieving avatars...");
            try
            {
                requestingInProgress = true;
                imagesRefreshing.Clear();

                //Update scroll items
                List<PersonaScrollItem> scrollItems = CreateScrollItems(personas, currentPersona);
                if(scrollItems.Count > 0)
                {
                    PersonaScrollItemStore.SetPersonas(scrollItems);
                    RefreshScrollItems(personas, scrollItems);
                }

                PersonaCarousel.Instance.Refresh();
                dashboardScreen.ShowUI(PersonaScrollItemStore.Count == 0);
                PersonaScrollItem_OnSelected(ActivePersona, PersonaScrollItemStore.GetCurrentPersonaIndex());
                requestingInProgress = false;
            }
            catch (Exception e)
            {
                EmergenceLogger.LogError(e.Message);
            }
            finally
            {
                Modal.Instance.Hide();
            }
        }
        
        private List<PersonaScrollItem> CreateScrollItems(List<Persona> personas, Persona currentPersona)
        {
            List<PersonaScrollItem> scrollItems = new List<PersonaScrollItem>();

            for (int i = 0; i < personas.Count; i++)
            {
                // Creation and setup of scroll item
                GameObject go = personaButtonPool.GetNewObject();
                go.transform.SetParent(personaScrollContents);
                go.transform.localScale = Vector3.one;

                scrollItems.Add(personaScrollContents.GetChild(i).GetComponent<PersonaScrollItem>());

                Persona persona = personas[i];

                // Avatar validation logic
                if (persona.avatar != null)
                {
                    if (string.IsNullOrEmpty(persona.avatar.avatarId) || string.IsNullOrEmpty(persona.avatar.meta.content.First().url))
                    {
                        persona.avatar = null;
                    }
                }
            }

            return scrollItems;
        }
        
        private void RefreshScrollItems(List<Persona> personas, List<PersonaScrollItem> scrollItems)
        {
            for (int i = 0; i < personas.Count; i++)
            {
                Persona persona = personas[i];
                imagesRefreshing.Add(persona.id);
                scrollItems[i].Refresh(DefaultAvatarThumbnail, persona);
            }
        }
        
        
        private void PersonaCarousel_OnArrowClicked(Persona persona)
        {
            OnNewItemSelected(persona);
        }

        private void PersonaScrollItem_OnSelected(Persona persona, int position)
        {
            if (OnNewItemSelected(persona)) 
                return;
            personaCarousel.GoToPosition(persona);
        }

        private bool OnNewItemSelected(Persona persona)
        {
            if (persona == null)
            {
                return true;
            }

            bool active = ActivePersona != null && persona.id == ActivePersona.id;

            dashboardScreen.ShowDetailsPanel(persona, active);
            return false;
        }

        private void PersonaScrollItem_OnImageCompleted(Persona persona, bool success)
        {
            if (ActivePersona != null)
            {
                if (ActivePersona.id.Equals(persona.id))
                {
                     dashboardScreen.ShowAvatar(persona.AvatarImage);
                }
            }

            if (imagesRefreshing.Contains(persona.id))
            {
                imagesRefreshing.Remove(persona.id);
                if (imagesRefreshing.Count <= 0 && !requestingInProgress)
                {
                    Modal.Instance.Hide();
                }
            }
        }
    }
}