using System.Collections.Generic;
using UnityEngine;
using VibrantGrassShaderTools;

namespace VibrantGrassShader
{
    public class LightAndInteractionMaster : MonoBehaviour
    {
        [Foldout("Data (Don't Touch)", true)]
        [SerializeField] private Shader _lightDrawShader = null, _interactDrawShader;

        [HideInInspector] public Dictionary<GameObject, LightAndInteractObjectsDetection.VGSLIMainValues> LightAndInteractObjects = new Dictionary<GameObject, LightAndInteractObjectsDetection.VGSLIMainValues>();

        [HideInInspector] public Material _LightDrawMaterial, _InteractDrawMaterial;
        public delegate void VGSLIMasterDelegate();
        public VGSLIMasterDelegate DetectLightAndInteractEvent, DrawLightAndInteractionEvent, LightAndInteractWasJustDisabledEvent;
        private List<Object> ObjectsToDestroyOnVGSMCDisable = new List<Object>();

        private MainControls VGSMC;
        private void Start()
        {
            //FirstFrameTrigger = false;
            VGSMC = GetComponentInParent<MainControls>();
            LightAndInteractObjects = new Dictionary<GameObject, LightAndInteractObjectsDetection.VGSLIMainValues>();
            VGSMC.EventForLiIntHasChanged += UpdateDict;
            TimeBetweenRaycasts = 0.0f;
            MaxDetectionFPS = 60;
            RaycastTrigger1 = false;
            VGSMC.DestroyInstancedMaterials_Event += DestroyCreatedObjects;
            VGSMC.InstanceMaterials_Event += CreateMaterialsWhenNecessary;
            if (VGSMC.InstanceMaterialsOnStart) CreateMaterialsWhenNecessary();
        }

        private void CreateMaterialsWhenNecessary()
        {
            if (_LightDrawMaterial == null)
            {
                _LightDrawMaterial = new Material(_lightDrawShader);
                _LightDrawMaterial.SetVectorArray("_Coordinate", new Vector4[VGSMC.MaxLightPerGrassField]);
                _LightDrawMaterial.SetColorArray("_Color", new Color[VGSMC.MaxLightPerGrassField]);
                _LightDrawMaterial.SetFloatArray("_Size", new float[VGSMC.MaxLightPerGrassField]);
                _LightDrawMaterial.SetFloatArray("_Strength", new float[VGSMC.MaxLightPerGrassField]);
                _LightDrawMaterial.SetInt("_ArraysLength", VGSMC.MaxLightPerGrassField);
                ObjectsToDestroyOnVGSMCDisable.Add(_LightDrawMaterial);
            }
            if (_InteractDrawMaterial == null)
            {
                _InteractDrawMaterial = new Material(_interactDrawShader);
                _InteractDrawMaterial.SetVectorArray("_Coordinate", new Vector4[VGSMC.MaxLightPerGrassField]);
                _InteractDrawMaterial.SetFloatArray("_Size", new float[VGSMC.MaxLightPerGrassField]);
                _InteractDrawMaterial.SetFloatArray("_Strength", new float[VGSMC.MaxLightPerGrassField]);
                _InteractDrawMaterial.SetInt("_ArraysLength", VGSMC.MaxLightPerGrassField);

                ObjectsToDestroyOnVGSMCDisable.Add(_InteractDrawMaterial);
            }
        }

        private void DestroyCreatedObjects()
        {
            if (Application.isPlaying == true && ObjectsToDestroyOnVGSMCDisable.Count > 0)
            {
                for (int i = 0; i < ObjectsToDestroyOnVGSMCDisable.Count; i++)
                { if (ObjectsToDestroyOnVGSMCDisable[i] != null) Destroy(ObjectsToDestroyOnVGSMCDisable[i]); }
                ObjectsToDestroyOnVGSMCDisable.Clear();
            }
        }


        private void UpdateDict()
        {
            foreach (KeyValuePair<GameObject, DynamicLightAndInteract> item in VGSMC.IntLigDict)
            {
                if (LightAndInteractObjects.ContainsKey(item.Key) == false)
                {
                    LightAndInteractObjects.Add(item.Key, new LightAndInteractObjectsDetection.VGSLIMainValues());
                    LightAndInteractObjects[item.Key].PerGrassFieldValues = new Dictionary<GameObject, LightAndInteractObjectsDetection.VGSLIGrassFieldValues>();
                    for (int i = 0; i < VGSMC.GrassFieldList.Count; i++)
                    {
                        LightAndInteractObjects[item.Key].PerGrassFieldValues.Add(VGSMC.GrassFieldList[i], new LightAndInteractObjectsDetection.VGSLIGrassFieldValues());
                    }
                    LightAndInteractObjects[item.Key].VGSLightAndInteract = item.Value;
                }
            }
            List<GameObject> GOToRemove = new List<GameObject>();
            foreach (KeyValuePair<GameObject, LightAndInteractObjectsDetection.VGSLIMainValues> item in LightAndInteractObjects)
            {
                if (VGSMC.IntLigDict.ContainsKey(item.Key) == false) GOToRemove.Add(item.Key);
            }
            for (int i = 0; i < GOToRemove.Count; i++) { LightAndInteractObjects.Remove(GOToRemove[i]); }
        }

