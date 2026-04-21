using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barrier : MonoBehaviour
{
    public GameObject destroyEffectPrefab;

    // 定义一个静态事件，当任何障碍物被撞时发出信号
    // 参数含义：Collider(撞到的物体), Vector3(撞击位置)
    public delegate void BarrierHitAction();
    public static event BarrierHitAction OnAnyBarrierHit;

    private bool hasCollided = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        // 1. 严谨判定：防止连环碰撞或非玩家物体触发
        if (hasCollided) return;
        if (!other.CompareTag("Player") && other.name != "Player") return;

        hasCollided = true;

        // 2. 发出信号：告诉所有监听者“有个障碍物被撞了”
        // 这里不直接操作 Player 的 HP，非常干净
        OnAnyBarrierHit?.Invoke();

        // 3. 视觉反馈：生成破碎特效
        if (destroyEffectPrefab != null)
        {
            Instantiate(destroyEffectPrefab, transform.position, Quaternion.identity);
            // 注意：特效的销毁我们已经在粒子系统的 Stop Action 里设为 Destroy 了
        }

        // 4. 严谨处理：隐藏自己。不要直接 Destroy(gameObject)，
        // 否则 InfiniteGround 的 activeBarriers 列表会出现空引用错误。
        GetComponent<MeshRenderer>().enabled = false;
        GetComponent<Collider>().enabled = false;
    }

    // 当地块循环重置时，InfiniteGround 会重新激活这个障碍物
    // 我们需要重置它的状态
    private void OnEnable()
    {
        hasCollided = false;
        GetComponent<MeshRenderer>().enabled = true;
        GetComponent<Collider>().enabled = true;
    }
}
