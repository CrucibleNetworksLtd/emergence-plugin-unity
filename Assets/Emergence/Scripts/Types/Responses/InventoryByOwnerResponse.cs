using System.Collections.Generic;

namespace EmergenceSDK
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