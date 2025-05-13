using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace VibrantGrassShader
{
#if UNITY_EDITOR
    [ExecuteInEditMode]
    public class ControlArrowsCircle : MonoBehaviour
    {
        private ControlArrows SVGControlArrows;
        private MeshRenderer meshRenderer, ParentMeshRenderer;

        private void OnEnable()
        {
            if (Application.isPlaying == false)
            {
                SVGControlArrows = GetComponentInParent<ControlArrows>();
                ParentMeshRenderer = transform.parent.gameObject.GetComponent<MeshRenderer>();
                meshRenderer = GetComponent<MeshRenderer>();
                meshRenderer.enabled = false;
                EditorApplication.update += EditorUpdate;
            }
        }

        private void OnDisable()
        {
            if (Application.isPlaying == false) EditorApplication.update -= EditorUpdate;
        }

        private int FrameCount;
        public bool test;
        void EditorUpdate()
        {
            if (Application.isPlaying == false && this != null)
            {
                if (FrameCount == 0)
                {
                    meshRenderer.sharedMaterial = SVGControlArrows.InstancedMaterial;
                }
                transform.LookAt(SVGControlArrows.ScreenWorldPosition);
                if (ParentMeshRenderer.enabled == false) meshRenderer.enabled = false;
                else meshRenderer.enabled = true;
                FrameCount += 1;
            }
        }
    }
#endif

}