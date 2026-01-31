using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PumpkinMask : PlayerMask
{
    private GameObject instantiatedOverlay;
    private GameObject[] hiddenAreas; // 存储场景中所有的隐藏区域

    public override void ApplyEffect(PlayerController player)
    {
        if (maskCoverPrefab != null)
        {
            // 寻找场景中的 Canvas
            Canvas canvas = FindFirstObjectByType<Canvas>();
            instantiatedOverlay = Instantiate(maskCoverPrefab, canvas.transform);
        }

        hiddenAreas = GameObject.FindGameObjectsWithTag("PumpkinMaskCover");
        foreach (GameObject area in hiddenAreas)
        {
            area.SetActive(false);
        }
    }

    public override void RemoveEffect(PlayerController player)
    {
        if (instantiatedOverlay != null)
        {
            Destroy(instantiatedOverlay);
        }

        if (hiddenAreas != null)
        {
            foreach (GameObject area in hiddenAreas)
            {
                if (area != null) area.SetActive(true);
            }
        }
    }
}
