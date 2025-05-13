using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VibrantGrassShaderTools;

namespace VibrantGrassShader
{
    [ExecuteInEditMode]
    public class DynamicLightAndInteract : MonoBehaviour
    {
        [Foldout("Light", true)]
        [SerializeField] public bool EnableLight = true;
        [SerializeField, ColorUsage(true, true)] public Color LightColor = new Color(1.059274f, 0.8207985f, 0.1608321f);
        [SerializeField] public float LightSize = 10.0f;
        [SerializeField] public Vector2 LightHeightInvLerpValuesAB = new Vector2(0.7f, 1.5f);

        [Foldout("Interaction", true)]
        [SerializeField] public bool EnableInteraction = true;
        [SerializeField] public float InteractObjectAddedHeight = 0.1f;
        [SerializeField]
        public Vector2 InteractSizeMinMax = new Vector2(0.0f, 0.02f), InteractStrengthMinMax = new Vector2(0.0f, 0.3f),
            InteractObjectSpeedInvLerpValuesAB = new Vector2(0.0f, 4.0f), InteractGrassHeightAboveObjectInvLerpValuesAB = new Vector2(0.0f, 0.2f);
#if UNITY_EDITOR
        [SerializeField] private bool DebugAddedHeight = false;
#endif
        [Foldout("Interaction Audio", true)]
        [SerializeField] private bool EnableAudio = true;
        [SerializeField] private AudioClip grassRustleSound = null;
        [SerializeField] private float InteractAudioMaxDistance = 200.0f;
        [SerializeField] public static Keyframe[] DefaultCurveKeyframes = new Keyframe[2] { new Keyframe(0.0f, 1.0f, 0.0f, -1.412f), new Keyframe(1.0f, 0.0f, 0.0f, 0.0f) };
        [SerializeField] private AnimationCurve InteractAudioDistanceCurve = new AnimationCurve(DefaultCurveKeyframes);//EaseInOut(0.0f, 1.0f, 1.0f, 0.0f)
        [SerializeField] private Vector2 InteractAudioVolumeMinMax = new Vector2(0.01f, 0.1f);
        [SerializeField] private Vector2 InteractAudioObjectSpeedInvLerpValuesAB = new Vector2(1.0f, 4.0f);
        [SerializeField] public Vector2 InteractAudioGrassHeightInvLerpValuesAB = new Vector2(0.0f, 0.7f);
        [SerializeField] private Vector2 InteractAudioSmoothTimeUpAndDown = new Vector2(0.2f, 0.1f);
        [HideInInspector] public Vector3 ObjectVelocity;
        [HideInInspector] public float InteractSizeApplied = 0.0f, InteractStrengthApplied = 0.0f, InteractAudioSpeedValueApplied = 0.0f;
        [Foldout("Performance", true)]
        [SerializeField] public float LightDrawDistanceFromSource = 50.0f, InteractDrawDistanceFromSource = 50.0f, InteractMaxHeight = 15.0f;
        //[SerializeField] public int LightAndInteractMinFPS = 50;
        //[SerializeField] public int LightFPS = 60, InteractFPS = 60;
#if UNITY_EDITOR
        [SerializeField] private bool DebugLightMaxDrawDistance = false, DebugInteractMaxDrawDistance = false;
        public AudioClip GrassRustleSound
        {
            get { return grassRustleSound; }
            set
            {
                DynamicLightAndInteract DyLightAndInt = GetComponent<DynamicLightAndInteract>();
                SerializedObject serializedObject = new SerializedObject(DyLightAndInt);
                grassRustleSound = null;
                grassRustleSound = value;//Reset so it detects the change
                SerializedProperty serializedProperty = serializedObject.FindProperty("grassRustleSound");
                serializedProperty.objectReferenceValue = value;
                serializedObject.ApplyModifiedProperties();
            }
        }

#endif
        //[Foldout("Usable Data", true)]
        [SerializeField, HideInInspector] public float InteractAudioValue0To1 = 0.0f;
        private float InteractHeightMultiplier;
        [HideInInspector] public Dictionary<GameObject, float> InteractAudioHeightMultiplierDict = new Dictionary<GameObject, float>();
        private AudioSource audioSourceGrassRustle;

        private void Start()
        {
            if (Application.isPlaying == true)
            {
                if (EnableAudio == true) AddAudioSourceComponent();
                InteractAudioHeightMultiplierDict = new Dictionary<GameObject, float>();
            }
        }

        private void AddAudioSourceComponent()
        {
            audioSourceGrassRustle = gameObject.AddComponent<AudioSource>();
            audioSourceGrassRustle.clip = grassRustleSound;
            audioSourceGrassRustle.volume = 0.0f;
            audioSourceGrassRustle.loop = true;
            audioSourceGrassRustle.playOnAwake = true;
            audioSourceGrassRustle.spatialBlend = 1.0f;
            audioSourceGrassRustle.PlayDelayed(Random.Range(0.0f, 25.0f));
            audioSourceGrassRustle.dopplerLevel = 0.0f;
            audioSourceGrassRustle.rolloffMode = AudioRolloffMode.Custom;
            audioSourceGrassRustle.maxDistance = InteractAudioMaxDistance;
            audioSourceGrassRustle.SetCustomCurve(AudioSourceCurveType.CustomRolloff, InteractAudioDistanceCurve);
            audioSourceGrassRustle.enabled = false;
            audioSourceGrassRustle.enabled = true;//Disable and enable to avoid not working on start 
        }
#if UNITY_EDITOR
        private void OnEnable()
        {
            if (Application.isPlaying == false)
            {
                EditorApplication.update += EditorUpdates;
                if (grassRustleSound == null) FindAndAssignDefaultAudioClip();
            }
        }
        private void OnDisable()
        {
            if (Application.isPlaying == false) EditorApplication.update -= EditorUpdates;
        }

