using System.Collections.Generic;
using UnityEngine;
using VibrantGrassShaderTools;
using UnityEditor;

namespace VibrantGrassShader
{
    [ExecuteInEditMode]
    public class GrassMeshHeightCutter : MonoBehaviour
    {
        [Foldout("Don't Touch", true)]
        [SerializeField] private int MaximumIterationsPerFrame;
        private GrassFieldMaster _grassFieldMaster;
        private MeshFilter meshFilter;
        private MeshRenderer _meshRenderer;
        [SerializeField, HideInInspector] private GrassWrap GSWrap;

        private bool FirstFrame, CutFirstFrame;
        private void Start()
        {
            if (Application.isPlaying == true) Destroy(this);
        }

#if UNITY_EDITOR

        void EditorUpdates()
        {
            if (Application.isPlaying == false && this != null)
            {
                if (FirstFrame == false)
                {
                    AddMethodsToMainControlEvent();
                    FirstFrame = true;
                }
                if (Cutting == true)
                {
                    if (GSWrap.CreatedMesh != null)
                    {
                        if (CutFirstFrame == true)
                        {
                            NewMesh = GSWrap.CreatedMesh;
                            OldMesh = meshFilter.sharedMesh;
                            OldVertices = OldMesh.vertices;
                            OldTriangles = OldMesh.triangles;
                            OldUV2s = OldMesh.uv2;
                            OldUV1s = OldMesh.uv;
                            NewMesh.vertices = OldVertices;
                            NewMesh.triangles = OldTriangles;
                            AllKeptVertices = new List<Vector3>();
                            AllKeptTriangles = new List<int>();
                            CurrentTrianglesIteration = 0;
                            TrianglesIterationsLeft = 100;
                            CutFirstFrame = false;
                        }
                        CutMethod();
                    }
                }
            }
        }
        private void OnEnable()
        {
            if (Application.isPlaying == false)
            {
                EditorApplication.update += EditorUpdates;
                _grassFieldMaster = GetComponentInParent<GrassFieldMaster>();
                meshFilter = GetComponent<MeshFilter>();
                Cutting = false;
                FirstFrame = false;
                GSWrap = GetComponent<GrassWrap>();
                _meshRenderer = GetComponent<MeshRenderer>();
            }
        }
        private void OnDisable()
        {
            if (Application.isPlaying == false)
            {
                EditorApplication.update -= EditorUpdates;
                RemoveMethodsToMainControlEvent();
            }
        }


        private void AddMethodsToMainControlEvent()
        {
            _grassFieldMaster.StartCuttingEvent += StartCuttingMethod;
        }
        private void RemoveMethodsToMainControlEvent()
        {
            _grassFieldMaster.StartCuttingEvent -= StartCuttingMethod;
        }

        private Mesh OldMesh;
        private bool Cutting;
        private List<Vector3> AllKeptVertices = new List<Vector3>();
        private List<int> AllKeptTriangles = new List<int>();
        private int[] OldTriangles;
        private Vector3[] OldVertices;
        private int CurrentTrianglesIteration;
        private int TrianglesIterationsLeft;
        private Vector2[] OldUV2s, OldUV1s;
        [SerializeField, HideInInspector] private Mesh NewMesh = null;
        [SerializeField, HideInInspector] private float HeightCutThreshold = 0.0f;
        private void CutMethod()
        {
            if (TrianglesIterationsLeft <= 0)
            {
                if (AllKeptTriangles.Count > 0)
                {
                    int[] TrianglesResult = new int[AllKeptTriangles.Count];
                    for (int m = 0; m < AllKeptTriangles.Count; m++)
                    {
                        TrianglesResult[m] = AllKeptTriangles[m];
                    }
                    NewMesh.triangles = TrianglesResult;
                }
                NewMesh.RecalculateBounds();
                meshFilter.sharedMesh = NewMesh;
                Cutting = false;
                return;
            }
            TrianglesIterationsLeft = OldTriangles.Length - CurrentTrianglesIteration;
            int IterationsToDo2 = Mathf.RoundToInt(Mathf.Clamp(TrianglesIterationsLeft, 0.0f, MaximumIterationsPerFrame));
            int j = CurrentTrianglesIteration;
            if (IterationsToDo2 > 0)
            {
                int IterationCeiling = Mathf.RoundToInt(Mathf.Clamp((float)IterationsToDo2 + (float)CurrentTrianglesIteration, 0.0f, OldTriangles.Length));
                while (j < IterationCeiling)
                {
                    int p = j;
                    bool TipIsFlattened = false;
                    while (p <= j + 2)
                    {
                        int Index = OldTriangles[p];
                        float rValue = _grassFieldMaster.HeightTexture.GetPixel
                            (Mathf.FloorToInt(OldUV2s[Index].x * _grassFieldMaster.HeightTexture.width), Mathf.FloorToInt(OldUV2s[Index].y * _grassFieldMaster.HeightTexture.height)).r;
                        float TipValue = OldUV1s[Index].y;
                        if (rValue <= HeightCutThreshold && TipValue > 0.9f) TipIsFlattened = true;
                        p += 1;
                    }
                    if (TipIsFlattened == false)
                    {
                        AllKeptTriangles.Add(OldTriangles[j]);
                        AllKeptTriangles.Add(OldTriangles[j + 1]);
                        AllKeptTriangles.Add(OldTriangles[j + 2]);
                    }
                    j += 3;
                }
            }

            CurrentTrianglesIteration += MaximumIterationsPerFrame;
        }

        private void StartCuttingMethod()
        {
            bool VerticesHaveToBeCut = false;
            if (_grassFieldMaster == null) _grassFieldMaster = GetComponentInParent<GrassFieldMaster>();
            if (_grassFieldMaster.HeightTexture != null)
            {
                NewMesh = GSWrap.CreatedMesh;
                if (NewMesh == null)
                {
                    //Check if need to create and wrap a new mesh
                    HeightCutThreshold = _meshRenderer.sharedMaterial.GetFloat("_HeightCutThreshold");
                    Color[] pixelColors = _grassFieldMaster.HeightTexture.GetPixels();
                    for (int i = 0; i < pixelColors.Length; i++)
                    { if (pixelColors[i].r <= HeightCutThreshold) VerticesHaveToBeCut = true; }
                    if (VerticesHaveToBeCut == true)
                    {
                        GSWrap.CutWithHeightTexture = true;
                        _grassFieldMaster.WrapMethod();
                        CutFirstFrame = true;
                        Cutting = true;
                    }
                }
            }
        }
#endif

    }
}