using UnityEngine;

public class TriggerAccelerateMove : MonoBehaviour
{
    [Header("需要进入触发区域的物体A")]
    public GameObject objectA;

    [Header("要移动的物体B")]
    public Transform objectToMove;

    [Header("物体B要移动到的位置")]
    public Transform targetPosition;

    [Header("初始速度")]
    public float initialSpeed = 1f;

    [Header("加速度（每秒增加）")]
    public float acceleration = 2f;

    [Header("到达目标的最小距离")]
    public float stopDistance = 0.1f;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!hasTriggered && other.gameObject == objectA)
        {
            hasTriggered = true;
            StartCoroutine(MoveWithAcceleration());
        }
    }

    private System.Collections.IEnumerator MoveWithAcceleration()
    {
        float currentSpeed = initialSpeed;

        while (Vector3.Distance(objectToMove.position, targetPosition.position) > stopDistance)
        {
            Vector3 direction = (targetPosition.position - objectToMove.position).normalized;
            objectToMove.position += direction * currentSpeed * Time.deltaTime;

            currentSpeed += acceleration * Time.deltaTime;

            yield return null;
        }

        // 精准对齐目标
        objectToMove.position = targetPosition.position;
    }
}
