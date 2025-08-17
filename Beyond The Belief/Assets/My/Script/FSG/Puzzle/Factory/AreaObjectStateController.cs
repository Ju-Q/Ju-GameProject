using UnityEngine;
using System.Collections;

public class AreaObjectStateController : MonoBehaviour
{
    [Header("玩家标签")]
    public string playerTag = "Player";

    [Header("进入区域时保持开启的物体")]
    public GameObject[] objectsToKeepOn;

    [Header("进入区域时保持关闭的物体")]
    public GameObject[] objectsToKeepOff;

    [Header("进入区域延迟（秒）")]
    public float enterDelay = 0f;

    [Header("离开区域延迟（秒）")]
    public float exitDelay = 0f;

    // 存储原始状态
    private bool[] originalOnStates;
    private bool[] originalOffStates;

    private Coroutine enterCoroutine;
    private Coroutine exitCoroutine;

    private void Start()
    {
        // 记录初始状态
        originalOnStates = new bool[objectsToKeepOn.Length];
        for (int i = 0; i < objectsToKeepOn.Length; i++)
        {
            if (objectsToKeepOn[i] != null)
                originalOnStates[i] = objectsToKeepOn[i].activeSelf;
        }

        originalOffStates = new bool[objectsToKeepOff.Length];
        for (int i = 0; i < objectsToKeepOff.Length; i++)
        {
            if (objectsToKeepOff[i] != null)
                originalOffStates[i] = objectsToKeepOff[i].activeSelf;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            if (exitCoroutine != null)
                StopCoroutine(exitCoroutine); // 防止退出恢复还没结束

            enterCoroutine = StartCoroutine(HandleEnter());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            if (enterCoroutine != null)
                StopCoroutine(enterCoroutine); // 防止进入状态还没结束

            exitCoroutine = StartCoroutine(HandleExit());
        }
    }

    private IEnumerator HandleEnter()
    {
        if (enterDelay > 0f)
            yield return new WaitForSeconds(enterDelay);

        // 保持这些物体开启
        foreach (GameObject obj in objectsToKeepOn)
        {
            if (obj != null)
                obj.SetActive(true);
        }

        // 保持这些物体关闭
        foreach (GameObject obj in objectsToKeepOff)
        {
            if (obj != null)
                obj.SetActive(false);
        }
    }

    private IEnumerator HandleExit()
    {
        if (exitDelay > 0f)
            yield return new WaitForSeconds(exitDelay);

        // 恢复开启组原始状态
        for (int i = 0; i < objectsToKeepOn.Length; i++)
        {
            if (objectsToKeepOn[i] != null)
                objectsToKeepOn[i].SetActive(originalOnStates[i]);
        }

        // 恢复关闭组原始状态
        for (int i = 0; i < objectsToKeepOff.Length; i++)
        {
            if (objectsToKeepOff[i] != null)
                objectsToKeepOff[i].SetActive(originalOffStates[i]);
        }
    }
}
