using System.Collections.Generic;
using UnityEngine;
using VibrantGrassShaderTools;
using UnityEditor;
using static VibrantGrassShader.MainControls;
using static UnityEngine.Rendering.DebugUI;

namespace VibrantGrassShader
{
    [ExecuteInEditMode]
    public class GrassFieldMaster : MonoBehaviour
    {
#if UNITY_EDITOR
        [Foldout("Controls", true)]
        [SerializeField] private bool ResetAndWrap = false;
        [SerializeField] public bool CreateAndWrapMeshToHideFlattenedGrass = false, DeleteThisGrassField = false;
        [Foldout("Other", true)]
        [SerializeField] public bool OverwriteDefaultRootsHeightsPrecision = false;
        [SerializeField] public VGSRootsHeightsTextureSizeEnum RootsHeightsPrecision = VGSRootsHeightsTextureSizeEnum._1024;
        private VGSRootsHeightsTextureSizeEnum RootsHeightsTextureSizeOld = VGSRootsHeightsTextureSizeEnum._1024;

#endif
        [SerializeField] public bool OverwriteFlattenedGrass_HeightRemoved = false;
        [SerializeField] public float FlattenedGrass_HeightRemoved = 0.005f;

        [Foldout("Created Assets")]
        [SerializeField] public List<Object> CreatedAssets = new List<Object>();
        [Foldout("Data (Don't Touch)", true)]
        [SerializeField] public List<Object> ObjectsToDeleteOnStart = new List<Object>();
        [SerializeField] public GameObject ColliderForSplatMap = null, GrassMeshObject = null;

        [SerializeField] public GameObject MainControlObject;
        [SerializeField, HideInInspector] public Texture2D heightTexture = null, shadowTexture = null, colorTexture = null, groundNormalTexture = null, noMeshRootsHeightsTexture = null;
        [SerializeField] public GrassMeshMaster SVGMeshMaster;
        [SerializeField, HideInInspector] private Vector3[] CircularDirections = new Vector3[16];
#if UNITY_EDITOR
        [SerializeField] public GameObject TexPaintCollider = null;
        [SerializeField] private Object OriginalShadowTexture = null, OriginalHeightTexture = null, OriginalColorTexture = null, OriginalGroundNormalTexture = null, OriginalNoMeshRootsHeightsTexture = null;
        [SerializeField] public GrassMeshShadowsMaster SVGMShadowsMaster;
        [SerializeField] public GrassMeshHeightMaster SVGMHeightMaster;
        [SerializeField] public MeshColorTextureMaster SVGMColorTextureMaster;
        [SerializeField, HideInInspector] public GrassWrap GrassMeshWrapScript = null;
        [SerializeField] public bool ClearCollections = false;
#endif
        [SerializeField, HideInInspector]
        public Vector3[] GrassFieldPositionsGrid = new Vector3[0], GrassFieldGroundPositionsGrid = new Vector3[0],
            GroundAverageNormals = new Vector3[0];
        [SerializeField, HideInInspector] public Vector2[] GrassFieldNormalUVs = new Vector2[0];
        [HideInInspector] public MainControls VGSMC;
        private OutOfSightDisabler VGSDistanceFadeOutMaster;

        public Texture2D ShadowTexture
        {
            get { return shadowTexture; }
#if UNITY_EDITOR
            set
            {
                GrassFieldMaster grassShaderMaster2 = GetComponent<GrassFieldMaster>();
                SerializedObject serializedObject = new SerializedObject(grassShaderMaster2);
                shadowTexture = null;
                shadowTexture = value;//Reset so it detects the change
                SerializedProperty serializedProperty = serializedObject.FindProperty("shadowTexture");
                serializedProperty.objectReferenceValue = value;
                serializedObject.ApplyModifiedProperties();
            }
#endif
        }
        public Texture2D HeightTexture
        {
            get { return heightTexture; }
#if UNITY_EDITOR
            set
            {
                GrassFieldMaster grassShaderMaster2 = GetComponent<GrassFieldMaster>();
                SerializedObject serializedObject = new SerializedObject(grassShaderMaster2);
                heightTexture = null;
                heightTexture = value;//Reset so it detects the change 
                SerializedProperty serializedProperty = serializedObject.FindProperty("heightTexture");
                serializedProperty.objectReferenceValue = value;
                serializedObject.ApplyModifiedProperties();
            }
#endif
        }
        public Texture2D ColorTexture
        {
            get { return colorTexture; }
#if UNITY_EDITOR
            set
            {
                GrassFieldMaster grassShaderMaster2 = GetComponent<GrassFieldMaster>();
                SerializedObject serializedObject = new SerializedObject(grassShaderMaster2);
                colorTexture = null;
                colorTexture = value;//Reset so it detects the change 
                SerializedProperty serializedProperty = serializedObject.FindProperty("colorTexture");
                serializedProperty.objectReferenceValue = value;
                serializedObject.ApplyModifiedProperties();
            }
#endif
        }
        public Texture2D GroundNormalTexture
        {
            get { return groundNormalTexture; }
#if UNITY_EDITOR
            set
            {
                GrassFieldMaster grassShaderMaster2 = GetComponent<GrassFieldMaster>();
                SerializedObject serializedObject = new SerializedObject(grassShaderMaster2);
                groundNormalTexture = null;
                groundNormalTexture = value;//Reset so it detects the change 
                SerializedProperty serializedProperty = serializedObject.FindProperty("groundNormalTexture");
                serializedProperty.objectReferenceValue = value;
                serializedObject.ApplyModifiedProperties();
            }
#endif
        }
        public Texture2D NoMeshRootsHeightsTexture
        {
            get { return noMeshRootsHeightsTexture; }
#if UNITY_EDITOR
            set
            {
                GrassFieldMaster grassShaderMaster2 = GetComponent<GrassFieldMaster>();
                SerializedObject serializedObject = new SerializedObject(grassShaderMaster2);
                noMeshRootsHeightsTexture = null;
                noMeshRootsHeightsTexture = value;//Reset so it detects the change 
                SerializedProperty serializedProperty = serializedObject.FindProperty("noMeshRootsHeightsTexture");
                serializedProperty.objectReferenceValue = value;
                serializedObject.ApplyModifiedProperties();
            }
#endif
        }
        //[SerializeField, HideInInspector] public Vector3[] FurthestVerticesLocalPositions = new Vector3[0];
#if UNITY_EDITOR
        [HideInInspector] public bool CreateAndAssignTextures = false;

