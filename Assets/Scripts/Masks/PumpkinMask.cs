using UnityEngine;
using UnityEngine.Tilemaps;

public class PumpkinMask : PlayerMask
{
    private GameObject instantiatedOverlay;

    public override void ApplyEffect(PlayerController player)
    {
        //遮罩
        if (maskCoverPrefab != null)
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            instantiatedOverlay = Instantiate(maskCoverPrefab, canvas.transform);
        }

        //将 PumpkinMaskCover 设为全透明
        SetTilemapAlpha("PumpkinMaskCover", 0f);

        //PumpkinMaskCoverSpikes 设为不透明（恢复正常）
        SetTilemapAlpha("PumpkinMaskCoverSpikes", 1f);
    }

    public override void RemoveEffect(PlayerController player)
    {
        //销毁 UI
        if (instantiatedOverlay != null) Destroy(instantiatedOverlay);

        //效果反转
        SetTilemapAlpha("PumpkinMaskCover", 1f);
        SetTilemapAlpha("PumpkinMaskCoverSpikes", 0f);
    }

    private void SetTilemapAlpha(string tag, float alpha)
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag(tag);
        foreach (GameObject obj in targets)
        {
            if (obj.TryGetComponent<Tilemap>(out var tilemap))
            {
                Color c = tilemap.color;
                c.a = alpha;
                tilemap.color = c;
            }
        }
    }
}
