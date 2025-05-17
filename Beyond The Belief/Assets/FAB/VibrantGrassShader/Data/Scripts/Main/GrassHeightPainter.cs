using UnityEngine;
using UnityEditor;

namespace VibrantGrassShader
{
    [ExecuteInEditMode]
    public class GrassHeightPainter : MonoBehaviour
    {
#pragma warning disable 0219
#pragma warning disable 0414

        [SerializeField, HideInInspector] private MainControls SVGMC = null;
        [SerializeField, HideInInspector] private GrassTextureDrawing SVGMCTextureDrawing = null;
        private bool SaveForRevertTrigger1;
#pragma warning restore 0219
#pragma warning restore 0414

        private void Start()
        {
            if (Application.isPlaying == true) Destroy(this);
        }

#if UNITY_EDITOR

        void EditorUpdates()
        {
            if (Application.isPlaying == false && this != null)
            {
                if (SVGMCTextureDrawing.RayHitASplatMap == true && SVGMC.PressingAlt == false
                    && SVGMC.MouseLastClickWasInsideUI == false && SVGMC.MouseLeftClicking == true)
                {
                    bool InvertHeightBrushInverted = false;
                    if (SVGMC.InvertHeightBrush == false) InvertHeightBrushInverted = true;
                    //Draw
                    SVGMCTextureDrawing.SaturateResultOfCurrentBrush = false;
                    SVGMCTextureDrawing.SaturateCurrentBrush = false;
                    SVGMCTextureDrawing.ClampMinimumOfCurrentBrushTo0 = true;
                    //SVGMCTextureDrawing.SaturateCurrentBrush = true;
                    //Vector2 HeightPaintingMinMaxApplied = new Vector2(SVGMC.HeightPaintingMinMax.x - SVGMCTextureDrawing.HeightDefaultValue, 
                    //    SVGMC.HeightPaintingMinMax.y - SVGMCTextureDrawing.HeightDefaultValue);
                    if (SVGMC.UseNormalBrush == true) SVGMCTextureDrawing.SimpleDraw(SVGMCTextureDrawing.CurrentPaintingTexture,
                        SVGMCTextureDrawing.PaintTextCoordinate, Color.white, InvertHeightBrushInverted, true, 0.1f, 2.0f, SVGMC.HeightPaintingFloorAndCeiling);
                    //Soften
                    if (SVGMC.UseSoftenBrush == true) SVGMCTextureDrawing.SoftenPaint(SVGMCTextureDrawing.CurrentPaintingTexture, SVGMCTextureDrawing.PaintTextCoordinate);
                    //Texture Brush
                    if (SVGMC.UseTextureBrush == true) SVGMCTextureDrawing.TexturePaint(SVGMCTextureDrawing.CurrentPaintingTexture,
                        SVGMCTextureDrawing.PaintTextCoordinate, InvertHeightBrushInverted, SVGMC.HeightTextureBrushSelected, SVGMC.HeightPaintingFloorAndCeiling, 0.1f);
                    SaveForRevertTrigger1 = true;
                }
                //Save for Revert 
                if (SVGMC.MouseLeftClicking == false || SVGMCTextureDrawing.RayHitASplatMap == false)
                {
                    SVGMCTextureDrawing.ChangeTextureBrushIndex = true;
                    if (SVGMC.PressingAlt == false && SVGMC.MouseLastClickWasInsideUI == false)
                    {
                        if (SaveForRevertTrigger1 == true)
                        {
                            SVGMCTextureDrawing.SaveTextureForRevert(SVGMCTextureDrawing.CurrentPaintingTexture);
                            SaveForRevertTrigger1 = false;
                        }
                    }
                }
            }
        }

        private void OnEnable()
        {
            if (Application.isPlaying == false)
            {
                EditorApplication.update += EditorUpdates;
                SVGMCTextureDrawing = GetComponent<GrassTextureDrawing>();
                SVGMC = GetComponent<MainControls>();
                SaveForRevertTrigger1 = false;
            }
        }

        private void OnDisable()
        {
            if (Application.isPlaying == false)
            {
                EditorApplication.update -= EditorUpdates;
            }
        }

#endif

    }

}