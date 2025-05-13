using System.Collections.Generic;
using UnityEngine;
using VibrantGrassShaderTools;
using System.IO;
using UnityEditor;
using System.Linq;

namespace VibrantGrassShader
{
    [ExecuteInEditMode]
    public class GrassColorPainter : MonoBehaviour
    {
#pragma warning disable 0219
#pragma warning disable 0414
        [SerializeField, Foldout("Data (Don't Touch", true)] private Texture2D OriginalTexture = null;
        [SerializeField] private Dictionary<RenderTexture, Texture2D> AddAndRemoveDict = new Dictionary<RenderTexture, Texture2D>();
        [SerializeField] private List<Texture2D> RevertMaskTex2D = new List<Texture2D>();
        [SerializeField] private Shader AddTextureShader;
        private Material AddTextureMat, FillerMat;


        [SerializeField, HideInInspector] private MainControls SVGMC = null;
        [SerializeField, HideInInspector] private GrassTextureDrawing SVGMCTextureDrawing = null;
        [HideInInspector, SerializeField] public bool ReadyToUse;
        private bool SaveForRevertTrigger1;
        private bool FirstFrame;
        private void Start()
        {
            if (Application.isPlaying == true) Destroy(this);
        }
        [SerializeField, HideInInspector] private Material ApplyTextureAtCoordinateMat;
#pragma warning restore 0219
#pragma warning restore 0414


#if UNITY_EDITOR


        void EditorUpdates()
        {
            if (Application.isPlaying == false && this != null && ObjectEnabled == true)
            {
                if (FirstFrame == false)
                {
                    SVGMCTextureDrawing.SetRevertTexture(SVGMCTextureDrawing.CurrentPaintingTexture.width, SVGMCTextureDrawing.CurrentPaintingTexture.height);
                    SetValues();
                    FirstFrame = true;
                    ReadyToUse = true;
                    if (SVGMC.ColorMasks.Count == 0) SVGMC.PaintingAddColor = true;
                    SaveCheckRTs = new Dictionary<RenderTexture, RenderTexture>();
                    foreach (KeyValuePair<Texture2D, RenderTexture> item in SVGMC.ColorMasksRenderTextures)
                    {
                        RenderTexture RTToAdd = new RenderTexture(item.Value.width, item.Value.height, 0, item.Value.format);
                        SaveCheckRTs.Add(item.Value, RTToAdd);
                        ObjectsToDestroyWhenStopPainting.Add(RTToAdd);
                        Graphics.Blit(item.Value, SaveCheckRTs[item.Value]);
                    }
                }
                if (SVGMC.ColorMasksRenderTextures.Count > 0)
                {
                    ControlsLogic();
                    AssignResult();
                    SVGMCTextureDrawing.SeperateAndAssignRenderTextures();
                }
            }
        }

        private void OnEnable()
        {
            if (Application.isPlaying == false)
            {
                ObjectsToDestroyWhenStopPainting = new List<Object>();
                AssetsToDestroyWhenSaving = new List<Object>();
                EditorApplication.update += EditorUpdates;
                ReadyToUse = false;
                SVGMCTextureDrawing = GetComponent<GrassTextureDrawing>();
                SVGMC = GetComponent<MainControls>();
                SaveForRevertTrigger1 = false;
                AddEventsToMainControls();
                FirstFrame = false;
                AddTextureMat = new Material(AddTextureShader);
                ObjectsToDestroyWhenStopPainting.Add(AddTextureMat);
                FillerMat = new Material(SVGMCTextureDrawing.TextureFillerShader);
                ObjectsToDestroyWhenStopPainting.Add(FillerMat);
                ApplyTextureAtCoordinateMat = new Material(SVGMCTextureDrawing.ApplyTextureAtCoordinateShader);
                ObjectsToDestroyWhenStopPainting.Add(ApplyTextureAtCoordinateMat);
                RevertMaskTex2D.Clear();
                CurrentUndoRedoKey = 0;
                UndoRedoToRemove = 0;
                SVGMC.AddColorEvent += AddPaintingColorItems;
                SVGMC.RemoveColorEvent += RemoveItems;
                SVGMC.ChosenColorsForPainting = new Texture2DAndColorSerializableDictionary();
                SVGMC.ChosenColorsForPainting.Clear();
                foreach (KeyValuePair<Texture2D, Color> item in SVGMC.ChosenColorsSaved) SVGMC.ChosenColorsForPainting.Add(item.Key, item.Value);
                SVGMC.ColorMasksRenderTextures = new Dictionary<Texture2D, RenderTexture>();
                SVGMC.UnSavedColorMasks = new List<Texture2D>();
                AddAndRemoveDict = new Dictionary<RenderTexture, Texture2D>();
                LastChosenRTColorMask = SVGMC.CurrentTex2DColorMask;
                FirstDraw = false;
                ObjectEnabled = true;
            }
        }

