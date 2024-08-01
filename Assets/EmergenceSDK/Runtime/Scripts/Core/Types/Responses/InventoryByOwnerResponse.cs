using System.Collections.Generic;
using EmergenceSDK.Runtime.Types.Inventory;

namespace EmergenceSDK.Runtime.Types.Responses
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