using UnityEngine;
using VibrantGrassShaderTools;
using UnityEditor;
using System.Collections.Generic;

namespace VibrantGrassShader
{
    [ExecuteInEditMode]
    public class GrassMeshMaster : MonoBehaviour
    {
        [Foldout("Data (Don't Change)", true)]
        [SerializeField] private GameObject GrassShaderMasterObject = null;
        [SerializeField] public Shader DrawShader = null, BrushTextureShader = null;
        [SerializeField] public Material MainMatInstanced;

        public delegate void GrassMeshMasterDeleg();
        public GrassMeshMasterDeleg TranspSwitch_Event;
        private int FrameCount;
        private GrassFieldMaster _grassFieldMaster;
        private MeshRenderer _meshRenderer;
        [SerializeField, HideInInspector] private MainControls VGSMC;
        [SerializeField, HideInInspector] private float distBetweenLowestAndHighestPoint = 0.0f;
        [SerializeField, HideInInspector] private Vector3 visualCenter = Vector3.zero;
        public Vector3 BoundsCenterLocal
        {
            get { return visualCenter; }
            set
            {
#if UNITY_EDITOR
                SerializedObject serializedObject = new SerializedObject(this);
                visualCenter = Vector3.zero;
                visualCenter = Vector3.one;
                visualCenter = value;//Reset so it detects the change
                SerializedProperty serializedProperty = serializedObject.FindProperty("visualCenter");
                serializedProperty.vector3Value = value;
                serializedObject.ApplyModifiedProperties();
#endif
            }
        }
        public float DistBetweenLowestAndHighestPoint
        {
            get { return distBetweenLowestAndHighestPoint; }
            set
            {
#if UNITY_EDITOR
                SerializedObject serializedObject = new SerializedObject(this);
                distBetweenLowestAndHighestPoint = 0.0f;
                distBetweenLowestAndHighestPoint = 1.0f;
                distBetweenLowestAndHighestPoint = value;//Reset so it detects the change
                SerializedProperty serializedProperty = serializedObject.FindProperty("distBetweenLowestAndHighestPoint");
                serializedProperty.floatValue = value;
                serializedObject.ApplyModifiedProperties();
#endif
            }
        }
        private List<Object> ObjectsToDestroyOnDisable = new List<Object>();
        private bool StartHappend, WindPushStrengthWasChanged = false, WindSquashStrengthWasChanged = false;

        private void OnEnable()
        {
            InitializeValues();
#if UNITY_EDITOR
            if (Application.isPlaying == false)
            {
                EditorApplication.update += EditorUpdates;
                VGSMC.TranspSwitch_Event += TranspSwitchMethod;
                //MainControlsFileModification.OnWillSaveAssetEvent += ApplyMatChanges;
            }
            MainControlsTrigger1 = false;
#endif
            FrameCount = 0;
            if (Application.isPlaying == true)
            {
                if (StartHappend == true)
                {
                    if (WindPushStrengthWasChanged == true || WindSquashStrengthWasChanged == true) SetWindStrengthRegroupedMethod();
                }
                //Apply Bounds
                MeshFilter meshFilter = GetComponent<MeshFilter>();
                Bounds newBounds = meshFilter.mesh.bounds;
                if (_grassFieldMaster.NoMeshRootsHeightsTexture != null)
                {
                    newBounds = new Bounds(BoundsCenterLocal, new Vector3(VGSMC.GrassFieldMaxDistance * 1.2f, DistBetweenLowestAndHighestPoint * 1.3f, VGSMC.GrassFieldMaxDistance * 1.2f));
                }
                else
                {
                    newBounds = new Bounds(newBounds.center, new Vector3(newBounds.size.x * 1.1f, newBounds.size.y * 1.3f, newBounds.size.z * 1.1f));
                }
                meshFilter.mesh.bounds = newBounds;
            }
        }

        private void OnDisable()
        {
#if UNITY_EDITOR
            if (Application.isPlaying == false)
            {
                EditorApplication.update -= EditorUpdates;
                //MainControlsFileModification.OnWillSaveAssetEvent -= ApplyMatChanges;
                _meshRenderer.sharedMaterial = null;
                DestroyImmediate(MainMatInstanced);
                MainMatInstanced = null;
                VGSMC.TranspSwitch_Event -= TranspSwitchMethod;
            }
#endif
            if (Application.isPlaying == true)
            {
                if (VGSMC.DestroyInstMatsOnFieldDisable) DestroyCreatedObjectsMethod();
            }
        }

        private void Start()
        {
            if (Application.isPlaying == true)
            {
                StartHappend = true;
                WindPushStrengthOnStart = VGSMC.WindPushStrength;
                WindSquashStrengthOnStart = VGSMC.WindSquashStrength;
                VGSMC.InstanceMaterials_Event += InitializeValues;
                VGSMC.DestroyInstancedMaterials_Event += DestroyCreatedObjectsMethod;
                VGSMC.TranspSwitch_Event += TranspSwitchMethod;
                if (VGSMC.InstanceMaterialsOnStart) InitializeValues();
            }
        }

