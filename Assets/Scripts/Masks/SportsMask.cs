using UnityEngine;

public class SportsMask : PlayerMask
{
    [Header("效果设置")]
    public float weightScale = 2.5f; // 增加重力的倍率

    private GameObject instantiatedOverlay;

    public override void ApplyEffect(PlayerController player)
    {

        //遮罩
        if (maskCoverPrefab != null)
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            instantiatedOverlay = Instantiate(maskCoverPrefab, canvas.transform);
        }

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale *= weightScale;
        }
    }

    public override void RemoveEffect(PlayerController player)
    {
        //销毁 UI
        if (instantiatedOverlay != null) Destroy(instantiatedOverlay);


        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale /= weightScale;
        }
    }
}