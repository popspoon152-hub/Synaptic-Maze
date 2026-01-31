using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public MaskType requiredType; // 开启此门所需的具体面具类型
    public Sprite unlockedSprite; // 锁孔解锁后的贴图
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
        //更换贴图
        if (unlockedSprite != null)
            GetComponent<SpriteRenderer>().sprite = unlockedSprite;

        if (targetDoor != null)
        {
            //禁用Collider2D
            Collider2D[] colliders = targetDoor.GetComponentsInChildren<Collider2D>();
            foreach (var col in colliders)
            {
                col.enabled = false;
            }

            //触发开门动画
            if (targetDoor.TryGetComponent<Animator>(out var anim))
            {
                anim.SetTrigger("Open");
            }
        }
        this.enabled = false;
        gameObject.layer = LayerMask.NameToLayer("Default");
    }
}
