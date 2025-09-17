using UnityEngine;
using System.Collections;

public abstract class EnemyAttack : MonoBehaviour
{
    // 执行一次完整攻击的协程
    public abstract IEnumerator ExecuteAttack();
}
