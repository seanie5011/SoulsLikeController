using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    // can now use input system as a class
    PlayerControls playerControls;

    // movement input is a vector2
    Vector2 movementInput;
    // separate vector2 into two floats
    public float movementVerticalInput;
    public float movementHorizontalInput;

    // camera input
    Vector2 cameraInput;
    // separate vector2 into two floats
    public float cameraVerticalInput;
    public float cameraHorizontalInput;

    // action input
    public bool sprintingInput = false;
    public bool walkingInput = false;

    // animator
    AnimatorManager animatorManager;

    // on creation
    private void Awake()
    {
        // instantiate animator manager
        animatorManager = GetComponent<AnimatorManager>();
    }

    // when this script is enabled
    private void OnEnable()
    {
        // if there is no playerControls
        if (playerControls == null)
        {
            // make one
            playerControls = new PlayerControls();

            // subscribe the performed event to the i delegate
            // the i delegate assigns the value of movementInput
            // i is the callback context of the input actions
            // essentially, when an action is performed, reassign movementInput
            playerControls.PlayerMovement.Movement.performed += (i) =>
            {
                movementInput = i.ReadValue<Vector2>();
            };  // could rewrite without curved or curly brackets

            // now handle camera input
            playerControls.PlayerMovement.Camera.performed += (i) => cameraInput = i.ReadValue<Vector2>();

            // sprinting input
            // hold to sprint
            playerControls.PlayerActions.Sprinting.performed += (i) => sprintingInput = true;
            playerControls.PlayerActions.Sprinting.canceled += (i) => sprintingInput = false;

            // waling input (keyboard only)
            // hold to walk
            playerControls.PlayerActions.Walking.performed += (i) => walkingInput = true;
            playerControls.PlayerActions.Walking.canceled += (i) => walkingInput = false;
        }

        // enable controls
        playerControls.Enable();
    }

    // when the script is disabled
    private void OnDisable()
    {
        // disable controls
        playerControls.Disable();
    }

    // function to call input functions
    public void HandleAllInputs()
    {
        // movement input
        HandleMovementInput();
        // camera input
        HandleCameraInput();
        // expand by calling other inputs like jump, action, etc
    }

    // assign floats for movement inputs
    private void HandleMovementInput()
    {
        // assign both horizontal and vertical floats
        movementHorizontalInput = movementInput.x;
        movementVerticalInput = movementInput.y;

        // handle animations
        // for now no horizontal (strafing) animation, so keep zero
        // only vertical (forward) animation, so clamp and addition
        // values are positive only as we have no moving backwards animation
        float moveAmount = Mathf.Clamp01(Mathf.Abs(movementHorizontalInput) + Mathf.Abs(movementVerticalInput));

        // if walking input (keyboard only)
        // only works if moveAmount is 1, sets to 0.5
        if (walkingInput && !sprintingInput && moveAmount == 1f)
        { 
            moveAmount = 0.5f;
        }

        animatorManager.UpdateAnimatorValues(0, moveAmount, sprintingInput);
    }

    // assign floats for camera inputs
    private void HandleCameraInput()
    {
        // assign both horizontal and vertical floats
        cameraHorizontalInput = cameraInput.x;
        cameraVerticalInput = cameraInput.y;
    }
}
