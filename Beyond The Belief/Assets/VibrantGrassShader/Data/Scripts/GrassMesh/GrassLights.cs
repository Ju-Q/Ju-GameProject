using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using VibrantGrassShaderTools;

namespace VibrantGrassShader
{
    public class GrassLights : MonoBehaviour
    {
        [Foldout("Data (Don't Touch)", true)]
        [SerializeField] private GameObject GrassLandShaderMasterObject = null;

#if UNITY_EDITOR
        [SerializeField] private bool DebugSplatMap = false;
#endif

        private RenderTexture _LightSplatMap;
        private GrassFieldMaster GSM;
        private LightAndInteractionMaster VGSLIM;
        private LightAndInteractObjectsDetection VGSLIDetection;
        private MainControls VGSMC;
        private GrassMeshMaster grassMeshMaster;


        private List<RenderTexture> RTsToReleaseOnVGSMCDisable = new List<RenderTexture>();
        private bool FirstFrameTrigger;

        void Start()
        {
            GSM = GrassLandShaderMasterObject.GetComponent<GrassFieldMaster>();
            VGSMC = GSM.VGSMC;
            VGSLIM = VGSMC.gameObject.GetComponent<LightAndInteractionMaster>();
            VGSLIDetection = GetComponent<LightAndInteractObjectsDetection>();
            grassMeshMaster = GSM.GrassMeshObject.GetComponent<GrassMeshMaster>();

            VGSMC.DisablingVGSMCPlayMode += ReleaseTextures;
            VGSMC.ReEnablingVGSMCPlayMode += CreateRTIfNecessary;
            grassMeshMaster.TranspSwitch_Event += AssignLightTexture;
            FirstFrameTrigger = false;
        }

        private void OnEnable()
        {
            if (GSM == null) GSM = GrassLandShaderMasterObject.GetComponent<GrassFieldMaster>();
            if (VGSMC == null) VGSMC = GSM.VGSMC;
            if (VGSLIM == null) VGSLIM = VGSMC.gameObject.GetComponent<LightAndInteractionMaster>();
            if (VGSLIDetection == null) VGSLIDetection = GetComponent<LightAndInteractObjectsDetection>();
            AssignTextureTrigger1 = false;
            VGSLIM.DrawLightAndInteractionEvent += PaintingSystem;
            VGSLIM.LightAndInteractWasJustDisabledEvent += EraseEverythingMethod;
            VGSLIDetection.DisableAllLightFirstFrameEvent += EraseEverythingMethod;
        }

        private void OnDisable()
        {
            if (GSM == null) GSM = GrassLandShaderMasterObject.GetComponent<GrassFieldMaster>();
            if (VGSMC == null) VGSMC = GSM.VGSMC;
            if (VGSLIM == null) VGSLIM = VGSMC.gameObject.GetComponent<LightAndInteractionMaster>();
            VGSLIM.DrawLightAndInteractionEvent -= PaintingSystem;
            VGSLIM.LightAndInteractWasJustDisabledEvent -= EraseEverythingMethod;
            VGSLIDetection.DisableAllLightFirstFrameEvent -= EraseEverythingMethod;
            ReleaseTextures();
        }

        private void ReleaseTextures()
        {
            if (Application.isPlaying == true && RTsToReleaseOnVGSMCDisable.Count > 0)
            {
                RenderTexture.active = null;
                for (int i = 0; i < RTsToReleaseOnVGSMCDisable.Count; i++)
                { if (RTsToReleaseOnVGSMCDisable[i] != null) RTsToReleaseOnVGSMCDisable[i].Release(); }
                RTsToReleaseOnVGSMCDisable.Clear();
            }
        }

        private void Update()
        {
            if (FirstFrameTrigger == false)
            {
                CreateRTIfNecessary();
                FirstFrameTrigger = true;
            }
        }

        private void CreateRTIfNecessary()
        {
            if (_LightSplatMap == null)
            {
                _LightSplatMap = new RenderTexture(VGSMC.LightTextureSize, VGSMC.LightTextureSize, 0, RenderTextureFormat.ARGBFloat);
                RenderTexture BlackTextureTemp = RenderTexture.GetTemporary(_LightSplatMap.width, _LightSplatMap.height, 0, RenderTextureFormat.ARGBFloat);
                Graphics.Blit(BlackTextureTemp, _LightSplatMap);//KEEP THIS LINE HERE, otherwise you get multiple materials using the same texture (no idea why) 
                RenderTexture.ReleaseTemporary(BlackTextureTemp);
                if (RTsToReleaseOnVGSMCDisable.Contains(_LightSplatMap) == false) RTsToReleaseOnVGSMCDisable.Add(_LightSplatMap);
            }
        }

