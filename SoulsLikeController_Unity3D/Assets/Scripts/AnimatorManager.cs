using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorManager : MonoBehaviour
{
    // animator on player
    Animator animator;
    // used to id parameters in animator
    int horizontal;
    int vertical;

    // if using snapping
    public bool snapping = true;

    // on creation
    private void Awake()
    {
        // instantiate component
        animator = GetComponent<Animator>();
        // this obtains references to the horizonal and vertical parameters in the animator
        // essentially these are integer ids
        horizontal = Animator.StringToHash("Horizontal");
        vertical = Animator.StringToHash("Vertical");
    }

    // plays animation specified by string
    // if interacting then locked into animation
    public void PlayTargetAnimation(string targetAnimation, bool isInteracting)
    {
        // set the bool as inputted
        animator.SetBool("isInteracting", isInteracting);
        // cross fade between current and target
        animator.CrossFade(targetAnimation, 0.2f);
    }

    // changes animator paramaters based on movement
    public void UpdateAnimatorValues(float horizontalMovement, float verticalMovement, bool isSprinting)
    {
        // if snapping, use it
        if (snapping) 
        {
            horizontalMovement = SnapMovement(horizontalMovement);
            verticalMovement = SnapMovement(verticalMovement);
        }

        // check sprinting
        if (isSprinting && verticalMovement > 0.5)
        {
            verticalMovement = 2f;
        }

        // id the parameter want to change, the value to change it to, the damping (blend time), the delta time
        animator.SetFloat(horizontal, horizontalMovement, 0.1f, Time.deltaTime);
        animator.SetFloat(vertical, verticalMovement, 0.1f, Time.deltaTime);
    }

    // snap values if in range
    private float SnapMovement(float movement)
    {
        // if walking
        if (movement > 0 && movement <= 0.5f)
        {
            return 0.5f;
        }
        // if running
        else if (movement > 0.5f)
        {
            return 1f;
        }
        // negative directions
        else if (movement < 0 && movement >= -0.5f)
        {
            return -0.5f;
        }
        else if (movement < -0.5f)
        {
            return -1f;
        }
        // must be zero
        else
        {
            return 0;
        }
    }
}
