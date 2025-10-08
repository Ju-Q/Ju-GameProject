using System.Collections.Generic;
using UnityEngine;

public class DestructionWatcher : MonoBehaviour
{
    [Header("要侦测的物体（可包含任意GameObject）")]
    public GameObject[] watchedObjects;

    [Header("被激活的物体（开启 Collider）")]
    public GameObject targetObject;

    private Collider targetCollider;
    private List<GameObject> validObjects = new List<GameObject>();
    private bool colliderActivated = false;

    private void Start()
    {
        // 缓存 Collider 并禁用
        if (targetObject != null)
        {
            targetCollider = targetObject.GetComponent<Collider>();
            if (targetCollider != null)
                targetCollider.enabled = false;
            else
                Debug.LogWarning($"{targetObject.name} 上没有 Collider！");
        }

        // 过滤掉空引用
        foreach (var obj in watchedObjects)
        {
            if (obj != null)
                validObjects.Add(obj);
        }
    }

    private void Update()
    {
        // 已激活就不再检测
        if (colliderActivated)
            return;

        // 遍历检测是否有被销毁或禁用的对象
        for (int i = 0; i < validObjects.Count; i++)
        {
            var obj = validObjects[i];

            if (obj == null || !obj.activeInHierarchy)
            {
                ActivateTargetCollider();
                colliderActivated = true;
                return;
            }
        }
    }

    private void ActivateTargetCollider()
    {
        if (targetCollider != null)
        {
            targetCollider.enabled = true;
            Debug.Log($"检测到物体被销毁，已开启 {targetObject.name} 的 Collider！");
        }
        else
        {
            Debug.LogWarning("未找到可启用的 Collider。");
        }
    }
}
