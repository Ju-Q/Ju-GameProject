using System.Collections.Generic;
using UnityEngine;
using VibrantGrassShaderTools;

namespace VibrantGrassShader
{
    public class GrassInteraction : MonoBehaviour
    {
        [Foldout("Data (Don't Touch)", true)]
        [SerializeField] private GameObject GrassShaderMasterObject = null;
#if UNITY_EDITOR
        [SerializeField] private bool DebugSplatMap = false;
#endif
        private RenderTexture _NormalSplatMap;
        private GrassFieldMaster GSM;
        private LightAndInteractionMaster SVGLIM;
        private MainControls VGSMC;
        private LightAndInteractObjectsDetection VGSLIDetection;
        private GrassMeshMaster grassMeshMaster;

        private float TimeCountAfterObjectsGone, TimeBetweenPaint;
        private bool FirstFrameTrigger;

        void Start()
        {
            //GSM = GrassShaderMaster
            GSM = GrassShaderMasterObject.GetComponent<GrassFieldMaster>();
            SVGLIM = GSM.VGSMC.gameObject.GetComponent<LightAndInteractionMaster>();
            VGSMC = GrassShaderMasterObject.GetComponentInParent<MainControls>();
            VGSLIDetection = GetComponent<LightAndInteractObjectsDetection>();
            grassMeshMaster = GSM.GrassMeshObject.GetComponent<GrassMeshMaster>();
            TimeBetweenPaint = 0.0f;
            AssignTextureTrigger1 = false;
            FirstFrameTrigger = false;
            VGSMC.DisablingVGSMCPlayMode += ReleaseTextures;
            VGSMC.ReEnablingVGSMCPlayMode += CreateRTsIfNecessary;
            grassMeshMaster.TranspSwitch_Event += AssignInteractTexture;
            TimeCountAfterObjectsGone = VGSMC.InteractionMaximumErasingTime;
        }

        private void OnEnable()
        {
            if (VGSMC == null) VGSMC = GrassShaderMasterObject.GetComponentInParent<MainControls>();
            if (_NormalSplatMap != null)
            {
                if (VGSMC.InteractionErasingSpeed != 0.0f && SVGLIM._InteractDrawMaterial != null)
                {
                    RenderTexture tempSplatMapEraserTexture2 = RenderTexture.GetTemporary(_NormalSplatMap.width, _NormalSplatMap.height, 0, RenderTextureFormat.ARGBFloat);
                    SVGLIM._InteractDrawMaterial.SetFloat("_ErasingSpeed", 1.0f);
                    SVGLIM._InteractDrawMaterial.SetInt("_ArraysLength", 0);
                    Graphics.Blit(tempSplatMapEraserTexture2, _NormalSplatMap, SVGLIM._InteractDrawMaterial);
                    Graphics.Blit(_NormalSplatMap, OldNormalSplatMap);
                    RenderTexture.ReleaseTemporary(tempSplatMapEraserTexture2);
                }
                AssignInteractTexture();
            }
            FinishedErasing = false;
            AssignTextureTrigger1 = false;
            if (GSM == null) GSM = GrassShaderMasterObject.GetComponent<GrassFieldMaster>();
            if (SVGLIM == null) SVGLIM = GSM.VGSMC.gameObject.GetComponent<LightAndInteractionMaster>();
            SVGLIM.DrawLightAndInteractionEvent += PaintingSystem;
            SVGLIM.LightAndInteractWasJustDisabledEvent += DisableInteractions;
        }

        private void OnDisable()
        {
            if (GSM == null) GSM = GrassShaderMasterObject.GetComponent<GrassFieldMaster>();
            if (SVGLIM == null) SVGLIM = GSM.VGSMC.gameObject.GetComponent<LightAndInteractionMaster>();
            SVGLIM.DrawLightAndInteractionEvent -= PaintingSystem;
            SVGLIM.LightAndInteractWasJustDisabledEvent -= DisableInteractions;
            ReleaseTextures();
        }

