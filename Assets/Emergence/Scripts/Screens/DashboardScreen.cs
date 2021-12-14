using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Emergence
{
    public class DashboardScreen : MonoBehaviour
    {
        [Header("UI References")]
        public Transform personaScrollContents;

        [SerializeField]
        private Pool personaButtonPool;

        private void Start()
        {
            // TODO Delete this
            // Pool usage example
            GameObject go = personaButtonPool.GetNewObject();

            go.transform.SetParent(personaScrollContents);
            go.transform.localScale = Vector3.one; // Sometimes unity breaks the size when reparenting


            // When done
            personaButtonPool.ReturnUsedObject(go);
        }

        private void Update()
        {

        }
    }
}
