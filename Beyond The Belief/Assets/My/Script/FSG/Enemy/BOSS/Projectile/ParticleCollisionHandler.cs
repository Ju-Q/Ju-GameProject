using UnityEngine;

public class ParticleCollisionHandler : MonoBehaviour
{
    // 这里可以直接拖你的 ProjectileHitHandler 脚本进来
    public ProjectileHitHandler hitHandler;

    private void OnParticleCollision(GameObject other)
    {
        // 这个回调自动触发，other 是粒子碰撞到的对象
        if (hitHandler != null)
        {
            hitHandler.HandleHit(other);
        }
    }
}
