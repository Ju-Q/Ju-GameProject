using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VibrantGrassShaderTools;

namespace VibrantGrassShader
{
    [ExecuteInEditMode]
    public class GrassTextureDrawing : MonoBehaviour
    {
#pragma warning disable 0219
#pragma warning disable 0414

        [Foldout("Don't Touch", true)]
        [SerializeField]
        public Shader TextureCopierSmallToBig = null, DrawShader = null, TextureFillerShader = null,
            SoftenerShader = null, TextureBrushShader = null, TextureCopierBigToSmallShader = null, NormalizeProgressivelyShader = null, ApplyTextureAtCoordinateShader = null;
        [SerializeField] public Texture2D[] RoundBrushTexture = new Texture2D[0], SpreadBrushTexture = new Texture2D[0];
        [SerializeField] private int RevertChangesSize = 0;
        [SerializeField, HideInInspector] private Material DrawerMat, FillerMat, SoftenerMat, TextureBrushMat, NormalizeProgressivelyMat, TextureCopierSmallToBigMat;

        [SerializeField, HideInInspector] private MainControls VGSMC;
        [SerializeField, HideInInspector] public GameObject[] FarthestGrassFields;
        [SerializeField, HideInInspector] private Vector2[] FurthestPositions;
        [SerializeField, HideInInspector] public float XMaxDistanceAllFields, ZMaxDistanceAllFields;
        [SerializeField, HideInInspector] private int XLongestAmountOfGrassField, ZLongestAmountOfGrassField, TextureWidth, TextureHeight;
        [SerializeField, HideInInspector] public Dictionary<GameObject, Vector4> TextureMinMaxPositions;
        [SerializeField] public RenderTexture CurrentPaintingTexture, CurrentTextureMask;

        [SerializeField, HideInInspector] private Dictionary<GameObject, RenderTexture> _renderTexturesDict = new Dictionary<GameObject, RenderTexture>();
        private List<RenderTexture> RevertTexturesList = new List<RenderTexture>();
        private List<bool> RevertTexturesIsUsedList = new List<bool>();
        [SerializeField, HideInInspector] public bool RayHitASplatMap, RevertTrigger1;
        [SerializeField, HideInInspector] public float BrushStrengthInverterMultiplier = 1.0f;
        [SerializeField, HideInInspector] public int RevertKey;
        private int FrameCount, RevertedTextureKey;
        [HideInInspector] public bool ChangeTextureBrushIndex;

        public delegate void SVGMCDelegate();
        [HideInInspector] public bool SaturateCurrentBrush, SaturateResultOfCurrentBrush, ClampMinimumOfCurrentBrushTo0;
        public Vector2 TextRectSize;
        [HideInInspector] public Vector2 PaintTextCoordinate;
#pragma warning restore 0219
#pragma warning restore 0414

        private void Start()
        {
            if (Application.isPlaying == true) Destroy(this);
        }
        [HideInInspector, SerializeField] public RenderTextureFormat currentRTFormat;

#if UNITY_EDITOR


        private void OnEnable()
        {
            if (Application.isPlaying == false)
            {
                ObjectToRemoveWhenDonePainting = new List<Object>();
                EditorApplication.update += EditorUpdates;
                InitializeValues();
                VGSMC.RayDetectionEvent += OnScene;
                if (VGSMC.PaintShadows == true || VGSMC.PaintHeight == true)
                {
                    GetFarthestGrassFields();
                    SetAndPaintTexture();
                    SetRevertTexture(CurrentPaintingTexture.width, CurrentPaintingTexture.height);
                    if (VGSMC.PaintShadows == true) GetComponent<GrassShadowsPainter>().enabled = true;
                    if (VGSMC.PaintHeight == true) GetComponent<GrassHeightPainter>().enabled = true;
                    AddEventsToMainControls();
                }
                if (VGSMC.PaintColor == true)
                {
                    GetFarthestGrassFields();
                    SetCurrentPaintingTexture();
                    SetRevertTexture(CurrentPaintingTexture.width, CurrentPaintingTexture.height);
                    GetComponent<GrassColorPainter>().enabled = true;
                }
                //SVGMC.SaveCheckRT = new RenderTexture(CurrentPaintingTexture.width, CurrentPaintingTexture.height, 0, CurrentPaintingTexture.format);
                //Graphics.Blit(CurrentPaintingTexture, SVGMC.SaveCheckRT);
                VGSMC.TextureChangedBeforeExit = false;
                FrameCount = 0;
                ObjectEnabled = true;
            }
        }

        private void OnDisable()
        {
            if (Application.isPlaying == false)
            {
                EditorApplication.update -= EditorUpdates;
                VGSMC.RayDetectionEvent -= OnScene;
                GrassShadowsPainter grassShaderShadowsDrawer = GetComponent<GrassShadowsPainter>();
                if (grassShaderShadowsDrawer.enabled == true) grassShaderShadowsDrawer.enabled = false;
                GrassHeightPainter grassShaderHeightDrawer = GetComponent<GrassHeightPainter>();
                if (grassShaderHeightDrawer.enabled == true) grassShaderHeightDrawer.enabled = false;
                GrassColorPainter grassShaderColorDrawer = GetComponent<GrassColorPainter>();
                if (grassShaderColorDrawer.enabled == false) RemoveEventsToMainControls();
                if (grassShaderColorDrawer.enabled == true) grassShaderColorDrawer.enabled = false;
                ObjectEnabled = false;
                ClearStuffWhenStopPainting();
            }
        }
        private bool ObjectEnabled;