        private void EditorUpdates()
        {
            if (Application.isPlaying == false)
            {
                VariablesLogic();
                if (DebugAddedHeight == true)
                {
                    Debug.DrawLine(transform.position, transform.position + Vector3.up * InteractObjectAddedHeight, Color.green, 0.01f, false);
                }
            }
        }

        private void FindAndAssignDefaultAudioClip()
        {
            string[] AssetsFound = AssetDatabase.FindAssets("GoingThroughGrass");
            if (AssetsFound.Length > 0)
            {
                string FirstPath = AssetDatabase.GUIDToAssetPath(AssetsFound[0]);
                AudioClip ClipFound = AssetDatabase.LoadAssetAtPath<AudioClip>(FirstPath);
                GrassRustleSound = ClipFound;
            }
        }

#endif

        private void Update()
        {
            if (Application.isPlaying == true)
            {
#if UNITY_EDITOR
                VariablesLogic();
                if (DebugAddedHeight == true)
                {
                    Debug.DrawRay(transform.position, Vector3.up * InteractObjectAddedHeight, Color.green, 0.0f, false);
                }
                if (DebugLightMaxDrawDistance == true)
                {
                    Debug.DrawRay(transform.position, Vector3.right * LightDrawDistanceFromSource, Color.green, 0.0f, false);
                }
                if (DebugInteractMaxDrawDistance == true)
                {
                    Debug.DrawRay(transform.position, Vector3.right * InteractDrawDistanceFromSource, Color.blue, 0.0f, false);
                }
#endif
                ObjectVelocity = CalculateHorizVelocityForDict(gameObject);
                GetInteractSizeAndStrengthApplied();
                InteractionValueMethod();
                AudioRustleMethod();
            }
        }

        private void VariablesLogic()
        {
            InteractAudioVolumeMinMax.y = Mathf.Clamp(InteractAudioVolumeMinMax.y, InteractAudioVolumeMinMax.x, 1.0f);
            InteractAudioVolumeMinMax.x = Mathf.Clamp(InteractAudioVolumeMinMax.x, 0.0f, InteractAudioVolumeMinMax.y);
        }

        private float InteractionValueSmoothed, InteractionValueSmoothRef;
        private void AudioRustleMethod()
        {
            if (EnableAudio == true && audioSourceGrassRustle == null) AddAudioSourceComponent();
            if (EnableAudio == true && audioSourceGrassRustle != null)
            {
                float InteractionVolumeTarget = Mathf.Lerp(InteractAudioVolumeMinMax.x, InteractAudioVolumeMinMax.y, InteractAudioValue0To1);
                if (InteractAudioValue0To1 <= 0.0f) InteractionVolumeTarget = 0.0f;
                float SmoothTimeApplied = InteractAudioSmoothTimeUpAndDown.y;
                if (InteractionValueSmoothed < InteractionVolumeTarget) SmoothTimeApplied = InteractAudioSmoothTimeUpAndDown.x;
                InteractionValueSmoothed = Mathf.SmoothDamp(InteractionValueSmoothed, InteractionVolumeTarget, ref InteractionValueSmoothRef, SmoothTimeApplied);
                audioSourceGrassRustle.volume = InteractionValueSmoothed;
            }
            if (EnableAudio == false && audioSourceGrassRustle != null)
            {
                Destroy(audioSourceGrassRustle);
                InteractionValueSmoothed = 0.0f;
            }
        }

        private void InteractionValueMethod()
        {
            float HighestInteract = 0.0f;
            foreach (KeyValuePair<GameObject, float> item in InteractAudioHeightMultiplierDict)
            {
                float InteractHeightMultiplierFound = item.Value;
                if (InteractHeightMultiplierFound > HighestInteract) HighestInteract = InteractHeightMultiplierFound;
            }
            InteractHeightMultiplier = HighestInteract;
            InteractAudioValue0To1 = InteractHeightMultiplier * InteractAudioSpeedValueApplied;
        }

        private void GetInteractSizeAndStrengthApplied()
        {
            float SpeedLerpValueClamped = Mathf.Clamp01(Mathf.InverseLerp(InteractObjectSpeedInvLerpValuesAB.x, InteractObjectSpeedInvLerpValuesAB.y, ObjectVelocity.magnitude));
            InteractSizeApplied = Mathf.Lerp(InteractSizeMinMax.x, InteractSizeMinMax.y, SpeedLerpValueClamped);
            InteractStrengthApplied = Mathf.Lerp(InteractStrengthMinMax.x, InteractStrengthMinMax.y, SpeedLerpValueClamped);//Get Brush Size and Strength depending on ObjectVelocity, with Minimum being FloofMinBrushSizeAndStrengthList[i]

            //Audio
            InteractAudioSpeedValueApplied = Mathf.Clamp01(Mathf.InverseLerp(InteractAudioObjectSpeedInvLerpValuesAB.x, InteractAudioObjectSpeedInvLerpValuesAB.y, ObjectVelocity.magnitude));
        }

        private Vector3 OldPos;
        private Vector3 CalculateHorizVelocityForDict(GameObject Key)
        {
            Vector3 velocity;
            Vector3 currentframePosition = new Vector3(Key.transform.position.x, 0.0f, Key.transform.position.z);
            velocity = (currentframePosition - OldPos) / Time.deltaTime;

            OldPos = currentframePosition;
            return velocity;
        }

    }
}