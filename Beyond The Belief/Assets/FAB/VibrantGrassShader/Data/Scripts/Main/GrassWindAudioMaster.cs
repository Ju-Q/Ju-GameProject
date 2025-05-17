using System.Collections.Generic;
using UnityEngine;
using VibrantGrassShaderTools;
using UnityEditor;

namespace VibrantGrassShader
{
    [ExecuteInEditMode]
    public class GrassWindAudioMaster : MonoBehaviour
    {
        private MainControls SVGMC;
        [SerializeField, HideInInspector] public List<ShiniesVibrantGrassWindSourcePositionsValues> SourcePosValues = new List<ShiniesVibrantGrassWindSourcePositionsValues>();
#if UNITY_EDITOR
        [SerializeField, HideInInspector] private GameObject AudioSourcesParent = null;
        [Foldout("Data (Don't Touch)", true)]
        [SerializeField] private Object AudioSourceObjectPrefab = null;
#endif
        private AudioSource audioSourceWind;

#if UNITY_EDITOR
        void OnEnable()
        {
            SVGMC = GetComponent<MainControls>();
            if (Application.isPlaying == false)
            {
                EditorApplication.update += EditorUpdates;
            }
        }
        void OnDisable()
        {
            if (Application.isPlaying == false)
            {
                EditorApplication.update -= EditorUpdates;
                if (SVGMC.ShowAudioSourcesUIEnabled == true) SceneView.duringSceneGui -= OnSceneGUI;
            }
        }
#endif

        private void Start()
        {
            if (SVGMC == null) SVGMC = GetComponent<MainControls>();
            if (Application.isPlaying == true)
            {
                if (SVGMC.EnableWindAudio == true)
                {
                    AddAudioSourceForWind();
                }
            }
        }

        private void AddAudioSourceForWind()
        {
            audioSourceWind = gameObject.AddComponent<AudioSource>();
            audioSourceWind.clip = SVGMC.WindAudioClip;
            audioSourceWind.volume = 0.0f;
            audioSourceWind.loop = true;
            audioSourceWind.playOnAwake = true;
            audioSourceWind.spatialBlend = 0.0f;
            audioSourceWind.PlayDelayed(Random.Range(0.0f, 25.0f));
            audioSourceWind.dopplerLevel = 0.0f;
            audioSourceWind.enabled = false;
            audioSourceWind.enabled = true;//Disable and enable to avoid not working on start   
        }

        void EditorUpdates()
        {
            if (Application.isPlaying == false && this != null)
            {
#if UNITY_EDITOR
                if (SVGMC.ShowAudioSources == true)
                {
                    SpawnAudioSourcesWhenNecessary();
                    EditorUtility.SetDirty(this);
                    if (SVGMC.ShowAudioSourcesUIEnabled == false)
                    {
                        SceneView.duringSceneGui += OnSceneGUI;
                        SVGMC.ShowAudioSourcesUIEnabled = true;
                    }
                }
                else
                {
                    DestroyAudioSourcesWhenNecessary();
                    if (SVGMC.ShowAudioSourcesUIEnabled == true)
                    {
                        SceneView.duringSceneGui -= OnSceneGUI;
                        SVGMC.ShowAudioSourcesUIEnabled = false;
                    }
                }
#endif
            }
        }