        private void EditorUpdates()
        {
            if (Application.isPlaying == false && this != null && ObjectEnabled == true)
            {
                BrushControl();
                FrameCount += 1;
            }
        }


        private List<Object> ObjectToRemoveWhenDonePainting;
        private void ClearStuffWhenStopPainting()
        {
            _renderTexturesDict.Clear();
            RevertTexturesList.Clear();
            RevertTexturesIsUsedList.Clear();
            for (int i = 0; i < ObjectToRemoveWhenDonePainting.Count; i++)
            { if (ObjectToRemoveWhenDonePainting[i] != null) DestroyImmediate(ObjectToRemoveWhenDonePainting[i]); }
            FarthestGrassFields = null;
            TextureMinMaxPositions.Clear();
            FurthestPositions = null;
        }

        public void AddEventsToMainControls()
        {
            if (VGSMC == null) VGSMC = GetComponent<MainControls>();
            VGSMC.UndoEvent += UndoMethod2;
            VGSMC.RedoEvent += RedoMethod2;
            VGSMC.EraseTextureEvent += EraseMethod2;
            VGSMC.FillTextureEvent += FillMethod2;
        }
        public void RemoveEventsToMainControls()
        {
            if (VGSMC == null) VGSMC = GetComponent<MainControls>();
            VGSMC.UndoEvent -= UndoMethod2;
            VGSMC.RedoEvent -= RedoMethod2;
            VGSMC.EraseTextureEvent -= EraseMethod2;
            VGSMC.FillTextureEvent -= FillMethod2;
        }
        private void UndoMethod2()
        {
            UndoMethod(CurrentPaintingTexture);
            SeperateAndAssignRenderTextures();
        }
        private void RedoMethod2()
        {
            RedoMethod(CurrentPaintingTexture);
            SeperateAndAssignRenderTextures();
        }
        private void EraseMethod2()
        {
            Color colorApplied = Color.black;
            if (VGSMC.PaintShadows == true) colorApplied = Color.white;
            FillTexture(CurrentPaintingTexture, colorApplied);
            SaveTextureForRevert(CurrentPaintingTexture);
            SeperateAndAssignRenderTextures();
        }
        private void FillMethod2()
        {
            Color colorVectorApplied = Color.white;
            if (VGSMC.PaintShadows == true) colorVectorApplied = Color.black;
            if (VGSMC.PaintHeight == true)
            {
                if (VGSMC.HeightPaintingFloorAndCeiling.y > 0.0f) colorVectorApplied = new Color(VGSMC.HeightPaintingFloorAndCeiling.y, 1.0f, 1.0f, 0.0f);
                else colorVectorApplied = Color.white * 8.5f;
            }
            FillTexture(CurrentPaintingTexture, colorVectorApplied);
            SaveTextureForRevert(CurrentPaintingTexture);
            SeperateAndAssignRenderTextures();
        }

        public void FillTexture(RenderTexture RTToErase, Color FillColorVector)
        {
            //Erase
            FillerMat.SetVector("_ColorVector", FillColorVector);
            RenderTexture tempTexture = RenderTexture.GetTemporary(RTToErase.width, RTToErase.height, 0, currentRTFormat);
            Graphics.Blit(tempTexture, RTToErase, FillerMat);
            RenderTexture.ReleaseTemporary(tempTexture);
            //SVGMC.SavedLastChanges = false;
        }

        private float FarthestLeft, FarthestRight, FarthestForward, FarthestBackward;

