using System.Collections.Generic;

namespace EmergenceSDK.Types.Responses
{
    public class InventoryByOwnerResponse
    {
        public class Message 
        {
            public List<InventoryItem> items;
        }

        public Message message;

    }
}