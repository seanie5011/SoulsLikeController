using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEditor.PackageManager;
using UnityEngine;

public class PlayerLocomotion : MonoBehaviour
{
    // input manager used
    InputManager inputManager;
    // camera being used
    Transform cameraObject;
    // rigidbody used
    Rigidbody playerRigidbody;
    // player manager used
    PlayerManager playerManager;
    // animator used
    AnimatorManager animatorManager;

    // direction to move
    Vector3 moveDirection;
    // speeds to move and rotate at
    [Header("Movement Speeds")]
    [SerializeField] private float walkingSpeed = 2f;
    [SerializeField] private float runningSpeed = 5f;
    [SerializeField] private float sprintingSpeed = 7f;
    [SerializeField] private float rotationSpeed = 15f;

    // if using snapping
    [SerializeField] private bool snapping = true;

    // how long in air for
    private float inAirTimer = 0f;
    [Header("Falling/Jumping Parameters")]
    public bool isGrounded = true;  // whether on ground (set to what should be at start)
    [SerializeField] private float leapingSpeed = 2f;  // boost off ledge
    [SerializeField] private float fallingSpeed = 1000f;  // fall fast
    [SerializeField] private float maxDistance = 0.5f;  // for raycast
    [SerializeField] private LayerMask groundLayer;  // what can we stand on
    [SerializeField] private float rayCastHeightOffset = 0.5f;  // to set raycast slightly above feet

    public bool isJumping = false;
    [SerializeField] private float gravityIntensity = -10f;
    [SerializeField] private float jumpHeight = 1f;

    // stair target y
    private float targetY;

    // calls on creation
    private void Awake()
    {
        // instantiate components
        inputManager = GetComponent<InputManager>();
        playerRigidbody = GetComponent<Rigidbody>();
        cameraObject = Camera.main.transform;  // main camera
        playerManager = GetComponent<PlayerManager>();
        animatorManager = GetComponent<AnimatorManager>();
    }

    // handle movement and rotation
    public void HandleAllMovement()
    {
        // always want to be able to handle these
        HandleFallingAndLanding();

        // if can interact
        if (!playerManager.isInteracting)
        {
            // moving and rotating
            HandleMovement();
            HandleRotation();
        }
    }

    // assigning player movements
    private void HandleMovement()
    {
        // assign the z component (forward, so vertical input (w or s))
        moveDirection = cameraObject.forward * inputManager.movementVerticalInput;
        // add the x component (right, so horizontal input (a or d))
        moveDirection = moveDirection + cameraObject.right * inputManager.movementHorizontalInput;

        // remove y component
        moveDirection.y = 0;

        // snap if desired
        if (snapping)
        {
            moveDirection = SnapMovement(moveDirection);
        }

        // set speed depending on input
        float movementSpeed;

        // if sprinting and moving fast
        if (inputManager.sprintingInput && moveDirection.magnitude > 0.5)
        {
            movementSpeed = sprintingSpeed;
        }
        // if walking and moving fast (keyboard only)
        else if (inputManager.walkingInput && moveDirection.magnitude > 0.5)
        {
            movementSpeed = walkingSpeed;
        }
        // just moving fast
        else if (moveDirection.magnitude > 0.5)
        {
            movementSpeed = runningSpeed;
        }
        // just moving
        else
        {
            movementSpeed = walkingSpeed;
        }

        // assign the movement velocity to rigidbody by multiplying direction by speed
        playerRigidbody.velocity = new Vector3(moveDirection.x * movementSpeed, playerRigidbody.velocity.y, moveDirection.z * movementSpeed);
    }

