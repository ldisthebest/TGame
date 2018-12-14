using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController2D : MonoBehaviour {



    #region 事件委托
    public delegate void ShowBoxUiHandler(Vector2 playerPos);
    public event ShowBoxUiHandler ShowUiEvent;

    public delegate void HideBoxUiHandler();
    public event HideBoxUiHandler HideUiEvent;

    public delegate void PassLevelHander(Vector2 playerPos);
    public event PassLevelHander PassLevelEvent;

    public delegate void UpdateMoveHandler(Vector2 playerPos);
    public event UpdateMoveHandler OnMoveEvent;

    public delegate void EndMoveHandler(Vector2 playerPos);
    public event EndMoveHandler EndMoveEvent;

    #endregion

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

    Vector2 rayDirection;

    #endregion

    #region 序列化的私有字段

    [SerializeField]
    float rightBorder, leftBorder ,topBorder, bottomBorder;

    [SerializeField, Range(0, 2)]
    float horizontalRayLength, verticalRayLength;

    [SerializeField]
    float moveSpeed;

    [SerializeField]
    float pushSpeed;

    [SerializeField]
    float maxClimbHeight, maxFallHeight;

    [SerializeField]
    Vector2[] climbPointCurve;

    [SerializeField]
    Vector2[] fallCurve;    

    #endregion

    #region 公有字段

    [HideInInspector]
    public Box TheBox;

    [HideInInspector]
    public PlayerAction playerAction;

    [HideInInspector]
    public bool canPlayerControl;

    [HideInInspector]
    public bool playerPause;

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

    public Vector2 PlayerCenter
    {
        get
        {
            Rectangle playerRect = PlayerContour;
            return new Vector2((playerRect.minX+playerRect.maxX)/2,(playerRect.minY + playerRect.maxY)/2);
        }
    }

    #endregion

    bool isPlayPush = false;
    

    #region 初始化
    void Awake()
    {
        canPlayerControl = true;
        speed = moveSpeed;
        rayDirection = Vector2.right;
        playerTransform = transform;
        playerAction = GetComponent<PlayerAction>();
        routePoint = new List<Vector2>();
        GetDestination = true;
        mainCamera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        mask = GameObject.FindWithTag("Mask").GetComponent<Mask>();
        
    }

    private void SetPlayerContour()
    {
        playerContour.minX = playerTransform.position.x + leftBorder;
        playerContour.maxX = playerTransform.position.x + rightBorder;
        playerContour.minY = playerTransform.position.y + bottomBorder;
        playerContour.maxY = playerTransform.position.y + topBorder;
    }

    public void SetInitialPos(Vector2 startPos)
    { 
        playerTransform.position = startPos;
        SetPlayerContour();
        float x = MathCalulate.GetHalfValue(playerTransform.position.x);
        RaycastHit2D hit = Physics2D.Raycast(playerTransform.position,Vector2.down,20);
        if(hit.collider != null)
        {
            float colliderTopY = hit.collider.bounds.max.y;
            playerTransform.position = new Vector2(x, GetPlayerPosY(hit.collider));
        }
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

#if UNITY_EDITOR

        SetPlayerContour();
        MathCalulate.Drawline(playerContour);

#endif

    }

    bool AbleToMove()
    {
        //若是在滑动，则所有操作无效，优先级最高
        if (playerAction.CurrentState == PlayerState.Slide)
        {
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
        if(!canPlayerControl)
        {
            return;
        }
        GetDestination = false;
        pointIndex = 0;
        CalculateRoute();       
    }


    #endregion

    #region 寻路算法

    void AddRoutePoint(Vector2 currentPos)
    {
        //这种添加方式可能涉及到重复添加，所以加个判定
        Vector2 point = new Vector2(currentPos.x, GetPlayerPosY(currentPos));
        //不重复天添加
        if(routePoint.Count > 0 && point == routePoint[routePoint.Count - 1])
        {
            return;
        }
        //不添加起点
        if(MathCalulate.AlmostEqual(point,playerTransform.position))
        {
            return;
        }
        routePoint.Add(new Vector2(currentPos.x, GetPlayerPosY(currentPos)));
    }

    void AddRoutePoint(Vector2 currentPos, Collider2D collider)
    {
        routePoint.Add(new Vector2(currentPos.x, GetPlayerPosY(collider)));
    }

    void CalculateRoute()
    {
        float targetX = MathCalulate.GetHalfValue(hitPos.x);
        routePoint.Clear();
        //注意改变寻路目的地的第一个currentGetPos不能添加进关键点集合
        Vector2 currentGetPos = MathCalulate.GetHalfVector2(PlayerCenter);

        SetRayDirection();
        PlayerStuckInfo stuck = PlayerStuckInfo.Unknow;

        while (currentGetPos.x != targetX)
        {
            //主角遇到了底片顶点
            if (mask.IfVertexBlockInLand(currentGetPos + rayDirection) && !ClimbCollider(currentGetPos))
            {
                stuck = PlayerStuckInfo.PlayerStuckedByMaskVertex;
                break;
            }
            if (mask.IfPosJustOnBorderTop(currentGetPos))
            {
                stuck = PlayerStuckInfo.MoveToBorderTop;
                currentGetPos -= rayDirection;
                break;
                //ShowStuckInfo(PlayerStuckInfo.OnMaskBorderTop);
                //return;
            }
            Collider2D forwardCollider = ClimbCollider(currentGetPos);
            //前方无障碍
            if (forwardCollider == null)
            {
                //前方有坑
                if (ExistPit(currentGetPos))
                {
                    Collider2D pitCollider = FallCollider(currentGetPos);
                    //跳不下去
                    if (pitCollider == null)
                    {
                        stuck = PlayerStuckInfo.UnbaleToFall;
                        break;
                    }
                    //能跳下去
                    else
                    {
                        Vector2 judgePoint = currentGetPos + rayDirection - Vector2.up;
                        if (mask.IfPointAtBorderY(judgePoint/*, judgePoint - rayDirection*/))
                        {
                            stuck = PlayerStuckInfo.OnMaskInsideY;
                            break;
                        }
                        if(mask.IfPosJustOnBorderTop(judgePoint))
                        {
                            stuck = PlayerStuckInfo.FallToBorderTop;
                            break;
                        }
                        AddRoutePoint(currentGetPos);
                        //这个1是移动一个格子
                        currentGetPos += new Vector2(rayDirection.x * 1, -1);
                        AddRoutePoint(currentGetPos, pitCollider);
   
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
                if (NoneUpBarrier(currentGetPos) && CanClimb(currentGetPos))
                {                  
                    if (mask.IfPointAtBorderY(currentGetPos + Vector2.up, currentGetPos + Vector2.up + rayDirection))
                    {
                        stuck = PlayerStuckInfo.OnMaskInsideY;
                        break;
                    }
                    if (mask.IfPosJustOnBorderTop(currentGetPos + Vector2.up + rayDirection))
                    {
                        stuck = PlayerStuckInfo.OnMaskInsideY;
                        break;
                    }
                     AddRoutePoint(currentGetPos);
                    currentGetPos += Vector2.up + rayDirection;
                    AddRoutePoint(currentGetPos, forwardCollider);
                }
                //无法爬上去
                else
                {
                    stuck = PlayerStuckInfo.UnableToClimb;
                    break;
                }
            }
        }

        //结束寻路后可能需要添加最后一个currentGetPos
        AddRoutePoint(currentGetPos);  

        //如果能走开始改变主角动画行为
        if (routePoint.Count != 0)
        {
            UpdatePlayerAnimation(0);
            SetPlayerTowards();
            //关闭箱子的UI
            if(HideUiEvent != null)
            {
                HideUiEvent();
            }         
        }
        //否则显示不能走的原因
        else
        {
            ShowStuckInfo(stuck);
        }
        Debug.Log(routePoint.Count);
    }

    //false表示不需要向前移动，true表示需要向前移动
    public bool CalculateWithBox(Direction direct,Vector2 boxPos)
    {
        float targetX = hitPos.x;
        GetDestination = false;
        TheBox.SetColliderActive(false);
        routePoint.Clear();
        pointIndex = 0;

        Vector2 currentGetPos = MathCalulate.GetHalfVector2(PlayerCenter);
        Vector2 direction = (direct == Direction.right) ? Vector2.right : Vector2.left;

        rayDirection = direction;
        PlayerStuckInfo stuck = PlayerStuckInfo.Unknow;

        while (true)
        {
            if(mask.IfPosJustOnBorderTop(boxPos))
            {
                stuck = PlayerStuckInfo.BoxToBorderTop;
                break;
            }

            if (mask.IfVertexBlockInLand(currentGetPos + direction)) //主角前方遇到了底片顶点
            {
                if (TheBox.IsPush)
                {
                    currentGetPos += direction * -1f;
                    stuck = PlayerStuckInfo.PushStuckedByMaskVertex;
                }
                else
                {
                    stuck = PlayerStuckInfo.PullStuckedByMaskVertex;
                }
                break;
            }
            if (!ClimbCollider(currentGetPos))//前方无障碍
            {
                //判断主角否面临会推或拉到边框顶部
                if (mask.IfPosJustOnBorderTop(currentGetPos + rayDirection))
                {
                    if (TheBox.IsPush)
                    {
                        currentGetPos -= rayDirection;
                    }
                    stuck = PlayerStuckInfo.BoxToBorderTop;
                    break;
                }
                if (ExistPit(currentGetPos))//前方有坑
                {
                    if (TheBox.IsPush)
                    {
                        if (TooDeepToFallBox(currentGetPos))
                        {
                            currentGetPos -= direction;
                            stuck = PlayerStuckInfo.UnablePushToPit;
                            break;
                        }
                        //判断箱子是否会推到底片顶部
                        else if (mask.IfPosJustOnBorderTop(currentGetPos + rayDirection,currentGetPos + rayDirection - Vector2.up, currentGetPos + rayDirection - Vector2.up * 2))
                        {
                            stuck = PlayerStuckInfo.BoxToBorderTop;
                            currentGetPos -= direction;
                            break;
                        }
                    }
                    else
                    {

                        stuck = PlayerStuckInfo.UnablePullToPit;
                    }
                    break;
                   
                }
                else//前方无坑
                {
                    currentGetPos += rayDirection;
                }
                   
            }
            else//前方有障碍（墙体或者半墙）
            {
                if (TheBox.IsPush)
                {
                    currentGetPos += direction * -1f;
                    stuck = PlayerStuckInfo.UnablePushToWall;
                }
                else
                {
                    stuck = PlayerStuckInfo.UnablePullToWall;
                }                   
                break;
            }
        }

        AddRoutePoint(currentGetPos);

        TheBox.SetColliderActive(true);

        if(routePoint.Count == 0)
        {
            ShowStuckInfo(stuck);
            return false;
        }
        else
        {
            SetPlayerTowards(TheBox.IsPush);
            ChangeSpeed(PlayerState.Push);
            if (TheBox.IsPush)
                playerAction.SetPlayerAnimation(PlayerState.Push);
            else
                playerAction.SetPlayerAnimation(PlayerState.Pull);
        }

        return true;
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

    //新添加的
    Collider2D ClimbCollider(Vector2 currentGetPos)
    {
        return Physics2D.Raycast(currentGetPos, rayDirection, horizontalRayLength).collider;      
    }

    Collider2D FallCollider(Vector2 currentGetPos)
    {
        return Physics2D.Raycast(currentGetPos + rayDirection * horizontalRayLength + Vector2.down * verticalRayLength,
          Vector2.down, maxFallHeight).collider;
    }

    bool ExistPit(Vector2 currentGetPos)
    {
        return Physics2D.Raycast(currentGetPos + rayDirection * horizontalRayLength, Vector2.down, verticalRayLength).collider == null;
    }

    bool NoneUpBarrier(Vector2 currentGetPos)
    {
        return Physics2D.Raycast(currentGetPos, Vector2.up, verticalRayLength).collider == null;
    }

    bool CanClimb(Vector2 currentGetPos)
    {
        return Physics2D.Raycast(currentGetPos + maxClimbHeight * Vector2.up, rayDirection, horizontalRayLength).collider == null;
    }

    bool TooDeepToFallBox(Vector2 currentGetPos)
    {
        return Physics2D.Raycast(currentGetPos + rayDirection - Vector2.up*2, Vector2.down, verticalRayLength).collider == null;
    }


    #endregion

    #region 移动逻辑
    void MoveToRoutePoint()
    {
        if(routePoint.Count == 0 || playerPause || pointIndex == routePoint.Count)
        {
            GetDestination = true;
            return;
        }

        Vector2 currentPos = playerTransform.position;     

        if (currentPos != routePoint[pointIndex])
        {
            playerTransform.position = Vector2.MoveTowards(currentPos, routePoint[pointIndex], Time.deltaTime * speed);
            if(playerAction.IsPlayerWithBox() && !isPlayPush)
            {
                
                //循环播放
                AudioController.Instance.PlayMusic("Audio/push",3);
                isPlayPush = true;
            }
            if(OnMoveEvent != null)
            {
                OnMoveEvent(playerTransform.position);
            }
            
        }
        //达到了关键点
        else
        {
            //如果达到终点
            if ((Vector2)playerTransform.position == routePoint[routePoint.Count - 1])
            {
                isPlayPush = false;
                EndMove();
                return;
            }
            UpdatePlayerAnimation(++pointIndex);
        }
    }

    void EndMove()
    {
        if (playerAction.IsPlayerWithBox())
        {
           TheBox.EndMove();
           ChangeSpeed(PlayerState.Run);
        }
        playerAction.SetPlayerAnimation(PlayerState.Idel);
        GetDestination = true;
        if(ShowUiEvent != null)
        {
            ShowUiEvent(PlayerCenter);
        }      
        if(EndMoveEvent != null)
        {
            EndMoveEvent(playerTransform.position);
        }
        if(!canPlayerControl)
        {
            canPlayerControl = true;
        }
        CheckPassLevel();
    }
    #endregion

    #region 主角自动滑动相关
    void BeginSlide()
    {
        float playerX = MathCalulate.GetHalfValue(playerTransform.position.x);
        Vector2 playerCenter = new Vector2(MathCalulate.GetHalfValue(playerTransform.position.x), playerTransform.position.y); 
        //避免滑到到墙里面去       
        if (playerCenter != routePoint[routePoint.Count - 1])
        {
            float slideX = MathCalulate.GetHalfValue(playerTransform.position.x + 0.5f * rayDirection.x);
            playerCenter = new Vector2(slideX, playerTransform.position.y);
        }

        playerAction.SetPlayerAnimation(PlayerState.Slide);
        routePoint.Clear();
        pointIndex = 0;
        routePoint.Add(playerCenter);
    }

    #endregion

    #region 改变或者获得主角的一些属性

    void SetPlayerTowards()
    {
        float scale = playerTransform.localScale.x;
        if (rayDirection.x * scale < 0)//换方向
        {
            playerTransform.localScale = new Vector2(-scale, playerTransform.localScale.y);
        }
    }

    void SetPlayerTowards(bool isPush)
    {
        float scale = Mathf.Abs(playerTransform.localScale.x);

        if(isPush)
        {
            playerTransform.localScale = rayDirection == Vector2.right? new Vector2(scale, playerTransform.localScale.y) : new Vector2(-scale, playerTransform.localScale.y);
        }
        else
        {
            playerTransform.localScale = rayDirection == Vector2.left ? new Vector2(scale, playerTransform.localScale.y) : new Vector2(-scale, playerTransform.localScale.y);
        }
    }

    void SetRayDirection()
    {
        if(hitPos.x >= playerTransform.position.x)
        {
            rayDirection =  Vector2.right;
        }
        else
        {
            rayDirection = Vector2.left;
        }
    }

    float GetPlayerPosY(Collider2D collider)
    {
        float colliderTopY = collider.bounds.max.y;
        return playerTransform.position.y + colliderTopY - playerContour.minY;
    }

    float GetPlayerPosY(Vector2 currentPos)
    {
        return GetPlayerPosY(Physics2D.Raycast(currentPos, Vector2.down,verticalRayLength).collider);
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
        Vector2 currentPos = playerTransform.position;

        Vector2 nextPoint = routePoint[nextPointIndex];

        if(currentPos.x == nextPoint.x)
        {
            if(nextPoint.y > currentPos.y)
            {
                //上爬
            }
            else
            {
                //下爬
            }
        }
        else
        {
            float currentPosY = currentPos.y;
            float nextPointY = nextPoint.y;
            if (MathCalulate.AlmostEqual(currentPosY, nextPointY))
            {
                playerAction.SetPlayerAnimation(PlayerState.Run);
            }
            else if (currentPosY > nextPointY)
            {
                StartCoroutine(Fall());
            }
            else if (currentPosY < nextPointY)
            {
                playerAction.SetPlayerAnimation(PlayerState.Climb);
                StartCoroutine(Climb());
            }
        }    
    }

    public void ShowStuckInfo(PlayerStuckInfo stuck)
    {
        playerAction.ShowPlayerStuckInfo(stuck);
    }
    #endregion

    IEnumerator Climb()
    {
        playerPause = true;
        int index = 0;
        Vector2[] targetPos = new Vector2[climbPointCurve.Length];
        float x = rayDirection.x;
        for(int i = 0;i<targetPos.Length;i++)
        {
            targetPos[i] = (Vector2)playerTransform.position + new Vector2(climbPointCurve[i].x*x, climbPointCurve[i].y);
        }
        float climbSpeed = 1.5f;
        while ((Vector2)playerTransform.position != targetPos[climbPointCurve.Length - 1])
        {
            playerTransform.position = Vector2.MoveTowards(playerTransform.position, targetPos[index],climbSpeed * Time.deltaTime);
            if((Vector2)playerTransform.position == targetPos[index])
            {
                climbSpeed = 1.2f;
                if(index == 0)
                {
                    yield return new WaitForSeconds(0.25f);
                }
                else if(index == 1)
                {
                    playerAction.JustSetAnimation(PlayerState.Run);
                }
                index++;
            }
            yield return new WaitForSeconds(0.01f);
        }
        playerTransform.position = routePoint[pointIndex];
        //playerAction.JustSetAnimation(PlayerState.Idel);
        //pointIndex++;
        playerPause = false;
        GetDestination = false;
    }

    IEnumerator Fall()
    {
        playerPause = true;
        int index = 0;
        Vector2[] targetPos = new Vector2[fallCurve.Length];
        float x = rayDirection.x;
        for (int i = 0; i < targetPos.Length; i++)
        {
            targetPos[i] = (Vector2)playerTransform.position + new Vector2(fallCurve[i].x * x, fallCurve[i].y);
        }
        playerAction.JustSetAnimation(PlayerState.Run);
        float fallSpeed = moveSpeed;
        while ((Vector2)playerTransform.position != targetPos[fallCurve.Length - 1])
        {
            playerTransform.position = Vector2.MoveTowards(playerTransform.position, targetPos[index], fallSpeed * Time.deltaTime);
            if((Vector2)playerTransform.position == targetPos[0])
            {
                index++;
                playerAction.SetPlayerAnimation(PlayerState.Fall);
            }
            yield return new WaitForSeconds(0.01f);
        }
        playerTransform.position = routePoint[pointIndex];
        //playerAction.JustSetAnimation(PlayerState.Idel);
        //pointIndex++;
        playerPause = false;
        GetDestination = false;
    }


    #region 主角过关相关

    void CheckPassLevel()
    {
        if(PassLevelEvent != null)
        {
            PassLevelEvent(playerTransform.position);
        }       
    }

    public void AutoMove(Vector2 targetPos)
    {
        if (targetPos == (Vector2)playerTransform.position)
        {
            return;
        }
        GetDestination = false;
        routePoint.Add(targetPos);
        UpdatePlayerAnimation(0);
        
    }

    public void StopMove()
    {
        playerPause = true;
        playerAction.SetPlayerAnimation(PlayerState.Idel);
    }

    #endregion
}