        private void GetFarthestGrassFields()
        {
            FarthestLeft = 0.0f;
            FarthestRight = 0.0f;
            FarthestForward = 0.0f;
            FarthestBackward = 0.0f;
            GameObject ReferenceObject = VGSMC.GrassFieldList[0];
            FarthestGrassFields = new GameObject[4];
            Vector3[] FarthestPositionsVec3 = new Vector3[4];
            for (int i = 0; i < FarthestGrassFields.Length; i++)
            {
                FarthestGrassFields[i] = VGSMC.GrassFieldList[0];
                if (i == 0) FarthestPositionsVec3[i] = VGSMC.GrassFieldList[0].transform.position - (ReferenceObject.transform.right * (VGSMC.GrassFieldMaxDistance / 2));
                if (i == 1) FarthestPositionsVec3[i] = VGSMC.GrassFieldList[0].transform.position + (ReferenceObject.transform.right * (VGSMC.GrassFieldMaxDistance / 2));
                if (i == 2) FarthestPositionsVec3[i] = VGSMC.GrassFieldList[0].transform.position + (ReferenceObject.transform.forward * (VGSMC.GrassFieldMaxDistance / 2));
                if (i == 3) FarthestPositionsVec3[i] = VGSMC.GrassFieldList[0].transform.position - (ReferenceObject.transform.forward * (VGSMC.GrassFieldMaxDistance / 2));
            }
            for (int i = 0; i < VGSMC.GrassFieldList.Count; i++)
            {
                Vector3 LocalPos = VGSMC.GrassFieldList[0].transform.InverseTransformPoint(VGSMC.GrassFieldList[i].transform.position);
                if (LocalPos.x < FarthestLeft)
                {
                    FarthestGrassFields[0] = VGSMC.GrassFieldList[i];
                    FarthestLeft = LocalPos.x;
                    FarthestPositionsVec3[0] = VGSMC.GrassFieldList[i].transform.position
                        - (ReferenceObject.transform.right * (VGSMC.GrassFieldMaxDistance / 2));
                }
                if (LocalPos.x > FarthestRight)
                {
                    FarthestGrassFields[1] = VGSMC.GrassFieldList[i];
                    FarthestRight = LocalPos.x;
                    FarthestPositionsVec3[1] = VGSMC.GrassFieldList[i].transform.position
                        + (ReferenceObject.transform.right * (VGSMC.GrassFieldMaxDistance / 2));
                }
                if (LocalPos.z > FarthestForward)
                {
                    FarthestGrassFields[2] = VGSMC.GrassFieldList[i];
                    FarthestForward = LocalPos.z;
                    FarthestPositionsVec3[2] = VGSMC.GrassFieldList[i].transform.position
                        + (ReferenceObject.transform.forward * (VGSMC.GrassFieldMaxDistance / 2));
                }
                if (LocalPos.z < FarthestBackward)
                {
                    FarthestGrassFields[3] = VGSMC.GrassFieldList[i];
                    FarthestBackward = LocalPos.z;
                    FarthestPositionsVec3[3] = VGSMC.GrassFieldList[i].transform.position
                        - (ReferenceObject.transform.forward * (VGSMC.GrassFieldMaxDistance / 2));
                }
            }
            FurthestPositions = new Vector2[4];
            for (int i = 0; i < FarthestPositionsVec3.Length; i++) FurthestPositions[i] = new Vector2(FarthestPositionsVec3[i].x, FarthestPositionsVec3[i].z);
            //Debug 
            for (int i = 0; i < FarthestGrassFields.Length; i++)
            {
                Vector3 PosVec3 = new Vector3(FurthestPositions[i].x, ReferenceObject.transform.position.y, FurthestPositions[i].y);
                //Debug.DrawLine(PosVec3, PosVec3 + Vector3.up * 25.0f * (i + 1), Color.green, 10.0f);
            }
            Vector2[] FurthestPositionsLocalized = new Vector2[4];
            for (int i = 0; i < FurthestPositionsLocalized.Length; i++)
            {
                Vector3 PosLocalized = transform.InverseTransformPoint(new Vector3(FurthestPositions[i].x, 0.0f, FurthestPositions[i].y));
                FurthestPositionsLocalized[i] = new Vector2(PosLocalized.x, PosLocalized.z);
            }
            XMaxDistanceAllFields = Mathf.Abs(FurthestPositionsLocalized[0].x - FurthestPositionsLocalized[1].x);
            ZMaxDistanceAllFields = Mathf.Abs(FurthestPositionsLocalized[2].y - FurthestPositionsLocalized[3].y);
            XLongestAmountOfGrassField = Mathf.RoundToInt(XMaxDistanceAllFields / VGSMC.GrassFieldMaxDistance);
            ZLongestAmountOfGrassField = Mathf.RoundToInt(ZMaxDistanceAllFields / VGSMC.GrassFieldMaxDistance);
            if (XLongestAmountOfGrassField == 0) XLongestAmountOfGrassField = 1;
            if (ZLongestAmountOfGrassField == 0) ZLongestAmountOfGrassField = 1;
            TextureWidth = Mathf.FloorToInt(XLongestAmountOfGrassField * 128.0f);
            TextureHeight = Mathf.FloorToInt(ZLongestAmountOfGrassField * 128.0f);
            //TextureHeight
            TextureMinMaxPositions = new Dictionary<GameObject, Vector4>();
            for (int i = 0; i < VGSMC.GrassFieldList.Count; i++)
            {
                Vector3 _objectPos = VGSMC.GrassFieldList[i].transform.position;
                float _localPosX = FarthestGrassFields[1].transform.InverseTransformPoint(_objectPos).x;
                float _localPosZ = FarthestGrassFields[2].transform.InverseTransformPoint(_objectPos).z;
                Vector2 _localHorizPos = new Vector2(_localPosX, _localPosZ);
                Vector2 LocalXMinMax = new Vector2(_localPosX, _localPosX - (VGSMC.GrassFieldMaxDistance));
                Vector2 LocalZMinMax = new Vector2(_localPosZ, _localPosZ - (VGSMC.GrassFieldMaxDistance));
                Vector2 TextureXMinMax = -(LocalXMinMax / XMaxDistanceAllFields);
                Vector2 TextureZMinMax = -(LocalZMinMax / ZMaxDistanceAllFields);
                TextureMinMaxPositions.Add(VGSMC.GrassFieldList[i], new Vector4(1 - TextureXMinMax.y, 1 - TextureXMinMax.x, 1 - TextureZMinMax.y, 1 - TextureZMinMax.x));
            }
        }

        private void SetCurrentPaintingTexture()
        {
            //Paint on Texture
            RenderTexture RTToAdd1 = new RenderTexture(TextureWidth, TextureHeight, 0, currentRTFormat);
            RenderTexture RTToAdd2 = new RenderTexture(TextureWidth, TextureHeight, 0, currentRTFormat);
            CurrentPaintingTexture = RTToAdd1;
            CurrentTextureMask = RTToAdd2;
            ObjectToRemoveWhenDonePainting.Add(RTToAdd1);
            ObjectToRemoveWhenDonePainting.Add(RTToAdd2);
            FillerMat.SetColor("_Color", Color.white);
            RenderTexture tempTextureErase = RenderTexture.GetTemporary(CurrentPaintingTexture.width, CurrentPaintingTexture.height, 0, currentRTFormat);
            Graphics.Blit(tempTextureErase, CurrentPaintingTexture, FillerMat);
            RenderTexture.ReleaseTemporary(tempTextureErase);
        }

