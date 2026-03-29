using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;   // робот
    public Vector3 offset;     // смещение камеры

    void LateUpdate()
    {
        transform.position = target.position + offset;
    }
}