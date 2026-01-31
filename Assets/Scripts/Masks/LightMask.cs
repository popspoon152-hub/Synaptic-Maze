using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LightMask : PlayerMask
{
    [Header("叠加图片设置")]
    [SerializeField] private GameObject lightSpritePrefab;

    private GameObject instantiatedOverlay;
    private GameObject currentLightEffect;

    public override void ApplyEffect(PlayerController player)
    {
        if (maskCoverPrefab != null)
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            instantiatedOverlay = Instantiate(maskCoverPrefab, canvas.transform);
        }

        if (lightSpritePrefab != null)
        {
            currentLightEffect = Instantiate(lightSpritePrefab, player.transform);
            currentLightEffect.transform.localPosition = Vector3.zero;
        }

        SetDarknessAlpha(0f);
    }

    public override void RemoveEffect(PlayerController player)
    {
        if (instantiatedOverlay != null) Destroy(instantiatedOverlay);
        if (currentLightEffect != null) Destroy(currentLightEffect);

        // 恢复 darkness 层的默认效果 (假设默认 Alpha 是 0.8)
        SetDarknessAlpha(0.8f);
    }

    private void SetDarknessAlpha(float alpha)
    {
        Tilemap[] allMaps = FindObjectsByType<Tilemap>(FindObjectsSortMode.None);
        int targetLayer = LayerMask.NameToLayer("darkness");

        foreach (var map in allMaps)
        {
            if (map.gameObject.layer == targetLayer)
            {
                Color c = map.color;
                c.a = alpha;
                map.color = c;
            }
        }
    }
}
