using UnityEngine;
using System.Collections;

public class ToggleObjectsOnTrigger : MonoBehaviour
{
    [Header("延迟时间(秒)")]
    public float delay = 1f;

    [Header("需要切换的物体")]
    public GameObject[] targetObjects;

    [Header("触发一次后是否销毁该Trigger")]
    public bool destroyAfterTrigger = false;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        // 检查是否是玩家进入
        if (!hasTriggered && other.CompareTag("Player"))
        {
            hasTriggered = true;
            StartCoroutine(ToggleAfterDelay());
        }
    }

    private IEnumerator ToggleAfterDelay()
    {
        // 等待延迟
        yield return new WaitForSeconds(delay);

        // 切换所有物体状态
        foreach (GameObject obj in targetObjects)
        {
            if (obj != null)
            {
                obj.SetActive(!obj.activeSelf);
            }
        }

        // 如果只触发一次
        if (destroyAfterTrigger)
        {
            Destroy(gameObject);
        }
    }
}
