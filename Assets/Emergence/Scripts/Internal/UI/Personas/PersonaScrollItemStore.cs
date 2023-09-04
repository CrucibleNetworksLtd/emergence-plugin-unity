using System.Collections.Generic;
using System.Linq;
using EmergenceSDK.Services;

namespace EmergenceSDK.Internal.UI.Personas
{
    public class PersonaScrollItemStore
    {
        private List<PersonaScrollItem> items = new List<PersonaScrollItem>();
        private PersonaScrollItem currentPersonaItem;

        public void SetPersonas(List<PersonaScrollItem> itemsIn) => items = itemsIn;

        public PersonaScrollItem GetCurrentPersonaScrollItem()
        {
            var personaService = EmergenceServices.GetService<IPersonaService>();
            if(personaService.GetCurrentPersona(out var currentPersona))
            {
                currentPersonaItem = items.FirstOrDefault(item => item.Persona.id == currentPersona.id);
            }
            return currentPersonaItem;
        }

        public List<PersonaScrollItem> GetAllItems() => new List<PersonaScrollItem>(items);

        public void RemoveItem(string itemId) => items.RemoveAll(item => item.Persona.id == itemId);
    }
}