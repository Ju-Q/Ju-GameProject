using UnityEngine;

public class CanvasActivator : MonoBehaviour
{
    [Header("要激活的Canvas")]
    public GameObject canvasToActivate; // 拖拽你的Canvas物体（或UI面板）

    private bool isPlayerInZone = false;

    private void Start()
    {
        if (canvasToActivate != null)
        {
            canvasToActivate.SetActive(false); // 确保默认是隐藏的
        }
    }

    private void Update()
    {
        if (isPlayerInZone && Input.GetKeyDown(KeyCode.F))
        {
            if (canvasToActivate != null)
            {
                canvasToActivate.SetActive(true);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInZone = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInZone = false;
        }
    }
}