        //Performance
        [SerializeField] public bool GrassCloseTransparency = false;
        [SerializeField] private MeshRenderer GrassMeshRenderer;

        public delegate void GrassShaderMasterDelegate();
        //Undo Redo Events 
        public GrassShaderMasterDelegate UndoDrawingEvent, RedoDrawingEvent, SaveShadowTextureEvent, SaveHeightTextureEvent, SaveColorTextureEvent,
            StartCuttingEvent, WrapEvent, WrapCutMeshes, DeleteShadowTexEvent, DeleteHeightTexEvent, DeleteColorTexEvent;
        private void UndoShadowsMethod() { UndoDrawingEvent?.Invoke(); }
        private void RedoShadowsMethod() { RedoDrawingEvent?.Invoke(); }
        private bool CheckTextureSaving(ref bool TexIsEmpty)
        {
            Texture texToSave = null;
            if (WasPainting == WasPaintingTypes.Shadows) texToSave = GrassMeshRenderer.sharedMaterial.GetTexture("_shadowTexture");
            if (WasPainting == WasPaintingTypes.Height) texToSave = GrassMeshRenderer.sharedMaterial.GetTexture("_heightTexture");
            if (WasPainting == WasPaintingTypes.Color) texToSave = GrassMeshRenderer.sharedMaterial.GetTexture("_colorTexture");
            bool ConfirmSaving = false;
            if (texToSave == null) ConfirmSaving = false;
            else
            {
                RenderTexture RTToCompare = new RenderTexture(texToSave.width, texToSave.height, 0, SVGMCTextureDrawing.currentRTFormat);
                Graphics.Blit(texToSave, RTToCompare);
                ConfirmSaving = CompareTexturesAndCheckIfBlack(SaveCheckRT, RTToCompare, ref TexIsEmpty);
                DestroyImmediate(RTToCompare);
            }
            return ConfirmSaving;
            //Not working with the Colors because the textures are not always of the same size. 
        }
        private void SaveShadowTextureMethod()
        {
            bool IsEmpty = false;
            bool ConfirmSaving = CheckTextureSaving(ref IsEmpty);
            if (ConfirmSaving == true) SaveShadowTextureEvent?.Invoke();
            Graphics.Blit(GrassMeshRenderer.sharedMaterial.GetTexture("_shadowTexture"), SaveCheckRT);
        }
        private void SaveHeightTextureMethod()
        {
            bool IsEmpty = false;
            bool ConfirmSaving = CheckTextureSaving(ref IsEmpty);
            if (ConfirmSaving == true) SaveHeightTextureEvent?.Invoke();
            Graphics.Blit(GrassMeshRenderer.sharedMaterial.GetTexture("_heightTexture"), SaveCheckRT);
        }
        private void SaveColorTextureMethod()
        {
            bool TexIsEmpty = false;
            bool ConfirmSaving = CheckTextureSaving(ref TexIsEmpty);
            if (ConfirmSaving == true && TexIsEmpty == false) SaveColorTextureEvent?.Invoke();
            string TexAssetPath = AssetDatabase.GetAssetPath(ColorTexture);
            if (TexIsEmpty == true && TexAssetPath != null & TexAssetPath != "") AssetDatabase.DeleteAsset(TexAssetPath);
            else Graphics.Blit(GrassMeshRenderer.sharedMaterial.GetTexture("_colorTexture"), SaveCheckRT);
        }
        private void DeleteShadowTexMethod() { DeleteShadowTexEvent?.Invoke(); }
        private void DeleteHeightTexMethod() { DeleteHeightTexEvent?.Invoke(); }
        private void DeleteColorTexMethod() { DeleteColorTexEvent?.Invoke(); }
        private void StartCuttingMethod() { StartCuttingEvent?.Invoke(); }
        public void WrapMethod()
        {
            CreateNormalTexture = true;
            CreateAndAssignTextures = true;
            WrapEvent?.Invoke();
            CreateNormalTexture = false;
            CreateAndAssignTextures = false;
        }
        private void WrapCutMeshesMethod()
        {
            if (GrassMeshRenderer.sharedMaterial.GetTexture("_heightTexture") != null)
            {
                if (GrassMeshWrapScript.CreatedMesh != null)
                { if (GrassMeshWrapScript.CreatedMesh.triangles.Length < VGSMC.CurrentAppliedMeshOriginal.triangles.Length) WrapCutMeshes?.Invoke(); }
            }
        }

        private bool PaintingTrigger;
        private int ColorFrameCount1;
        private enum WasPaintingTypes { Shadows, Height, Color }
        private WasPaintingTypes WasPainting;
        private void SetAndPaintSaveCheckRTWhenPaintingStarted()
        {
            Texture texToSave = null;
            if (VGSMC.PaintShadows == true) texToSave = GrassMeshRenderer.sharedMaterial.GetTexture("_shadowTexture");
            if (VGSMC.PaintHeight == true) texToSave = GrassMeshRenderer.sharedMaterial.GetTexture("_heightTexture");
            if (VGSMC.PaintColor == true) texToSave = GrassMeshRenderer.sharedMaterial.GetTexture("_colorTexture");
            if (PaintingTrigger == false && texToSave != null)
            {
                if (VGSMC.PaintColor == true)
                {
                    if (ColorFrameCount1 > 5) PaintingTrigger = true;
                    ColorFrameCount1 += 1;
                }
                else PaintingTrigger = true;
                if (PaintingTrigger == true)
                {
                    SaveCheckRT = new RenderTexture(texToSave.width, texToSave.height, 0, SVGMCTextureDrawing.currentRTFormat);
                    Graphics.Blit(texToSave, SaveCheckRT);
                }
            }
            if (VGSMC.PaintShadows == false && VGSMC.PaintHeight == false && VGSMC.PaintColor == false)
            {
                if (PaintingTrigger == true)
                {
                    RenderTexture.active = null;
                    DestroyImmediate(SaveCheckRT);
                    PaintingTrigger = false;
                }
                PaintingTrigger = false;
                ColorFrameCount1 = 0;
            }
        }

