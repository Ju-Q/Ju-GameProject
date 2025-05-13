using System.Collections.Generic;
using UnityEngine;
using VibrantGrassShaderTools;
using UnityEditor;

namespace VibrantGrassShader
{
    [ExecuteInEditMode]
    public class MainControls : MonoBehaviour
    {
        [SerializeField, Foldout("Setup", true)] public GameObject _camera = null;
        [SerializeField] public LayerMask groundLayers = 1;
#if UNITY_EDITOR
        [SerializeField] public string createdFilesPath = null;
        [SerializeField, HideInInspector] public string GrassAssetFolderPath = null;
        [SerializeField] public bool initialize = false;
        [SerializeField, Foldout("UI", true)]
        public bool enableUI = false, EnableUIWhenSelected = true;
#endif
        [Foldout("Visuals", true)]
        [SerializeField] public Material createdMaterial = null;
        public enum VGSDensityEnum { _1, _2, _3, _4, _5, _6, _7, _8, _9, _10, _11, _12 };
        [SerializeField] private VGSDensityEnum GrassDensity;
        public enum VGSRootsHeightsTextureSizeEnum { _32, _64, _128, _256, _512, _1024, _2048 };
        [SerializeField] public VGSRootsHeightsTextureSizeEnum RootsHeightsPrecision = VGSRootsHeightsTextureSizeEnum._256;
#if UNITY_EDITOR
        private VGSRootsHeightsTextureSizeEnum RootsHeightsTextureQualityOld = VGSRootsHeightsTextureSizeEnum._256;
        [SerializeField] private Object MaterialPresets;
#endif
        [SerializeField] public float InteractionErasingSpeed = 0.05f;
        [Foldout("Performance", true)]
        [SerializeField] public bool EnableTransparency = true, EnableDistanceFadeTransparency = false, EnableDistanceFadeHeight = true;
        [SerializeField] public Vector2 DistanceFadeOutMinMax = new Vector2(6.5f, 7.5f);
        [SerializeField] public int LightAndInteractMaxFPS = 60, MaxLightPerGrassField = 50, MaxInteractPerGrassField = 50;
        //[SerializeField] public int InteractionMaxFPS = 60, InteractErasingMinFPS = 30;
        [SerializeField, HideInInspector] public int LightTextureSize = 128, InteractTextureSize = 512;
        [SerializeField] public float InteractionMaximumErasingTime = 10.0f;
        [SerializeField, HideInInspector] public int BaseTexturesSize = 128;
        [Foldout("Audio", true)]
        [SerializeField] public bool EnableWindAudio = false;
#if UNITY_EDITOR
        [SerializeField] public bool ShowAudioSources = false;
#endif
        [SerializeField] public AudioClip WindAudioClip = null;
        [SerializeField, Range(0.0f, 1.0f)] public float WindMaxVolume = 0.1f;
        [SerializeField] public float WindVolumeSmoothTime = 0.1f;
        [SerializeField] public AnimationCurve WindVolumeDistanceCurve = null;
#if UNITY_EDITOR
        [Foldout("Other", true)]
        [SerializeField] public bool DeleteAll = false, CreateMeshesToHideFlattenedGrass = false, ResetCutGrassFieldsWithMeshes = false;
        [SerializeField, HideInInspector] public Vector2 HeightPaintingFloorAndCeiling = new Vector2(0.0f, 0.85f);
        [SerializeField] public bool InvertHeightBrush = false;
        [SerializeField] public float BrushMaxSize = 100.0f;
        [SerializeField] private Object CreatedFilesDirectory;
#endif
        [SerializeField] private bool FindLightAndInteractObjectsOnStart = false;
#if UNITY_EDITOR
        [Foldout("Prefab", true)]
        [SerializeField] private bool CreatePrefabFromThisGameObject = false;
        [SerializeField] private Object CreatedPrefab = null;
#endif
        [Foldout("Secondary", true)]
        [SerializeField] public int MaxWrapVerticesPerFrame = 20000;
        [SerializeField] public float WrapMaxRayDistance = 500.0f, WrapBasePosAddedHeight = 250.0f;
        [SerializeField] public bool DebugWrapRay = false, WrapMeshes = false;
        [SerializeField, HideInInspector] public bool wrapAutomatically = true;
        [SerializeField, HideInInspector] public List<Object> CreatedAssets = new List<Object>();
        [SerializeField] public bool InstanceMaterialsOnStart = true, DestroyInstMatsOnFieldDisable = false;
#if UNITY_EDITOR
        [SerializeField] private bool FixUINotAppearingInSceneView = false;
        [SerializeField] public bool PreventMeshSelection = true;
        [SerializeField, HideInInspector] public KeyCode UndoKey = KeyCode.Z, RedoKey = KeyCode.Y;
#endif
        [Foldout("Runtime Tools", true)]
        [SerializeField] public bool DestroyRuntimeInstancedMats, InstanceRuntimeMats;
#if UNITY_EDITOR
        [SerializeField, Foldout("Painting Keyboard Shortcuts", true)]
        public KeyCode InvertBrushKey = KeyCode.LeftControl,
            ChangeBrushTypeKey = KeyCode.Space, ChangeSizeKey = KeyCode.LeftShift, ChangeStrengthKey = KeyCode.LeftControl;
#endif
        [Foldout("Data Changeable In Game", true)]
        [SerializeField] public Vector3 WindDirection = Vector2.zero;
        [SerializeField] public float WindPushStrength = 0.0f, WindSquashStrength = 0.0f;
        [SerializeField, HideInInspector] public float WindAudioVolumeApplied = 0.0f;
        [SerializeField, HideInInspector] public Vector3 WindAudioDirectionApplied = Vector3.zero;
        [SerializeField] public List<GameObject> LightAndInteractObjectsList = new List<GameObject>();
        //Important Events
        public delegate void MainControlsDelegate();
        //Destroys objects like materials and textures. Good to use before deleting the VibrantGrassShader permanently at runtime. 
        public MainControlsDelegate InstanceMaterials_Event, DestroyInstancedMaterials_Event;
        [Foldout("Data (Don't Touch)", true)]
        [SerializeField] public List<Object> ObjectsToDeleteOnStart = new List<Object>();
        [SerializeField] public bool IsUsedForInGameInstances = false;
        [SerializeField] public Mesh currentAppliedMeshOriginal = null, currentAppliedMeshCompressed = null;
        [SerializeField] public Object createdMeshFolder;
#if UNITY_EDITOR
        [SerializeField] public Mesh[] OriginalMeshes = new Mesh[0], OriginalMeshesCompressed = new Mesh[0];
        [SerializeField] public Object DefaultCreatedGrassFilesFolder = null, AssetDataFolder = null;
        [SerializeField] private bool showInitializeDebugUI = true;
        [SerializeField]
        public Texture2DAndColorSerializableDictionary ChosenColorsForPainting = new Texture2DAndColorSerializableDictionary(),
            ChosenColorsSaved = new Texture2DAndColorSerializableDictionary();
        [SerializeField] public List<Texture2D> ColorMasks = new List<Texture2D>(), UnSavedColorMasks = new List<Texture2D>();
        [SerializeField] public Dictionary<Texture2D, RenderTexture> ColorMasksRenderTextures = new Dictionary<Texture2D, RenderTexture>();
        [SerializeField]
        public GOAndVect2IntSerializableDict GridPositionsSaved = new GOAndVect2IntSerializableDict(),
            GridPositionsChanged = new GOAndVect2IntSerializableDict();
        [SerializeField] public Object GrassFieldBasePrefab = null;
        [SerializeField] public GrassTextureDrawing SVGMCTextureDrawing;
        [SerializeField] private bool creatingGrass;
        [SerializeField] private bool prefabInstanced;
        //[SerializeField, HideInInspector] public GameObject assetDestroyer = null;
        [SerializeField] private Texture2D currentColorMask;
        [SerializeField, HideInInspector] public bool CreatingGrassField = false, CreatingGrassFieldWithoutWrapping = false;
        [HideInInspector] public float GrassMeshOverallAlpha;
        [HideInInspector, SerializeField] public bool PaintShadows, PaintHeight, PaintColor;
#endif
        [SerializeField] public float GrassFieldMaxDistance = 14.80f, GrassFieldUVWorldDistance = 15.06f;
        [SerializeField, HideInInspector] public List<GameObject> GrassFieldList = new List<GameObject>();
        public delegate void SVGMainControlsDelegate();
        public SVGMainControlsDelegate DisablingVGSMCPlayMode, ReEnablingVGSMCPlayMode, TranspSwitch_Event, DisableControlArrows_Event;
#if UNITY_EDITOR
        [SerializeField] private string[] InitializeTutorialSteps = new string[0];
        [HideInInspector, SerializeField] public Dictionary<GameObject, GrassFieldMaster> GrassFieldMasters = new Dictionary<GameObject, GrassFieldMaster>();
        //[HideInInspector, SerializeField] public GameObject ParentOnSpawn = null;
        [HideInInspector, SerializeField] public Object WindDirectionObjectPrefab;
        [HideInInspector, SerializeField] public GameObject WindDirectionObject;
        [HideInInspector, SerializeField, Range(0.01f, 100.0f)] public float BrushSize = 25.0f;
        [HideInInspector, SerializeField, Range(0.1f, 1.0f)] public float BrushStrength = 0.1f;
        public SVGMainControlsDelegate UndoEvent, RedoEvent, WrapEvent, DisablingMainControlObjectEditMode, EraseTextureEvent,
            FillTextureEvent, SaveCurrentTextureEvent, CutUnusedMeshEvent, WrapCutMeshesEvent, AddColorEvent,
            DeleteCurrentTextureAssetEvent;
        public delegate void SVGMCRemoveColorDelegate(int ColorIndex);
        public SVGMCRemoveColorDelegate RemoveColorEvent;
        [HideInInspector] public bool ChangedSmthOtherThanTexture;
        [SerializeField, HideInInspector] private Vector3 windDirectionObjectPosition;
        [SerializeField, HideInInspector] private string createdFilesDirectoryPath = null;
        [HideInInspector] public int GrassFieldsToSpawnPerClick = 1;
        [HideInInspector] public Dictionary<GameObject, int> SpawnsLeftDict = new Dictionary<GameObject, int>();
#endif
        public delegate void SVGMainControlsDelegateInGame();
        public SVGMainControlsDelegateInGame EventForLiIntHasChanged;
        [SerializeField] public Material MainOriginalMatHDRP = null, OpaqueOriginalMatHDRP = null;
        [SerializeField] public Material MainOriginalMatURP = null, OpaqueOriginalMatURP = null;
        [SerializeField] public Material MainOriginalMatBuiltIn = null, OpaqueOriginalMatBuiltIn = null;
        [SerializeField] public Material createdOpaqueMat = null;
        [SerializeField] public Shader VibrantGrassShaderLinear = null, VibrantGrassShaderGamma = null;
        [HideInInspector, SerializeField] public float windDirectionDegrees = 0.0f;
        [HideInInspector] public Dictionary<GameObject, DynamicLightAndInteract> IntLigDict = new Dictionary<GameObject, DynamicLightAndInteract>();
        private bool ShowAudioSourcesOld;
        [HideInInspector] public bool ShowAudioSourcesUIEnabled, UsingGammaColorSpace;
#if UNITY_EDITOR
        public bool WrapAutomatically
        {
            get { return wrapAutomatically; }
            set
            {
                SerializedObject serializedObject = new SerializedObject(this);
                wrapAutomatically = false;
                wrapAutomatically = true;
                wrapAutomatically = value;//Reset so it detects the change
                SerializedProperty serializedProperty = serializedObject.FindProperty("wrapAutomatically");
                serializedProperty.boolValue = value;
                serializedObject.ApplyModifiedProperties();
            }
        }
#endif

        public LayerMask GroundLayers
        {
            get { return groundLayers; }
            set
            {
#if UNITY_EDITOR
                MainControls SVGMainControls = GetComponent<MainControls>();
                SerializedObject serializedObject = new SerializedObject(SVGMainControls);
                groundLayers = 0;
                groundLayers = 1;
                groundLayers = value;//Reset so it detects the change
                SerializedProperty serializedProperty = serializedObject.FindProperty("groundLayers");
                serializedProperty.intValue = value;
                serializedObject.ApplyModifiedProperties();
#endif
            }
        }

        public Mesh CurrentAppliedMeshOriginal
        {
            get { return currentAppliedMeshOriginal; }
            set
            {
#if UNITY_EDITOR
                MainControls SVGMainControls = GetComponent<MainControls>();
                SerializedObject serializedObject = new SerializedObject(SVGMainControls);
                currentAppliedMeshOriginal = null;
                currentAppliedMeshOriginal = value;//Reset so it detects the change
                SerializedProperty serializedProperty = serializedObject.FindProperty("currentAppliedMeshOriginal");
                serializedProperty.objectReferenceValue = value;
                serializedObject.ApplyModifiedProperties();
#endif
            }
        }

        public Mesh CurrentAppliedMeshCompressed
        {
            get { return currentAppliedMeshCompressed; }
            set
            {
#if UNITY_EDITOR
                MainControls SVGMainControls = GetComponent<MainControls>();
                SerializedObject serializedObject = new SerializedObject(SVGMainControls);
                currentAppliedMeshCompressed = null;
                currentAppliedMeshCompressed = value;//Reset so it detects the change
                SerializedProperty serializedProperty = serializedObject.FindProperty("currentAppliedMeshCompressed");
                serializedProperty.objectReferenceValue = value;
                serializedObject.ApplyModifiedProperties();
#endif
            }
        }

        public Object CreatedMeshFolder
        {
            get { return createdMeshFolder; }
            set
            {
#if UNITY_EDITOR
                MainControls SVGMainControls = GetComponent<MainControls>();
                SerializedObject serializedObject = new SerializedObject(SVGMainControls);
                createdMeshFolder = null;
                createdMeshFolder = value;//Reset so it detects the change
                SerializedProperty serializedProperty = serializedObject.FindProperty("createdMeshFolder");
                serializedProperty.objectReferenceValue = value;
                serializedObject.ApplyModifiedProperties();
#endif
            }
        }

        public float WindDirectionDegrees
        {
            get { return windDirectionDegrees; }
            set
            {
                windDirectionDegrees = value;
#if UNITY_EDITOR
                MainControls SVGMainControls = GetComponent<MainControls>();
                SerializedObject serializedObject = new SerializedObject(SVGMainControls);
                windDirectionDegrees = 0.0f;
                windDirectionDegrees = 1.0f;
                windDirectionDegrees = value;//Reset so it detects the change
                SerializedProperty serializedProperty = serializedObject.FindProperty("windDirectionDegrees");
                serializedProperty.floatValue = value;
                serializedObject.ApplyModifiedProperties();
#endif
            }
        }

        public Vector3 PropertyWindDirection
        {
            get { return WindDirection; }
            set
            {
#if UNITY_EDITOR
                MainControls SVGMainControls = GetComponent<MainControls>();
                SerializedObject serializedObject = new SerializedObject(SVGMainControls);
                WindDirection = Vector3.zero;
                WindDirection = Vector3.one;
                WindDirection = value;//Reset so it detects the change
                SerializedProperty serializedProperty = serializedObject.FindProperty("WindDirection");
                serializedProperty.vector3Value = value;
                serializedObject.ApplyModifiedProperties();
#endif
            }
        }

        public Material CreatedMainMat
        {
            get { return createdMaterial; }
            set
            {
#if UNITY_EDITOR
                SerializedObject serializedObject = new SerializedObject(this);
                createdMaterial = null;
                createdMaterial = value;//Reset so it detects the change
                SerializedProperty serializedProperty = serializedObject.FindProperty("createdMaterial");
                serializedProperty.objectReferenceValue = value;
                serializedObject.ApplyModifiedProperties();
#endif
            }
        }
        public Material CreatedOpaqueMat
        {
            get { return createdOpaqueMat; }
            set
            {
#if UNITY_EDITOR
                MainControls SVGMainControls = GetComponent<MainControls>();
                SerializedObject serializedObject = new SerializedObject(SVGMainControls);
                createdOpaqueMat = null;
                createdOpaqueMat = new Material(OpaqueOriginalMatHDRP);
                DestroyImmediate(createdOpaqueMat);
                createdOpaqueMat = value;//Reset so it detects the change
                SerializedProperty serializedProperty = serializedObject.FindProperty("createdOpaqueMat");
                serializedProperty.objectReferenceValue = value;
                serializedObject.ApplyModifiedProperties();
#endif
            }
        }
#if UNITY_EDITOR

        public string CreatedFilesPath
        {
            get { return createdFilesPath; }
            set
            {
                SerializedObject serializedObject = new SerializedObject(this);
                createdFilesPath = null;
                createdFilesPath = ":D";
                createdFilesPath = value;//Reset so it detects the change
                SerializedProperty serializedProperty = serializedObject.FindProperty("createdFilesPath");
                serializedProperty.stringValue = value;
                serializedObject.ApplyModifiedProperties();
            }
        }
        public string CreatedFilesDirectoryPath
        {
            get { return createdFilesDirectoryPath; }
            set
            {
                MainControls SVGMainControls = GetComponent<MainControls>();
                SerializedObject serializedObject = new SerializedObject(SVGMainControls);
                createdFilesDirectoryPath = null;
                createdFilesDirectoryPath = ":D";
                createdFilesDirectoryPath = value;//Reset so it detects the change
                SerializedProperty serializedProperty = serializedObject.FindProperty("createdFilesDirectoryPath");
                serializedProperty.stringValue = value;
                serializedObject.ApplyModifiedProperties();
            }
        }
        public Texture2D CurrentTex2DColorMask
        {
            get { return currentColorMask; }
            set
            {
                MainControls SVGMainControls = GetComponent<MainControls>();
                SerializedObject serializedObject = new SerializedObject(SVGMainControls);
                currentColorMask = null;
                currentColorMask = new Texture2D(0, 0);
                currentColorMask = value;//Reset so it detects the change
                SerializedProperty serializedProperty = serializedObject.FindProperty("currentColorMask");
                serializedProperty.objectReferenceValue = value;
                serializedObject.ApplyModifiedProperties();
            }
        }
        public Vector3 WindDirectionObjectLastPosition
        {
            get { return windDirectionObjectPosition; }
            set
            {
                MainControls SVGMainControls = GetComponent<MainControls>();
                SerializedObject serializedObject = new SerializedObject(SVGMainControls);
                windDirectionObjectPosition = Vector3.one;
                windDirectionObjectPosition = Vector3.zero;
                windDirectionObjectPosition = value;//Reset so it detects the change
                SerializedProperty serializedProperty = serializedObject.FindProperty("windDirectionObjectPosition");
                serializedProperty.vector3Value = value;
                serializedObject.ApplyModifiedProperties();
            }
        }
        public bool EnableUI
        {
            get { return enableUI; }
            set
            {
                MainControls SVGMainControls = GetComponent<MainControls>();
                SerializedObject serializedObject = new SerializedObject(SVGMainControls);
                enableUI = false;
                enableUI = true;
                enableUI = value;//Reset so it detects the change
                SerializedProperty serializedProperty = serializedObject.FindProperty("enableUI");
                serializedProperty.boolValue = value;
                serializedObject.ApplyModifiedProperties();
            }
        }
        public bool Initialize
        {
            get { return initialize; }
            set
            {
                MainControls SVGMainControls = GetComponent<MainControls>();
                SerializedObject serializedObject = new SerializedObject(SVGMainControls);
                initialize = false;
                initialize = true;
                initialize = value;//Reset so it detects the change
                SerializedProperty serializedProperty = serializedObject.FindProperty("initialize");
                serializedProperty.boolValue = value;
                serializedObject.ApplyModifiedProperties();
            }
        }
        public bool ShowInitializeDebugUI
        {
            get { return showInitializeDebugUI; }
            set
            {
                MainControls SVGMainControls = GetComponent<MainControls>();
                SerializedObject serializedObject = new SerializedObject(SVGMainControls);
                showInitializeDebugUI = false;
                showInitializeDebugUI = true;
                showInitializeDebugUI = value;//Reset so it detects the change
                SerializedProperty serializedProperty = serializedObject.FindProperty("showInitializeDebugUI");
                serializedProperty.boolValue = value;
                serializedObject.ApplyModifiedProperties();
            }
        }
        public bool InitializationFinished
        {
            get { return initializeTrigger1; }
            set
            {
                MainControls SVGMainControls = GetComponent<MainControls>();
                SerializedObject serializedObject = new SerializedObject(SVGMainControls);
                initializeTrigger1 = false;
                initializeTrigger1 = true;
                initializeTrigger1 = value;//Reset so it detects the change
                SerializedProperty serializedProperty = serializedObject.FindProperty("initializeTrigger1");
                serializedProperty.boolValue = value;
                serializedObject.ApplyModifiedProperties();
            }
        }
        public bool CreatingGrass
        {
            get { return creatingGrass; }
            set
            {
                MainControls SVGMainControls = GetComponent<MainControls>();
                SerializedObject serializedObject = new SerializedObject(SVGMainControls);
                creatingGrass = false;
                creatingGrass = true;
                creatingGrass = value;//Reset so it detects the change
                SerializedProperty serializedProperty = serializedObject.FindProperty("creatingGrass");
                serializedProperty.boolValue = value;
                serializedObject.ApplyModifiedProperties();
            }
        }
        public bool PrefabInstanced
        {
            get { return prefabInstanced; }
            set
            {
                MainControls SVGMainControls = GetComponent<MainControls>();
                SerializedObject serializedObject = new SerializedObject(SVGMainControls);
                prefabInstanced = false;
                prefabInstanced = true;
                prefabInstanced = value;//Reset so it detects the change
                SerializedProperty serializedProperty = serializedObject.FindProperty("prefabInstanced");
                serializedProperty.boolValue = value;
                serializedObject.ApplyModifiedProperties();
            }
        }


