using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ShowObjectTrigger: MonoBehaviour
{
    [Header("触发显示的物体")]
    public GameObject[] targetObjects;

    [Header("延迟设置（秒）")]
    public float enterDelay = 0.2f;  // 进入区域显示延迟
    public float exitDelay = 0.5f;   // 离开区域隐藏延迟

    [Header("玩家检测标签")]
    public string playerTag = "Player";

    private Coroutine currentRoutine;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(ChangeObjectStateAfterDelay(true, enterDelay));
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(ChangeObjectStateAfterDelay(false, exitDelay));
    }

    private IEnumerator ChangeObjectStateAfterDelay(bool show, float delay)
    {
        yield return new WaitForSeconds(delay);

        foreach (var obj in targetObjects)
        {
            if (obj != null)
                obj.SetActive(show);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Collider col = GetComponent<Collider>();
        if (col != null)
            Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
    }
}
