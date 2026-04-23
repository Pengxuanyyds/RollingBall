using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bonus : MonoBehaviour
{
    // 定义收集事件
    public delegate void BonusCollectedAction();
    public static event BonusCollectedAction OnBonusCollected;

    private bool hasCollected = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasCollected) return;
        if (other.CompareTag("Player") || other.name == "Player")
        {
            hasCollected = true;

            // 发出加血信号
            OnBonusCollected?.Invoke();

            // 隐藏自己
            GetComponent<MeshRenderer>().enabled = false;
            GetComponent<Collider>().enabled = false;
        }
    }

    private void OnEnable()
    {
        hasCollected = false;
        GetComponent<MeshRenderer>().enabled = true;
        GetComponent<Collider>().enabled = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
