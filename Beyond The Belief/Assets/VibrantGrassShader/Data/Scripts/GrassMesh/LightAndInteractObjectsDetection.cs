using System.Collections.Generic;
using UnityEngine;
using VibrantGrassShaderTools;

namespace VibrantGrassShader
{
    public class LightAndInteractObjectsDetection : MonoBehaviour
    {
        [Foldout("Data (Don't Touch)", true)]
        [SerializeField] private GameObject GrassLandShaderMasterObject = null;
        private GrassFieldMaster GSM;
        private LightAndInteractionMaster VGSInteractMaster;
        private GrassFieldCreator VGSFieldCreator;
        private float GrassFieldMaxDistance, GrassFieldHalfDistance;

        public delegate void VGSLIDetectionDelegate();
        public VGSLIDetectionDelegate DisableAllLightFirstFrameEvent;


        private void Start()
        {
            GSM = GrassLandShaderMasterObject.GetComponent<GrassFieldMaster>();
            VGSInteractMaster = GSM.VGSMC.gameObject.GetComponent<LightAndInteractionMaster>();
            VGSFieldCreator = GrassLandShaderMasterObject.GetComponent<GrassFieldCreator>();
            GrassFieldMaxDistance = GSM.VGSMC.GrassFieldMaxDistance;
            GrassFieldHalfDistance = GrassFieldMaxDistance / 2.0f;
        }

        private void OnEnable()
        {
            if (GSM == null) GSM = GrassLandShaderMasterObject.GetComponent<GrassFieldMaster>();
            if (VGSInteractMaster == null) VGSInteractMaster = GSM.VGSMC.gameObject.GetComponent<LightAndInteractionMaster>();
            VGSInteractMaster.DetectLightAndInteractEvent += LightAndInteractObjects;
            LightObjectActiveAndEnabledOld = false;
        }

        private void OnDisable()
        {
            if (GSM == null) GrassLandShaderMasterObject.GetComponent<GrassFieldMaster>();
            if (VGSInteractMaster == null) VGSInteractMaster = GSM.VGSMC.gameObject.GetComponent<LightAndInteractionMaster>();
            VGSInteractMaster.DetectLightAndInteractEvent -= LightAndInteractObjects;
        }

