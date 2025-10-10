using System.Collections.Generic;
using UnityEngine;

public class AreaObjectStateManager : MonoBehaviour
{
    // 单例（方便访问）
    public static AreaObjectStateManager Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // 每个对象的“被保持关闭/开启”的计数器
    private Dictionary<GameObject, int> offCounter = new Dictionary<GameObject, int>();
    private Dictionary<GameObject, int> onCounter = new Dictionary<GameObject, int>();
    private Dictionary<GameObject, bool> originalState = new Dictionary<GameObject, bool>();

    public void RequestKeepOff(GameObject obj)
    {
        if (obj == null) return;

        if (!originalState.ContainsKey(obj))
            originalState[obj] = obj.activeSelf;

        if (!offCounter.ContainsKey(obj))
            offCounter[obj] = 0;

        offCounter[obj]++;
        obj.SetActive(false);
    }

    public void ReleaseKeepOff(GameObject obj)
    {
        if (obj == null) return;
        if (!offCounter.ContainsKey(obj)) return;

        offCounter[obj]--;
        if (offCounter[obj] <= 0)
        {
            offCounter.Remove(obj);
            RestoreIfNoControl(obj);
        }
    }

    public void RequestKeepOn(GameObject obj)
    {
        if (obj == null) return;

        if (!originalState.ContainsKey(obj))
            originalState[obj] = obj.activeSelf;

        if (!onCounter.ContainsKey(obj))
            onCounter[obj] = 0;

        onCounter[obj]++;
        obj.SetActive(true);
    }

    public void ReleaseKeepOn(GameObject obj)
    {
        if (obj == null) return;
        if (!onCounter.ContainsKey(obj)) return;

        onCounter[obj]--;
        if (onCounter[obj] <= 0)
        {
            onCounter.Remove(obj);
            RestoreIfNoControl(obj);
        }
    }

    private void RestoreIfNoControl(GameObject obj)
    {
        // 当没有任何区域要求它保持开或关时，恢复原状态
        if (!offCounter.ContainsKey(obj) && !onCounter.ContainsKey(obj))
        {
            if (originalState.ContainsKey(obj))
                obj.SetActive(originalState[obj]);
        }
    }
}
