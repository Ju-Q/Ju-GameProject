using UnityEngine;

[RequireComponent(typeof(Collider))]
public abstract class ProjectileBase: MonoBehaviour
{
    protected ProjectileHitHandler hitHandler;

    protected virtual void Start()
    {
        hitHandler = GetComponent<ProjectileHitHandler>();
    }

    // 每种子弹都要实现自己的 Move()
    protected abstract void Move();

    protected virtual void Update()
    {
        Move();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hitHandler != null)
            hitHandler.HandleHit(other.gameObject);

        Destroy(gameObject); // 命中后销毁（可在子类里改）
    }
}
