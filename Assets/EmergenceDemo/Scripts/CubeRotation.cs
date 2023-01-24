using UnityEngine;

public class CubeRotation : MonoBehaviour
{
    private float x;
    private float y;
    private float z;

    private float speedX;
    private float speedY;
    private float speedZ;

    void Start()
    {
        x = Random.Range(-100.0f, 100.0f);
        y = Random.Range(-100.0f, 100.0f);
        z = Random.Range(-100.0f, 100.0f);

        speedX = Random.Range(-10.0f, 10.0f);
        speedY = Random.Range(-10.0f, 10.0f);
        speedZ = Random.Range(-10.0f, 10.0f);
    }

    void Update()
    {
        x += speedX * Time.deltaTime;
        y += speedY * Time.deltaTime;
        z += speedZ * Time.deltaTime;
        transform.localRotation = Quaternion.Euler(x, y, z);
    }
}
