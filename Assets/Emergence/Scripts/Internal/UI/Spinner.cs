using UnityEngine;

namespace EmergenceSDK.Internal.UI
{
    public class Spinner : MonoBehaviour
    {
        [SerializeField]
        private float speed = 1.0f;

        private void Update()
        {
            transform.Rotate(Vector3.forward * 360.0f * speed * Time.deltaTime);
        }
    }
}
