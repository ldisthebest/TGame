using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerState
{
    Idel = 0,
    Run = 1,
    Climb = 2
}

public class PlayerAction : MonoBehaviour {

    Animator animator;
    PlayerState currentState;
    public PlayerState CurrentState
    {
        get
        {
            return currentState;
        }
    }

    void Awake()
    {
        animator = GetComponent<Animator>();
        currentState = PlayerState.Idel;
    }
    public void SetPlayerAnimation(PlayerState state)
    {
        currentState = state;
        switch(state)
        {
            case PlayerState.Idel:
                animator.Play("Idel");
                break;
            case PlayerState.Run:
                animator.Play("Run");
                break;
            case PlayerState.Climb:
                animator.Play("Climb");
                break;
        }
    }
	
}
