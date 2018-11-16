using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//public struct BoxContour
//{
//    public Vector2 TopLeft;
//    public Vector2 TopRigtht;
//    public Vector2 BottomLeft;
//    public Vector2 BottomRight;
//}
public enum LandBox
{
    none = 0,
    rigthHalf = 1,
    leftHalf = 2,
    whole = 3
} 

public class PlayerController2D : MonoBehaviour {
   

    Transform playerTransform;
    Camera mainCamera;

    [SerializeField]
    float halfWidth, halfHeight;

    [HideInInspector]
    public Rectangle playerContour;

    [SerializeField,Range(0, 2)]
    float horizontalRayLength,verticalRayLength;

    [SerializeField]
    float moveSpeed;

    //------------------------------------- add by lld -----------------------------------------
    [SerializeField]
    float pushSpeed;

    [SerializeField]
    float maxClimbHeight,maxFallHeight;

   // float targetX;

    List<Vector2> rotePoint;
    int pointIndex;

    bool GetDestination;

    PlayerAction playerAction;
    
    /**************************************add by ld*****************/
    [SerializeField]
    public Mask mask;

    [SerializeField, Range(0, 1)]
    float shortHorizontalRayLength;

    float targetX;

    // bool hitMaskWhenPlayerMove;
    bool hitMask = false;
    /**************************************add by ld*****************/

    void Awake()
    {
        playerTransform = transform;
        playerAction = GetComponent<PlayerAction>();
        rotePoint = new List<Vector2>();
        GetDestination = true;
       // hitMaskWhenPlayerMove = false;
        mainCamera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
       
        SetInitialPos();
    }

    /**************************************add by ld*****************/
    public Rectangle GetPlayerContour()
    {
        playerContour.minX = playerTransform.position.x - halfWidth;
        playerContour.maxX = playerTransform.position.x + halfWidth;
        playerContour.minY = playerTransform.position.y - halfHeight;
        playerContour.maxY = playerTransform.position.y + halfHeight;
        return playerContour;

    }
    /**************************************add by ld*****************/

    void SetInitialPos()
    {
        float x = MathCalulate.GetHalfValue(playerTransform.position.x);
        playerTransform.position = new Vector2(x, playerTransform.position.y);
        GetPlayerContour();/**************************************add by ld*****************/

        RaycastHit2D hit = Physics2D.Raycast(playerTransform.position,Vector2.down,20);
        if(hit.collider != null)
        {
            float colliderTopY = hit.collider.bounds.max.y;
            playerTransform.position += Vector3.up * (colliderTopY - playerContour.minY);           
        }
        float y = MathCalulate.GetHalfValue(playerTransform.position.y);
        playerTransform.position = new Vector2(playerTransform.position.x, y);
    }

    void Update()
    {
        //-------------------------------- this line has been changed ----------------------------------------
        if (Input.GetMouseButtonDown(0) && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            if(playerAction.CurrentState == PlayerState.Climb)
            {
                return;
            }

            Vector2 hitPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            targetX = hitPos.x;
            //Debug.Log(hitPos);

            /**************************************add by ld*****************/

            //targetX = MathCalulate.GetHalfValue(hitPos.x);

            //----------------------------------------- add by lld -----------------------------------------------
            //作用是，点击空白地方，结束推箱子移动效果。卡格子（待优化），结束移动。
            if (playerTransform.childCount != 0)
            {
                Transform box = playerTransform.GetChild(0);
                Vector3 gridCenter = new Vector3(MathCalulate.GetHalfValue(box.position.x), MathCalulate.GetHalfValue(box.position.y), box.position.z);
                Vector3 playerCenter=new Vector3(MathCalulate.GetHalfValue(playerTransform.position.x), MathCalulate.GetHalfValue(playerTransform.position.y), playerTransform.position.z);
                if (!mask.IfPointAtBorderX(gridCenter)&&!mask.IfPointAtBorderX(playerCenter))
                {                 
                    box.GetComponent<Box>().boxUI.EndMove();
                    box.position = gridCenter;
                    playerTransform.position = playerCenter;
                    GetDestination = true;
                }
                return;
            }

            if (!mask.IsInRectangle(hitPos))
            {
                GetDestination = false;
                CaculateRote();
                pointIndex = 0;
            }
            else
            {
                hitMask = true;
            }
            //else if (playerAction.CurrentState != PlayerState.Idel)//如果在走路或跳跃的时候点到了mask就当啥也没发生
            //{
            //    hitMaskWhenPlayerMove = true;
            //}

            /**************************************add by ld*****************/
        }

        /**************************************add by ld*****************/
        if (Input.GetMouseButtonUp(0))
        {
            //if (hitMaskWhenPlayerMove)
            //{
            //    hitMaskWhenPlayerMove = false;
            //    return;
            //}

            if (mask.hasDrag)
            {
                GetDestination = true;
            }
            else if(hitMask)
            {            
                GetDestination = false;
                CaculateRote();
                pointIndex = 0; 
            }
            hitMask = false;
        }
        /**************************************add by ld*****************/

        if (!GetDestination)
        {
            MoveToRotePoint();
        }
        else
        {
            //------------------------------------------- add by lld ----------------------------------------------
            if (playerTransform.childCount != 0)
            {
                playerTransform.GetChild(0).gameObject.GetComponent<Box>().boxUI.EndMove();
            }

            playerAction.SetPlayerAnimation(PlayerState.Idel);
        }


#if UNITY_EDITOR
        if(rotePoint.Count > 0)
        {
            Debug.DrawLine(playerTransform.position, rotePoint[0], Color.red);
            for (int i = 0; i < rotePoint.Count - 1; i++)
            {
                Debug.DrawLine(rotePoint[i], rotePoint[i + 1], Color.red);
            }
        }      
#endif 
    }

