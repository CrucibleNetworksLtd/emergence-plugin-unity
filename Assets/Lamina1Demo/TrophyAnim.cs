using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EmergenceSDK
{
    public class TrophyAnim : MonoBehaviour
    {
        public float rotationSpeed = 45f; // Degrees per second for rotation
        public float moveDistance = 1f;   // Distance to move up and down
        public float moveSpeed = 1f;      // Speed of the up and down movement

        private Vector3 initialPosition;
        private float maxY;
        private float minY;
        private bool movingUp = true;

        void Start()
        {
            initialPosition = transform.position;
            maxY = initialPosition.y + moveDistance / 2;
            minY = initialPosition.y - moveDistance / 2;
        }

        void Update()
        {
            // Rotate the object around the Y-axis
            transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);

            // Move the object up and down along the Y-axis
            if (movingUp)
            {
                transform.Translate(Vector3.up * moveSpeed * Time.deltaTime);
                if (transform.position.y >= maxY)
                {
                    movingUp = false;
                }
            }
            else
            {
                transform.Translate(Vector3.down * moveSpeed * Time.deltaTime);
                if (transform.position.y <= minY)
                {
                    movingUp = true;
                }
            }
        }
    }
}