        private void SetAndPaintTexture()
        {
            //Paint on Texture
            RenderTexture RTToAdd1 = new RenderTexture(TextureWidth, TextureHeight, 0, currentRTFormat);
            RenderTexture RTToAdd2 = new RenderTexture(TextureWidth, TextureHeight, 0, currentRTFormat);
            CurrentPaintingTexture = RTToAdd1;
            CurrentTextureMask = RTToAdd2;
            ObjectToRemoveWhenDonePainting.Add(RTToAdd1);
            ObjectToRemoveWhenDonePainting.Add(RTToAdd2);

            TextureCopierSmallToBigMat.SetTexture("_MainTex", CurrentPaintingTexture);
            FillerMat.SetColor("_Color", Color.white);
            RenderTexture tempTextureErase = RenderTexture.GetTemporary(CurrentPaintingTexture.width, CurrentPaintingTexture.height, 0, currentRTFormat);
            Graphics.Blit(tempTextureErase, CurrentPaintingTexture, FillerMat);
            RenderTexture.ReleaseTemporary(tempTextureErase);
            for (int i = 0; i < VGSMC.GrassFieldList.Count; i++)
            {
                Vector2 XMinMax = new Vector2(TextureMinMaxPositions[VGSMC.GrassFieldList[i]].x, TextureMinMaxPositions[VGSMC.GrassFieldList[i]].y);
                Vector2 ZMinMax = new Vector2(TextureMinMaxPositions[VGSMC.GrassFieldList[i]].z, TextureMinMaxPositions[VGSMC.GrassFieldList[i]].w);
                Texture2D TexToAdd = null;
                if (VGSMC.PaintShadows == true) TexToAdd = VGSMC.GrassFieldList[i].GetComponent<GrassFieldMaster>().ShadowTexture;
                if (VGSMC.PaintColor == true) TexToAdd = VGSMC.GrassFieldList[i].GetComponent<GrassFieldMaster>().ColorTexture;
                float UseColorInstead = 0.0f;
                if (VGSMC.PaintHeight == true)
                {
                    TexToAdd = VGSMC.GrassFieldList[i].GetComponent<GrassFieldMaster>().HeightTexture;
                    if (TexToAdd == null)
                    {
                        UseColorInstead = 1.0f;
                        Color colorToApply = new Color(0.85f, 0.0f, 0.0f, 0.0f);
                        TextureCopierSmallToBigMat.SetVector("_ColorVectorToApply", colorToApply);
                    }
                }
                TextureCopierSmallToBigMat.SetTexture("_TexToAdd", TexToAdd);
                TextureCopierSmallToBigMat.SetFloat("_UseColorInstead", UseColorInstead);
                Vector4 CoordX = new Vector4(XMinMax.x, XMinMax.y, 0, 0);
                Vector4 CoordY = new Vector4(ZMinMax.x, ZMinMax.y, 0, 0);
                TextureCopierSmallToBigMat.SetVector("_CoordinateX", CoordX);
                TextureCopierSmallToBigMat.SetVector("_CoordinateY", CoordY);

                RenderTexture tempTexture = RenderTexture.GetTemporary(CurrentPaintingTexture.width, CurrentPaintingTexture.height, 0, currentRTFormat);
                Graphics.Blit(CurrentPaintingTexture, tempTexture);
                TextureCopierSmallToBigMat.SetTexture("_MainTex", null);//This line is there to prevent an error.
                Graphics.Blit(tempTexture, CurrentPaintingTexture, TextureCopierSmallToBigMat);
                RenderTexture.ReleaseTemporary(tempTexture);

                //Texture Mask
                TextureCopierSmallToBigMat.SetFloat("_UseColorInstead", 1.0f);
                TextureCopierSmallToBigMat.SetVector("_ColorVectorToApply", Color.white);
                RenderTexture tempTexture2 = RenderTexture.GetTemporary(CurrentTextureMask.width, CurrentTextureMask.height, 0, currentRTFormat);
                Graphics.Blit(CurrentTextureMask, tempTexture2);
                Graphics.Blit(tempTexture2, CurrentTextureMask, TextureCopierSmallToBigMat);
                RenderTexture.ReleaseTemporary(tempTexture2);
            }
            SeperateAndAssignRenderTextures();
        }

