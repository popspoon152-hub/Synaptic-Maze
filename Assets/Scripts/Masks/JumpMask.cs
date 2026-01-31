using UnityEngine;

public class JumpMask : PlayerMask
{
    [Header("跳跃面具设置")]
    [SerializeField] private float jumpMultiplier = 1.5f; // 跳跃高度倍率

    private GameObject instantiatedOverlay;

    public override void ApplyEffect(PlayerController player)
    {
        //遮罩
        if (maskCoverPrefab != null)
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            instantiatedOverlay = Instantiate(maskCoverPrefab, canvas.transform);
        }
        
        player.jumpHeight *= jumpMultiplier;

        player.CalculatePhysics();
    }

    public override void RemoveEffect(PlayerController player)
    {
        //销毁 UI
        if (instantiatedOverlay != null) Destroy(instantiatedOverlay);

        player.jumpHeight /= jumpMultiplier;

        player.CalculatePhysics();
    }
}