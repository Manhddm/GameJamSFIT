using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayOneShotBehaviour : StateMachineBehaviour
{
    public AudioClip soundToPlay;

    public float volume = 1;
    public bool playOnEnter = true, playOnExit = false, playAfterDelay = false;

    public float playDelay;
    private bool hasDelayedSoundPlay = false;
    private float timeSinceEntered;
    
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (playOnEnter)
        {
            AudioSource.PlayClipAtPoint(soundToPlay,animator.gameObject.transform.position,volume);
        }

        timeSinceEntered = 0f;
        hasDelayedSoundPlay = false;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (playAfterDelay && !hasDelayedSoundPlay)
        {
            timeSinceEntered += Time.deltaTime;
            if (timeSinceEntered > playDelay)
            {
                AudioSource.PlayClipAtPoint(soundToPlay,animator.gameObject.transform.position,volume);
                hasDelayedSoundPlay = true;
                timeSinceEntered = 0f;
            }
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (playOnExit)
        {
            AudioSource.PlayClipAtPoint(soundToPlay,animator.gameObject.transform.position,volume);
        }
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
