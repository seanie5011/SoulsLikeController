using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    // objects
    InputManager inputManager;

    // following
    private Transform targetTransform;  // the object the camera will follow
    private Vector3 cameraFollowVelocity = Vector3.zero;  // the speed and direction it is going now
    [SerializeField] private float cameraFollowSpeed = 0.03f;  // the speed it will follow at

    // rotating
    private float lookAngle;
    [SerializeField] private float cameraLookSpeed = 2f;  // the speed it will look at

    private float pivotAngle;
    [SerializeField] private float cameraPivotSpeed = 2f;  // the speed it will pivot at
    [SerializeField] private float minimumPivotAngle = -30f;
    [SerializeField] private float maximumPivotAngle = 30f;
    [SerializeField] private Transform cameraPivot;  // the object used to pivot

    [SerializeField] private float cameraSmoothTime = 1f;  // used to lerp rotations

    // collisions
    private Transform cameraTransform;  // transform of the actual camera object in scene
    private float defaultCameraZ;  // the cameras default z-position (local)
    [SerializeField] private float cameraCollisionRadius = 0.2f;
    [SerializeField] private LayerMask collisionLayers;  // the layers the camera collides with
    [SerializeField] private float cameraCollisonOffset = 0.2f;  // how much camera escapes from wall when colliding
    [SerializeField] private float minimumCollisionOffset = 0.2f;  // the minimum amount camera escapes from wall
    [SerializeField] private float collisionSmoothening = 0.2f;  // controls the smoothening from default to collision offset positions

    private void Awake()
    {
        // gets the transform from the player
        // assumes only one
        targetTransform = FindObjectOfType<PlayerManager>().transform;

        // instantiate input manager
        // assumes only one
        inputManager = FindObjectOfType<InputManager>();

        // assign camera transform
        cameraTransform = Camera.main.transform;
        // assign cameras default local z-position
        defaultCameraZ = cameraTransform.localPosition.z;
    }

    // handles all camera inputs
    public void HandleAllCameraMovement()
    {
        FollowTarget();
        RotateCamera();
        HandleCameraCollisions();
    }

    // follow any given target
    private void FollowTarget()
    {
        // smooth damp to slowly transition
        // this returns the position we will move camera to now
        // not necessarily the exact target, instead it is smoothly
        // damping as it reaches it (so camera slows as it reaches target)
        Vector3 targetPosition = Vector3.SmoothDamp(
            transform.position, 
            targetTransform.position, 
            ref cameraFollowVelocity, 
            cameraFollowSpeed
        );

        transform.position = targetPosition;
    }

    // rotate camera using either mouse or right joystick
    private void RotateCamera()
    {
        // set the look angle (rotate up and down)
        lookAngle = Mathf.Lerp(lookAngle, lookAngle + (inputManager.cameraHorizontalInput * cameraLookSpeed), cameraSmoothTime * Time.deltaTime);  // lerp to reduce jitter
        // set the transforms (global) rotation
        transform.rotation = Quaternion.Euler(0, lookAngle, 0);

        // set the pivot angle (rotate left to right)
        pivotAngle = Mathf.Lerp(pivotAngle, pivotAngle - (inputManager.cameraVerticalInput * cameraPivotSpeed), cameraSmoothTime * Time.deltaTime);
        // clamp so not too far
        pivotAngle = Mathf.Clamp(pivotAngle, minimumPivotAngle, maximumPivotAngle);
        // set the pivots local rotation
        cameraPivot.localRotation = Quaternion.Euler(pivotAngle, 0, 0);
    }

    // handle case where camera collides with objects (eg: walls)
    private void HandleCameraCollisions()
    {
        // want camera to stay at default z unless we hit something
        // we use a spherecast to see if we have hit something

        // want our target to be the default camera z
        float targetPositionZ = defaultCameraZ;

        // direction from the camera object to the camera pivot object
        Vector3 direction = cameraTransform.position - cameraPivot.position;
        direction.Normalize();

        // use raycast hit to tell us about the cameras collision (if there is one)
        RaycastHit hitInfo;
        // use sphere cast to see if we have hit an object
        // spherecast sends a capsule-like cast of a radius in a direction from origin out to a max distance,
        // checking for collisions with anything on the designated layers and records result in out
        if (Physics.SphereCast(cameraPivot.position, cameraCollisionRadius, direction, out hitInfo, Mathf.Abs(targetPositionZ), collisionLayers))
        {
            // get distance from camera pivot to the collision
            float distance = Vector3.Distance(cameraPivot.position, hitInfo.point);
            // set target position using this distance and offset
            targetPositionZ = -(distance - cameraCollisonOffset);
        }

        // if the target position is at a very small offset
        if (Mathf.Abs(targetPositionZ) < minimumCollisionOffset)
        {
            // set it to minimum
            targetPositionZ = targetPositionZ - minimumCollisionOffset;
        }

        // initialise the new camera position at the current local
        Vector3 newCameraPosition = cameraTransform.localPosition;
        // set this z to lerp between target and current
        newCameraPosition.z = Mathf.Lerp(newCameraPosition.z, targetPositionZ, collisionSmoothening);
        // set the current local to new
        cameraTransform.localPosition = newCameraPosition;
    }
}
