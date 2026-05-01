using System.Collections.Generic;
using UnityEngine;

public class BarrierManager : MonoBehaviour
{
    public Transform player;
    public GameObject barrierPrefab;
    public GameObject bonusPrefab;
    public GameObject sideSceneryPrefab;

    [Header("Spawn Rules")]
    public float spawnDistanceAhead = 400f;
    public float despawnDistanceBehind = 30f;
    public float rowInterval = 10f;
    [Range(0f, 1f)] public float spawnDensity = 0.4f;
    public bool keepOneLaneOpen = true;

    [Header("Obstacle Size")]
    public float groundSurfaceY = 0.5f;
    public Vector2 obstacleUniformScaleRange = new Vector2(0.95f, 1.1f);

    [Header("Side Scenery")]
    public float sideSceneryRowInterval = 70f;
    [Range(0f, 1f)] public float sideSceneryDensity = 0.75f;
    public Vector2 sideSceneryScaleRange = new Vector2(0.9f, 1.2f);
    public float sideSceneryY = 0.5f;
    public float leftSceneryX = -8.5f;
    public float rightSceneryX = 8.5f;

    private float nextSpawnZ;
    private float nextSideSceneryZ;
    private float[] lanes = { -3f, -1.5f, 0f, 1.5f, 3f };

    private Queue<GameObject> barrierPool = new Queue<GameObject>();
    private Queue<GameObject> bonusPool = new Queue<GameObject>();
    private Queue<GameObject> sideSceneryPool = new Queue<GameObject>();
    private List<GameObject> activeBarriers = new List<GameObject>();
    private List<GameObject> activeBonuses = new List<GameObject>();
    private List<GameObject> activeSideScenery = new List<GameObject>();

    void Start()
    {
        nextSpawnZ = player.position.z + 50f;
        nextSideSceneryZ = player.position.z + 40f;
    }

    void Update()
    {
        while (nextSpawnZ < player.position.z + spawnDistanceAhead)
        {
            SpawnRow(nextSpawnZ);
            nextSpawnZ += rowInterval;
        }

        while (nextSideSceneryZ < player.position.z + spawnDistanceAhead)
        {
            SpawnSideSceneryRow(nextSideSceneryZ);
            nextSideSceneryZ += sideSceneryRowInterval;
        }

        RecycleObjects(activeBarriers, barrierPool);
        RecycleObjects(activeBonuses, bonusPool);
        RecycleObjects(activeSideScenery, sideSceneryPool);
    }

    void SpawnRow(float zPos)
    {
        int openLaneIndex = keepOneLaneOpen ? Random.Range(0, lanes.Length) : -1;

        for (int laneIndex = 0; laneIndex < lanes.Length; laneIndex++)
        {
            if (laneIndex == openLaneIndex) continue;

            float laneX = lanes[laneIndex];
            if (Random.value < spawnDensity)
            {
                bool isBonus = Random.value < 0.1f;
                GameObject go = GetFromPool(isBonus ? bonusPool : barrierPool, isBonus ? bonusPrefab : barrierPrefab);

                if (isBonus)
                {
                    go.transform.localScale = Vector3.one;
                    activeBonuses.Add(go);
                }
                else
                {
                    float uniformScale = Random.Range(obstacleUniformScaleRange.x, obstacleUniformScaleRange.y);
                    go.transform.localScale = Vector3.one * uniformScale;
                    activeBarriers.Add(go);
                }

                float spawnY = isBonus ? 1f : groundSurfaceY;
                go.transform.position = new Vector3(laneX, spawnY, zPos);
                go.SetActive(true);
            }
        }
    }

    GameObject GetFromPool(Queue<GameObject> pool, GameObject prefab)
    {
        if (pool.Count > 0) return pool.Dequeue();

        GameObject newObj = Instantiate(prefab, transform);
        newObj.SetActive(false);
        return newObj;
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

    void SpawnSideSceneryRow(float zPos)
    {
        TrySpawnSideScenery(leftSceneryX, zPos);
        TrySpawnSideScenery(rightSceneryX, zPos);
    }

    void TrySpawnSideScenery(float xPos, float zPos)
    {
        if (sideSceneryPrefab == null) return;
        if (Random.value > sideSceneryDensity) return;

        GameObject scenery = GetFromPool(sideSceneryPool, sideSceneryPrefab);
        float uniformScale = Random.Range(sideSceneryScaleRange.x, sideSceneryScaleRange.y);
        scenery.transform.localScale = Vector3.one * uniformScale;
        scenery.transform.position = new Vector3(xPos, sideSceneryY, zPos);
        scenery.SetActive(true);
        activeSideScenery.Add(scenery);
    }
}