        public void InitializeValues()
        {
            RevertedTextureKey = RevertChangesSize + 2;
            RevertKey = 0;
            VGSMC = GetComponent<MainControls>();
            DrawerMat = new Material(DrawShader);
            FillerMat = new Material(TextureFillerShader);
            SoftenerMat = new Material(SoftenerShader);
            BigToSmallMat = new Material(TextureCopierBigToSmallShader);
            TextureBrushMat = new Material(TextureBrushShader);
            NormalizeProgressivelyMat = new Material(NormalizeProgressivelyShader);
            TextureCopierSmallToBigMat = new Material(TextureCopierSmallToBig);
            ObjectToRemoveWhenDonePainting.Add(DrawerMat);
            ObjectToRemoveWhenDonePainting.Add(FillerMat);
            ObjectToRemoveWhenDonePainting.Add(SoftenerMat);
            ObjectToRemoveWhenDonePainting.Add(BigToSmallMat);
            ObjectToRemoveWhenDonePainting.Add(TextureBrushMat);
            ObjectToRemoveWhenDonePainting.Add(NormalizeProgressivelyMat);
            ObjectToRemoveWhenDonePainting.Add(TextureCopierSmallToBigMat);
            _renderTexturesDict.Clear();
            for (int i = 0; i < VGSMC.GrassFieldList.Count; i++)
            {
                currentRTFormat = RenderTextureFormat.ARGBFloat;
                if (VGSMC.PaintShadows == true) currentRTFormat = RenderTextureFormat.RFloat;
                if (VGSMC.PaintHeight == true) currentRTFormat = RenderTextureFormat.RFloat;
                if (VGSMC.PaintColor == true) currentRTFormat = RenderTextureFormat.ARGBHalf;
                RenderTexture RTToAdd = new RenderTexture(VGSMC.BaseTexturesSize, VGSMC.BaseTexturesSize, 0, currentRTFormat);
                _renderTexturesDict.Add(VGSMC.GrassFieldList[i], RTToAdd);
                ObjectToRemoveWhenDonePainting.Add(RTToAdd);
            }
            VGSMC.HeightTextureBrushSelected = 0;
        }

        public void BrushControl()
        {
            //Erase Trigger
            if (VGSMC.InvertCurrentBrush == true) { BrushStrengthInverterMultiplier = 1.0f; }
            else { BrushStrengthInverterMultiplier = -1.0f; }
        }

        public void SimpleDraw(RenderTexture renderTexture, Vector2 TextCoordinates, Color color, bool? Invert = false, bool? OverwriteColor = false,
            float? StrengthMultiplier = 1, float? SizeMultiplier = 1, Vector2? PaintingValueMinMax = null)
        {
            DrawerMat.SetTexture("_MainTex", renderTexture);
            DrawerMat.SetVector("_Coordinate", new Vector4(TextCoordinates.x, TextCoordinates.y, 0, 0));
            DrawerMat.SetColor("_Color", color);
            float MinGrassFieldLine = Mathf.Round(Mathf.Min(XLongestAmountOfGrassField, ZLongestAmountOfGrassField));
            DrawerMat.SetFloat("_MinGrassFieldLine", MinGrassFieldLine);
            if (SaturateCurrentBrush == true) DrawerMat.SetFloat("_SaturateDraw", 1.0f);
            else DrawerMat.SetFloat("_SaturateDraw", 0.0f);
            if (OverwriteColor == true) DrawerMat.SetFloat("_OverwriteColor", 1.0f);
            else DrawerMat.SetFloat("_OverwriteColor", 0.0f);
            if (SaturateResultOfCurrentBrush == true) DrawerMat.SetFloat("_SaturateResult", 1.0f);
            else DrawerMat.SetFloat("_SaturateResult", 0.0f);
            if (ClampMinimumOfCurrentBrushTo0 == true) DrawerMat.SetFloat("_ClampMinTo0", 1.0f);
            else DrawerMat.SetFloat("_ClampMinTo0", 0.0f);
            if (PaintingValueMinMax != null)
            {
                Vector2 RedPaintingMinMax = (Vector2)PaintingValueMinMax;
                DrawerMat.SetFloat("_MinRedPaintingValue", RedPaintingMinMax.x);
                DrawerMat.SetFloat("_MaxRedPaintingValue", RedPaintingMinMax.y);
            }
            else
            {
                DrawerMat.SetFloat("_MinRedPaintingValue", 0.0f);
                DrawerMat.SetFloat("_MaxRedPaintingValue", 0.0f);
            }
            float BrushStrengthInverted = 1.0f;
            if (Invert == true) BrushStrengthInverted = -1.0f;
            float SizeMultiplier2 = 1;
            if (SizeMultiplier != null) SizeMultiplier2 = (float)SizeMultiplier;
            //float LongestGrassFieldLine = Mathf.Max((float)XLongestAmountOfGrassField, (float)XLongestAmountOfGrassField);
            //DrawerMat.SetFloat("_Size", SVGMC.BrushSize * SizeMultiplier2 / LongestGrassFieldLine);
            DrawerMat.SetFloat("_Size", VGSMC.BrushSize * SizeMultiplier2);
            float StrengthMultiplier2 = (float)StrengthMultiplier;
            DrawerMat.SetFloat("_Strength", VGSMC.BrushStrength * BrushStrengthInverterMultiplier * BrushStrengthInverted * StrengthMultiplier2 * Mathf.Clamp01(Time.deltaTime * Time.deltaTime * 3000.0f));
            RenderTexture tempTexture = RenderTexture.GetTemporary(renderTexture.width, renderTexture.height, 0, currentRTFormat);
            Graphics.Blit(renderTexture, tempTexture);
            DrawerMat.SetTexture("_MainTex", null);//This line is there to prevent an error.
            Graphics.Blit(tempTexture, renderTexture, DrawerMat);
            RenderTexture.ReleaseTemporary(tempTexture);
            SeperateAndAssignRenderTextures();
            //SVGMC.SavedLastChanges = false;
        }