    public void SetPlayerTowards(float destinationX)
    {
        float offoset = destinationX - playerTransform.position.x;
        float scale = playerTransform.localScale.x;
        if(offoset * scale < 0)//换方向
        {
            playerTransform.localScale = new Vector2(-scale, playerTransform.localScale.y);
        }
    }

    void CaculateRote()
    {
        rotePoint.Clear();
        UpdateTargetX(ref targetX);
        SetPlayerTowards(targetX);
        Vector2 playerPos = playerTransform.position;
        Vector2 currentGetPos = new Vector2(MathCalulate.GetHalfValue(playerPos.x), playerPos.y);

        while(currentGetPos.x != targetX)
        {
            if(IfMaskVertexBlockInLand(currentGetPos + GetRayDirection()) || mask.IfPosJustOnBorderTop(currentGetPos + GetRayDirection())) //主角遇到了底片顶点
            {
                //currentGetPos -= GetRayDirection();
                break;
            }
            RaycastHit2D climbHit = RayToForward(currentGetPos);
            if (climbHit.collider == null)//前方无障碍
            {
                if(RayFromForwardToDown(currentGetPos).collider == null)//前方有坑
                {
                    //rotePoint.Add(currentGetPos);
                    RaycastHit2D fallHit = RayToCheckFall(currentGetPos);
                    if (fallHit.collider == null)//跳不下去
                    {
                        break;
                    }
                    else//能跳下去
                    {
                        rotePoint.Add(currentGetPos);

                        currentGetPos += new Vector2(GetRayDirection().x*1, -1);//这个1是移动一个格子
                        /*************************add by ld**********************/
                        if (mask.IfPointAtBorderY(currentGetPos) || mask.IfPosJustOnBorderTop(currentGetPos) ||mask.IfPointAtBorderY(currentGetPos - GetRayDirection()))
                        {
                            currentGetPos -= new Vector2(GetRayDirection().x * 1, -1);
                            break;
                        }
                        /*************************add by ld**********************/
                        rotePoint.Add(currentGetPos);
                    }
                }
                else//前方无坑可以直接走
                {
                    currentGetPos = currentGetPos + GetRayDirection() * 1f;
                }
            }
            else//前方有障碍
            {
                //rotePoint.Add(currentGetPos);
                if (RayToUp(currentGetPos).collider == null)//头顶无障碍
                {
                    if(RayToCheckClimb(currentGetPos).collider == null)//能爬上去
                    {
                        rotePoint.Add(currentGetPos);
                        //float colliderTopY = climbHit.collider.bounds.max.y;
                        //currentGetPos += Vector2.up;
                        /*************************add by ld**********************/
                        if(mask.IfPointAtBorderY(MathCalulate.GetHalfVector2(currentGetPos + Vector2.up)))
                        {
                            break;
                        }
                        /*************************add by ld**********************/
                        currentGetPos += Vector2.up + GetRayDirection();
                        rotePoint.Add(currentGetPos);
                    }
                    else//不能爬上去
                    {
                        break;
                    }
                }
                else//头顶有障碍
                {
                    break;
                }
            }
        }
        if(rotePoint.Count == 0 || rotePoint[rotePoint.Count-1] != currentGetPos)
        {
            rotePoint.Add(currentGetPos);//添加终点
        }

        //更新卡住了的点TODO,可能会导致list长度为0
        Vector2 lastPos = rotePoint[rotePoint.Count - 1];
        if (mask.IfPointAtBorderX(lastPos))
        {
            rotePoint.RemoveAt(rotePoint.Count - 1);
            if(rotePoint.Count == 0)
            {
                rotePoint.Add(lastPos - GetRayDirection());
                return;
            }
            Vector2 lastSecondPos = rotePoint[rotePoint.Count - 1];
            if(lastPos.x == lastSecondPos.x || lastPos.y == lastSecondPos.y)//非跳的情况要加回去
            {
                rotePoint.Add(lastPos - GetRayDirection());
            }
        }


    }