    // assigning player rotations
    private void HandleRotation()
    {
        // initialise what direction we want to turn the player
        Vector3 targetDirection = Vector3.zero;
        // player should face direction its moving so same as movement
        targetDirection = cameraObject.forward * inputManager.movementVerticalInput;
        // add the x component (right, so horizontal input (a or d))
        targetDirection = targetDirection + cameraObject.right * inputManager.movementHorizontalInput;
        // normalise
        targetDirection.Normalize();
        // remove y component
        targetDirection.y = 0;

        // if player stops we want to keep rotation
        if (targetDirection == Vector3.zero)
        {
            // set to current direction
            targetDirection = transform.forward;
        }

        // create a rotation object
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        // change rotation by Slerping betweem current and target affected by rotation speed
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    // want to shoot a raycast to see if we are standing on an object
    private void HandleFallingAndLanding()
    {
        // info on cast
        RaycastHit hitInfo;
        // where cast starts from (player feet)
        Vector3 rayCastOrigin = transform.position;
        // need height offset to cast just slightly above players feet
        rayCastOrigin.y = rayCastOrigin.y + rayCastHeightOffset;

        // if not on the ground or jumping
        if (!isGrounded && !isJumping)
        {
            // if not locked in animation
            if (!playerManager.isInteracting)
            {
                // lock into falling animation
                animatorManager.PlayTargetAnimation("Fall", true);
            }

            // increase timer in air
            inAirTimer += Time.deltaTime;
            // leap forward off ledge first
            playerRigidbody.AddForce(transform.forward * leapingSpeed);
            // add downward force
            playerRigidbody.AddForce(-Vector3.up * inAirTimer * fallingSpeed);
        }

        // first set target to current
        targetY = transform.position.y;

        // if cast detects ground beneath us (no longer falling)
        if (Physics.SphereCast(rayCastOrigin, 0.2f, -Vector3.up, out hitInfo, maxDistance, groundLayer)) 
        {
            // if we are not currently grounded and locked into animation (falling)
            if (!isGrounded && playerManager.isInteracting)
            {
                // lock into land animation
                animatorManager.PlayTargetAnimation("Land", true);
            }

            // get raycast new y
            targetY = hitInfo.point.y;

            // reset timer (have now landed)
            inAirTimer = 0f;
            // now grounded
            isGrounded = true;
        }
        // no ground detected
        else
        {
            // not on ground
            isGrounded = false;
        }

        // stair check
        if (isGrounded && !isJumping)
        {
            if (playerManager.isInteracting || inputManager.moveAmount > 0f)
            {
                transform.position = Vector3.Lerp(transform.position, new Vector3(transform.position.x, targetY, transform.position.z), Time.deltaTime / 0.1f);
            }
            else
            {
                transform.position = new Vector3(transform.position.x, targetY, transform.position.z);
            }
        }
    }

    // if jumping input used
    public void HandleJumping()
    {
        // if we are on the ground
        if (isGrounded && !isJumping)
        {
            // set animation bool and target animations
            animatorManager.animator.SetBool("isJumping", true);
            animatorManager.PlayTargetAnimation("Jump", false);

            // get speed based on kinematics equation (v^2 = u^2 + 2*a*s)
            float jumpingSpeed = Mathf.Sqrt(-2 * gravityIntensity * jumpHeight);
            // make new vector
            Vector3 moveVelocity = moveDirection;
            moveVelocity.y = jumpingSpeed;
            // set to rigidbody
            playerRigidbody.velocity = moveVelocity;
            Debug.Log("yes");
        }
    }

    // snap vector3 if in range
    private Vector3 SnapMovement(Vector3 movementVector)
    {
        // vector we will return
        Vector3 snappedVector = movementVector.normalized;

        // magnitude of the original vector
        float movement = movementVector.magnitude;

        // if walking
        if (movement > 0 && movement <= 0.5f)
        {
            snappedVector *= 0.5f;
        }
        // if running
        else if (movement > 0.5f)
        {
            snappedVector *= 1f;
        }
        // negative directions
        else if (movement < 0 && movement >= -0.5f)
        {
            snappedVector *= -0.5f;
        }
        else if (movement < -0.5f)
        {
            snappedVector *= -1f;
        }
        // must be zero
        else
        {
            snappedVector *= 0;
        }

        return snappedVector;
    }
}
