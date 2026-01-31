using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoverMask : PlayerMask
{
    private GameObject instantiatedOverlay;

    public override void ApplyEffect(PlayerController player)
    {
        //екеж
        if (maskCoverPrefab != null)
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            instantiatedOverlay = Instantiate(maskCoverPrefab, canvas.transform);
        }
    }

    public override void RemoveEffect(PlayerController player)
    {
        //ЯњЛй UI
        if (instantiatedOverlay != null) Destroy(instantiatedOverlay);
    }
}