        private string OldParentName;
        private void RenameUsingParentName()
        {
            if (transform.parent.gameObject.name != OldParentName)
            {
                int EndingNumbers = 0;
                string currentName = gameObject.name;
                char[] SeperatorChar = new char[] { char.Parse("_") };
                string[] PathSplit = currentName.Split(SeperatorChar);
                int.TryParse(PathSplit[PathSplit.Length - 1], out EndingNumbers);

                gameObject.name = transform.parent.gameObject.name + "GrassField" + "_" + EndingNumbers.ToString();
            }
            OldParentName = transform.parent.gameObject.name;
        }

        private RenderTexture SaveCheckRT;
        private bool CompareTexturesAndCheckIfBlack(RenderTexture RT1, RenderTexture RT2, ref bool IsBlack)
        {
            int QualDivider = 10;
            Vector2Int RT1Width = new Vector2Int(RT1.width / QualDivider, RT1.height / QualDivider);

            Texture2D tex1 = new Texture2D(RT1Width.x, RT1Width.y, TextureFormat.RGBAHalf, false);
            RenderTexture.active = RT1;
            RenderTexture tempRT1 = RenderTexture.GetTemporary(RT1Width.x, RT1Width.y, 0, RT1.format);
            Graphics.Blit(RT1, tempRT1);
            RenderTexture.active = tempRT1;
            tex1.ReadPixels(new Rect(0, 0, RT1Width.x, RT1Width.y), 0, 0);
            tex1.Apply();
            Color[] colors1 = tex1.GetPixels();
            DestroyImmediate(tex1);

            Vector2Int RT2Width = new Vector2Int(RT2.width / QualDivider, RT2.height / QualDivider);
            Texture2D tex2 = new Texture2D(RT2Width.x, RT2Width.y, TextureFormat.RGBAHalf, false);
            RenderTexture.active = RT2;
            RenderTexture tempRT2 = RenderTexture.GetTemporary(RT2Width.x, RT2Width.y, 0, RT2.format);
            Graphics.Blit(RT2, tempRT2);
            RenderTexture.active = tempRT2;
            tex2.ReadPixels(new Rect(0, 0, RT2Width.x, RT2Width.y), 0, 0);
            tex2.Apply();
            Color[] colors2 = tex2.GetPixels();
            DestroyImmediate(tex2);
            bool ColorsAreDifferent = false;
            //IsBlack = true;
            IsBlack = false;
            for (int i = 0; i < colors1.Length; i++)
            {
                if (colors1[i].r != colors2[i].r || colors1[i].g != colors2[i].g || colors1[i].b != colors2[i].b)
                {
                    ColorsAreDifferent = true;
                    break;
                }
                //Couldn't get this to work properly
                //if (colors2[i].r != 0.0f && colors2[i].g != 0.0f && colors2[i].b != 0.0f && colors2[i].a != 0.0f) IsBlack = false;
            }
            colors1 = null;
            colors2 = null;
            return ColorsAreDifferent;
        }

        private bool DrawHeightTrigger, DrawShadowTrigger, DrawColorTrigger, TextureTrigger1;

        [HideInInspector, SerializeField] private bool grassFieldJustSpawned;
        public bool GrassFieldJustSpawned
        {
            get { return grassFieldJustSpawned; }
            set
            {
                GrassFieldMaster grassShaderMaster2 = GetComponent<GrassFieldMaster>();
                SerializedObject serializedObject = new SerializedObject(grassShaderMaster2);
                grassFieldJustSpawned = false;
                grassFieldJustSpawned = true;
                grassFieldJustSpawned = value;//Reset so it detects the change
                SerializedProperty serializedProperty = serializedObject.FindProperty("grassFieldJustSpawned");
                serializedProperty.boolValue = grassFieldJustSpawned;
                serializedObject.ApplyModifiedProperties();
            }
        }
        [SerializeField, HideInInspector] private GrassTextureDrawing SVGMCTextureDrawing;
        private bool GroundNormalTextureNotCreatedYet;
        private void OnEnable()
        {
            if (transform.parent != null)
            {
                MainControlObject = transform.parent.gameObject;
                if (VGSMC == null) VGSMC = GetComponentInParent<MainControls>();
                if (VGSDistanceFadeOutMaster == null) VGSDistanceFadeOutMaster = GetComponentInParent<OutOfSightDisabler>();
            }
            if (VGSMC == null) VGSMC = MainControlObject.GetComponent<MainControls>();
            if (Application.isPlaying == false)
            {
                GrassMeshShadowsMaster ShadowsMaster = GrassMeshObject.GetComponent<GrassMeshShadowsMaster>();
                MeshColorTextureMaster ColorTextureMaster = GrassMeshObject.GetComponent<MeshColorTextureMaster>();
                if (ObjectsToDeleteOnStart.Contains(ShadowsMaster) == false) ObjectsToDeleteOnStart.Add(ShadowsMaster);
                if (ObjectsToDeleteOnStart.Contains(ColorTextureMaster) == false) ObjectsToDeleteOnStart.Add(ColorTextureMaster);
                EditorUtility.SetDirty(this);
                //if (SVGAssetDestroyerValues == null) SVGAssetDestroyerValues = new ShiniesVibrantGrassAssetDestroyerValues();
                EditorApplication.update += EditorUpdates;
                //if (SVGAssetDestroyerValues.CreatedAssets.Count == 0)
                if (CreatedAssets.Count == 0)
                {
                    //SVGAssetDestroyerValues.CreatedAssets = new List<Object>();
                    CreatedAssets = new List<Object>();
                    EditorUtility.SetDirty(this);
                    //EditorUtility.SetDirty(SVGAssetDestroyerValues);
                }
                //SpawnDestroyerObjectIfNecessary();
                //AssignAssetsToDestroy(SVGMC.AssetDestroyerGO);
                //AssetsCreatedCountLastFrame = SVGAssetDestroyerValues.CreatedAssets.Count;
                SVGMCTextureDrawing = GetComponentInParent<GrassTextureDrawing>();
                GrassMeshRenderer = GrassMeshObject.GetComponent<MeshRenderer>();
                GrassMeshWrapScript = GrassMeshObject.GetComponent<GrassWrap>();
            }
            CreateAndAssignTextures = false;
            ResetAndWrap = false;
            CreateNormalTexture = false;
            CreateHeightTexture = false;
            CreateNoMeshRootsHeightTexture = false;
            FrameCountUpdate = 0;
            EventsAddedToMainControls = false;
            DrawTrigger1 = false;
            PaintingTrigger = false;
            ColorFrameCount1 = 0;
            OldLocalPosition = transform.localPosition;
            OldLocalEulerAngles = transform.localEulerAngles;
            OldLocalScale = transform.localScale;
            CircularDirectionsOldLength = CircularDirections.Length;
            GotFurthestVerticesAfterWrapping = false;
            GroundNormalTextureNotCreatedYet = false;
            if (GroundNormalTexture == null) GroundNormalTextureNotCreatedYet = true;
            if (OverwriteDefaultRootsHeightsPrecision == true) RootsHeightsTextureSizeOld = RootsHeightsPrecision;
            else RootsHeightsTextureSizeOld = VGSMC.RootsHeightsPrecision;
            CreateAndWrapMeshToHideFlattenedGrass = false;
            DeleteThisGrassField = false;
            ResetAndWrap = false;
        }

