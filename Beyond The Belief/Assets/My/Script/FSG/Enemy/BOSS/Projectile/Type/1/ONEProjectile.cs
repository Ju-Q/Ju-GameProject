using UnityEngine;

public class ONEProjectile: MonoBehaviour
{
    public float Speed = 10f;      // 子弹速度
    public float LifeTime = 5f;    // 存活时间
    private ProjectileHitHandler hitHandler;
    private void Awake()
    {
        // 获取挂在同一个物体上的 HitHandler
        hitHandler = GetComponent<ProjectileHitHandler>();
    }
    private void Start()
    {
        Destroy(gameObject, LifeTime); // 过时销毁，避免场景里残留
    }

    private void Update()
    {
        transform.Translate(Vector3.forward * Speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Hit Player!");
            if (hitHandler != null)
            {
                hitHandler.HandleHit(other.gameObject);
            }
            Destroy(gameObject); // 碰到玩家后销毁
        }
    }
}
