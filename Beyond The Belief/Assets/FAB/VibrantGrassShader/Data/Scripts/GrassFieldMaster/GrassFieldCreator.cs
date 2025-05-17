using System.Collections.Generic;
using UnityEngine;
using VibrantGrassShaderTools;
using UnityEditor;

namespace VibrantGrassShader
{
    [ExecuteInEditMode]
    public class GrassFieldCreator : MonoBehaviour
    {
#if UNITY_EDITOR
        [Foldout("Data (don't touch)", true)]
        [SerializeField] public Object GrassFieldBasePrefab = null, ControlArrowPrefab = null;
        [SerializeField] public List<GameObject> ControlArrows = new List<GameObject>();
        [SerializeField] private float ControlArrowsHeight = 150.0f, ControlArrowsScale = 2.0f, ControlArrowsDistance = 2.0f;
        [SerializeField, HideInInspector] public MainControls VGSMC = null;

        public delegate void ControlsDelegate();
        public ControlsDelegate DisableControlsEvent;
        private HierarchyOrganizationTools hierarchyOrganizationTools;

        private void OnEnable()
        {
            if (Application.isPlaying == false)
            {
                VGSMC = GetComponentInParent<MainControls>();
                EditorApplication.update += EditorUpdates;
                DisableControlsEvent += DisableControlsMethod;
                VGSMC.DisableControlArrows_Event += DisableControlsMethod;
                hierarchyOrganizationTools = new HierarchyOrganizationTools();
                FrameCount = 0;
                ControlArrows = new List<GameObject>();
                SVGWrapMaster = GetComponent<WrapMaster>();
                GrassFieldBasePrefab = VGSMC.GrassFieldBasePrefab;
            }
            IntersectionTransparencyOld = VGSMC.EnableTransparency;
        }
        private void OnDisable()
        {
            if (Application.isPlaying == false)
            {
                EditorApplication.update -= EditorUpdates;
                DisableControlsEvent -= DisableControlsMethod;
                VGSMC.DisableControlArrows_Event -= DisableControlsMethod;
            }
        }
        private void Start()
        {
            if (Application.isPlaying == true) Destroy(this);
        }

        void EditorUpdates()
        {
            if (Application.isPlaying == false && this != null)
            {
                EnableAndDisableArrowsMethod();
                if (VGSMC.DebugWrapRay == true) DebugMethod();
                if (VGSMC.CreatingGrass == false) KeepSpawningInDirection = false;
            }
        }

        private void DebugMethod()
        {
            Debug.DrawLine(transform.position + Vector3.up * VGSMC.WrapBasePosAddedHeight,
                transform.position + Vector3.up * VGSMC.WrapBasePosAddedHeight + Vector3.down * VGSMC.WrapMaxRayDistance, Color.blue, 0.0166f);
        }