        public void NormalizeProgressively(RenderTexture renderTexture, Vector2 TextCoordinates, float? StrengthMultiplier = 1)
        {
            NormalizeProgressivelyMat.SetTexture("_MainTex", renderTexture);
            NormalizeProgressivelyMat.SetVector("_Coordinate", new Vector4(TextCoordinates.x, TextCoordinates.y, 0, 0));
            NormalizeProgressivelyMat.SetFloat("_Size", VGSMC.BrushSize);
            float StrengthMultiplier2 = (float)StrengthMultiplier;
            NormalizeProgressivelyMat.SetFloat("_Strength", VGSMC.BrushStrength * BrushStrengthInverterMultiplier * StrengthMultiplier2 * Mathf.Clamp01(Time.deltaTime * 100.0f));
            RenderTexture tempTexture = RenderTexture.GetTemporary(renderTexture.width, renderTexture.height, 0, currentRTFormat);
            Graphics.Blit(renderTexture, tempTexture);
            NormalizeProgressivelyMat.SetTexture("_MainTex", null);//This line is there to prevent an error.
            Graphics.Blit(tempTexture, renderTexture, NormalizeProgressivelyMat);
            RenderTexture.ReleaseTemporary(tempTexture);
            SeperateAndAssignRenderTextures();
            //SVGMC.SavedLastChanges = false;
        }

        public void SoftenPaint(RenderTexture renderTexture, Vector2 TextCoordinates)
        {
            DrawerMat.SetTexture("_MainTex", renderTexture);
            DrawerMat.SetTexture("_TexMask", CurrentTextureMask);
            SoftenerMat.SetVector("_Coordinate", new Vector4(TextCoordinates.x, TextCoordinates.y, 0, 0));
            SoftenerMat.SetFloat("_Size", VGSMC.BrushSize);
            SoftenerMat.SetFloat("_Strength", VGSMC.BrushStrength * Mathf.Clamp01(Time.deltaTime * 100.0f));
            SoftenerMat.SetInt("_Iterations", 1000);
            float MinGrassFieldLine = Mathf.Round(Mathf.Min(XLongestAmountOfGrassField, ZLongestAmountOfGrassField));
            SoftenerMat.SetFloat("_MinGrassFieldLine", MinGrassFieldLine);
            RenderTexture tempTexture = RenderTexture.GetTemporary(renderTexture.width, renderTexture.height, 0, currentRTFormat);
            Graphics.Blit(renderTexture, tempTexture);
            Graphics.Blit(tempTexture, renderTexture, SoftenerMat);
            RenderTexture.ReleaseTemporary(tempTexture);
            SeperateAndAssignRenderTextures();
            //SVGMC.SavedLastChanges = false;
        }

        private int TextureIndex;
        public void TexturePaint(RenderTexture renderTexture, Vector2 _coordinate, bool? Invert = false, int TextureStyle = 0, Vector2? PaintingValueMinMax = null, 
            float? BrushStrengthMultiplier = 1.0f)
        {
            Texture2D[] BrushTexturesToUse = RoundBrushTexture;
            if (TextureStyle == 1) BrushTexturesToUse = SpreadBrushTexture;
            if (ChangeTextureBrushIndex == true)
            {
                if (TextureIndex < BrushTexturesToUse.Length - 1)
                {
                    TextureIndex += 1;
                }
                else TextureIndex = 0;
                ChangeTextureBrushIndex = false;
            }
            float BrushStrengthInverted = 1.0f;
            if (Invert == true) BrushStrengthInverted = -1.0f;
            if (TextureIndex < BrushTexturesToUse.Length)
                TextureBrushMat.SetTexture("_BrushTex", BrushTexturesToUse[TextureIndex]);
            TextureBrushMat.SetVector("_Coordinate", new Vector4(_coordinate.x, _coordinate.y, 0, 0));
            float BrushSizeResult = Mathf.Clamp01(VGSMC.BrushSize / VGSMC.BrushMaxSize);
            TextureBrushMat.SetFloat("_Size", BrushSizeResult);
            float BrushStrengthMultiplier2 = (float)BrushStrengthMultiplier;
            TextureBrushMat.SetFloat("_Strength", VGSMC.BrushStrength * BrushStrengthInverterMultiplier * BrushStrengthInverted * BrushStrengthMultiplier2 * Mathf.Clamp01(Time.deltaTime * 100.0f));
            float MinGrassFieldLine = Mathf.Round(Mathf.Min(XLongestAmountOfGrassField, ZLongestAmountOfGrassField));
            TextureBrushMat.SetFloat("_MinGrassFieldSize", VGSMC.BaseTexturesSize);
            if (SaturateCurrentBrush == true) TextureBrushMat.SetFloat("_SaturateBrush", 1.0f);
            else TextureBrushMat.SetFloat("_SaturateBrush", 0.0f);
            if (SaturateResultOfCurrentBrush == true) TextureBrushMat.SetFloat("_SaturateResult", 1.0f);
            else TextureBrushMat.SetFloat("_SaturateResult", 0.0f);
            if (ClampMinimumOfCurrentBrushTo0 == true) TextureBrushMat.SetFloat("_ClampMinTo0", 1.0f);
            else TextureBrushMat.SetFloat("_ClampMinTo0", 0.0f);
            if (PaintingValueMinMax != null)
            {
                Vector2 RedPaintingMinMax = (Vector2)PaintingValueMinMax;
                TextureBrushMat.SetFloat("_MinRedPaintingValue", RedPaintingMinMax.x);
                TextureBrushMat.SetFloat("_MaxRedPaintingValue", RedPaintingMinMax.y);
            }
            else
            {
                TextureBrushMat.SetFloat("_MinRedPaintingValue", 0.0f);
                TextureBrushMat.SetFloat("_MaxRedPaintingValue", 0.0f);
            }
            RenderTexture tempTexture = RenderTexture.GetTemporary(renderTexture.width, renderTexture.height, 0, RenderTextureFormat.ARGBFloat);
            Graphics.Blit(renderTexture, tempTexture);
            Graphics.Blit(tempTexture, renderTexture, TextureBrushMat);
            RenderTexture.ReleaseTemporary(tempTexture);
            SeperateAndAssignRenderTextures();
            //SVGMC.SavedLastChanges = false;
        }

