using UnityEngine;

public class SelfDestruct : MonoBehaviour
{
    [Tooltip("物体在多少秒后销毁")]
    public float destroyAfterSeconds = 5f;

    void Start()
    {
        // 在指定时间后销毁当前物体
        Destroy(gameObject, destroyAfterSeconds);
    }
}
