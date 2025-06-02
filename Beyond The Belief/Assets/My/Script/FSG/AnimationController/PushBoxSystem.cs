using UnityEngine;

public class PushBoxSystem : MonoBehaviour
{
    [Header("References")]
    public Animator playerAnimator;
    public CharacterController characterController;
    public Transform playerTransform;

    [Header("Settings")]
    public float normalSpeed = 5f;
    public float pushSpeed = 2f;
    public float checkDistance = 1.2f;
    public LayerMask boxLayer;
    public float positionCorrectionStrength = 5f;

    [Header("Debug")]
    public bool showDebugRays = true;
    public float raycastHeight = 0.5f;

    // Internal
    private GameObject currentBox;
    private bool isPushing = false;
    private Vector3 pushDirection;
    private readonly Vector3[] checkDirections = new Vector3[]
    {
        Vector3.forward,
        Vector3.back,
        Vector3.right,
        Vector3.left
    };

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (isPushing)
            {
                ReleaseBox();
            }
            else
            {
                TryBindBox();
            }
        }

        HandlePushMovement();
    }

    void TryBindBox()
    {
        Vector3 rayStart = playerTransform.position + Vector3.up * raycastHeight;

        foreach (var direction in checkDirections)
        {
            if (showDebugRays)
                Debug.DrawRay(rayStart, direction * checkDistance, Color.green, 2f);

            if (Physics.Raycast(rayStart, direction, out RaycastHit hit, checkDistance, boxLayer))
            {
                Rigidbody rb = hit.collider.GetComponent<Rigidbody>();
                if (rb == null) continue;

                currentBox = hit.collider.gameObject;
                isPushing = true;
                pushDirection = direction;

                // 设置角色面向箱子
                playerTransform.forward = direction;

                playerAnimator.SetTrigger("PushIdle");
                playerAnimator.SetBool("IsPushing", true);
                playerAnimator.SetBool("IsPulling", false);
                return;
            }
        }
    }

    void ReleaseBox()
    {
        isPushing = false;

        if (currentBox != null && currentBox.TryGetComponent<Rigidbody>(out var rb))
            rb.velocity = Vector3.zero;

        currentBox = null;

        playerAnimator.SetBool("IsPushing", false);
        playerAnimator.SetBool("IsPulling", false);
        playerAnimator.ResetTrigger("PushIdle");
    }

    void HandlePushMovement()
    {
        if (!isPushing || currentBox == null) return;

        float moveInput = 0f;
        bool isPulling = false;

        if (pushDirection == Vector3.forward || pushDirection == Vector3.back)
        {
            moveInput = Input.GetAxis("Vertical");
            isPulling = (pushDirection == Vector3.forward && moveInput > 0) ||
                        (pushDirection == Vector3.back && moveInput < 0);
        }
        else
        {
            moveInput = Input.GetAxis("Horizontal");
            isPulling = (pushDirection == Vector3.right && moveInput < 0) ||
                        (pushDirection == Vector3.left && moveInput > 0);
        }

        if (Mathf.Abs(moveInput) > 0.1f)
        {
            playerAnimator.SetBool("IsPulling", isPulling);
            playerAnimator.SetBool("PushIdle", false);

            // 拉时面向相反方向
            playerTransform.forward = isPulling ? -pushDirection : pushDirection;

            Vector3 moveVector = pushDirection * moveInput * pushSpeed * Time.deltaTime;
            MoveBoxAndPlayerTogether(moveVector);
        }
        else
        {
            playerAnimator.SetBool("PushIdle", true);
            playerAnimator.SetBool("IsPulling", false);
        }
    }

    void MoveBoxAndPlayerTogether(Vector3 moveVector)
    {
        if (!CanBoxMove(moveVector))
        {
            playerAnimator.SetBool("PushIdle", true);
            playerAnimator.SetBool("IsPulling", false);
            return;
        }

        // Move box
        if (currentBox.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.MovePosition(rb.position + moveVector);
        }
        else
        {
            currentBox.transform.position += moveVector;
        }

        // Move player
        characterController.Move(moveVector);

        // Distance correction
        float currentDistance = Vector3.Distance(currentBox.transform.position, playerTransform.position);
        float desiredDistance = checkDistance * 0.9f;
        if (currentDistance > desiredDistance)
        {
            Vector3 correction = (currentBox.transform.position - playerTransform.position).normalized *
                                 (currentDistance - desiredDistance) * positionCorrectionStrength * Time.deltaTime;
            characterController.Move(correction);
        }
    }

    bool CanBoxMove(Vector3 moveVector)
    {
        if (Physics.BoxCast(
            currentBox.transform.position,
            currentBox.GetComponent<Collider>().bounds.extents * 0.9f,
            moveVector.normalized,
            out RaycastHit hit,
            currentBox.transform.rotation,
            moveVector.magnitude + 0.1f,
            ~boxLayer))
        {
            if (!hit.collider.isTrigger && hit.collider.gameObject != gameObject)
            {
                if (showDebugRays)
                    Debug.DrawRay(currentBox.transform.position, moveVector, Color.red, 1f);
                return false;
            }
        }
        return true;
    }

    void OnDrawGizmosSelected()
    {
        if (playerTransform == null) return;

        Gizmos.color = Color.blue;
        foreach (var dir in checkDirections)
        {
            Gizmos.DrawLine(playerTransform.position, playerTransform.position + dir * checkDistance);
        }

        if (currentBox != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(currentBox.transform.position, currentBox.GetComponent<Collider>().bounds.size * 0.9f);
        }
    }
}
