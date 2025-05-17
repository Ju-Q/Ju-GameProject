using UnityEngine;

public class RespawnManager : MonoBehaviour
{
    public static RespawnManager Instance { get; private set; }

    private Transform currentRespawnPoint;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// 设置当前触发区域的重生点
    /// </summary>
    /// <param name="respawnPoint">指定的重生位置</param>
    public void SetCurrentRespawnPoint(Transform respawnPoint)
    {
        currentRespawnPoint = respawnPoint;
    }

    /// <summary>
    /// 获取当前的重生点
    /// </summary>
    /// <returns>重生点的Transform</returns>
    public Transform GetCurrentRespawnPoint()
    {
        return currentRespawnPoint;
    }
}
