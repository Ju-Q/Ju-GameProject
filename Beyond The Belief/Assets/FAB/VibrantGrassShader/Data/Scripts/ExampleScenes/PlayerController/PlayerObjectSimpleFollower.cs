using UnityEngine;

namespace VibrantGrassShader
{
    public class PlayerObjectSimpleFollower : MonoBehaviour
    {
        [SerializeField] private GameObject PlayerController = null;
        [SerializeField] private float MovementSpeed = 1.0f;
        private Vector3 PositionSmoothRef;
        private CharacterController characterController;

        private void Start()
        {
            characterController = GetComponent<CharacterController>();
        }
        void Update()
        {
            characterController.Move((PlayerController.transform.position - transform.position) * MovementSpeed * Time.deltaTime);
        }
    }
}