        [HideInInspector] public Vector2 WasSpawnedInDirection = Vector2.up;
        [HideInInspector] public bool KeepSpawningInDirection = false;
        [HideInInspector] public GameObject GrassFieldThatStartedSpawning = null;
        public void AddGrassField(Vector2 Direction)
        {
            if (VGSMC.InitializationFinished == true)
            {
                if (VGSMC.WrapAutomatically == true) VGSMC.CreatingGrassField = true;
                if (VGSMC.InitializationFinished == true) VGSMC.CreatingGrassFieldWithoutWrapping = true;
            }
            GrassFieldMaster grassShaderMaster = GetComponent<GrassFieldMaster>();
            string PrefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(GrassFieldBasePrefab);
            GrassFieldBasePrefab = AssetDatabase.LoadAssetAtPath(PrefabPath, typeof(Object));
            GameObject objectSpawned = PrefabUtility.InstantiatePrefab(GrassFieldBasePrefab, transform.parent) as GameObject;
            PrefabInstanceStatus prefabInstanceStatus = PrefabUtility.GetPrefabInstanceStatus(objectSpawned);
            if (prefabInstanceStatus == PrefabInstanceStatus.Connected) PrefabUtility.UnpackPrefabInstance(objectSpawned, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            Vector2 AddedPos = Direction * VGSMC.GrassFieldMaxDistance;
            
            float Height = transform.position.y;
            RaycastHit rayCastHit = new RaycastHit();
            bool rayHitBool = Physics.Raycast(transform.position + new Vector3(AddedPos.x, 0.0f, AddedPos.y) + Vector3.up * VGSMC.WrapBasePosAddedHeight, Vector3.down, out rayCastHit,
                VGSMC.WrapMaxRayDistance, VGSMC.GroundLayers, QueryTriggerInteraction.Ignore);
            if (rayHitBool == true) Height = rayCastHit.point.y;
            objectSpawned.transform.position = new Vector3(transform.position.x + AddedPos.x, Height, transform.position.z + AddedPos.y);
            GrassFieldMaster OriginalGSM = GetComponent<GrassFieldMaster>();
            GrassFieldMaster SpawnedGSM = objectSpawned.GetComponent<GrassFieldMaster>();
            GrassFieldCreator SpawnedGFCreator = objectSpawned.GetComponent<GrassFieldCreator>();
            SpawnedGSM.OldLocalPosition = objectSpawned.transform.localPosition;
            SpawnedGSM.OldLocalEulerAngles = objectSpawned.transform.localEulerAngles;
            SpawnedGSM.OldLocalScale = objectSpawned.transform.localScale;
            SpawnedGFCreator.GrassFieldBasePrefab = GrassFieldBasePrefab;
            SpawnedGFCreator.WasSpawnedInDirection = Direction;
            if (GrassFieldThatStartedSpawning != null && VGSMC.SpawnsLeftDict.ContainsKey(GrassFieldThatStartedSpawning) == true)
            {
                if (VGSMC.SpawnsLeftDict[GrassFieldThatStartedSpawning] > 0)
                {
                    VGSMC.SpawnsLeftDict[GrassFieldThatStartedSpawning] -= 1;
                    if (VGSMC.SpawnsLeftDict[GrassFieldThatStartedSpawning] > 0)
                    {
                        SpawnedGFCreator.KeepSpawningInDirection = true;
                        SpawnedGFCreator.GrassFieldThatStartedSpawning = GrassFieldThatStartedSpawning;
                    }
                    else VGSMC.SpawnsLeftDict.Remove(GrassFieldThatStartedSpawning);
                }
            }
            if (VGSMC.WrapAutomatically == true) SpawnedGSM.WrapEvent?.Invoke();
            Transform transformFound;
            int HighestNumberFound = hierarchyOrganizationTools.GetHighestNumberInChildren(transform.parent, out transformFound);
            string Result = transformFound.gameObject.name.Replace(HighestNumberFound.ToString(), (HighestNumberFound + 1).ToString());
            objectSpawned.name = Result;
        }
        private int FrameCount;
        private WrapMaster SVGWrapMaster;
        private bool IntersectionTransparencyOld;
        private void EnableAndDisableArrowsMethod()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode == false)
            {
                if (VGSMC.CreatingGrass == true && VGSMC.Wrapping == false && VGSMC.WrapMeshes == false)
                {
                    if (ControlArrows.Count < 4)
                    {
                        ControlArrows.Clear();
                        string PrefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(ControlArrowPrefab);
                        ControlArrowPrefab = AssetDatabase.LoadAssetAtPath(PrefabPath, typeof(Object));
                        int j = 0;
                        while (j <= 3)
                        {
                            Vector2 direction = Vector2.zero;
                            if (j == 0) direction = Vector2.down;
                            if (j == 1) direction = Vector2.left;
                            if (j == 2) direction = Vector2.up;
                            if (j == 3) direction = Vector2.right;
                            Vector3 directionResult = transform.TransformDirection(new Vector3(direction.x, 0.0f, direction.y)).normalized;
                            GameObject ArrowSpawned = PrefabUtility.InstantiatePrefab(ControlArrowPrefab, transform) as GameObject;
                            PrefabInstanceStatus prefabInstanceStatus = PrefabUtility.GetPrefabInstanceStatus(ArrowSpawned);
                            if (prefabInstanceStatus == PrefabInstanceStatus.Connected) PrefabUtility.UnpackPrefabInstance(ArrowSpawned, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
                            ControlArrows shiniesVibrantGrassControlArrows = ArrowSpawned.GetComponent<ControlArrows>();
                            shiniesVibrantGrassControlArrows.GrassFieldBase = gameObject;
                            Vector3 basePos = transform.position;
                            Vector3 RayBasePos = transform.position + Vector3.up * VGSMC.WrapBasePosAddedHeight;
                            RaycastHit raycastHit = new RaycastHit();
                            bool rayHit = false;
                            RaycastHit[] allRayHits = Physics.RaycastAll(RayBasePos, Vector3.down, VGSMC.WrapMaxRayDistance, SVGWrapMaster.GrassGroundLayers, QueryTriggerInteraction.Ignore);
                            for (int k = 0; k < allRayHits.Length; k++)
                            {
                                raycastHit = allRayHits[k];
                                rayHit = true;
                            }
                            if (rayHit == true) basePos = raycastHit.point;
                            Vector3 ArrowPosition = basePos + (Vector3.up * ControlArrowsHeight) + (directionResult * ControlArrowsDistance);
                            ArrowSpawned.transform.localScale = Vector3.one * ControlArrowsScale;
                            ArrowSpawned.transform.position = ArrowPosition;
                            ArrowSpawned.transform.LookAt(ArrowSpawned.transform.position + directionResult * 10.0f);
                            shiniesVibrantGrassControlArrows.ArrowDirection = new Vector2(directionResult.x, directionResult.z);
                            ControlArrows.Add(ArrowSpawned);
                            j += 1;
                        }
                    }
                    FrameCount = 0;
                    DisabledByIntersectionTransparency = false;
                }
            }
            if (VGSMC.CreatingGrass == false || VGSMC.Wrapping == true && ControlArrows.Count == 4)
            {
                if (FrameCount == 2)//Wait for a frame so the ControlArrow is Initialized 
                { DisableControlsEvent?.Invoke(); }
                FrameCount += 1;
            }
            if (DisabledByIntersectionTransparency == true)
            {
                if (DisableByIntersectTranspFrameCount >= 10) VGSMC.CreatingGrass = true;
                DisableByIntersectTranspFrameCount += 1;
            }
            if (VGSMC.EnableTransparency != IntersectionTransparencyOld)
            {
                if (VGSMC.CreatingGrass == true)
                {
                    VGSMC.CreatingGrass = false;
                    DisabledByIntersectionTransparency = true;
                }
                DisableByIntersectTranspFrameCount = 0;
            }
            IntersectionTransparencyOld = VGSMC.EnableTransparency;
        }
        private bool DisabledByIntersectionTransparency;
        private int DisableByIntersectTranspFrameCount;

        private void DisableControlsMethod()
        {
            ControlArrows.Clear();
        }
#endif
    }
}