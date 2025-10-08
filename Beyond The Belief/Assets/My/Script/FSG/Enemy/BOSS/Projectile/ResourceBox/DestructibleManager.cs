using UnityEngine;

public class DestructibleManager : MonoBehaviour
{
    [Header("要监听的可破坏物体")]
    public DestructibleObject[] destructibleObjects;

    [Header("被激活的物体（开启 Collider）")]
    public GameObject targetObject;

    private Collider targetCollider;

    private void Start()
    {
        // ✅ 给每个可破坏物体绑定事件
        foreach (var obj in destructibleObjects)
        {
            if (obj != null)
                obj.OnDestroyed += HandleObjectDestroyed;
        }

        // ✅ 缓存 Collider 引用，并关闭
        if (targetObject != null)
        {
            targetCollider = targetObject.GetComponent<Collider>();
            if (targetCollider != null)
                targetCollider.enabled = false;
            else
                Debug.LogWarning($"{targetObject.name} 上没有找到任何 Collider！");
        }
    }

    private void HandleObjectDestroyed(DestructibleObject destroyedObj)
    {
        Debug.Log($"{destroyedObj.name} 被摧毁，开启 {targetObject.name} 的 Collider");

        if (targetCollider != null)
        {
            targetCollider.enabled = true;
        }
        else
        {
            Debug.LogWarning($"{targetObject.name} 没有可启用的 Collider！");
        }

        // ⚠️ 如果只要触发一次，可以解绑所有事件
        foreach (var obj in destructibleObjects)
        {
            if (obj != null)
                obj.OnDestroyed -= HandleObjectDestroyed;
        }
    }
}
