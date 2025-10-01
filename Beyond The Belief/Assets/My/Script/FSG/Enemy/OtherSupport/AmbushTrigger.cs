using UnityEngine;

public class AmbushTrigger : MonoBehaviour
{
    [Header("需要激活的埋伏敌人列表（普通小怪）")]
    public EnemyAmbushAI[] ambushEnemies; // 支持多个敌人

    [Header("需要激活的埋伏敌人列表（Boss小怪）")]
    public BossMiniAmbushAI[] bossAmbushEnemies; // 支持多个Boss小怪

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 激活普通小怪
            foreach (var enemy in ambushEnemies)
            {
                if (enemy != null)
                {
                    enemy.ActivateAmbush();
                }
            }

            // 激活Boss小怪
            foreach (var bossEnemy in bossAmbushEnemies)
            {
                if (bossEnemy != null)
                {
                    bossEnemy.ActivateAmbush();
                }
            }

            gameObject.SetActive(false); // 防止重复触发
        }
    }
}
