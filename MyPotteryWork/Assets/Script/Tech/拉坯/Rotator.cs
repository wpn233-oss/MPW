using UnityEngine;

public class Rotator : MonoBehaviour
{
    public float rotationSpeed = 1000f; // Ðý×ªËÙ¶È

    void Update()
    {
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0, Space.World);
    }
}