        void Update()
        {
            if (Application.isPlaying == true)
            {
                AudioMethod();
            }
        }
#if UNITY_EDITOR
        private void SpawnAudioSourcesWhenNecessary()
        {
            if (AudioSourcesParent == null)
            {
                AudioSourcesParent = new GameObject("AudioSources");
                //AudioSourcesParent.transform.SetParent(transform);
                AudioSourcesParent.transform.SetAsLastSibling();
                AudioSourcesParent.transform.position = transform.position;
                AudioSourcesParent.transform.eulerAngles = transform.eulerAngles;
                for (int i = 0; i < SourcePosValues.Count; i++)
                {
                    SpawnAudioSourceObject(SourcePosValues[i].Index, SourcePosValues[i].Position, SourcePosValues[i].DistanceInterpolationMinMax, SourcePosValues[i]);
                }
                if (SourcePosValues.Count <= 0)
                {
                    SpawnAudioSourceObject(0, transform.position, new Vector2(10.0f, 30.0f), null);
                }
                if (SourcePosValues.Count > 0) Selection.activeGameObject = SourcePosValues[0].GOSpawned;
            }
            List<ShiniesVibrantGrassWindSourcePositionsValues> SourcesToRemove = new List<ShiniesVibrantGrassWindSourcePositionsValues>();
            for (int i = 0; i < SourcePosValues.Count; i++) { if (SourcePosValues[i].GOSpawned == null) SourcesToRemove.Add(SourcePosValues[i]); }
            for (int i = 0; i < SourcesToRemove.Count; i++) { SourcePosValues.Remove(SourcesToRemove[i]); }
            for (int i = 0; i < AudioSourcesParent.transform.childCount; i++)
            {
                GameObject childFound = AudioSourcesParent.transform.GetChild(i).gameObject;
                AudioSourceObject SVGAudioSourceObject = childFound.GetComponent<AudioSourceObject>();
                if (SVGAudioSourceObject != null)
                {
                    if (SourcePosValues.Contains(SVGAudioSourceObject.SVGWindSourcePosValues) == false || SVGAudioSourceObject.SVGWindSourcePosValues == null)
                    {
                        SVGAudioSourceObject.SVGWindSourcePosValues = new ShiniesVibrantGrassWindSourcePositionsValues();
                        SourcePosValues.Add(SVGAudioSourceObject.SVGWindSourcePosValues);
                    }
                }
            }
        }

