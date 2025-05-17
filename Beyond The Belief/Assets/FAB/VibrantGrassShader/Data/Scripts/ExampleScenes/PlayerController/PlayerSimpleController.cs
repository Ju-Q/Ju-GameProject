using UnityEngine;
using VibrantGrassShaderTools;

namespace VibrantGrassShader
{
    public class PlayerSimpleController : MonoBehaviour
    {
        [SerializeField] public GameObject _camera = null;
        [SerializeField] private LayerMask GroundLayers = 0;
        [SerializeField] public float MovementSpeed = 0.0f, SpeedSmoothTime = 0.0f, HoverHeight = 0.0f;
        [Foldout("Controls")]
        [SerializeField]
        public KeyCode Forward = KeyCode.UpArrow, Backward = KeyCode.DownArrow, Left = KeyCode.LeftArrow,
            Right = KeyCode.RightArrow, Run = KeyCode.LeftShift, ForwardAlternative = KeyCode.Z, ForwardAlternative2 = KeyCode.W,
            LeftAlternative = KeyCode.A, LeftAlternative2 = KeyCode.Q, BackwardAlternative = KeyCode.S, RightAlternative = KeyCode.D;
        [HideInInspector]
        public Vector3 TotalMoveAmount,
            InputDirectionWithCam, HorizontalMoveAmountSmoothed;
        private float SpeedSmoothTimeApplied;
        private Vector3 InputDirectionClamped, SpeedSmoothRef;
        [HideInInspector] public CharacterController _characterController;
        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            SpeedSmoothTimeApplied = SpeedSmoothTime;
        }

        void Update()
        {
            //Get InputDirection 
            Vector3 InputDirection = Vector3.zero;
            if (Input.GetKey(Forward) || Input.GetKey(ForwardAlternative) || Input.GetKey(ForwardAlternative2)) InputDirection += new Vector3(0.0f, 0.0f, 1.0f);
            if (Input.GetKey(Backward) || Input.GetKey(BackwardAlternative)) InputDirection += new Vector3(0.0f, 0.0f, -1.0f);
            if (Input.GetKey(Left) || Input.GetKey(LeftAlternative) || Input.GetKey(LeftAlternative2)) InputDirection += new Vector3(-1.0f, 0.0f, 0.0f);
            if (Input.GetKey(Right) || Input.GetKey(RightAlternative)) InputDirection += new Vector3(1.0f, 0.0f, 0.0f);
            InputDirectionClamped = Vector3.ClampMagnitude(InputDirection, 1.0f);//Clamp Magnitude to avoid going fast with diagonals 
            InputDirectionWithCam = new Vector3(_camera.transform.TransformDirection(InputDirectionClamped).x, InputDirectionClamped.y, _camera.transform.TransformDirection(InputDirectionClamped).z);

            //Movement Calcul 
            HorizontalMoveAmountSmoothed = Vector3.SmoothDamp(HorizontalMoveAmountSmoothed, InputDirectionWithCam * MovementSpeed, ref SpeedSmoothRef, SpeedSmoothTimeApplied);

            float YMovement = 0.0f;
            RaycastHit groundRayHit = new RaycastHit();
            bool GroundHit = Physics.Raycast(transform.position + Vector3.up * 10.0f, Vector3.down, out groundRayHit, 50.0f, GroundLayers, QueryTriggerInteraction.Ignore);
            if (GroundHit == true)
            {
                YMovement = (groundRayHit.point.y + HoverHeight - transform.position.y) * 10.0f;
            }
            TotalMoveAmount = new Vector3(HorizontalMoveAmountSmoothed.x, YMovement, HorizontalMoveAmountSmoothed.z);
            _characterController.Move(TotalMoveAmount * Time.deltaTime);
        }
    }
}