        [SerializeField, HideInInspector] private bool ShowArrowsToActivate;
        [SerializeField, HideInInspector] private GrassColorPainter SVGColorPainter;
        [SerializeField, HideInInspector] private AssetsOrganizationTools _assetsOrganizationTools;
        [SerializeField] private Shader RootHeightsPainterShader = null, TextureColorSoftenerShader = null, VertexNormalPainterShader = null;
        [HideInInspector] public Material RootHeightsPainterMat = null, TextureColorSoftenerMat = null, VertexNormalPainterMat = null;
        private OutOfSightDisabler outOfSightDisabler;

#endif
        private void OnEnable()
        {
#if UNITY_EDITOR
            RootsHeightsTextureQualityOld = RootsHeightsPrecision;
            CreatePrefabFromThisGameObject = false;
            ResetAndWrapMeshesOld = false;
            CreatePrefabTrigger1 = false;
            CreatePrefabFrameCount = 0;
            if (InitializationFinished == false) InitializationFinishedFrameCount = 0;
            else InitializationFinishedFrameCount = 100;
            ActiveGOTrigger1 = false;
            EnableTransparencyOld = EnableTransparency;
#endif
            if (Application.isPlaying == false)
            {
#if UNITY_EDITOR
                MainControlsFileModification.OnWillSaveAssetEvent += WhenSavingAsset;
                //Check if using Gamma rendering 
                bool UsingGamma = false;
                if (PlayerSettings.colorSpace == ColorSpace.Gamma) UsingGamma = true;
                UsingGammaColorSpace = true;
                UsingGammaColorSpace = false;
                SerializedObject serializedObject = new SerializedObject(this);
                UsingGammaColorSpace = UsingGamma;
                SerializedProperty serializedProperty = serializedObject.FindProperty("UsingGammaColorSpace");
                serializedProperty.boolValue = UsingGamma;
                serializedObject.ApplyModifiedProperties();

                outOfSightDisabler = GetComponent<OutOfSightDisabler>();
                GrassTextureDrawing grassTextureDrawing = GetComponent<GrassTextureDrawing>();
                GrassShadowsPainter grassShadowsPainter = GetComponent<GrassShadowsPainter>();
                GrassHeightPainter grassHeightPainter = GetComponent<GrassHeightPainter>();
                GrassColorPainter grassColorPainter = GetComponent<GrassColorPainter>();
                if (ObjectsToDeleteOnStart.Contains(grassTextureDrawing) == false) ObjectsToDeleteOnStart.Add(grassTextureDrawing);
                if (ObjectsToDeleteOnStart.Contains(grassShadowsPainter) == false) ObjectsToDeleteOnStart.Add(grassShadowsPainter);
                if (ObjectsToDeleteOnStart.Contains(grassHeightPainter) == false) ObjectsToDeleteOnStart.Add(grassHeightPainter);
                if (ObjectsToDeleteOnStart.Contains(grassColorPainter) == false) ObjectsToDeleteOnStart.Add(grassColorPainter);
                EditorUtility.SetDirty(this);
                if (RootHeightsPainterMat == null) RootHeightsPainterMat = new Material(RootHeightsPainterShader);
                if (TextureColorSoftenerMat == null) TextureColorSoftenerMat = new Material(TextureColorSoftenerShader);
                if (VertexNormalPainterMat == null) VertexNormalPainterMat = new Material(VertexNormalPainterShader);
                CreatingGrassOld = false;
                SpawnsLeftDict = new Dictionary<GameObject, int>();
                VersionFixFrameCount = 0;
                if (PrefabWasCreated == true)
                {
                    PrefabInstanceStatus prefabInstanceStatus = PrefabUtility.GetPrefabInstanceStatus(gameObject);
                    if (prefabInstanceStatus == PrefabInstanceStatus.Connected)
                    {
                        SerializedObject serializedObject2 = new SerializedObject(this);
                        if (CreatedPrefab == null) CreatedPrefab = AssetDatabase.LoadAssetAtPath(PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(gameObject), typeof(Object)) as Object;
                        SerializedProperty serializedProperty2 = serializedObject2.FindProperty("CreatedPrefab");
                        serializedProperty2.objectReferenceValue = CreatedPrefab;
                        serializedObject2.ApplyModifiedProperties();
                        PrefabUtility.UnpackPrefabInstance(gameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                    }
                }
                WrappingTrigger1 = false;
                WrappingEverything = false;
                WrapButtonTrigger1 = false;
                WrapButtonTrigger2 = false;
                WrapAutomaticButtonTrigger1 = false;
                ShowAudioSourcesOld = false;
                ShowAudioSourcesUIEnabled = false;
                OldGrassDensity = GrassDensity;
                DeleteGOAndAssetsTrigger = false;
                DeleteGameObjectOnlyTrigger = false;
                GOTryingToBeDeleted = new List<GameObject>();
                LeftControlPressedTrigger = false;
                LeftControlPressed = false;
                DisableUIButtonClicked = false;
                BrushSizeOld = BrushSize;
                BrushStrengthOld = BrushStrength;
                LeftClickedInsideDrawUIAndHolding = false;
                MouseInsideDrawUI = false;
                ChangingCreatedFilesPath = false;
                CreatedFilesPathChangeTrigger2 = false;
                CreatedPathChangeCancel = false;
                CreatedPathChangeConfirm = false;
                CreatedFilesPathOld = CreatedFilesPath;
                DeleteInterceptionTrigger1 = false;
                ShowingDeleteAssetsUI = false;
                TryingToDeleteGO = false;
                EditorApplication.update += EditorUpdate;
                LeftClickingTrigger1 = false;
                MouseLeftClickUpFirstFrame = false;
                MouseOnColorCross = false;
                ShowAudioSources = false;
                EnableUI = false;
                MouseLastClickWasInsideUI = false;
                MouseClickedInsideUI = false;
                HeightTextureRoundBrushHoverTrigger1 = false;
                HeightTextureRoundBrushHover = false;
                HeightTextureSpreadBrushHoverTrigger1 = false;
                HeightTextureSpreadBrushHover = false;
                CreatingGrassField = false;
                CreatingGrassFieldWithoutWrapping = false;
                WindDirectionSpawnedByShowArrows = false;
                WindDirectionSpawnedByPainting = false;
                WindDirectionSpawnedByShowControls = false;
                ShowUITrigger = false;
                WrapMeshes = false;
                ResetCutGrassFieldsWithMeshes = false;
                _assetsOrganizationTools = new AssetsOrganizationTools();
                if (CreatedAssets.Count == 0)
                {
                    CreatedAssets = new List<Object>();
                    EditorUtility.SetDirty(this);
                }
                BaseTexturesSize = 128;
                OldSiblingsCount = 0;
                GrassFieldList.Clear();
                GrassFieldMasters.Clear();
                AddAndRemoveGrassFields();
                ShowArrowsToActivate = false;
                if (CreatingGrass == true) ShowArrowsToActivate = true;
                CreatingGrass = false;
                EditorFrameCount = 0;
                ExitingDrawTrigger1 = false;
                PaintHeight = false;
                PaintShadows = false;
                PaintColor = false;
                ExitButtonTrigger1 = false;
                EditorApplication.wantsToQuit += wantToQuit;
                ShowConfirmColorRemovalUI = false;
                ConfirmColorRemovalUI = false;
                if (PrefabInstanced == false)
                {
                    AssignDefaultPaths();
                    PrefabInstanced = true;
                }
                SVGColorPainter = GetComponent<GrassColorPainter>();
                RemoveColor = false;
                ColorBackgroundRectAddedHeightMultiplier = 0;
                ShowingBrushTypeUI = false;
                MouseInsideSceneView = false;
                BrushTypeChangeKeyDown = false;
                ChangeBrushTypeButtonWasPressed = false;
                if (SVGMCTextureDrawing == null) SVGMCTextureDrawing = GetComponent<GrassTextureDrawing>();
                GroundLayersOld = GroundLayers;
                Wrapping = false;
#endif
            }
            else
            {
                if (InGameFrameCount > 1) ReEnablingVGSMCPlayMode?.Invoke();
            }
        }

        private void OnDisable()
        {
            if (Application.isPlaying == false)
            {
#if UNITY_EDITOR
                MainControlsFileModification.OnWillSaveAssetEvent -= WhenSavingAsset;
                if (RootHeightsPainterMat != null) DestroyImmediate(RootHeightsPainterMat);
                if (TextureColorSoftenerMat != null) DestroyImmediate(TextureColorSoftenerMat);
                if (VertexNormalPainterMat != null) DestroyImmediate(VertexNormalPainterMat);

                DisablingMainControlObjectEditMode?.Invoke();
                if (EnableUI == true || ShowInitializeDebugUI == true) DisableUIMethod();
                EditorApplication.update -= EditorUpdate;
                EditorApplication.hierarchyWindowItemOnGUI -= OnHierarchyGUI;
                SceneView.duringSceneGui -= OnSceneGUIForDeletion;
                SceneView.duringSceneGui -= OnSceneGUIForCreatedAssetsPath;
#endif
            }
            else
            {
                if (InGameFrameCount > 1) DisablingVGSMCPlayMode?.Invoke();
            }
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
            //for (int i = 0; i < CreatedAssets.Count; i++) 
            //{
            //    if (CreatedAssets[i] is GameObject || CreatedAssets[i] == null) continue;
            //    Resources.UnloadAsset(CreatedAssets[i]);
            //}
        }

#if UNITY_EDITOR
        [HideInInspector] public bool Wrapping;
        private bool WrappingTrigger1, WrappingTrigger2, ResetAndWrapMeshesOld, WrappingEverything;
        private int CurrentGrassFieldToWrap, FramesDelayWhenWrappingOneAtATime;
        private void WrapOneAtATimeMethod()
        {
            if (CreatingGrass == false && WrapMeshes == true && ResetAndWrapMeshesOld == false) WrappingEverything = true;
            ResetAndWrapMeshesOld = WrapMeshes;
            if (WrapMeshes == true)
            {
                bool CurrentGrassFieldExists = false;
                if (GrassFieldList.Count > CurrentGrassFieldToWrap && GrassFieldMasters.ContainsKey(GrassFieldList[CurrentGrassFieldToWrap])) CurrentGrassFieldExists = true;
                if (WrappingTrigger2 == true)
                {
                    DisableControlArrows_Event?.Invoke();
                    for (int i = 0; i < GrassFieldList.Count; i++)
                    { GrassFieldList[i].SetActive(false); }
                    WrappingTrigger2 = false;
                }
                if (CurrentGrassFieldExists == true)
                {
                    bool Skip = false;
                    if (WrappingTrigger1 == false)
                    {
                        if (WrappingEverything == false)
                        {
                            if (GrassFieldMasters[GrassFieldList[CurrentGrassFieldToWrap]].GrassMeshWrapScript.CreatedMesh != null
                                || GrassFieldMasters[GrassFieldList[CurrentGrassFieldToWrap]].NoMeshRootsHeightsTexture != null)
                            {
                                Skip = true;
                                CurrentGrassFieldToWrap += 1;
                            }
                        }
                        if (Skip == false)
                        {
                            //Wait a few frames to give time to delete collections 
                            if (FramesDelayWhenWrappingOneAtATime > 1)
                            {
                                GrassFieldList[CurrentGrassFieldToWrap].SetActive(true);
                                GrassFieldMasters[GrassFieldList[CurrentGrassFieldToWrap]].WrapMethod();
                                WrappingTrigger1 = true;
                                Wrapping = true;
                            }
                            FramesDelayWhenWrappingOneAtATime += 1;
                        }
                    }
                    if (Skip == false)
                    {
                        if (GrassFieldMasters[GrassFieldList[CurrentGrassFieldToWrap]].GrassMeshWrapScript.Wrapping == false
                             && WrappingTrigger1 == true)
                        {
                            FramesDelayWhenWrappingOneAtATime = 0;
                            CurrentGrassFieldToWrap += 1;
                            WrappingTrigger1 = false;
                        }
                    }
                }
                else WrapMeshes = false;
            }
            else
            {
                //when done, Save and reset values. 
                if (Wrapping == true) UnityEditor.SceneManagement.EditorSceneManager.SaveScene(gameObject.scene);
                Wrapping = false;
                CurrentGrassFieldToWrap = 0;
                if (WrappingTrigger2 == false)
                {
                    for (int i = 0; i < GrassFieldList.Count; i++)
                    { GrassFieldList[i].SetActive(true); }
                    WrappingTrigger2 = true;
                }
            }
        }

        protected virtual void OnHierarchyGUI(int instanceID, Rect selectionRect)
        {
            InterceptKeyboardDelete();
        }
        private void OnSceneGUIForDeletion(SceneView scene)
        {
            if (FixUINotAppearingInSceneView == true) ScreenWidthAndHeight = new Vector2(Screen.width / 2.0f, Screen.height / 2.0f);
            else ScreenWidthAndHeight = new Vector2(Screen.width, Screen.height);
            Handles.BeginGUI();
            InterceptKeyboardDelete();
            if (ShowingDeleteAssetsUI == true) DeleteAssetsUI(scene);
            Handles.EndGUI();
        }
        private void OnSceneGUIForCreatedAssetsPath(SceneView scene)
        {
            if (FixUINotAppearingInSceneView == true) ScreenWidthAndHeight = new Vector2(Screen.width / 2.0f, Screen.height / 2.0f);
            else ScreenWidthAndHeight = new Vector2(ScreenWidthAndHeight.x, Screen.height);
            Handles.BeginGUI();
            Color OriginalColor = GUI.color;
            Color BackgroundOriginalColor = GUI.backgroundColor;
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            Event _event = Event.current;
            Vector2 ScreenCenterPos = new Vector2(ScreenWidthAndHeight.x / 2.0f, ScreenWidthAndHeight.y / 2.0f);
            Rect ScreenCenterRect = new Rect(ScreenCenterPos - (DeletionBoxSize / 2.0f), DeletionBoxSize);
            GUILayout.BeginArea(ScreenCenterRect);
            Color OriginalBackgroundColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.0f, 0.0f, 0.0f, 2.0f);
            GUILayout.Box("", GUILayout.MinHeight(ScreenCenterRect.height), GUILayout.MinWidth(ScreenCenterRect.width));
            GUI.backgroundColor = OriginalBackgroundColor;
            GUILayout.EndArea();
            Rect ButtonRect1 = new Rect(ScreenCenterPos
                + new Vector2(ButtonsAddedDistance, 0.0f) - new Vector2(ButtonsDefaultSize.x / 2.0f, ButtonsDefaultSize.y / 2.0f), ButtonsDefaultSize);
            Rect ButtonRect2 = new Rect(ScreenCenterPos
                - new Vector2(ButtonsAddedDistance, 0.0f) - new Vector2(ButtonsDefaultSize.x / 2.0f, ButtonsDefaultSize.y / 2.0f), ButtonsDefaultSize);
            Rect Button2NoteRect = new Rect(ButtonRect1.position + DeleteButtonNoteAddedPosition, DeleteButtonNoteSize);
            Rect TitleRect = new Rect(ScreenCenterPos + ChangePathUITitleAddedPosition - (ChangePathUITitleSize / 2.0f), ChangePathUITitleSize);
            GUILayout.BeginArea(TitleRect);
            GUI.backgroundColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
            GUIStyle TitleBoxStyle = new GUIStyle(GUI.skin.box);
            GUI.backgroundColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
            GUI.color = Color.white;
            TitleBoxStyle.normal.textColor = Color.white;
            TitleBoxStyle.hover.textColor = Color.white;
            TitleBoxStyle.fontSize = ChangePathUITitleFontSize;
            GUILayout.Box("Move Created Files To :", TitleBoxStyle, GUILayout.MinHeight(TitleRect.height), GUILayout.MinWidth(TitleRect.width));
            GUILayout.EndArea();
            Rect PathRect = new Rect(ScreenCenterPos + ChangePathUIPathAddedPosition - (ChangePathUIPathSize / 2.0f), ChangePathUIPathSize);
            GUILayout.BeginArea(PathRect);
            GUIStyle PathTextFieldStyle = new GUIStyle(GUI.skin.box);
            PathTextFieldStyle.fontSize = ChangePathUIPathFontSize;
            PathTextFieldStyle.normal.textColor = Color.white;
            PathTextFieldStyle.hover.textColor = Color.white;
            CreatedFilesPath = GUILayout.TextField(CreatedFilesPath, PathTextFieldStyle, GUILayout.MinHeight(PathRect.height), GUILayout.MinWidth(PathRect.width));
            GUI.backgroundColor = OriginalBackgroundColor;
            GUI.color = OriginalColor;
            GUILayout.EndArea();

            GUIStyle ButtonStyle = new GUIStyle(GUI.skin.button);
            ButtonStyle.fixedHeight = ButtonRect1.height;
            ButtonStyle.fixedWidth = ButtonRect1.width;
            GUILayout.BeginArea(ButtonRect1);
            bool DeleteAssetsButton = GUILayout.Button("Confirm", ButtonStyle);
            GUILayout.EndArea();
            GUILayout.BeginArea(Button2NoteRect);
            GUIStyle DeleteUIBoxStyle1 = new GUIStyle(GUI.skin.box);
            GUI.backgroundColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
            GUI.color = Color.white;
            DeleteUIBoxStyle1.normal.textColor = Color.white;
            DeleteUIBoxStyle1.hover.textColor = Color.white;
            GUILayout.Box("(Moves created files)", DeleteUIBoxStyle1, GUILayout.Width(Button2NoteRect.size.x), GUILayout.Height(Button2NoteRect.size.y));
            GUI.backgroundColor = BackgroundOriginalColor;
            GUI.color = OriginalColor;
            GUILayout.EndArea();
            GUILayout.BeginArea(ButtonRect2);
            bool CancelButton = GUILayout.Button("Cancel", ButtonStyle);
            //Should add "Delete object without deleting corresponding assets ?" Why would anyone want that tho xD ? 
            if (DeleteAssetsButton == true) CreatedPathChangeConfirm = true;
            if (CancelButton == true) CreatedPathChangeCancel = true;
            GUILayout.EndArea();

            Handles.EndGUI();
        }
        private void DeletionWithButton()
        {
            foreach (KeyValuePair<GameObject, GrassFieldMaster> item in GrassFieldMasters)
            {
                if (item.Value.DeleteThisGrassField == true)
                {
                    TryingToDeleteGO = true;
                    GOTryingToBeDeleted.Add(item.Key);
                    item.Value.DeleteThisGrassField = false;
                }
            }
            if (DeleteAll == true)
            {
                TryingToDeleteGO = true;
                GOTryingToBeDeleted.Add(gameObject);
                DeleteAll = false;
            }
        }
        private void InterceptKeyboardDelete()
        {
            var currentEvent = Event.current;
            if (currentEvent.keyCode == KeyCode.Delete)
            {
                TryingToDeleteGO = true;
                for (int i = 0; i < Selection.gameObjects.Length; i++)
                {
                    GOTryingToBeDeleted.Add(Selection.gameObjects[i]);
                }
                currentEvent.type = EventType.Used;
            }
        }
        private bool TryingToDeleteGO;
        private List<GameObject> GOTryingToBeDeleted = new List<GameObject>();

        private void EnableUIMethod()
        {
            //Find other SVGMCs and disable their ShowUI.
            MainControls[] SVGMCFound = (MainControls[])FindObjectsOfType(typeof(MainControls));
            for (int i = 0; i < SVGMCFound.Length; i++)
            {
                if (SVGMCFound[i] != this)
                {
                    if (SVGMCFound[i].EnableUI == true)
                    {
                        if (SVGMCFound[i].PaintShadows == false && SVGMCFound[i].PaintHeight == false && SVGMCFound[i].PaintColor == false)
                        {
                            SVGMCFound[i].EnableUI = false;
                        }
                        else
                        {
                            Debug.Log("Please Stop Painting before enabling the UI of another Vibrant Grass Shader");
                            EnableUI = false;
                        }
                    }
                    if (SVGMCFound[i].InitializationFinished == false)
                    {
                        Debug.Log("Please finish the Initialization process before using another Vibrant Grass Shader");
                        EnableUI = false;
                    }
                }
            }
            SceneView.duringSceneGui += OnSceneGUI;
        }
        private void DisableUIMethod() { SceneView.duringSceneGui -= OnSceneGUI; }

        [SerializeField, HideInInspector] public bool UnityWantsToQuit;

        private bool wantToQuit()
        {
            UnityWantsToQuit = true;
            return true;
        }

        private void InitializePaintingValues()
        {
            BrushTypeChangeKeyDown = false;
            UndoAction = false;
            RedoAction = false;
            StillLeftClickingAfterPressingAlt = false;
            MouseLeftClicking = false;
            ChangedSmthOtherThanTexture = false;
            ExitButtonTrigger1 = false;
            DrawUIEnabled = true;
            SelectingNormal = true;
            SelectingSoften = false;
            SelectingTexture = false;
            SelectingLight = false;
            UseSoftenBrush = false;
            UseTextureBrush = false;
            UseLightBrush = false;
            UseNormalBrush = true;
        }
#endif
        private void Start()
        {
#if UNITY_EDITOR
            CreatingGrass = false;
#endif
            InGameFrameCount = 0;
            if (Application.isPlaying == true)
            {
                for (int i = 0; i < ObjectsToDeleteOnStart.Count; i++)
                { if (ObjectsToDeleteOnStart[i] != null) Destroy(ObjectsToDeleteOnStart[i]); }
            }
        }
#if UNITY_EDITOR

