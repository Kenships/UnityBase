using _Project.Scripts.Core.InputManagement.ScriptableObjects;
using KinematicCharacterController;
using UnityEngine;

namespace _Project.Scripts.Core.Character
{
    public class CharacterController : MonoBehaviour, ICharacterController
    {
        [SerializeField] private InputReaderSO inputReader;
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private KinematicCharacterMotor motor;

        [SerializeField] private float walkSpeed = 5f;
        [SerializeField] private float sprintSpeed = 10f;
        [SerializeField] private float rotationSpeed = 10f;
        [SerializeField] private float jumpHeight = 5f;
        [SerializeField] private float gravity = 20f;

        private Vector2 rawMoveInput;
        private Vector3 moveInputVector;
        private float currentMovementSpeed;
        private Vector3 lastLookDirection;
        private bool jumpRequested;

        private void Start()
        {
            motor = GetComponent<KinematicCharacterMotor>();
            motor.CharacterController = this;
            currentMovementSpeed = walkSpeed;
            lastLookDirection = Vector3.forward;
        }

        private void OnEnable()
        {
            inputReader.OnMoveEvent += HandleMove;
            inputReader.OnSprintEvent += HandleSprint;
            inputReader.OnJumpEvent += HandleJump;
        }

        private void OnDisable()
        {
            inputReader.OnMoveEvent -= HandleMove;
            inputReader.OnSprintEvent -= HandleSprint;
            inputReader.OnJumpEvent -= HandleJump;
        }

        private void HandleMove(Vector2 movementInput)
        {
            rawMoveInput = movementInput;
        }
        private void UpdateMoveVector()
        {
            Vector3 cameraForward = cameraTransform.forward;
            Vector3 cameraRight = cameraTransform.right;
            cameraForward.y = 0;
            cameraRight.y = 0;
            cameraForward.Normalize();
            cameraRight.Normalize();

            // character direction is relative to the camera
            moveInputVector = (cameraForward * rawMoveInput.y + cameraRight * rawMoveInput.x).normalized;

            if (moveInputVector.sqrMagnitude > 0.01f)
            {
                // for rotation
                lastLookDirection = moveInputVector.normalized;
            }
        }

        private void HandleSprint(bool isSprinting)
        {
            if (isSprinting)
            {
                currentMovementSpeed = sprintSpeed;
            }
            else
            {
                currentMovementSpeed = walkSpeed;
            }
        }

        private void HandleJump()
        {
            jumpRequested = true;
        }

        public void BeforeCharacterUpdate(float deltaTime)
        {
            UpdateMoveVector();
        }

        public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lastLookDirection);
            currentRotation = Quaternion.Slerp(currentRotation, targetRotation, rotationSpeed * deltaTime);
        }

        public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            if (jumpRequested && motor.GroundingStatus.IsStableOnGround)
            {
                currentVelocity.y = Mathf.Sqrt(2 * jumpHeight * gravity);
                jumpRequested = false;
                motor.ForceUnground();
            }

            if (!motor.GroundingStatus.IsStableOnGround)
            {
                currentVelocity.y -= gravity * deltaTime;
            }
            else if (currentVelocity.y < 0)
            {
                currentVelocity.y = 0;
            }

            Vector3 horizontalTarget = moveInputVector * currentMovementSpeed;
            currentVelocity.x = Mathf.Lerp(currentVelocity.x, horizontalTarget.x, 10f * deltaTime);
            currentVelocity.z = Mathf.Lerp(currentVelocity.z, horizontalTarget.z, 10f * deltaTime);
        }

        public void AfterCharacterUpdate(float deltaTime) { }
        public bool IsColliderValidForCollisions(Collider coll) { return true; }
        public void OnDiscreteCollisionDetected(Collider hitCollider) { }
        public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) { }
        public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) { }
        public void PostGroundingUpdate(float deltaTime) { }
        public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport) { }
    }
}