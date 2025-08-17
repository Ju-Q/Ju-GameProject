using UnityEngine;
using System.Collections;
using StarterAssets;

public class CameraTriggerLook : MonoBehaviour
{
    public Transform npc;
    public Camera mainCamera;
    public float lookDuration = 2.0f;
    public float moveTime = 1.0f; // 摄像头移动到目标位置的总时间
    public Vector3 offsetFromNPC = new Vector3(0, 2, -5);

    [Header("Camera Move Curve")]
    public AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // 默认缓入缓出

    private bool hasTriggered = false;
    private Vector3 savedLocalPosition;
    private Quaternion savedLocalRotation;
    private Transform cameraParent;
    private ThirdPersonController playerController; // 引用玩家控制脚本

    void Start()
    {
        cameraParent = mainCamera.transform.parent;

        // 找到玩家控制器（假设摄像机的父物体就是玩家）
        playerController = cameraParent.GetComponent<ThirdPersonController>();
        if (playerController == null)
        {
            Debug.LogWarning("未找到 ThirdPersonController，请手动指定或检查父物体。");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!hasTriggered && other.CompareTag("Player"))
        {
            savedLocalPosition = mainCamera.transform.localPosition;
            savedLocalRotation = mainCamera.transform.localRotation;

            hasTriggered = true;
            StartCoroutine(LookAtNPC());
        }
    }

    IEnumerator LookAtNPC()
    {
        // 禁用玩家移动
        if (playerController != null)
            playerController.canMove = false;

        mainCamera.transform.SetParent(null);

        Vector3 targetPosition = npc.position + offsetFromNPC;
        Quaternion targetRotation = Quaternion.LookRotation(npc.position - targetPosition);

        // --- Step 1: 移动到 NPC ---
        yield return StartCoroutine(MoveCamera(mainCamera.transform.position, targetPosition,
                                               mainCamera.transform.rotation, targetRotation));

        yield return new WaitForSeconds(lookDuration);

        // --- Step 2: 返回玩家 ---
        Vector3 targetReturnPosition = cameraParent.TransformPoint(savedLocalPosition);
        Quaternion targetReturnRotation = cameraParent.rotation * savedLocalRotation;

        yield return StartCoroutine(MoveCamera(mainCamera.transform.position, targetReturnPosition,
                                               mainCamera.transform.rotation, targetReturnRotation));

        mainCamera.transform.SetParent(cameraParent);
        mainCamera.transform.localPosition = savedLocalPosition;
        mainCamera.transform.localRotation = savedLocalRotation;

        // 允许玩家移动
        if (playerController != null)
            playerController.canMove = true;
    }

    IEnumerator MoveCamera(Vector3 startPos, Vector3 endPos, Quaternion startRot, Quaternion endRot)
    {
        float elapsed = 0f;

        while (elapsed < moveTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / moveTime);

            float curveT = moveCurve.Evaluate(t);

            mainCamera.transform.position = Vector3.Lerp(startPos, endPos, curveT);
            mainCamera.transform.rotation = Quaternion.Slerp(startRot, endRot, curveT);

            yield return null;
        }
    }
}
