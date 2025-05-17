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
    /// ���õ�ǰ���������������
    /// </summary>
    /// <param name="respawnPoint">ָ��������λ��</param>
    public void SetCurrentRespawnPoint(Transform respawnPoint)
    {
        currentRespawnPoint = respawnPoint;
    }

    /// <summary>
    /// ��ȡ��ǰ��������
    /// </summary>
    /// <returns>�������Transform</returns>
    public Transform GetCurrentRespawnPoint()
    {
        return currentRespawnPoint;
    }
}