        private void Start()
        {
            if (Application.isPlaying == true)
            {
                for (int i = 0; i < ObjectsToDeleteOnStart.Count; i++)
                { if (ObjectsToDeleteOnStart[i] != null) Destroy(ObjectsToDeleteOnStart[i]); }
            }
        }

        private bool EventsAddedToMainControls, DrawTrigger1, ShowDeleteAssetsUI;
        private int FrameCountUpdate;

        private int ClearCollectionFrameCount;
        private void EditorUpdates()
        {
            if (Application.isPlaying == false && this != null)
            {
                if (ClearCollections == true)
                {
                    if (ClearCollectionFrameCount == 0) GrassFieldPositionsGrid = null;
                    if (ClearCollectionFrameCount == 1) GrassFieldNormalUVs = null;
                    if (ClearCollectionFrameCount == 2) GroundAverageNormals = null;
                    if (ClearCollectionFrameCount == 3)
                    {
                        GrassFieldGroundPositionsGrid = null;
                        EditorUtility.SetDirty(this);
                        ClearCollections = false;
                    }
                    ClearCollectionFrameCount += 1;
                }
                else ClearCollectionFrameCount = 0;
                if (CreateAndWrapMeshToHideFlattenedGrass == true)
                {
                    StartCuttingEvent?.Invoke();
                    //Automatically overwrite FlattenedGrassHeightRemoved and make it 0.0f
                    SerializedObject serializedObject = new SerializedObject(this);
                    FlattenedGrass_HeightRemoved = 0.0f;
                    FlattenedGrass_HeightRemoved = 1.0f;
                    OverwriteFlattenedGrass_HeightRemoved = true;
                    OverwriteFlattenedGrass_HeightRemoved = false;
                    SerializedProperty serializedProperty = serializedObject.FindProperty("OverwriteFlattenedGrass_HeightRemoved");
                    SerializedProperty serializedProperty2 = serializedObject.FindProperty("FlattenedGrass_HeightRemoved");
                    serializedProperty.boolValue = true;
                    serializedProperty2.floatValue = 0.0f;
                    serializedObject.ApplyModifiedProperties();
                    CreateAndWrapMeshToHideFlattenedGrass = false;
                }
                if (RootsHeightsPrecision != RootsHeightsTextureSizeOld && OverwriteDefaultRootsHeightsPrecision == true) ResetAndWrap = true;
                if (OverwriteDefaultRootsHeightsPrecision == true) RootsHeightsTextureSizeOld = RootsHeightsPrecision;
                else RootsHeightsTextureSizeOld = VGSMC.RootsHeightsPrecision;

                List<Object> ObjectsToRemoveFromList = new List<Object>();
                for (int i = 0; i < CreatedAssets.Count; i++) { if (CreatedAssets[i] == null) ObjectsToRemoveFromList.Add(CreatedAssets[i]); }
                for (int i = 0; i < ObjectsToRemoveFromList.Count; i++) { CreatedAssets.Remove(ObjectsToRemoveFromList[i]); }

                if (GroundNormalTextureNotCreatedYet == true && GroundNormalTexture != null
                   && VGSMC.InitializationFinished == true)
                {
                    VGSMC.CreatingGrassField = false;
                    GroundNormalTextureNotCreatedYet = false;
                }
                //if (SVGAssetDestroyerValues.CreatedAssets.Count != AssetsCreatedCountLastFrame)
                //{
                //    AssignAssetsToDestroy(SVGMC.assetDestroyer);
                //}
                //AssetsCreatedCountLastFrame = SVGAssetDestroyerValues.CreatedAssets.Count;
                if (ResetAndWrap == true)
                {
                    WrapMethod();
                    ResetAndWrap = false;
                }
                if (FrameCountUpdate == 0) AddOtherEventsToMainControlObject();
                FrameCountUpdate += 1;
                RenameUsingParentName();
                LockTransform();
                SetAndPaintSaveCheckRTWhenPaintingStarted();
                TexturePainting();
                if (VGSMC.PaintShadows == true || VGSMC.PaintHeight == true || VGSMC.PaintColor == true)
                {
                    if (DrawTrigger1 == false)
                    {
                        AddPaintingEventsToMainControlObject();
                        EventsAddedToMainControls = true;
                        DrawTrigger1 = true;
                    }
                    if (VGSMC.PaintShadows) WasPainting = WasPaintingTypes.Shadows;
                    if (VGSMC.PaintHeight) WasPainting = WasPaintingTypes.Height;
                    if (VGSMC.PaintColor) WasPainting = WasPaintingTypes.Color;
                }
                else
                {
                    if (DrawTrigger1 == true)
                    {
                        if (EventsAddedToMainControls == true) RemovePaintingEventsFromMainControlObject();
                        DrawTrigger1 = false;
                    }
                }
                //if (MainControlObject != null && MainControlObjectOtherMethodsAdded == false)
                //{
                //    AddOtherEventsToMainControlObject();
                //    MainControlObjectOtherMethodsAdded = true;
                //}
                if (CircularDirections.Length != CircularDirectionsOldLength)
                {
                    AssignCircularDirections();
                }
                CircularDirectionsOldLength = CircularDirections.Length;
                GetFurthestVerticesAfterWrapping();

            }
        }
        [SerializeField, HideInInspector] private bool GotFurthestVerticesAfterWrapping = false;
        private void GetFurthestVerticesAfterWrapping()
        {
            if (GrassMeshWrapScript.Wrapping == true)
            {
                GotFurthestVerticesAfterWrapping = true;
            }
            else
            {
                if (GotFurthestVerticesAfterWrapping == true)
                {
                    GetFurthestVerticesInDirection();
                    GotFurthestVerticesAfterWrapping = false;
                }
            }
        }
        [SerializeField, HideInInspector] private int CircularDirectionsOldLength;
        private void AssignCircularDirections()
        {
            for (int i = 0; i < CircularDirections.Length; i++)
            {
                Vector3 direction = GeneratedDirections.GetDiskDirectionWithDegrees(i * (360.0f / (CircularDirections.Length)));
                CircularDirections[i] = direction;
            }
        }
        public void GetFurthestVerticesInDirection()
        {
            List<Vector3> CreatedMeshVerticesRendered = new List<Vector3>();
            Mesh meshToUse = VGSMC.CurrentAppliedMeshOriginal;
            if (GrassMeshWrapScript.CreatedMesh != null) meshToUse = GrassMeshWrapScript.CreatedMesh;
            Vector3[] CreatedMeshVertices = meshToUse.vertices;
            if (VGSDistanceFadeOutMaster.DistFadeOutValuesDict.ContainsKey(gameObject) == false) VGSDistanceFadeOutMaster.DistFadeOutValuesDict.Add(gameObject, new VibrantGrassShaderDistanceFadeOutValues());
            VGSDistanceFadeOutMaster.DistFadeOutValuesDict[gameObject].FurthestVerticesLocalPositions = new Vector3[CircularDirections.Length];
            for (int i = 0; i < CircularDirections.Length; i++)
            {
                Vector3 CurrentDirection = CircularDirections[i];
                Vector3 FurthestVertexPosition = Vector3.zero;
                float HighestDistance = 0.0f;
                for (int j = 0; j < CreatedMeshVertices.Length; j++)
                {
                    float DotResult = Vector3.Dot(CreatedMeshVertices[j], CurrentDirection);
                    float AngleToDirection = Vector2.Angle(new Vector2(CreatedMeshVertices[j].x, CreatedMeshVertices[j].z), new Vector2(CurrentDirection.x, CurrentDirection.z));
                    if (DotResult > HighestDistance && AngleToDirection <= 2.0f)
                    {
                        FurthestVertexPosition = CreatedMeshVertices[j];
                        HighestDistance = DotResult;
                    }
                }
                VGSDistanceFadeOutMaster.DistFadeOutValuesDict[gameObject].FurthestVerticesLocalPositions[i] = FurthestVertexPosition;
            }
            EditorUtility.SetDirty(this);
            EditorUtility.SetDirty(VGSDistanceFadeOutMaster);
        }

