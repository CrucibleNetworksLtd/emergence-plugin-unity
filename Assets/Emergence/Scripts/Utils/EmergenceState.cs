using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Emergence
{
    public class EmergenceState : MonoBehaviour
    {
        public static bool IsConnected
        {
            get;
            private set;
        }

        public static Persona CurrentPersona
        {
            get;
            private set;
        }
    }
}
