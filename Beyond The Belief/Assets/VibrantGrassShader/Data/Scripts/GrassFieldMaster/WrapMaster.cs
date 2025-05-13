using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VibrantGrassShaderTools;
using UnityEditor;

namespace VibrantGrassShader
{
    [ExecuteInEditMode]
    public class WrapMaster : MonoBehaviour
    {
#pragma warning disable 0219
#pragma warning disable 0414

        [Foldout("Don't Touch", true)]
        [SerializeField] GrassFieldMaster grassShaderMaster;
        [Foldout("Don't Touch", true)]
        [SerializeField] private bool Wrap = false, WrapCutMeshes = false;
        [SerializeField] public bool CutOutOfGroundMeshWhenWrapping = false;
        [SerializeField] public bool ShowDebugs = false;
        [SerializeField, HideInInspector] public LayerMask GrassGroundLayers = 0;
        [SerializeField] public int MaximumIterationsPerFrame = 9999;
        public delegate void WrapDelegate();
        public WrapDelegate WrapEvent, WrapCutMeshesEvent;
        private MainControls SVGMC;
#pragma warning restore 0219
#pragma warning restore 0414

        private void Start()
        {
            if (Application.isPlaying == true) Destroy(this);
        }
#if UNITY_EDITOR
        private void OnEnable()
        {
            if (Application.isPlaying == false)
            {
                EditorApplication.update += EditorUpdates;
                grassShaderMaster.WrapEvent += WrapMethod;
                grassShaderMaster.WrapCutMeshes += WrapCutMeshesMethod;
                SVGMC = GetComponentInParent<MainControls>();
                if (SVGMC != null && transform.parent != null)
                {
                    GrassGroundLayers = SVGMC.GroundLayers;
                }
            }
            Wrap = false;
            WrapCutMeshes = false;
        }

        private void OnDisable()
        {
            if (Application.isPlaying == false)
            {
                EditorApplication.update -= EditorUpdates;
                grassShaderMaster.WrapEvent -= WrapMethod;
                grassShaderMaster.WrapCutMeshes -= WrapCutMeshesMethod;
            }
        }


        private void EditorUpdates()
        {
            if (Application.isPlaying == false && this != null)
            {
                GrassGroundLayers = SVGMC.GroundLayers;
                MaximumIterationsPerFrame = GetMaxIterationMultipleOfThree(SVGMC.MaxWrapVerticesPerFrame);
                if (Wrap == true)
                {
                    WrapMethod();
                    Wrap = false;
                }
                if (WrapCutMeshes == true)
                {
                    WrapCutMeshesMethod();
                    WrapCutMeshes = false;
                }
            }
        }
        public void WrapMethod() { WrapEvent?.Invoke(); }
        public void WrapCutMeshesMethod() { WrapCutMeshesEvent?.Invoke(); }

        private int GetMaxIterationMultipleOfThree(int Value)
        {
            float a = Value / 3.0f;
            int Rounded = Mathf.RoundToInt(a);
            int Result = Rounded * 3;
            return Result;
        }
#endif
    }
}