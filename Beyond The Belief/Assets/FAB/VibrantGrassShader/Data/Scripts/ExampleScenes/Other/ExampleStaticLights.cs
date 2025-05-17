using UnityEngine;

namespace VibrantGrassShader
{
    [ExecuteInEditMode]
    public class ExampleStaticLights : MonoBehaviour
    {
        [SerializeField, ColorUsage(true, true)] private Color Color1, Color2;
        [SerializeField, Range(0.0f, 1.0f)] private float FresnelPower = 0.3f;
        [SerializeField] private Material OriginalMaterial;
        [SerializeField, HideInInspector] private MeshRenderer meshRenderer;
        private Material InstancedMaterial;

        void OnEnable()
        {
            if (Application.isPlaying == false)
            {
                meshRenderer = GetComponent<MeshRenderer>();
                InstancedMaterial = new Material(OriginalMaterial);
                InstancedMaterial.name = OriginalMaterial.name + "_Instanced";
                InstancedMaterial.SetColor("_Color1", Color1);
                InstancedMaterial.SetColor("_Color2", Color2);
                InstancedMaterial.SetFloat("_FresnelPower", FresnelPower);
                meshRenderer.sharedMaterial = InstancedMaterial;
            }
        }
    }

}