        void Update()
        {
            if (FirstFrameTrigger == false)
            {
                CreateRTsIfNecessary();
                FirstFrameTrigger = true;
            }
            TimeBetweenPaint += Time.deltaTime;
        }

        private void CreateRTsIfNecessary()
        {
            if (this != null && _NormalSplatMap == null && SVGLIM._InteractDrawMaterial != null)
            {
                _NormalSplatMap = new RenderTexture(VGSMC.InteractTextureSize, VGSMC.InteractTextureSize, 0, RenderTextureFormat.ARGBFloat);
                RenderTexture tempSplatMapEraserTexture1 = RenderTexture.GetTemporary(_NormalSplatMap.width, _NormalSplatMap.height, 0, RenderTextureFormat.ARGBFloat);
                SVGLIM._InteractDrawMaterial.SetFloat("_ErasingSpeed", 1.0f);
                SVGLIM._InteractDrawMaterial.SetInt("_ArraysLength", 0);
                Graphics.Blit(tempSplatMapEraserTexture1, _NormalSplatMap, SVGLIM._InteractDrawMaterial);
                RenderTexture.ReleaseTemporary(tempSplatMapEraserTexture1);
                OldNormalSplatMap = new RenderTexture(_NormalSplatMap.width, _NormalSplatMap.height, 0, RenderTextureFormat.ARGBFloat);
            }
        }

        private bool FinishedErasing, InteractDeactivatedTrigger1;
        private void PaintingSystem()
        {
            bool InteractActivated = false;
            TexCoordsList = new List<Vector4>();
            SizeList = new List<float>();
            StrengthList = new List<float>();
            if (VGSLIDetection.DetectedInteractObject == true)
            {
                foreach (KeyValuePair<GameObject, LightAndInteractObjectsDetection.VGSLIMainValues> item in SVGLIM.LightAndInteractObjects)
                {
                    LightAndInteractObjectsDetection.VGSLIMainValues VGSLIMainValues = item.Value;
                    DynamicLightAndInteract VGSLI = VGSLIMainValues.VGSLightAndInteract;
                    if (VGSLI.EnableInteraction == true && VGSLIMainValues.GOUsable == true)
                    {
                        LightAndInteractObjectsDetection.VGSLIGrassFieldValues PerGrassFieldValues = VGSLIMainValues.PerGrassFieldValues[GrassShaderMasterObject];
                        if (PerGrassFieldValues.InteractHeightHit == true)
                        {
                            GameObject KeyObject = item.Key;
                            InteractActivated = true;
                            FillTheLists(KeyObject, VGSLI, VGSLIMainValues, PerGrassFieldValues);
                        }
                        else VGSLI.InteractAudioHeightMultiplierDict[gameObject] = 0.0f;
                    }
                }
            }
            if (InteractActivated == true)
            {
                if (GSM.SVGMeshMaster.MainMatInstanced != null) GSM.SVGMeshMaster.MainMatInstanced.SetInt("_InteractTextureAssigned_Data_Don_t_change", 1);
                FinishedErasing = false;
                TimeCountAfterObjectsGone = 0.0f;
                if (AssignTextureTrigger1 == false)
                {
                    AssignInteractTexture();
                    AssignTextureTrigger1 = true;
                }
                if (TexCoordsList.Count > 0) DrawInteraction();
                InteractDeactivatedTrigger1 = true;
            }
            else
            {
                if (InteractDeactivatedTrigger1 == true)
                {
                    DisableInteractions();
                    InteractDeactivatedTrigger1 = false;
                }
                if (VGSMC.InteractionErasingSpeed > 0.0f && SVGLIM._InteractDrawMaterial != null)
                {
                    if (TimeCountAfterObjectsGone < VGSMC.InteractionMaximumErasingTime)
                    {
                        FinishedErasing = false;
                        SVGLIM._InteractDrawMaterial.SetFloat("_ErasingSpeed", Mathf.Clamp01(VGSMC.InteractionErasingSpeed * TimeBetweenPaint));
                        SVGLIM._InteractDrawMaterial.SetInt("_ArraysLength", 0);
                        Graphics.Blit(OldNormalSplatMap, _NormalSplatMap, SVGLIM._InteractDrawMaterial);
                        Graphics.Blit(_NormalSplatMap, OldNormalSplatMap);
                    }
                    else
                    {
                        //Erase
                        if (FinishedErasing == false)
                        {
                            SVGLIM._InteractDrawMaterial.SetFloat("_ErasingSpeed", 1.0f);
                            SVGLIM._InteractDrawMaterial.SetInt("_ArraysLength", 0);
                            Graphics.Blit(OldNormalSplatMap, _NormalSplatMap, SVGLIM._InteractDrawMaterial);
                            Graphics.Blit(_NormalSplatMap, OldNormalSplatMap);
                            FinishedErasing = true;
                            RenderTexture.active = null;
                            ReleaseTextures();
                        }
                    }
                    TimeCountAfterObjectsGone += Time.fixedDeltaTime;
                }
            }
            TimeBetweenPaint = 0.0f;

        }
        private bool AssignTextureTrigger1;

