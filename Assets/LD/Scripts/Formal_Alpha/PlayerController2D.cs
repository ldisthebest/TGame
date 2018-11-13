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

    [SerializeField]
    float maxClimbHeight,maxFallHeight;

   // float targetX;

    List<Vector2> rotePoint;
    int pointIndex;

    bool GetDestination;

    PlayerAction playerAction;

    void Awake()
    {
        playerTransform = transform;
        playerAction = GetComponent<PlayerAction>();
        rotePoint = new List<Vector2>();
        GetDestination = true;
        mainCamera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
       
        SetInitialPos();
    }

    private void UpdatePlayerContour()
    {
        playerContour.minX = playerTransform.position.x - halfWidth;
        playerContour.maxX = playerTransform.position.x + halfWidth;
        playerContour.minY = playerTransform.position.y - halfHeight;
        playerContour.maxY = playerTransform.position.y + halfHeight;
        //#if UNITY_EDITOR
        //        Debug.DrawLine(Contour.BottomLeft, Contour.BottomRight, Color.green);
        //        Debug.DrawLine(Contour.BottomRight, Contour.TopRigtht, Color.green);
        //        Debug.DrawLine(Contour.TopRigtht, Contour.TopLeft, Color.green);
        //        Debug.DrawLine(Contour.TopLeft, Contour.BottomLeft, Color.green);
        //#endif



    }

    void SetInitialPos()
    {
        float x = MathCalulate.GetHalfValue(playerTransform.position.x);
        playerTransform.position = new Vector2(x, playerTransform.position.y);
        UpdatePlayerContour();

        RaycastHit2D hit = Physics2D.Raycast(playerTransform.position,Vector2.down,20);
        if(hit.collider != null)
        {
            float colliderTopY = hit.collider.bounds.max.y;
            playerTransform.position += Vector3.up * (colliderTopY - playerContour.minY);           
        }
        //float y = MathCalulate.GetHalfValue(playerTransform.position.y);
        //playerTransform.position = new Vector2(playerTransform.position.x,y);
    }

    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            Vector2 hitPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            //Debug.Log(hitPos);
            float targetX = MathCalulate.GetHalfValue(hitPos.x);            
            CaculateRote(targetX);
            GetDestination = false;
            pointIndex = 0;
        }
        if(!GetDestination)
        {
            MoveToRotePoint();
        }
        else
        {
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
        //UpdatePlayerContour();
        //RayToForward(playerTransform.position);
        //RayToUp(playerTransform.position);
        //RayToDown(playerTransform.position);
    }

    void SetPlayerTowards(float destinationX)
    {
        float offoset = destinationX - playerTransform.position.x;
        float scale = playerTransform.localScale.x;
        if(offoset * scale < 0)//换方向
        {
            playerTransform.localScale = new Vector2(-scale, playerTransform.localScale.y);
        }
    }

    void CaculateRote(float targetX)
    {
        rotePoint.Clear();
        SetPlayerTowards(targetX);
        Vector2 playerPos = playerTransform.position;
        Vector2 currentGetPos = new Vector2(MathCalulate.GetHalfValue(playerPos.x), playerPos.y);

        while(currentGetPos.x != targetX)
        {
            RaycastHit2D climbHit = RayToForward(currentGetPos);
            if (climbHit.collider == null)//前方无障碍
            {
                if(RayFromForwardToDown(currentGetPos).collider == null)//前方有坑
                {
                    rotePoint.Add(currentGetPos);
                    RaycastHit2D fallHit = RayToCheckFall(currentGetPos);
                    if (fallHit.collider == null)//跳不下去
                    {
                        break;
                    }
                    else//能跳下去
                    {
                        float colliderTopY = fallHit.collider.bounds.max.y;
                        currentGetPos += new Vector2(GetRayDirection().x*1, -1);//这个1是移动一个格子
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
                rotePoint.Add(currentGetPos);
                if (RayToUp(currentGetPos).collider == null)//头顶无障碍
                {
                    if(RayToCheckClimb(currentGetPos).collider == null)//能爬上去
                    {
                        float colliderTopY = climbHit.collider.bounds.max.y;
                        currentGetPos += new Vector2(GetRayDirection().x * 1, 1);
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
        return Physics2D.Raycast(currentGetPos, GetRayDirection(), horizontalRayLength);
    }

    RaycastHit2D RayToUp(Vector2 currentGetPos)
    {
        return Physics2D.Raycast(currentGetPos, Vector2.up, verticalRayLength);
    }

    RaycastHit2D RayFromForwardToDown(Vector2 currentGetPos)
    {
        return Physics2D.Raycast(currentGetPos + GetRayDirection()*horizontalRayLength, Vector2.down, verticalRayLength);
    }

    RaycastHit2D RayToCheckClimb(Vector2 currentGetPos)
    {
        return Physics2D.Raycast(currentGetPos + maxClimbHeight * Vector2.up, GetRayDirection(), horizontalRayLength);
    }

    RaycastHit2D RayToCheckFall(Vector2 currentGetPos)
    {
        return Physics2D.Raycast(currentGetPos + GetRayDirection() * horizontalRayLength + Vector2.down * verticalRayLength,
            Vector2.down, verticalRayLength);
    }

    void MoveToRotePoint()
    {
        playerAction.SetPlayerAnimation(PlayerState.Run);
        Vector2 currentPos = playerTransform.position;
        
        if (currentPos != rotePoint[pointIndex])
        {
            playerTransform.position = Vector2.MoveTowards(currentPos, rotePoint[pointIndex], Time.deltaTime * moveSpeed);
            
        }
        else
        {
            if ((Vector2)playerTransform.position == rotePoint[rotePoint.Count - 1])
            {
                GetDestination = true;
                return;
            }
            pointIndex++;
        }
    }
}
