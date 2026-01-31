using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("追踪目标")]
    [SerializeField] private Transform target;
    [SerializeField] private float smoothTime = 0.3f;
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10);

    [Header("死区设置 (Dead Zone)")]
    [SerializeField] private float thresholdX = 1f;
    [SerializeField] private float thresholdY = 0.5f;

    [Header("边界限制 (Map Bounds)")]
    [SerializeField] private bool useBounds = true;
    [SerializeField] private Vector2 minBounds; // 地图左下角坐标
    [SerializeField] private Vector2 maxBounds; // 地图右上角坐标

    private Vector3 currentVelocity = Vector3.zero;
    private Vector3 targetPos;

    void LateUpdate()
    {
        if (target == null) return;

        targetPos = target.position + offset;

        float deltaX = target.position.x - transform.position.x;
        float deltaY = target.position.y - transform.position.y;

        if (Mathf.Abs(deltaX) < thresholdX) targetPos.x = transform.position.x;
        if (Mathf.Abs(deltaY) < thresholdY) targetPos.y = transform.position.y;

        if (useBounds)
        {
            targetPos.x = Mathf.Clamp(targetPos.x, minBounds.x, maxBounds.x);
            targetPos.y = Mathf.Clamp(targetPos.y, minBounds.y, maxBounds.y);
        }

        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref currentVelocity, smoothTime);
    }

    private void OnDrawGizmos()
    {
        if (!useBounds) return;
        Gizmos.color = Color.red;
        Vector3 center = new Vector3((minBounds.x + maxBounds.x) / 2, (minBounds.y + maxBounds.y) / 2, 0);
        Vector3 size = new Vector3(maxBounds.x - minBounds.x, maxBounds.y - minBounds.y, 1);
        Gizmos.DrawWireCube(center, size);
    }
}
