using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;
using KinematicCharacterController.Examples;

namespace KinematicCharacterController.Examples
{
    public class ExamplePlayer : MonoBehaviour
    {
        public ExampleCharacterController Character;
        public ExampleCharacterCamera CharacterCamera;

        // private const string MouseXInput = "Mouse X";
        // private const string MouseYInput = "Mouse Y";
        // private const string MouseScrollInput = "Mouse ScrollWheel";
        // private const string HorizontalInput = "Horizontal";
        // private const string VerticalInput = "Vertical";
        
        public Func<float>  MouseXInput { get; set; }
        public Func<float>  MouseYInput { get; set; }
        public Func<float>  MouseScrollInput { get; set; }
        public Func<float> HorizontalInput { get; set; }
        public Func<float> VerticalInput { get; set; }
        public Func<bool> JumpDownInput { get; set; }
        public Func<bool> CrouchDownInput { get; set; }
        public Func<bool> CrouchUpInput { get; set; }
        public Func<bool> ZoomInput { get; set; }
        public Func<bool> LockInput { get; set; }

        private PlayerCharacterInputs characterInputs = new();

        private void Start()
        {
            //Cursor.lockState = CursorLockMode.Locked;

            // Tell camera to follow transform
            CharacterCamera.SetFollowTransform(Character.CameraFollowPoint);

            // Ignore the character's collider(s) for camera obstruction checks
            CharacterCamera.IgnoredColliders.Clear();
            CharacterCamera.IgnoredColliders.AddRange(Character.GetComponentsInChildren<Collider>());

            //SetDefaultInput();
        }

        private void Update()
        {
            if (LockInput())
            {
                Cursor.lockState = CursorLockMode.Locked;
            }

            HandleCharacterInput();
        }

        private void LateUpdate()
        {
            // Handle rotating the camera along with physics movers
            if (CharacterCamera.RotateWithPhysicsMover && Character.Motor.AttachedRigidbody != null)
            {
                CharacterCamera.PlanarDirection = Character.Motor.AttachedRigidbody.GetComponent<PhysicsMover>().RotationDeltaFromInterpolation * CharacterCamera.PlanarDirection;
                CharacterCamera.PlanarDirection = Vector3.ProjectOnPlane(CharacterCamera.PlanarDirection, Character.Motor.CharacterUp).normalized;
            }

            HandleCameraInput();
        }

        private void SetDefaultInput()
        {
            MouseXInput = () => Input.GetAxis("Mouse X");
            MouseYInput = () => Input.GetAxis("Mouse Y");
            MouseScrollInput = () => Input.GetAxis("Mouse ScrollWheel");
            HorizontalInput = () => Input.GetAxis("Horizontal");
            VerticalInput = () => Input.GetAxis("Vertical");
            JumpDownInput = () => Input.GetKeyDown(KeyCode.Space);
            CrouchDownInput = () => Input.GetKeyDown(KeyCode.C);
            CrouchUpInput = () => Input.GetKeyUp(KeyCode.C);
            ZoomInput = () => Input.GetMouseButtonDown(1);
            LockInput = () => Input.GetMouseButtonDown(0);
        }

        private void HandleCameraInput()
        {
            // Create the look input vector for the camera
            float mouseLookAxisUp = MouseYInput();
            float mouseLookAxisRight = MouseXInput();
            
            Vector3 lookInputVector = new Vector3(mouseLookAxisRight, mouseLookAxisUp, 0f);

            // Prevent moving the camera while the cursor isn't locked
            // if (Cursor.lockState != CursorLockMode.Locked)
            // {
            //     lookInputVector = Vector3.zero;
            // }

            // Input for zooming the camera (disabled in WebGL because it can cause problems)
            float scrollInput = -MouseScrollInput();
#if UNITY_WEBGL
        scrollInput = 0f;
#endif

            // Apply inputs to the camera
            CharacterCamera.UpdateWithInput(Time.deltaTime, scrollInput, lookInputVector);

            // Handle toggling zoom level
            if (ZoomInput())
            {
                CharacterCamera.TargetDistance = (CharacterCamera.TargetDistance == 0f) ? CharacterCamera.DefaultDistance : 0f;
            }
        }

        private void HandleCharacterInput()
        { 
            // Build the CharacterInputs struct
            characterInputs.MoveAxisForward = VerticalInput();
            characterInputs.MoveAxisRight = HorizontalInput();
            characterInputs.CameraRotation = CharacterCamera.Transform.rotation;
            characterInputs.JumpDown = JumpDownInput();
            characterInputs.CrouchDown = CrouchDownInput();
            characterInputs.CrouchUp = CrouchUpInput();

            // Apply inputs to character
            Character.SetInputs(ref characterInputs);
        }
    }
}