        private int EditorFrameCount;
        [SerializeField, HideInInspector] private bool initializeTrigger1 = false;
        private bool ExitingDrawTrigger1, ShowUITrigger, DeleteInterceptionTrigger1;
        private string CreatedFilesPathOld, CreatedFilesPathBeforeChange;
        [HideInInspector] public bool ChangingCreatedFilesPath;
        private bool CreatedFilesPathChangeTrigger2, CreatedPathChangeCancel, CreatedPathChangeConfirm;
        private bool MouseInsideDrawUI, LeftClickedInsideDrawUIAndHolding;
        private float BrushSizeOld, BrushStrengthOld;
        private LayerMask GroundLayersOld;
        private VGSDensityEnum OldGrassDensity;
        private bool ActiveGOTrigger1;
        private int InitializationFinishedFrameCount, CreatingGrassFieldFrameCount;
        private Vector2 ScreenWidthAndHeight = new Vector2(Screen.width, Screen.height);
        private bool CreatingGrassOld = false;
        void EditorUpdate()
        {
            if (CreatingGrass != CreatingGrassOld) SpawnsLeftDict = new Dictionary<GameObject, int>();
            CreatingGrassOld = CreatingGrass;
            if (WrapAutomatically == true) GrassFieldsToSpawnPerClick = 1;
            if (GrassFieldsToSpawnPerClick < 1) GrassFieldsToSpawnPerClick = 1;
            if (GrassFieldsToSpawnPerClick > 19) GrassFieldsToSpawnPerClick = 19;
            if (RootsHeightsPrecision != RootsHeightsTextureQualityOld)
            {
                Debug.Log("Click 'Create Grass - Wrap Everything' to apply changes");
                Debug.Log("This can be overwritten for each grass field on GrassFieldMaster");
                RootsHeightsTextureQualityOld = RootsHeightsPrecision;
            }
            UnityVersionFixes();
            //Create Prefab
            if (CreatePrefabFromThisGameObject == true)
            {
                DisableUIButtonClicked = true;
                CreatePrefabFrameCount = 0;
                CreatePrefabTrigger1 = true;
                SerializedObject serializedObject = new SerializedObject(this);
                PrefabWasCreated = true;
                SerializedProperty serializedProperty = serializedObject.FindProperty("PrefabWasCreated");
                serializedProperty.boolValue = PrefabWasCreated;
                serializedObject.ApplyModifiedProperties();
                CreatePrefabFromThisGameObject = false;
            }
            if (CreatePrefabTrigger1 == true)
            {
                if (CreatePrefabFrameCount == 10 && EnableUI == false && WindDirectionObject == null)
                {
                    Debug.Log("To update this prefab, create a new prefab then delete the old one. Do not use the old one.");
                    CreatePrefab();
                    CreatePrefabTrigger1 = false;
                }
                CreatePrefabFrameCount += 1;
            }
            //WrapButtons Logic
            if (WrapAutomaticButtonTrigger1 == true)
            {
                if (WrapAutomatically == true) WrapAutomatically = false;
                else WrapAutomatically = true;
                WrapAutomaticButtonTrigger1 = false;
            }
            if (WrapButtonTrigger1 == true)
            {
                if (WrapMeshes == false)
                {
                    WrappingEverything = false;
                    WrapMeshes = true;
                }
                else WrapMeshes = false;
                WrapButtonTrigger1 = false;
            }
            if (WrapButtonTrigger2 == true)
            {
                WrappingEverything = true;
                WrapMeshes = true;
                WrapButtonTrigger2 = false;
            }
            if (CreatingGrassFieldWithoutWrapping == true)
            {
                if (CreatingGrassFieldFrameCount < 1) CreatingGrassFieldFrameCount += 1;
                else
                {
                    CreatingGrassFieldWithoutWrapping = false;
                    CreatingGrassFieldFrameCount = 0;
                }
            }
            if (Wrapping == true)
            {
                PaintShadows = false;
                PaintHeight = false;
                PaintColor = false;
            }
            //Enable Transparency after Initializing
            if (InitializationFinished == true)
            {
                if (InitializationFinishedFrameCount == 10 && CreatingGrassField == false) EnableTransparency = true;
                if (InitializationFinishedFrameCount <= 10) InitializationFinishedFrameCount += 1;
            }
            //WindDirection 
            float DegreesToRadian = ((WindDirectionDegrees - 90) * -1) * Mathf.Deg2Rad;
            PropertyWindDirection = new Vector3(Mathf.Cos(DegreesToRadian), 0.0f, Mathf.Sin(DegreesToRadian));

            //Lock Scale
            transform.localScale = Vector3.one;
            Vector3 globalScale = Vector3.one;
            transform.localScale = new Vector3(globalScale.x / transform.lossyScale.x, globalScale.y / transform.lossyScale.y, globalScale.z / transform.lossyScale.z);

            if (EnableUIWhenSelected == true && Selection.activeGameObject == gameObject)
            {
                if (ActiveGOTrigger1 == false)
                {
                    EnableUI = true;
                    ActiveGOTrigger1 = true;
                }
            }
            else ActiveGOTrigger1 = false;
            if (GrassDensity != OldGrassDensity)
            {
                string originalString = GrassDensity.ToString();
                int IntResult = 0;
                int.TryParse(originalString.Replace("_", ""), out IntResult);
                IntResult -= 1;
                if (OriginalMeshes.Length > IntResult)
                {
                    if (OriginalMeshes[IntResult] != null)
                    {
                        if (CreatedMeshFolder != null) AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(CreatedMeshFolder));
                        CurrentAppliedMeshOriginal = OriginalMeshes[IntResult];
                        CurrentAppliedMeshCompressed = OriginalMeshesCompressed[IntResult];
                        WrappingEverything = true;
                        WrapMeshes = true;
                    }
                }
            }
            OldGrassDensity = GrassDensity;
            if (Application.isPlaying == false && this != null)
            {
                if (GroundLayers != GroundLayersOld)
                {
                    if (GroundLayers.value == (GroundLayers.value | (1 << 5)))
                    {
                        GroundLayers &= ~(1 << 5);
                        Debug.Log("Can't use the Layer 5 (UI). It was removed automatically.");
                    }

                    GroundLayersOld = GroundLayers;
                }

                List<Object> ObjectsToRemoveFromList = new List<Object>();
                for (int i = 0; i < CreatedAssets.Count; i++) { if (CreatedAssets[i] == null) ObjectsToRemoveFromList.Add(CreatedAssets[i]); }
                for (int i = 0; i < ObjectsToRemoveFromList.Count; i++) { CreatedAssets.Remove(ObjectsToRemoveFromList[i]); }

                if (MouseInsideDrawUI == true)
                {
                    if (MouseLeftClickDownFirstFrame == true)
                    {
                        LeftClickedInsideDrawUIAndHolding = true;
                    }
                }
                MouseInsideDrawUI = false;
                if (MouseLeftClickUpFirstFrame == true)
                {
                    LeftClickedInsideDrawUIAndHolding = false;
                }
                if (LeftClickedInsideDrawUIAndHolding == true || ChangeSizeKeyDown == true || ChangeStrengthKeyDown == true)
                {
                    BrushSizeOld = BrushSize;
                    BrushStrengthOld = BrushStrength;
                }
                if (InitializationFinished == true)
                {
                    if (ChangingCreatedFilesPath == true)
                    {
                        if (CreatedFilesPathChangeTrigger2 == false)
                        {
                            SceneView.duringSceneGui += OnSceneGUIForCreatedAssetsPath;
                            CreatedFilesPathChangeTrigger2 = true;
                        }
                        if (CreatedPathChangeCancel == true || CreatedPathChangeConfirm == true)
                        {
                            SceneView.duringSceneGui -= OnSceneGUIForCreatedAssetsPath;
                            if (CreatedPathChangeConfirm == true)
                            {
                                bool DirectoryMoved = MoveCreatedAssetsToNewDirectory(CreatedFilesPath);
                                if (DirectoryMoved == true)
                                {
                                    AssetDatabase.Refresh();
                                    string OriginalDirectoryPath = CreatedFilesDirectoryPath;
                                    CreatedFilesDirectoryPath = CreatedFilesPath + "/" + CreatedFilesDirectory.name;
                                    CreatedFilesDirectory = AssetDatabase.LoadAssetAtPath(CreatedFilesDirectoryPath, typeof(Object));
                                    AssetDatabase.DeleteAsset(OriginalDirectoryPath);
                                    List<Object> ObjectsToDelete = new List<Object>();
                                    for (int i = 0; i < CreatedAssets.Count; i++)
                                    {
                                        if (CreatedAssets[i] == null) ObjectsToDelete.Add(CreatedAssets[i]);
                                    }
                                    for (int i = 0; i < ObjectsToDelete.Count; i++)
                                    {
                                        CreatedAssets.Remove(ObjectsToDelete[i]);
                                    }
                                    CreatedAssets.Add(CreatedFilesDirectory);
                                    AssetDatabase.Refresh();
                                    AssetDatabase.SaveAssets();
                                    EditorUtility.SetDirty(this);
                                }
                            }
                            if (CreatedPathChangeCancel == true)
                            {
                                CreatedFilesPath = null;
                                CreatedFilesPath = CreatedFilesPathBeforeChange;
                                SerializedObject serializedObject = new SerializedObject(this);
                                SerializedProperty serializedProperty = serializedObject.FindProperty("createdFilesPath");
                                serializedProperty.stringValue = CreatedFilesPathBeforeChange;
                                serializedObject.ApplyModifiedProperties();
                                EditorUtility.SetDirty(this);
                                CreatedFilesPathOld = CreatedFilesPath;
                            }
                            CreatedPathChangeCancel = false;
                            CreatedPathChangeConfirm = false;
                            ChangingCreatedFilesPath = false;
                        }
                    }
                    else CreatedFilesPathChangeTrigger2 = false;
                    if (CreatedFilesPath != CreatedFilesPathOld)
                    {
                        if (ChangingCreatedFilesPath == false)
                        {
                            CreatedFilesPathBeforeChange = CreatedFilesPathOld;
                            ChangingCreatedFilesPath = true;
                        }
                    }
                    CreatedFilesPathOld = CreatedFilesPath;
                }
                else CreatedFilesPathOld = CreatedFilesPath;
                if (InitializationFinished == true)
                {
                    GameObject[] SelectedGO = Selection.gameObjects;
                    bool GrassFieldSelected = false;
                    for (int i = 0; i < SelectedGO.Length; i++)
                    {
                        if (GrassFieldList.Contains(SelectedGO[i]) == true) GrassFieldSelected = true;
                    }
                    if (Selection.Contains(gameObject) || GrassFieldSelected == true)
                    {
                        if (DeleteInterceptionTrigger1 == false)
                        {
                            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
                            SceneView.duringSceneGui += OnSceneGUIForDeletion;
                            DeleteInterceptionTrigger1 = true;
                        }
                    }
                    else
                    {
                        if (DeleteInterceptionTrigger1 == true && TryingToDeleteGO == false)
                        {
                            EditorApplication.hierarchyWindowItemOnGUI -= OnHierarchyGUI;
                            SceneView.duringSceneGui -= OnSceneGUIForDeletion;
                            DeleteInterceptionTrigger1 = false;
                        }
                    }
                }
                if (ShowAudioSources == true) EnableUI = false;
                if (ShowAudioSourcesOld == true && ShowAudioSources == false)
                {
                    if (ShowAudioSourcesUIEnabled == false)
                    {
                        EnableUI = true;
                        ShowAudioSourcesOld = ShowAudioSources;
                    }
                }
                else ShowAudioSourcesOld = ShowAudioSources;

                if (InitializationFinished == false) EnableUI = false;
                if (EnableUI == false)
                {
                    CreatingGrass = false;
                    PaintShadows = false;
                    PaintHeight = false;
                    PaintColor = false;
                }
                if (EnableUI == true || ShowInitializeDebugUI == true)
                {
                    if (ShowUITrigger == false)
                    {
                        EnableUIMethod();
                        ShowUITrigger = true;
                    }
                }
                if (EnableUI == false && ShowUITrigger == true && ShowInitializeDebugUI == false)
                {
                    DisableUIMethod();
                    ShowUITrigger = false;
                }
                if (EnableUI == true)
                {
                    if (WindDirectionSpawnedByShowControls == false && EditorFrameCount > 5 && InitializationFinished == true)
                    {
                        WindDirectionObject = SpawnWindDirectionObjectIfNecessary();
                        WindDirectionSpawnedByShowControls = true;
                    }
                }
                else WindDirectionSpawnedByShowControls = false;
                if (CreatingGrass == false)
                {
                    if (WindDirectionSpawnedByShowArrows == false && EditorFrameCount > 5 && InitializationFinished)
                    {
                        WindDirectionObject = SpawnWindDirectionObjectIfNecessary();
                        WindDirectionSpawnedByShowArrows = true;
                    }
                }
                else WindDirectionSpawnedByShowArrows = false;
                if (PaintShadows == false && PaintHeight == false && PaintColor == false)
                {
                    if (WindDirectionSpawnedByPainting == false && EditorFrameCount > 5 && InitializationFinished)
                    {
                        WindDirectionObject = SpawnWindDirectionObjectIfNecessary();
                        WindDirectionSpawnedByPainting = true;
                    }
                }
                if (PaintShadows == true || PaintHeight == true || PaintColor == true) WindDirectionSpawnedByPainting = false;
                if (WindDirectionObject != null) WindDirectionObjectLastPosition = WindDirectionObject.transform.position;


                CreateFirstGrassFieldIfNotAlreadyHere();
                //CopyMaterialControlValuesOntoOtherMaterial(CreatedMainMat, ref createdOpaqueMat);

                if (InitializationFinished == true && CreatingGrassField == false)
                {
                    WrapOneAtATimeMethod(); 
                    if (ResetCutGrassFieldsWithMeshes == true)
                    {
                        WrapCutMeshesEvent?.Invoke();
                        ResetCutGrassFieldsWithMeshes = false;
                    }
                    if (CreateMeshesToHideFlattenedGrass == true)
                    {
                        CutUnusedMeshEvent?.Invoke();
                        CreateMeshesToHideFlattenedGrass = false;
                    }
                }
                if (InitializationFinished == false || CreatingGrassField == true)
                {
                    WrapMeshes = false;
                    ResetCutGrassFieldsWithMeshes = false;
                    CreateMeshesToHideFlattenedGrass = false;
                }
                if (Initialize == true && InitializationFinished == false)
                {
                    PrefabInstanceStatus prefabInstanceStatus = PrefabUtility.GetPrefabInstanceStatus(gameObject);
                    if (prefabInstanceStatus == PrefabInstanceStatus.Connected) PrefabUtility.UnpackPrefabInstance(gameObject, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
                    bool FolderPathIsValid = false;
                    if (AssetDatabase.IsValidFolder(CreatedFilesPath) == false)
                    {
                        Debug.Log("Created Files Path isn't Valid");
                        Initialize = false;
                    }
                    else FolderPathIsValid = true;
                    if (FolderPathIsValid == true)
                    {
                        CreatingGrassField = true;
                        CreateMaterialsAssets();
                        WrapMeshes = true;
                        WrapEvent?.Invoke();
                        CreatingGrass = true;
                        EnableUI = true;
                        ShowInitializeDebugUI = false;
                        InitializationFinished = true;
                    }
                }
                if (Initialize == false && InitializationFinished == true)
                {
                    Debug.Log("You cannot initialize twice. To wrap the mesh again, use 'MainControls-Secondary-Reset And Wrap Meshes'");
                    Initialize = true;
                }
                //Disable ShowControls when entering Drawing 
                if (PaintShadows == true || PaintHeight == true || PaintColor == true)
                {
                    CreatingGrass = false;
                    if (ExitingDrawTrigger1 == false)
                    {
                        if (PaintShadows == true) GetComponent<GrassShadowsPainter>().enabled = true;
                        if (PaintHeight == true)
                        {
                            HeightPaintingFloorAndCeiling = new Vector2(0.0f, 0.85f);
                            GetComponent<GrassHeightPainter>().enabled = true;
                        }
                        if (PaintColor == true) GetComponent<GrassColorPainter>().enabled = true;
                        SVGMCTextureDrawing.enabled = true;
                        InitializePaintingValues();
                        ExitingDrawTrigger1 = true;
                    }
                    MouseScrollSizeAndStrength();
                    ExitingDrawTrigger1 = true;
                }
                if (PaintShadows == false && PaintHeight == false && PaintColor == false)
                {
                    if (ExitingDrawTrigger1 == true)
                    {
                        ExitingDrawTrigger1 = false;
                    }
                    if (SVGMCTextureDrawing.enabled == true) SVGMCTextureDrawing.enabled = false;
                }

                AddAndRemoveGrassFields();
                ControlsLogic();
                if (EditorFrameCount == 5) if (ShowArrowsToActivate == true) CreatingGrass = true;
                if (this != null) AddAndRemoveGridValues();
                if (SessionState.GetBool("GSMUnityLaunchFirstFrame", false) == false)
                {
                    //As Unity starts, Reset Draw booleans so you can select objects (Selecting is disabled while Drawing) 
                    PaintHeight = false;
                    PaintShadows = false;
                    PaintColor = false;
                    SessionState.SetBool("GSMUnityLaunchFirstFrame", true);
                }
                EditorFrameCount += 1;
            }
        }

        [SerializeField] public Shader GrassShader2019 = null, GrassShader2019Gamma = null, UIShader2019 = null, UITransparentShader2019 = null;
        [SerializeField] public Material ColorUIMat = null, TransparentUIMat = null, WindDirectionObjectMat = null;
        [SerializeField] private bool MaterialsChangedForUnity2019 = false;
        private int VersionFixFrameCount;
        private void UnityVersionFixes()
        {
            bool SwitchToGamma = false;
            bool SwitchToLinear = false;
            bool SwitchToGamma2019 = false;
            bool SwitchToLinear2019 = false;
            if (CreatedMainMat != null)
            {
                bool UsingGamma = false;
                if (PlayerSettings.colorSpace == ColorSpace.Gamma) UsingGamma = true;
                if (Application.unityVersion.Contains("2019") == true)
                {
                    if (VersionFixFrameCount >= 10)
                    {
                        if (UsingGamma == true && CreatedMainMat.shader != GrassShader2019Gamma) SwitchToGamma2019 = true;
                        if (UsingGamma == false && CreatedMainMat.shader != GrassShader2019) SwitchToLinear2019 = true;
                    }
                }
                else
                {
                    if (UsingGamma == true && CreatedMainMat.shader != VibrantGrassShaderGamma) SwitchToGamma = true;
                    if (UsingGamma == false && CreatedMainMat.shader != VibrantGrassShaderLinear) SwitchToLinear = true;
                }
            }
            VersionFixFrameCount += 1;
            if (SwitchToGamma == true || SwitchToLinear == true)
            {
                Shader shaderToApply = null;
                if (SwitchToGamma == true) shaderToApply = VibrantGrassShaderGamma;
                if (SwitchToLinear == true) shaderToApply = VibrantGrassShaderLinear;
                MainOriginalMatBuiltIn.shader = shaderToApply;
                MainOriginalMatURP.shader = shaderToApply;
                MainOriginalMatHDRP.shader = shaderToApply;
                OpaqueOriginalMatBuiltIn.shader = shaderToApply;
                OpaqueOriginalMatURP.shader = shaderToApply;
                OpaqueOriginalMatHDRP.shader = shaderToApply;
                CreatedMainMat.shader = shaderToApply;
                CreatedOpaqueMat.shader = shaderToApply;
                for (int i = 0; i < GrassFieldList.Count; i++)
                { GrassFieldMasters[GrassFieldList[0]].GrassMeshObject.GetComponent<MeshRenderer>().sharedMaterial.shader = shaderToApply; }
                AssetDatabase.SaveAssets();
            }
            if (SwitchToGamma2019 == true || SwitchToLinear2019 == true)
            {
                if (GrassShader2019Gamma != null)
                {
                    Shader shaderToApply = GrassShader2019;
                    if (SwitchToGamma2019 == true) shaderToApply = GrassShader2019Gamma;
                    MainOriginalMatBuiltIn.shader = shaderToApply;
                    MainOriginalMatURP.shader = shaderToApply;
                    MainOriginalMatHDRP.shader = shaderToApply;
                    OpaqueOriginalMatBuiltIn.shader = shaderToApply;
                    OpaqueOriginalMatURP.shader = shaderToApply;
                    OpaqueOriginalMatHDRP.shader = shaderToApply;
                    CreatedMainMat.shader = shaderToApply;
                    CreatedOpaqueMat.shader = shaderToApply;

                    ColorUIMat.shader = UIShader2019;
                    TransparentUIMat.shader = UITransparentShader2019;
                    WindDirectionObjectMat.shader = UIShader2019;
                    for (int i = 0; i < GrassFieldList.Count; i++)
                    { GrassFieldMasters[GrassFieldList[0]].GrassMeshObject.GetComponent<MeshRenderer>().sharedMaterial.shader = shaderToApply; }
                    SerializedObject serializedObject = new SerializedObject(this);
                    MaterialsChangedForUnity2019 = true;
                    SerializedProperty serializedProperty = serializedObject.FindProperty("MaterialsChangedForUnity2019");
                    serializedProperty.boolValue = MaterialsChangedForUnity2019;
                    serializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(MainOriginalMatBuiltIn);
                    AssetDatabase.SaveAssets();
                }
                else
                {
                    Debug.Log("The Example scenes don't work properly in Unity 2019 when rendering in Gamma Color Space");
                }
            }
        }

        private bool WindDirectionSpawnedByShowControls, WindDirectionSpawnedByShowArrows, WindDirectionSpawnedByPainting, 
            CreatePrefabTrigger1;
        [SerializeField, HideInInspector] private bool PrefabWasCreated;
        private int CreatePrefabFrameCount;

