using System.Collections.Generic;
using UnityEngine;

public class CrowdSystem : MonoBehaviour
{
    public GameObject humanPrefab;
    public Transform robot;

    public int maxDensity = 80;
    public int targetDensity = 20;

    public float spawnRangeX = 15f;
    public float spawnRangeZ = 5f;

    public float minSpeed = 0.5f;
    public float maxSpeed = 3f;

    public float acceleration = 1;
    private float currentCrowdSpeed;
    private float targetCrowdSpeed;

    private List<GameObject> humans = new List<GameObject>();

    void Start()
    {
        InitPool();
        float t = (float)targetDensity / maxDensity;
        currentCrowdSpeed = Mathf.Lerp(maxSpeed, minSpeed, t);
        targetCrowdSpeed = currentCrowdSpeed;
        UpdateSpeed();
    }

    void Update()
    {
        UpdateVisibility();
        MoveLoop();

        currentCrowdSpeed = Mathf.MoveTowards(
            currentCrowdSpeed,
            targetCrowdSpeed,
            acceleration * Time.deltaTime);

        UpdateSpeed();
    }

    void InitPool()
    {
        for (int i = 0; i < maxDensity; i++)
        {
            Vector3 pos = new Vector3(
                robot.position.x + Random.Range(-spawnRangeX, spawnRangeX),
                1,
                Random.Range(-spawnRangeZ, spawnRangeZ)
            );

            GameObject h = Instantiate(humanPrefab, pos, Quaternion.identity, transform);
            humans.Add(h);
        }
    }

    void UpdateVisibility()
    {
        for (int i = 0; i < humans.Count; i++)
        {
            bool shouldBeVisible = i < targetDensity;

            foreach (var r in humans[i].GetComponentsInChildren<MeshRenderer>())
            {
                r.enabled = shouldBeVisible;
            }
        }
    }

    public float GetCurrentSpeed()
    {
        return currentCrowdSpeed;
    }

    void UpdateSpeed()
    {
        foreach (var h in humans)
        {
            var walker = h.GetComponent<CrowdWalker>();
            if (walker != null)
                walker.speed = currentCrowdSpeed;
        }
    }

    void MoveLoop()
    {
        foreach (var h in humans)
        {
            // если ушёл назад за робота
            if (h.transform.position.x < robot.position.x - 10f)
            {
                // телепорт вперёд
                Vector3 pos = new Vector3(
                    robot.position.x + spawnRangeX,
                    1,
                    Random.Range(-spawnRangeZ, spawnRangeZ)
                );

                h.transform.position = pos;
            }
        }
    }

    // для UI
    public void SetDensity(float value)
    {
        targetDensity = Mathf.RoundToInt(value);

        float t = (float)targetDensity / maxDensity;
        targetCrowdSpeed = Mathf.Lerp(maxSpeed, minSpeed, t);
    }
}