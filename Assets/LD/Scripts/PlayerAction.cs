using UnityEngine;
using DragonBones;

public enum PlayerState
{
    Idel = 0,
    Run,
    Climb,
    Fall,
    Push,
    Pull,
    Slide,
    Stuck
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
    void PlayAnimation(string AnimClip,float animaSpeed,float fadeSpeed = 0,int playTimes = -1)
    {
        armture.animation.FadeIn(AnimClip, fadeSpeed, playTimes);
        armture.animation.timeScale = animaSpeed;
    }

    void PlayRunAnimation()
    {
        string lastAnimationName = armture.animation.lastAnimationName;
        if (lastAnimationName != "walk")
        {
            float fadeSpeed = 0;
            if(lastAnimationName == "climb")
            {
                fadeSpeed = 1;
            }
            else if (lastAnimationName == "jump")
            {
                fadeSpeed = 0.5f;
            }
            armture.animation.FadeIn("walk", fadeSpeed);
            armture.animation.timeScale = 3.5f;
        }
        
    }


    #endregion

    #region 公有方法，外部调用
    public void SetPlayerAnimation(PlayerState state)
    {
        CurrentState = state;
        JustSetAnimation(state);
    }

    public void JustSetAnimation(PlayerState state)
    {
        Debug.Log(armture.animation.lastAnimationName);
        switch (state)
        {
            case PlayerState.Idel:
                PlayAnimation("breath", 0.6f, 0.1f);
                break;
            case PlayerState.Run:
                PlayRunAnimation();
                break;
            case PlayerState.Climb:
                PlayAnimation("climb", 1.5f, 0.0f, 1);
                break;
            case PlayerState.Fall:
                PlayAnimation("jump", 4.2f, 0, 1);
                break;
            case PlayerState.Push:
                PlayAnimation("pushbox", 2.8f,0.8f);
                break;
            //暂无拉箱子动画，以推箱子动画代替
            case PlayerState.Pull:
                PlayAnimation("towingbox", 2.8f);
                break;
            case PlayerState.Slide:
                PlayAnimation("walk", 3.5f, 0);
                break;
            case PlayerState.Stuck:
                PlayAnimation("on", 1, 0,1);
                break;
        }
    }

    public bool CanPlayerChangeRoute()
    {
        if(CurrentState == PlayerState.Climb || CurrentState == PlayerState.Fall)
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
        SetPlayerAnimation(PlayerState.Stuck);
        //显示图片
        Debug.Log(stuckInfo.ToString());
    }

    #endregion
}
