using UnityEngine;
using UnityEditor;
using VibrantGrassShaderTools;

namespace VibrantGrassShader
{
    [ExecuteInEditMode]
    public class ControlArrows : MonoBehaviour
    {
#if UNITY_EDITOR
        [HideInInspector] public Vector2 ArrowDirection;
        [Foldout("Data (Don't touch)", true)]
        [SerializeField] public Color OriginalColor = Color.blue, MouseHoverColor = Color.green, HoverClickColor = Color.white;
        [SerializeField] public GameObject GrassFieldBase = null;
        [SerializeField] private GrassFieldCreator VGSFieldCreator = null;
        [SerializeField] private AnimationCurve ScaleUpCurve = null;
        [SerializeField] private Vector2 HoverScaleMinMax = Vector2.zero;
        [SerializeField] private float HoverClickScaleMax = 0.0f, HoverAnimationSpeed = 0.0f, HoverClickAnimationSpeed = 0.0f;

        [HideInInspector] public bool MouseHoveringObject = false, MouseLeftClickFirstFrame = false;
        public delegate void SVGControlArrows();
        public SVGControlArrows MouseLeftClickReleaseEvent;
        [HideInInspector] public Material InstancedMaterial;
        private MeshRenderer meshRenderer;
        private SphereCollider _sphereCollider;
        [SerializeField, HideInInspector] public MainControls VGSMC;
        [SerializeField] private Material OriginalMat;

        private bool FirstFrame = false;
        private void OnEnable()
        {
            FirstFrame = false;
            DisablingByScript = false;
            OneFrameAfterCreation = false;
            MouseLeftClickTrigger1 = false;
            HoverAnimationLerpValue = 0.0f;
            HoverClickAnimationLerpValue = 0.0f;
            MouseLeftClickReleaseEvent += TriggerGrassFieldCreationWhenPossible;
            meshRenderer = GetComponent<MeshRenderer>();
            InstancedMaterial = new Material(OriginalMat);
            meshRenderer.enabled = false;
            CheckedIfAlreadyAddedGrassField = false;
            EditorApplication.update += EditorUpdates;
            OldCreatingGrassField = false;
        }


        private void OnDisable()
        {
            EditorApplication.update -= EditorUpdates;
            MouseLeftClickReleaseEvent -= TriggerGrassFieldCreationWhenPossible;
            if (EditorApplication.isPlayingOrWillChangePlaymode == false && Time.frameCount != 0
                        && Time.renderedFrameCount != 0 || DisablingByScript == true)
            {
                OnSceneAdded = false;
                SceneView.duringSceneGui -= OnScene;
                if (VGSFieldCreator != null) VGSFieldCreator.DisableControlsEvent -= DisableArrow;
                if (VGSMC != null) VGSMC.DisableControlArrows_Event -= DisableArrow;
            }
            if (InstancedMaterial != null) DestroyImmediate(InstancedMaterial);
        }

        private void Awake()
        {
            FirstFrame = false;
        }

