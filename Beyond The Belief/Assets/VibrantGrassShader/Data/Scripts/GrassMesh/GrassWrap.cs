using System.Collections.Generic;
using UnityEngine;
using VibrantGrassShaderTools;
using UnityEditor;

namespace VibrantGrassShader
{
    [ExecuteInEditMode]
    public class GrassWrap : MonoBehaviour
    {
#if UNITY_EDITOR
        [Foldout("Data (don't touch)", true)]
        [SerializeField] private Mesh createdMesh = null;
        [SerializeField] private GrassFieldMaster grassShaderMaster = null;
        [SerializeField] private WrapMaster grassShaderWrapMaster = null;
        [SerializeField] private string newFolderName = "GrassField";
        [SerializeField] private bool enableCutting = false;
        [SerializeField] private MainControls VGSMC = null;
        public delegate void WrapDelegate();
        public WrapDelegate WrapJustFinishedEvent;

        public Mesh CreatedMesh
        {
            get { return createdMesh; }
            set
            {
                SerializedObject serializedObject = new SerializedObject(this);
                createdMesh = null;
                createdMesh = value;//Reset so it detects the change
                SerializedProperty serializedProperty = serializedObject.FindProperty("createdMesh");
                serializedProperty.objectReferenceValue = value;
                serializedObject.ApplyModifiedProperties();
            }
        }

        public string NewFolderName
        {
            get { return newFolderName; }
            set
            {
                SerializedObject serializedObject = new SerializedObject(this);
                newFolderName = null;
                newFolderName = value;//Reset so it detects the change
                SerializedProperty serializedProperty = serializedObject.FindProperty("newFolderName");
                serializedProperty.stringValue = value;
                serializedObject.ApplyModifiedProperties();
            }
        }
        public bool EnableCutting
        {
            get { return enableCutting; }
            set
            {
                SerializedObject serializedObject = new SerializedObject(this);
                enableCutting = false;
                enableCutting = true;
                enableCutting = value;//Reset so it detects the change
                SerializedProperty serializedProperty = serializedObject.FindProperty("enableCutting");
                serializedProperty.boolValue = value;
                serializedObject.ApplyModifiedProperties();
            }
        }

        [SerializeField, HideInInspector] public bool Wrapping = false;
        private Vector3[] OriginalVertices = null, MovedVertices = null;
        int FramesToWait;

#endif
        private void Start()
        {
            if (Application.isPlaying == true) Destroy(this);
        }

#if UNITY_EDITOR
        private GrassMeshMaster grassMeshMaster;
        private void OnEnable()
        {
            if (Application.isPlaying == false)
            {
                EditorApplication.update += EditorUpdates;
                if (grassShaderMaster == null) grassShaderMaster = GetComponentInParent<GrassFieldMaster>();
                if (grassShaderWrapMaster == null) grassShaderWrapMaster = GetComponentInParent<WrapMaster>();
                VGSMC = grassShaderMaster.gameObject.GetComponentInParent<MainControls>();
                grassShaderWrapMaster.WrapEvent += InitializeWrap;
                grassShaderWrapMaster.WrapCutMeshesEvent += InitializeWrapForCutMeshes;
                meshFilter = GetComponent<MeshFilter>();
                Wrapping = false;
                WasWrappingLastFrame = false;
                grassMeshMaster = GetComponent<GrassMeshMaster>();
            }
        }

        private void OnDisable()
        {
            if (Application.isPlaying == false)
            {
                EditorApplication.update -= EditorUpdates;
                grassShaderWrapMaster.WrapEvent -= InitializeWrap;
                grassShaderWrapMaster.WrapCutMeshesEvent -= InitializeWrapForCutMeshes;
            }
        }

