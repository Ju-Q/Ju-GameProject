using UnityEngine;

public class SeesawController : MonoBehaviour
{
    public string boxLayerName = "BoxLayer";
    public string playerTag = "Player";  // 主角用Tag区分

    private void OnCollisionEnter(Collision collision)
    {
        // 只允许主角和BoxLayer的物体产生影响
        if (collision.collider.CompareTag(playerTag) ||
            collision.collider.gameObject.layer == LayerMask.NameToLayer(boxLayerName))
        {
            // 碰到的物体可影响跷跷板（默认物理引擎就会处理重力和力矩）
        }
        else
        {
            // 对于不允许的物体，可以直接忽略碰撞
            Physics.IgnoreCollision(collision.collider, GetComponent<Collider>());
        }
    }
}
