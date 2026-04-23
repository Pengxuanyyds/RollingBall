using System.Collections.Generic;
using UnityEngine;

public class BarrierManager : MonoBehaviour
{
    public Transform player;
    public GameObject barrierPrefab;
    public GameObject bonusPrefab;       // Heart (heal +1 HP)
    public GameObject shieldPrefab;      // Shield (block 1 hit)

    [Header("Spawn Settings")]
    public float spawnDistanceAhead = 400f;
    public float despawnDistanceBehind = 30f;
    public float rowInterval = 10f;

    [Header("Difficulty Curve")]
    [Range(0f, 1f)] public float startDensity = 0.12f;
    [Range(0f, 1f)] public float maxDensity = 0.35f;
    public float densityRampDistance = 2000f;

    [Header("Spawn Probability")]
    [Range(0f, 1f)] public float shieldChance = 0.05f;
    [Range(0f, 1f)] public float bonusChance = 0.10f;

    private float nextSpawnZ;
    private float[] lanes = { -3f, -1.5f, 0f, 1.5f, 3f };

    private Queue<GameObject> barrierPool = new Queue<GameObject>();
    private Queue<GameObject> bonusPool = new Queue<GameObject>();
    private Queue<GameObject> shieldPool = new Queue<GameObject>();

    private List<GameObject> activeBarriers = new List<GameObject>();
    private List<GameObject> activeBonuses = new List<GameObject>();
    private List<GameObject> activeShields = new List<GameObject>();

    void Start()
    {
        nextSpawnZ = player.position.z + 50f;
    }

    void Update()
    {
        while (nextSpawnZ < player.position.z + spawnDistanceAhead)
        {
            SpawnRow(nextSpawnZ);
            nextSpawnZ += rowInterval;
        }

        RecycleObjects(activeBarriers, barrierPool);
        RecycleObjects(activeBonuses, bonusPool);
        RecycleObjects(activeShields, shieldPool);
    }

    float GetCurrentDensity()
    {
        float distance = player.position.z;
        float t = Mathf.Clamp01(distance / densityRampDistance);
        return Mathf.Lerp(startDensity, maxDensity, t);
    }

    void SpawnRow(float zPos)
    {
        float density = GetCurrentDensity();
        int spawned = 0;
        int maxPerRow = 3;

        foreach (float laneX in lanes)
        {
            if (spawned >= maxPerRow) break;
            if (Random.value < density)
            {
                float roll = Random.value;
                GameObject go;

                if (roll < shieldChance && shieldPrefab != null)
                {
                    go = GetFromPool(shieldPool, shieldPrefab);
                    go.transform.localScale = Vector3.one;
                    activeShields.Add(go);
                }
                else if (roll < shieldChance + bonusChance)
                {
                    go = GetFromPool(bonusPool, bonusPrefab);
                    go.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
                    activeBonuses.Add(go);
                }
                else
                {
                    go = GetFromPool(barrierPool, barrierPrefab);
                    float width = Random.Range(1.0f, 1.4f);
                    go.transform.localScale = new Vector3(width, 1f, 2f);
                    activeBarriers.Add(go);
                }

                go.transform.position = new Vector3(laneX, 1f, zPos);
                go.SetActive(true);
                spawned++;
            }
        }
    }

    GameObject GetFromPool(Queue<GameObject> pool, GameObject prefab)
    {
        if (pool.Count > 0) return pool.Dequeue();
        return Instantiate(prefab, transform);
    }

    void RecycleObjects(List<GameObject> activeList, Queue<GameObject> pool)
    {
        for (int i = activeList.Count - 1; i >= 0; i--)
        {
            GameObject obj = activeList[i];
            if (obj.transform.position.z < player.position.z - despawnDistanceBehind)
            {
                obj.SetActive(false);
                pool.Enqueue(obj);
                activeList.RemoveAt(i);
            }
        }
    }
}
