using UnityEngine;
using UnityEditor;

namespace VibrantGrassShader
{
    [ExecuteInEditMode]
    public class GrassShadowsPainter : MonoBehaviour
    {
#pragma warning disable 0219
#pragma warning disable 0414

        [SerializeField, HideInInspector] private MainControls SVGMainControls = null;
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
                if (SVGMCTextureDrawing.RayHitASplatMap == true && SVGMainControls.PressingAlt == false
                    && SVGMainControls.MouseLastClickWasInsideUI == false && SVGMainControls.MouseLeftClicking == true)
                {
                    //Draw
                    SVGMCTextureDrawing.SaturateResultOfCurrentBrush = true;
                    SVGMCTextureDrawing.SaturateCurrentBrush = false;
                    SVGMCTextureDrawing.ClampMinimumOfCurrentBrushTo0 = false;
                    if (SVGMainControls.UseNormalBrush == true) SVGMCTextureDrawing.SimpleDraw(SVGMCTextureDrawing.CurrentPaintingTexture,
                        SVGMCTextureDrawing.PaintTextCoordinate, Color.red, false, true, 0.25f, 3.0f);
                    //Soften
                    if (SVGMainControls.UseSoftenBrush == true) SVGMCTextureDrawing.SoftenPaint(SVGMCTextureDrawing.CurrentPaintingTexture, SVGMCTextureDrawing.PaintTextCoordinate);
                    SaveForRevertTrigger1 = true;
                }
                //Save for Revert 
                if (SVGMainControls.MouseLeftClicking == false || SVGMCTextureDrawing.RayHitASplatMap == false)
                {
                    if (SVGMainControls.PressingAlt == false && SVGMainControls.MouseLastClickWasInsideUI == false)
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
                SVGMainControls = GetComponent<MainControls>();
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