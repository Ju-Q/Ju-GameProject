using UnityEngine;

public class ProjectileHitHandler : MonoBehaviour
{
    public void HandleHit(GameObject target)
    {
        if (!target.CompareTag("Player"))
            return;

        // 优先查找玩家上的 PlayerHealth 组件并调用
        var ph = target.GetComponent<PlayerHealth>();
        if (ph != null)
        {
            ph.TakeHit();
        }
        else
        {
            // 备用方案：如果忘记挂组件，给个提示并临时添加一个（推荐在编辑器里手动挂）
            Debug.LogWarning("ProjectileHitHandler: PlayerHealth component not found on target. Adding one at runtime with default settings.");
            ph = target.AddComponent<PlayerHealth>();
            ph.TakeHit();
        }
    }
}