        private void CreateMaterial(bool TransparencyEnabled)
        {
            Material MainMatToUse = VGSMC.CreatedMainMat;
            Material OpaqueMatToUse = VGSMC.CreatedOpaqueMat;
            if (VGSMC.CreatedMainMat == null) MainMatToUse = VGSMC.MainOriginalMatBuiltIn;
            if (VGSMC.CreatedOpaqueMat == null) OpaqueMatToUse = VGSMC.OpaqueOriginalMatBuiltIn;
            Material MatApplied = MainMatToUse;
            if (TransparencyEnabled == false) MatApplied = OpaqueMatToUse;
            Material MainMatToAdd = new Material(MatApplied);
            MainMatInstanced = MainMatToAdd;
            MainMatInstanced.name = MainMatToUse.name + "_Instanced";
        }

        private void DestroyCreatedObjectsMethod()
        {
            _meshRenderer.sharedMaterial = null;
            for (int i = 0; i < ObjectsToDestroyOnDisable.Count; i++)
            { if (ObjectsToDestroyOnDisable[i] != null) Destroy(ObjectsToDestroyOnDisable[i]); }
        }

        private void TranspSwitchMethod()
        {
            if (MainMatInstanced != null)
            {
                if (Application.isPlaying) Destroy(MainMatInstanced);
                else DestroyImmediate(MainMatInstanced);
                MainMatInstanced = null;
            }
            InitializeValues();
            SetWindStrengthRegroupedMethod();
            TranspSwitch_Event?.Invoke();
        }

        public void InitializeValues()
        {
            if (_meshRenderer == null) _meshRenderer = GetComponent<MeshRenderer>();
            if (_grassFieldMaster == null) _grassFieldMaster = GrassShaderMasterObject.GetComponent<GrassFieldMaster>();
            if (transform.parent != null) VGSMC = GetComponentInParent<MainControls>();
            if (VGSMC != null && transform.parent != null)
            {
                if (MainMatInstanced == null) CreateMaterial(VGSMC.EnableTransparency);
                if (_grassFieldMaster.OverwriteFlattenedGrass_HeightRemoved == true)
                { MainMatInstanced.SetFloat("_FlattenedGrassHeightRemoved", _grassFieldMaster.FlattenedGrass_HeightRemoved); }
                if (VGSMC.UsingGammaColorSpace == true)
                { MainMatInstanced.SetInt("_UsingGammaColorSpace_Data_Don_t_change", 1); }

                AssignTexturesToMat(MainMatInstanced);
                int RootsHeightsTexAssigned = 1;
                if (MainMatInstanced.GetTexture("_NoMeshRootsHeightsTexture") == null) RootsHeightsTexAssigned = 0;
                MainMatInstanced.SetInt("_RootsHeightsTexAssigned", RootsHeightsTexAssigned);

                _meshRenderer.sharedMaterial = MainMatInstanced;
                if (ObjectsToDestroyOnDisable.Contains(MainMatInstanced) == false) ObjectsToDestroyOnDisable.Add(MainMatInstanced);

                SetWindDirection(MainMatInstanced);
#if UNITY_EDITOR
                //Add Events 
                _grassFieldMaster.DeleteShadowTexEvent += DeleteShadowTextureAsset;
                _grassFieldMaster.DeleteHeightTexEvent += DeleteHeightTextureAsset;
                _grassFieldMaster.DeleteColorTexEvent += DeleteColorTextureAsset;
#endif
            }
        }

        public void AssignTexturesToMat(Material MatToAssignTo)
        {
            MatToAssignTo.SetTexture("_shadowTexture", _grassFieldMaster.ShadowTexture);
            MatToAssignTo.SetTexture("_heightTexture", _grassFieldMaster.HeightTexture);
            MatToAssignTo.SetTexture("_colorTexture", _grassFieldMaster.ColorTexture);
            MatToAssignTo.SetTexture("_groundNormalTexture", _grassFieldMaster.GroundNormalTexture);
            MatToAssignTo.SetTexture("_NoMeshRootsHeightsTexture", _grassFieldMaster.NoMeshRootsHeightsTexture);
        }

        private void EditorUpdates()
        {
            if (Application.isPlaying == false && this != null)
            {
#if UNITY_EDITOR
                MainControlChanges();
                if (VGSMC.CreatedMainMat != null)
                {
                    if (VGSMC.PaintShadows == false && VGSMC.PaintHeight == false && VGSMC.PaintColor == false && VGSMC.CreatingGrass == false)
                    {
                        float? FlattenedGrassHeightRemovedResult = null;
                        if (_grassFieldMaster.OverwriteFlattenedGrass_HeightRemoved == true) FlattenedGrassHeightRemovedResult = _grassFieldMaster.FlattenedGrass_HeightRemoved;
                        VGSMC.CopyMaterialControlValuesOntoOtherMaterial(VGSMC.CreatedMainMat, ref MainMatInstanced, FlattenedGrassHeightRemovedResult);
                    }
                    if (MainMatInstanced != null) SetWindDirection(MainMatInstanced);
                }
                UpdateMethod();
                //Select GrassField automatically if the mesh was selected in Scene View
                if (_grassFieldMaster.VGSMC.PreventMeshSelection == true)
                { if (Selection.activeGameObject == gameObject) Selection.activeGameObject = gameObject.transform.parent.gameObject; }
#endif
            }
        }