        private void OnDisable()
        {
            if (Application.isPlaying == false)
            {
                EditorApplication.update -= EditorUpdates;
                RemoveEventsFromMainControls();
                SVGMC.AddColorEvent -= AddPaintingColorItems;
                SVGMC.RemoveColorEvent -= RemoveItems;
                ReadyToUse = false;
                ClearAndDestroyStuffWhenStopPainting();
                ObjectEnabled = false;
            }
        }

        private List<Object> ObjectsToDestroyWhenStopPainting, AssetsToDestroyWhenSaving;
        private void ClearAndDestroyStuffWhenStopPainting()
        {
            for (int i = 0; i < ObjectsToDestroyWhenStopPainting.Count; i++) 
            { 
                if (ObjectsToDestroyWhenStopPainting[i] != null) DestroyImmediate(ObjectsToDestroyWhenStopPainting[i]);
            }
        }

        private bool ObjectEnabled;

        private Dictionary<RenderTexture, RenderTexture> SaveCheckRTs = new Dictionary<RenderTexture, RenderTexture>();
        private bool CompareTextures(RenderTexture RT1, RenderTexture RT2)
        {
            int QualDivider = 8;
            Vector2Int RT1Size = new Vector2Int(RT1.width / QualDivider, RT1.height / QualDivider);
            RenderTexture.active = RT1;
            RenderTexture tempRT1 = RenderTexture.GetTemporary(RT1Size.x, RT1Size.y, 0, RT1.format);
            Graphics.Blit(RT1, tempRT1);
            RenderTexture.active = tempRT1;
            Texture2D tex1 = new Texture2D(RT1Size.x, RT1Size.y, TextureFormat.RGBAHalf, false);
            tex1.ReadPixels(new Rect(0, 0, RT1Size.x, RT1Size.y), 0, 0);
            tex1.Apply();
            RenderTexture.ReleaseTemporary(tempRT1);
            Color[] colors1 = tex1.GetPixels();
            DestroyImmediate(tex1);

            Vector2Int RT2Size = new Vector2Int(RT2.width / QualDivider, RT2.height / QualDivider);
            RenderTexture.active = RT2;
            RenderTexture tempRT2 = RenderTexture.GetTemporary(RT2Size.x, RT2Size.y, 0, RT2.format);
            Graphics.Blit(RT2, tempRT2);
            RenderTexture.active = tempRT2;
            Texture2D tex2 = new Texture2D(RT2Size.x, RT2Size.y, TextureFormat.RGBAHalf, false);
            tex2.ReadPixels(new Rect(0, 0, RT2Size.x, RT2Size.y), 0, 0);
            tex2.Apply();
            RenderTexture.ReleaseTemporary(tempRT1);

            Color[] colors2 = tex2.GetPixels();
            DestroyImmediate(tex2);
            bool ColorsAreDifferent = false;
            for (int i = 0; i < colors1.Length; i++)
            {
                if (colors1[i] != colors2[i])
                {
                    ColorsAreDifferent = true;
                    break;
                }
            }
            colors1 = null;
            colors2 = null;
            return ColorsAreDifferent;
        }

