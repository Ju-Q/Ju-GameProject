using UnityEngine;
using UnityEditor;

namespace VibrantGrassShader
{
    [ExecuteInEditMode]
    public class GrassMeshShadowsMaster : MonoBehaviour
    {
#pragma warning disable 0219
#pragma warning disable 0414

        [HideInInspector] public RenderTexture ShadowRenderTexture;
        private GrassFieldMaster grassShaderMaster;
        [SerializeField, HideInInspector] private MainControls SVGMC = null;
        private MeshRenderer meshRenderer;
        private GrassMeshMaster GSMeshMaster;
#pragma warning restore 0219
#pragma warning restore 0414

        private void Start()
        {
            if (Application.isPlaying == true) Destroy(this);
        }
#if UNITY_EDITOR


        private void OnEnable()
        {
            if (Application.isPlaying == false)
            {
                EditorApplication.update += EditorUpdates;
                meshRenderer = GetComponent<MeshRenderer>();
                grassShaderMaster = GetComponentInParent<GrassFieldMaster>();
                GSMeshMaster = GetComponentInParent<GrassMeshMaster>();
                AddMethodsToMainControlEvent();
                SVGMC = grassShaderMaster.gameObject.GetComponentInParent<MainControls>();
            }
        }
        private void OnDisable()
        {
            if (Application.isPlaying == false)
            {
                EditorApplication.update -= EditorUpdates;
                RemoveMethodsToMainControlEvent();
                if (meshRenderer.sharedMaterial != null)
                {
                    if (GSMeshMaster.MainMatInstanced != null) GSMeshMaster.AssignTexturesAndWindDirection(GSMeshMaster.MainMatInstanced);
                }
            }
        }

        void EditorUpdates()
        {
            if (Application.isPlaying == false && this != null)
            {
                if (SVGMC.PaintShadows == true) meshRenderer.sharedMaterial.SetTexture("_shadowTexture", ShadowRenderTexture);
            }
        }

        private void AddMethodsToMainControlEvent() { grassShaderMaster.SaveShadowTextureEvent += SaveRenderTextureToPNG; }
        private void RemoveMethodsToMainControlEvent() { grassShaderMaster.SaveShadowTextureEvent -= SaveRenderTextureToPNG; }

        private void SaveRenderTextureToPNG()
        {
            //Set Shadow Texture 
            string ShadowTexturePath = AssetDatabase.GetAssetPath(grassShaderMaster.ShadowTexture);
            if (ShadowTexturePath != "" && ShadowTexturePath != null)
            {
                var tex = new Texture2D(ShadowRenderTexture.width, ShadowRenderTexture.height, TextureFormat.RFloat, false);
                RenderTexture.active = ShadowRenderTexture;

                tex.ReadPixels(new Rect(0, 0, ShadowRenderTexture.width, ShadowRenderTexture.height), 0, 0);
                tex.Apply();

                //File.WriteAllBytes(ShadowTexturePath, ImageConversion.EncodeToEXR(tex));
                System.IO.File.WriteAllBytes(ShadowTexturePath, tex.EncodeToEXR(Texture2D.EXRFlags.CompressZIP));
                AssetDatabase.ImportAsset(ShadowTexturePath);
                DestroyImmediate(tex);
            }
        }
#endif
    }
}