        [SerializeField, HideInInspector] public Vector3 OldLocalPosition = Vector3.zero, OldLocalEulerAngles = Vector3.zero, OldLocalScale = Vector3.one;
        private void LockTransform()
        {
            if (transform.localPosition != OldLocalPosition || transform.localEulerAngles != OldLocalEulerAngles || transform.localScale != OldLocalScale)
            { Debug.Log("You can't change the GrassFields transform"); }
            if (transform.localPosition != OldLocalPosition) transform.localPosition = OldLocalPosition;
            if (transform.localEulerAngles != OldLocalEulerAngles) transform.localEulerAngles = OldLocalEulerAngles;
            if (transform.localScale != OldLocalScale) transform.localScale = OldLocalScale;
            OldLocalPosition = transform.localPosition;
            OldLocalEulerAngles = transform.localEulerAngles;
            OldLocalScale = transform.localScale;
            //Add Debug
        }

        private void OnValidate()
        {
            CreateAndAssignTextures = false;
        }

        private bool WasPaintingShadows, WasPaintingHeight, WasPaintingColor;
        private void AddPaintingEventsToMainControlObject()
        {
            MainControls SVGMainControls = MainControlObject.GetComponent<MainControls>();
            SVGMainControls.UndoEvent += UndoShadowsMethod;
            SVGMainControls.RedoEvent += RedoShadowsMethod;
            SVGMainControls.DisablingMainControlObjectEditMode += RemovePaintingEventsFromMainControlObject;
            //SVGMainControls.EraseTextureEvent += EraseTextureMethod;
            WasPaintingShadows = false;
            WasPaintingHeight = false;
            WasPaintingColor = false;
            if (VGSMC.PaintShadows == true)
            {
                SVGMainControls.SaveCurrentTextureEvent += SaveShadowTextureMethod;
                SVGMainControls.DeleteCurrentTextureAssetEvent += DeleteShadowTexMethod;
                WasPaintingShadows = true;
            }
            if (VGSMC.PaintHeight == true)
            {
                SVGMainControls.SaveCurrentTextureEvent += SaveHeightTextureMethod;
                SVGMainControls.DeleteCurrentTextureAssetEvent += DeleteHeightTexMethod;
                WasPaintingHeight = true;
            }
            if (VGSMC.PaintColor == true)
            {
                SVGMainControls.SaveCurrentTextureEvent += SaveColorTextureMethod;
                SVGMainControls.DeleteCurrentTextureAssetEvent += DeleteColorTexMethod;
                WasPaintingColor = true;
            }
        }

