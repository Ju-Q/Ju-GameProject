using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VibrantGrassShaderTools;
using System;

namespace VibrantGrassShader
{
    [ExecuteInEditMode]
    public class GroundNormalTextureMaster : MonoBehaviour
    {
        [Foldout("Data (don't touch)", true)]
#pragma warning disable 0219 
#pragma warning disable 0414 
        [SerializeField] private GrassFieldCreator VGSFieldCreator = null;
        [SerializeField] private int SmoothingStrength = 0;
        [SerializeField] public RenderTexture _targetRT = null;
        [SerializeField] private Vector3[] GrassCorners = new Vector3[4];
        [SerializeField] private int AmountOfRaysPerLine;
        [SerializeField] private GrassFieldMaster grassShaderMaster;
        [SerializeField] private GrassWrap GrassMeshWrap;
        [SerializeField] private WrapMaster GSWrapMaster;
        [SerializeField] private GrassMeshMaster GSMeshMaster;
        [SerializeField] private bool Debugging = false;
        [SerializeField] private int TextureSize = 64;
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
                grassShaderMaster = GetComponentInParent<GrassFieldMaster>();
                GrassMeshWrap = GetComponent<GrassWrap>();
                AddEventsToWrap();
                EventsAdded = false;
            }
        }
        private bool EventsAdded;
        private void OnDisable()
        {
            if (Application.isPlaying == false)
            {
                EditorApplication.update -= EditorUpdates;
                if (EventsAdded == true) RemoveEventsToWrap();
                EventsAdded = false;
            }
        }
        void EditorUpdates()
        {
            if (Application.isPlaying == false && this != null)
            {
                if (Debugging == true) DebuggingMethod();
                //if (grassShaderMaster.Test == true)
                //{
                //    //Array.Clear(grassShaderMaster.GrassFieldPositionsGrid, 0, grassShaderMaster.GrassFieldPositionsGrid.Length);
                //    //Array.Clear(grassShaderMaster.GrassFieldNormalUVs, 0, grassShaderMaster.GrassFieldNormalUVs.Length);
                //    //Array.Clear(grassShaderMaster.GroundAverageNormals, 0, grassShaderMaster.GroundAverageNormals.Length);
                //    //Array.Clear(grassShaderMaster.GrassFieldGroundPositionsGrid, 0, grassShaderMaster.GrassFieldGroundPositionsGrid.Length);
                //    //Debug.Log(grassShaderMaster.GrassFieldPositionsGrid.Length);
                //    //grassShaderMaster.GrassFieldNormalUVs = null;
                //    //grassShaderMaster.GroundAverageNormals = null;
                //    //grassShaderMaster.GrassFieldGroundPositionsGrid = null;
                //    grassShaderMaster.Test = false;
                //}
            }
        }

        private void DebuggingMethod()
        {
            Debug.Log(grassShaderMaster.GroundAverageNormals.Length);
            for (int i = 0; i < grassShaderMaster.GroundAverageNormals.Length; i++)
            {
                Debug.DrawLine(grassShaderMaster.GrassFieldGroundPositionsGrid[i], grassShaderMaster.GrassFieldGroundPositionsGrid[i] + grassShaderMaster.GroundAverageNormals[i] * 50.0f, Color.green, 10.0f);
            }
            Debugging = false;
        }

        private void GetExtremitiesAndMakeAGrid()
        {
            GrassCorners = new Vector3[4];
            float HalfFieldDist = VGSFieldCreator.VGSMC.GrassFieldMaxDistance / 2.0f;
            Vector3 BotLeftGlobal = new Vector3(-HalfFieldDist, 0.0f, -HalfFieldDist);
            Vector3 BotRightGlobal = new Vector3(HalfFieldDist, 0.0f, -HalfFieldDist);
            Vector3 TopleftGlobal = new Vector3(-HalfFieldDist, 0.0f, HalfFieldDist);
            Vector3 TopRightGlobal = new Vector3(HalfFieldDist, 0.0f, HalfFieldDist);
            GrassCorners[0] = transform.TransformPoint(BotLeftGlobal);
            GrassCorners[1] = transform.TransformPoint(BotRightGlobal);
            GrassCorners[2] = transform.TransformPoint(TopleftGlobal);
            GrassCorners[3] = transform.TransformPoint(TopRightGlobal);

            grassShaderMaster.GrassFieldPositionsGrid = new Vector3[AmountOfRaysPerLine * AmountOfRaysPerLine];
            grassShaderMaster.GrassFieldNormalUVs = new Vector2[grassShaderMaster.GrassFieldPositionsGrid.Length];
            float distPerRay = VGSFieldCreator.VGSMC.GrassFieldMaxDistance / (AmountOfRaysPerLine - 1);
            for (int x = 0; x < AmountOfRaysPerLine; x++)
            {
                float currentXDist = distPerRay * x;
                for (int y = 0; y < AmountOfRaysPerLine; y++)
                {
                    float currentYDist = distPerRay * y;
                    Vector3 PosToAdd = GrassCorners[0] + (transform.TransformDirection(Vector3.right) * currentXDist) + (transform.TransformDirection(Vector3.forward) * currentYDist);
                    int CurrentIndex = (x * AmountOfRaysPerLine) + y;
                    grassShaderMaster.GrassFieldPositionsGrid[CurrentIndex] = PosToAdd;
                    Vector3 PosLocalized = transform.InverseTransformPoint(PosToAdd);
                    float XUV = Mathf.InverseLerp(-HalfFieldDist, HalfFieldDist, PosLocalized.x);
                    float YUV = Mathf.InverseLerp(-HalfFieldDist, HalfFieldDist, PosLocalized.z);
                    grassShaderMaster.GrassFieldNormalUVs[CurrentIndex] = new Vector2(XUV, YUV);
                }
            }
            grassShaderMaster.GroundAverageNormals = new Vector3[grassShaderMaster.GrassFieldPositionsGrid.Length];
            grassShaderMaster.GrassFieldGroundPositionsGrid = new Vector3[grassShaderMaster.GrassFieldPositionsGrid.Length];
            List<Vector3> PositionsOnGround = new List<Vector3>();
            for (int i = 0; i < grassShaderMaster.GrassFieldPositionsGrid.Length; i++)
            {
                Vector3 StartPos = grassShaderMaster.GrassFieldPositionsGrid[i] + Vector3.up * grassShaderMaster.VGSMC.WrapBasePosAddedHeight;
                RaycastHit raycastHit = new RaycastHit();
                bool rayHit = false;
                RaycastHit[] allRayHits = Physics.RaycastAll(StartPos, Vector3.down, grassShaderMaster.VGSMC.WrapMaxRayDistance, GSWrapMaster.GrassGroundLayers, QueryTriggerInteraction.Ignore);
                for (int j = 0; j < allRayHits.Length; j++)
                {
                    raycastHit = allRayHits[j];
                    rayHit = true;
                }

                if (rayHit == true)
                {
                    grassShaderMaster.GroundAverageNormals[i] = raycastHit.normal;
                    grassShaderMaster.GrassFieldGroundPositionsGrid[i] = raycastHit.point;
                    PositionsOnGround.Add(raycastHit.point);
                }
                else
                {
                    grassShaderMaster.GrassFieldGroundPositionsGrid[i] = grassShaderMaster.GrassFieldPositionsGrid[i];
                }
            }
            if (grassShaderMaster.GroundNormalTexture == null)
            {
                grassShaderMaster.CreateNormalTexture = true;
                grassShaderMaster.CreateAndAssignTextures = true;
                grassShaderMaster.CreateTextureInProject();
                grassShaderMaster.CreateNormalTexture = false;
                grassShaderMaster.CreateAndAssignTextures = false;
            }
            MakeATextureOfGroundVector();
            grassShaderMaster.ClearCollections = true;
            EditorUtility.SetDirty(this);
            EditorUtility.SetDirty(grassShaderMaster);
        }

        private void MakeATextureOfGroundVector()
        {
            _targetRT = new RenderTexture(TextureSize, TextureSize, 0, RenderTextureFormat.ARGBFloat);
            int MaximumArraySize = 900;
            int AmountOfSteps = 1;
            if (grassShaderMaster.GroundAverageNormals.Length > MaximumArraySize)
            {
                AmountOfSteps = Mathf.CeilToInt((float)grassShaderMaster.GroundAverageNormals.Length / (float)MaximumArraySize);
            }
            for (int S = 0; S < AmountOfSteps; S++)
            {
                List<Vector4> normalVector4List = new List<Vector4>();
                List<Vector4> uvVector4List = new List<Vector4>();
                int CurrentArraySize = Mathf.FloorToInt(Mathf.Clamp((float)grassShaderMaster.GroundAverageNormals.Length,
                    0.0f, Mathf.Clamp(grassShaderMaster.GroundAverageNormals.Length - (MaximumArraySize * S), 0.0f, MaximumArraySize)));
                for (int i = 0; i < CurrentArraySize; i++)
                {
                    int Index = (MaximumArraySize * S) + i;
                    Vector3 _normalVector = transform.InverseTransformDirection(grassShaderMaster.GroundAverageNormals[Index]).normalized;
                    //Vector3 Cross1 = Vector3.Cross(_normalVector, Vector3.up);
                    //Vector3 SlopeDirection = Vector3.Cross(_normalVector, - Cross1);
                    //float SlopeDegrees = Vector3.Angle(Vector3.up, SlopeDirection);
                    //normalVectorRemapped0To1.Add(new Vector4(Mathf.InverseLerp(-1.0f, 1.0f, _normalVector.x), Mathf.InverseLerp(-1.0f, 1.0f, _normalVector.y), Mathf.InverseLerp(-1.0f, 1.0f, _normalVector.z), SlopeDegrees));
                    normalVector4List.Add(new Vector4(_normalVector.x, _normalVector.y, _normalVector.z, 0.0f));
                    //normalVectorRemapped0To1.Add(new Vector4(Mathf.InverseLerp(-1.0f, 1.0f, _normalVector.x), Mathf.InverseLerp(-1.0f, 1.0f, _normalVector.y), Mathf.InverseLerp(-1.0f, 1.0f, _normalVector.z), 0.0f));
                    uvVector4List.Add(new Vector4(1 - grassShaderMaster.GrassFieldNormalUVs[Index].x, 1 - grassShaderMaster.GrassFieldNormalUVs[Index].y, 0.0f, 0.0f));
                }
                float UVDistOfEachVector = 1.0f / (AmountOfRaysPerLine - 1);
                grassShaderMaster.VGSMC.VertexNormalPainterMat.SetVectorArray("_normalVectors", normalVector4List);
                grassShaderMaster.VGSMC.VertexNormalPainterMat.SetVectorArray("_uvCoordinates", uvVector4List);
                grassShaderMaster.VGSMC.VertexNormalPainterMat.SetFloat("_DistForEachVector", UVDistOfEachVector);
                grassShaderMaster.VGSMC.VertexNormalPainterMat.SetFloat("_ArraysLength", uvVector4List.Count);
                RenderTexture tempTexture = RenderTexture.GetTemporary(_targetRT.width, _targetRT.height, 0, RenderTextureFormat.ARGBFloat);
                Graphics.Blit(_targetRT, tempTexture);
                Graphics.Blit(tempTexture, _targetRT, grassShaderMaster.VGSMC.VertexNormalPainterMat);
                RenderTexture.ReleaseTemporary(tempTexture);
            }
            for (int i = 0; i < SmoothingStrength; i++)
            {
                RenderTexture tempTexture2 = RenderTexture.GetTemporary(_targetRT.width, _targetRT.height, 0, RenderTextureFormat.ARGBFloat);
                Graphics.Blit(_targetRT, tempTexture2, grassShaderMaster.VGSMC.TextureColorSoftenerMat);
                Graphics.Blit(tempTexture2, _targetRT);
                RenderTexture.ReleaseTemporary(tempTexture2);
            }
            SaveRenderTextureToPNG(_targetRT);
            GSMeshMaster.MainMatInstanced.SetTexture("_groundNormalTexture", grassShaderMaster.GroundNormalTexture);
        }

        private void AddEventsToWrap()
        {
            GrassMeshWrap.WrapJustFinishedEvent += GetExtremitiesAndMakeAGrid;
            EventsAdded = true;
        }
        private void RemoveEventsToWrap()
        {
            GrassMeshWrap.WrapJustFinishedEvent -= GetExtremitiesAndMakeAGrid;
            EventsAdded = false;
        }

        private void SaveRenderTextureToPNG(RenderTexture RTToSave)
        {
            //Set Color Texture 
            string GroundNormalTexturePath = AssetDatabase.GetAssetPath(grassShaderMaster.GroundNormalTexture);
            if (GroundNormalTexturePath != "" && GroundNormalTexturePath != null)
            {
                var tex = new Texture2D(RTToSave.width, RTToSave.height, TextureFormat.RGBAFloat, false);
                RenderTexture.active = RTToSave;

                tex.ReadPixels(new Rect(0, 0, RTToSave.width, RTToSave.height), 0, 0);
                tex.Apply();

                //File.WriteAllBytes(GroundNormalTexturePath, tex.EncodeToEXR(Texture2D.EXRFlags.CompressZIP));//No idea why Encoding To EXR is what makes it work. 
                //File.WriteAllBytes(GroundNormalTexturePath, tex.EncodeToEXR());
                System.IO.File.WriteAllBytes(GroundNormalTexturePath, tex.EncodeToEXR(Texture2D.EXRFlags.CompressZIP));
                //EncodeToPNG doesn't keep the right value, even if the values are from 0 t 1. The file is in PNG too... 
                AssetDatabase.ImportAsset(GroundNormalTexturePath);
                DestroyImmediate(tex);
            }
        }
#endif
    }

}