        private float WindPushStrengthOnStart, WindSquashStrengthOnStart;
        private void UpdateMethod()
        {
            if (FrameCount > 1)
            {
                string colorTexturePropName = "_colorTexture";
                if (_meshRenderer.sharedMaterial != null)
                {
                    if (_meshRenderer.sharedMaterial.HasProperty(colorTexturePropName) == true)
                    {
                        if (_meshRenderer.sharedMaterial.GetTexture(colorTexturePropName) == null) _meshRenderer.sharedMaterial.SetInt("_colorTextureAssigned", 0);
                        else _meshRenderer.sharedMaterial.SetInt("_colorTextureAssigned", 1);
                    }
                }
                if (MainMatInstanced != null) SetWindDirection(MainMatInstanced);
                SetWindStrengthRegroupedMethod();
            }
            if (_meshRenderer.sharedMaterial != null)
            {
                if (_meshRenderer.sharedMaterial.GetTexture("_heightTexture") == null) _meshRenderer.sharedMaterial.SetInt("_HeightTextureAssigned", 0);
                else _meshRenderer.sharedMaterial.SetInt("_HeightTextureAssigned", 1);
            }
            FrameCount += 1;
        }
        private void Update()
        {
            if (Application.isPlaying == true)
            {
                UpdateMethod();
            }
        }

#if UNITY_EDITOR
        private void DeleteShadowTextureAsset() { AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(_grassFieldMaster.ShadowTexture)); }
        private void DeleteHeightTextureAsset() { AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(_grassFieldMaster.HeightTexture)); }
        private void DeleteColorTextureAsset() { AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(_grassFieldMaster.ColorTexture)); }
        private void DeleteGroundNormalTextureAsset() { AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(_grassFieldMaster.GroundNormalTexture)); }

        private bool MainControlsTrigger1;
        private void MainControlChanges()
        {
            if (_grassFieldMaster.MainControlObject != null)
            {
                if (MainControlsTrigger1 == false)
                {
                    VGSMC = _grassFieldMaster.MainControlObject.GetComponent<MainControls>();
                    MainControlsTrigger1 = true;
                }
                if (_meshRenderer.sharedMaterial != null) _meshRenderer.sharedMaterial.SetFloat("_overallAlpha", VGSMC.GrassMeshOverallAlpha);
            }
        }
#endif

        public void AssignTexturesAndWindDirection(Material MaterialToChange)
        {
            MaterialToChange.SetTexture("_shadowTexture", _grassFieldMaster.ShadowTexture);
            MaterialToChange.SetTexture("_heightTexture", _grassFieldMaster.HeightTexture);
            MaterialToChange.SetTexture("_colorTexture", _grassFieldMaster.ColorTexture);
            SetWindDirection(MaterialToChange);
        }

        private void SetWindDirection(Material MaterialToChange)
        {
            if (VGSMC != null)
            {
                if (VGSMC.PropertyWindDirection.magnitude != 0.0f)
                {
                    Vector2 WindDirVec2Normalized = new Vector2(VGSMC.PropertyWindDirection.x, VGSMC.PropertyWindDirection.z).normalized;
                    Vector3 DirectionVectorLocalized = transform.InverseTransformDirection(new Vector3(WindDirVec2Normalized.x,
                        0.0f, WindDirVec2Normalized.y));
                    MaterialToChange.SetVector("_windDirection", new Vector2(DirectionVectorLocalized.x, DirectionVectorLocalized.z));
                    MaterialToChange.SetFloat("_windDirectionDegrees", -(VGSMC.WindDirectionDegrees + 135.0f));
                }
            }
        }

        private void SetWindStrengthRegroupedMethod()
        {
            if (VGSMC.WindPushStrength != WindPushStrengthOnStart) WindPushStrengthWasChanged = true;
            if (WindPushStrengthWasChanged == true)
            {
                if (MainMatInstanced != null) SetWindStrength(MainMatInstanced, true, false, VGSMC.WindPushStrength);
            }
            if (VGSMC.WindSquashStrength != WindSquashStrengthOnStart) WindSquashStrengthWasChanged = true;
            if (WindSquashStrengthWasChanged == true)
            {
                if (MainMatInstanced != null) SetWindStrength(MainMatInstanced, false, true, null, VGSMC.WindSquashStrength);
            }
        }

        private void SetWindStrength(Material MaterialToChange, bool ChangePush, bool ChangeSquash, float? PushStrength = null, float? SquashStrength = null)
        {
            if (PushStrength != null)
            {
                float PushStrengthResult = (float)PushStrength;
                if (ChangePush == true)
                {
                    MaterialToChange.SetFloat("_WindPushStrength", PushStrengthResult);
                }
            }
            if (SquashStrength != null)
            {
                float SquashStrengthResult = (float)SquashStrength;
                if (ChangeSquash == true)
                {
                    MaterialToChange.SetFloat("_WindSquashStrength", SquashStrengthResult);
                }
            }
        }
    }
}