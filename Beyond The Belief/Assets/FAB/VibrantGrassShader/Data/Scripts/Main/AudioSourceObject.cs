using UnityEngine;
using UnityEditor;

namespace VibrantGrassShader
{
#if UNITY_EDITOR
    [ExecuteInEditMode]
    public class AudioSourceObject : MonoBehaviour
    {
        [SerializeField] private bool SpawnNewAudioSource = false, DeleteThisAudioSource = false, StickToGround = false;
        [SerializeField] public Vector2 DistanceInterpolationMinMax = new Vector2(5.0f, 10.0f);
        [HideInInspector] public GrassWindAudioMaster SVGMCWindAudioMaster;
        [HideInInspector] public ShiniesVibrantGrassWindSourcePositionsValues SVGWindSourcePosValues;
        [HideInInspector] public MainControls VGSMC;
        private GameObject InsideSphere;

        private void OnEnable()
        {
            if (this != null && gameObject != null)
            {
                InsideSphere = transform.GetChild(0).gameObject;
                EditorApplication.update += EditorUpdate;
            }
        }

        private void OnDisable()
        {
            if (this != null && gameObject != null)
            {
                EditorApplication.update -= EditorUpdate;
            }
        }

        void EditorUpdate()
        {
            if (this != null && gameObject != null)
            {
                if (SVGWindSourcePosValues != null)
                {
                    DistanceInterpolationMinMax.y = Mathf.Clamp(DistanceInterpolationMinMax.y, DistanceInterpolationMinMax.x, Mathf.Infinity);
                    DistanceInterpolationMinMax.x = Mathf.Clamp(DistanceInterpolationMinMax.x, 0.0f, DistanceInterpolationMinMax.y);
                    transform.localScale = new Vector3(DistanceInterpolationMinMax.y * 2.0f, DistanceInterpolationMinMax.y * 2.0f, DistanceInterpolationMinMax.y * 2.0f);
                    SVGWindSourcePosValues.DistanceInterpolationMinMax = DistanceInterpolationMinMax;
                    SVGWindSourcePosValues.Position = transform.position;
                    if (SpawnNewAudioSource == true)
                    {
                        int IndexToAdd = 0;
                        for (int i = 0; i < SVGMCWindAudioMaster.SourcePosValues.Count; i++)
                        {
                            if (SVGMCWindAudioMaster.SourcePosValues[i].Index > IndexToAdd) IndexToAdd = SVGMCWindAudioMaster.SourcePosValues[i].Index;
                        }
                        IndexToAdd += 1;
                        GameObject GOSpawned = SVGMCWindAudioMaster.SpawnAudioSourceObject(IndexToAdd, transform.position, DistanceInterpolationMinMax, null);
                        Selection.activeGameObject = GOSpawned;
                        SpawnNewAudioSource = false;
                    }
                }
                if (InsideSphere != null)
                {
                    Vector3 InsideSphereScale = new Vector3(DistanceInterpolationMinMax.x * 2.0f / transform.localScale.x,
                        DistanceInterpolationMinMax.x * 2.0f / transform.localScale.y, DistanceInterpolationMinMax.x * 2.0f / transform.localScale.z);
                    InsideSphere.transform.localScale = InsideSphereScale;
                    InsideSphere.transform.position = transform.position;
                }
                if (StickToGround == true)
                {
                    float StickDistance = 1000.0f;
                    RaycastHit rayHit = new RaycastHit();
                    bool RayHit = Physics.Raycast(transform.position + Vector3.up * (StickDistance / 2.0f), Vector3.down, out rayHit, StickDistance, VGSMC.GroundLayers, QueryTriggerInteraction.Ignore);
                    if (RayHit == true) transform.position = new Vector3(transform.position.x, rayHit.point.y, transform.position.z);
                }
                if (DeleteThisAudioSource == true) DestroyImmediate(gameObject);
            }
        }
    }
#endif
}