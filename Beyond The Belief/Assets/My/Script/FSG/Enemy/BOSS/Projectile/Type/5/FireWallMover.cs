using UnityEngine;

public class FireWallMover : MonoBehaviour
{
    [Header("移动参数")]
    public float speed = 5f;            // 推进速度（米/秒）
    public float maxDistance = 20f;     // 最大推进距离
    public float lifeTime = 5f;         // 最大存在时间（保险）

    [Header("伤害冷却")]
    public float damageCooldown = 3f;   // 同一个玩家触发伤害的冷却时间（秒）

    private Vector3 direction;
    private Vector3 startPosition;
    private float timer;

    private ProjectileHitHandler hitHandler;

    private float lastDamageTime = -Mathf.Infinity; // 上次触发伤害的时间

    private void Awake()
    {
        // 获取同物体上的 ProjectileHitHandler
        hitHandler = GetComponent<ProjectileHitHandler>();
        if (hitHandler == null)
        {
            Debug.LogWarning("FireWallMover: ProjectileHitHandler not found on prefab. 请挂在同一物体上！");
        }
    }

    /// <summary>
    /// 初始化火焰墙移动方向（只接受水平方向）
    /// </summary>
    public void Init(Vector3 dir)
    {
        dir.y = 0f;
        direction = dir.normalized;
        startPosition = transform.position;
        timer = 0f;

        // 自动旋转火焰墙面向移动方向
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        }
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;

        if (Vector3.Distance(startPosition, transform.position) >= maxDistance)
        {
            Destroy(gameObject);
        }

        timer += Time.deltaTime;
        if (timer >= lifeTime)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || hitHandler == null)
            return;

        // 检查冷却
        if (Time.time - lastDamageTime >= damageCooldown)
        {
            hitHandler.HandleHit(other.gameObject);
            lastDamageTime = Time.time; // 更新上次触发时间
        }
    }
}