        private void ReleaseTextures()
        {
            if (GSM.SVGMeshMaster.MainMatInstanced != null) GSM.SVGMeshMaster.MainMatInstanced.SetInt("_InteractTextureAssigned_Data_Don_t_change", 0);
            RenderTexture.active = null;
            if (_NormalSplatMap != null) _NormalSplatMap.Release();
            if (_NormalSplatMap != null) OldNormalSplatMap.Release();
            TimeCountAfterObjectsGone = VGSMC.InteractionMaximumErasingTime;
        }

        private void DisableInteractions()
        {
            foreach (KeyValuePair<GameObject, LightAndInteractObjectsDetection.VGSLIMainValues> item in SVGLIM.LightAndInteractObjects)
            {
                DynamicLightAndInteract VGSLI = item.Value.VGSLightAndInteract;
                if (VGSLI.InteractAudioHeightMultiplierDict.ContainsKey(gameObject) == true) VGSLI.InteractAudioHeightMultiplierDict[gameObject] = 0.0f;
            }
        }

        private void AssignInteractTexture()
        {
            if (GSM.SVGMeshMaster.MainMatInstanced != null) GSM.SVGMeshMaster.MainMatInstanced.SetTexture("_NormalSplatTexture", _NormalSplatMap);
        }


        private List<Vector4> TexCoordsList = new List<Vector4>();
        private List<float> SizeList = new List<float>(), StrengthList = new List<float>();
        private void FillTheLists(GameObject _KeyObject, DynamicLightAndInteract VGSLI, LightAndInteractObjectsDetection.VGSLIMainValues SVGLIValues, LightAndInteractObjectsDetection.VGSLIGrassFieldValues PerGrassFieldValues)
        {
            TexCoordsList.Add(PerGrassFieldValues.TexCoord);
            SizeList.Add(VGSLI.InteractSizeApplied);
            float AudioHeightMultiplierFound = 0.0f;
            float CurrentGrassHeight = 0.85f;
            if (GSM.HeightTexture != null)
            {
                Vector2 TexCoordTexCollider = PerGrassFieldValues.TexCoord;
                CurrentGrassHeight = GSM.HeightTexture.GetPixel(Mathf.FloorToInt((TexCoordTexCollider.x) * GSM.HeightTexture.width),
                    Mathf.FloorToInt((TexCoordTexCollider.y) * GSM.HeightTexture.height)).r;
            }
            //Interaction Movement 
            float GOHeightToCheck = SVGLIValues.LastDetectedHeight + VGSLI.InteractObjectAddedHeight;
            float GrassHeightAboveObject = Mathf.Max(0.0f, CurrentGrassHeight - GOHeightToCheck);
            float StrengthLerped = VGSLI.InteractStrengthApplied * Mathf.Lerp(0.2f, 1.0f, Mathf.Clamp01(Mathf.InverseLerp(VGSLI.InteractGrassHeightAboveObjectInvLerpValuesAB.x, VGSLI.InteractGrassHeightAboveObjectInvLerpValuesAB.y, GrassHeightAboveObject)));
            float SmoothRef = PerGrassFieldValues.InteractStrengthSmoothRef;
            if (GOHeightToCheck > CurrentGrassHeight) StrengthLerped = 0.0f;
            PerGrassFieldValues.InteractStrengthResult = Mathf.SmoothDamp(PerGrassFieldValues.InteractStrengthResult, StrengthLerped, ref SmoothRef, 0.01f);
            PerGrassFieldValues.InteractStrengthSmoothRef = SmoothRef;
            //Interaction Audio 
            AudioHeightMultiplierFound = Mathf.Clamp01(Mathf.InverseLerp(VGSLI.InteractAudioGrassHeightInvLerpValuesAB.x,
                VGSLI.InteractAudioGrassHeightInvLerpValuesAB.y, GrassHeightAboveObject));
            float StrengthResult = PerGrassFieldValues.InteractStrengthResult * Mathf.Clamp01(TimeBetweenPaint);
            if (SVGLIValues.GOBelowMaximumHeight == false)
            {
                StrengthResult = 0.0f;
                AudioHeightMultiplierFound = 0.0f;
            }
            StrengthList.Add(StrengthResult);
            //InteractObject Values
            float AudioHeightMultiplier = AudioHeightMultiplierFound;
            if (PerGrassFieldValues.InteractHeightHit == false
                    || PerGrassFieldValues.TexCoord.x < 0.0f || PerGrassFieldValues.TexCoord.x > 1.0f
                    || PerGrassFieldValues.TexCoord.y < 0.0f || PerGrassFieldValues.TexCoord.y > 1.0f) AudioHeightMultiplier = 0.0f;
            if (VGSLI.InteractAudioHeightMultiplierDict.ContainsKey(gameObject) == false)
            { VGSLI.InteractAudioHeightMultiplierDict.Add(gameObject, AudioHeightMultiplier); }
            else VGSLI.InteractAudioHeightMultiplierDict[gameObject] = AudioHeightMultiplier;
        }

