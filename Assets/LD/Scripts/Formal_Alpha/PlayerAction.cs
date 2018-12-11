using UnityEngine;
using DragonBones;

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
    BoxToBorderTop,
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

    UnityArmatureComponent armture;

    #endregion

    #region 自动属性

    public PlayerState CurrentState { get; private set; }

    #endregion

    #region 初始化

    void Awake()
    {
        armture = GetComponent<UnityArmatureComponent>();
        CurrentState = PlayerState.Idel;
    }

    #endregion

    #region 私有方法
    void PlayAnimation(string AnimClip,float animaSpeed)
    {
        if (armture.animation.lastAnimationName != AnimClip)
        {
            if(AnimClip == "呼吸")
            {
                armture.animation.FadeIn(AnimClip,0.1f);
            }
            else
            {
                armture.animation.Play(AnimClip);
            }            
            armture.animation.timeScale = animaSpeed;
            //Debug.Log(AnimClip);
        }
    }
    
    #endregion

    #region 公有方法，外部调用
    public void SetPlayerAnimation(PlayerState state)
    {
        CurrentState = state;
        switch (state)
        {
            case PlayerState.Idel:
                PlayAnimation("呼吸",0.6f);
                break;
            case PlayerState.Run:
                PlayAnimation("走",3.5f);
                break;
            case PlayerState.Climb:
                PlayAnimation("走",3.5f);
                break;
            case PlayerState.Push:
                PlayAnimation("走", 3.5f);
                break;
            //暂无拉箱子动画，以推箱子动画代替
            case PlayerState.Pull:
                PlayAnimation("走", 3.5f);
                break;
            case PlayerState.Slide:
                PlayAnimation("走",3.5f);
                break;
        }
    }

    public bool CanPlayerChangeRoute()
    {
        if(CurrentState == PlayerState.Climb)
        {
            return false;
        }
        return true;
    }

    public bool IsPlayerWithBox()
    {
        if(CurrentState == PlayerState.Push || CurrentState == PlayerState.Pull ||CurrentState == PlayerState.Slide)
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
