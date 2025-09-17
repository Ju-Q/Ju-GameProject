using UnityEngine;

public class SeesawTrigger : MonoBehaviour
{
    public Transform seesawBoard;  // 木板
    public float playerWeight = 50f; // 主角重量
    public string playerTag = "Player";
    public string boxLayerName = "BoxLayer";

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            AddWeight(other.transform.position, playerWeight);
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer(boxLayerName))
        {
            Rigidbody boxRb = other.attachedRigidbody;
            if (boxRb != null)
            {
                AddWeight(other.transform.position, boxRb.mass);
            }
        }
    }

    private void AddWeight(Vector3 pos, float weight)
    {
        Rigidbody rb = seesawBoard.GetComponent<Rigidbody>();
        if (rb == null) return;

        // 计算施力点（本地位置）
        Vector3 localPos = seesawBoard.InverseTransformPoint(pos);
        // 力作用方向（往下）
        Vector3 forceDir = Vector3.down * weight * 9.8f;
        rb.AddForceAtPosition(forceDir, pos);
    }
}
