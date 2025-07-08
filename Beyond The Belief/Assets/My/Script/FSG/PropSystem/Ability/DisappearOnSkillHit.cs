using UnityEngine;

public class DisappearOnSkillHit : MonoBehaviour
{
    // 设置一个可选的Tag，用于确认是技能触发器触碰了它（可根据需要设置）
    public string skillTriggerTag = "SkillTrigger";

    private void OnTriggerEnter(Collider other)
    {
        // 判断是否是技能范围 Trigger
        if (other.CompareTag(skillTriggerTag))
        {
            Debug.Log("被技能击中，消失！");
            // 方式1：禁用
            gameObject.SetActive(false);

            // 方式2：销毁
            // Destroy(gameObject);
        }
    }
}
