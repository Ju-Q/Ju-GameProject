using UnityEngine;
using VibrantGrassShaderTools;

namespace VibrantGrassShader
{
    public class MeshDistanceFadeOut : MonoBehaviour
    {
        [Foldout("Data (Don't Touch)")]
        [SerializeField] private GameObject GrassShaderMasterObject = null;

        private string GrassTransparency;
        //private bool MaterialSwitchTrigger1, MaterialSwitchTrigger2;

        private GrassInteraction _grassShaderFloof;
        private GrassLights _grassShaderLights;
        private GrassMeshMaster _grassShaderMeshMaster;
        private GrassFieldMaster GSM;
        private MainControls SVGMC;
        private MeshRenderer _meshRenderer;
        private MeshFilter _meshFilter;
        private GrassWindAudioMaster SVGMCWindAudioMaster;
        private OutOfSightDisabler VGSDistFadeOutMaster;

        void Start()
        {
            GSM = GrassShaderMasterObject.GetComponent<GrassFieldMaster>();
            _meshRenderer = GetComponent<MeshRenderer>();
            _meshFilter = GetComponent<MeshFilter>();
            _grassShaderMeshMaster = GetComponent<GrassMeshMaster>();
            _grassShaderLights = GetComponent<GrassLights>();
            _grassShaderFloof = GetComponent<GrassInteraction>();
            SVGMC = GSM.MainControlObject.GetComponent<MainControls>();
            SVGMCWindAudioMaster = GSM.MainControlObject.GetComponent<GrassWindAudioMaster>();
            VGSDistFadeOutMaster = GSM.MainControlObject.GetComponent<OutOfSightDisabler>();
            FrameCount = 0;
            AddedToDistFadeOut = false;
        }

        private int FrameCount;
        private bool AddedToDistFadeOut;
        void Update()
        {
            if (FrameCount >= 1 && AddedToDistFadeOut == false)
            {
                if (VGSDistFadeOutMaster.DistFadeOutValuesDict.ContainsKey(GrassShaderMasterObject) == true)
                {
                    if (VGSDistFadeOutMaster.DistFadeOutValuesDict[GrassShaderMasterObject].meshRenderer == null) VGSDistFadeOutMaster.DistFadeOutValuesDict[GrassShaderMasterObject].meshRenderer = _meshRenderer;
                    if (VGSDistFadeOutMaster.DistFadeOutValuesDict[GrassShaderMasterObject].meshFilter == null) VGSDistFadeOutMaster.DistFadeOutValuesDict[GrassShaderMasterObject].meshFilter = _meshFilter;
                    VGSDistFadeOutMaster.DistFadeOutValuesDict[GrassShaderMasterObject].GOEnabledBySight = true;
                    VGSDistFadeOutMaster.DistFadeOutValuesDict[GrassShaderMasterObject].GOEnableInDistance = true;
                    AddedToDistFadeOut = true;
                }
            }
            DistanceFadeOutMethod();
            FrameCount += 1;
        }

        private void OnEnable()
        {
            FrameCount = 0;
            if (_meshRenderer == null) _meshRenderer = GetComponent<MeshRenderer>();
            _meshRenderer.enabled = false;
            DistFadeOutTrigger1 = false;
            if (_grassShaderMeshMaster == null) _grassShaderMeshMaster = GetComponent<GrassMeshMaster>();
            if (_grassShaderMeshMaster.MainMatInstanced != null) _grassShaderMeshMaster.MainMatInstanced.SetInt("_distanceFadePositionActivated", 1);
        }

        private bool DistFadeOutTrigger1;
        private void DistanceFadeOutMethod()
        {
            if (SVGMC._camera != null)
            {
                if (SVGMC.EnableDistanceFadeTransparency == true || SVGMC.EnableDistanceFadeHeight == true)
                {
                    if (SVGMC.EnableDistanceFadeTransparency == true) _grassShaderMeshMaster.MainMatInstanced.SetInt("_distanceFadeAlphaActivated", 1);
                    else _grassShaderMeshMaster.MainMatInstanced.SetInt("_distanceFadeAlphaActivated", 0);
                    if (SVGMC.EnableDistanceFadeHeight == true)
                    { _grassShaderMeshMaster.MainMatInstanced.SetInt("_distanceFadePositionActivated", 1); }
                    else _grassShaderMeshMaster.MainMatInstanced.SetInt("_distanceFadePositionActivated", 0);
                    _grassShaderMeshMaster.MainMatInstanced.SetVector("_cameraPosition", SVGMC._camera.transform.position);
                    _grassShaderMeshMaster.MainMatInstanced.SetFloat("_distanceFadeStart", SVGMC.DistanceFadeOutMinMax.x);
                    _grassShaderMeshMaster.MainMatInstanced.SetFloat("_distanceFadeEnd", SVGMC.DistanceFadeOutMinMax.y);

                    bool InRange = false;
                    for (int i = 0; i < VGSDistFadeOutMaster.DistFadeOutValuesDict[GrassShaderMasterObject].FurthestVerticesLocalPositions.Length; i++)
                    {
                        Vector3 VertexWorldPos = transform.TransformPoint(VGSDistFadeOutMaster.DistFadeOutValuesDict[GrassShaderMasterObject].FurthestVerticesLocalPositions[i]);
                        Vector2 FlatCamPos = new Vector2(SVGMC._camera.transform.position.x, SVGMC._camera.transform.position.z);
                        Vector2 FlatVertexPos = new Vector2(VertexWorldPos.x, VertexWorldPos.z);
                        float distanceToCam = Vector2.Distance(FlatCamPos, FlatVertexPos);
                        if (distanceToCam < SVGMC.DistanceFadeOutMinMax.y)
                        {
                            InRange = true;
                        }
                    }
                    if (InRange == true) _meshRenderer.enabled = true;
                    else _meshRenderer.enabled = false;
                    DistFadeOutTrigger1 = false;
                }
                if (SVGMC.EnableDistanceFadeTransparency == false && SVGMC.EnableDistanceFadeHeight == false)
                {
                    if (DistFadeOutTrigger1 == false)
                    {
                        _grassShaderMeshMaster.MainMatInstanced.SetInt("_distanceFadeAlphaActivated", 0);
                        _grassShaderMeshMaster.MainMatInstanced.SetInt("_distanceFadePositionActivated", 0);
                        if (SVGMC.IsUsedForInGameInstances == false) _meshRenderer.enabled = true;
                        DistFadeOutTrigger1 = true;
                    }
                }
            }
        }
    }
}