        public GameObject SpawnAudioSourceObject(int Index, Vector3 Position, Vector2 DistanceInterpolationMinMax, ShiniesVibrantGrassWindSourcePositionsValues SVGWindSourcePosValues)
        {
            string PrefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(AudioSourceObjectPrefab);
            GameObject GOSpawned = PrefabUtility.InstantiatePrefab(AudioSourceObjectPrefab, transform) as GameObject;
            PrefabInstanceStatus prefabInstanceStatus = PrefabUtility.GetPrefabInstanceStatus(GOSpawned);
            if (prefabInstanceStatus == PrefabInstanceStatus.Connected) PrefabUtility.UnpackPrefabInstance(GOSpawned, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
            GOSpawned.name = gameObject.name + "AudioSource_" + Index;
            GOSpawned.transform.position = Position;
            GOSpawned.transform.SetParent(AudioSourcesParent.transform);
            AudioSourceObjectPrefab = AssetDatabase.LoadAssetAtPath(PrefabPath, typeof(Object));
            AudioSourceObject SVGAudioSourceObject = GOSpawned.GetComponent<AudioSourceObject>();
            SVGAudioSourceObject.SVGMCWindAudioMaster = this;
            ShiniesVibrantGrassWindSourcePositionsValues SVGWindSourcePosValuesApplied = SVGWindSourcePosValues;
            if (SVGWindSourcePosValues == null)
            {
                SVGWindSourcePosValuesApplied = new ShiniesVibrantGrassWindSourcePositionsValues();
                SourcePosValues.Add(SVGWindSourcePosValuesApplied);
            }
            SVGAudioSourceObject.VGSMC = SVGMC;
            SVGAudioSourceObject.SVGWindSourcePosValues = SVGWindSourcePosValuesApplied;
            SVGAudioSourceObject.DistanceInterpolationMinMax = DistanceInterpolationMinMax;
            SVGWindSourcePosValuesApplied.GOSpawned = GOSpawned;
            SVGWindSourcePosValuesApplied.Index = Index;
            return GOSpawned;
        }

        private void DestroyAudioSourcesWhenNecessary()
        {
            if (AudioSourcesParent != null) DestroyImmediate(AudioSourcesParent);
        }
#endif

        private float VolumeSmoothed, VolumeSmoothRef;
        private void AudioMethod()
        {
            if (SVGMC.EnableWindAudio == true && SVGMC._camera != null)
            {
                if (audioSourceWind == null) AddAudioSourceForWind();
                float BiggestDistInvLerpValue = 0.0f;
                Vector3 AverageDirection = Vector3.zero;
                for (int i = 0; i < SourcePosValues.Count; i++)
                {
                    float distance = Vector3.Distance(SourcePosValues[i].Position, SVGMC._camera.transform.position);
                    float CurrentValue = 1 - Mathf.Clamp01(Mathf.InverseLerp(SourcePosValues[i].DistanceInterpolationMinMax.x, SourcePosValues[i].DistanceInterpolationMinMax.y, distance));
                    if (CurrentValue > BiggestDistInvLerpValue) BiggestDistInvLerpValue = CurrentValue;
                    Vector3 direction = (SourcePosValues[i].Position - SVGMC._camera.transform.position).normalized;
                    Vector3 directionMultiplied = direction * CurrentValue;
                    AverageDirection += directionMultiplied;
                }
                float VolumeTarget = SVGMC.WindVolumeDistanceCurve.Evaluate(BiggestDistInvLerpValue);
                VolumeSmoothed = Mathf.SmoothDamp(VolumeSmoothed, VolumeTarget, ref VolumeSmoothRef, SVGMC.WindVolumeSmoothTime);
                audioSourceWind.volume = VolumeSmoothed * SVGMC.WindMaxVolume;
                AverageDirection = AverageDirection.normalized;
                float DotForPan = Vector3.Dot(AverageDirection, SVGMC._camera.transform.right);
                audioSourceWind.panStereo = Mathf.Lerp(DotForPan, 0.0f, Mathf.Clamp01(BiggestDistInvLerpValue));
                SVGMC.WindAudioVolumeApplied = VolumeSmoothed;
                SVGMC.WindAudioDirectionApplied = AverageDirection;
            }
            if (SVGMC.EnableWindAudio == false)
            {
                Destroy(audioSourceWind);
                VolumeSmoothed = 0.0f;
            }
        }

#if UNITY_EDITOR
        private Vector2 UIRectangleAddedPosition = new Vector2(2.5f, 190.0f), UIRectangleSize = new Vector2(215.0f, 26.0f);
        private int FontSize = 13;
        private void OnSceneGUI(SceneView scene)
        {
            Handles.BeginGUI();
            Event _event = Event.current;
            Rect MainUIRect1 = new Rect(new Vector2(Screen.width - UIRectangleAddedPosition.x - UIRectangleSize.x, Screen.height - UIRectangleAddedPosition.y - UIRectangleSize.y), UIRectangleSize);
            Color GUIOldColor = GUI.color;
            GUIStyle GONameBoxStyle = new GUIStyle(GUI.skin.box);
            GUI.color = Color.white;
            GUI.enabled = true;
            GUILayout.BeginArea(MainUIRect1);
            GUI.backgroundColor = Color.white;
            GUIStyle ButtonStyle1 = new GUIStyle(GUI.skin.button);
            ButtonStyle1.fixedWidth  = UIRectangleSize.x;
            ButtonStyle1.fixedHeight = UIRectangleSize.y;
            ButtonStyle1.fontSize = FontSize;
            bool DisableUIButton = GUILayout.Button("Disable Audio Sources Display", ButtonStyle1);
            if (DisableUIButton == true) SVGMC.ShowAudioSources = false;
            GUILayout.EndArea();
            Handles.EndGUI();
        }
#endif
    }

    [System.Serializable]
    public class ShiniesVibrantGrassWindSourcePositionsValues
    {
        public int Index;
        public Vector3 Position;
        public GameObject GOSpawned;
        public Vector2 DistanceInterpolationMinMax;
    }

}