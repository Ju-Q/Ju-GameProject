using UnityEngine;
using VibrantGrassShaderTools;
using System.Collections.Generic;


namespace VibrantGrassShader
{
    public class OutOfSightDisabler : MonoBehaviour
    {
        [SerializeField, Foldout("Don't Touch", true)] private Object VisibilityMeshPrefab = null;
        private MainControls VGSMC;
        [SerializeField, HideInInspector] public GOAndVGSDistValuesSerializableDictionary DistFadeOutValuesDict = new GOAndVGSDistValuesSerializableDictionary();
        private Camera camComponent;

        void Start()
        {
            VGSMC = GetComponent<MainControls>();
            DistanceFadeOutTrigger1 = false;
            FrameCount = 0;
        }

        private int FrameCount;
        void Update()
        {
            if (FrameCount >= 5)
            {
                for (int i = 0; i < VGSMC.GrassFieldList.Count; i++)
                {
                    GameObject CurrentGO = VGSMC.GrassFieldList[i];
                    if (CurrentGO != null)
                    {
                        if (DistFadeOutValuesDict[CurrentGO].VisibilityGO == null
                            && DistFadeOutValuesDict[CurrentGO].meshFilter != null)
                        {
                            SpawnVisibilityMesh(CurrentGO.transform, DistFadeOutValuesDict[CurrentGO].meshFilter.mesh,
                                   DistFadeOutValuesDict[CurrentGO].meshRenderer.sharedMaterial, DistFadeOutValuesDict[CurrentGO], CurrentGO.name + "_VisibilityMesh");
                        }
                    }
                }
                DistanceFadeOutMethod();
            }
            FrameCount += 1;
        }
        private void SpawnVisibilityMesh(Transform transform, Mesh mesh, Material material, VibrantGrassShaderDistanceFadeOutValues VGSDistFadeOutValues, string name)
        {
            GameObject GOSpawned = Instantiate(VisibilityMeshPrefab) as GameObject;
            MeshFilter SpawnedMeshFilter = GOSpawned.AddComponent<MeshFilter>();
            GOSpawned.transform.parent = VGSMC.transform;
            GOSpawned.transform.position = transform.position;
            GOSpawned.transform.localRotation = transform.localRotation;
            GOSpawned.transform.localScale = transform.localScale;
            GOSpawned.name = name;
            MeshRenderer SpawnedMeshRenderer = GOSpawned.GetComponent<MeshRenderer>();
            SpawnedMeshFilter.mesh = mesh;
            SpawnedMeshRenderer.sharedMaterial = material;
            VGSDistFadeOutValues.VisibilityGO = GOSpawned;
            VGSDistFadeOutValues.VisibilityMeshRenderer = SpawnedMeshRenderer;
            GOSpawned.SetActive(false);
        }

