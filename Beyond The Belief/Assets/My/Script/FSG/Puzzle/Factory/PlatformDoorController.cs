using UnityEngine;
using System.Collections.Generic;

public class PlatformDoorController : MonoBehaviour
{
    [Header("Platform Settings")]
    public Transform platform;
    public Transform platformEndPos;
    public Transform platformBoxPos;
    public float moveTime = 2f;
    public float boxMoveTime = 3f;
    public float boxHoldTime = 1f;

    [Header("Door Settings")]
    public Transform door;
    public Transform doorEndPos;
    public Transform doorBoxPos;

    [Header("Detection Settings")]
    public Transform checkCenter;
    public Vector3 checkBoxSize = new Vector3(1, 2, 1);
    public LayerMask playerLayer;
    public LayerMask boxLayer;
    public float playerLeaveDelay = 0.2f;

    // 状态数据
    private Vector3 platformStartPos;
    private Vector3 doorStartPos;
    private float leaveTimer = 0f;
    private float boxTimer = 0f;

    // 匀加速运动参数
    private Vector3 platformMoveTarget;
    private Vector3 doorMoveTarget;
    private Vector3 platformMoveStart;
    private Vector3 doorMoveStart;
    private float moveElapsed = 0f;
    private float totalDuration = 0f;

    // 记录平台上的箱子
    private List<Transform> boxesOnPlatform = new List<Transform>();

    void Start()
    {
        platformStartPos = platform.position;
        doorStartPos = door.position;

        if (checkCenter == null)
        {
            GameObject temp = new GameObject("CheckCenter");
            temp.transform.SetParent(transform);
            temp.transform.position = platform.position + Vector3.up * 1f;
            checkCenter = temp.transform;
        }

        // 给所有子物体添加碰撞转发器
        AddCollisionRelayToChildren();
    }

    void Update()
    {
        // 玩家检测
        bool playerDetected = Physics.CheckBox(
            checkCenter.position,
            checkBoxSize * 0.5f,
            Quaternion.identity,
            playerLayer
        );

        // 箱子检测
        bool boxDetected = Physics.CheckBox(
            checkCenter.position,
            checkBoxSize * 0.5f,
            Quaternion.identity,
            boxLayer
        );

        // 玩家缓冲
        if (playerDetected)
            leaveTimer = playerLeaveDelay;
        else
            leaveTimer -= Time.deltaTime;

        bool playerActive = leaveTimer > 0f;

        // 默认状态
        Vector3 newPlatformTarget = platformStartPos;
        Vector3 newDoorTarget = doorStartPos;
        float newDuration = moveTime;

        if (playerActive)
        {
            // 玩家触发
            newPlatformTarget = platformEndPos.position;
            newDoorTarget = doorEndPos.position;
            newDuration = moveTime;
            boxTimer = 0f;
        }
        else if (boxDetected || boxTimer > 0f)
        {
            if (boxDetected)
                boxTimer = boxHoldTime;
            else
                boxTimer -= Time.deltaTime;

            if (boxTimer > 0f)
            {
                // 箱子触发
                newPlatformTarget = platformBoxPos.position;
                newDoorTarget = doorBoxPos.position;
                newDuration = boxMoveTime;
            }
            else
            {
                // 回到初始
                newPlatformTarget = platformStartPos;
                newDoorTarget = doorStartPos;
                newDuration = boxMoveTime;
            }
        }

        // 如果目标改变 → 重置加速度运动
        if (newPlatformTarget != platformMoveTarget || newDoorTarget != doorMoveTarget)
        {
            platformMoveStart = platform.position;
            doorMoveStart = door.position;
            platformMoveTarget = newPlatformTarget;
            doorMoveTarget = newDoorTarget;
            totalDuration = newDuration;
            moveElapsed = 0f;
        }

        // 匀加速更新
        moveElapsed += Time.deltaTime;
        float t = Mathf.Clamp01(moveElapsed / totalDuration);

        // 平台
        Vector3 pDelta = platformMoveTarget - platformMoveStart;
        float pDist = pDelta.magnitude;
        float aP = (2 * pDist) / (totalDuration * totalDuration);
        float sP = 0.5f * aP * (moveElapsed * moveElapsed);
        Vector3 newPlatformPos = platformMoveStart + pDelta.normalized * Mathf.Min(sP, pDist);

        // 门（用同样的时间）
        Vector3 dDelta = doorMoveTarget - doorMoveStart;
        float dDist = dDelta.magnitude;
        float aD = (2 * dDist) / (totalDuration * totalDuration);
        float sD = 0.5f * aD * (moveElapsed * moveElapsed);
        Vector3 newDoorPos = doorMoveStart + dDelta.normalized * Mathf.Min(sD, dDist);

        // 平台位移差（让箱子跟随）
        Vector3 platformMoveDelta = newPlatformPos - platform.position;

        platform.position = newPlatformPos;
        door.position = newDoorPos;

        // 平台上的箱子跟随
        foreach (Transform box in boxesOnPlatform)
        {
            if (box != null)
                box.position += platformMoveDelta;
        }
    }

    // 平台碰撞进入
    public void OnPlatformCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Box"))
        {
            if (!boxesOnPlatform.Contains(collision.transform))
                boxesOnPlatform.Add(collision.transform);
        }
    }

    // 平台碰撞退出
    public void OnPlatformCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Box"))
        {
            boxesOnPlatform.Remove(collision.transform);
        }
    }

    // 给子物体挂转发器
    private void AddCollisionRelayToChildren()
    {
        Collider[] colliders = platform.GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            if (col.gameObject.GetComponent<PlatformCollisionRelay>() == null)
            {
                PlatformCollisionRelay relay = col.gameObject.AddComponent<PlatformCollisionRelay>();
                relay.controller = this;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (checkCenter != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(checkCenter.position, checkBoxSize);
        }
    }
}

// 碰撞转发器
public class PlatformCollisionRelay : MonoBehaviour
{
    [HideInInspector] public PlatformDoorController controller;

    void OnCollisionEnter(Collision collision)
    {
        controller.OnPlatformCollisionEnter(collision);
    }

    void OnCollisionExit(Collision collision)
    {
        controller.OnPlatformCollisionExit(collision);
    }
}