    Vector2 GetRayDirection()
    {
        Vector2 rayDirection = Vector2.right;
        if (playerTransform.localScale.x < 0)//主角向左
        {
            rayDirection *= -1;
        }
        return rayDirection;
    }

    RaycastHit2D RayToForward(Vector2 currentGetPos)
    {
        return Physics2D.Raycast(currentGetPos + GetRayDirection()*0.1f, GetRayDirection(), horizontalRayLength);
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

    void MoveToRotePoint()
    {
        if(rotePoint.Count == 0)
        {
            GetDestination = true;
            return;
        }

        //-------------------------------------------- changed by lld ---------------------------------------------
        float speed = moveSpeed;
        Vector2 currentPos = playerTransform.position;

        if ((pointIndex > 0 && rotePoint[pointIndex].y != rotePoint[pointIndex - 1].y)
               || (rotePoint.Count > 0 && pointIndex == 0 && rotePoint[0].y != currentPos.y))
        {
            playerAction.SetPlayerAnimation(PlayerState.Climb);
        }
        else if(playerTransform.childCount != 0)
        {
            speed = pushSpeed;
            if (playerTransform.GetChild(0).GetComponent<Box>().IsPush)
                playerAction.SetPlayerAnimation(PlayerState.Push);
            else
                playerAction.SetPlayerAnimation(PlayerState.Pull);
        }
        else
        {
            playerAction.SetPlayerAnimation(PlayerState.Run);
        }

        if (currentPos != rotePoint[pointIndex])
        {
            playerTransform.position = Vector2.MoveTowards(currentPos, rotePoint[pointIndex], Time.deltaTime * speed);
            
        }
        else
        {
            if ((Vector2)playerTransform.position == rotePoint[rotePoint.Count - 1])
            {
                CheckPassLevel();
                GetDestination = true;
                return;
            }
            pointIndex++;
        }
    }


    /************add by ld****************/
    void UpdateTargetX(ref float targetX)
    {
        float x = MathCalulate.GetHalfValue(targetX);
        if((x == mask.GetMinX() || x == mask.GetMaxX()))//点到了底片边界
        {
            float y = MathCalulate.GetHalfValue(playerTransform.position.y);
            if(y >= mask.GetMinY() && y <= mask.GetMaxY())
            {
                if (targetX < mask.GetMinX() || (x == mask.GetMaxX() && targetX < mask.GetMaxX()))
                {
                    x--;
                }
                else
                {
                    x++;
                }
            }
            
        }
        targetX = x;
        
    }

    bool IfMaskVertexBlockInLand(Vector2 landPos)
    {
        landPos = MathCalulate.GetHalfVector2(landPos);
        Vector2[] vertex =
        {
            new Vector2(mask.GetMinX(),mask.GetMinY()),
            new Vector2(mask.GetMinX(),mask.GetMaxY()),
            new Vector2(mask.GetMaxX(),mask.GetMinY()),
            new Vector2(mask.GetMaxX(),mask.GetMaxY())
        };
        for(int i = 0;i<vertex.Length;i++)
        {
            if(vertex[i] == landPos)
            {
                return true;
            }
        }
        return false;
    }

    LandBox TwoRayToForward(Vector2 currentGetPos)
    {
        RaycastHit2D shortRay = Physics2D.Raycast(currentGetPos, GetRayDirection(), shortHorizontalRayLength);
        RaycastHit2D longRay =  Physics2D.Raycast(currentGetPos, GetRayDirection(), horizontalRayLength);
        if(shortRay.collider == null && longRay.collider == null)
        {
            return LandBox.none;
        }
        else if(shortRay.collider != null && longRay.collider != null)
        {
            return LandBox.whole;
        }
        else if(shortRay.collider == null && longRay.collider != null)
        {
            return GetRayDirection() == Vector2.right ? LandBox.rigthHalf : LandBox.leftHalf;
        }
        else
        {
            return GetRayDirection() == Vector2.left ? LandBox.rigthHalf : LandBox.leftHalf;
        }
    }

    LandBox TwoRayFromForwardToDown(Vector2 currentGetPos)
    {
        RaycastHit2D shortRay = Physics2D.Raycast(currentGetPos + GetRayDirection() * shortHorizontalRayLength, Vector2.down, verticalRayLength);
        RaycastHit2D longRay = Physics2D.Raycast(currentGetPos + GetRayDirection() * horizontalRayLength, Vector2.down, verticalRayLength);
        if (shortRay.collider == null && longRay.collider == null)
        {
            return LandBox.none;
        }
        else if (shortRay.collider != null && longRay.collider != null)
        {
            return LandBox.whole;
        }
        else if (shortRay.collider == null && longRay.collider != null)
        {
            return GetRayDirection() == Vector2.right ? LandBox.rigthHalf : LandBox.leftHalf;
        }
        else
        {
            return GetRayDirection() == Vector2.left ? LandBox.rigthHalf : LandBox.leftHalf;
        }
    }

    LandBox TwoRayToCheckFall(Vector2 currentGetPos)
    {
        RaycastHit2D shortRay = Physics2D.Raycast(currentGetPos + GetRayDirection() * shortHorizontalRayLength + Vector2.down * verticalRayLength,
            Vector2.down, maxFallHeight);
        RaycastHit2D longRay = Physics2D.Raycast(currentGetPos + GetRayDirection() * horizontalRayLength + Vector2.down * verticalRayLength,
            Vector2.down, maxFallHeight);
        if(shortRay.collider != null && longRay.collider != null)
        {
            return LandBox.whole;
        }
        else
        {
            return LandBox.none;
        }
    }

    bool CanFallInHalf(Vector2 currentGetPos)
    {
        RaycastHit2D noneBox1 = RayToForward(currentGetPos + GetRayDirection());
        RaycastHit2D noneBox2 = RayToForward(currentGetPos + GetRayDirection() - Vector2.up);
        LandBox wholeBox1 = TwoRayFromForwardToDown(currentGetPos - Vector2.up);
        
        RaycastHit2D wholeBox2 = RayToForward(currentGetPos - Vector2.up*2 + GetRayDirection());
        if (noneBox1.collider == null && noneBox2.collider == null && wholeBox1 == LandBox.whole && wholeBox2.collider != null)
        {
            return true;
        }
        return false;
    }

    bool IfCanClimb(ref Vector2 currentGetPos)//前方有整格障碍的时候判断判断能不能爬
    {
        rotePoint.Add(currentGetPos);
        if (RayToUp(currentGetPos).collider == null)//头顶无障碍
        {
            if (mask.IfPointAtBorderY(currentGetPos + Vector2.up)) //主角的头顶接近底片
            {
                return false;
            }
            if (RayToCheckClimb(currentGetPos).collider == null)//能爬上去/*************可能会卡住************/
            {

                currentGetPos += new Vector2(GetRayDirection().x * 1, 1);

                if (mask.IfPointAtBorderY(currentGetPos))
                {
                    currentGetPos -= new Vector2(GetRayDirection().x * 1, 1);
                    return false;
                }
                rotePoint.Add(currentGetPos);
            }
            else//不能爬上去
            {
                return false;
            }
        }
        else//头顶有障碍
        {
            return false;
        }
        return true;
    }

    void CaculateRoute()
    {
        Debug.Log("寻路计算");
        rotePoint.Clear();
        UpdateTargetX(ref targetX);
        SetPlayerTowards(targetX);
        Vector2 playerPos = playerTransform.position;
        //Vector2 currentGetPos = new Vector2(MathCalulate.GetHalfValue(playerPos.x), playerPos.y);
        Vector2 currentGetPos = MathCalulate.GetHalfVector2(playerPos);

        while (currentGetPos.x != targetX)
        {
            if (IfMaskVertexBlockInLand(currentGetPos + GetRayDirection())) //主角前方遇到了底片顶点
            {
                break;
            }
            LandBox forwardBox = TwoRayToForward(currentGetPos);
            if (forwardBox == LandBox.none)//前方无障碍
            {
                LandBox pit = TwoRayFromForwardToDown(currentGetPos);
                if (pit == LandBox.none)//前方有坑
                {
                    rotePoint.Add(currentGetPos);
                    if (TwoRayToCheckFall(currentGetPos) == LandBox.none)//跳不下去
                    {
                        break;
                    }
                    else//能跳下去
                    {
                        currentGetPos += new Vector2(GetRayDirection().x * 1, -1);//这个1是移动一个格子
                        if (mask.IfPointAtBorderY(currentGetPos))
                        {
                            currentGetPos -= new Vector2(GetRayDirection().x * 1, -1);
                            break;
                        }
                        rotePoint.Add(currentGetPos);
                    }
                }
                else if(pit == LandBox.whole)//前方无坑可以直接走
                {
                    currentGetPos = currentGetPos + GetRayDirection() * 1f;
                }
                else//前方是个半坑
                {
                    if(((pit == LandBox.leftHalf && GetRayDirection() == Vector2.right)||
                        (pit == LandBox.rigthHalf && GetRayDirection() == Vector2.left)) && CanFallInHalf(currentGetPos))
                    {
                        rotePoint.Add(currentGetPos + GetRayDirection());
                        currentGetPos += GetRayDirection()*2 - Vector2.up;
                        rotePoint.Add(currentGetPos);
                    }
                    else
                    {
                        break;
                    }
                    
                }
            }
            else if(forwardBox == LandBox.whole)//前方有整格障碍
            {
               if(!IfCanClimb(ref currentGetPos))
                {
                    break;
                }
            }
            else//前方有半格障碍
            {
                LandBox pit = TwoRayFromForwardToDown(currentGetPos);
                bool condition1 = (GetRayDirection() == Vector2.right && forwardBox == LandBox.rigthHalf) && (pit != LandBox.rigthHalf && pit != LandBox.none);
                bool condition2 = (GetRayDirection() == Vector2.left && forwardBox == LandBox.leftHalf) && (pit != LandBox.leftHalf && pit != LandBox.none);
                if(condition1 || condition2)
                {
                    if (!IfCanClimb(ref currentGetPos))
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
               
            }
        }


        if (rotePoint.Count == 0 || rotePoint[rotePoint.Count - 1] != currentGetPos)
        {
            rotePoint.Add(currentGetPos);//添加终点
        }

        //更新卡住了的点TODO,可能会导致list长度为0
        if(mask.IfPointAtBorderX(rotePoint[rotePoint.Count - 1]))
        {
            rotePoint.RemoveAt(rotePoint.Count - 1);
        }

    }

    void CheckPassLevel()
    {
        LevelManager.Instance.CanPassLevel();
    }

    public void CalculateWithBox(bool moveRight)
    {
        GetDestination = false;
        playerTransform.GetChild(0).GetComponent<BoxCollider2D>().enabled = false;
        rotePoint.Clear();
        pointIndex = 0;

        Vector2 playerPos = playerTransform.position;
        Vector2 currentGetPos = MathCalulate.GetHalfVector2(playerPos);
        targetX = moveRight ? playerPos.x + 20 : playerPos.x - 20;
        Vector2 direction = moveRight ? Vector2.right : Vector2.left;

        while (currentGetPos.x != targetX)
        {
            Debug.Log(currentGetPos);
            if (IfMaskVertexBlockInLand(currentGetPos + GetRayDirection())) //主角前方遇到了底片顶点
            {
                if (playerTransform.GetChild(0).position.x > playerPos.x == moveRight)
                {
                    currentGetPos += direction * -1f;                 
                }
                break;
            }
            LandBox forwardBox = TwoRayToForward(currentGetPos);
            if (forwardBox == LandBox.none)//前方无障碍
            {
                Debug.Log("前方无障碍");
                LandBox pit = TwoRayFromForwardToDown(currentGetPos);
                if (pit == LandBox.none)//前方有坑
                {
                    Debug.Log("前方有坑");
                    break;
                }                
                else
                {
                    Debug.Log("前走一格");
                    currentGetPos = currentGetPos + GetRayDirection() * 1f;
                }
            }
            else//前方有障碍（墙体或者半墙）
            {
                Debug.Log("前方有障碍");
                if (playerTransform.GetChild(0).position.x > playerPos.x == moveRight)
                {
                    Debug.Log("推箱子中，往后退一格");
                    currentGetPos += direction * -1f;
                }
                break;
            }
        }

        if (mask.IfPointAtBorderX(currentGetPos + direction * 1f) && playerTransform.GetChild(0).GetComponent<Box>().IsPush)//不能把箱子推到边框上
        {
            Debug.Log("不能把箱子推到边框上：" + currentGetPos);
            currentGetPos += direction * -1f;
        }

        if (rotePoint.Count == 0 || rotePoint[rotePoint.Count - 1] != currentGetPos)
        {
            Debug.Log("添加终点：" + currentGetPos);
            rotePoint.Add(currentGetPos);//添加终点
        }
        
        playerTransform.GetChild(0).GetComponent<BoxCollider2D>().enabled = true;
    }
}
   