        public void SetRevertTexture(int RTWidth, int RTHeight)
        {
            //Set RevertTextures 
            RevertTexturesList.Clear();
            RevertTexturesIsUsedList.Clear();
            for (int i = 0; i < RevertChangesSize; i++)
            {
                if (RevertTexturesList.Count < RevertChangesSize)
                {
                    //RenderTexture RTToAdd = new RenderTexture(RTWidth, RTHeight, 0, currentRTFormat);
                    //RevertTexturesList.Add(RTToAdd);
                    //ObjectToRemoveWhenDonePainting.Add(RTToAdd);
                    RevertTexturesList.Add(null);
                }

                if (RevertTexturesIsUsedList.Count < RevertChangesSize) RevertTexturesIsUsedList.Add(false);
            }
            Graphics.Blit(CurrentPaintingTexture, RevertTexturesList[0]);
            RevertTexturesIsUsedList[0] = true;
        }

        public void SaveTextureForRevert(RenderTexture RTToApply)
        {
            //Find the first unused texture 
            for (int i = 0; i < RevertChangesSize; i++)
            {
                if (i == RevertedTextureKey)
                {
                    //Clear all RenderTextures that won't be used for Revert 
                    for (int i2 = i + 1; i2 < RevertChangesSize; i2++)
                    {
                        if (RevertTexturesList[i2] != null) DestroyImmediate(RevertTexturesList[i2]);
                        RevertTexturesIsUsedList[i2] = false;
                        RevertedTextureKey = RevertChangesSize + 2;
                    }
                }
                if (RevertTexturesIsUsedList[i] == false)
                {
                    //Set "IsUsed" and set RevertKey 
                    RevertKey = 0;
                    RevertTexturesIsUsedList[i] = true;
                    if (RevertTexturesList[i] == null)
                    {
                        RenderTexture RTToAdd = new RenderTexture(RTToApply.width, RTToApply.height, 0, currentRTFormat); ;
                        RevertTexturesList[i] = RTToAdd;
                        ObjectToRemoveWhenDonePainting.Add(RTToAdd);
                    }
                    Graphics.Blit(RTToApply, RevertTexturesList[i]);
                    break;
                }
                //When reaching maximum Revert Size
                if (i == RevertChangesSize - 1)
                {
                    RevertKey = 0;
                    if (RevertTexturesList[0] != null) DestroyImmediate(RevertTexturesList[0]);
                    RevertTexturesList.RemoveAt(0);
                    RenderTexture RTToAdd = new RenderTexture(RTToApply);
                    RevertTexturesList.Add(RTToAdd);
                    ObjectToRemoveWhenDonePainting.Add(RTToAdd);
                    RevertTexturesIsUsedList[i] = true;
                    Graphics.Blit(RTToApply, RevertTexturesList[i]);
                }
            }
        }
        public void UndoMethod(RenderTexture RTToApplyOn)
        {
            int UsedTextures = -1;
            for (int i = 0; i < RevertChangesSize; i++)
            {
                if (RevertTexturesIsUsedList[i] == true)
                {
                    UsedTextures += 1;
                }
            }
            int KeyToApply = 0;
            if (RevertKey > -UsedTextures)
            {
                if (RevertTexturesIsUsedList.Count > RevertKey - 1 + UsedTextures)
                {
                    RevertKey = RevertKey - 1;
                    KeyToApply = RevertKey + UsedTextures;
                    RevertTrigger1 = true;
                }

                if (RevertTrigger1 == true)
                {
                    if (RevertTexturesList[KeyToApply] == null)
                    {
                        RenderTexture RTToAdd = new RenderTexture(TextureWidth, TextureHeight, 0, currentRTFormat);
                        RevertTexturesList[KeyToApply] = RTToAdd;
                        ObjectToRemoveWhenDonePainting.Add(RTToAdd);
                    }
                    RenderTexture TextureToApply = RevertTexturesList[KeyToApply];
                    Graphics.Blit(TextureToApply, RTToApplyOn);
                    RevertTrigger1 = false;
                    RevertedTextureKey = KeyToApply;
                }
            }
            SeperateAndAssignRenderTextures();
        }
        public void RedoMethod(RenderTexture RTToApplyOn)
        {
            int UsedTextures = -1;
            for (int i = 0; i < RevertChangesSize; i++)
            {
                if (RevertTexturesIsUsedList[i] == true)
                {
                    UsedTextures += 1;
                }
            }
            int KeyToApply = 0;
            if (RevertTexturesIsUsedList.Count > RevertKey + 1 + UsedTextures)
            {
                if (RevertTexturesIsUsedList[RevertKey + 1 + UsedTextures] == true)
                {
                    RevertKey = RevertKey + 1;
                    KeyToApply = RevertKey + UsedTextures;
                    RevertTrigger1 = true;
                }
            }
            if (RevertTrigger1 == true)
            {
                if (RevertTexturesList[KeyToApply] == null)
                {
                    RenderTexture RTToAdd = new RenderTexture(TextureWidth, TextureHeight, 0, currentRTFormat);
                    RevertTexturesList[KeyToApply] = RTToAdd;
                    ObjectToRemoveWhenDonePainting.Add(RTToAdd);
                }
                RenderTexture TextureToApply = RevertTexturesList[KeyToApply];
                Graphics.Blit(TextureToApply, RTToApplyOn);
                RevertTrigger1 = false;
                RevertedTextureKey = KeyToApply;
            }
            SeperateAndAssignRenderTextures();
        }

