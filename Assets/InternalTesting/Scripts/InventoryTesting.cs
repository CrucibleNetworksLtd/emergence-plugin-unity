#if UNITY_EDITOR

using System.Collections.Generic;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Services;
using EmergenceSDK.Types;
using EmergenceSDK.Types.Inventory;
using UnityEditor;
using UnityEngine;

namespace EmergenceSDK
{
    public class InventoryTesting : BaseTestWindow
    {
        
        private List<InventoryItem> inventoryItems;
        
        private void OnGUI()
        {
            if (!ReadyToTest(out var msg))
            {
                EditorGUILayout.LabelField(msg);
                return;
            }
            
            EditorGUILayout.LabelField("Test Inventory Service");
            
            if (GUILayout.Button("InventoryByOwner")) 
                EmergenceServices.GetService<IInventoryService>().InventoryByOwner(EmergenceSingleton.Instance.GetCachedAddress(), InventoryChain.AnyCompatible, (inventory) => inventoryItems = inventory, EmergenceLogger.LogError);
            
            EditorGUILayout.LabelField("Retrieved Inventory:");
            foreach (var item in inventoryItems)
            {
                EditorGUILayout.LabelField("Item: " + item.Meta.Name);
                EditorGUILayout.LabelField("Contract: " + item.Contract);
            }
        }

        protected override void CleanUp()
        {
        }
    }
}

#endif