        private void CreatePrefab()
        {
            PrefabInstanceStatus prefabInstanceStatus = PrefabUtility.GetPrefabInstanceStatus(gameObject);
            if (prefabInstanceStatus == PrefabInstanceStatus.Connected) PrefabUtility.UnpackPrefabInstance(gameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            CreatedDirectoryIfNecessary();
            string PrefabsFolderPath = CreatedFilesDirectoryPath + "/_Prefabs";//UnderScore because it's added to the folder automatically 
            if (AssetDatabase.IsValidFolder(PrefabsFolderPath) == false)
            {
                AssetDatabase.CreateFolder(CreatedFilesDirectoryPath, "/Prefabs");
            }
            string prefabPath = PrefabsFolderPath + "/" + gameObject.name + ".prefab";
            prefabPath = AssetDatabase.GenerateUniqueAssetPath(prefabPath);
            PrefabUtility.SaveAsPrefabAsset(gameObject, prefabPath);
            SerializedObject serializedObject = new SerializedObject(this);
            CreatedPrefab = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(Object)) as Object;
            SerializedProperty serializedProperty = serializedObject.FindProperty("CreatedPrefab");
            serializedProperty.objectReferenceValue = CreatedPrefab;
            serializedObject.ApplyModifiedProperties();
            CreatedAssets.Add(CreatedPrefab);
            AssetDatabase.SaveAssets();
            Selection.activeObject = CreatedPrefab;
        }

        private bool MoveCreatedAssetsToNewDirectory(string NewPath)
        {
            bool ValidNewPath = false, NewDirectoryPathAlreadyExists = false;
            if (AssetDatabase.IsValidFolder(NewPath) == false) Debug.Log("New Path isn't Valid");
            if (NewPath == CreatedFilesPathBeforeChange) Debug.Log("New Path is the same as the Original Path");
            if (AssetDatabase.IsValidFolder(NewPath) == true && NewPath != CreatedFilesPathBeforeChange) ValidNewPath = true;
            if (ValidNewPath == true)
            {
                string NewDirectoryPath = NewPath + "/" + CreatedFilesDirectory.name;
                if (AssetDatabase.IsValidFolder(NewDirectoryPath) == true)
                {
                    NewDirectoryPathAlreadyExists = true;
                }
                if (NewDirectoryPathAlreadyExists == true)
                {
                    AssetsOrganizationTools assetsOrganizationTools = new AssetsOrganizationTools();
                    string ObjectFound = null;
                    string[] StringSeperator = new string[1] { "_" };
                    string[] SplitPath = CreatedFilesDirectory.name.Split(StringSeperator, System.StringSplitOptions.None);
                    string DirectoryNameTrimmed = null;
                    for (int i = 0; i < SplitPath.Length; i++)
                    {
                        if (i < SplitPath.Length - 1) DirectoryNameTrimmed += SplitPath[i];
                    }
                    int HighestNumberFound = assetsOrganizationTools.GetHighestNumberInFolder(NewPath, DirectoryNameTrimmed, "_", out ObjectFound);
                    CreatedFilesDirectory.name = "_" + DirectoryNameTrimmed + "_" + (HighestNumberFound + 1).ToString();
                    NewDirectoryPath = NewPath + "/" + CreatedFilesDirectory.name;
                }

                FileUtil.MoveFileOrDirectory(AssetDatabase.GetAssetPath(CreatedFilesDirectory), NewDirectoryPath);
                FileUtil.MoveFileOrDirectory(AssetDatabase.GetAssetPath(CreatedFilesDirectory) + ".meta", NewDirectoryPath + ".meta");
            }
            return ValidNewPath;
        }

        public void CopyMaterialControlValuesOntoOtherMaterial(Material MaterialToCopyFrom, ref Material MaterialToPasteOn, float? OverwritenFlattenedGrassHeightRemoved = null)
        {
            if (MaterialToCopyFrom != null && MaterialToPasteOn != null)
            {
                MaterialToPasteOn.SetFloat("_WindHillCollisionStopHeight", MaterialToCopyFrom.GetFloat("_WindHillCollisionStopHeight"));
                MaterialToPasteOn.SetFloat("_IntersectionHeightInvLerpValueA", MaterialToCopyFrom.GetFloat("_IntersectionHeightInvLerpValueA"));
                MaterialToPasteOn.SetFloat("_IntersectionHeightInvLerpValueB", MaterialToCopyFrom.GetFloat("_IntersectionHeightInvLerpValueB"));
                MaterialToPasteOn.SetFloat("_IntersectionHeightMinDistance", MaterialToCopyFrom.GetFloat("_IntersectionHeightMinDistance"));
                MaterialToPasteOn.SetFloat("_WindHillCollisionSlowDownHeightStart", MaterialToCopyFrom.GetFloat("_WindHillCollisionSlowDownHeightStart"));
                MaterialToPasteOn.SetFloat("_WindHillCollisionSlowDownStrength", MaterialToCopyFrom.GetFloat("_WindHillCollisionSlowDownStrength"));
                MaterialToPasteOn.SetFloat("_PaintedColorsShadowMultiplier", MaterialToCopyFrom.GetFloat("_PaintedColorsShadowMultiplier"));
                MaterialToPasteOn.SetColor("_StalkColorTip", MaterialToCopyFrom.GetColor("_StalkColorTip"));
                MaterialToPasteOn.SetColor("_StalkColorRoot", MaterialToCopyFrom.GetColor("_StalkColorRoot"));
                MaterialToPasteOn.SetFloat("_StalkColorGradientDistance", MaterialToCopyFrom.GetFloat("_StalkColorGradientDistance"));
                MaterialToPasteOn.SetFloat("_ColorNoiseScale", MaterialToCopyFrom.GetFloat("_ColorNoiseScale"));
                MaterialToPasteOn.SetFloat("_IntersectionHeightMultiplier", MaterialToCopyFrom.GetFloat("_IntersectionHeightMultiplier"));
                MaterialToPasteOn.SetFloat("_WindHeightInvLerpValueA", MaterialToCopyFrom.GetFloat("_WindHeightInvLerpValueA"));
                MaterialToPasteOn.SetFloat("_WindHeightInvLerpValueB", MaterialToCopyFrom.GetFloat("_WindHeightInvLerpValueB"));
                MaterialToPasteOn.SetColor("_ColorNoiseColor", MaterialToCopyFrom.GetColor("_ColorNoiseColor"));
                MaterialToPasteOn.SetColor("_ShadowColorRoot", MaterialToCopyFrom.GetColor("_ShadowColorRoot"));
                MaterialToPasteOn.SetFloat("_ColorNoiseContrast", MaterialToCopyFrom.GetFloat("_ColorNoiseContrast"));
                MaterialToPasteOn.SetFloat("_WindSpeed", MaterialToCopyFrom.GetFloat("_WindSpeed"));
                MaterialToPasteOn.SetFloat("_WindScale", MaterialToCopyFrom.GetFloat("_WindScale"));
                MaterialToPasteOn.SetFloat("_HeightCutThreshold", MaterialToCopyFrom.GetFloat("_HeightCutThreshold"));
                MaterialToPasteOn.SetFloat("_WindPushStrength", MaterialToCopyFrom.GetFloat("_WindPushStrength"));
                MaterialToPasteOn.SetFloat("_WindSquashStrength", MaterialToCopyFrom.GetFloat("_WindSquashStrength"));
                MaterialToPasteOn.SetColor("_WindAddedColor", MaterialToCopyFrom.GetColor("_WindAddedColor"));
                MaterialToPasteOn.SetFloat("_WindMessinessNoiseScale", MaterialToCopyFrom.GetFloat("_WindMessinessNoiseScale"));
                MaterialToPasteOn.SetFloat("_WindMessinessNoiseStrength", MaterialToCopyFrom.GetFloat("_WindMessinessNoiseStrength"));
                MaterialToPasteOn.SetFloat("_WindMessinessNoiseSpeed", MaterialToCopyFrom.GetFloat("_WindMessinessNoiseSpeed"));
                MaterialToPasteOn.SetVector("_windDirection", MaterialToCopyFrom.GetVector("_windDirection"));
                MaterialToPasteOn.SetFloat("_windDirectionDegrees", MaterialToCopyFrom.GetFloat("_windDirectionDegrees"));
                //MaterialToPasteOn.SetFloat("_TipMessinessNoiseScale", MaterialToCopyFrom.GetFloat("_TipMessinessNoiseScale"));
                MaterialToPasteOn.SetVector("_TipMessinessNoiseMultiplier", MaterialToCopyFrom.GetVector("_TipMessinessNoiseMultiplier"));
                MaterialToPasteOn.SetFloat("_TipPositionNoiseScale", MaterialToCopyFrom.GetFloat("_TipPositionNoiseScale"));
                MaterialToPasteOn.SetFloat("_WindSideRandomPushStrength", MaterialToCopyFrom.GetFloat("_WindSideRandomPushStrength"));
                MaterialToPasteOn.SetFloat("_WindRandomSideNoiseScale", MaterialToCopyFrom.GetFloat("_WindRandomSideNoiseScale"));
                MaterialToPasteOn.SetFloat("_WindRandomSideNoiseSpeed", MaterialToCopyFrom.GetFloat("_WindRandomSideNoiseSpeed"));
                MaterialToPasteOn.SetVector("_TipPositionNoiseMultiplier", MaterialToCopyFrom.GetVector("_TipPositionNoiseMultiplier"));
                MaterialToPasteOn.SetFloat("_IntersectionTransparencyMaxDistance", MaterialToCopyFrom.GetFloat("_IntersectionTransparencyMaxDistance"));
                MaterialToPasteOn.SetFloat("_IntersectionTransparencyStartPosition", MaterialToCopyFrom.GetFloat("_IntersectionTransparencyStartPosition"));
                MaterialToPasteOn.SetFloat("_IntersectionTransparencyEndPosition", MaterialToCopyFrom.GetFloat("_IntersectionTransparencyEndPosition"));
                MaterialToPasteOn.SetColor("_ShadowColorTip", MaterialToCopyFrom.GetColor("_ShadowColorTip"));
                MaterialToPasteOn.SetColor("_ShadowNoiseColor", MaterialToCopyFrom.GetColor("_ShadowNoiseColor"));
                MaterialToPasteOn.SetFloat("_ShadowNoiseContrast", MaterialToCopyFrom.GetFloat("_ShadowNoiseContrast"));
                MaterialToPasteOn.SetFloat("_MinimumHeight", MaterialToCopyFrom.GetFloat("_MinimumHeight"));
                MaterialToPasteOn.SetFloat("_WindMinStrength", MaterialToCopyFrom.GetFloat("_WindMinStrength"));
                if (OverwritenFlattenedGrassHeightRemoved == null) MaterialToPasteOn.SetFloat("_FlattenedGrassHeightRemoved", MaterialToCopyFrom.GetFloat("_FlattenedGrassHeightRemoved"));
                else MaterialToPasteOn.SetFloat("_FlattenedGrassHeightRemoved", (float)OverwritenFlattenedGrassHeightRemoved);
                //EditorUtility.SetDirty(MaterialToPasteOn);
                //I mean I couldn't find a better way to copy paste everything except the surface options xD... And changing the surface options isn't easier x) 
            }
        }

        private void CreateFirstGrassFieldIfNotAlreadyHere()
        {
            if (GrassFieldList.Count <= 0)
            {
                string PrefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(GrassFieldBasePrefab);
                GrassFieldBasePrefab = AssetDatabase.LoadAssetAtPath(PrefabPath, typeof(Object));
                GameObject objectSpawned = PrefabUtility.InstantiatePrefab(GrassFieldBasePrefab, transform) as GameObject;
                PrefabInstanceStatus prefabInstanceStatus = PrefabUtility.GetPrefabInstanceStatus(objectSpawned);
                if (prefabInstanceStatus == PrefabInstanceStatus.Connected) PrefabUtility.UnpackPrefabInstance(objectSpawned, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                objectSpawned.transform.position = transform.position;
                GrassFieldMaster SpawnedGSM = objectSpawned.GetComponent<GrassFieldMaster>();
                GrassFieldCreator SpawnedSVGFC = objectSpawned.GetComponent<GrassFieldCreator>();
                SpawnedSVGFC.GrassFieldBasePrefab = GrassFieldBasePrefab;
                objectSpawned.name = gameObject.name + "GrassField_1";
            }
        }

        private void AddAndRemoveGridValues()
        {
            for (int i = 0; i < GrassFieldListLastFrame.Count; i++)
            {
                if (GrassFieldListLastFrame[i] == null)
                {
                    GridPositionsChanged.Remove(GrassFieldListLastFrame[i]);
                    EditorUtility.SetDirty(this);
                }
            }
            for (int i = 0; i < GrassFieldList.Count; i++)
            {
                if (GrassFieldList[i] != null)
                {
                    int XGridPos = Mathf.RoundToInt((transform.InverseTransformPoint(GrassFieldList[i].transform.position).x - XYGridStartPosLocal.x) / GrassFieldMaxDistance);
                    int YGridPos = Mathf.RoundToInt((transform.InverseTransformPoint(GrassFieldList[i].transform.position).z - XYGridStartPosLocal.y) / GrassFieldMaxDistance);
                    if (GridPositionsChanged.ContainsKey(GrassFieldList[i]) == false)
                    {
                        GridPositionsChanged.Add(GrassFieldList[i], new Vector2Int(XGridPos, YGridPos));
                        EditorUtility.SetDirty(this);
                    }
                }
            }
            GrassFieldListLastFrame = new List<GameObject>();
            for (int i = 0; i < GrassFieldList.Count; i++) GrassFieldListLastFrame.Add(GrassFieldList[i]);
        }

        public void DeleteUILogic()
        {
            if (TryingToDeleteGO == true)
            {
                ShowingDeleteAssetsUI = true;
                if (DeleteGameObjectOnlyTrigger == true || DeleteGOAndAssetsTrigger == true)
                {
                    if (PrefabWasCreated == true)
                    {
                        PrefabInstanceStatus prefabInstanceStatus = PrefabUtility.GetPrefabInstanceStatus(gameObject);
                        if (prefabInstanceStatus == PrefabInstanceStatus.Connected) PrefabUtility.UnpackPrefabInstance(gameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                    }
                    GrassFieldMaster SVGFieldMaster = null;
                    bool DeletingMainControl = false;
                    List<GameObject> GOToDelete = new List<GameObject>();
                    for (int i = 0; i < GOTryingToBeDeleted.Count; i++)
                    {
                        if (GOTryingToBeDeleted[i] == gameObject) DeletingMainControl = true;
                    }
                    if (DeletingMainControl == false)
                    {
                        for (int i = 0; i < GOTryingToBeDeleted.Count; i++)
                        {
                            if (DeleteGOAndAssetsTrigger == true)
                            {
                                List<Object> AssetsToDelete = new List<Object>();
                                if (GOTryingToBeDeleted[i] != gameObject)
                                {
                                    SVGFieldMaster = GOTryingToBeDeleted[i].GetComponent<GrassFieldMaster>();
                                    for (int k = 0; k < SVGFieldMaster.CreatedAssets.Count; k++) { AssetsToDelete.Add(SVGFieldMaster.CreatedAssets[k]); }
                                }
                                for (int j = 0; j < AssetsToDelete.Count; j++)
                                {
                                    if (AssetsToDelete[j] != null) AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(AssetsToDelete[j]));
                                }
                            }
                            if (SVGFieldMaster != null) EditorUtility.SetDirty(this);
                            GOToDelete.Add(GOTryingToBeDeleted[i]);
                        }
                    }
                    else
                    {
                        if (DeleteGOAndAssetsTrigger == true) AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(CreatedFilesDirectory));
                        DestroyImmediate(gameObject);
                    }
                    for (int i = 0; i < GOToDelete.Count; i++)
                    {
                        GameObject GO = GOToDelete[i];
                        outOfSightDisabler.DistFadeOutValuesDict.Remove(GO);
                        EditorUtility.SetDirty(outOfSightDisabler);
                    }
                    for (int i = 0; i < GOToDelete.Count; i++)
                    { DestroyImmediate(GOToDelete[i]); }
                    TryingToDeleteGO = false;
                    ShowingDeleteAssetsUI = false;
                    DeleteGOAndAssetsTrigger = false;
                    DeleteGameObjectOnlyTrigger = false;
                    GOTryingToBeDeleted = new List<GameObject>();
                }
                if (CancelTrigger == true)
                {
                    TryingToDeleteGO = false;
                    ShowingDeleteAssetsUI = false;
                    CancelTrigger = false;
                    GOTryingToBeDeleted = new List<GameObject>();
                }
            }
        }
#endif
        private int InGameFrameCount;
        private void Update()
        {
            if (Application.isPlaying == true)
            {
#if UNITY_EDITOR
                LockDistanceFadeOut();
#endif
                TranspApplyChanges(true);
                if (DestroyRuntimeInstancedMats == true)
                {
                    DestroyInstancedMaterials_Event?.Invoke();
                    DestroyRuntimeInstancedMats = false;
                }
                if (InstanceRuntimeMats == true)
                {
                    InstanceMaterials_Event?.Invoke();
                    InstanceRuntimeMats = false;
                }
                DataUsableInGame();
                if (InGameFrameCount == 0)
                {
                    IntLigDict = new Dictionary<GameObject, DynamicLightAndInteract>();
                    if (FindLightAndInteractObjectsOnStart == true)
                    {
                        DynamicLightAndInteract[] VGSLightInteract = FindObjectsOfType<DynamicLightAndInteract>();
                        for (int i = 0; i < VGSLightInteract.Length; i++)
                        {
                            if (LightAndInteractObjectsList.Contains(VGSLightInteract[i].gameObject) == false) LightAndInteractObjectsList.Add(VGSLightInteract[i].gameObject);
                        }
                    }
                    for (int i = 0; i < LightAndInteractObjectsList.Count; i++)
                    {
                        DynamicLightAndInteract VGSLightAndInteract = LightAndInteractObjectsList[i].GetComponent<DynamicLightAndInteract>();
                        IntLigDict.Add(LightAndInteractObjectsList[i].gameObject, VGSLightAndInteract);
                    }
                    EventForLiIntHasChanged?.Invoke();
                }
                UpdateLightAndInteractDict();
                InGameFrameCount += 1;
            }
        }
        private void DataUsableInGame()
        {
            float Dot = Vector2.Dot(new Vector2(1.0f, 0.0f), new Vector2(PropertyWindDirection.x, PropertyWindDirection.z));
            float DegreesResult;
            if (Dot <= 0.0f)
            {
                DegreesResult = 360.0f - Vector2.Angle(new Vector2(0.0f, 1.0f), new Vector2(PropertyWindDirection.x, PropertyWindDirection.z));
            }
            else
            {
                DegreesResult = Vector2.Angle(new Vector2(0.0f, 1.0f), new Vector2(PropertyWindDirection.x, PropertyWindDirection.z));
            }
            windDirectionDegrees = DegreesResult;
        }
        private void UpdateLightAndInteractDict()
        {
            bool ListHasChanged = false;
            for (int i = 0; i < LightAndInteractObjectsList.Count; i++)
            {
                if (LightAndInteractObjectsList[i] != null
                    && IntLigDict.ContainsKey(LightAndInteractObjectsList[i]) == false)
                {
                    DynamicLightAndInteract VGSLightAndInteract = LightAndInteractObjectsList[i].GetComponent<DynamicLightAndInteract>();
                    IntLigDict.Add(LightAndInteractObjectsList[i].gameObject, VGSLightAndInteract);
                    ListHasChanged = true;
                }
            }
            List<GameObject> GOToRemove = new List<GameObject>();
            foreach (KeyValuePair<GameObject, DynamicLightAndInteract> item in IntLigDict)
            {
                if (LightAndInteractObjectsList.Contains(item.Key) == false
                    || item.Key == null)
                {
                    GOToRemove.Add(item.Key);
                    ListHasChanged = true;
                }
            }
            for (int i = 0; i < GOToRemove.Count; i++) { IntLigDict.Remove(GOToRemove[i]); }
            if (ListHasChanged == true) EventForLiIntHasChanged?.Invoke();
        }

