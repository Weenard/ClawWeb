using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UIElements;

public class ObstaclesScenarioController : MonoBehaviour
{
    [Header("Setup")]
    public GameObject obstaclePrefab;
    public Transform spawnParent;

    [Header("Limits")]
    public int MaxObstacles = 5;

    public Vector3 Min = new Vector3(-5, 0, -5);
    public Vector3 Max = new Vector3(5, 0, 5);
    public float Step = 0.5f;

    private float x_ = 0, y_ = 0, z_ = 0;

    private List<GameObject> obstacles = new List<GameObject>();

    public int ObstacleCount => obstacles.Count;

    // =========================
    // API для WebSocket
    // =========================

    public bool TryAddObstacle(Vector3 position, out string error)
    {
        error = null;

        if (obstacles.Count >= MaxObstacles)
        {
            error = "Max obstacles reached";
            return false;
        }

        if (!IsInsideBounds(position))
        {
            error = "Position out of bounds";
            return false;
        }

        var obj = Instantiate(obstaclePrefab, position + spawnParent.position, Quaternion.identity);
        obstacles.Add(obj);

        return true;
    }

    public void ClearObstacles()
    {
        foreach (var o in obstacles)
        {
            if (o != null)
                Destroy(o);
        }

        obstacles.Clear();
    }

    public void ResetScenario()
    {
        ClearObstacles();
    }

    // =========================

    private bool IsInsideBounds(Vector3 pos)
    {
        return pos.x >= Min.x && pos.x <= Max.x &&
               pos.y >= Min.y && pos.y <= Max.y &&
               pos.z >= Min.z && pos.z <= Max.z;
    }
}