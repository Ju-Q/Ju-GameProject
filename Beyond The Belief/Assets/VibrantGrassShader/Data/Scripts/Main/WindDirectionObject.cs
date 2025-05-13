using UnityEngine;
using UnityEditor;

namespace VibrantGrassShader
{
    [ExecuteInEditMode]
    public class WindDirectionObject : MonoBehaviour
    {
#if UNITY_EDITOR
        [HideInInspector, SerializeField] public MainControls SVGMC;

        private void OnEnable()
        {
            if (Application.isPlaying == false)
            {
                EditorApplication.update += EditorUpdates;
                OldXLocalEuler = transform.localEulerAngles.x;
                OldZLocalEuler = transform.localEulerAngles.z;
                OldGlobalEuler = transform.eulerAngles;
                OldLocalEuler = transform.localEulerAngles;
            }
        }
        private void OnDisable()
        {
            if (Application.isPlaying == false) EditorApplication.update -= EditorUpdates;
        }

        void EditorUpdates()
        {
            if (Application.isPlaying == false && this != null)
            {
                SVGMC.WindDirectionDegrees = transform.eulerAngles.y;
                if (SVGMC.CreatingGrass == true || SVGMC.EnableUI == false
                    || SVGMC.PaintShadows || SVGMC.PaintHeight || SVGMC.PaintColor) DestroyImmediate(gameObject);
                if (this != null)//Idk why on first frame it said that this class was trying to be accessed even tho it was destroyed so xD... 
                {
                    LockXZRotation();
                    GlobalizeRotation();
                }
            }
        }

        private Vector3 OldGlobalEuler, OldLocalEuler;
        private void GlobalizeRotation()
        {
            if (transform.localEulerAngles == OldLocalEuler && transform.eulerAngles != OldGlobalEuler) transform.eulerAngles = OldGlobalEuler;
            OldGlobalEuler = transform.eulerAngles;
            OldLocalEuler = transform.localEulerAngles;
        }

        float OldXLocalEuler, OldZLocalEuler;
        private void LockXZRotation()
        {
            if (transform.localEulerAngles.x != OldXLocalEuler) transform.localEulerAngles = new Vector3(0.0f, transform.localEulerAngles.y, 0.0f);
            if (transform.localEulerAngles.z != OldZLocalEuler) transform.localEulerAngles = new Vector3(0.0f, transform.localEulerAngles.y, 0.0f);
            if (transform.localEulerAngles.x != OldXLocalEuler || transform.localEulerAngles.z != OldZLocalEuler)
            {
                Debug.Log("You can change only the Y Rotation of the Wind Direction");
            }
            OldXLocalEuler = transform.localEulerAngles.x;
            OldZLocalEuler = transform.localEulerAngles.z;
        }
#endif
        private void Start()
        {
            if (Application.isPlaying == true)
            {
                DestroyImmediate(gameObject);
            }
        }
    }

}