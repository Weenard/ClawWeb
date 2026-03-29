using System;
using UnityEngine;
using UnityEngine.AI;

public class Portal : MonoBehaviour
{
    public Transform targetPoint; // куда телепортируем

    private void OnTriggerEnter(Collider other)
    {
        var agent = other.GetComponent<NavMeshAgent>();

        if (agent != null)
        {
            agent.Warp(targetPoint.position);
        }
        else
        {
            other.transform.position = targetPoint.position;
        }
        Debug.Log("enter");
    }
}