using System.Collections.Generic;
using UnityEngine;

public class PlayerController2D : MonoBehaviour {

    #region 非序列化的私有字段

    float speed;

    Transform playerTransform;

    Camera mainCamera;

    private Rectangle playerContour;

    List<Vector2> routePoint;

    int pointIndex;

    bool GetDestination;

    Mask mask;

    Vector2 hitPos;

    bool hitMask = false;

    #endregion

    #region 序列化的私有字段

    [SerializeField]
    float halfWidth, halfHeight;

    [SerializeField, Range(0, 2)]
    float horizontalRayLength, verticalRayLength;

    [SerializeField]
    float moveSpeed;

    [SerializeField]
    float pushSpeed;

    [SerializeField]
    float maxClimbHeight, maxFallHeight;

    #endregion

    #region 公有字段

    [HideInInspector]
    public Box TheBox;

    [HideInInspector]
    public PlayerAction playerAction;

    #endregion

    #region 公有属性

    public Rectangle PlayerContour
    {
        get
        {
            SetPlayerContour();
            return playerContour;
        }
    }

    public Vector2 PlayerPos
    {
        get
        {
            return playerTransform.position;
        }
    }

    #endregion

    

    /*下面是函数*/

    #region 初始化
    void Awake()
    {
        speed = moveSpeed;
        playerTransform = transform;
        playerAction = GetComponent<PlayerAction>();
        routePoint = new List<Vector2>();
        GetDestination = true;
        mainCamera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        mask = GameObject.FindWithTag("Mask").GetComponent<Mask>();
        SetInitialPos();
    }

    private void SetPlayerContour()
    {
        playerContour.minX = playerTransform.position.x - halfWidth;
        playerContour.maxX = playerTransform.position.x + halfWidth;
        playerContour.minY = playerTransform.position.y - halfHeight;
        playerContour.maxY = playerTransform.position.y + halfHeight;
    }

    private void SetInitialPos()
    {
        float x = MathCalulate.GetHalfValue(playerTransform.position.x);
        playerTransform.position = new Vector2(x, playerTransform.position.y);
        SetPlayerContour();

        RaycastHit2D hit = Physics2D.Raycast(playerTransform.position,Vector2.down,20);
        if(hit.collider != null)
        {
            float colliderTopY = hit.collider.bounds.max.y;
            playerTransform.position += Vector3.up * (colliderTopY - playerContour.minY);           
        }
        float y = MathCalulate.GetHalfValue(playerTransform.position.y);
        playerTransform.position = new Vector2(playerTransform.position.x, y);
    }

    #endregion