        [SerializeField, HideInInspector] private bool EnableTransparencyOld;
        private void TranspApplyChanges(bool InGame)
        {
            if (EnableTransparency != EnableTransparencyOld) TranspSwitch_Event?.Invoke();
            EnableTransparencyOld = EnableTransparency;
        }

#if UNITY_EDITOR
        private void LockDistanceFadeOut()
        {
            float YAddedMin = 0.0f;
            float MinimumYValue = 7.5f;
            //if (DistanceFadeOutDistanceMinMax.y < MinimumYValue) Debug.Log("DistanceFadeOutDistanceMinMax can't have its maximum value below 7.5f");
            if (DistanceFadeOutMinMax.x > 0.0f && DistanceFadeOutMinMax.y > 0.0f) YAddedMin = 0.001f;
            DistanceFadeOutMinMax.y = Mathf.Clamp(DistanceFadeOutMinMax.y, Mathf.Clamp(DistanceFadeOutMinMax.x + YAddedMin, MinimumYValue, Mathf.Infinity), Mathf.Infinity);
            DistanceFadeOutMinMax.x = Mathf.Clamp(DistanceFadeOutMinMax.x, 0.0f, DistanceFadeOutMinMax.y);
        }
        [SerializeField, HideInInspector] public Vector2 XYGridStartPosLocal;
        private List<GameObject> GrassFieldListLastFrame = new List<GameObject>();
        private bool StillLeftClickingAfterPressingAlt, MouseInsideSceneViewTrigger1, PaintShadowsFirstFrame, PaintHeightFirstFrame, PaintColorFirstFrame;
        private Vector2 HeightPaintingMinMaxTriggerLastFrame;
        private void ControlsLogic()
        {
            LeftControlPressed = LeftControlPressedTrigger;
            if (DisableUIButtonClicked == true) EnableUI = false;
            DisableUIButtonClicked = false;
            LockDistanceFadeOut();
            TranspApplyChanges(false);
            //PaintShadowsMaterialSwitch();
            if (ChangingCreatedFilesPath == true) CreatingGrass = false;
            DeleteUILogic();
            DeletionWithButton();
            if (EditorWindow.mouseOverWindow != null)
            {
                if (ToggleFullScreen == true)
                {
                    if (EditorWindow.mouseOverWindow.maximized == false) EditorWindow.mouseOverWindow.maximized = true;
                    else EditorWindow.mouseOverWindow.maximized = false;
                    ToggleFullScreen = false;
                }
            }
            HeightPaintingFloorAndCeiling.y = Mathf.Clamp(HeightPaintingFloorAndCeiling.y, HeightPaintingFloorAndCeiling.x, Mathf.Infinity);
            HeightPaintingFloorAndCeiling.x = Mathf.Clamp(HeightPaintingFloorAndCeiling.x, 0.0f, HeightPaintingFloorAndCeiling.y);
            HeightPaintingMinMaxTriggerLastFrame = HeightPaintingFloorAndCeiling;
            ShowTextureBrushPickerUI = UseTextureBrush;
            HeightTextureRoundBrushHover = HeightTextureRoundBrushHoverTrigger1;
            HeightTextureSpreadBrushHover = HeightTextureSpreadBrushHoverTrigger1;
            if (HeightTextureRoundBrushHover == true && MouseLeftClicking == true) HeightTextureBrushSelected = 0;
            if (HeightTextureSpreadBrushHover == true && MouseLeftClicking == true) HeightTextureBrushSelected = 1;
            if (PressingAlt == true)
            {
                StillLeftClickingAfterPressingAlt = MouseLeftClickingTrigger;
                MouseLeftClicking = false;
            }
            if (PressingAlt == false)
            {
                if (StillLeftClickingAfterPressingAlt == true)
                {
                    if (MouseLeftClickingTrigger == false)
                    {
                        StillLeftClickingAfterPressingAlt = false;
                        MouseLeftClicking = false;
                    }
                }
                else MouseLeftClicking = MouseLeftClickingTrigger;
            }
            //if (MouseHoverUI == true) MouseLeftClicking = false;
            if (MouseInsideSceneView == true)
            {
                if (MouseInsideSceneViewTrigger1 == false)
                {
                    MouseLeftClicking = false;
                    MouseLeftClickingTrigger = false;
                    InvertCurrentBrush = false;
                    MouseInsideSceneViewTrigger1 = true;
                    BrushTypeChangeKeyDown = false;
                    ChangeStrengthKeyDown = false;
                    ChangeSizeKeyDown = false;
                }
            }
            else
            {
                MouseLeftClicking = false;
                MouseLeftClickingTrigger = false;
                InvertCurrentBrush = false;
                MouseInsideSceneViewTrigger1 = false;
                BrushTypeChangeKeyDown = false;
                ChangeStrengthKeyDown = false;
                ChangeSizeKeyDown = false;
            }
            if (ShowingBrushTypeUI == true)
            {
                BrushTypeButtonsDistAnimated = Mathf.Lerp(50.0f, 150.0f, BrushTypeTime);
                BrushTypeTime += Time.deltaTime * 10.0f;
            }
            else
            {
                BrushTypeButtonsDistAnimated = 50.0f;
                BrushTypeTime = 0.0f;
            }

            //Color Brush Changes 
            if (PaintColor == true)
            {
                if (PaintColorFirstFrame == false)
                {
                    BrushSize = 30.0f;
                    BrushSizeOld = BrushSize;
                    BrushStrength = 0.3f;
                    BrushStrengthOld = BrushStrength;
                    PaintColorFirstFrame = true;
                }

                if (PaintColorTriggerSCENE1 == true)
                {
                    if (ChosenColorsForPainting.ContainsKey(CurrentTex2DColorMask) == true) ChosenColorsForPainting[CurrentTex2DColorMask] = CurrentColorPicked;
                    EditorUtility.SetDirty(this);
                }
            }
            else
            {
                PaintColorFirstFrame = false;
                PaintColorTriggerSCENE1 = false;
            }
            if (PaintShadows == true)
            {
                if (PaintShadowsFirstFrame == false)
                {
                    BrushSize = 50.0f;
                    BrushSizeOld = BrushSize;
                    BrushStrength = 0.2f;
                    BrushStrengthOld = BrushStrength;
                    PaintShadowsFirstFrame = true;
                }
            }
            else
            {
                PaintShadowsFirstFrame = false;
                GrassMeshOverallAlpha = 1.0f;
            }

            if (PaintHeight == true)
            {
                if (PaintHeightFirstFrame == false)
                {
                    PaintHeightFirstFrame = true;
                    BrushSize = 50.0f;
                    BrushSizeOld = BrushSize;
                    BrushStrength = 1.0f;
                    BrushStrengthOld = BrushStrength;
                }
            }
            else
            {
                PaintHeightFirstFrame = false;
            }

            if (MouseOnColorCross == true && MouseLeftClickUpFirstFrame == true && AddedColorLastFrame == false)
            {
                ConfirmColorRemovalUI = true;
            }

            //Color Add and Remove
            if (RemoveColor == true && LeftClickingAfterRemoval == false)
            {
                RemoveColorEvent.Invoke(ColorToRemove);
                ChangedSmthOtherThanTexture = true;
                RemoveColor = false;
                ConfirmColorRemovalUI = false;
            }
            if (LeftClickingAfterRemoval == true)
            {
                if (MouseLeftClicking == false)
                {
                    LeftClickingAfterRemoval = false;
                }
            }
            if (PaintingAddColor == true)
            {
                AddedColorLastFrame = true;
                AddColorEvent?.Invoke();
                ChangedSmthOtherThanTexture = true;
                PaintingAddColor = false;
            }
            else AddedColorLastFrame = false;
            ShowConfirmColorRemovalUI = ConfirmColorRemovalUI;

            //Color ScrollBar detection
            if (MouseOnColorScroll == true && MouseLeftClicking == true) DraggingColorScrollBar = true;
            if (DraggingColorScrollBar == true && MouseLeftClicking == false) DraggingColorScrollBar = false;

            //Left Click Triggers 
            if (MouseLeftClickDownFirstFrame == true) MouseLeftClickDownFirstFrame = false;
            if (MouseLeftClicking == true && LeftClickingTrigger1 == false)
            {
                MouseLeftClickDownFirstFrame = true;
                LeftClickingTrigger1 = true;
            }
            if (MouseLeftClickUpFirstFrame == true) MouseLeftClickUpFirstFrame = false;
            if (MouseOverUI == true && MouseLeftClickDownFirstFrame == true)
            {
                MouseClickedInsideUI = true;
                MouseLastClickWasInsideUI = true;
            }
            if (MouseLeftClickUpFirstFrame == true) MouseClickedInsideUI = false;
            if (MouseOverUI == false && MouseLeftClickDownFirstFrame == true) MouseLastClickWasInsideUI = false;

            if (MouseLeftClicking == false && LeftClickingTrigger1 == true)
            {
                MouseLeftClickUpFirstFrame = true;
                LeftClickingTrigger1 = false;
            }

            if (MouseInsideSceneView == true && ChangeBrushTypeButtonWasPressed == false) ShowingBrushTypeUI = BrushTypeChangeKeyDown;
            if (ChangeBrushTypeButtonWasPressed == true)
            {
                ShowingBrushTypeUI = true;
                MousePosBeforeBrushType = CenterOfSceneScreen;
            }
            if (ChangingBrushLeftClicked == true)
            {
                ShowingBrushTypeUI = false;
                ChangingBrushLeftClicked = false;
                ChangeBrushTypeButtonWasPressed = false;
            }
            MouseOverUI = false;
        }
        private bool PaintColorTriggerSCENE1, LeftClickingTrigger1;
        private float BrushTypeTime, BrushTypeButtonsDistAnimated;

        [HideInInspector] public bool TextureChangedBeforeExit;
        private void SaveTexturesMethod()
        {
            SaveCurrentTextureEvent();
            ChangedSmthOtherThanTexture = false;
        }

        public bool CompareTextures(RenderTexture RT1, RenderTexture RT2)
        {
            Texture2D tex1 = new Texture2D(RT1.width, RT1.height, TextureFormat.RGBA32, false);
            RenderTexture.active = RT1;
            tex1.ReadPixels(new Rect(0, 0, RT1.width, RT1.height), 0, 0);
            tex1.Apply();
            Color[] colors1 = tex1.GetPixels();
            Texture2D tex2 = new Texture2D(RT2.width, RT2.height, TextureFormat.RGBA32, false);
            RenderTexture.active = RT2;
            tex2.ReadPixels(new Rect(0, 0, RT2.width, RT2.height), 0, 0);
            tex2.Apply();
            Color[] colors2 = tex2.GetPixels();
            bool ColorsAreDifferent = false;
            for (int i = 0; i < colors1.Length; i++)
            {
                if (colors1[i] != colors2[i])
                {
                    ColorsAreDifferent = true;
                }
            }
            return ColorsAreDifferent;
        }



        private void MouseScrollSizeAndStrength()
        {
            if (ChangeSizeKeyDown == true && MouseScrollAdded.y != 0.0f)
            {
                BrushSize += -MouseScrollAdded.y;
                BrushSize = Mathf.Clamp(BrushSize, 1.0f, BrushMaxSize);
            }
            if (ChangeStrengthKeyDown == true && MouseScrollAdded.y != 0.0f)
            {
                BrushStrength += -MouseScrollAdded.y / 100;
                BrushStrength = Mathf.Clamp(BrushStrength, 0.02f, 1.0f);
            }
            MouseScrollAdded = Vector2.zero;
        }

