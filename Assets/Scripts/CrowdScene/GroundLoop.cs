using UnityEngine;

public class GroundLoop : MonoBehaviour
{
    public float length = 20f;

    public CrowdSystem crowdSystem;
    void Update()
    {
        // бесконечный луп
        if (transform.position.x < -length)
        {
            transform.position += new Vector3(length * 2f, 0, 0);
        }
    }
}