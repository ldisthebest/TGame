using UnityEngine;

public enum PlayerState
{
    Idel = 0,
    Run = 1,
    Climb = 2,
    Push = 3,
    Pull = 4
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
                PlayAnimation("Idel");
                break;
            case PlayerState.Run:
                PlayAnimation("Run");
                break;
            case PlayerState.Climb:
                PlayAnimation("Climb");
                break;
            case PlayerState.Push:
                PlayAnimation("Push");
                break;
            //暂无拉箱子动画，以推箱子动画代替
            case PlayerState.Pull:
                PlayAnimation("Push");
                break;
        }
    }

    void PlayAnimation(string AnimClip)
    {
        if(!animator.GetCurrentAnimatorStateInfo(0).IsName(AnimClip))
        {
            animator.Play(AnimClip);
        }
    }
	
}