        private bool OneFrameAfterCreation;
        [HideInInspector] public bool GrassFieldCreationActivated, CheckedIfAlreadyAddedGrassField;
        private bool OldCreatingGrassField;
        private int OldGrassFieldList;
        public bool Test;
        private void EditorUpdates()
        {
            if (this != null && InstancedMaterial != null)
            {
                if (FirstFrame == false)
                {
                    InitializeArrow();
                    DisableIfAlreadyAddedGrassField();
                    OldGrassFieldList = VGSMC.GrassFieldList.Count;
                    FirstFrame = true;
                }
                else
                {
                    MouseHoveringObject = MouseHoveringObjectTrigger1;
                    if (VGSMC.Wrapping == true) MouseHoveringObject = false;
                    if (VGSMC.CreatingGrassFieldWithoutWrapping == true) DisableIfAlreadyAddedGrassField();
                    if (OldCreatingGrassField == true && VGSMC.CreatingGrassField == false) DisableIfAlreadyAddedGrassField();
                    OldCreatingGrassField = VGSMC.CreatingGrassField;
                    if (VGSMC.GrassFieldList.Count < OldGrassFieldList) DisableIfAlreadyAddedGrassField();
                    OldGrassFieldList = VGSMC.GrassFieldList.Count;
                    if (OneFrameAfterCreation == true)
                    {
                        Selection.activeObject = null;
                        OneFrameAfterCreation = false;
                    }
                    if (GrassFieldCreationActivated == true && VGSMC.ShowingDeleteAssetsUI == false)
                    {
                        HoverScaleChangeMethod();
                        ColorChangeMethod();
                        //ChangeCursorMethod();
                        //TriggerGrassFieldCreation();
                    }
                    if (VGSFieldCreator.KeepSpawningInDirection == true && VGSFieldCreator.WasSpawnedInDirection == ArrowDirection)
                    {
                        TriggerGrassFieldCreationWhenPossible();
                    }
                }
                if (VGSMC.ShowingDeleteAssetsUI == false)
                {
                    if (VGSMC.MouseLeftClicking == true)
                    {
                        MouseLeftClickFirstFrame = false;
                        if (MouseLeftClickTrigger1 == false)
                        {
                            MouseLeftClickFirstFrame = true;
                            MouseLeftClickTrigger1 = true;
                        }
                        if (MouseHoveringObject == true && VGSMC.PressingAlt == false) ClickingWhileHovering = true;
                    }
                    else
                    {
                        if (MouseLeftClickTrigger1 == true)
                        {
                            //MouseLeftClickReleaseEvent?.Invoke();
                            MouseLeftClickTrigger1 = false;
                        }
                        MouseLeftClickTrigger1 = false;
                        MouseLeftClickFirstFrame = false;
                    }
                    if (ClickingWhileHovering == true) ClickingWhileHoveringMethod();
                }
                if (MouseHoveringObject == false || VGSMC.PressingAlt || VGSMC.MouseLeftClicking == false) ClickingWhileHovering = false;
                if (VGSMC.CreatingGrassField == true || GrassFieldDetected == true)
                {
                    GrassFieldCreationActivated = false;
                    if (_sphereCollider.enabled == true) _sphereCollider.enabled = false;
                    if (meshRenderer.enabled == true) meshRenderer.enabled = false;
                    MouseHoveringObject = false;
                    if (OnSceneAdded == true)
                    {
                        SceneView.duringSceneGui -= OnScene;
                        OnSceneAdded = false;
                    }
                    //If there is already a grassfield in the direction and it's supposed to keep spawning, then cancel the spawning
                    if (VGSFieldCreator.KeepSpawningInDirection == true && VGSFieldCreator.WasSpawnedInDirection == ArrowDirection)
                    {
                        VGSFieldCreator.KeepSpawningInDirection = false;
                        if (VGSFieldCreator.GrassFieldThatStartedSpawning != null && VGSMC.SpawnsLeftDict.ContainsKey(VGSFieldCreator.GrassFieldThatStartedSpawning))
                        { VGSMC.SpawnsLeftDict.Remove(VGSFieldCreator.GrassFieldThatStartedSpawning); }
                    }
                }
                if (VGSMC.CreatingGrassField == false && GrassFieldDetected == false && CheckedIfAlreadyAddedGrassField == true)
                {
                    GrassFieldCreationActivated = true;
                    if (_sphereCollider.enabled == false) _sphereCollider.enabled = true;
                    if (meshRenderer.enabled == false) meshRenderer.enabled = true;
                    if (OnSceneAdded == false)
                    {
                        SceneView.duringSceneGui += OnScene;
                        OnSceneAdded = true;
                    }
                }
            }
        }
        private bool ClickingWhileHovering;
        [HideInInspector] public float HoverClickAnimationLerpValue;
        private void ClickingWhileHoveringMethod()
        {
            if (VGSMC.MouseLeftClicking == false && MouseHoveringObject == true && VGSMC.PressingAlt == false)
            {
                if (VGSMC.SpawnsLeftDict.ContainsKey(GrassFieldBase) == false) VGSMC.SpawnsLeftDict.Add(GrassFieldBase, VGSMC.GrassFieldsToSpawnPerClick);
                VGSFieldCreator.GrassFieldThatStartedSpawning = GrassFieldBase;
                MouseLeftClickReleaseEvent?.Invoke();
            }
        }

        private bool MouseLeftClickTrigger1, GrassFieldDetected;
        private void DisableIfAlreadyAddedGrassField()
        {
            Vector2 OwnerFieldHorizPos = new Vector2(GrassFieldBase.transform.position.x, GrassFieldBase.transform.position.z);
            GrassFieldDetected = false;
            for (int i = 0; i < VGSMC.GrassFieldList.Count; i++)
            {
                if (VGSMC.GrassFieldList[i] != GrassFieldBase)
                {
                    Vector2 GrassFieldHorizontalPos = new Vector2(VGSMC.GrassFieldList[i].transform.position.x, VGSMC.GrassFieldList[i].transform.position.z);
                    Vector2 direction = GrassFieldHorizontalPos - OwnerFieldHorizPos;
                    float angle = Vector2.Angle(direction, ArrowDirection);
                    if (angle <= 1.0f && direction.magnitude <= (VGSFieldCreator.VGSMC.GrassFieldMaxDistance + 10.0f)) GrassFieldDetected = true;
                }
            }
            CheckedIfAlreadyAddedGrassField = true;
        }

        private void ColorChangeMethod()
        {
            Color TargetColor = OriginalColor;
            if (MouseHoveringObject == true && VGSMC.PressingAlt == false) TargetColor = MouseHoverColor;
            if (ClickingWhileHovering == true) TargetColor = HoverClickColor;
            InstancedMaterial.SetColor("_UnlitColor", TargetColor);
        }

