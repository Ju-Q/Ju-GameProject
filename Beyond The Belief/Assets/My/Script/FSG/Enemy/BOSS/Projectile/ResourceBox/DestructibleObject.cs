using UnityEngine;

public class DestructibleObject : MonoBehaviour
{
    [Header("可破坏物体设置")]
    public int hitsToDestroy = 3;         // 需要命中几次才会被销毁
    public GameObject hitEffectPrefab;    // 受击时的特效（比如火花）
    public GameObject destroyEffect;      // 销毁时的特效

    private int currentHits = 0;

    public void TakeHit()
    {
        currentHits++;

        // 每次受击时播放受击特效
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
        }

        Debug.Log($"{name} 被击中 {currentHits}/{hitsToDestroy}");

        if (currentHits >= hitsToDestroy)
        {
            if (destroyEffect != null)
            {
                Instantiate(destroyEffect, transform.position, Quaternion.identity);
            }
            Destroy(gameObject);
        }
    }
}