        private void EraseEverythingMethod()
        {
            if (_LightSplatMap != null) DrawLight(true);
        }

        private void PaintingSystem()
        {
            if (VGSLIDetection.DetectedLightObject == true)
            {
                TexCoordsList = new List<Vector4>();
                ColorList = new List<Color>();
                SizeList = new List<float>();
                StrengthList = new List<float>();
                foreach (KeyValuePair<GameObject, LightAndInteractObjectsDetection.VGSLIMainValues> item in VGSLIM.LightAndInteractObjects)
                {
                    LightAndInteractObjectsDetection.VGSLIMainValues VGSLIMainValues = item.Value;
                    DynamicLightAndInteract VGSLI = VGSLIMainValues.VGSLightAndInteract;
                    LightAndInteractObjectsDetection.VGSLIGrassFieldValues PerGrassFieldValues = VGSLIM.LightAndInteractObjects[item.Key].PerGrassFieldValues[GrassLandShaderMasterObject];
                    if (VGSLIMainValues.GOUsable == true && VGSLI.EnableLight == true
                        && PerGrassFieldValues.LightHeightHit == true) FillTheLists(item.Key, VGSLIMainValues, VGSLI, PerGrassFieldValues);
                }
                if (AssignTextureTrigger1 == false)
                {
                    AssignLightTexture();
                    AssignTextureTrigger1 = true;
                }
                if (_LightSplatMap != null) DrawLight(false);
            }
        }

        private bool AssignTextureTrigger1;
        private void AssignLightTexture()
        {
            GSM.SVGMeshMaster.MainMatInstanced.SetTexture("_LightSplatTexture", _LightSplatMap);
        }

        private List<Vector4> TexCoordsList = new List<Vector4>();
        private List<Color> ColorList = new List<Color>();
        private List<float> SizeList = new List<float>(), StrengthList = new List<float>();
        private void FillTheLists(GameObject _KeyObject, LightAndInteractObjectsDetection.VGSLIMainValues SVGLIMainValues,
            DynamicLightAndInteract VGSLI, LightAndInteractObjectsDetection.VGSLIGrassFieldValues PerGrassFieldValues)
        {
            TexCoordsList.Add(PerGrassFieldValues.TexCoord);
            ColorList.Add(VGSLI.LightColor);
            SizeList.Add(VGSLI.LightSize);
            float HeightInvLerp = Mathf.InverseLerp(VGSLI.LightHeightInvLerpValuesAB.x, VGSLI.LightHeightInvLerpValuesAB.y, Mathf.Abs(SVGLIMainValues.LastDetectedHeight));
            float StrengthApplied = 1 - Mathf.Clamp(HeightInvLerp, 0.0f, 1.0f);
            StrengthList.Add(StrengthApplied);
        }

        private void DrawLight(bool Erase)
        {
            if (VGSLIM._LightDrawMaterial != null)
            {
                RenderTexture tempTexture = RenderTexture.GetTemporary(_LightSplatMap.width, _LightSplatMap.height, 0, RenderTextureFormat.ARGBFloat);
                if (TexCoordsList.Count <= 0 || Erase == true)
                {
                    TexCoordsList = new List<Vector4>();
                    ColorList = new List<Color>();
                    SizeList = new List<float>();
                    StrengthList = new List<float>();
                    TexCoordsList.Add(Vector4.zero);
                    ColorList.Add(Color.black);
                    SizeList.Add(0.0f);
                    StrengthList.Add(0.0f);
                }
                if (TexCoordsList.Count < VGSMC.MaxLightPerGrassField)
                {
                    VGSLIM._LightDrawMaterial.SetVectorArray("_Coordinate", TexCoordsList);
                    VGSLIM._LightDrawMaterial.SetColorArray("_Color", ColorList);
                    VGSLIM._LightDrawMaterial.SetFloatArray("_Size", SizeList);
                    VGSLIM._LightDrawMaterial.SetFloatArray("_Strength", StrengthList);
                    VGSLIM._LightDrawMaterial.SetInt("_ArraysLength", TexCoordsList.Count);
                    Graphics.Blit(tempTexture, _LightSplatMap, VGSLIM._LightDrawMaterial);
                    RenderTexture.ReleaseTemporary(tempTexture);
                }
            }
        }

#if UNITY_EDITOR
        private void OnGUI()
        {
            if (DebugSplatMap == true)
            {
                GUI.DrawTexture(new Rect(0, 0, 256, 256), _LightSplatMap, ScaleMode.ScaleToFit, false, 1);
            }
        }
#endif
    }
}