        //[HideInInspector] public int MaxInFieldInteractFPS;
        [HideInInspector] public bool DetectedLightObject, NeedToApplyLight, DetectedInteractObject, LightObjectActiveAndEnabled, LightObjectActiveAndEnabledOld;
        private void LightAndInteractObjects()
        {
            DetectedLightObject = false;
            DetectedInteractObject = false;
            float currentTime = Time.time;
            LightObjectActiveAndEnabled = false;
            MainControls VGSCM = GSM.VGSMC;
            foreach (KeyValuePair<GameObject, VGSLIMainValues> item in VGSInteractMaster.LightAndInteractObjects)
            {
                VGSLIMainValues VGSLigIntMainValues = item.Value;
                if (VGSLigIntMainValues.GOUsable == true)
                {
                    DynamicLightAndInteract VGSLI = VGSLigIntMainValues.VGSLightAndInteract;
                    VGSLIGrassFieldValues PerGrassFieldValues = VGSLigIntMainValues.PerGrassFieldValues[GrassLandShaderMasterObject];
                    bool ApplyLight = true;
                    bool ApplyInteract = true;
                    //bool ApplyLight = false;
                    //bool ApplyInteract = false;
                    //if (currentTime >= PerGrassFieldValues.NextLightTimeStep) ApplyLight = true;
                    //if (currentTime >= PerGrassFieldValues.NextInteractTimeStep) ApplyInteract = true;
                    //Might try to make the FPS limiter work later.
                    //Check GOExists first 
                    bool EnableLight = VGSLI.EnableLight;
                    if (ApplyLight == true || ApplyInteract == true)
                    {
                        bool EnableInteract = VGSLI.EnableInteraction;
                        if (EnableLight == true || EnableInteract == true)
                        {
                            GameObject KeyGO = item.Key;
                            Vector3 GOLocalPos = Vector3.zero;
                            bool LightEnabled = false;
                            float GrassFieldHalfDistancePlusSizeLight = 0.0f;
                            if (ApplyLight == true)
                            {
                                //PerGrassFieldValues.NextLightTimeStep = currentTime + VGSLigIntMainValues.LightDelay;
                                if (EnableLight == true)
                                {
                                    LightEnabled = true;
                                    GOLocalPos = transform.InverseTransformPoint(VGSLigIntMainValues.GOPosition);
                                    GrassFieldHalfDistancePlusSizeLight = GrassFieldHalfDistance + VGSLI.LightDrawDistanceFromSource;
                                    if (GOLocalPos.x <= GrassFieldHalfDistancePlusSizeLight && GOLocalPos.x >= -GrassFieldHalfDistancePlusSizeLight
                                        && GOLocalPos.z <= GrassFieldHalfDistancePlusSizeLight && GOLocalPos.z >= -GrassFieldHalfDistancePlusSizeLight)
                                    {
                                        if (Mathf.Abs(VGSLigIntMainValues.LastDetectedHeight) > VGSLI.LightHeightInvLerpValuesAB.y) PerGrassFieldValues.LightHeightHit = false;
                                        else
                                        {
                                            PerGrassFieldValues.LightHeightHit = true;
                                            DetectedLightObject = true;
                                        }
                                        //Inverse Lerp done manually to be unclamped 
                                        float XTexCoord = (GOLocalPos.x - GrassFieldMaxDistance / 2.0f) / (-GrassFieldMaxDistance / 2.0f - GrassFieldMaxDistance / 2.0f);
                                        float YTexCoord = (GOLocalPos.z - GrassFieldMaxDistance / 2.0f) / (-GrassFieldMaxDistance / 2.0f - GrassFieldMaxDistance / 2.0f);
                                        PerGrassFieldValues.TexCoord = new Vector2(XTexCoord, YTexCoord);
                                    }
                                    else PerGrassFieldValues.LightHeightHit = false;
                                }
                            }
                            if (ApplyInteract == true)
                            {
                                //PerGrassFieldValues.NextInteractTimeStep = currentTime + VGSLigIntMainValues.InteractDelay;
                                if (EnableInteract == true)
                                {
                                    float GrassFieldHalfDistancePlusSizeInteract = GrassFieldHalfDistance + VGSLI.InteractDrawDistanceFromSource;
                                    if (LightEnabled == false) GOLocalPos = transform.InverseTransformPoint(VGSLigIntMainValues.GOPosition);
                                    bool CheckPosition = true;
                                    if (GrassFieldHalfDistancePlusSizeInteract < GrassFieldHalfDistancePlusSizeLight && DetectedLightObject == true)
                                    {
                                        CheckPosition = false;
                                        DetectedInteractObject = true;
                                    }
                                    if (CheckPosition == true)
                                    {
                                        if (GOLocalPos.x <= GrassFieldHalfDistancePlusSizeInteract && GOLocalPos.x >= -GrassFieldHalfDistancePlusSizeInteract
                                            && GOLocalPos.z <= GrassFieldHalfDistancePlusSizeInteract && GOLocalPos.z >= -GrassFieldHalfDistancePlusSizeInteract)
                                        {
                                            if (Mathf.Abs(VGSLigIntMainValues.LastDetectedHeight) > VGSLI.InteractMaxHeight) PerGrassFieldValues.InteractHeightHit = false;
                                            else PerGrassFieldValues.InteractHeightHit = true;
                                            if (LightEnabled == false)
                                            {
                                                //Inverse Lerp done manually to be unclamped 
                                                float XTexCoord = (GOLocalPos.x - GrassFieldMaxDistance / 2.0f) / (-GrassFieldMaxDistance / 2.0f - GrassFieldMaxDistance / 2.0f);
                                                float YTexCoord = (GOLocalPos.z - GrassFieldMaxDistance / 2.0f) / (-GrassFieldMaxDistance / 2.0f - GrassFieldMaxDistance / 2.0f);
                                                PerGrassFieldValues.TexCoord = new Vector2(XTexCoord, YTexCoord);
                                            }
                                            DetectedInteractObject = true;
                                        }
                                        else PerGrassFieldValues.InteractHeightHit = false;
                                    }
                                    else PerGrassFieldValues.InteractHeightHit = false;
                                }
                            }
                        }
                    }
                    if (EnableLight == true && PerGrassFieldValues.LightHeightHit == true) LightObjectActiveAndEnabled = true;
                }
            }
            if (LightObjectActiveAndEnabled == false && LightObjectActiveAndEnabledOld == true)
            {
                DisableAllLightFirstFrameEvent?.Invoke();
            }
            LightObjectActiveAndEnabledOld = LightObjectActiveAndEnabled;
            //if (DetectedInteractObject == true) MaxInFieldInteractFPS = Mathf.Min(GSM.VGSMC.InteractionMaxFPS, HighestInteractFPSFound);
        }


        public class VGSLIMainValues
        {
            public float LightDelay = 0.0f, InteractDelay = 0.0f;
            public bool GOUsable, GOBelowMaximumHeight;
            public DynamicLightAndInteract VGSLightAndInteract;
            public Vector3 GOPosition, LastDetectedGroundPos;
            public float LastDetectedHeight;
            public Dictionary<GameObject, VGSLIGrassFieldValues> PerGrassFieldValues = new Dictionary<GameObject, VGSLIGrassFieldValues>();
        }
        public class VGSLIGrassFieldValues
        {
            public bool LightHeightHit = false;
            public Vector2 TexCoord = Vector2.zero;
            public bool InteractHeightHit = false;
            public float InteractStrengthSmoothRef = 0.0f, InteractStrengthResult = 0.0f, NextLightTimeStep = 0.0f, NextInteractTimeStep = 0.0f;
        }
    }
}