        private void RemovePaintingEventsFromMainControlObject()
        {
            MainControls SVGMainControls = MainControlObject.GetComponent<MainControls>();
            SVGMainControls.UndoEvent -= UndoShadowsMethod;
            SVGMainControls.RedoEvent -= RedoShadowsMethod;
            SVGMainControls.DisablingMainControlObjectEditMode -= RemovePaintingEventsFromMainControlObject;
            //SVGMainControls.EraseTextureEvent -= EraseTextureMethod;
            if (WasPaintingShadows == true)
            {
                SVGMainControls.SaveCurrentTextureEvent -= SaveShadowTextureMethod;
                SVGMainControls.DeleteCurrentTextureAssetEvent -= DeleteShadowTexMethod;
            }
            if (WasPaintingHeight == true)
            {
                SVGMainControls.SaveCurrentTextureEvent -= SaveHeightTextureMethod;
                SVGMainControls.DeleteCurrentTextureAssetEvent -= DeleteHeightTexMethod;
            }
            if (WasPaintingColor == true)
            {
                SVGMainControls.SaveCurrentTextureEvent -= SaveColorTextureMethod;
                SVGMainControls.DeleteCurrentTextureAssetEvent -= DeleteColorTexMethod;
            }
        }

        private void AddOtherEventsToMainControlObject()
        {
            if (MainControlObject != null)
            {
                if (VGSMC == null) VGSMC = MainControlObject.GetComponent<MainControls>();
                VGSMC.CutUnusedMeshEvent += StartCuttingMethod;
                VGSMC.WrapEvent += WrapMethod;
                //WrapEvent += CreateMaterialsAndTextureInProject;//To create the GroundNormalTexture 
                VGSMC.WrapCutMeshesEvent += WrapCutMeshesMethod;
                VGSMC.SaveCurrentTextureEvent += CreateCurrentPaintingTextureAsset;
            }
        }

        private void RemoveOtherEventsToMainControlObject()
        {
            if (VGSMC == null) VGSMC = MainControlObject.GetComponent<MainControls>();
            VGSMC.CutUnusedMeshEvent -= StartCuttingMethod;
            VGSMC.WrapEvent -= WrapMethod;
            //WrapEvent -= CreateMaterialsAndTextureInProject;//To create the GroundNormalTexture 
            VGSMC.WrapCutMeshesEvent -= WrapCutMeshesMethod;
            VGSMC.SaveCurrentTextureEvent -= CreateCurrentPaintingTextureAsset;
        }

        private void OnDisable()
        {
            if (Application.isPlaying == false)
            {
                EditorApplication.update -= EditorUpdates;
                RemoveOtherEventsToMainControlObject();
            }
            //EditorUtility.SetDirty(this);
            //for (int i = 0; i < CreatedAssets.Count; i++) { Resources.UnloadAsset(CreatedAssets[i]); }
        }

        private void CreateCurrentPaintingTextureAsset()
        {
            Texture texToCreate = null;
            if (GrassMeshRenderer == null) GrassMeshRenderer = GetComponent<MeshRenderer>();
            if (VGSMC.PaintShadows == true && ShadowTexture == null) texToCreate = GrassMeshRenderer.sharedMaterial.GetTexture("_shadowTexture");
            if (VGSMC.PaintHeight == true && HeightTexture == null) texToCreate = GrassMeshRenderer.sharedMaterial.GetTexture("_heightTexture");
            if (VGSMC.PaintColor == true && ColorTexture == null) texToCreate = GrassMeshRenderer.sharedMaterial.GetTexture("_colorTexture");
            bool ConfirmCreation = false;
            if (texToCreate == null) ConfirmCreation = false;
            else
            {
                RenderTexture RTToCompare = new RenderTexture(texToCreate.width, texToCreate.height, 0, SVGMCTextureDrawing.currentRTFormat);
                Graphics.Blit(texToCreate, RTToCompare);
                bool IsBlack = false;
                ConfirmCreation = CompareTexturesAndCheckIfBlack(SaveCheckRT, RTToCompare, ref IsBlack);
                DestroyImmediate(RTToCompare);
                if (IsBlack == true) ConfirmCreation = false;
            }
            MainControls SVGMainControls = MainControlObject.GetComponent<MainControls>();
            if (ConfirmCreation == true)
            {
                CreateAndAssignTextures = true;
                if (SVGMainControls.PaintShadows == true)
                {
                    if (ShadowTexture == null) CreateTextureInProject();
                    SaveShadowTextureEvent?.Invoke();
                }
                if (SVGMainControls.PaintHeight == true)
                {
                    if (HeightTexture == null) CreateTextureInProject();
                    SaveHeightTextureEvent?.Invoke();
                }
                if (SVGMainControls.PaintColor == true)
                {
                    if (ColorTexture == null) CreateTextureInProject();
                    SaveColorTextureEvent?.Invoke();
                }
                CreateAndAssignTextures = false;
                Graphics.Blit(texToCreate, SaveCheckRT);
            }
        }

