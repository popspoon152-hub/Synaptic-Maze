using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public enum MaskType 
{ 
    None, 
    PumpkinMask 
};

public abstract class PlayerMask : MonoBehaviour
{
    public MaskType maskType;
    public string maskName;
    public GameObject originalPrefab;
    public GameObject maskCoverPrefab;

    public abstract void ApplyEffect(PlayerController player);
    public abstract void RemoveEffect(PlayerController player);

    public virtual void BeConsumed()
    {
        Destroy(gameObject);
    }
}
