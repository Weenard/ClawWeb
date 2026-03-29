using UnityEngine;

public class CrowdWalker : MonoBehaviour
{
    public float speed = 2f;
    public Transform robot;
    public float avoidDistance = 2f;
    public float pushStrength = 3f;
    void Update()
    {
        Vector3 dir = Vector3.left;

        float dist = Vector3.Distance(transform.position, robot.position);

        if (dist < avoidDistance)
        {
            Vector3 push = transform.position - robot.position;
            push.y = 0;
            Vector3 pushDir = push.normalized;
            dir += pushDir * pushStrength;
        }

        transform.Translate(dir.normalized * speed * Time.deltaTime);
    }
}