        private bool WasWrappingLastFrame;
        [SerializeField] private bool ShowDebug = false;
        void EditorUpdates()
        {
            if (Application.isPlaying == false && this != null)
            {
                if (Wrapping == true)
                {
                    WrapMethod();
                }
                else
                {
                    if (WasWrappingLastFrame == true)
                    {
                        WrapJustFinishedEvent?.Invoke();
                        WasWrappingLastFrame = false;
                        if (VGSMC.InitializationFinished == true && VGSMC.Wrapping == true) grassShaderMaster.gameObject.SetActive(false);
                    }
                }
                if (grassShaderWrapMaster.ShowDebugs == true)
                {
                    DebugMethod();
                }
                if (ShowDebug == true)
                {
                    //float HalfFieldDist = VGSMC.GrassFieldMaxDistance / 2.0f;
                    //for (int x = 0; x < MovedVertices.Length; x++)
                    //{
                    //    float XUV = Mathf.Lerp(-HalfFieldDist, HalfFieldDist, HeightsUVs[x].x);
                    //    float YUV = Mathf.Lerp(-HalfFieldDist, HalfFieldDist, HeightsUVs[x].y);
                    //    Vector2 HorizPos = new Vector2(XUV, YUV);
                    //    //Debug.DrawRay(transform.position + new Vector3(HorizPos.x, 0.0f, HorizPos.y),  Vector3.right * 0.01f, Color.red, 10.0f);
                    //    Debug.DrawRay(new Vector3(transform.position.x + HorizPos.x, 
                    //        transform.position.y - HeightsForRT[x], transform.position.z + HorizPos.y),  
                    //        Vector3.up * 0.01f, Color.red, 10.0f);
                    //    Debug.DrawRay(new Vector3(transform.position.x + HorizPos.x, 
                    //        transform.position.y - HeightsForRT[x], transform.position.z + HorizPos.y),  
                    //        Vector3.right * 0.01f, Color.blue, 10.0f);
                    //}
                    ShowDebug = false;
                }
            }
        }

        public void InitializeWrapForCutMeshes()
        {
            CutWithResetCutMeshes = true;
            InitializeWrap();
        }

        public void InitializeWrap()
        {
            CheckAndAssignMesh();
            SetValues();
            Wrapping = true;
        }