        private Material BigToSmallMat;
        public void SeperateAndAssignRenderTextures()
        {
            for (int i = 0; i < VGSMC.GrassFieldList.Count; i++)
            {
                GameObject _gameObject = VGSMC.GrassFieldList[i];
                Vector2 XMinMax = new Vector2(TextureMinMaxPositions[_gameObject].y,
                    TextureMinMaxPositions[_gameObject].x);
                Vector2 ZMinMax = new Vector2(TextureMinMaxPositions[_gameObject].w,
                    TextureMinMaxPositions[_gameObject].z);
                BigToSmallMat.SetVector("_CoordinateX", new Vector4(XMinMax.x, XMinMax.y, 0, 0));
                BigToSmallMat.SetVector("_CoordinateY", new Vector4(ZMinMax.x, ZMinMax.y, 0, 0));
                BigToSmallMat.SetTexture("_OriginalTex", CurrentPaintingTexture);
                RenderTexture tempTexture = RenderTexture.GetTemporary(VGSMC.BaseTexturesSize, VGSMC.BaseTexturesSize, 0, currentRTFormat);
                Graphics.Blit(tempTexture, _renderTexturesDict[_gameObject], BigToSmallMat);
                RenderTexture.ReleaseTemporary(tempTexture);
                if (_renderTexturesDict.ContainsKey(_gameObject) == true && VGSMC.GrassFieldMasters.ContainsKey(_gameObject) == true)
                {
                    if (VGSMC.PaintShadows == true) VGSMC.GrassFieldMasters[_gameObject].SVGMShadowsMaster.ShadowRenderTexture = _renderTexturesDict[_gameObject];
                    if (VGSMC.PaintHeight == true) VGSMC.GrassFieldMasters[_gameObject].SVGMHeightMaster.HeightRenderTexture = _renderTexturesDict[_gameObject];
                    if (VGSMC.PaintColor == true) VGSMC.GrassFieldMasters[_gameObject].SVGMColorTextureMaster.ColorRenderTexture = _renderTexturesDict[_gameObject];
                }
            }
        }


        private void OnScene(SceneView scene)
        {
            Event _event = Event.current;
            Vector3 mousePos = _event.mousePosition;
            float ppp = EditorGUIUtility.pixelsPerPoint;
            mousePos.y = scene.camera.pixelHeight - mousePos.y * ppp;
            mousePos.x *= ppp;
            Ray ray = scene.camera.ScreenPointToRay(mousePos);
            RaycastHit[] raycastHits = Physics.RaycastAll(ray, Mathf.Infinity, VGSMC.GroundLayers, QueryTriggerInteraction.Ignore);
            RayHitASplatMap = false;
            float closestDist = Mathf.Infinity;
            RaycastHit closestRayHit = new RaycastHit();
            for (int i = 0; i < raycastHits.Length; i++)
            {
                if (raycastHits[i].distance < closestDist)
                {
                    closestDist = raycastHits[i].distance;
                    closestRayHit = raycastHits[i];
                }
            }
            if (raycastHits.Length > 0)
            {
                RayHitASplatMap = true;
                float XLocal = FarthestGrassFields[1].transform.InverseTransformPoint(closestRayHit.point).x + (VGSMC.GrassFieldMaxDistance / 2)
                    + (TextureMinMaxPositions[FarthestGrassFields[1]] * XMaxDistanceAllFields).x;
                float ZLocal = FarthestGrassFields[2].transform.InverseTransformPoint(closestRayHit.point).z + (VGSMC.GrassFieldMaxDistance / 2)
                    + (TextureMinMaxPositions[FarthestGrassFields[2]] * ZMaxDistanceAllFields).z;
                float XForTex = XLocal / XMaxDistanceAllFields;
                float YForTex = ZLocal / ZMaxDistanceAllFields;
                Vector2 TextureCoordToUse = new Vector2(XForTex, YForTex);
                PaintTextCoordinate = TextureCoordToUse;
            }


            //Handles.BeginGUI();
            //Rect TextureRect = new Rect(Vector2.zero, TextRectSize);
            //GUILayout.BeginArea(TextureRect);
            //GUIStyle BoxStyle = new GUIStyle(GUI.skin.box);
            //BoxStyle.fixedHeight = TextRectSize.y;
            //BoxStyle.fixedWidth = TextRectSize.x;
            //GUILayout.Box(test._targetRT, BoxStyle);
            //if (SVGMC.ColorMasks.Count > 0)
            //{
            //    if (SVGMC.ColorMasksRenderTextures.ContainsKey(SVGMC.ColorMasks[0])) GUILayout.Box(SVGMC.ColorMasksRenderTextures[SVGMC.ColorMasks[0]], BoxStyle);
            //}
            //GUILayout.EndArea();
            Handles.EndGUI();
        }
#endif

    }

}