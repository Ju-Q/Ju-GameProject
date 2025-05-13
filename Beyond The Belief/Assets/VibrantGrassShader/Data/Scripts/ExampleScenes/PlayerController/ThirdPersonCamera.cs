using UnityEngine;

namespace VibrantGrassShader
{
    public class ThirdPersonCamera : MonoBehaviour
    {
        [SerializeField] public GameObject TargetPos = null;
        [SerializeField]
        private float mouseSensitivity = 0.0f, mouseRotationSmoothTime = 0.0f,
            DistFromPlayer = 0.0f, CamHeight = 0.0f, CamPositionSmoothTime = 0.0f, CamYPosSmoothTime = 0.0f, ObstacleDistance = 0.0f, ObstacleDistanceSmoothTime = 0.0f;
        [SerializeField] private Vector2 pitchMinMax = Vector2.zero;
        [HideInInspector] public bool CameraControlsActivated = true;
        [HideInInspector] public Vector3 AnimatedRotation = Vector3.zero, AnimatedPosition = Vector3.zero;
        private Vector3 ControlledRotation, ControlledPosition;

        private float yaw, pitch;
        Vector3 CurrentRotation, camPosTarget, mouseRotationSmoothVelocity, camPositionSmoothVelocity, CameraPositionSmoothed;
        Vector3 AddedPositionSmoothed;
        float ObstacleDistancePosSmoothed, ObstacleDistancePosSmoothTarget, ObstacleDistancePosSmoothRef;

        private void Awake()
        {
            Cursor.visible = false;
            //transform.position = TargetPos.transform.position - transform.forward * (DistFromPlayer) + new Vector3(0, CamHeight, 0); 
            CameraControlsActivated = true;
        }

        void LateUpdate()
        {
            CameraControlsApplication();
        }

        private void CameraControlsApplication()
        {
            //Need to be Rotation first, then Position 
            if (CameraControlsActivated == true)
            {
                ControlledRotation = CameraRotation();
            }
            transform.eulerAngles = ControlledRotation;

            if (CameraControlsActivated == true)
            {
                ControlledPosition = CameraPosition();
            }
            transform.position = ControlledPosition;
        }

        private float TargetPosYSmoothed, camPosYSmoothRef;
        private Vector3 CameraPosition()
        {
            //Calculate where the Camera should go 
            TargetPosYSmoothed = Mathf.SmoothDamp(TargetPosYSmoothed, TargetPos.transform.position.y, ref camPosYSmoothRef, CamYPosSmoothTime);
            Vector3 TargetPosWithYSmoothed = new Vector3(TargetPos.transform.position.x, TargetPosYSmoothed, TargetPos.transform.position.z);
            camPosTarget = TargetPosWithYSmoothed - transform.forward * DistFromPlayer + new Vector3(0, CamHeight, 0);
            CameraPositionSmoothed = Vector3.SmoothDamp(CameraPositionSmoothed, camPosTarget, ref camPositionSmoothVelocity, CamPositionSmoothTime);
            //Detect Obstacles 
            Vector3 RayDirection = camPosTarget - TargetPosWithYSmoothed;
            float MaxDistance = Vector3.Distance(camPosTarget, TargetPosWithYSmoothed);
            bool ObstacleDetected = Physics.Raycast(TargetPosWithYSmoothed, RayDirection, out RaycastHit hitInfo, MaxDistance + ObstacleDistance);

            Vector3 CameraPositionResult = new Vector3(CameraPositionSmoothed.x, camPosTarget.y, CameraPositionSmoothed.z);
            if (ObstacleDetected == true)
            {
                ObstacleDistancePosSmoothTarget = ObstacleDistance;
                Vector3 PositionTargetResult = hitInfo.point + (-RayDirection.normalized * ObstacleDistance);
                transform.position = PositionTargetResult;//transform.position 
                CameraPositionSmoothed = PositionTargetResult;
                CameraPositionResult = PositionTargetResult;
            }
            else
            {
                ObstacleDistancePosSmoothTarget = 0.0f;
            }
            ObstacleDistancePosSmoothed = Mathf.SmoothDamp(ObstacleDistancePosSmoothed, ObstacleDistancePosSmoothTarget, ref ObstacleDistancePosSmoothRef, ObstacleDistanceSmoothTime);
            AddedPositionSmoothed = transform.forward * ObstacleDistancePosSmoothed;
            AddedPositionSmoothed = Vector3.zero;

            Vector3 CameraPositionResult2 = CameraPositionResult;
            return CameraPositionResult2;
        }

        private Vector3 CameraRotation()
        {
            //reset Inputs to 0 the first frame ? (To Avoid Teleport xD) 
            yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
            pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
            float pitchClamped = Mathf.Clamp(pitch, pitchMinMax.x, pitchMinMax.y);
            pitch = pitchClamped;
            CurrentRotation = Vector3.SmoothDamp(CurrentRotation, new Vector3(pitchClamped, yaw), ref mouseRotationSmoothVelocity, mouseRotationSmoothTime);
            //transform.eulerAngles = CurrentRotation;
            return CurrentRotation;
        }
    }
}