        private void SaveColorMasksToPNGWhenNecessary()
        {
            for (int i = 0; i < AssetsToDestroyWhenSaving.Count; i++)
            {
                string Path = AssetDatabase.GetAssetPath(AssetsToDestroyWhenSaving[i]);
                if (AssetsToDestroyWhenSaving[i] != null)
                {
                    if (Path != null && Path != "") AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(AssetsToDestroyWhenSaving[i]));
                    else DestroyImmediate(AssetsToDestroyWhenSaving[i]);
                }
            }
            Dictionary<Texture2D, Texture2D> OldTex2DNewTex2D = new Dictionary<Texture2D, Texture2D>();
            List<Texture2D> TexturesToKeep = new List<Texture2D>();
            foreach (KeyValuePair<RenderTexture, Texture2D> item in AddAndRemoveDict)
            {
                if (SVGMC.ColorMasks.Contains(item.Value) == true)
                {
                    bool ConfirmSaving = CompareTextures(item.Key, SaveCheckRTs[item.Key]);
                    if (ConfirmSaving == true) SaveTexture(item.Key, item.Value);
                }
                else
                {
                    Texture2D CreatedTexture = CreateColorMaskAssets(item.Value);
                    SaveTexture(item.Key, CreatedTexture);
                    OldTex2DNewTex2D.Add(item.Value, CreatedTexture);
                    TexturesToKeep.Add(CreatedTexture);
                }
            }
            //Delete the textures that are no longer used 
            List<Texture2D> TexturesToDelete = new List<Texture2D>();
            foreach (Texture2D tex2D in SVGMC.ColorMasks)
            {
                if (AddAndRemoveDict.ContainsValue(tex2D) == false && TexturesToKeep.Contains(tex2D) == false) TexturesToDelete.Add(tex2D);
            }
            foreach (Texture2D tex2D in TexturesToDelete)
            {
                SVGMC.ColorMasks.Remove(tex2D);
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(tex2D));
            }
            //Remove the old textures to replace them with the new ones
            foreach (KeyValuePair<Texture2D, Texture2D> item2 in OldTex2DNewTex2D)
            {
                int UnSavedColorIndexToInsertAt = 0;
                for (int i = 0; i < SVGMC.UnSavedColorMasks.Count; i++)
                {
                    if (SVGMC.UnSavedColorMasks[i] == item2.Key) UnSavedColorIndexToInsertAt = i;
                }
                SVGMC.UnSavedColorMasks.Remove(item2.Key);
                SVGMC.UnSavedColorMasks.Insert(UnSavedColorIndexToInsertAt, item2.Value);
                if (SVGMC.ColorMasksRenderTextures.ContainsKey(item2.Key) == true)
                {
                    SVGMC.ColorMasksRenderTextures.Add(item2.Value, SVGMC.ColorMasksRenderTextures[item2.Key]);
                    SVGMC.ColorMasksRenderTextures.Remove(item2.Key);
                }
                if (SVGMC.ChosenColorsForPainting.ContainsKey(item2.Key) == true)
                {
                    SVGMC.ChosenColorsForPainting.Add(item2.Value, SVGMC.ChosenColorsForPainting[item2.Key]);
                    SVGMC.ChosenColorsForPainting.Remove(item2.Key);
                }
                if (AddAndRemoveDict.ContainsValue(item2.Key) == true)
                {
                    AddAndRemoveDict[SVGMC.ColorMasksRenderTextures[item2.Value]] = item2.Value;
                }
                if (SVGMC.CurrentTex2DColorMask == item2.Key) SVGMC.CurrentTex2DColorMask = item2.Value;
            }
            SVGMC.ColorMasks = new List<Texture2D>();
            for (int i = 0; i < SVGMC.UnSavedColorMasks.Count; i++) SVGMC.ColorMasks.Add(SVGMC.UnSavedColorMasks[i]);
            //Save Colors
            SVGMC.ChosenColorsSaved = new Texture2DAndColorSerializableDictionary();
            foreach (KeyValuePair<Texture2D, Color> item in SVGMC.ChosenColorsForPainting) SVGMC.ChosenColorsSaved.Add(item.Key, item.Value);
            if (SVGMC.ColorMasks.Count <= 0) SVGMC.DeleteCurrentTextureAssetEvent?.Invoke();

            //Assign Grid 
            Vector3 XZGridStartPosLocalVec3 = new Vector3(transform.InverseTransformPoint(SVGMCTextureDrawing.FarthestGrassFields[0].transform.position).x, 0.0f,
            transform.InverseTransformPoint(SVGMCTextureDrawing.FarthestGrassFields[3].transform.position).z);
            SVGMC.XYGridStartPosLocal = new Vector2(XZGridStartPosLocalVec3.x, XZGridStartPosLocalVec3.z);
            SVGMC.GridPositionsSaved = new GOAndVect2IntSerializableDict();
            for (int i = 0; i < SVGMC.GrassFieldList.Count; i++)
            {
                if (SVGMC.GridPositionsSaved.ContainsKey(SVGMC.GrassFieldList[i]) == false)
                {
                    Vector3 GrassFieldLocalPos = transform.InverseTransformPoint(SVGMC.GrassFieldList[i].transform.position);
                    int XGridPos = Mathf.RoundToInt((GrassFieldLocalPos.x - SVGMC.XYGridStartPosLocal.x) / SVGMC.GrassFieldMaxDistance);
                    int YGridPos = Mathf.RoundToInt((GrassFieldLocalPos.z - SVGMC.XYGridStartPosLocal.y) / SVGMC.GrassFieldMaxDistance);
                    SVGMC.GridPositionsSaved.Add(SVGMC.GrassFieldList[i], new Vector2Int(XGridPos, YGridPos));
                }
            }

