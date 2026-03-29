using UnityEngine;

public class GroundMover : MonoBehaviour
{
    public float speed = 2f;
    public CrowdSystem system;

    void Update()
    {
        Vector3 dir = Vector3.left;
        speed = system.GetCurrentSpeed();
        transform.Translate(dir.normalized * speed * Time.deltaTime);
    }
}