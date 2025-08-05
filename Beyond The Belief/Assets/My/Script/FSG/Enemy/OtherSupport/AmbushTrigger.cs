using UnityEngine;

public class AmbushTrigger : MonoBehaviour
{
    [Header("需要激活的埋伏敌人列表")]
    public EnemyAmbushAI[] ambushEnemies; // 支持多个敌人

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            foreach (var enemy in ambushEnemies)
            {
                if (enemy != null)
                {
                    enemy.ActivateAmbush();
                }
            }

            gameObject.SetActive(false); // 防止重复触发
        }
    }
}