        private void TexturePainting()
        {
            //Get Component 
            if (TextureTrigger1 == false)
            {
                TextureTrigger1 = true;
            }
            //Height 
            TextureDrawingTriggers(true, false, false, VGSMC.PaintHeight, DrawHeightTrigger);
            //Shadows
            TextureDrawingTriggers(false, true, false, VGSMC.PaintShadows, DrawShadowTrigger);
            TextureDrawingTriggers(false, false, true, VGSMC.PaintColor, DrawColorTrigger);
            if (VGSMC.PaintHeight == true || VGSMC.PaintShadows == true) Tools.hidden = true;
            if (VGSMC.PaintHeight == false && VGSMC.PaintShadows == false) Tools.hidden = false;
        }

        private void TextureDrawingTriggers(bool IsHeight, bool IsShadow, bool IsColor, bool Draw, bool DrawTrigger1)
        {
            bool DrawTrigger2 = false;
            //If DrawTexture Checked 
            if (Draw == true)
            {
                if (DrawTrigger1 == false) DrawTrigger2 = true;
                if (IsHeight == true) { DrawHeightTrigger = true; }
                if (IsShadow == true) { DrawShadowTrigger = true; }
                if (IsColor == true) { DrawColorTrigger = true; }
            }
            else//If DrawTexture Un-Checked 
            {
                if (DrawTrigger1 == true) DrawTrigger2 = true;
                if (IsHeight == true) { DrawHeightTrigger = false; }
                if (IsShadow == true) { DrawShadowTrigger = false; }
                if (IsColor == true) { DrawColorTrigger = false; }
            }

            //Apply 
            if (DrawTrigger2 == true)
            {
                if (IsHeight == true) SVGMHeightMaster.enabled = Draw;
                if (IsShadow == true) SVGMShadowsMaster.enabled = Draw;
                if (IsColor == true) SVGMColorTextureMaster.enabled = Draw;
                DrawTrigger2 = false;
            }
        }

