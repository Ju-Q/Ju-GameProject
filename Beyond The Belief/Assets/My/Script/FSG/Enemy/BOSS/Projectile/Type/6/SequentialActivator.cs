using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SequentialActivator : MonoBehaviour
{
    [Header("需要依次开启的物体列表")]
    public List<GameObject> objectsToActivate = new List<GameObject>();

    [Header("每个物体之间的延迟（秒）")]
    public float interval = 0.5f;

    private bool hasActivatedAll = false; // ✅ 所有物体是否已经开启过

    /// <summary>
    /// 外部调用：开始依次开启物体
    /// </summary>
    public void ActivateSequentially()
    {
        if (hasActivatedAll) return; // 已经开启过就不再执行
        StartCoroutine(ActivateCoroutine());
    }

    private IEnumerator ActivateCoroutine()
    {
        for (int i = 0; i < objectsToActivate.Count; i++)
        {
            GameObject obj = objectsToActivate[i];
            if (obj != null && !obj.activeSelf)
            {
                obj.SetActive(true);
                Debug.Log($"[SequentialActivator] 激活 {obj.name}");
            }

            // 等待间隔
            yield return new WaitForSeconds(interval);
        }

        hasActivatedAll = true; // 所有物体已经开启过
    }

    /// <summary>
    /// 可选：重置，允许再次调用
    /// </summary>
    public void ResetActivator()
    {
        hasActivatedAll = false;
        foreach (var obj in objectsToActivate)
        {
            if (obj != null)
                obj.SetActive(false);
        }
    }
}
