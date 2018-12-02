using UnityEngine;

public enum PlayerState
{
    Idel = 0,
    Run = 1,
    Climb = 2,
    Push = 3,
    Pull = 4,
    Slide = 5
}

public enum PlayerStuckInfo
{
    UnableToClimb = 0,
    UnbaleToFall,
    UnablePullToWall,
    UnablePushToWall,
    UnablePullToPit,
    UnablePushToPit,
    OnMaskBorderTop,
    MoveToBorderTop,
    FallToBorderTop,
    OnMaskInsideY,
    PlayerStuckedByMaskVertex,
    PushStuckedByMaskVertex,
    PullStuckedByMaskVertex,
    Unknow
}

public class PlayerAction : MonoBehaviour {

    #region 私有字段

    Animator animator;

    PlayerState currentState;

    #endregion

    #region 公有属性

    public PlayerState CurrentState
    {
        get
        {
            return currentState;
        }
    }

    #endregion

    #region 初始化

    void Awake()
    {
        animator = GetComponent<Animator>();
        currentState = PlayerState.Idel;
    }

    #endregion

    #region 私有方法
    void PlayAnimation(string AnimClip)
    {
        if (!animator.GetCurrentAnimatorStateInfo(0).IsName(AnimClip))
        {
            animator.Play(AnimClip);
            
        }
    }
    
    #endregion

    #region 公有方法，外部调用
    public void SetPlayerAnimation(PlayerState state)
    {
        currentState = state;
        switch (state)
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
            case PlayerState.Slide:
                PlayAnimation("Push");
                break;
        }
    }

    public bool CanPlayerChangeRoute()
    {
        if(currentState == PlayerState.Climb)
        {
            return false;
        }
        return true;
    }

    public bool IsPlayerWithBox()
    {
        if(currentState == PlayerState.Push || currentState == PlayerState.Pull)
        {
            return true;
        }
        return false;
    }

    public void ShowPlayerStuckInfo(PlayerStuckInfo stuckInfo)
    {
        //播放摇头动画
        //显示图片
        Debug.Log(stuckInfo.ToString());
    }

    #endregion
}