        [SerializeField, HideInInspector] private MeshFilter meshFilter = null;
        private List<int> AllKeptTriangles = new List<int>();
        private int[] OriginalTriangles;
        private int CurrentTrianglesIteration;
        private int TrianglesIterationsLeft;
        [HideInInspector] public bool CutWithHeightTexture = false, CutWithResetCutMeshes = false;
        private void WrapMethod()
        {
            WasWrappingLastFrame = true;
            if (TrianglesIterationsLeft <= 0)
            {
                if (AllKeptTriangles.Count == 0)
                {
                    Debug.Log("No collider found. Please make sure the grass field is above a collider with one of the Layers assigned in 'MainControls-Setup-GroundLayers'");
                    Debug.Log("On 'MainControls-Secondary', Click 'Reset And Wrap Meshes' to wrap again after fixing the problem.");
                }
                //If there was nothing cut, then use Texture for height instead of Mesh 
                if (AllKeptTriangles.Count < OriginalTriangles.Length || CutWithHeightTexture == true)
                {
                    if (grassShaderMaster.NoMeshRootsHeightsTexture != null)
                    { AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(grassShaderMaster.NoMeshRootsHeightsTexture)); }
                    //Create Mesh
                    if (CreatedMesh == null) CreateMeshAsset();
                    CreatedMesh.vertices = OriginalVertices;
                    CreatedMesh.triangles = OriginalTriangles;
                    CreatedMesh.RecalculateBounds();
                    //Set Vertices
                    List<Vector3> MovedVerticesInList = new List<Vector3>();
                    for (int i = 0; i < MovedVertices.Length; i++)
                    { MovedVerticesInList.Add(MovedVertices[i]); }
                    CreatedMesh.SetVertices(MovedVerticesInList);
                    //Set Triangles
                    if (AllKeptTriangles.Count > 0 && EnableCutting == true && grassShaderWrapMaster.CutOutOfGroundMeshWhenWrapping == true)
                    {
                        int[] TrianglesResult = new int[AllKeptTriangles.Count];
                        for (int m = 0; m < AllKeptTriangles.Count; m++)
                        {
                            TrianglesResult[m] = AllKeptTriangles[m];
                        }
                        CreatedMesh.triangles = TrianglesResult;
                    }
                    CreatedMesh.RecalculateBounds();
                    meshFilter.sharedMesh = CreatedMesh;
                    CutWithResetCutMeshes = false;
                }
                if (AllKeptTriangles.Count >= OriginalTriangles.Length && CutWithHeightTexture == false)
                {
                    if (CreatedMesh != null) AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(CreatedMesh));
                    meshFilter.sharedMesh = VGSMC.CurrentAppliedMeshOriginal;
                    CreatedMesh = null;
                    SaveTextureForRootsHeightsAndCreateTexIfNecessary();
                }
                CutWithHeightTexture = false;
                Wrapping = false;
                if (CreatedMesh != null) CreatedMesh.normals = new Vector3[0];
                //Debug.Log(AllKeptTriangles.Count + " : " + OriginalTriangles.Length + " : " + OriginalVertices.Length + " = null");
                AllKeptTriangles = null;
                OriginalVertices = null;
                OriginalTriangles = null;
                MovedVertices = null;
                int RootsHeightsTexAssigned = 1;
                if (grassShaderMaster.SVGMeshMaster.MainMatInstanced.GetTexture("_NoMeshRootsHeightsTexture") == null) RootsHeightsTexAssigned = 0;
                grassShaderMaster.SVGMeshMaster.MainMatInstanced.SetInt("_RootsHeightsTexAssigned", RootsHeightsTexAssigned);
                return;
            }
            TrianglesIterationsLeft = OriginalTriangles.Length - CurrentTrianglesIteration;
            int IterationsToDo2 = Mathf.RoundToInt(Mathf.Clamp(TrianglesIterationsLeft, 0.0f, grassShaderWrapMaster.MaximumIterationsPerFrame));
            int j = CurrentTrianglesIteration;
            if (IterationsToDo2 > 0)
            {
                int IterationCeiling = Mathf.RoundToInt(Mathf.Clamp((float)IterationsToDo2 + (float)CurrentTrianglesIteration, 0.0f, OriginalTriangles.Length));
                while (j < IterationCeiling)
                {
                    int p = j;
                    bool AddTriangle = true;
                    while (p <= j + 2)
                    {
                        int Index = OriginalTriangles[p];
                        Vector3 RayBasePos = new Vector3(transform.TransformPoint(MovedVertices[Index]).x,
                        transform.position.y + VGSMC.WrapBasePosAddedHeight, transform.TransformPoint(MovedVertices[Index]).z);
                        RaycastHit[] allRayHits = Physics.RaycastAll(RayBasePos, Vector3.down, VGSMC.WrapMaxRayDistance, grassShaderWrapMaster.GrassGroundLayers, QueryTriggerInteraction.Ignore);
                        bool rayHit = false;
                        float ClosestDist = Mathf.Infinity;
                        RaycastHit ClosestRaycastHit = new RaycastHit();
                        for (int i = 0; i < allRayHits.Length; i++)
                        {
                            float dist = allRayHits[i].distance;
                            if (dist < ClosestDist)
                            {
                                ClosestDist = dist;
                                ClosestRaycastHit = allRayHits[i];
                            }
                            rayHit = true;
                        }
                        if (rayHit == true)
                        {
                            MovedVertices[Index].y = transform.InverseTransformPoint(ClosestRaycastHit.point).y;
                        }
                        else
                        {
                            if (EnableCutting == true && grassShaderWrapMaster.CutOutOfGroundMeshWhenWrapping == true) AddTriangle = false;
                            break;
                        }
                        p += 1;
                    }
                    if (AddTriangle == true && EnableCutting == true && grassShaderWrapMaster.CutOutOfGroundMeshWhenWrapping == true)
                    {
                        AllKeptTriangles.Add(OriginalTriangles[j]);
                        AllKeptTriangles.Add(OriginalTriangles[j + 1]);
                        AllKeptTriangles.Add(OriginalTriangles[j + 2]);
                    }
                    j += 3;
                }
            }
            CurrentTrianglesIteration += grassShaderWrapMaster.MaximumIterationsPerFrame;
        }


        private void SetValues()
        {
            OriginalVertices = VGSMC.CurrentAppliedMeshOriginal.vertices;
            OriginalTriangles = VGSMC.CurrentAppliedMeshOriginal.triangles;
            MovedVertices = OriginalVertices;
            CurrentTrianglesIteration = 0;
            TrianglesIterationsLeft = 100;
        }

        private void CreateMeshAsset()
        {
            VGSMC.CreatedDirectoryIfNecessary();
            //Check if Folder Exists, create a new one if not 
            string MeshesFolderPath = VGSMC.CreatedFilesDirectoryPath + "/_Meshes";//UnderScore because it's added to the folder automatically
            if (AssetDatabase.IsValidFolder(MeshesFolderPath) == false)
            {
                AssetDatabase.CreateFolder(VGSMC.CreatedFilesDirectoryPath, "/Meshes");
            }
            string NewCreatedFolderPath = MeshesFolderPath + "/_" + NewFolderName;
            if (AssetDatabase.IsValidFolder(NewCreatedFolderPath) == false)
            {
                AssetDatabase.CreateFolder(VGSMC.CreatedFilesDirectoryPath + "/_Meshes", "/" + NewFolderName);
                VGSMC.CreatedMeshFolder = AssetDatabase.LoadAssetAtPath<Object>(NewCreatedFolderPath);
            }
            //Check the already created Meshes 
            System.IO.DirectoryInfo Directory = new System.IO.DirectoryInfo(VGSMC.CreatedFilesDirectoryPath);
            int AddedInt = 1;
            string[] FoldersToSearch = new string[] { NewCreatedFolderPath };
            string[] AlreadyCreatedMeshes = AssetDatabase.FindAssets("t:Mesh", FoldersToSearch);
            //Get the highest numbers + 1
            for (int i = 0; i < AlreadyCreatedMeshes.Length; i++)
            {
                string Path = AssetDatabase.GUIDToAssetPath(AlreadyCreatedMeshes[i]);
                int EndingNumbers = 0;
                char[] SeperatorChar = new char[] { char.Parse("_"), char.Parse(".") };
                string[] PathSplit = Path.Split(SeperatorChar);
                int.TryParse(PathSplit[PathSplit.Length - 2], out EndingNumbers);
                if (EndingNumbers >= AddedInt) { AddedInt = EndingNumbers + 1; }
            }
            string NewMeshPath2 = NewCreatedFolderPath + "/" + Directory.Name + NewFolderName + "_" + AddedInt + ".asset";
            Mesh mesh = Instantiate(VGSMC.CurrentAppliedMeshCompressed) as Mesh;
            AssetDatabase.CreateAsset(mesh, NewMeshPath2);
            AssetDatabase.SaveAssets();
            mesh = AssetDatabase.LoadAssetAtPath(NewMeshPath2, typeof(Mesh)) as Mesh;
            CreatedMesh = mesh;
            grassShaderMaster.CreatedAssets.Add(CreatedMesh);
            VGSMC.CreatedAssets.Add(CreatedMesh);
        }

        private void CheckAndAssignMesh()
        {
            MeshFilter meshFilterCheck = GetComponent<MeshFilter>();
            meshFilter = null;
            AllKeptTriangles = new List<int>();
            if (meshFilterCheck != null) meshFilter = meshFilterCheck;
        }

        private void DebugMethod()
        {
            RaycastHit raycastHit = new RaycastHit();
            RaycastHit[] allRayHits = Physics.RaycastAll(transform.position + Vector3.up * VGSMC.WrapBasePosAddedHeight, Vector3.down,
                VGSMC.WrapMaxRayDistance, grassShaderWrapMaster.GrassGroundLayers, QueryTriggerInteraction.Ignore);
            bool rayHit = false;
            for (int i = 0; i < allRayHits.Length; i++)
            {
                raycastHit = allRayHits[i];
                rayHit = true;
            }
            if (rayHit == true)
            { Debug.DrawRay(transform.position + Vector3.up * VGSMC.WrapBasePosAddedHeight, Vector3.down * raycastHit.distance, Color.green); }
            else
            { Debug.DrawRay(transform.position + Vector3.up * VGSMC.WrapBasePosAddedHeight, Vector3.down * VGSMC.WrapMaxRayDistance, Color.red); }
        }
        private void OnDrawGizmos()
        {
            if (grassShaderWrapMaster.ShowDebugs == true)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(transform.position + (Vector3.up * VGSMC.WrapBasePosAddedHeight), 2.0f);
            }
        }

        [SerializeField] private int SmoothingStrength = 10;
        public void SaveTextureForRootsHeightsAndCreateTexIfNecessary()
        {
            //Make the Grid
            float HalfUVWorldDist = VGSMC.GrassFieldUVWorldDistance / 2.0f;
            Vector2[] HeightsUVs = new Vector2[MovedVertices.Length];
            float[] HeightsForRT = new float[MovedVertices.Length];
            Vector3[] LowestAndHighestVertices = new Vector3[2];
            for (int x = 0; x < MovedVertices.Length; x++)
            {
                Vector3 pos = new Vector3(MovedVertices[x].x, 0.0f, MovedVertices[x].z);
                float XUV = Mathf.Clamp01(Mathf.InverseLerp(-HalfUVWorldDist, HalfUVWorldDist, pos.x));
                float YUV = Mathf.Clamp01(Mathf.InverseLerp(-HalfUVWorldDist, HalfUVWorldDist, pos.z));
                HeightsUVs[x] = new Vector2(XUV, YUV);
                HeightsForRT[x] = - MovedVertices[x].y;
                if (MovedVertices[x].y < LowestAndHighestVertices[0].y) LowestAndHighestVertices[0] = MovedVertices[x];
                if (MovedVertices[x].y > LowestAndHighestVertices[1].y) LowestAndHighestVertices[1] = MovedVertices[x];
            }
            grassMeshMaster.DistBetweenLowestAndHighestPoint = Mathf.Abs(LowestAndHighestVertices[1].y - LowestAndHighestVertices[0].y);
            Vector3 BetweenLowestAndHighestLocal = LowestAndHighestVertices[0] + ((LowestAndHighestVertices[1] - LowestAndHighestVertices[0]) / 2.0f);
            grassMeshMaster.BoundsCenterLocal = new Vector3(0.0f, BetweenLowestAndHighestLocal.y, 0.0f);
            //Make A Texture Of Roots Heights
            string StringToParse = VGSMC.RootsHeightsPrecision.ToString();
            if (grassShaderMaster.OverwriteDefaultRootsHeightsPrecision == true) StringToParse = grassShaderMaster.RootsHeightsPrecision.ToString();
            int IntResult = 1024;
            int.TryParse(StringToParse.Replace("_", ""), out IntResult);
            int RootHeightTextureSize = IntResult;
            RenderTexture HeightRTTemp = RenderTexture.GetTemporary(RootHeightTextureSize, RootHeightTextureSize, 0, RenderTextureFormat.RFloat);
            int MaximumArraySize = 1000;
            int AmountOfSteps = 1;
            if (HeightsForRT.Length > MaximumArraySize) AmountOfSteps = Mathf.CeilToInt((float)HeightsForRT.Length / (float)MaximumArraySize);
            for (int S = 0; S < AmountOfSteps; S++)
            {
                List<float> HeightsList = new List<float>();
                List<Vector4> uvVector4List = new List<Vector4>();
                int CurrentArraySize = Mathf.FloorToInt(Mathf.Clamp((float)HeightsForRT.Length,
                    0.0f, Mathf.Clamp(HeightsForRT.Length - (MaximumArraySize * S), 0.0f, MaximumArraySize)));
                for (int i = 0; i < CurrentArraySize; i++)
                {
                    int Index = (MaximumArraySize * S) + i;
                    HeightsList.Add(HeightsForRT[Index] + 10.0f);
                    uvVector4List.Add(new Vector4(1.0f - HeightsUVs[Index].x, 1.0f - HeightsUVs[Index].y, 0.0f, 0.0f));
                }
                VGSMC.RootHeightsPainterMat.SetFloatArray("_heightFloats", HeightsList);
                VGSMC.RootHeightsPainterMat.SetVectorArray("_uvCoordinates", uvVector4List);
                VGSMC.RootHeightsPainterMat.SetFloat("_ArraysLength", uvVector4List.Count);
                VGSMC.RootHeightsPainterMat.SetInt("_TexSize", RootHeightTextureSize);
                RenderTexture tempTexture = RenderTexture.GetTemporary(HeightRTTemp.width, HeightRTTemp.height, 0, RenderTextureFormat.RFloat);
                Graphics.Blit(HeightRTTemp, tempTexture);
                Graphics.Blit(tempTexture, HeightRTTemp, VGSMC.RootHeightsPainterMat);
                RenderTexture.ReleaseTemporary(tempTexture);
            }
            for (int i = 0; i < SmoothingStrength; i++)
            {
                RenderTexture tempTexture2 = RenderTexture.GetTemporary(HeightRTTemp.width, HeightRTTemp.height, 0, RenderTextureFormat.RFloat);
                Graphics.Blit(HeightRTTemp, tempTexture2, VGSMC.TextureColorSoftenerMat);
                Graphics.Blit(tempTexture2, HeightRTTemp);
                RenderTexture.ReleaseTemporary(tempTexture2);
            }
            //Save Texture
            if (grassShaderMaster.NoMeshRootsHeightsTexture == null)
            {
                grassShaderMaster.CreateNoMeshRootsHeightTexture = true;
                grassShaderMaster.CreateAndAssignTextures = true;
                grassShaderMaster.CreateTextureInProject();
                grassShaderMaster.CreateNoMeshRootsHeightTexture = false;
                grassShaderMaster.CreateAndAssignTextures = false;
            }
            string RootsHeightsTexturePath = AssetDatabase.GetAssetPath(grassShaderMaster.NoMeshRootsHeightsTexture);
            if (RootsHeightsTexturePath != "" && RootsHeightsTexturePath != null)
            {
                var tex = new Texture2D(HeightRTTemp.width, HeightRTTemp.height, TextureFormat.RFloat, false);
                RenderTexture.active = HeightRTTemp;

                tex.ReadPixels(new Rect(0, 0, HeightRTTemp.width, HeightRTTemp.height), 0, 0);
                tex.Apply();

                //System.IO.File.WriteAllBytes(RootsHeightsTexturePath, tex.EncodeToEXR());
                System.IO.File.WriteAllBytes(RootsHeightsTexturePath, tex.EncodeToEXR(Texture2D.EXRFlags.CompressZIP));
                AssetDatabase.ImportAsset(RootsHeightsTexturePath);
                DestroyImmediate(tex);
            }
            RenderTexture.ReleaseTemporary(HeightRTTemp);
            grassShaderMaster.SVGMeshMaster.MainMatInstanced.SetTexture("_NoMeshRootsHeightsTexture", grassShaderMaster.NoMeshRootsHeightsTexture);
        }
#endif
    }
}