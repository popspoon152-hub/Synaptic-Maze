using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Vector3 lastCheckPointPos; // 记录最后一次检查点的位置
    public GameObject player;
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        lastCheckPointPos = player.transform.position;
    }

    public void UpdateCheckPoint(Vector3 pos)
    {
        lastCheckPointPos = pos;
    }
}
