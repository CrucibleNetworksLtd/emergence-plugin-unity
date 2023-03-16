using System;
using System.Collections.Generic;
using UnityEngine;

namespace EmergenceSDK.EmergenceDemo.Scripts
{
    public class DemoStationController : MonoBehaviour
    {
        
        public DemoStation<DemoOpenOverlay> openOverlay;
        
        public DemoStation<DemoDynamicMetadataController> dynamicMetadataController;
        public DemoStation<DemoInventoryService> inventoryService;
        public DemoStation<DemoMintAvatar> mintAvatar;
        public DemoStation<DemoReadMethod> readMethod;
        public DemoStation<DemoSignMessage> signMessage;
        public DemoStation<DemoWriteMethod> writeMethod;

        private List<IDemoStation> stations;

        public void Awake()
        {
            stations = new List<IDemoStation>()
            {
                dynamicMetadataController as IDemoStation,
                inventoryService as IDemoStation,
                mintAvatar as IDemoStation,
                readMethod as IDemoStation,
                signMessage as IDemoStation,
                writeMethod as IDemoStation
            };
        }
    }
}