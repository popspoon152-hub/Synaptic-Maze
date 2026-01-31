using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 如果碰到的是“尖刺”
        if (collision.CompareTag("Spikes") || collision.CompareTag("PumpkinMaskCoverSpikes"))
        {
            Respawn();
        }
        // 如果碰到的是“检查点”
        if (collision.CompareTag("Checkpoint"))
        {
            GameManager.Instance.UpdateCheckPoint(collision.transform.position);
            Debug.Log("检查点已更新！");
        }
    }

    void Respawn()
    {
        // 将玩家位置重置到记录的检查点
        transform.position = GameManager.Instance.lastCheckPointPos;
        // 如果你有刚体，重置速度防止惯性导致再次掉落
        GetComponent<Rigidbody2D>().velocity = Vector2.zero;
    }
}
