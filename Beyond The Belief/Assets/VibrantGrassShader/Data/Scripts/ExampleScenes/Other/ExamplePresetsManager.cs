using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using VibrantGrassShaderTools;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace VibrantGrassShader
{
    [ExecuteInEditMode]
    public class ExamplePresetsManager : MonoBehaviour
    {
#if UNITY_EDITOR
        enum PresetsEnum { Green, Blue, Pink, White, Cold, Fire, WindyPurple, WindyGreen, WindyBlue, WindyPink };
        [Foldout("Presets Controls", true)]
        [SerializeField] private PresetsEnum ColorPreset;
        [SerializeField, Foldout("Data (Don't Touch)", true)] private GameObject VGSObject, GroundExample, SceneSettings;
        [SerializeField] private List<Material> GrassMaterialsPresetsHDRP, GroundMaterialsPresetsHDRP;
        [SerializeField] private List<VolumeProfile> VolumeProfilesPresetsHDRP;
        [SerializeField] private List<Material> GrassMaterialsPresetsURP, GroundMaterialsPresetsURP;
        [SerializeField] private List<VolumeProfile> VolumeProfilesPresetsURP;
        [SerializeField, HideInInspector] private MainControls VGSMaincontrols;
        [SerializeField, HideInInspector] private MeshRenderer GroundExampleMeshRenderer;
        [SerializeField, HideInInspector] private Volume ExampleVolume;
        [SerializeField, HideInInspector] private List<PresetsEnum> PresetsList;
        [SerializeField] private bool UsingBuiltIn, UsingURP, UsingHDRP;
        [SerializeField] private float SunIntensityHDRP, SunIntensityBuiltInAndURP;

        private void OnEnable()
        {
            if (Application.isPlaying == false)
            {
                EditorApplication.update += EditorUpdate;
                PresetsList = new List<PresetsEnum>();
                PresetsList.Add(PresetsEnum.Green);
                PresetsList.Add(PresetsEnum.Blue);
                PresetsList.Add(PresetsEnum.Pink);
                PresetsList.Add(PresetsEnum.White);
                PresetsList.Add(PresetsEnum.Cold);
                PresetsList.Add(PresetsEnum.Fire);
                PresetsList.Add(PresetsEnum.WindyPurple);
                PresetsList.Add(PresetsEnum.WindyGreen);
                PresetsList.Add(PresetsEnum.WindyBlue);
                PresetsList.Add(PresetsEnum.WindyPink);
                VGSMaincontrols = VGSObject.GetComponent<MainControls>();
                GroundExampleMeshRenderer = GroundExample.GetComponent<MeshRenderer>();
                ExampleVolume = SceneSettings.GetComponent<Volume>();
                ColorPresetOld = ColorPreset;
            }
        }
        private void MoveSceneView()
        {
            SceneView.lastActiveSceneView.pivot = new Vector3(-5.0f, 100.0f, 0.0f);
            SceneView.lastActiveSceneView.size = 25.0f;
            GameObject QuaternionGO = new GameObject();
            QuaternionGO.transform.eulerAngles = new Vector3(5.0f, -155.0f, 0.0f);
            SceneView.lastActiveSceneView.rotation = QuaternionGO.transform.rotation;
            DestroyImmediate(QuaternionGO);
        }
        private void OnDisable()
        {
            if (Application.isPlaying == false)
            {
                EditorApplication.update -= EditorUpdate;
            }
        }

        private PresetsEnum ColorPresetOld;
        void EditorUpdate()
        {
            //Do that once per Unity Session
            if (SessionState.GetBool("VGSExampleSceneUnityLaunchFirstFrame", false) == false)
            {
                if (Application.unityVersion.Contains("2019") == true && VGSMaincontrols.CreatedMainMat.shader != VGSMaincontrols.GrassShader2019)
                {
                    VGSMaincontrols.CreatedMainMat.shader = VGSMaincontrols.GrassShader2019;
                    VGSMaincontrols.CreatedOpaqueMat.shader = VGSMaincontrols.GrassShader2019;
                    VGSMaincontrols.EnableTransparency = false;
                }
                MoveSceneView();
                SessionState.SetBool("VGSExampleSceneUnityLaunchFirstFrame", true);
            }
            if (ColorPreset != ColorPresetOld) ApplyPreset();
            ColorPresetOld = ColorPreset;
        }

        private void ApplyPreset()
        {
            if (UsingBuiltIn == true || UsingURP == true || UsingHDRP == true)
            {
                int IndexToApply = 0;
                for (int i = 0; i < PresetsList.Count; i++) { if (ColorPreset == PresetsList[i]) IndexToApply = i; }
                if (UsingBuiltIn == true)
                {
                    if (GrassMaterialsPresetsURP.Count > IndexToApply) VGSMaincontrols.createdMaterial.CopyPropertiesFromMaterial(GrassMaterialsPresetsURP[IndexToApply]);
                    if (GroundMaterialsPresetsURP.Count > IndexToApply) GroundExampleMeshRenderer.sharedMaterial.CopyPropertiesFromMaterial(GroundMaterialsPresetsURP[IndexToApply]);
                    EditorUtility.SetDirty(VGSMaincontrols.createdMaterial);
                    EditorUtility.SetDirty(GroundExampleMeshRenderer.sharedMaterial);
                }
                if (UsingURP == true)
                {
                    if (GrassMaterialsPresetsURP.Count > IndexToApply) VGSMaincontrols.createdMaterial.CopyPropertiesFromMaterial(GrassMaterialsPresetsURP[IndexToApply]);
                    if (GroundMaterialsPresetsURP.Count > IndexToApply) GroundExampleMeshRenderer.sharedMaterial.CopyPropertiesFromMaterial(GroundMaterialsPresetsURP[IndexToApply]);
                    if (VolumeProfilesPresetsURP.Count > IndexToApply) ExampleVolume.sharedProfile = VolumeProfilesPresetsURP[IndexToApply];
                    EditorUtility.SetDirty(VGSMaincontrols.createdMaterial);
                    EditorUtility.SetDirty(GroundExampleMeshRenderer.sharedMaterial);
                    EditorUtility.SetDirty(ExampleVolume.sharedProfile);
                }
                if (UsingHDRP == true)
                {
                    if (GrassMaterialsPresetsHDRP.Count > IndexToApply) VGSMaincontrols.createdMaterial.CopyPropertiesFromMaterial(GrassMaterialsPresetsHDRP[IndexToApply]);
                    if (GroundMaterialsPresetsHDRP.Count > IndexToApply) GroundExampleMeshRenderer.sharedMaterial.CopyPropertiesFromMaterial(GroundMaterialsPresetsHDRP[IndexToApply]);
                    if (VolumeProfilesPresetsHDRP.Count > IndexToApply) ExampleVolume.sharedProfile = VolumeProfilesPresetsHDRP[IndexToApply];
                    EditorUtility.SetDirty(VGSMaincontrols.createdMaterial);
                    EditorUtility.SetDirty(GroundExampleMeshRenderer.sharedMaterial);
                    EditorUtility.SetDirty(ExampleVolume.sharedProfile);
                }
            }
        }
#endif
    }
}