        private void Update()
        {
            GetGroundPositions();
        }

        private float TimeBetweenRaycasts;
        [HideInInspector] public int MaxDetectionFPS;
        private bool RaycastTrigger1;
        private void GetGroundPositions()
        {
            //int MaxFPS = Mathf.Max(VGSMC.LightMaxFPS, VGSMC.InteractionMaxFPS);
            //int MaxSVGLIFPS = 0;
            //foreach (KeyValuePair<GameObject, VibrantGrassShaderLightAndInteractObjectsDetection.VGSLIMainValues> item in LightAndInteractObjects)
            //{
            //    if (item.Value.VGSLightAndInteract.EnableLight == true
            //        && item.Value.VGSLightAndInteract.LightAndInteractMinFPS > MaxSVGLIFPS) MaxSVGLIFPS = item.Value.VGSLightAndInteract.LightAndInteractMinFPS;
            //    //if (item.Value.VGSLightAndInteract.EnableInteraction == true
            //    //    && item.Value.VGSLightAndInteract.InteractFPS > MaxSVGLIFPS) MaxSVGLIFPS = item.Value.VGSLightAndInteract.InteractFPS;
            //}
            //if (MaxSVGLIFPS != 0)
            if (LightAndInteractObjects.Count > 0)
            {
                //MaxDetectionFPS = Mathf.Min(MaxFPS, MaxSVGLIFPS);
                //MaxDetectionFPS = Mathf.Min(VGSMC.LightAndInteractMaxFPS, MaxSVGLIFPS);
                //float DelayBetweenRaycasts = 1.0f / (float)MaxDetectionFPS;
                float DelayBetweenRaycasts = 1.0f / (float)VGSMC.LightAndInteractMaxFPS;
                if (TimeBetweenRaycasts >= DelayBetweenRaycasts)
                {
                    foreach (KeyValuePair<GameObject, LightAndInteractObjectsDetection.VGSLIMainValues> item in LightAndInteractObjects)
                    {
                        LightAndInteractObjectsDetection.VGSLIMainValues VGSLIMainValues = item.Value;
                        DynamicLightAndInteract VGSLI = VGSLIMainValues.VGSLightAndInteract;
                        //VGSLIMainValues.LightDelay = 1.0f / (float)(Mathf.Min(VGSLI.LightFPS, VGSMC.LightMaxFPS));
                        //VGSLIMainValues.InteractDelay = 1.0f / (float)(Mathf.Min(VGSLI.InteractFPS, VGSMC.InteractionMaxFPS));
                        GameObject KeyObject = item.Key;
                        if (KeyObject != null && KeyObject.activeInHierarchy == true)
                        {
                            VGSLIMainValues.GOUsable = true;
                            VGSLIMainValues.GOPosition = KeyObject.transform.position;
                            RaycastHit raycastHit = new RaycastHit();
                            float MaxLightHeight = 0.0f;
                            float MaxInteractHeight = 0.0f;
                            bool EnableRaycast = false;
                            if (VGSLI.EnableLight == true)
                            {
                                MaxLightHeight = VGSLI.LightHeightInvLerpValuesAB.y;
                                EnableRaycast = true;
                            }
                            if (VGSLI.EnableInteraction == true)
                            {
                                MaxInteractHeight = VGSLI.InteractMaxHeight;
                                EnableRaycast = true;
                            }
                            if (EnableRaycast == true)
                            {
                                float MaxHeight = Mathf.Max(MaxLightHeight, MaxInteractHeight);
                                bool RayHit = Physics.Raycast(KeyObject.transform.position + Vector3.up * MaxHeight, Vector3.down, out raycastHit,
                                    MaxHeight * 2.0f, VGSMC.GroundLayers, QueryTriggerInteraction.Ignore);
                                if (RayHit == true)
                                {
                                    VGSLIMainValues.LastDetectedGroundPos = raycastHit.point;
                                    VGSLIMainValues.GOBelowMaximumHeight = true;
                                }
                                else VGSLIMainValues.GOBelowMaximumHeight = false;
                                VGSLIMainValues.LastDetectedHeight = KeyObject.transform.position.y - VGSLIMainValues.LastDetectedGroundPos.y;
                            }
                        }
                        else VGSLIMainValues.GOUsable = false;
                    }
                    DetectLightAndInteractEvent?.Invoke();
                    DrawLightAndInteractionEvent?.Invoke();
                    TimeBetweenRaycasts = 0.0f;
                    RaycastTrigger1 = true;
                }
            }
            else
            {
                if (RaycastTrigger1 == true)
                {
                    Debug.Log("Disable Light And Interact");
                    LightAndInteractWasJustDisabledEvent?.Invoke();
                    RaycastTrigger1 = false;
                }
                TimeBetweenRaycasts = 0.0f;
            }
            TimeBetweenRaycasts += Time.deltaTime;
        }
    }
}