        private bool DistanceFadeOutTrigger1;
        private void DistanceFadeOutMethod()
        {
            GameObject camGO = VGSMC._camera;
            if (camComponent == null) camComponent = camGO.GetComponent<Camera>();
            Plane[] camPlanes = GeometryUtility.CalculateFrustumPlanes(camComponent);
            Vector3 CamPosCur = camGO.transform.position;

            for (int i = 0; i < VGSMC.GrassFieldList.Count; i++)
            {
                GameObject grassFieldGO = VGSMC.GrassFieldList[i];
                if (DistFadeOutValuesDict[grassFieldGO].meshRenderer != null)
                {
                    VibrantGrassShaderDistanceFadeOutValues VGSDistValues = DistFadeOutValuesDict[grassFieldGO];
                    bool ViewFrustumResult = false;
                    if (VGSMC.EnableDistanceFadeTransparency == true || VGSMC.EnableDistanceFadeHeight == true)
                    {
                        VGSDistValues.GOEnableInDistance = true;
                        ViewFrustumResult = VGSDistValues.GOInViewFrustum;
                    }
                    if (VGSDistValues.GOEnabledBySight == true
                        && VGSDistValues.meshRenderer.isVisible == false
                        && VGSDistValues.GOEnableInDistance == true
                        && ViewFrustumResult == false
                        && VGSDistValues.FrameCountAfterSwitch > 1)
                    {
                        grassFieldGO.SetActive(false);
                        VGSDistValues.VisibilityGO.SetActive(true);
                        VGSDistValues.GOEnabledBySight = false;
                    }
                    if (VGSDistValues.GOEnabledBySight == false
                        && VGSDistValues.VisibilityMeshRenderer.isVisible == true)
                    {
                        VGSDistValues.FrameCountAfterSwitch = 0;
                        VGSDistValues.GOEnabledBySight = true;
                        grassFieldGO.SetActive(true);
                        VGSDistValues.VisibilityGO.SetActive(false);
                    }
                    VGSDistValues.FrameCountAfterSwitch += 1;
                }
            }

            if (VGSMC.EnableDistanceFadeTransparency == true || VGSMC.EnableDistanceFadeHeight == true)
            {
                for (int i = 0; i < VGSMC.GrassFieldList.Count; i++)
                {
                    GameObject grassFieldGO = VGSMC.GrassFieldList[i];
                    VibrantGrassShaderDistanceFadeOutValues VGSDistValues = DistFadeOutValuesDict[grassFieldGO];
                    if (GeometryUtility.TestPlanesAABB(camPlanes, VGSDistValues.meshRenderer.bounds))
                    { VGSDistValues.GOInViewFrustum = true; }
                    else VGSDistValues.GOInViewFrustum = false;
                    if (VGSDistValues.GOInViewFrustum == true)
                    {
                        Vector3[] FurthestVerticesLocalPositions = VGSDistValues.FurthestVerticesLocalPositions;
                        if (camGO != null && VGSMC.DistanceFadeOutMinMax.y > 0.0f)
                        {
                            bool InRange = false;
                            for (int j = 0; j < FurthestVerticesLocalPositions.Length; j++)
                            {
                                Vector3 VertexWorldPos = grassFieldGO.transform.TransformPoint(FurthestVerticesLocalPositions[j]);
                                Vector2 FlatCamPos = new Vector2(CamPosCur.x, CamPosCur.z);
                                Vector2 FlatVertexPos = new Vector2(VertexWorldPos.x, VertexWorldPos.z);
                                float distanceToCam = Vector2.Distance(FlatCamPos, FlatVertexPos);
                                if (distanceToCam < VGSMC.DistanceFadeOutMinMax.y) InRange = true;
                            }
                            if (InRange == true)
                            {
                                if (grassFieldGO.activeSelf == false) grassFieldGO.SetActive(true);
                                VGSDistValues.GOEnableInDistance = true;
                            }
                            else
                            {
                                if (grassFieldGO.activeSelf == true) grassFieldGO.SetActive(false);
                                VGSDistValues.GOEnableInDistance = false;
                            }
                        }
                    }
                    else
                    {
                        VGSDistValues.GOEnableInDistance = true;
                    }
                }
                DistanceFadeOutTrigger1 = true;
            }
            if (VGSMC.EnableDistanceFadeTransparency == false && VGSMC.EnableDistanceFadeHeight == false)
            {
                if (DistanceFadeOutTrigger1 == true)
                {
                    for (int i = 0; i < VGSMC.GrassFieldList.Count; i++)
                    {
                        GameObject grassFieldGO = VGSMC.GrassFieldList[i];
                        if (DistFadeOutValuesDict[grassFieldGO].GOInViewFrustum == true)
                        {
                            if (grassFieldGO.activeSelf == false) grassFieldGO.SetActive(true);
                        }
                    }
                    DistanceFadeOutTrigger1 = false;
                }
            }
        }
    }

    [System.Serializable]
    public class VibrantGrassShaderDistanceFadeOutValues
    {
        public Vector3[] FurthestVerticesLocalPositions;
        public MeshRenderer meshRenderer, VisibilityMeshRenderer;
        public MeshFilter meshFilter;
        public GameObject VisibilityGO;
        public bool GOEnabledBySight, GOEnableInDistance, GOInViewFrustum;
        public int FrameCountAfterSwitch;
    }
}