        private int OldSiblingsCount;
        private void AddAndRemoveGrassFields()
        {
            if (transform.childCount != OldSiblingsCount)
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    GrassFieldMaster grassShaderMaster = transform.GetChild(i).gameObject.GetComponent<GrassFieldMaster>();
                    if (grassShaderMaster != null)
                    {
                        if (GrassFieldList.Contains(transform.GetChild(i).gameObject) == false)
                        {
                            GrassFieldList.Add(transform.GetChild(i).gameObject);
                            GrassFieldMasters.Add(transform.GetChild(i).gameObject, grassShaderMaster);
                        }
                    }
                }
                List<GameObject> GrassFieldsToRemove = new List<GameObject>();
                for (int i = 0; i < GrassFieldList.Count; i++)
                {
                    if (GrassFieldList[i] == null) GrassFieldsToRemove.Add(GrassFieldList[i]);
                }
                foreach (GameObject _go in GrassFieldsToRemove)
                {
                    GrassFieldMasters.Remove(_go);
                    GrassFieldList.Remove(_go);
                }
            }
            OldSiblingsCount = transform.childCount;
        }
        private GameObject SpawnWindDirectionObjectIfNecessary()
        {
            bool CreateWindDirectionObject2 = false;
            CreateWindDirectionObject2 = true;
            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).gameObject.GetComponent<WindDirectionObject>() != null)
                {
                    CreateWindDirectionObject2 = false;
                }
            }
            if (CreateWindDirectionObject2 == true)
            {
                string PrefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(WindDirectionObjectPrefab);
                WindDirectionObjectPrefab = AssetDatabase.LoadAssetAtPath(PrefabPath, typeof(Object));
                GameObject objectSpawned = PrefabUtility.InstantiatePrefab(WindDirectionObjectPrefab, transform) as GameObject;
                PrefabInstanceStatus prefabInstanceStatus = PrefabUtility.GetPrefabInstanceStatus(objectSpawned);
                if (prefabInstanceStatus == PrefabInstanceStatus.Connected) PrefabUtility.UnpackPrefabInstance(objectSpawned, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                if (WindDirectionObjectLastPosition != Vector3.zero) objectSpawned.transform.position = WindDirectionObjectLastPosition;
                else objectSpawned.transform.position = transform.GetChild(0).transform.position + (Vector3.up * 15.0f);
                objectSpawned.transform.localScale = Vector3.one * 0.5f;
                float WindDirectionDegrees = this.WindDirectionDegrees;
                float DegreesToRadian = ((WindDirectionDegrees - 90) * -1) * Mathf.Deg2Rad;
                Vector2 DirectionVector = new Vector2(Mathf.Cos(DegreesToRadian), Mathf.Sin(DegreesToRadian));
                objectSpawned.transform.LookAt(objectSpawned.transform.position + new Vector3(DirectionVector.x, 0.0f, DirectionVector.y));
                objectSpawned.name = transform.name + "WindDirection";
                //objectSpawned.transform.SetSiblingIndex(transform.GetSiblingIndex() + 1);
                objectSpawned.transform.SetAsLastSibling();
                WindDirectionObject SVGWindDirectionObject = objectSpawned.GetComponent<WindDirectionObject>();
                SVGWindDirectionObject.SVGMC = this;
                return objectSpawned;
            }
            else return null;
        }


        [HideInInspector]
        public bool MouseLeftClicking, PressingAlt, InvertCurrentBrush, UndoAction,
            RedoAction, UseNormalBrush, UseSoftenBrush, UseTextureBrush, UseLightBrush, BrushTypeChangeKeyDown,
            ChangeSizeKeyDown, ChangeStrengthKeyDown, MouseOverUI, MouseClickedInsideUI, MouseLastClickWasInsideUI, MouseInsideSceneView, MouseOnColorScroll;
        [HideInInspector] public int HeightTextureBrushSelected;
        private bool HeightTextureRoundBrushHover, HeightTextureRoundBrushHoverTrigger1, HeightTextureSpreadBrushHover, HeightTextureSpreadBrushHoverTrigger1, ShowTextureBrushPickerUI;
        private bool MouseLeftClickingTrigger, ShowingBrushTypeUI, MouseLeftClickDownFirstFrame, MouseLeftClickUpFirstFrame, AddedColorLastFrame;
        [HideInInspector, SerializeField] private bool ChangeBrushTypeButtonWasPressed;
        [HideInInspector] public Vector2 MouseScrollAdded;
        private Vector2 UICreatingGrassRectAddedPos = new Vector2(203.4f, -100.0f), UICreatingGrassRectSize = new Vector2(233.79f, 138.0f);
        private Color WrapCancelColor = new Color(1.0f, 0.2733624f, 0.2f, 1.0f);
        private Vector2 UIRectangleSize = new Vector2(400.0f, 126.27f), UIRectangleAddedPosition = new Vector2(3.36f, 47.0f), UIDrawRectangleSize = new Vector2(250.0f, 274.97f),
            UIDrawRectangleAddedPosition = new Vector2(4.3f, 190.6f), KeyCodeAddedPos = new Vector2(2.71f, 218.1f), BrushStrengthAddedPosition = new Vector2(0.0f, 129.1f), OverallAlphaAddedPosition = new Vector2(0.0f, 174.5f);
        private Color SelectedColor = new Color(0.2586667f, 0.6073044f, 0.7607843f, 1f);
        private Vector2 QuittingCheckRectSize = new Vector2(175.0f, 20.0f), WheelButtonsSize = new Vector2(120.0f, 25.0f);
        private Vector2 ColorPickerRectSize = new Vector2(298.3f, 50.0f), ColorPickerRectPosition = new Vector2(-293.1f, 261.0f),
            ColorFieldRectPos = new Vector2(0.7f, -46.1f), ColorFieldRectSize = new Vector2(275.0f, 45.0f), CrossButtonAddedPos = new Vector2(66.4f, 1.38f),
            ColorBackgroundRectAddedPos = new Vector2(6.54f, 5.07f), ColorBackgroundRectSize = new Vector2(298.8f, 64.8f);
        private float ColorPickerTextureBrushAddedHeight = -79.0f, ScrollSizeX = 289.17f, ScrollSizeY = 211.5f, CrossButtonWidth = 25.44f, CrossButtonAddedXPosFix = 3.1f, CrossButtonAddedYPosFix = 21.0f, CrossButtonAddedYBASEPosFix = 1.77f,
            ColorBackgroundRectAddedHeightPerLine = 21.0f;
        private int ColorRectMaximumHeightMultiplier = 10, ColorMasksTextFontSize = 15;
        public int SelectedColorIndex;
        [ColorUsage(true, true)]
        private Color CrossButtonNormal = new Color(1.637361f, 1.637361f, 1.637361f, 1.0f), CrossButtonHover = new Color(0.8396226f, 0.8396226f, 0.8396226f, 1.0f),
            CrossButtonClick = new Color(0.0f, 0.0f, 0.0f, 1.0f), OutlineButtonColor = new Color(2.116059f, 3.154069f, 4.541205f, 1.0f);
        private Vector4 ScrollRectPosSize = new Vector4(279.4f, 25.4f, 20.0f, 236.0f);
        [HideInInspector] private bool ExitButtonTrigger1, DrawUIEnabled, WrapAutomaticButtonTrigger1, WrapButtonTrigger1, WrapButtonTrigger2;
        private Vector2 InitializeUIBoxPosMultiplier = new Vector2(0.0f, 0.2f), InitializeUIBoxSize = new Vector2(323.0f, 183.17f), InitializeUITextBoxSize = new Vector2(254.89f, 100.0f);
        private float InitializeUIBoxTextStartYPos = 20.0f;
        [SerializeField] private float[] InitializeUITextBoxAddedPosPerText = new float[3] { -98.9f, -25.0f, 16.9f };
        private Color InitializeUIBoxColor = new Color(0.0f, 0.0f, 0.0f, 1.0f);
        private int InitializeUIBoxTextSize = 15;
        private Vector2 HeightTexturePickerTexturesRectAddedPos = new Vector2(-153.1f, 195.2f), HeightTexturePickerTexturesRectSize = new Vector2(150.0f, 75.0f),
            HeightPaintingMinMaxBackgroundAddedPos = new Vector2(-160.05f, 86.1f), HeightPaintingMinMaxBackgroundSize = new Vector2(159.59f, 188.2f), HeightPaintingMinMaxAddedPos = new Vector2(4.3f, 9.8f), HeightPaintingMinMaxAddedPos2 = new Vector2(0.0f, 43.74f), HeightPaintingMinMaxSize = new Vector2(152.06f, 100.0f);
        private Vector2 CreationUISpawnAmountTextBoxAddedPos = new Vector2(30.0f, -26.0f), CreationUISpawnAmountTextRectSize = new Vector2(200.0f, 30.0f), CreationUISpawnAmountTextBoxSize = new Vector2(160.0f, 0.0f);
        private Vector2 CreationUISpawnAmountIntFieldAddedPos = new Vector2(152.0f, 0.0f), CreationUISpawnAmountIntFieldRectSize = new Vector2(30.0f, 30.0f), CreationUISpawnAmountIntFieldBoxSize = new Vector2(30.0f, 22.0f);
        private int CreationUISpawnAmountTextFontSize = 13, CreationUISpawnAmountIntFieldFontSize = 13;
        [ColorUsage(true, true)] private Color HeightTexturePickerOutlineColor = new Color(1.02f, 1.435f, 6.0f, 1.0f);
        public Color CurrentColorPicked;
        private int ColorBackgroundRectAddedHeightMultiplier, ChosenColorCountLastFrame, ColorToRemove;
        private Vector2 ColorScrollPosition;
        private bool RemoveColor, LeftClickingAfterRemoval, ConfirmColorRemovalUI, ShowConfirmColorRemovalUI, DraggingColorScrollBar,
            ToggleFullScreen, MouseOnColorCross, LeftControlPressedTrigger, LeftControlPressed;
        [HideInInspector] public bool PaintingAddColor;
        public delegate void SVGMCDelegateONSCENE(SceneView scene);
        public SVGMCDelegateONSCENE RayDetectionEvent;
        public bool ShowingDeleteAssetsUI = false;
        private bool DisableUIButtonClicked;
        private void OnSceneGUI(SceneView scene)
        {
            if (FixUINotAppearingInSceneView == true) ScreenWidthAndHeight = new Vector2(Screen.width / 2.0f, Screen.height / 2.0f);
            else ScreenWidthAndHeight = new Vector2(Screen.width, Screen.height);
            Handles.BeginGUI();
            Event _event = Event.current;
            if (PaintShadows == true || PaintHeight == true || PaintColor == true)
            {
                OnSceneCtrlZAndYCancel(_event);
                RayDetectionEvent?.Invoke(scene);
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
                Tools.hidden = true;
            }
            Vector3 mousePos = _event.mousePosition;
            float ppp = EditorGUIUtility.pixelsPerPoint;
            mousePos.y = scene.camera.pixelHeight - mousePos.y * ppp;
            mousePos.x *= ppp;
            if (_event.type == EventType.MouseDown && _event.button == 0) MouseLeftClickingTrigger = true;
            if (_event.type == EventType.MouseUp && _event.button == 0) MouseLeftClickingTrigger = false;
            GUI.enabled = false;
            Color GUIOriginalBackgroundColor = GUI.backgroundColor;
            Color GUIOriginalColor = GUI.color;
            GUI.backgroundColor = Color.white * 0.0f;
            Rect SceneViewRect = new Rect(Vector2.zero, new Vector2(ScreenWidthAndHeight.x, ScreenWidthAndHeight.y));
            GUILayout.BeginArea(SceneViewRect);
            GUILayout.Box("", GUILayout.MinHeight(SceneViewRect.height - 40.0f), GUILayout.MinWidth(SceneViewRect.width));
            if (MouseLeftClicking == false)//This is here to prevent a glitch (Mouse no longer inside scene view when left clicking and moving mouse pos) 
            {
                bool MouseInsideSceneViewCheck = SceneViewRect.Contains(_event.mousePosition);
                if (MouseInsideSceneViewCheck == true) MouseInsideSceneView = true;
                else MouseInsideSceneView = false;
            }

            GUILayout.EndArea();
            GUI.enabled = true;
            GUI.backgroundColor = Color.white * 0.0f;

            if (_event.alt == false) PressingAlt = false;
            if (_event.alt == true) PressingAlt = true;
            if (_event.type == EventType.KeyDown && _event.keyCode == InvertBrushKey) InvertCurrentBrush = true;
            if (_event.type == EventType.KeyUp && _event.keyCode == InvertBrushKey) InvertCurrentBrush = false;
            if (_event.type == EventType.KeyDown && _event.keyCode == ChangeBrushTypeKey) BrushTypeChangeKeyDown = true;
            if (_event.type == EventType.KeyUp && _event.keyCode == ChangeBrushTypeKey) BrushTypeChangeKeyDown = false;
            if (_event.type == EventType.KeyDown && _event.keyCode == ChangeSizeKey) ChangeSizeKeyDown = true;
            if (_event.type == EventType.KeyUp && _event.keyCode == ChangeSizeKey) ChangeSizeKeyDown = false;
            if (_event.type == EventType.KeyDown && _event.keyCode == ChangeStrengthKey) ChangeStrengthKeyDown = true;
            if (_event.type == EventType.ScrollWheel)
            {
                MouseScrollAdded = _event.delta;
                if (ChangeSizeKeyDown == true || ChangeStrengthKeyDown == true) _event.Use();
            }
            if (_event.type == EventType.KeyUp && _event.keyCode == ChangeStrengthKey) ChangeStrengthKeyDown = false;
            if (PaintShadows || PaintHeight || PaintColor || CreatingGrass)
            {
                if (_event.type == EventType.KeyDown && _event.keyCode == UndoKey && UndoAction == false)
                {
                    UndoAction = true;
                    UndoEvent?.Invoke();
                }
                if (_event.type == EventType.KeyUp && _event.keyCode == UndoKey && UndoAction == true) UndoAction = false;
                if (_event.type == EventType.KeyDown && _event.keyCode == RedoKey && RedoAction == false)
                {
                    RedoAction = true;
                    RedoEvent?.Invoke();
                }
                if (_event.type == EventType.KeyUp && _event.keyCode == RedoKey && RedoAction == true) RedoAction = false;
                if(_event.type == EventType.KeyDown)
                {
                    if (_event.keyCode == KeyCode.Z || _event.keyCode == KeyCode.Y)
                    {
                        if (CreatingGrass == true) Debug.Log("Please do not use Ctrl-Z/Ctrl-Y while Creation Arrows are enabled");
                        _event.Use();
                    }
                }
            }

            if (ShowInitializeDebugUI == true)
            {
                Rect InitializeMsgRect = new Rect(new Vector2((ScreenWidthAndHeight.x / 2.0f) - InitializeUIBoxSize.x / 2.0f + ScreenWidthAndHeight.x * InitializeUIBoxPosMultiplier.x,
                    (ScreenWidthAndHeight.y / 2.0f) - InitializeUIBoxSize.y / 2.0f + ScreenWidthAndHeight.y * InitializeUIBoxPosMultiplier.y), InitializeUIBoxSize);
                GUILayout.BeginArea(InitializeMsgRect);
                bool GUIOldState = GUI.enabled;
                GUI.enabled = false;
                Color OldBackgroundColor = GUI.backgroundColor;
                GUI.backgroundColor = InitializeUIBoxColor * 4.0f;
                GUILayout.Box("", GUILayout.MinHeight(InitializeMsgRect.height), GUILayout.MinWidth(InitializeMsgRect.width));
                GUILayout.EndArea();
                GUI.backgroundColor = OldBackgroundColor;
                GUIStyle BoxStyle1 = new GUIStyle(GUI.skin.box);
                BoxStyle1.fontSize = InitializeUIBoxTextSize;
                Color TextColor = new Color(1, 1, 1, 1) * 1.6f;
                BoxStyle1.normal.textColor = TextColor;
                BoxStyle1.hover.textColor = TextColor;
                for (int i = 0; i < InitializeTutorialSteps.Length; i++)
                {
                    Rect InitializeMsgRectText = new Rect(new Vector2((ScreenWidthAndHeight.x / 2.0f) - (InitializeUITextBoxSize.x / 2.0f) + ScreenWidthAndHeight.x * InitializeUIBoxPosMultiplier.x,
                        (ScreenWidthAndHeight.y / 2.0f) + (InitializeUITextBoxAddedPosPerText[i]) + InitializeUIBoxTextStartYPos + ScreenWidthAndHeight.y * InitializeUIBoxPosMultiplier.y), InitializeUITextBoxSize);
                    GUILayout.BeginArea(InitializeMsgRectText);
                    GUILayout.Box(InitializeTutorialSteps[i], BoxStyle1, GUILayout.MinHeight(InitializeMsgRectText.height), GUILayout.MinWidth(InitializeMsgRectText.width));
                    GUILayout.EndArea();
                }
                GUI.enabled = GUIOldState;
            }

            if (EnableUI == true && CreatingGrassField == false)
            {
                bool GUIOldState2 = GUI.enabled;
                GUI.enabled = false;
                if (ShowConfirmColorRemovalUI == true || ShowingBrushTypeUI == true) GUI.enabled = false;
                Rect MainUIRect1 = new Rect(new Vector2(ScreenWidthAndHeight.x - UIRectangleAddedPosition.x - UIRectangleSize.x, ScreenWidthAndHeight.y - UIRectangleAddedPosition.y - UIRectangleSize.y), UIRectangleSize);
                Rect GONameRect = new Rect(MainUIRect1.position + GONameAddedPosition, new Vector2(MainUIRect1.size.x, 22.0f));
                GUILayout.BeginArea(GONameRect);
                Color GUIOldColor = GUI.color;
                GUIStyle GONameBoxStyle = new GUIStyle(GUI.skin.box);
                Color TextColor = new Color(10.0f, 10.0f, 10.0f, 10.0f);
                GUI.color = TextColor;
                GONameBoxStyle.normal.textColor = TextColor;
                GONameBoxStyle.hover.textColor = TextColor;
                GUILayout.Box("UI For : " + gameObject.name, GONameBoxStyle, GUILayout.MinWidth(GONameRect.size.x), GUILayout.MinHeight(GONameRect.size.y));
                GUI.color = GUIOldColor;
                GUILayout.EndArea();
                GUI.enabled = GUIOldState2;

                GUILayout.BeginArea(MainUIRect1);
                GUI.backgroundColor = Color.white;
                //Show Controls
                bool GUIEnabledOlfCreatingGrass = GUI.enabled;
                if (PaintHeight == true || PaintShadows == true || PaintColor == true)
                {
                    GUI.enabled = false;
                    CreatingGrass = false;
                }
                else GUI.enabled = GUIEnabledOlfCreatingGrass;
                string ShowControlString = "Create Grass";
                if (CreatingGrass == true)
                {
                    GUI.backgroundColor = SelectedColor * 2.0f;
                    ShowControlString = "Stop Creating Grass";
                }
                bool ShowControlsClicked = GUILayout.Button(ShowControlString);
                GUI.backgroundColor = Color.white;
                if (ShowControlsClicked == true)
                {
                    if (CreatingGrass == true) CreatingGrass = false;
                    else CreatingGrass = true;
                }
                //Draw Shadows 
                if (PaintHeight == true || PaintColor == true || Wrapping == true) GUI.enabled = false;
                else GUI.enabled = true;
                bool DrawShadowsButton;
                string PaintShadowsString = "Paint Shadows";
                if (PaintShadows == true)
                {
                    GUI.backgroundColor = SelectedColor * 2.0f;
                    PaintShadowsString = "Stop Painting Shadows";
                }
                DrawShadowsButton = GUILayout.Button(PaintShadowsString);
                GUI.backgroundColor = Color.white;
                if (DrawShadowsButton == true)
                {
                    if (PaintShadows == true) ExitButtonTrigger1 = true;
                    else PaintShadows = true;
                }
                DrawUIEnabled = true;
                if (ShowConfirmColorRemovalUI == false && ShowingBrushTypeUI == false) if (PaintHeight == true || PaintColor == true) GUI.enabled = true;

                //Draw Height
                if (PaintShadows == true || PaintColor == true || Wrapping == true) GUI.enabled = false;
                string PaintHeightString = "Paint Height";
                if (PaintHeight == true)
                {
                    GUI.backgroundColor = SelectedColor * 2.0f;
                    PaintHeightString = "Stop Painting Height";
                }
                bool DrawHeightButton = GUILayout.Button(PaintHeightString);
                GUI.backgroundColor = Color.white;
                if (DrawHeightButton == true)
                {
                    if (PaintHeight == true) ExitButtonTrigger1 = true;
                    else PaintHeight = true;
                }
                if (ShowConfirmColorRemovalUI == false && ShowingBrushTypeUI == false) if (PaintShadows == true || PaintColor == true) GUI.enabled = true;

                //Draw Colors 
                if (PaintShadows == true || PaintHeight == true || Wrapping == true) GUI.enabled = false;
                string PaintColorString = "Paint Color";
                if (PaintColor == true)
                {
                    GUI.backgroundColor = SelectedColor * 2.0f;
                    PaintColorString = "Stop Painting Color";
                }
                bool DrawColorButton = GUILayout.Button(PaintColorString);
                GUI.backgroundColor = Color.white;
                if (DrawColorButton == true)
                {
                    if (PaintColor == true) ExitButtonTrigger1 = true;
                    else PaintColor = true;
                }
                if (ShowConfirmColorRemovalUI == false && ShowingBrushTypeUI == false) if (PaintShadows == true || PaintHeight == true) GUI.enabled = true;
                //Toggle Full Screen
                bool ToggleFullScreenButton = GUILayout.Button("Toggle Full Screen");
                if (ToggleFullScreenButton == true) ToggleFullScreen = true;
                bool DisableUIButtonClickedTrigger1 = GUILayout.Button("Disable UI");
                if (DisableUIButtonClickedTrigger1 == true) DisableUIButtonClicked = true;

                if (ShowConfirmColorRemovalUI == false && ShowingBrushTypeUI == false) if (PaintHeight == true || PaintShadows == true || PaintColor == true) GUI.enabled = true;
                GUILayout.EndArea();
                //Creating Grass UI
                if (CreatingGrass == true)
                {
                    bool GUIEnabledOriginal = GUI.enabled;
                    GUI.enabled = true;
                    if (WrapAutomatically == true) GUI.enabled = false;
                    Rect CreatingGrassRect = new Rect(new Vector2(MainUIRect1.x + UICreatingGrassRectAddedPos.x - UICreatingGrassRectSize.x / 2.0f,
                        MainUIRect1.y + UICreatingGrassRectAddedPos.y - UICreatingGrassRectSize.y / 2.0f), UICreatingGrassRectSize);
                    //Spawn per click Text
                    GUIStyle SpawnPerClickBoxStyle = new GUIStyle(GUI.skin.box);
                    SpawnPerClickBoxStyle.normal.textColor = Color.white;
                    SpawnPerClickBoxStyle.hover.textColor = Color.white;
                    SpawnPerClickBoxStyle.fontSize = CreationUISpawnAmountTextFontSize;
                    Rect SpawnPerClickTextRect = new Rect(CreatingGrassRect.position + CreationUISpawnAmountTextBoxAddedPos, CreationUISpawnAmountTextRectSize);
                    GUILayout.BeginArea(SpawnPerClickTextRect);
                    GUILayout.Box("Grass Fields Per click : ", SpawnPerClickBoxStyle, GUILayout.MinWidth(CreationUISpawnAmountTextBoxSize.x), GUILayout.MinHeight(CreationUISpawnAmountTextBoxSize.y));
                    GUILayout.EndArea();
                    //Spawn per click IntField
                    Rect SpawnPerClickIntFieldRect = new Rect(SpawnPerClickTextRect.position + CreationUISpawnAmountIntFieldAddedPos, CreationUISpawnAmountIntFieldRectSize);
                    GUIStyle SpawnPerClickIntFieldBoxStyle = new GUIStyle(GUI.skin.textField);
                    SpawnPerClickIntFieldBoxStyle.normal.textColor = Color.white;
                    SpawnPerClickIntFieldBoxStyle.hover.textColor = Color.white;
                    SpawnPerClickIntFieldBoxStyle.alignment = TextAnchor.MiddleCenter;
                    SpawnPerClickIntFieldBoxStyle.fontSize = CreationUISpawnAmountIntFieldFontSize;
                    GUILayout.BeginArea(SpawnPerClickIntFieldRect);
                    GrassFieldsToSpawnPerClick = EditorGUILayout.IntField(GrassFieldsToSpawnPerClick, SpawnPerClickIntFieldBoxStyle, 
                        GUILayout.MaxWidth(CreationUISpawnAmountIntFieldBoxSize.x), GUILayout.MaxHeight(CreationUISpawnAmountIntFieldBoxSize.y));
                    GUILayout.EndArea();
                    //Can't Select while hovering the Buttons 
                    //Rect UIHoverRect = new Rect(UICreatingGrassRectAddedPos.position, UICreatingGrasRectSize.size * 1.10f);
                    GUILayout.BeginArea(CreatingGrassRect);
                    Color OriginalColor = GUI.color;
                    Color OriginalBackgroundColor = GUI.backgroundColor;
                    GUI.color = Color.white;
                    GUI.backgroundColor = Color.white;
                    GUIStyle CreatingGrassButtonStyle1 = new GUIStyle(GUI.skin.button);
                    CreatingGrassButtonStyle1.fixedHeight = CreatingGrassRect.height / 3.5f;
                    CreatingGrassButtonStyle1.fontSize = 18;
                    GUILayout.BeginVertical();
                    GUI.backgroundColor = Color.white;
                    //Wrap Button
                    string WrapButtonString = "Wrap New";
                    if (WrapMeshes == true)
                    {
                        WrapButtonString = "Stop Wrapping";
                        GUI.backgroundColor = WrapCancelColor * 2.0f;
                    }
                    bool WrapButton = GUILayout.Button(WrapButtonString, CreatingGrassButtonStyle1);
                    if (WrapButton == true) WrapButtonTrigger1 = true;
                    //Wrap Everything Button
                    GUI.backgroundColor = Color.white;
                    string WrapEverythingButtonString = "Wrap Everything";
                    bool WrapEverythingButton = GUILayout.Button(WrapEverythingButtonString, CreatingGrassButtonStyle1);
                    if (WrapEverythingButton == true) WrapButtonTrigger2 = true;
                    //Wrap Automatically button
                    GUI.enabled = true;
                    GUI.backgroundColor = Color.white;
                    string WrapAutoString = "Wrapping Manually";
                    if (WrapAutomatically == true)
                    {
                        WrapAutoString = "Wrapping Automatically";
                        GUI.backgroundColor = SelectedColor * 2.0f;
                    }
                    bool ClickedWrapAutomatic = GUILayout.Button(WrapAutoString, CreatingGrassButtonStyle1);
                    if (ClickedWrapAutomatic == true) WrapAutomaticButtonTrigger1 = true;
                    GUILayout.EndVertical();
                    //GUILayout.Box("", GUILayout.MinHeight(UIHoverRect.height), GUILayout.MinWidth(UIHoverRect.width));
                    GUI.color = OriginalColor;
                    GUI.backgroundColor = OriginalBackgroundColor;
                    GUILayout.EndArea();
                    GUI.enabled = GUIEnabledOriginal;
                }

                //Save on Exit Check 
                if (PaintHeight == false && PaintShadows == false && PaintColor == false) ExitButtonTrigger1 = false;
                if (ExitButtonTrigger1 == true)
                {
                    HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
                    MouseOverUI = true;
                    DrawUIEnabled = false;
                    Rect ExitCheckRect = new Rect(new Vector2(ScreenWidthAndHeight.x / 2.0f
                          - QuittingCheckRectSize.x / 2.0f, ScreenWidthAndHeight.y / 2.0f - QuittingCheckRectSize.y - 50.0f), QuittingCheckRectSize);
                    GUI.color = Color.white * 1.5f;
                    GUILayout.BeginArea(ExitCheckRect);
                    bool SaveBeforeQuitting = GUILayout.Button("Save before Quitting");
                    GUILayout.EndArea();
                    GUI.color = GUIOriginalColor;
                    Rect ExitCheckRect2 = new Rect(new Vector2(ScreenWidthAndHeight.x / 2.0f
                         - QuittingCheckRectSize.x / 2.0f, ScreenWidthAndHeight.y / 2.0f - QuittingCheckRectSize.y), QuittingCheckRectSize);
                    GUILayout.BeginArea(ExitCheckRect2);
                    bool QuitButton = GUILayout.Button("Quit Without Saving");
                    GUILayout.EndArea();
                    Rect ExitCheckCancelRect = new Rect(new Vector2(ScreenWidthAndHeight.x / 2.0f - QuittingCheckRectSize.x / 2.0f, ScreenWidthAndHeight.y / 2.0f - QuittingCheckRectSize.y + 50.0f), QuittingCheckRectSize);
                    GUILayout.BeginArea(ExitCheckCancelRect);
                    bool CancelButton = GUILayout.Button("Cancel");
                    GUILayout.EndArea();
                    if (SaveBeforeQuitting == true)
                    {
                        SaveTexturesMethod();
                        PaintShadows = false;
                        PaintHeight = false;
                        PaintColor = false;
                        DrawUIEnabled = true;
                        ExitButtonTrigger1 = false;
                    }
                    if (QuitButton == true)
                    {
                        PaintShadows = false;
                        PaintColor = false;
                        PaintHeight = false;
                        DrawUIEnabled = true;
                    }
                    if (CancelButton == true)
                    {
                        ExitButtonTrigger1 = false;
                        DrawUIEnabled = true;
                    }
                }

                //Can't Select while hovering the Buttons
                GUILayout.BeginArea(MainUIRect1);
                GUI.enabled = false;
                GUILayout.Box("", GUILayout.MinHeight(MainUIRect1.height), GUILayout.MinWidth(MainUIRect1.width));
                if (GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                {
                    MouseOverUI = true;
                    HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
                }
                if (ShowConfirmColorRemovalUI == false && ShowingBrushTypeUI == false) GUI.enabled = true;
                GUILayout.EndArea();

                if (DrawUIEnabled == true)
                {
                    if (PaintShadows == true || PaintHeight == true || PaintColor == true)
                    {
                        Rect DrawingRectangle = new Rect(new Vector2(ScreenWidthAndHeight.x - UIDrawRectangleAddedPosition.x - UIDrawRectangleSize.x,
                            ScreenWidthAndHeight.y - UIDrawRectangleAddedPosition.y - UIDrawRectangleSize.y), UIDrawRectangleSize);
                        //Can't Select while hovering the Buttons 
                        Rect UIHoverRect = new Rect(DrawingRectangle.position, DrawingRectangle.size * 1.10f);
                        GUILayout.BeginArea(DrawingRectangle);
                        Color OriginalColor = GUI.color;
                        GUI.color = Color.black;
                        GUILayout.Box("", GUILayout.MinHeight(UIHoverRect.height), GUILayout.MinWidth(UIHoverRect.width));
                        GUI.color = OriginalColor;
                        if (GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                        {
                            MouseOverUI = true;
                            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
                        }
                        GUILayout.EndArea();

                        GUILayout.BeginArea(DrawingRectangle);
                        bool SaveTextureClicked = GUILayout.Button("Save");
                        if (SaveTextureClicked == true) SaveTexturesMethod();
                        GUILayout.BeginHorizontal();
                        bool EraseTexture = GUILayout.Button("Erase");
                        if (EraseTexture == true) EraseTextureEvent();
                        bool FillTexture = GUILayout.Button("Fill");
                        if (FillTexture == true) FillTextureEvent();
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        bool UndoButtonDown = GUILayout.Button("Undo (" + UndoKey.ToString() + ")");
                        if (UndoButtonDown == true) UndoEvent?.Invoke();
                        bool RedoButtonDown = GUILayout.Button("Redo (" + RedoKey.ToString() + ")");
                        if (RedoButtonDown == true) RedoEvent?.Invoke();
                        GUILayout.EndHorizontal();
                        bool ChangeBrushTypeButton = GUILayout.Button("Change Brush Type (" + ChangeBrushTypeKey.ToString() + ")");
                        if (ChangeBrushTypeButton == true) ChangeBrushTypeButtonWasPressed = true;

                        GUIStyle BrushSizeTextStyle = new GUIStyle(GUI.skin.box);
                        BrushSizeTextStyle.normal.textColor = Color.white;
                        BrushSizeTextStyle.hover.textColor = Color.white;
                        GUILayout.Box("Size (" + ChangeSizeKey.ToString() + " + Mouse Wheel)", BrushSizeTextStyle, GUILayout.MinHeight(20.0f), GUILayout.MinWidth(DrawingRectangle.width));
                        if (LeftClickedInsideDrawUIAndHolding == true || ChangeSizeKeyDown == true) BrushSize = GUILayout.HorizontalSlider(BrushSize, 1.0f, BrushMaxSize);
                        else BrushSize = GUILayout.HorizontalSlider(BrushSizeOld, 1.0f, BrushMaxSize);
                        GUILayout.EndArea();
                        if (UIHoverRect.Contains(Event.current.mousePosition))
                        {
                            MouseInsideDrawUI = true;
                        }
                        Rect BrushStrengthRect = new Rect(new Vector2(DrawingRectangle.position.x + BrushStrengthAddedPosition.x,
                        DrawingRectangle.position.y + BrushStrengthAddedPosition.y), UIDrawRectangleSize);
                        GUILayout.BeginArea(BrushStrengthRect);
                        GUILayout.Box("Strength (" + ChangeStrengthKey.ToString() + " + Mouse Wheel)", BrushSizeTextStyle, GUILayout.MinHeight(20.0f), GUILayout.MinWidth(DrawingRectangle.width));
                        if (LeftClickedInsideDrawUIAndHolding == true || ChangeStrengthKeyDown == true) BrushStrength = GUILayout.HorizontalSlider(BrushStrength, 0.02f, 1.0f);
                        else BrushStrength = GUILayout.HorizontalSlider(BrushStrengthOld, 0.02f, 1.0f);
                        GUILayout.EndArea();
                        if (PaintShadows == true)
                        {
                            Rect OverallAlphaRect = new Rect(new Vector2(DrawingRectangle.position.x + OverallAlphaAddedPosition.x,
                                DrawingRectangle.position.y + OverallAlphaAddedPosition.y), UIDrawRectangleSize);
                            GUILayout.BeginArea(OverallAlphaRect);
                            GUILayout.Box("Transparency Debug", BrushSizeTextStyle, GUILayout.MinHeight(20.0f), GUILayout.MinWidth(DrawingRectangle.width));
                            GrassMeshOverallAlpha = GUILayout.HorizontalSlider(GrassMeshOverallAlpha, 0.02f, 1.0f);
                            GUILayout.EndArea();
                        }
                        //Height BrushTexture Picker
                        if (PaintHeight == true)
                        {
                            Rect HeightPaintingMinMaxBackgroundRect = new Rect(DrawingRectangle.position + HeightPaintingMinMaxBackgroundAddedPos, HeightPaintingMinMaxBackgroundSize);
                            GUILayout.BeginArea(HeightPaintingMinMaxBackgroundRect);
                            GUILayout.Box("", GUILayout.MinWidth(HeightPaintingMinMaxBackgroundRect.width), GUILayout.MinHeight(HeightPaintingMinMaxBackgroundRect.height));
                            if (GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                            {
                                MouseOverUI = true;
                            }
                            GUILayout.EndArea();
                            Rect HeightPaintingMinMaxRect = new Rect(DrawingRectangle.position + HeightPaintingMinMaxBackgroundAddedPos + HeightPaintingMinMaxAddedPos, HeightPaintingMinMaxSize);
                            GUILayout.BeginArea(HeightPaintingMinMaxRect);
                            GUILayout.Box("Painting Floor : ", BrushSizeTextStyle, GUILayout.MinHeight(20.0f), GUILayout.MinWidth(HeightPaintingMinMaxSize.x));
                            HeightPaintingFloorAndCeiling.x = EditorGUILayout.FloatField(HeightPaintingFloorAndCeiling.x);
                            GUILayout.EndArea();
                            Rect HeightPaintingMinMaxRect2 = new Rect(DrawingRectangle.position + HeightPaintingMinMaxBackgroundAddedPos + HeightPaintingMinMaxAddedPos + HeightPaintingMinMaxAddedPos2, HeightPaintingMinMaxSize);
                            GUILayout.BeginArea(HeightPaintingMinMaxRect2);
                            GUILayout.Box("Painting Ceiling : ", BrushSizeTextStyle, GUILayout.MinHeight(20.0f), GUILayout.MinWidth(HeightPaintingMinMaxSize.x));
                            HeightPaintingFloorAndCeiling.y = EditorGUILayout.FloatField(HeightPaintingFloorAndCeiling.y);
                            GUILayout.EndArea();
                        }
                        if (ShowTextureBrushPickerUI == true)
                        {
                            GUI.color = OriginalColor;
                            GUI.backgroundColor = GUIOriginalBackgroundColor;
                            Rect HeightBrushTexturePickerTexturesRect = new Rect(DrawingRectangle.position + HeightTexturePickerTexturesRectAddedPos, HeightTexturePickerTexturesRectSize);
                            Vector2 AddedPosition = Vector2.zero;
                            AddedPosition = new Vector2(HeightBrushTexturePickerTexturesRect.width / 2.0f, 0.0f);
                            HeightTextureGUIBrushSelection(DrawingRectangle, HeightTextureRoundBrushHover, Vector2.zero, 0);
                            HeightTextureGUIBrushSelection(DrawingRectangle, HeightTextureSpreadBrushHover, AddedPosition, 1);

                            GUILayout.BeginArea(HeightBrushTexturePickerTexturesRect);
                            GUIStyle HeightTexPickerBoxStyle1 = new GUIStyle(GUI.skin.box);
                            HeightTexPickerBoxStyle1.fixedWidth = HeightBrushTexturePickerTexturesRect.width / 2.0f;
                            HeightTexPickerBoxStyle1.fixedHeight = HeightBrushTexturePickerTexturesRect.height;
                            GUILayout.Box(SVGMCTextureDrawing.RoundBrushTexture[0], HeightTexPickerBoxStyle1);
                            HeightTextureRoundBrushHoverTrigger1 = false;
                            if (GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                            {
                                HeightTextureRoundBrushHoverTrigger1 = true;
                            }
                            GUILayout.EndArea();
                            Rect HeightBrushTexturePickerTexturesRect2 = new Rect(HeightBrushTexturePickerTexturesRect.position
                                + (new Vector2(HeightBrushTexturePickerTexturesRect.width / 2.0f, 0.0f)),
                                HeightBrushTexturePickerTexturesRect.size);
                            GUILayout.BeginArea(HeightBrushTexturePickerTexturesRect2);
                            GUILayout.Box(SVGMCTextureDrawing.SpreadBrushTexture[0], HeightTexPickerBoxStyle1);
                            HeightTextureSpreadBrushHoverTrigger1 = false;
                            if (GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                            {
                                HeightTextureSpreadBrushHoverTrigger1 = true;
                            }
                            GUILayout.EndArea();
                        }
                        //Color Picker 
                        if (PaintColor == true && SVGColorPainter.enabled == true && SVGColorPainter.ReadyToUse == true)
                        {
                            if (ShowConfirmColorRemovalUI == true || LeftClickingAfterRemoval == true || ShowingBrushTypeUI == true) GUI.enabled = false;
                            float AddedHeight = 0.0f;
                            if (UseTextureBrush == true) AddedHeight = ColorPickerTextureBrushAddedHeight;
                            Vector2 OriginalPosition = DrawingRectangle.position + new Vector2(0.0f, AddedHeight);
                            Rect ColorPickerRect = new Rect(OriginalPosition.x + ColorPickerRectPosition.x, OriginalPosition.y + ColorPickerRectPosition.y
                                - (Mathf.Clamp(ColorBackgroundRectAddedHeightMultiplier, 0, ColorRectMaximumHeightMultiplier) * ColorBackgroundRectAddedHeightPerLine),
                                ColorPickerRectSize.x, ColorPickerRectSize.y * Mathf.Ceil(ChosenColorsForPainting.Count / 3 + 1));
                            Rect ColorFieldRect = new Rect(OriginalPosition.x + ColorPickerRectPosition.x + ColorFieldRectPos.x,
                                OriginalPosition.y + ColorPickerRectPosition.y + ColorFieldRectPos.y
                                - (Mathf.Clamp(ColorBackgroundRectAddedHeightMultiplier, 0, ColorRectMaximumHeightMultiplier) * ColorBackgroundRectAddedHeightPerLine),
                                ColorFieldRectSize.x, ColorFieldRectSize.y);
                            Rect ColorPaintingBackground = new Rect(ColorFieldRect.position - ColorBackgroundRectAddedPos,
                                new Vector2(ColorBackgroundRectSize.x, ColorBackgroundRectSize.y
                                + (Mathf.Clamp(ColorBackgroundRectAddedHeightMultiplier, 0, ColorRectMaximumHeightMultiplier) * ColorBackgroundRectAddedHeightPerLine)));
                            GUILayout.BeginArea(ColorPaintingBackground);
                            Color originalGUIColor = GUI.color;
                            GUI.color = Color.black;
                            GUILayout.Box("", GUILayout.MinHeight(ColorPaintingBackground.height), GUILayout.MinWidth(ColorPaintingBackground.width));
                            GUILayout.EndArea();
                            GUI.color = originalGUIColor;

                            //MouseOverUI 
                            Rect ColorPickerMouseHoverUIRect = new Rect(ColorPaintingBackground.position, new Vector2(ColorPaintingBackground.width, ColorPaintingBackground.height + 5.0f));
                            GUILayout.BeginArea(ColorPaintingBackground);
                            GUI.color = new Color(0, 0, 0, 0);
                            GUILayout.Box("", GUILayout.MinHeight(ColorPickerMouseHoverUIRect.height), GUILayout.MinWidth(ColorPickerMouseHoverUIRect.width));
                            if (GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                            {
                                MouseOverUI = true;
                                //HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));//This line prevents the Color Picking to work xD... 
                            }
                            GUI.color = originalGUIColor;
                            GUILayout.EndArea();

                            GUILayout.BeginArea(ColorFieldRect);
                            CurrentColorPicked = EditorGUILayout.ColorField(CurrentColorPicked);
                            GUIStyle ColorFieldTextBoxStyle = new GUIStyle(GUI.skin.box);
                            ColorFieldTextBoxStyle.fontSize = ColorMasksTextFontSize;
                            ColorFieldTextBoxStyle.normal.textColor = Color.white * 0.9f;
                            ColorFieldTextBoxStyle.hover.textColor = Color.white * 0.9f;
                            GUILayout.Box("Color Masks", ColorFieldTextBoxStyle, GUILayout.MinWidth(ColorFieldRect.width), GUILayout.MinHeight(25.0f));
                            GUILayout.EndArea();

                            //Scroll Box to detect Clicks on it
                            if (ColorBackgroundRectAddedHeightMultiplier > ColorRectMaximumHeightMultiplier)
                            {
                                Rect ScrollRect = new Rect(ColorPaintingBackground.position + new Vector2(ScrollRectPosSize.x, ScrollRectPosSize.y),
                                  new Vector2(ScrollRectPosSize.z, ScrollRectPosSize.w));
                                GUILayout.BeginArea(ScrollRect);
                                Color OriginalBoxColor = GUI.color;
                                GUI.color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
                                GUILayout.Box("", GUILayout.MinWidth(ScrollRect.x), GUILayout.MinHeight(ScrollRect.y));
                                GUI.color = OriginalBoxColor;
                                if (GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition)) MouseOnColorScroll = true;
                                else MouseOnColorScroll = false;
                                GUILayout.EndArea();
                            }

                            GUILayout.BeginArea(ColorPickerRect);
                            if (ChosenColorsForPainting.Count != ChosenColorCountLastFrame) ColorScrollPosition.y = ScrollSizeY;
                            ChosenColorCountLastFrame = ChosenColorsForPainting.Count;
                            ColorScrollPosition = GUILayout.BeginScrollView(ColorScrollPosition, GUILayout.Width(ScrollSizeX), GUILayout.Height(ScrollSizeY));
                            bool ColorButton = false;
                            GUIStyle ColorButtonStyle = new GUIStyle(GUI.skin.button);
                            ColorButtonStyle.fixedWidth = ColorPickerRectSize.x / 3.4f;
                            int AmountOfLines = Mathf.CeilToInt(ChosenColorsForPainting.Count / 3.0f) + 1;
                            bool AddColorButtonAdded = false;
                            ColorBackgroundRectAddedHeightMultiplier = Mathf.CeilToInt((ChosenColorsForPainting.Count + 1) / 3.0f);
                            bool MouseOnACross = false;
                            for (int i = 0; i < AmountOfLines; i++)
                            {
                                GUILayout.BeginHorizontal();
                                int currentColorColumn = 0;
                                while (currentColorColumn <= 2)
                                {
                                    int CurrentColorIndex = (i * 3) + currentColorColumn;
                                    if (AmountOfLines > 1)
                                    {
                                        if (PaintColorTriggerSCENE1 == false && i == 0 && currentColorColumn == 0)
                                        {
                                            CurrentTex2DColorMask = UnSavedColorMasks[0];
                                            CurrentColorPicked = ChosenColorsForPainting[UnSavedColorMasks[0]];
                                            SelectedColorIndex = 0;
                                            PaintColorTriggerSCENE1 = true;
                                        }
                                        if (ChosenColorsForPainting.Count > CurrentColorIndex)
                                        {
                                            if (CurrentColorIndex == SelectedColorIndex)
                                            {
                                                GUI.backgroundColor = OutlineButtonColor;
                                                Rect OutlineRect = new Rect(new Vector2(1.5f + (DrawingRectangle.width / 3 + 7.5f) * currentColorColumn, 21.0f * i),
                                                    new Vector2(150.0f, 150.0f));
                                                GUILayout.BeginArea(OutlineRect);
                                                GUIStyle OutlineButtonStyle = new GUIStyle(GUI.skin.button);
                                                OutlineButtonStyle.fixedWidth = ColorPickerRectSize.x / 3.4f;
                                                OutlineButtonStyle.fixedWidth += 4.0f;
                                                OutlineButtonStyle.fixedHeight = 23.0f;
                                                bool OutlineButton = GUILayout.Button("", OutlineButtonStyle);//Used only for visuals 
                                                GUILayout.EndArea();
                                            }
                                            GUI.backgroundColor = ChosenColorsForPainting[UnSavedColorMasks[CurrentColorIndex]] * 2.0f;
                                            ColorButton = GUILayout.Button("", ColorButtonStyle);
                                            if (ColorButton == true)
                                            {
                                                SelectedColorIndex = CurrentColorIndex;
                                                CurrentTex2DColorMask = UnSavedColorMasks[CurrentColorIndex];
                                                CurrentColorPicked = ChosenColorsForPainting[UnSavedColorMasks[CurrentColorIndex]];
                                            }
                                            if (ChosenColorsForPainting.Count > 0)
                                            {
                                                Rect CrossArea = new Rect(new Vector2(((ColorButtonStyle.fixedWidth + CrossButtonAddedXPosFix) * currentColorColumn) + CrossButtonAddedPos.x,
                                                    CrossButtonAddedYPosFix * i + CrossButtonAddedYBASEPosFix),
                                                    new Vector2(CrossButtonWidth, CrossButtonAddedYPosFix * (i + 1)));
                                                GUILayout.BeginArea(CrossArea);
                                                GUI.backgroundColor = CrossButtonNormal;
                                                bool RemoveColorButton = GUILayout.Button("X");//Used only for visuals
                                                if (GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition) && ShowConfirmColorRemovalUI == false
                                                    && ShowingBrushTypeUI == false && DraggingColorScrollBar == false)
                                                {
                                                    MouseOnACross = true;
                                                    ColorToRemove = CurrentColorIndex;
                                                    GUI.backgroundColor = CrossButtonHover;
                                                    if (MouseLeftClickDownFirstFrame == true)
                                                    {
                                                        //ConfirmColorRemoval = true;
                                                        LeftClickingAfterRemoval = true;
                                                        GUI.backgroundColor = CrossButtonClick;
                                                    }
                                                }
                                                GUILayout.EndArea();
                                                if (ShowConfirmColorRemovalUI == false && ShowingBrushTypeUI == false && DraggingColorScrollBar == false)
                                                {
                                                    Rect CrossAreaHover = new Rect(CrossArea.position, CrossArea.size);
                                                    GUILayout.BeginArea(CrossAreaHover);
                                                    bool RemoveColorButtonHover = GUILayout.Button("X");//Used only for visuals 
                                                    GUILayout.EndArea();
                                                }
                                            }
                                        }
                                    }
                                    GUI.backgroundColor = Color.white;
                                    if (ChosenColorsForPainting.Count > CurrentColorIndex) currentColorColumn += 1;
                                    else
                                    {
                                        bool AddColorButton = GUILayout.Button("Add Color", ColorButtonStyle);
                                        if (AddColorButton == true && LeftClickingAfterRemoval == false) PaintingAddColor = true;
                                        AddColorButtonAdded = true;
                                        break;
                                    }
                                }
                                GUILayout.EndHorizontal();
                                if (AddColorButtonAdded == true) break;
                            }
                            MouseOnColorCross = MouseOnACross;
                            GUI.backgroundColor = Color.grey;
                            GUILayout.EndArea();
                            GUILayout.EndScrollView();
                            if (ShowConfirmColorRemovalUI == true)
                            {
                                MouseOverUI = true;
                                GUI.enabled = true;
                                float BackgroundWidth = 500.0f;
                                float BackgroundHeight = 300.0f;
                                Rect RemovalConfirmUIBackground = new Rect(ScreenWidthAndHeight.x / 2 - BackgroundWidth / 2, ScreenWidthAndHeight.y / 2 - BackgroundHeight / 2 - 10.0f,
                                    BackgroundWidth, BackgroundHeight);
                                GUILayout.BeginArea(RemovalConfirmUIBackground);
                                GUI.backgroundColor = Color.grey;
                                GUILayout.Box("", GUILayout.MinHeight(BackgroundHeight), GUILayout.MinWidth(BackgroundWidth));
                                GUI.backgroundColor = Color.white;
                                GUILayout.EndArea();
                                float DistFromCenter = 150.0f;
                                Rect RemoveConfirmRect = new Rect(ScreenWidthAndHeight.x / 2 - 175 / 2 - DistFromCenter, ScreenWidthAndHeight.y / 2 - 25.0f, 175.0f, 25.0f);
                                GUILayout.BeginArea(RemoveConfirmRect);
                                bool RemoveColorButton = GUILayout.Button("Remove Color");
                                GUILayout.EndArea();
                                Rect CancelRect = new Rect(ScreenWidthAndHeight.x / 2 - 175 / 2 + DistFromCenter, ScreenWidthAndHeight.y / 2 - 25.0f, 175.0f, 25.0f);
                                GUILayout.BeginArea(CancelRect);
                                bool CancelRemovalColor = GUILayout.Button("Cancel");
                                GUILayout.EndArea();
                                if (RemoveColorButton == true)
                                {
                                    RemoveColor = true;
                                    ConfirmColorRemovalUI = false;
                                }
                                if (CancelRemovalColor == true)
                                {
                                    ConfirmColorRemovalUI = false;
                                }
                                GUI.enabled = false;
                            }
                        }
                        Rect KeyCodesRect = new Rect(DrawingRectangle.position + KeyCodeAddedPos, DrawingRectangle.size);
                        GUILayout.BeginArea(KeyCodesRect);
                        GUIStyle CurrentBoxStyle = new GUIStyle(GUI.skin.box);
                        CurrentBoxStyle.normal.textColor = Color.white;
                        CurrentBoxStyle.hover.textColor = Color.white;
                        GUILayout.Box("Paint : " + "Left Click", CurrentBoxStyle, GUILayout.MinHeight(23.0f), GUILayout.MinWidth(125.0f));
                        GUILayout.Box("Invert Brush : " + InvertBrushKey.ToString() + " + Left Click", CurrentBoxStyle, GUILayout.MinHeight(23.0f), GUILayout.MinWidth(225.0f));
                        GUILayout.EndArea();

                        CenterOfSceneScreen = new Vector2(ScreenWidthAndHeight.x / 2, ScreenWidthAndHeight.y / 2);
                        if (BrushTypeChangeKeyDown == false && ShowingBrushTypeUI == false) MousePosBeforeBrushType = _event.mousePosition;
                        if (ShowingBrushTypeUI == true)
                        {
                            Vector2 NormalRectPosition = MousePosBeforeBrushType - WheelButtonsSize / 2 + Vector2.left * BrushTypeButtonsDistAnimated;
                            Vector2 SoftenRectPosition = MousePosBeforeBrushType - WheelButtonsSize / 2 + Vector2.right * BrushTypeButtonsDistAnimated;
                            Vector2 TextureRectPosition = MousePosBeforeBrushType - WheelButtonsSize / 2 + Vector2.down * BrushTypeButtonsDistAnimated * 0.75f;
                            Vector2 LightRectPosition = MousePosBeforeBrushType - WheelButtonsSize / 2 + Vector2.up * BrushTypeButtonsDistAnimated * 0.75f;
                            Vector2 NormalRectButtonCenterForDist = MousePosBeforeBrushType + Vector2.left * BrushTypeButtonsDistAnimated;
                            Vector2 SoftenRectButtonCenterForDist = MousePosBeforeBrushType + Vector2.right * BrushTypeButtonsDistAnimated;
                            Vector2 TextureRectButtonCenterForDist = MousePosBeforeBrushType + Vector2.down * BrushTypeButtonsDistAnimated;
                            Vector2 LightRectButtonCenterForDist = MousePosBeforeBrushType + Vector2.up * BrushTypeButtonsDistAnimated;
                            SelectingNormal = true;
                            SelectingSoften = false;
                            SelectingTexture = false;
                            SelectingLight = false;
                            float ClosestDistance = Vector2.Distance(_event.mousePosition, NormalRectButtonCenterForDist);
                            float DistToSoftButton = Vector2.Distance(_event.mousePosition, SoftenRectButtonCenterForDist);
                            float DistToTextureButton = Vector2.Distance(_event.mousePosition, TextureRectButtonCenterForDist);
                            float DistToLightButton = Vector2.Distance(_event.mousePosition, LightRectButtonCenterForDist);
                            if (DistToSoftButton < ClosestDistance)
                            {
                                SelectingNormal = false;
                                SelectingSoften = true;
                                ClosestDistance = DistToSoftButton;
                            }
                            if (PaintHeight == true || PaintColor == true)
                            {
                                if (DistToTextureButton < ClosestDistance)
                                {
                                    SelectingNormal = false;
                                    SelectingSoften = false;
                                    SelectingTexture = true;
                                    ClosestDistance = DistToTextureButton;
                                }
                            }
                            if (PaintColor == true)
                            {
                                if (DistToLightButton < ClosestDistance)
                                {
                                    SelectingNormal = false;
                                    SelectingSoften = false;
                                    SelectingTexture = false;
                                    SelectingLight = true;
                                    ClosestDistance = DistToLightButton;
                                }
                            }
                            if (ShowingBrushTypeUI == true) GUI.enabled = true;
                            Rect NormalBrushRect = new Rect(NormalRectPosition, WheelButtonsSize);
                            GUILayout.BeginArea(NormalBrushRect);
                            float DistanceToNormalBrushButton = Vector2.Distance(_event.mousePosition, WheelButtonsSize / 2);
                            GUI.backgroundColor = Color.grey;
                            if (SelectingNormal == true) GUI.backgroundColor = Color.white;
                            bool NormalBrush = GUILayout.Button("Normal", GUILayout.MinWidth(WheelButtonsSize.x), GUILayout.MinHeight(WheelButtonsSize.y));
                            GUILayout.EndArea();

                            Rect SoftenBrushRect = new Rect(SoftenRectPosition, WheelButtonsSize);
                            GUILayout.BeginArea(SoftenBrushRect);
                            GUI.backgroundColor = Color.grey;
                            if (SelectingSoften == true) GUI.backgroundColor = Color.white;
                            bool SoftenBrush = GUILayout.Button("Soften Edges", GUILayout.MinWidth(WheelButtonsSize.x), GUILayout.MinHeight(WheelButtonsSize.y));
                            GUILayout.EndArea();

                            if (PaintHeight == true || PaintColor == true)
                            {
                                Rect TextureBrushRect = new Rect(TextureRectPosition, WheelButtonsSize);
                                GUILayout.BeginArea(TextureBrushRect);
                                GUI.backgroundColor = Color.grey;
                                if (SelectingTexture == true) GUI.backgroundColor = Color.white;
                                bool TextureBrush = GUILayout.Button("Randomize", GUILayout.MinWidth(WheelButtonsSize.x), GUILayout.MinHeight(WheelButtonsSize.y));
                                GUILayout.EndArea();
                            }
                            if (PaintColor == true)
                            {
                                Rect LightBrushRect = new Rect(LightRectPosition, WheelButtonsSize);
                                GUILayout.BeginArea(LightBrushRect);
                                GUI.backgroundColor = Color.grey;
                                if (SelectingLight == true) GUI.backgroundColor = Color.white;
                                bool LightBrush = GUILayout.Button("Light", GUILayout.MinWidth(WheelButtonsSize.x), GUILayout.MinHeight(WheelButtonsSize.y));
                                GUILayout.EndArea();
                            }
                            if (ShowingBrushTypeUI == true) GUI.enabled = false;

                            if (MouseLeftClickingTrigger == true) ChangingBrushLeftClicked = true;
                        }
                        if (ShowingBrushTypeUI == false)
                        {
                            UseNormalBrush = false;
                            UseSoftenBrush = false;
                            UseLightBrush = false;
                            UseTextureBrush = false;
                            if (SelectingNormal == true) UseNormalBrush = true;
                            if (SelectingSoften == true) UseSoftenBrush = true;
                            if (SelectingTexture == true) UseTextureBrush = true;
                            if (SelectingLight == true) UseLightBrush = true;
                            //if (BrushTypeChangeKeyDown == false) ChangingBrushLeftClicked = false;
                            //ChangeBrushTypeButtonWasPressed = false;
                        }
                        if (BrushTypeChangeKeyDown == true)
                        {
                            GUI.enabled = false;
                            Rect BrushTypeRect = new Rect(Vector2.zero, new Vector2(ScreenWidthAndHeight.x, ScreenWidthAndHeight.y));
                            GUILayout.BeginArea(BrushTypeRect);
                            GUI.backgroundColor = new Color(0, 0, 0, 0);
                            GUILayout.Box("", GUILayout.MinHeight(BrushTypeRect.height), GUILayout.MinWidth(BrushTypeRect.width));
                            if (BrushTypeRect.Contains(Event.current.mousePosition))
                            {
                                MouseOverUI = true;
                                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
                            }
                            GUI.enabled = true;
                            GUILayout.EndArea();
                        }
                    }
                }
                if (PaintShadows == false && PaintHeight == false && PaintColor == false)
                {
                    UseSoftenBrush = false;
                    UseLightBrush = false;
                    UseTextureBrush = false;
                    SelectingSoften = false;
                    SelectingTexture = false;
                    SelectingLight = false;
                }
            }

            if (CreatingGrassField == true)
            {
                Vector2 ScreenCenter = new Vector2(ScreenWidthAndHeight.x / 2.0f, ScreenWidthAndHeight.y / 2.0f);
                Rect LoadingBox = new Rect(ScreenCenter - (LoadingBoxSize / 2.0f), LoadingBoxSize);
                GUILayout.BeginArea(LoadingBox);
                Color OriginalBackgroundColor = GUI.backgroundColor;
                GUI.backgroundColor = new Color(0.0f, 0.0f, 0.0f, 2.0f);
                GUILayout.Box("", GUILayout.MinWidth(LoadingBox.width), GUILayout.MinHeight(LoadingBox.height));
                GUILayout.EndArea();
                Rect LoadingBoxText = new Rect(ScreenCenter - (LoadingBoxTextSize / 2.0f), LoadingBoxTextSize);
                GUILayout.BeginArea(LoadingBoxText);
                GUI.backgroundColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
                GUIStyle BoxStyle1 = new GUIStyle(GUI.skin.box);
                BoxStyle1.fontSize = 13;
                BoxStyle1.normal.textColor = Color.white;
                GUILayout.Box("Loading...", BoxStyle1);
                GUI.backgroundColor = OriginalBackgroundColor;
                GUILayout.EndArea();
            }

            //Rect TextureRect = new Rect(Vector2.zero, TestUIBoxSize);
            //GUILayout.BeginArea(TextureRect);
            //GUIStyle BoxStyle = new GUIStyle(GUI.skin.box);
            //BoxStyle.fixedWidth = TextureRect.width;
            //BoxStyle.fixedHeight = TextureRect.height ;
            //GUILayout.Box(GrassFieldMasters[GrassFieldList[0]].SVGMeshMaster.MainMatInstanced.GetTexture("_NormalSplatTexture"), BoxStyle);
            //GUILayout.EndArea();
            //if (SVGMC.ColorMasks.Count > 0)
            //{
            //    if (SVGMC.ColorMasksRenderTextures.ContainsKey(SVGMC.ColorMasks[0])) GUILayout.Box(SVGMC.ColorMasksRenderTextures[SVGMC.ColorMasks[0]], BoxStyle);
            //}

            Handles.EndGUI();
        }

        private void OnSceneCtrlZAndYCancel(Event _event)
        {
            if (LeftControlPressed == true)
            {
                if (_event.keyCode == KeyCode.Z && _event.type == EventType.KeyDown)
                {
                    if (UndoKey == KeyCode.Z && UndoAction == false)
                    {
                        UndoAction = true;
                        UndoEvent?.Invoke();
                    }
                    _event.type = EventType.Used;
                }
                if (_event.keyCode == KeyCode.Y && _event.type == EventType.KeyDown)
                {
                    if (RedoKey == KeyCode.Y && RedoAction == false)
                    {
                        RedoAction = true;
                        RedoEvent?.Invoke();
                    }
                    _event.type = EventType.Used;
                }
            }
            if (_event.keyCode == KeyCode.LeftControl)
            {
                if (_event.type == EventType.KeyDown) LeftControlPressedTrigger = true;
                if (_event.type == EventType.KeyUp) LeftControlPressedTrigger = false;
            }
        }

        private void HeightTextureGUIBrushSelection(Rect DrawingRectangle, bool HoverBool, Vector2 AddedPos, int SelectionInt)
        {
            Color GUIOriginalColor = GUI.color;
            Color GUIOriginalBackgroundColor = GUI.backgroundColor;
            Rect HeightBrushTexturePickerTexturesRect = new Rect(DrawingRectangle.position + HeightTexturePickerTexturesRectAddedPos + AddedPos, HeightTexturePickerTexturesRectSize);
            //Round
            Rect HeightBrushTexturePickerTexturesRect3 = new Rect(HeightBrushTexturePickerTexturesRect.position,
                HeightBrushTexturePickerTexturesRect.size);
            GUILayout.BeginArea(HeightBrushTexturePickerTexturesRect3);
            GUILayout.BeginHorizontal();
            GUIStyle HeightTexPickerBoxStyle3 = new GUIStyle(GUI.skin.box);
            HeightTexPickerBoxStyle3.fixedWidth = HeightBrushTexturePickerTexturesRect.width / 2.0f;
            HeightTexPickerBoxStyle3.fixedHeight = HeightBrushTexturePickerTexturesRect.height;
            Color OriginalBackgroundColorHeightPicker1 = GUI.backgroundColor;
            Color ColorApplied = Color.black;
            if (HoverBool == true) ColorApplied = Color.white * 5.0f;
            if (HeightTextureBrushSelected == SelectionInt) ColorApplied = HeightTexturePickerOutlineColor * 2.0f;
            GUI.backgroundColor = ColorApplied;
            GUI.color = ColorApplied;
            GUILayout.Box("", HeightTexPickerBoxStyle3);
            GUI.backgroundColor = GUIOriginalBackgroundColor;
            GUI.color = GUIOriginalColor;
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private Vector2 LoadingBoxSize = new Vector2(100.0f, 26.0f), LoadingBoxTextSize = new Vector2(66.666f, 23.33f), GONameAddedPosition = new Vector2(0.0f, -20.22f);

        private void CreateMaterialsAssets()
        {
            CreatedDirectoryIfNecessary();
            //Check if Folders are Valid 
            bool AssetValid = false, NewFolderValid = false;
            if (AssetDatabase.IsValidFolder(GrassAssetFolderPath) == false)
            {
                Debug.Log("GrassShader Asset Folder not found. Assign it in 'Data (Don't Touch)'");
            }
            else { AssetValid = true; }
            if (AssetDatabase.IsValidFolder(CreatedFilesDirectoryPath) == false || CreatedFilesDirectoryPath == null)
            {
                Debug.Log("New Folder Path not found");
            }
            else { NewFolderValid = true; }
            //If both folders are valid 
            if (AssetValid == true && NewFolderValid == true)
            {
                string NewFolderMaterialsPath = CreatedFilesDirectoryPath + "/_Materials";//UnderScore because it's added to the folder automatically
                if (AssetDatabase.IsValidFolder(NewFolderMaterialsPath) == false)
                {
                    AssetDatabase.CreateFolder(CreatedFilesDirectoryPath, "/Materials");
                }

                //Check the already created Textures 
                System.IO.DirectoryInfo Directory = new System.IO.DirectoryInfo(CreatedFilesDirectoryPath);
                int MainAddedInt = 1;
                int OpaqueAddedInt = 1;
                string[] FoldersToSearch = new string[] { NewFolderMaterialsPath };
                string[] AlreadyCreatedMaterials = AssetDatabase.FindAssets("t:Material", FoldersToSearch);
                //Get the highest number + 1
                for (int i = 0; i < AlreadyCreatedMaterials.Length; i++)
                {
                    string Path = AssetDatabase.GUIDToAssetPath(AlreadyCreatedMaterials[i]);
                    //If contains "Main"
                    if (Path.Contains(Directory.Name + "Main"))
                    {
                        int EndingNumbers = 0;
                        char[] SeperatorChar = new char[] { char.Parse("_"), char.Parse(".") };
                        string[] PathSplit = Path.Split(SeperatorChar);
                        int.TryParse(PathSplit[PathSplit.Length - 2], out EndingNumbers);
                        if (EndingNumbers >= MainAddedInt) { MainAddedInt = EndingNumbers + 1; }
                    }
                    //If contains "Opaque"
                    if (Path.Contains(Directory.Name + "Opaque"))
                    {
                        int EndingNumbers = 0;
                        char[] SeperatorChar = new char[] { char.Parse("_"), char.Parse(".") };
                        string[] PathSplit = Path.Split(SeperatorChar);
                        int.TryParse(PathSplit[PathSplit.Length - 2], out EndingNumbers);
                        if (EndingNumbers >= OpaqueAddedInt) { OpaqueAddedInt = EndingNumbers + 1; }
                    }
                }
                //Create Assets 
                string NewMainMaterialPathInt = NewFolderMaterialsPath + "/" + Directory.Name + "Main" + "_" + MainAddedInt + ".mat";
                string NewOpaqueMaterialPathInt = NewFolderMaterialsPath + "/" + Directory.Name + "Opaque" + "_" + OpaqueAddedInt + ".mat";
                Material MainMatToUse = MainOriginalMatBuiltIn;
                Material OpaqueMatToUse = OpaqueOriginalMatBuiltIn;
                if (UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline != null)
                {
                    if (UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline.name.Contains("HDRP") == true)
                    {
                        MainMatToUse = MainOriginalMatHDRP;
                        OpaqueMatToUse = OpaqueOriginalMatHDRP;
                    }
                    if (UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline.name.Contains("URP") == true)
                    {
                        MainMatToUse = MainOriginalMatURP;
                        OpaqueMatToUse = OpaqueOriginalMatURP;
                    }
                }
                AssetDatabase.CreateAsset(new Material(MainMatToUse), NewMainMaterialPathInt);
                AssetDatabase.CreateAsset(new Material(OpaqueMatToUse), NewOpaqueMaterialPathInt);
                //Assign Materials
                CreatedMainMat = AssetDatabase.LoadAssetAtPath<Material>(NewMainMaterialPathInt);
                CreatedOpaqueMat = AssetDatabase.LoadAssetAtPath<Material>(NewOpaqueMaterialPathInt);
                CreatedAssets.Add(CreatedMainMat);
                CreatedAssets.Add(CreatedOpaqueMat);
                EditorUtility.SetDirty(this);
            }
        }

        public void DeleteAssetsUI(SceneView scene)
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            Event _event = Event.current;
            Vector2 ScreenCenterPos = new Vector2(ScreenWidthAndHeight.x / 2.0f, ScreenWidthAndHeight.y / 2.0f);
            Rect ScreenCenterRect = new Rect(ScreenCenterPos - (DeletionBoxSize / 2.0f), DeletionBoxSize);
            GUILayout.BeginArea(ScreenCenterRect);
            Color OriginalBackgroundColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.0f, 0.0f, 0.0f, 2.0f);
            GUILayout.Box("", GUILayout.MinHeight(ScreenCenterRect.height), GUILayout.MinWidth(ScreenCenterRect.width));
            GUI.backgroundColor = OriginalBackgroundColor;
            GUILayout.EndArea();
            Rect ButtonRectUp = new Rect(ScreenCenterPos
                - new Vector2(0.0f, DeleteButtonsAddedDistance) - new Vector2(ButtonsDefaultSize.x / 2.0f, ButtonsDefaultSize.y / 2.0f), ButtonsDefaultSize);
            Rect ButtonRectCenter = new Rect(ScreenCenterPos
                - new Vector2(DeleteCenterButtonSize.x / 2.0f, DeleteCenterButtonSize.y / 2.0f), DeleteCenterButtonSize);
            Rect ButtonRectDown = new Rect(ScreenCenterPos
                + new Vector2(0.0f, DeleteButtonsAddedDistance) - new Vector2(ButtonsDefaultSize.x / 2.0f, ButtonsDefaultSize.y / 2.0f), ButtonsDefaultSize);
            Rect ButtonCenterRect = new Rect(ButtonRectCenter.position + DeleteButtonNoteAddedPosition, DeleteButtonNoteSize);
            Rect ButtonDownRect = new Rect(ButtonRectDown.position + DeleteButtonNoteAddedPosition, DeleteButtonNoteSize);
            GUIStyle ButtonStyle = new GUIStyle(GUI.skin.button);
            ButtonStyle.fixedHeight = ButtonsDefaultSize.y;
            ButtonStyle.fixedWidth = ButtonsDefaultSize.x;
            GUIStyle ButtonStyleCenter = new GUIStyle(GUI.skin.button);
            ButtonStyleCenter.fixedHeight = ButtonRectCenter.height;
            ButtonStyleCenter.fixedWidth = ButtonRectCenter.width;
            //ButtonUp
            GUILayout.BeginArea(ButtonRectUp);
            bool CancelButton = GUILayout.Button("Cancel", ButtonStyle);
            GUILayout.EndArea();
            //ButtonCenter
            GUILayout.BeginArea(ButtonRectCenter);
            bool DeleteAssetsButton = GUILayout.Button("Delete GameObject AND Created Assets", ButtonStyleCenter);
            GUILayout.EndArea();
            //ButtonDown
            GUILayout.BeginArea(ButtonRectDown);
            bool DeleteOnlyGameObject = GUILayout.Button("Delete GameObject Only", ButtonStyle);
            GUILayout.EndArea();
            //Note1
            GUILayout.BeginArea(ButtonCenterRect);
            GUIStyle DeleteUIBoxStyle1 = new GUIStyle(GUI.skin.box);
            Color BackgroundOriginalColor = GUI.backgroundColor;
            Color OriginalColor = GUI.color;
            GUI.backgroundColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
            GUI.color = Color.white;
            DeleteUIBoxStyle1.normal.textColor = Color.white;
            DeleteUIBoxStyle1.hover.textColor = Color.white;
            GUILayout.Box("(Can't Undo)", DeleteUIBoxStyle1, GUILayout.Width(ButtonRectCenter.width), GUILayout.Height(ButtonCenterRect.size.y));
            GUI.backgroundColor = BackgroundOriginalColor;
            GUI.color = OriginalColor;
            GUILayout.EndArea();
            //Note2
            GUILayout.BeginArea(ButtonDownRect);
            GUIStyle DeleteUIBoxStyle2 = new GUIStyle(GUI.skin.box);
            Color BackgroundOriginalColor2 = GUI.backgroundColor;
            Color OriginalColor2 = GUI.color;
            GUI.backgroundColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
            GUI.color = Color.white;
            DeleteUIBoxStyle2.normal.textColor = Color.white;
            DeleteUIBoxStyle2.hover.textColor = Color.white;
            GUILayout.Box("(Can't Undo)", DeleteUIBoxStyle2, GUILayout.Width(ButtonRectDown.width), GUILayout.Height(ButtonRectDown.size.y));
            GUI.backgroundColor = BackgroundOriginalColor2;
            GUI.color = OriginalColor2;
            GUILayout.EndArea();
            if (DeleteAssetsButton == true) DeleteGOAndAssetsTrigger = true;
            if (DeleteOnlyGameObject == true) DeleteGameObjectOnlyTrigger = true;
            if (CancelButton == true) CancelTrigger = true;
        }
        private float DeleteButtonsAddedDistance = 70.0f;
        private Vector2 DeleteCenterButtonSize = new Vector2(280.0f, 27.0f);

        private Vector2 DeletionBoxSize = new Vector2(700.0f, 300.0f), ButtonsDefaultSize = new Vector2(200.0f, 25.0f),
        DeleteButtonNoteAddedPosition = new Vector2(-7.2f, 25.0f), DeleteButtonNoteSize = new Vector2(215.0f, 50.0f), ChangePathUITitleAddedPosition = new Vector2(0.0f, -115), ChangePathUITitleSize = new Vector2(320.0f, 33.0f),
            ChangePathUIPathAddedPosition = new Vector2(0.0f, -50.0f), ChangePathUIPathSize = new Vector2(500.0f, 80.0f);
        private int ChangePathUITitleFontSize = 21, ChangePathUIPathFontSize = 15;
        private float ButtonsAddedDistance = 150.0f;
        private bool DeleteGOAndAssetsTrigger, DeleteGameObjectOnlyTrigger, CancelTrigger;


        public void CreatedDirectoryIfNecessary()
        {
            bool NewFolderValid = false;
            if (AssetDatabase.IsValidFolder(CreatedFilesPath) == false || CreatedFilesPath == null)
            {
                Debug.Log("New Folder Path not found");
            }
            else { NewFolderValid = true; }
            if (NewFolderValid == true)
            {
                bool CreatedNewFolder = false;
                if (CreatedFilesDirectoryPath != null && CreatedFilesDirectoryPath != "")
                {
                    if (AssetDatabase.IsValidFolder(CreatedFilesDirectoryPath) == false) CreatedNewFolder = true;
                }
                if (CreatedFilesDirectoryPath == null || CreatedFilesDirectoryPath == "") CreatedNewFolder = true;
                if (CreatedNewFolder == true)
                {
                    //get highest number... 
                    string objectFoundPath = null;
                    int HighestNumberFound = _assetsOrganizationTools.GetHighestNumberInFolder(CreatedFilesPath, "VibrantGrassShaderCreatedFiles", "_", out objectFoundPath, false);
                    string CreatedFilesDirectoryName = "VibrantGrassShaderCreatedFiles" + "_" + (HighestNumberFound + 1).ToString();
                    AssetDatabase.CreateFolder(CreatedFilesPath, "/" + CreatedFilesDirectoryName);
                    CreatedFilesDirectoryPath = CreatedFilesPath + "/_" + CreatedFilesDirectoryName;//Underscore because it's created automatically 
                    CreatedFilesDirectory = AssetDatabase.LoadAssetAtPath(CreatedFilesDirectoryPath, typeof(Object));
                    CreatedAssets.Add(CreatedFilesDirectory);
                    EditorUtility.SetDirty(this);
                }
            }
        }


        private Vector2 MousePosBeforeBrushType, CenterOfSceneScreen;
        private bool SelectingNormal, SelectingSoften, SelectingTexture, SelectingLight, ChangingBrushLeftClicked;

        private void AssignDefaultPaths()
        {
            SerializedObject serializedObject = new SerializedObject(GetComponent<MainControls>());
            string GrassAssetFilesPath = AssetDatabase.GetAssetPath(AssetDataFolder);
            GrassAssetFolderPath = null;
            GrassAssetFolderPath = GrassAssetFilesPath;//Reset so it detects the change
            SerializedProperty serializedProperty2 = serializedObject.FindProperty("GrassAssetFolderPath");
            serializedProperty2.stringValue = GrassAssetFilesPath;

            string CreatedFilesPathResult = null;
            if (CreatedFilesPath == "" || CreatedFilesPath == null) CreatedFilesPathResult = AssetDatabase.GetAssetPath(DefaultCreatedGrassFilesFolder);
            if (CreatedFilesPath != "" && CreatedFilesPath != null) CreatedFilesPathResult = CreatedFilesPath;
            //Created Files Path 
            CreatedFilesPath = null;
            CreatedFilesPath = CreatedFilesPathResult;//Reset so it detects the change
            CreatedFilesPathOld = CreatedFilesPath;
            SerializedProperty serializedProperty3 = serializedObject.FindProperty("createdFilesPath");
            serializedProperty3.stringValue = CreatedFilesPathResult;
            serializedObject.ApplyModifiedProperties();
        }

        private void WhenSavingAsset()
        {
            CopyMaterialControlValuesOntoOtherMaterial(CreatedMainMat, ref createdOpaqueMat);
        }
#endif
    }

#if UNITY_EDITOR
    public class MainControlsFileModification : AssetModificationProcessor
    {
        public delegate void ModifDeleg();
        static public ModifDeleg OnWillSaveAssetEvent;
        static string[] OnWillSaveAssets(string[] paths)
        {
            OnWillSaveAssetEvent?.Invoke();
            return paths;
        }
    }
#endif
}