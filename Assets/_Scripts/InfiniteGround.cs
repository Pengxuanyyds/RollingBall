using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteGround : MonoBehaviour
{
    public Transform player;
    public float groundLength = 1000f;

    void Update()
    {
        // 当玩家完全跑过这块地的中心，就把它挪到最前面
        if (player.position.z > transform.position.z + (groundLength / 2f) + 100f)
        {
            Relocate();
        }
    }

    void Relocate()
    {
        InfiniteGround[] allGrounds = Object.FindObjectsByType<InfiniteGround>(FindObjectsSortMode.None);
        float maxZ = -100000f;
        foreach (var g in allGrounds)
        {
            if (g.transform.position.z > maxZ) maxZ = g.transform.position.z;
        }

        Vector3 newPos = transform.position;
        newPos.z = maxZ + groundLength;
        transform.position = newPos;
    }
}