        [SerializeField, HideInInspector] public bool CreateNormalTexture, CreateHeightTexture, CreateNoMeshRootsHeightTexture;
        public void CreateTextureInProject()
        {
            VGSMC.CreatedDirectoryIfNecessary();
            //Check if Folders are Valid 
            bool AssetValid = false, NewFolderValid = false;
            if (AssetDatabase.IsValidFolder(VGSMC.GrassAssetFolderPath) == false)
            {
                Debug.Log("GrassShader Asset Folder not found. Assign it in 'Data (Don't Touch)'");
            }
            else { AssetValid = true; }
            if (AssetDatabase.IsValidFolder(VGSMC.CreatedFilesDirectoryPath) == false || VGSMC.CreatedFilesDirectoryPath == null)
            {
                Debug.Log("New Folder Path not found");
            }
            else { NewFolderValid = true; }

            //If both folders are valid 
            if (AssetValid == true && NewFolderValid == true)
            {
                if (CreateAndAssignTextures == true)
                {
                    //Check if Folder Exists, create a new one if not 
                    string NewFolderTexturesPath = VGSMC.CreatedFilesDirectoryPath + "/_Textures";//UnderScore because it's added to the folder automatically
                    if (AssetDatabase.IsValidFolder(NewFolderTexturesPath) == false)
                    {
                        AssetDatabase.CreateFolder(VGSMC.CreatedFilesDirectoryPath, "/Textures");
                    }

                    //Check the already created Textures 
                    System.IO.DirectoryInfo Directory = new System.IO.DirectoryInfo(VGSMC.CreatedFilesDirectoryPath);
                    int HeightAddedInt = 1;
                    int ShadowAddedInt = 1;
                    int ColorAddedInt = 1;
                    int GroundNormalAddedInt = 1;
                    int NoMeshRootsHeightsAddedInt = 1;
                    string[] FoldersToSearch = new string[] { NewFolderTexturesPath };
                    string[] AlreadyCreatedTextures = AssetDatabase.FindAssets("t:texture2D", FoldersToSearch);
                    //Get the highest numbers + 1
                    for (int i = 0; i < AlreadyCreatedTextures.Length; i++)
                    {
                        string Path = AssetDatabase.GUIDToAssetPath(AlreadyCreatedTextures[i]);
                        if (VGSMC.PaintHeight == true || CreateHeightTexture == true)
                        {
                            if (HeightTexture == null)
                            {
                                //If contains "Height"
                                if (Path.Contains(Directory.Name + "Height"))
                                {
                                    int EndingNumbers = 0;
                                    char[] SeperatorChar = new char[] { char.Parse("_"), char.Parse(".") };
                                    string[] PathSplit = Path.Split(SeperatorChar);
                                    int.TryParse(PathSplit[PathSplit.Length - 2], out EndingNumbers);
                                    if (EndingNumbers >= HeightAddedInt) { HeightAddedInt = EndingNumbers + 1; }
                                }
                            }
                        }
                        if (VGSMC.PaintShadows == true && ShadowTexture == null)
                        {
                            //If contains "Shadow"
                            if (Path.Contains(Directory.Name + "Shadow"))
                            {
                                int EndingNumbers = 0;
                                char[] SeperatorChar = new char[] { char.Parse("_"), char.Parse(".") };
                                string[] PathSplit = Path.Split(SeperatorChar);
                                int.TryParse(PathSplit[PathSplit.Length - 2], out EndingNumbers);
                                if (EndingNumbers >= ShadowAddedInt) { ShadowAddedInt = EndingNumbers + 1; }
                            }
                        }
                        if (VGSMC.PaintColor == true && ColorTexture == null)
                        {
                            //If contains "Color"
                            if (Path.Contains(Directory.Name + "Color") && Path.Contains("Mask") == false)
                            {
                                int EndingNumbers = 0;
                                char[] SeperatorChar = new char[] { char.Parse("_"), char.Parse(".") };
                                string[] PathSplit = Path.Split(SeperatorChar);
                                int.TryParse(PathSplit[PathSplit.Length - 2], out EndingNumbers);
                                if (EndingNumbers >= ColorAddedInt) { ColorAddedInt = EndingNumbers + 1; }
                            }
                        }
                        if (CreateNormalTexture == true && GroundNormalTexture == null)
                        {
                            //If contains "GroundNormal"
                            if (Path.Contains(Directory.Name + "GroundNormal"))
                            {
                                int EndingNumbers = 0;
                                char[] SeperatorChar = new char[] { char.Parse("_"), char.Parse(".") };
                                string[] PathSplit = Path.Split(SeperatorChar);
                                int.TryParse(PathSplit[PathSplit.Length - 2], out EndingNumbers);
                                if (EndingNumbers >= GroundNormalAddedInt) { GroundNormalAddedInt = EndingNumbers + 1; }
                            }
                        }
                        if (CreateNoMeshRootsHeightTexture == true && NoMeshRootsHeightsTexture == null)
                        {
                            //If contains "GroundNormal"
                            if (Path.Contains(Directory.Name + "NoMeshRootsHeights"))
                            {
                                int EndingNumbers = 0;
                                char[] SeperatorChar = new char[] { char.Parse("_"), char.Parse(".") };
                                string[] PathSplit = Path.Split(SeperatorChar);
                                int.TryParse(PathSplit[PathSplit.Length - 2], out EndingNumbers);
                                if (EndingNumbers >= NoMeshRootsHeightsAddedInt) { NoMeshRootsHeightsAddedInt = EndingNumbers + 1; }
                            }
                        }
                    }
                    if (HeightTexture == null)
                    {
                        if (VGSMC.PaintHeight == true || CreateHeightTexture == true)
                        {
                            //Have to Copy to get the Import Settings right 
                            string OriginalHeightPath = AssetDatabase.GetAssetPath(OriginalHeightTexture);
                            string NewHeightTexturePathInt = NewFolderTexturesPath + "/" + Directory.Name + "Height" + "_" + HeightAddedInt + ".exr";
                            AssetDatabase.CopyAsset(OriginalHeightPath, NewHeightTexturePathInt);
                            //Assign Textures
                            HeightTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(NewHeightTexturePathInt);
                            //SVGAssetDestroyerValues.CreatedAssets.Add(HeightTexture);
                            CreatedAssets.Add(HeightTexture);
                            VGSMC.CreatedAssets.Add(HeightTexture);
                        }
                    }
                    if (ShadowTexture == null && VGSMC.PaintShadows == true)
                    {
                        string NewShadowTexturePathWithInt = NewFolderTexturesPath + "/" + Directory.Name + "Shadow" + "_" + ShadowAddedInt + ".exr";
                        string OriginalShadowPath = AssetDatabase.GetAssetPath(OriginalShadowTexture);
                        //Have to Copy to get the Import Settings right 
                        AssetDatabase.CopyAsset(OriginalShadowPath, NewShadowTexturePathWithInt);
                        //Assign Textures
                        ShadowTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(NewShadowTexturePathWithInt);
                        //SVGAssetDestroyerValues.CreatedAssets.Add(ShadowTexture);
                        CreatedAssets.Add(ShadowTexture);
                        VGSMC.CreatedAssets.Add(ShadowTexture);
                    }
                    if (ColorTexture == null && VGSMC.PaintColor == true)
                    {
                        string NewColorTexturePathWithInt = NewFolderTexturesPath + "/" + Directory.Name + "Color" + "_" + ColorAddedInt + ".exr";
                        string OriginalColorPath = AssetDatabase.GetAssetPath(OriginalColorTexture);
                        //Have to Copy to get the Import Settings right 
                        AssetDatabase.CopyAsset(OriginalColorPath, NewColorTexturePathWithInt);
                        //Assign Textures
                        ColorTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(NewColorTexturePathWithInt);
                        //SVGAssetDestroyerValues.CreatedAssets.Add(ColorTexture);
                        CreatedAssets.Add(ColorTexture);
                        VGSMC.CreatedAssets.Add(ColorTexture);
                    }
                    if (GroundNormalTexture == null && CreateNormalTexture == true)
                    {
                        string NewGroundNormalTexturePathWithInt = NewFolderTexturesPath + "/" + Directory.Name + "GroundNormal" + "_" + GroundNormalAddedInt + ".exr";
                        string OriginalGroundNormalPath = AssetDatabase.GetAssetPath(OriginalGroundNormalTexture);
                        //Have to Copy to get the Import Settings right 
                        AssetDatabase.CopyAsset(OriginalGroundNormalPath, NewGroundNormalTexturePathWithInt);
                        //Assign Textures
                        GroundNormalTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(NewGroundNormalTexturePathWithInt);
                        //SVGAssetDestroyerValues.CreatedAssets.Add(GroundNormalTexture);
                        CreatedAssets.Add(GroundNormalTexture);
                        VGSMC.CreatedAssets.Add(GroundNormalTexture);
                    }
                    if (NoMeshRootsHeightsTexture == null && CreateNoMeshRootsHeightTexture == true)
                    {
                        string NewNoMeshRootsHeightsTexturePathWithInt = NewFolderTexturesPath + "/" + Directory.Name + "NoMeshRootsHeights" + "_" + NoMeshRootsHeightsAddedInt + ".exr";
                        string OriginalNoMeshRootsHeightsPath = AssetDatabase.GetAssetPath(OriginalNoMeshRootsHeightsTexture);
                        //Have to Copy to get the Import Settings right 
                        AssetDatabase.CopyAsset(OriginalNoMeshRootsHeightsPath, NewNoMeshRootsHeightsTexturePathWithInt);
                        //Assign Textures
                        NoMeshRootsHeightsTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(NewNoMeshRootsHeightsTexturePathWithInt);
                        CreatedAssets.Add(NoMeshRootsHeightsTexture);
                        VGSMC.CreatedAssets.Add(NoMeshRootsHeightsTexture);
                    }
                    EditorUtility.SetDirty(this);
                }
            }
        }
        //void OnDrawGizmos()
        //{
        //    Gizmos.DrawWireCube(GrassMeshRenderer.bounds.center, GrassMeshRenderer.bounds.size);
        //}
#endif
    }
}