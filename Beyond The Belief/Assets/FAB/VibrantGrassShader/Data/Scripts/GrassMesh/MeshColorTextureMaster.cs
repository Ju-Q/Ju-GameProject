using UnityEngine;
using UnityEditor;

namespace VibrantGrassShader
{
    [ExecuteInEditMode]
    public class MeshColorTextureMaster : MonoBehaviour
    {
#pragma warning disable 0219
#pragma warning disable 0414

        [HideInInspector] public RenderTexture ColorRenderTexture;
        private GrassFieldMaster grassShaderMaster;
        private MeshRenderer meshRenderer;
        private GrassMeshMaster GSMeshMaster;
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
                if (SVGMC.PaintColor == true) meshRenderer.sharedMaterial.SetTexture("_colorTexture", ColorRenderTexture);
            }
        }

        private void AddMethodsToMainControlEvent() { grassShaderMaster.SaveColorTextureEvent += SaveRenderTextureToPNG; }
        private void RemoveMethodsToMainControlEvent() { grassShaderMaster.SaveColorTextureEvent -= SaveRenderTextureToPNG; }

        private void SaveRenderTextureToPNG()
        {
            //Set Color Texture 
            string ColorTexturePath = AssetDatabase.GetAssetPath(grassShaderMaster.ColorTexture);
            if (ColorTexturePath != "" && ColorTexturePath != null)
            {
                var tex = new Texture2D(ColorRenderTexture.width, ColorRenderTexture.height, TextureFormat.RGBAFloat, false);
                RenderTexture.active = ColorRenderTexture;

                tex.ReadPixels(new Rect(0, 0, ColorRenderTexture.width, ColorRenderTexture.height), 0, 0);
                tex.Apply();

                //File.WriteAllBytes(ColorTexturePath, ImageConversion.EncodeToEXR(tex));
                System.IO.File.WriteAllBytes(ColorTexturePath, tex.EncodeToEXR(Texture2D.EXRFlags.CompressZIP));
                AssetDatabase.ImportAsset(ColorTexturePath);
                DestroyImmediate(tex);
            }
        }
#endif
    }
}