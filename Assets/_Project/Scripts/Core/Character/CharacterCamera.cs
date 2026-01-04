using _Project.Scripts.Core.InputManagement.ScriptableObjects;
using KinematicCharacterController.Examples;
using System.Linq;
using UnityEngine;

namespace _Project.Scripts.Core.Character
{
    public class CharacterCamera : MonoBehaviour
    {
        [SerializeField] private InputReaderSO inputReader;

        [SerializeField] private ExampleCharacterCamera orbitCamera;
        [SerializeField] private Transform cameraFollowPoint;
        [SerializeField] private CharacterController character;

        private Vector3 lookInputVector = Vector3.zero;
        private void Start()
        {
            orbitCamera.SetFollowTransform(cameraFollowPoint);
            orbitCamera.IgnoredColliders = character.GetComponentsInChildren<Collider>().ToList();
        }
        private void OnEnable()
        {
            inputReader.OnLookEvent += HandleCameraInput;
        }

        private void OnDisable()
        {
            inputReader.OnLookEvent -= HandleCameraInput;
        }
        private void LateUpdate()
        {
            UpdateCamera();
        }
        private void HandleCameraInput(Vector2 lookInput)
        {
            lookInputVector = new Vector3(lookInput.x, lookInput.y, 0);
        }
        private void UpdateCamera()
        {
            orbitCamera.UpdateWithInput(Time.deltaTime, 0, lookInputVector);
        }
    }
}
