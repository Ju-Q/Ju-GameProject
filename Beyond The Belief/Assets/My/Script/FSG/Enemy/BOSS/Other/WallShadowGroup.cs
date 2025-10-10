using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[System.Serializable]
public class ShadowWall
{
    public MeshRenderer wall;
    [HideInInspector] public int hideCount = 0;
}

public class WallShadowGroup : MonoBehaviour
{
    public List<ShadowWall> walls = new List<ShadowWall>();

    public void HideWalls()
    {
        foreach (var w in walls)
        {
            if (w.wall == null) continue;
            w.hideCount++;
            w.wall.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
        }
    }

    public void ShowWalls()
    {
        foreach (var w in walls)
        {
            if (w.wall == null) continue;
            w.hideCount = Mathf.Max(0, w.hideCount - 1);
            if (w.hideCount == 0)
                w.wall.shadowCastingMode = ShadowCastingMode.On;
        }
    }
}
