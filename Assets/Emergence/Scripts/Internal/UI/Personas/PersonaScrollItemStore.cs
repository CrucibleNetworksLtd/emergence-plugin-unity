using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EmergenceSDK.Services;

namespace EmergenceSDK.Internal.UI.Personas
{
    public class PersonaScrollItemStore : IEnumerable<PersonaScrollItem>
    {
        private List<PersonaScrollItem> items = new List<PersonaScrollItem>();
        private PersonaScrollItem currentPersonaItem;

        public PersonaScrollItem this[int index]
        {
            get => items[index];
            set => items[index] = value;
        }

        public int Count => items.Count;

        public int GetCurrentPersonaIndex()
        {
            RefreshCurrentPersona();
            return items.FindIndex(p => p.Persona.id == currentPersonaItem.Persona.id);
        }
        
        public void SetPersonas(PersonaScrollItem[] itemsIn) => items = itemsIn.ToList();
        public void SetPersonas(List<PersonaScrollItem> itemsIn) => items = itemsIn;

        public PersonaScrollItem GetCurrentPersonaScrollItem()
        {
            RefreshCurrentPersona();
            return currentPersonaItem;
        }

        private void RefreshCurrentPersona()
        {
            var personaService = EmergenceServices.GetService<IPersonaService>();
            if (personaService.GetCurrentPersona(out var currentPersona))
            {
                currentPersonaItem = items.FirstOrDefault(item => item.Persona?.id == currentPersona.id);
            }
        }

        public List<PersonaScrollItem> GetAllItems() => new List<PersonaScrollItem>(items);
        public List<PersonaScrollItem> GetNonActiveItems()
        {
            return items.FindAll(item =>
            {
                RefreshCurrentPersona();
                return item.Persona.id != currentPersonaItem?.Persona.id;
            });
        }

        public int GetIndex(PersonaScrollItem item) => items.FindIndex(i => i.Persona.id == item.Persona.id);

        public void RemoveItem(string itemId) => items.RemoveAll(item => item.Persona.id == itemId);


        public IEnumerator<PersonaScrollItem> GetEnumerator()
        {
            return items.GetEnumerator();
        }
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}