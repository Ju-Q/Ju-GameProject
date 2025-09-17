using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyAttackController : MonoBehaviour
{
    public List<EnemyAttack> AttackSequence; // 攻击顺序列表
    public List<int> AttackRepeatCount;      // 每种攻击重复次数

    private void Start()
    {
        StartCoroutine(PerformAttacks());
    }

    private IEnumerator PerformAttacks()
    {
        for (int i = 0; i < AttackSequence.Count; i++)
        {
            var attack = AttackSequence[i];
            int repeat = (i < AttackRepeatCount.Count) ? AttackRepeatCount[i] : 1;

            for (int r = 0; r < repeat; r++)
            {
                yield return attack.ExecuteAttack();
            }
        }
    }
}
