using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrierManager : MonoBehaviour
{
    public Transform player;
    public GameObject barrierPrefab;
    public GameObject bonusPrefab;

    [Header("生成规则")]
    public float spawnDistanceAhead = 400f;  // 永远在玩家前方 400 米处生成（玩家绝对看不见突然蹦出来）
    public float despawnDistanceBehind = 30f;// 甩在身后 30 米后回收
    public float rowInterval = 10f;          // 每隔 10 米生成一排
    [Range(0f, 1f)] public float spawnDensity = 0.4f; // 每一排每个车道出现障碍物的概率

    private float nextSpawnZ;
    private float[] lanes = { -3f, -1.5f, 0f, 1.5f, 3f };

    // --- 两个队列作为对象池 (Queue 是最适合做对象池的结构) ---
    private Queue<GameObject> barrierPool = new Queue<GameObject>();
    private Queue<GameObject> bonusPool = new Queue<GameObject>();

    // --- 记录当前在场景中激活的物体 ---
    private List<GameObject> activeBarriers = new List<GameObject>();
    private List<GameObject> activeBonuses = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        // 游戏开始时，从玩家前方 50 米处开始铺设障碍物
        nextSpawnZ = player.position.z + 50f;
    }

    // Update is called once per frame
    void Update()
    {
        // 1. 动态生成逻辑：
        // 只要预定生成的 Z 坐标 小于 玩家位置+视野前方距离，就持续生成
        // 用 while 是为了防止玩家速度太快导致某一帧跨度太大
        while (nextSpawnZ < player.position.z + spawnDistanceAhead)
        {
            SpawnRow(nextSpawnZ);
            nextSpawnZ += rowInterval;
        }

        // 2. 动态回收逻辑：
        RecycleObjects(activeBarriers, barrierPool);
        RecycleObjects(activeBonuses, bonusPool);
    }

    // 在指定的 Z 坐标生成一排物体
    void SpawnRow(float zPos)
    {
        foreach (float laneX in lanes)
        {
            // 根据概率判定这个车道要不要放东西
            if (Random.value < spawnDensity)
            {
                bool isBonus = Random.value < 0.1f; // 10%的概率是心形奖励

                // 从对应的池子里拿物体
                GameObject go = GetFromPool(isBonus ? bonusPool : barrierPool, isBonus ? bonusPrefab : barrierPrefab);

                // --- 坐标与缩放设置 ---
                // 因为脱离了地面的缩放干扰，这里的尺寸是绝对的物理尺寸，非常精准
                if (isBonus)
                {
                    go.transform.localScale = Vector3.one;
                    activeBonuses.Add(go);
                }
                else
                {
                    float width = Random.Range(1.2f, 2.5f);
                    go.transform.localScale = new Vector3(width, 1f, 2f);
                    activeBarriers.Add(go);
                }

                go.transform.position = new Vector3(laneX, 1f, zPos);
                go.SetActive(true);
            }
        }
    }

    // 对象池获取逻辑
    GameObject GetFromPool(Queue<GameObject> pool, GameObject prefab)
    {
        if (pool.Count > 0) return pool.Dequeue();

        // 如果池子空了（或者第一次运行），就真正实例化一个
        GameObject newObj = Instantiate(prefab, transform);
        return newObj;
    }

    // 回收逻辑 (倒序遍历防止列表索引越界)
    void RecycleObjects(List<GameObject> activeList, Queue<GameObject> pool)
    {
        for (int i = activeList.Count - 1; i >= 0; i--)
        {
            GameObject obj = activeList[i];

            // 如果这个物体已经被玩家甩在身后了
            if (obj.transform.position.z < player.position.z - despawnDistanceBehind)
            {
                obj.SetActive(false);    // 隐藏
                pool.Enqueue(obj);       // 放回池子
                activeList.RemoveAt(i);  // 从活跃列表剔除
            }
        }
    }
}
