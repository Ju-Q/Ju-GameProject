using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class WallShadowTrigger : MonoBehaviour
{
    [Header("触发对应的墙体组")]
    public WallShadowGroup[] wallGroups;

    [Header("延迟设置")]
    public float enterDelay = 0.2f;
    public float exitDelay = 0.5f;

    [Header("玩家检测标签")]
    public string playerTag = "Player";

    private Coroutine currentRoutine;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(ChangeShadowStateAfterDelay(true, enterDelay));
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(ChangeShadowStateAfterDelay(false, exitDelay));
    }

    private IEnumerator ChangeShadowStateAfterDelay(bool shadowOnly, float delay)
    {
        yield return new WaitForSeconds(delay);

        foreach (var group in wallGroups)
        {
            if (group == null) continue;

            if (shadowOnly)
                group.HideWalls();
            else
                group.ShowWalls();
        }
    }


    /*private void OnDrawGizmos()
    {
        MeshCollider meshCollider = GetComponent<MeshCollider>();
        if (meshCollider != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(meshCollider.bounds.center, meshCollider.bounds.size);
        }
    }*/

}
