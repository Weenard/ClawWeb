using UnityEngine;
using UnityEngine.AI;

public class RobotNavMover : MonoBehaviour
{
    private NavMeshAgent agent;

    public Transform target;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.SetDestination(target.position);
    }

    void Update()
    {
        agent.SetDestination(target.position);
    }
}