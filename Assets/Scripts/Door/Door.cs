using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public MaskType requiredType; // 开启此门所需的具体面具类型
    public GameObject targetDoor;

    public bool TryOpen(MaskType playerMaskType)
    {
        if (playerMaskType == requiredType)
        {
            ExecuteOpen();
            return true;
        }

        
        return false;
    }

    private void ExecuteOpen()
    {
        if (targetDoor != null)
        {
            Destroy(targetDoor);
        }

        Destroy(gameObject);
    }
}