            SVGMC.GridPositionsChanged = new GOAndVect2IntSerializableDict();
            foreach (KeyValuePair<GameObject, Vector2Int> item in SVGMC.GridPositionsSaved) SVGMC.GridPositionsChanged.Add(item.Key, item.Value);
            EditorUtility.SetDirty(this);
            EditorUtility.SetDirty(SVGMC);
        }

        private void AddEventsToMainControls()
        {
            if (SVGMC == null) SVGMC = GetComponent<MainControls>();
            SVGMC.UndoEvent += UndoMethod;
            SVGMC.RedoEvent += RedoMethod;
            SVGMC.EraseTextureEvent += EraseMethod2;
            SVGMC.FillTextureEvent += FillMethod2;
            SVGMC.SaveCurrentTextureEvent += SaveColorMasksToPNGWhenNecessary;
        }

        private void RemoveEventsFromMainControls()
        {
            if (SVGMC == null) SVGMC = GetComponent<MainControls>();
            SVGMC.UndoEvent -= UndoMethod;
            SVGMC.RedoEvent -= RedoMethod;
            SVGMC.EraseTextureEvent -= EraseMethod2;
            SVGMC.FillTextureEvent -= FillMethod2;
            SVGMC.SaveCurrentTextureEvent -= SaveColorMasksToPNGWhenNecessary;
        }

        private void EraseMethod2()
        {
            bool AlreadySaved = SaveForUndoRedoCheckBeforePainting();
            if (AlreadySaved == false) SaveForUndoRedoMethod();
            SVGMCTextureDrawing.FillTexture(SVGMC.ColorMasksRenderTextures[SVGMC.CurrentTex2DColorMask], Color.black);
            AssignResult();
            SaveForUndoRedoMethod();
            LastColorMaskPaintedOn = SVGMC.CurrentTex2DColorMask;
        }

        private void FillMethod2()
        {
            bool AlreadySaved = SaveForUndoRedoCheckBeforePainting();
            if (AlreadySaved == false) SaveForUndoRedoMethod();
            SVGMCTextureDrawing.FillTexture(SVGMC.ColorMasksRenderTextures[SVGMC.CurrentTex2DColorMask], SVGMC.ChosenColorsForPainting[SVGMC.CurrentTex2DColorMask]);
            AssignResult();
            SaveForUndoRedoMethod();
            LastColorMaskPaintedOn = SVGMC.CurrentTex2DColorMask;
        }

        private int CurrentUndoRedoKey;
        private void UndoMethod()
        {
            int keyResult = RevertMaskTex2D.Count - 1 - CurrentUndoRedoKey - 1;
            if (keyResult >= 0)
            {
                if (RevertMaskTex2D.Count > keyResult + 1)
                {
                    if (RevertMaskTex2D[keyResult] != RevertMaskTex2D[keyResult + 1])
                    {
                        SVGMCTextureDrawing.UndoMethod(SVGMC.ColorMasksRenderTextures[RevertMaskTex2D[keyResult]]);
                        if ((RevertMaskTex2D.Count - 1 - CurrentUndoRedoKey - 1) >= 0) CurrentUndoRedoKey += 1;
                        keyResult = RevertMaskTex2D.Count - 1 - CurrentUndoRedoKey - 1;
                    }
                }
                SVGMCTextureDrawing.UndoMethod(SVGMC.ColorMasksRenderTextures[RevertMaskTex2D[keyResult]]);
                AssignResult();
                if ((RevertMaskTex2D.Count - 1 - CurrentUndoRedoKey - 1) >= 0) CurrentUndoRedoKey += 1;
                UndoRedoToRemove = RevertMaskTex2D.Count - 1 - keyResult;
            }
        }
        private void RedoMethod()
        {
            int keyResult = RevertMaskTex2D.Count - 1 - CurrentUndoRedoKey + 1;//+ 1 to get the next one
            if (keyResult >= 0 && keyResult < RevertMaskTex2D.Count)
            {
                if (RevertMaskTex2D.Count > keyResult + 1)
                {
                    if (RevertMaskTex2D[keyResult] != RevertMaskTex2D[keyResult + 1])
                    {
                        SVGMCTextureDrawing.RedoMethod(SVGMC.ColorMasksRenderTextures[RevertMaskTex2D[keyResult]]);
                        if (CurrentUndoRedoKey - 1 >= 0) CurrentUndoRedoKey -= 1;
                        keyResult = RevertMaskTex2D.Count - 1 - CurrentUndoRedoKey + 1;
                    }
                }
                SVGMCTextureDrawing.RedoMethod(SVGMC.ColorMasksRenderTextures[RevertMaskTex2D[keyResult]]);
                AssignResult();
                if (CurrentUndoRedoKey - 1 >= 0) CurrentUndoRedoKey -= 1;
                UndoRedoToRemove = RevertMaskTex2D.Count - 1 - keyResult;
            }
        }

        private void SetValues()
        {
            for (int i = 0; i < SVGMC.ColorMasks.Count; i++)
            {
                if (SVGMC.ChosenColorsForPainting.ContainsKey(SVGMC.ColorMasks[i]) == false) SVGMC.ChosenColorsForPainting.Add(SVGMC.ColorMasks[i], Color.white);
                SVGMC.UnSavedColorMasks.Add(SVGMC.ColorMasks[i]);
                RenderTexture RTToAdd = new RenderTexture(SVGMCTextureDrawing.CurrentPaintingTexture.width, SVGMCTextureDrawing.CurrentPaintingTexture.height, 0, SVGMCTextureDrawing.currentRTFormat);
                SVGMC.ColorMasksRenderTextures.Add(SVGMC.ColorMasks[i],
                    RTToAdd);
                ObjectsToDestroyWhenStopPainting.Add(RTToAdd);
                AddAndRemoveDict.Add(RTToAdd, SVGMC.ColorMasks[i]);
            }
            int LowestSavedX = 1000000;
            int HighestSavedX = -1000000;
            int LowestSavedY = 1000000;
            int HighestSavedY = -1000000;
            int LowestChangedX = 1000000;
            int HighestChangedX = -1000000;
            int LowestChangedY = 1000000;
            int HighestChangedY = -1000000;
            foreach (KeyValuePair<GameObject, Vector2Int> item in SVGMC.GridPositionsSaved)
            {
                if (item.Value.x < LowestSavedX) LowestSavedX = item.Value.x;
                if (item.Value.y < LowestSavedY) LowestSavedY = item.Value.y;
                if (item.Value.x > HighestSavedX) HighestSavedX = item.Value.x;
                if (item.Value.y > HighestSavedY) HighestSavedY = item.Value.y;
            }
            foreach (KeyValuePair<GameObject, Vector2Int> item in SVGMC.GridPositionsChanged)
            {
                if (item.Value.x < LowestChangedX) LowestChangedX = item.Value.x;
                if (item.Value.y < LowestChangedY) LowestChangedY = item.Value.y;
                if (item.Value.x > HighestChangedX) HighestChangedX = item.Value.x;
                if (item.Value.y > HighestChangedY) HighestChangedY = item.Value.y;
            }
            int ChangedXScale = Mathf.RoundToInt(Mathf.Abs(HighestChangedX - LowestChangedX) + 1.0f);
            int ChangedYScale = Mathf.RoundToInt(Mathf.Abs(HighestChangedY - LowestChangedY) + 1.0f);
            int SavedXScale = Mathf.RoundToInt(Mathf.Abs(HighestSavedX - LowestSavedX + 1.0f));
            int SavedYScale = Mathf.RoundToInt(Mathf.Abs(HighestSavedY - LowestSavedY) + 1.0f);
            float XOffset = (1.0f / ChangedXScale) * (-LowestChangedX);//Not sure why (1.0f / SavedXScale) works when I thought it should be (1.0f / ChangedXScale) xD... 
            float YOffset = (1.0f / ChangedYScale) * (-LowestChangedY);
            float XScale = 1.0f - (1.0f / ChangedXScale) * (ChangedXScale - SavedXScale);
            float YScale = 1.0f - (1.0f / ChangedYScale) * (ChangedYScale - SavedYScale);
            Vector2 AppliedOffset = new Vector2(XOffset, YOffset);
            Vector2 AppliedScale = new Vector2(XScale, YScale);
            ApplyTextureAtCoordinateMat.SetColor("_OutOfBoundColorToApply", Color.black);
            ApplyTextureAtCoordinateMat.SetFloat("_TexToApplyOffsetX", AppliedOffset.x);
            ApplyTextureAtCoordinateMat.SetFloat("_TexToApplyOffsetY", AppliedOffset.y);
            ApplyTextureAtCoordinateMat.SetFloat("_TexToApplyScaleX", AppliedScale.x);
            ApplyTextureAtCoordinateMat.SetFloat("_TexToApplyScaleY", AppliedScale.y);
            for (int i = 0; i < SVGMC.ColorMasks.Count; i++) Graphics.Blit(SVGMC.ColorMasks[i], SVGMC.ColorMasksRenderTextures[SVGMC.ColorMasks[i]], ApplyTextureAtCoordinateMat);
        }

        private void AddPaintingColorItems()
        {
            Texture2D UnsavedCOlorMaskToAdd = new Texture2D(0, 0);
            SVGMC.UnSavedColorMasks.Add(UnsavedCOlorMaskToAdd);
            ObjectsToDestroyWhenStopPainting.Add(UnsavedCOlorMaskToAdd);
            for (int i = 0; i < SVGMC.UnSavedColorMasks.Count; i++)
            {
                if (SVGMC.ColorMasksRenderTextures.ContainsKey(SVGMC.UnSavedColorMasks[i]) == false)
                {
                    RenderTexture RTToAdd = new RenderTexture(SVGMCTextureDrawing.CurrentPaintingTexture.width,
                    SVGMCTextureDrawing.CurrentPaintingTexture.height, 0, SVGMCTextureDrawing.currentRTFormat);
                    SVGMC.ColorMasksRenderTextures.Add(SVGMC.UnSavedColorMasks[i], RTToAdd);
                    ObjectsToDestroyWhenStopPainting.Add(RTToAdd);
                }
                if (SVGMC.ChosenColorsForPainting.ContainsKey(SVGMC.UnSavedColorMasks[i]) == false) SVGMC.ChosenColorsForPainting.Add
                        (SVGMC.UnSavedColorMasks[i], Color.white);
            }
            Texture2D AddedTexture = SVGMC.UnSavedColorMasks[SVGMC.UnSavedColorMasks.Count - 1];
            RenderTexture AddedRT = SVGMC.ColorMasksRenderTextures[AddedTexture];
            RenderTexture TexToAdd = new RenderTexture(AddedRT.width, AddedRT.height, 0, AddedRT.format);
            SaveCheckRTs.Add(AddedRT, TexToAdd);
            ObjectsToDestroyWhenStopPainting.Add(TexToAdd);
            FillerMat.SetColor("_Color", Color.black);
            RenderTexture tempTextureErase = RenderTexture.GetTemporary(SVGMCTextureDrawing.CurrentPaintingTexture.width,
                SVGMCTextureDrawing.CurrentPaintingTexture.height, 0, SVGMCTextureDrawing.currentRTFormat);
            Graphics.Blit(tempTextureErase, SVGMC.ColorMasksRenderTextures[SVGMC.UnSavedColorMasks[SVGMC.UnSavedColorMasks.Count - 1]], FillerMat);
            RenderTexture.ReleaseTemporary(tempTextureErase);
            SaveForRevertTrigger1 = true;
            AddAndRemoveDict.Add(SVGMC.ColorMasksRenderTextures[SVGMC.UnSavedColorMasks[SVGMC.UnSavedColorMasks.Count - 1]], SVGMC.UnSavedColorMasks[SVGMC.UnSavedColorMasks.Count - 1]);
            SVGMC.CurrentTex2DColorMask = SVGMC.UnSavedColorMasks[SVGMC.UnSavedColorMasks.Count - 1];
            SVGMC.SelectedColorIndex = SVGMC.UnSavedColorMasks.Count - 1;
            SVGMC.CurrentColorPicked = Color.white;
            Graphics.Blit(AddedRT, SaveCheckRTs[AddedRT]);
            EditorUtility.SetDirty(this);
            EditorUtility.SetDirty(SVGMC);
        }

        private void RemoveItems(int IndexToRemove)
        {
            Texture2D MaskKey = SVGMC.UnSavedColorMasks[IndexToRemove];
            SVGMC.UnSavedColorMasks.RemoveAt(IndexToRemove);
            if (IndexToRemove - 1 >= 0) SVGMC.CurrentTex2DColorMask = SVGMC.UnSavedColorMasks[IndexToRemove - 1];
            else if (SVGMC.UnSavedColorMasks.Count > 0) SVGMC.CurrentTex2DColorMask = SVGMC.UnSavedColorMasks[0];
            RenderTexture ColorMaskRT = SaveCheckRTs[SVGMC.ColorMasksRenderTextures[MaskKey]];
            //SaveCheckRTs.Remove(ColorMaskRT);
            //AddAndRemoveDict.Remove(ColorMaskRT);
            //SVGMC.ColorMasksRenderTextures.Remove(MaskKey);
            RevertMaskTex2D.Clear();
            SaveForRevertTrigger1 = true;
            SVGMC.ChosenColorsForPainting.Remove(MaskKey);
            AssetsToDestroyWhenSaving.Add(MaskKey);
            //ObjectToDestroyWhenStopPainting.Add(ColorMaskRT);
            if (SVGMC.ChosenColorsForPainting.ContainsKey(SVGMC.CurrentTex2DColorMask) == true) SVGMC.CurrentColorPicked = SVGMC.ChosenColorsForPainting[SVGMC.CurrentTex2DColorMask];
            if (SVGMC.ColorMasksRenderTextures.Count == 0)
            {
                SVGMCTextureDrawing.FillTexture(SVGMCTextureDrawing.CurrentPaintingTexture, Color.black);
                AssignResult();
            }
            EditorUtility.SetDirty(this);
            EditorUtility.SetDirty(SVGMC);
        }

        private bool SaveForUndoRedoCheckBeforePainting()
        {
            bool AlreadySaved = false;
            if (SaveForRevertTrigger1 == false && UndoRedoToRemove > 0)
            {
                for (int i = 0; i < UndoRedoToRemove; i++) RevertMaskTex2D.RemoveAt(RevertMaskTex2D.Count - 1);
                UndoRedoToRemove = 0;
                SaveForUndoRedoMethod();
                AlreadySaved = true;
            }
            if (ChangedColorMask == true && AlreadySaved == false)
            {
                SaveForUndoRedoMethod();
                ChangedColorMask = false;
            }
            if (FirstDraw == false && AlreadySaved == false)
            {
                SaveForUndoRedoMethod();
                FirstDraw = true;
            }
            return AlreadySaved;
        }

        private int UndoRedoToRemove;
        private Texture2D LastChosenRTColorMask;
        private bool ChangedColorMask, FirstDraw;
        private Texture2D LastColorMaskPaintedOn;
        private void ControlsLogic()
        {
            if (SVGMC.CurrentTex2DColorMask != LastChosenRTColorMask) ChangedColorMask = true;
            LastChosenRTColorMask = SVGMC.CurrentTex2DColorMask;
            if (SVGMC.CurrentTex2DColorMask == LastColorMaskPaintedOn) ChangedColorMask = false;
            if (SVGMCTextureDrawing.RayHitASplatMap == true && SVGMC.PressingAlt == false
                && SVGMC.MouseLastClickWasInsideUI == false && SVGMC.MouseLeftClicking == true)
            {
                SaveForUndoRedoCheckBeforePainting();
                SVGMCTextureDrawing.SaturateResultOfCurrentBrush = false;
                SVGMCTextureDrawing.ClampMinimumOfCurrentBrushTo0 = false;
                //Draw 
                SVGMCTextureDrawing.SaturateCurrentBrush = true;
                if (SVGMC.UseNormalBrush == true) SVGMCTextureDrawing.SimpleDraw(SVGMC.ColorMasksRenderTextures[SVGMC.CurrentTex2DColorMask],
                    SVGMCTextureDrawing.PaintTextCoordinate, Color.white, true, false, 1.0f);
                SVGMCTextureDrawing.SaturateCurrentBrush = false;
                //Soften
                if (SVGMC.UseSoftenBrush == true) SVGMCTextureDrawing.SoftenPaint(SVGMC.ColorMasksRenderTextures[SVGMC.CurrentTex2DColorMask], SVGMCTextureDrawing.PaintTextCoordinate);
                //Texture Brush
                SVGMCTextureDrawing.SaturateCurrentBrush = true;
                if (SVGMC.UseTextureBrush == true) SVGMCTextureDrawing.TexturePaint(SVGMC.ColorMasksRenderTextures[SVGMC.CurrentTex2DColorMask],
                    SVGMCTextureDrawing.PaintTextCoordinate, true, SVGMC.HeightTextureBrushSelected);
                SVGMCTextureDrawing.SaturateCurrentBrush = false;
                //Light Brush
                if (SVGMC.UseLightBrush == true && SVGMC.InvertCurrentBrush == false) SVGMCTextureDrawing.SimpleDraw(SVGMC.ColorMasksRenderTextures[SVGMC.CurrentTex2DColorMask],
                    SVGMCTextureDrawing.PaintTextCoordinate, Color.white, true, false);
                if (SVGMC.UseLightBrush && SVGMC.InvertCurrentBrush == true) SVGMCTextureDrawing.NormalizeProgressively(SVGMC.ColorMasksRenderTextures[SVGMC.CurrentTex2DColorMask],
                    SVGMCTextureDrawing.PaintTextCoordinate);
                SaveForRevertTrigger1 = true;
                AssignResult();
                LastColorMaskPaintedOn = SVGMC.CurrentTex2DColorMask;
            }
            //Save for Revert 
            if (SVGMC.MouseLeftClicking == false || SVGMCTextureDrawing.RayHitASplatMap == false)
            {
                SVGMCTextureDrawing.ChangeTextureBrushIndex = true;
                if (SVGMC.MouseLastClickWasInsideUI == false)
                {
                    if (SaveForRevertTrigger1 == true)
                    {
                        SaveForUndoRedoMethod();
                        SaveForRevertTrigger1 = false;
                    }
                }
            }
        }

        private void SaveForUndoRedoMethod()
        {
            SVGMCTextureDrawing.SaveTextureForRevert(SVGMC.ColorMasksRenderTextures[SVGMC.CurrentTex2DColorMask]);
            RevertMaskTex2D.Add(SVGMC.CurrentTex2DColorMask);
            CurrentUndoRedoKey = 0;
            UndoRedoToRemove = 0;
        }

        private void AssignResult()
        {
            RenderTexture tempTexture = RenderTexture.GetTemporary(SVGMCTextureDrawing.CurrentPaintingTexture.width,
                SVGMCTextureDrawing.CurrentPaintingTexture.height, 0, SVGMCTextureDrawing.currentRTFormat);
            FillerMat.SetColor("_Color", Color.black);
            RenderTexture tempTextureErase = RenderTexture.GetTemporary(SVGMCTextureDrawing.CurrentPaintingTexture.width,
                SVGMCTextureDrawing.CurrentPaintingTexture.height, 0, SVGMCTextureDrawing.currentRTFormat);
            Graphics.Blit(tempTextureErase, tempTexture, FillerMat);
            RenderTexture.ReleaseTemporary(tempTextureErase);
            for (int i = 0; i < SVGMC.UnSavedColorMasks.Count; i++)
            {
                AddTextureMat.SetTexture("_AddedTex", SVGMC.ColorMasksRenderTextures[SVGMC.UnSavedColorMasks[i]]);
                AddTextureMat.SetColor("_Color", SVGMC.ChosenColorsForPainting[SVGMC.UnSavedColorMasks[i]]);
                Graphics.Blit(tempTexture, SVGMCTextureDrawing.CurrentPaintingTexture, AddTextureMat);
                Graphics.Blit(SVGMCTextureDrawing.CurrentPaintingTexture, tempTexture);
            }
            SVGMCTextureDrawing.SeperateAndAssignRenderTextures();
            RenderTexture.ReleaseTemporary(tempTexture);
        }

        private Texture2D CreateColorMaskAssets(Texture2D TextureToSave)
        {
            SVGMC.CreatedDirectoryIfNecessary();

            //Check if Folder Exists, create a new one if not 
            string NewFolderTexturesPath = SVGMC.CreatedFilesDirectoryPath + "/_Textures";//UnderScore because it's added to the folder automatically
            if (AssetDatabase.IsValidFolder(NewFolderTexturesPath) == false) AssetDatabase.CreateFolder(SVGMC.CreatedFilesDirectoryPath, "/Textures");
            //Check if Folder Exists, create a new one if not 
            string NewFolderTexturesPath2 = NewFolderTexturesPath + "/_ColorMasks";//UnderScore because it's added to the folder automatically
            if (AssetDatabase.IsValidFolder(NewFolderTexturesPath2) == false) AssetDatabase.CreateFolder(NewFolderTexturesPath, "/ColorMasks");

            //Check the already created Textures 
            DirectoryInfo Directory = new DirectoryInfo(NewFolderTexturesPath2);
            int AddedInt = 1;
            string[] FoldersToSearch = new string[] { NewFolderTexturesPath2 };
            string[] AlreadyCreatedTextures = AssetDatabase.FindAssets("t:texture2D", FoldersToSearch);
            //Get the highest numbers + 1
            for (int i = 0; i < AlreadyCreatedTextures.Length; i++)
            {
                string currentTexPath = AssetDatabase.GUIDToAssetPath(AlreadyCreatedTextures[i]);
                //If contains "Color"
                if (currentTexPath.Contains(NewFolderTexturesPath2) && currentTexPath.Contains("ColorMask"))
                {
                    int EndingNumbers = 0;
                    char[] SeperatorChar = new char[] { char.Parse("_"), char.Parse(".") };
                    string[] PathSplit = currentTexPath.Split(SeperatorChar);
                    int.TryParse(PathSplit[PathSplit.Length - 2], out EndingNumbers);
                    if (EndingNumbers >= AddedInt) { AddedInt = EndingNumbers + 1; }
                }
            }
            //Create Assets 
            DirectoryInfo DirectoryCreatedGrassFields = new DirectoryInfo(SVGMC.CreatedFilesDirectoryPath);
            string NewTexturePathInt = NewFolderTexturesPath2 + "/" + DirectoryCreatedGrassFields.Name + "ColorMask" + "_" + AddedInt + ".exr";
            string OriginalPath = AssetDatabase.GetAssetPath(OriginalTexture);
            AssetDatabase.CopyAsset(OriginalPath, NewTexturePathInt);
            TextureToSave = AssetDatabase.LoadAssetAtPath<Texture2D>(NewTexturePathInt);
            SVGMC.CreatedAssets.Add(TextureToSave);
            EditorUtility.SetDirty(SVGMC);
            for (int y = 0; y < TextureToSave.height; y++)
            {
                for (int x = 0; x < TextureToSave.width; x++)
                {
                    TextureToSave.SetPixel(x, y, Color.black);
                }
            }
            TextureToSave.Apply();
            return TextureToSave;
        }

        private void SaveTexture(RenderTexture RenderTextureToSave, Texture2D TextureToSaveAt)
        {
            string NewTexturePathInt = AssetDatabase.GetAssetPath(TextureToSaveAt);
            if (NewTexturePathInt != "" && NewTexturePathInt != null)
            {
                var tex = new Texture2D(RenderTextureToSave.width, RenderTextureToSave.height, TextureFormat.RGBAFloat, false);
                RenderTexture.active = RenderTextureToSave;

                tex.ReadPixels(new Rect(0, 0, RenderTextureToSave.width, RenderTextureToSave.height), 0, 0);
                tex.Apply();

                System.IO.File.WriteAllBytes(NewTexturePathInt, tex.EncodeToEXR(Texture2D.EXRFlags.CompressZIP));
                AssetDatabase.ImportAsset(NewTexturePathInt);
                DestroyImmediate(tex);
            }
            //Assign Textures
            TextureToSaveAt = AssetDatabase.LoadAssetAtPath<Texture2D>(NewTexturePathInt);
            Graphics.Blit(RenderTextureToSave, SaveCheckRTs[RenderTextureToSave]);
        }
#endif

    }

}