    #region 点击事件寻路逻辑

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            if (!AbleToMove())
            {
                return;
            }
            OnMouseDownEvent();
        }

        //松开鼠标并且是点了底片的情况
        if (Input.GetMouseButtonUp(0) && hitMask)
        {
            OnMouseUpEvent();
        }

        //如果未到达终点
        if (!GetDestination)
        {
            MoveToRoutePoint();
        }
    }

    bool AbleToMove()
    {
        //若是在滑动，则所有操作无效，优先级最高
        if (playerAction.CurrentState == PlayerState.Slide)
        {
            SlideToTargrt();
            return false;
        }
        //如果主角不能改变寻路
        if (!playerAction.CanPlayerChangeRoute())
        {
            return false;
        }

        //作用是，点击空白地方，结束推箱子移动效果。开始向前滑动。
        if (playerAction.IsPlayerWithBox())
        {
            BeginSlide();
            return false;
        }
        return true;
    }

    void OnMouseDownEvent()
    {
        hitPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);

        //如果没点中底片直接寻路开始
        if (!mask.IsInRectangle(hitPos))
        {
            BeginNavigation();
        }
        //点中了底片
        else
        {
            hitMask = true;
        }
    }

    void OnMouseUpEvent()
    {
        //如果没拖动底片才寻路
        if (!mask.hasDrag && mask.GetDragOffoset() == Vector2.zero)
        {
            BeginNavigation();
        }
        else
        {
            //否则松手时恢复底片的hasDrag为false
            mask.RevertDragMark();
        }
        hitMask = false;
    }

    void BeginNavigation()
    {
        GetDestination = false;
        pointIndex = 0;
        CalculateRoute();       
    }

    #endregion

    #region 寻路算法
    void CalculateRoute()
    {
        float targetX = MathCalulate.GetHalfValue(hitPos.x);
        routePoint.Clear();
        SetPlayerTowards(targetX);
        //注意改变寻路目的地的第一个currentGetPos不能添加进关键点集合
        Vector2 playerPos = playerTransform.position;
        Vector2 currentGetPos = MathCalulate.GetHalfVector2(playerPos);

        Vector2 rayDirection = GetRayDirection();

        while (currentGetPos.x != targetX)
        {
            //主角遇到了底片顶点
            if (mask.IfVertexBlockInLand(currentGetPos + rayDirection) || mask.IfPosJustOnBorderTop(currentGetPos + rayDirection)) 
            {
                break;
            }
            RaycastHit2D climbHit = RayToForward(currentGetPos);
            //前方无障碍
            if (RayToForward(currentGetPos).collider == null)
            {
                //前方有坑
                if (RayFromForwardToDown(currentGetPos).collider == null)
                {
                    //跳不下去
                    if (RayToCheckFall(currentGetPos).collider == null)
                    {
                        break;
                    }
                    //能跳下去
                    else
                    {
                        Vector2 judgePoint = currentGetPos + rayDirection - Vector2.up;
                        if (mask.IfPointAtBorderY(judgePoint, judgePoint - rayDirection) || mask.IfPosJustOnBorderTop(judgePoint))
                        {
                            break;
                        }
                        routePoint.Add(currentGetPos);
                        //这个1是移动一个格子
                        currentGetPos += new Vector2(rayDirection.x * 1, -1);
                        routePoint.Add(currentGetPos);
                    }
                }
                //前方无坑可以直接走
                else
                {
                    currentGetPos = currentGetPos + rayDirection * 1f;
                }
            }
            //前方有障碍
            else
            {
                //头顶无障碍并且高度合适能爬上去
                if (RayToUp(currentGetPos).collider == null && RayToCheckClimb(currentGetPos).collider == null)
                {                  
                    if (mask.IfPointAtBorderY(currentGetPos + Vector2.up,currentGetPos + Vector2.up + rayDirection))
                    {
                        break;
                    }
                    routePoint.Add(currentGetPos);
                    currentGetPos += Vector2.up + rayDirection;
                    routePoint.Add(currentGetPos);
                }
                //无法爬上去
                else
                {
                    break;
                }
            }
        }

        //结束寻路后判断是否添加最后一个currentGetPos
        int count = routePoint.Count;
        if ((count == 0 && playerPos != currentGetPos) || (count > 0 && routePoint[count - 1] != currentGetPos))
        {
            routePoint.Add(currentGetPos);
        }
        //开始改变主角动画行为
        if(routePoint.Count != 0)
        {
            UpdatePlayerAnimation(0);
        }

    }

    public void CalculateWithBox(Direction direct)
    {
        float targetX = hitPos.x;
        GetDestination = false;
        TheBox.SetColliderActive(false);
        routePoint.Clear();
        pointIndex = 0;

        Vector2 playerPos = playerTransform.position;
        Vector2 currentGetPos = MathCalulate.GetHalfVector2(playerPos);
        Vector2 direction = (direct == Direction.right) ? Vector2.right : Vector2.left;

        Vector2 rayDirection = GetRayDirection();

        while (true)
        {

            if (mask.IfVertexBlockInLand(currentGetPos + rayDirection)) //主角前方遇到了底片顶点
            {
                if (TheBox.IsPush)
                    currentGetPos += direction * -1f;

                break;
            }
            if (!RayToForward(currentGetPos))//前方无障碍
            {
                if (!RayFromForwardToDown(currentGetPos))//前方有坑
                    break;
                else
                    currentGetPos = currentGetPos + rayDirection * 1f;
            }
            else//前方有障碍（墙体或者半墙）
            {
                if (TheBox.IsPush)
                    currentGetPos += direction * -1f;
                break;
            }
        }

        if (routePoint.Count == 0 || routePoint[routePoint.Count - 1] != currentGetPos)
        {
            routePoint.Add(currentGetPos);//添加终点
        }

        TheBox.SetColliderActive(true);
    }
    #endregion

    #region 射线相关函数

    Vector2 GetRayDirection()
    {
        if (playerTransform.localScale.x < 0)//主角向左
        {
            return Vector2.left;
        }
        else
        {
            return Vector2.right;
        }
    }

    RaycastHit2D RayToForward(Vector2 currentGetPos)
    {
        return Physics2D.Raycast(currentGetPos, GetRayDirection(), horizontalRayLength);
    }

    RaycastHit2D RayToUp(Vector2 currentGetPos)
    {
        return Physics2D.Raycast(currentGetPos, Vector2.up, verticalRayLength);
    }

    RaycastHit2D RayFromForwardToDown(Vector2 currentGetPos)
    {
        return Physics2D.Raycast(currentGetPos + GetRayDirection() * horizontalRayLength, Vector2.down, verticalRayLength);
    }

    RaycastHit2D RayToCheckClimb(Vector2 currentGetPos)
    {
        return Physics2D.Raycast(currentGetPos + maxClimbHeight * Vector2.up, GetRayDirection(), horizontalRayLength);
    }

    RaycastHit2D RayToCheckFall(Vector2 currentGetPos)
    {
        return Physics2D.Raycast(currentGetPos + GetRayDirection() * horizontalRayLength + Vector2.down * verticalRayLength,
            Vector2.down, maxFallHeight); 
    }

    #endregion

    #region 移动逻辑
    void MoveToRoutePoint()
    {
        if(routePoint.Count == 0)
        {
            GetDestination = true;
            return;
        }

        Vector2 currentPos = playerTransform.position;     

        if (currentPos != routePoint[pointIndex])
        {
            playerTransform.position = Vector2.MoveTowards(currentPos, routePoint[pointIndex], Time.deltaTime * speed);
            
        }
        //达到了关键点
        else
        {
            //如果达到终点
            if ((Vector2)playerTransform.position == routePoint[routePoint.Count - 1])
            {
                EndMove();
                return;
            }
            UpdatePlayerAnimation(++pointIndex);
        }
    }

    void EndMove()
    {
         if (playerTransform.childCount != 0)
         {
            TheBox.boxUI.EndMove();
         }

        playerAction.SetPlayerAnimation(PlayerState.Idel);
        GetDestination = true;
        CheckPassLevel();
    }
    #endregion

    #region 主角自动滑动相关
    void BeginSlide()
    {
        Vector2 playerCenter = MathCalulate.GetHalfVector2(playerTransform.position); 
        //避免滑到到墙里面去       
        if (playerCenter != routePoint[routePoint.Count - 1])
        {
            float lerp = 0.5f * playerTransform.localScale.x;
            playerCenter = MathCalulate.GetHalfVector2(new Vector2(playerTransform.position.x + lerp, playerTransform.position.y));
        }
        playerAction.SetPlayerAnimation(PlayerState.Slide);
        routePoint.Clear();
        pointIndex = 0;
        routePoint.Add(playerCenter);
    }

    void SlideToTargrt()
    {
        Vector2 currentPos = playerTransform.position;
        if (currentPos != routePoint[0])
        {
            playerTransform.position = Vector2.MoveTowards(currentPos, routePoint[0], Time.deltaTime * pushSpeed);
        }
        else
        {
            playerAction.SetPlayerAnimation(PlayerState.Idel);           
            TheBox.boxUI.EndMove();
            CheckPassLevel();
        }
    }
    #endregion

    #region 改变主角的一些状态
    public void SetPlayerTowards(float destinationX)
    {
        float offoset = destinationX - playerTransform.position.x;
        float scale = playerTransform.localScale.x;
        if (offoset * scale < 0)//换方向
        {
            playerTransform.localScale = new Vector2(-scale, playerTransform.localScale.y);
        }
    }

    public void ChangeSpeed(PlayerState state)
    {
        if (state == PlayerState.Run)
            speed = moveSpeed;
        else
            speed = pushSpeed;
    }

    //通过下一个关键点来判断主角的动画
    void UpdatePlayerAnimation(int nextPointIndex)
    {
        float currentPosY = playerTransform.position.y;

        float nextPointY = routePoint[nextPointIndex].y;

        if (currentPosY == nextPointY)
        {
            playerAction.SetPlayerAnimation(PlayerState.Run);
        }
        else if (currentPosY > nextPointY)
        {
            playerAction.SetPlayerAnimation(PlayerState.Climb);
        }
        else if (currentPosY < nextPointY)
        {
            playerAction.SetPlayerAnimation(PlayerState.Climb);
        }
    }
    #endregion

    void CheckPassLevel()
    {
        LevelManager.Instance.CanPassLevel();
    }
}



