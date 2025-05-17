using UnityEngine;

namespace VibrantGrassShader
{
    public class ExampleLightAndInteractObject : MonoBehaviour
    {
        [SerializeField] private float MovementSpeed = 50.0f;
        [SerializeField] private float StickDistance = 1000.0f;
        [SerializeField] private LayerMask GroundLayers;

        private bool RotateTrigger1;
        void Update()
        {
            RaycastHit rayHit = new RaycastHit();
            bool OnGround = Physics.Raycast(transform.position + Vector3.up * (StickDistance / 2.0f), Vector3.down, out rayHit, StickDistance, GroundLayers, QueryTriggerInteraction.Ignore);
            if (OnGround == false && RotateTrigger1 == false)
            {
                transform.Rotate(Vector3.up, 180.0f);
                RotateTrigger1 = true;
            }
            if (OnGround == true)
            {
                RotateTrigger1 = false;
                transform.position = new Vector3(transform.position.x, rayHit.point.y, transform.position.z);
            }
            transform.Translate(Vector3.forward * MovementSpeed * Time.deltaTime);
        }
    }
}