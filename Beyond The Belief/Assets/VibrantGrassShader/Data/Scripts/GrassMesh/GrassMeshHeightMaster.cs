using UnityEngine;
using UnityEditor;

namespace VibrantGrassShader
{
    [ExecuteInEditMode]
    public class GrassMeshHeightMaster : MonoBehaviour
    {
#pragma warning disable 0219
#pragma warning disable 0414

        [HideInInspector] public RenderTexture HeightRenderTexture;
        private GrassFieldMaster grassShaderMaster;
        private MeshRenderer meshRenderer;
        private GrassMeshMaster GSMeshMaster;
        private GrassWrap grassShaderWrap;
        [SerializeField, HideInInspector] private MainControls SVGMC = null;
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
                if (grassShaderMaster.HeightTexture == null)
                {
                    grassShaderWrap = GetComponent<GrassWrap>();
                    //grassShaderWrap.WrapJustFinishedEvent += CreateDefaultTexture;
                }
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
                if (SVGMC.PaintHeight == true) meshRenderer.sharedMaterial.SetTexture("_heightTexture", HeightRenderTexture);
            }
        }
        private void AddMethodsToMainControlEvent() { grassShaderMaster.SaveHeightTextureEvent += SaveRenderTextureToPNG; }
        private void RemoveMethodsToMainControlEvent() { grassShaderMaster.SaveHeightTextureEvent -= SaveRenderTextureToPNG; }

        private void SaveRenderTextureToPNG()
        {
            //Set Height Texture 
            string HeightTexturePath = AssetDatabase.GetAssetPath(grassShaderMaster.HeightTexture);
            if (HeightTexturePath != "" && HeightTexturePath != null)
            {
                var tex = new Texture2D(HeightRenderTexture.width, HeightRenderTexture.height, TextureFormat.RFloat, false);
                RenderTexture.active = HeightRenderTexture;

                tex.ReadPixels(new Rect(0, 0, HeightRenderTexture.width, HeightRenderTexture.height), 0, 0);
                tex.Apply();

                //File.WriteAllBytes(HeightTexturePath, ImageConversion.EncodeToEXR(tex));
                System.IO.File.WriteAllBytes(HeightTexturePath, tex.EncodeToEXR(Texture2D.EXRFlags.CompressZIP));
                AssetDatabase.ImportAsset(HeightTexturePath);
                DestroyImmediate(tex);
            }
        }
#endif
    }
}