        [HideInInspector] public float HoverAnimationLerpValue, HoverClickingAnimationLerpValue;
        private bool HoverTrigger1, HoverClickTrigger1;
        private void HoverScaleChangeMethod()
        {
            float HoverTarget = 0.0f;
            float HoverClickTarget = 0.0f;
            if (MouseHoveringObject == true && VGSMC.PressingAlt == false)
            {
                if (HoverTrigger1 == false)
                {
                    //HoverAnimationLerpValue = 0.0f;
                    HoverTrigger1 = true;
                }
                HoverTarget = 1.0f;
            }
            else
            {
                if (HoverTrigger1 == true)
                {
                    //HoverAnimationLerpValue = 1.0f;
                    HoverTrigger1 = false;
                }
                HoverTarget = 0.0f;
            }
            if (ClickingWhileHovering == true)
            {
                if (HoverClickTrigger1 == false)
                {
                    //HoverClickingAnimationLerpValue = 0.0f;
                    HoverClickTrigger1 = true;
                }
                HoverClickTarget = 1.0f;
            }
            else
            {
                if (HoverClickTrigger1 == true)
                {
                    //HoverClickingAnimationLerpValue = 1.0f;
                    HoverClickTrigger1 = false;
                }
                HoverClickTarget = 0.0f;
            }
            HoverAnimationLerpValue = Mathf.MoveTowards(HoverAnimationLerpValue, HoverTarget, HoverAnimationSpeed);
            float AnimatedResult = ScaleUpCurve.Evaluate(HoverAnimationLerpValue);
            HoverClickingAnimationLerpValue = Mathf.MoveTowards(HoverClickingAnimationLerpValue, HoverClickTarget, HoverClickAnimationSpeed);
            float AnimatedResult2 = ScaleUpCurve.Evaluate(HoverClickingAnimationLerpValue);
            transform.localScale = Vector3.one * (Mathf.Lerp(HoverScaleMinMax.x, HoverScaleMinMax.y, AnimatedResult)
                + Mathf.Lerp(0.0f, HoverClickScaleMax, AnimatedResult2));
        }

        private bool OnSceneAdded;
        private void InitializeArrow()
        {
            if (this != null)
            {
                VGSMC = GetComponentInParent<MainControls>();
                OnSceneAdded = true;
                SceneView.duringSceneGui += OnScene;
                VGSFieldCreator = GrassFieldBase.GetComponent<GrassFieldCreator>();
                VGSFieldCreator.DisableControlsEvent += DisableArrow;
                VGSMC.DisableControlArrows_Event += DisableArrow;
                _sphereCollider = GetComponent<SphereCollider>();
                meshRenderer.sharedMaterial = InstancedMaterial;
            }
        }

        private void TriggerGrassFieldCreationWhenPossible()
        {
            if (VGSMC.Wrapping == false && VGSMC.CreatingGrassField == false && GrassFieldCreationActivated == true && this != null)
            {
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
                VGSFieldCreator.AddGrassField(ArrowDirection);
                OneFrameAfterCreation = true;
                VGSFieldCreator.KeepSpawningInDirection = false;
            }
        }

        private bool DisablingByScript;
        private void DisableArrow()
        {
            if (this != null)
            {
                DisablingByScript = true;
                if (gameObject != null) DestroyImmediate(gameObject);
            }
        }

        private bool MouseHoveringObjectTrigger1;
        [HideInInspector] public Vector3 ScreenWorldPosition;
        private Vector3 mouseScreenPos;
        private void OnScene(SceneView scene)
        {
            if (GrassFieldCreationActivated == true && this != null)
            {
                Event _event = Event.current;

                if (_event.type == EventType.MouseMove)
                {
                    mouseScreenPos = _event.mousePosition;
                    float ppp = EditorGUIUtility.pixelsPerPoint;
                    mouseScreenPos.y = scene.camera.pixelHeight - mouseScreenPos.y * ppp;
                    mouseScreenPos.x *= ppp;
                }

                ScreenWorldPosition = scene.camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0.0f));
                Ray ray = scene.camera.ScreenPointToRay(mouseScreenPos);
                RaycastHit[] raycastHits = Physics.RaycastAll(ray);
                GameObject ClosestArrowHit = null;
                float ClosestArrowDistance = Mathf.Infinity;
                for (int i = 0; i < raycastHits.Length; i++)
                {
                    if (raycastHits[i].distance < ClosestArrowDistance)
                    {
                        ControlArrows SVGControlArrows = raycastHits[i].collider.gameObject.GetComponent<ControlArrows>();
                        if (SVGControlArrows != null)
                        {
                            ClosestArrowDistance = raycastHits[i].distance;
                            ClosestArrowHit = raycastHits[i].collider.gameObject;
                        }
                    }
                }
                if (ClosestArrowHit == gameObject)
                {
                    MouseHoveringObjectTrigger1 = true;
                    HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
                }
                else MouseHoveringObjectTrigger1 = false;
            }
        }
#endif
        private void Start()
        {
            if (Application.isPlaying == true) Destroy(gameObject);
        }
    }
}