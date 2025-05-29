using UnityEngine;

public class PushBoxController : MonoBehaviour
{
    [Header("推箱检测")]
    public Transform raycastOrigin; // 角色射线起点（建议贴近脚部）
    public float raycastHeightOffset = -0.5f;
    public float detectDistance = 1.5f;
    public LayerMask boxLayer;

    [Header("推箱参数")]
    public float pushSpeed = 2.0f;
    public float maxPushDistance = 2.0f;

    private GameObject currentBox;
    private bool isPushing = false;
    private bool isBoxStillDetected = false;

    private Animator animator;
    private CharacterController characterController;

    private float pushStartTime = 0f;

    void Start()
    {
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
    }

    void Update()
    {
        DetectBox();

        if (!isPushing && Input.GetKeyDown(KeyCode.F) && currentBox != null)
        {
            EnterPushState();
        }

        if (isPushing)
        {
            HandlePushMovement();

            // ✅ 加入0.2秒退出缓冲
            if (Time.time - pushStartTime > 0.2f)
            {
                float distanceToBox = Vector3.Distance(transform.position, currentBox.transform.position);
                if (!isBoxStillDetected || distanceToBox > maxPushDistance)
                {
                    ExitPushState();
                }
            }
        }

    }

    void DetectBox()
    {
        isBoxStillDetected = false;

        Vector3 origin = raycastOrigin.position + Vector3.up * raycastHeightOffset;
        Vector3 direction = transform.forward;
        Ray ray = new Ray(origin, direction);

        Debug.DrawRay(origin, direction * detectDistance, Color.red);

        if (Physics.Raycast(ray, out RaycastHit hit, detectDistance, boxLayer))
        {
            GameObject hitBox = hit.collider.gameObject;

            // 如果正在推，只检查是否仍然检测到当前箱子
            if (isPushing)
            {
                if (hitBox == currentBox)
                {
                    isBoxStillDetected = true;
                }
            }
            else
            {
                currentBox = hitBox;
            }
        }
        else
        {
            if (!isPushing)
            {
                currentBox = null;
            }
        }
    }



    void EnterPushState()
    {
        isPushing = true;
        pushStartTime = Time.time; // ← 这一句确保加入
        animator.SetBool("IsPushing", true);

        Vector3 boxDir = (currentBox.transform.position - transform.position).normalized;
        boxDir.y = 0;
        transform.rotation = Quaternion.LookRotation(boxDir);

        Debug.Log("进入推箱状态: " + currentBox.name);
    }


    void ExitPushState()
    {
        isPushing = false;
        animator.SetBool("IsPushing", false);
        currentBox = null;
        Debug.Log("退出推箱状态");
    }

    void HandlePushMovement()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 inputDir = new Vector3(h, 0f, v);

        if (inputDir.magnitude > 0.1f)
        {
            Vector3 moveDir = transform.forward * inputDir.z;
            Vector3 pushDir = moveDir.normalized * pushSpeed * Time.deltaTime;

            // 推动箱子和角色
            characterController.Move(pushDir);
            currentBox.transform.position += pushDir;

            animator.speed = 1.0f; // 播放动画
        }
        else
        {
            animator.speed = 0f; // 停止动画
        }
    }
}
