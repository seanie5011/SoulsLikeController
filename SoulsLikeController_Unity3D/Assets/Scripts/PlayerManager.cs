using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// handles everything to be called for player
public class PlayerManager : MonoBehaviour
{
    // objects
    InputManager inputManager;
    PlayerLocomotion playerLocomotion;
    CameraManager cameraManager;

    // animation
    Animator animator;
    public bool isInteracting = true;

    // on creation
    private void Awake()
    {
        // get components
        inputManager = GetComponent<InputManager>();
        playerLocomotion = GetComponent<PlayerLocomotion>();
        animator = GetComponent<Animator>();
        // assumes only one camera manager
        cameraManager = FindObjectOfType<CameraManager>();
    }

    // every frame
    private void Update()
    {
        // call inputs
        inputManager.HandleAllInputs();
    }

    // every physics frame
    private void FixedUpdate()
    {
        // affects rigidbody so in fixed update
        playerLocomotion.HandleAllMovement();
    }

    // calls after frame has ended
    private void LateUpdate()
    {
        // camera stuff after everything else, so late update
        cameraManager.HandleAllCameraMovement();

        // set the players bool to animators bool
        isInteracting = animator.GetBool("isInteracting");
    }
}