        private RenderTexture OldNormalSplatMap;
        private void DrawInteraction()
        {
            if (SVGLIM._InteractDrawMaterial != null && TexCoordsList.Count < VGSMC.MaxLightPerGrassField)
            {
                SVGLIM._InteractDrawMaterial.SetFloat("_ErasingSpeed", Mathf.Clamp01(VGSMC.InteractionErasingSpeed * TimeBetweenPaint));
                SVGLIM._InteractDrawMaterial.SetVectorArray("_Coordinate", TexCoordsList);
                SVGLIM._InteractDrawMaterial.SetFloatArray("_Size", SizeList);
                SVGLIM._InteractDrawMaterial.SetFloatArray("_Strength", StrengthList);
                SVGLIM._InteractDrawMaterial.SetInt("_ArraysLength", TexCoordsList.Count);
                Graphics.Blit(OldNormalSplatMap, _NormalSplatMap, SVGLIM._InteractDrawMaterial);
                Graphics.Blit(_NormalSplatMap, OldNormalSplatMap);
            }
        }

#if UNITY_EDITOR
        private void OnGUI()
        {
            if (DebugSplatMap == true)
            {
                GUI.DrawTexture(new Rect(0, 0, 256, 256), _NormalSplatMap, ScaleMode.ScaleToFit, false, 1);
                //GUI.DrawTexture(new Rect(0, 0, 256, 256), OldNormalSplatMap, ScaleMode.ScaleToFit, false, 1);
            }
        }
#endif
    }
}