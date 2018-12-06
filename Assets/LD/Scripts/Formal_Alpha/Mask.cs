using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mask : MonoBehaviour {

    #region 事件委托
    public delegate void UpdateBoxColliderHandler(Rectangle rect);

    public event UpdateBoxColliderHandler UpdateColliderEvent;
    #endregion

    #region 序列化的私有字段

    [SerializeField]
    float halfWidth;

    [SerializeField]
    float halfHeight;

    [SerializeField]
    float outHalfWidth, outHalfHeight;

    [SerializeField]
    float attachSpeed;

    [SerializeField]
    PlayerAction playerAction;

    #endregion

    #region 非序列化的私有字段

    Vector3 attachPos;

    bool getAttached;

    bool hitted;

    Camera camer;

    PlayerController2D player;

    Transform maskTransform;

    Vector2 firstHitPos;

    Vector2 startPos;

    MaskCollider maskCollider;

    #endregion

    #region 公有字段

    [HideInInspector]
    public bool hasDrag;

    #endregion

    #region 初始化

    void Awake()
    {
        maskTransform = transform;
        player = playerAction.GetComponent<PlayerController2D>();      
        camer = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        maskCollider = GetComponent<MaskCollider>();
        hasDrag = false;
        hitted = false;
        getAttached = true;
        InitMask();
        maskCollider.InitWorldColliders(GetOutMaskContour());
    }

    void InitMask()
    {
        Vector2 halfPos = MathCalulate.GetHalfVector2(maskTransform.position);
        maskTransform.position = new Vector3(halfPos.x, halfPos.y, -1);
    }

    #endregion

    #region 设置并获得底片属性

    private float GetMinX()
    {
        return maskTransform.position.x - halfWidth;
    }

    private float GetMaxX()
    {
        return maskTransform.position.x + halfWidth;
    }

    private float GetMinY()
    {
        return maskTransform.position.y - halfHeight;
    }

    private float GetMaxY()
    {
        return maskTransform.position.y + halfHeight;
    }

    private float GetOutMinX()
    {
        return maskTransform.position.x - outHalfWidth;
    }

    private float GetOutMaxX()
    {
        return maskTransform.position.x + outHalfWidth;
    }

    private float GetOutMinY()
    {
        return maskTransform.position.y - outHalfHeight;
    }

    private float GetOutMaxY()
    {
        return maskTransform.position.y + outHalfHeight;
    }

    private Rectangle GetOutMaskContour()
    {
        Rectangle outMaskContour;
        outMaskContour.minX = GetOutMinX();
        outMaskContour.minY = GetOutMinY();
        outMaskContour.maxX = GetOutMaxX();
        outMaskContour.maxY = GetOutMaxY();
        return outMaskContour;
    }

    private Rectangle GetOutMaskContour(Vector2 aboutPos)
    {
        Rectangle outMaskContour;
        outMaskContour.minX = aboutPos.x - outHalfWidth; 
        outMaskContour.minY = aboutPos.y - outHalfHeight;
        outMaskContour.maxX = aboutPos.x + outHalfWidth;
        outMaskContour.maxY = aboutPos.y + outHalfHeight; ;
        return outMaskContour;
    }

    #endregion

    #region 判断点与底片的位置关系

    public bool IsInRectangle(Vector2 pos)
    {
        float x = pos.x;
        float y = pos.y;
        if(x >= GetOutMinX() && x <= GetOutMaxX() && y>= GetOutMinY() && y <= GetOutMaxY())
        {
            return true;
        }
        return false;
    }

    bool IfIntersectionWithPlayer(Vector2 pos)
    {
        Rectangle maskAboutRect;
        maskAboutRect.minX = pos.x - outHalfWidth;
        maskAboutRect.maxX = pos.x + outHalfWidth;
        maskAboutRect.minY = pos.y - outHalfHeight;
        maskAboutRect.maxY = pos.y + outHalfHeight;

        if (MathCalulate.ConvergenceRectangle(maskAboutRect, player.PlayerContour) != null)
        {
            return true;
        }
        return false;
    }

    public bool IfPointAtBorderY(params Vector2[] point)
    {
        for(int i = 0;i<point.Length;i++)
        {
            if (point[i].x >= GetMinX() && point[i].x <= GetMaxX())
            {
                if (point[i].y == GetMinY() || point[i].y == GetMaxY())
                {
                    return true;
                }
            }
        }
        
        return false;
    }

    public bool IfPointAtBorderX(Vector2 point)
    {
        if (point.y >= GetMinY() && point.y <= GetMaxY())
        {
            if (point.x == GetMinX() || point.x == GetMaxX())
            {
                return true;
            }
        }
        return false;
    }

    public bool IfPosJustOnBorderTop(params Vector2[] point)
    {
        for(int i = 0;i<point.Length;i++)
        {
            if (point[i].x <= GetMaxX() && point[i].x >= GetMinX() && point[i].y - GetMaxY() == 1)
            {
                return true;
            }
        }
       
        return false;
    }

    public bool IfPosJustOnBorderBottom(Vector2 point)
    {
        if (point.x <= GetMaxX() && point.x >= GetMinX() && point.y - GetMinY() == 1)
        {
            return true;
        }
        return false;
    }

    public bool IfVertexBlockInLand(Vector2 landPos)
    {
        Vector2[] vertex =
       {
            new Vector2(GetMinX(),GetMinY()),
            new Vector2(GetMinX(),GetMaxY()),
            new Vector2(GetMaxX(),GetMinY()),
            new Vector2(GetMaxX(),GetMaxY())
        };
        for (int i = 0; i < vertex.Length; i++)
        {
            if (vertex[i] == landPos)
            {
                return true;
            }
        }
        return false;
    }

    #endregion

    #region 拖动底片的逻辑

    void Update()
    {

        if (Input.GetMouseButtonDown(0) && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            if(!OnMouseDownInMask())
            {
                return;
            }
        }
        if(hitted)
        {
            HittedEvent();
            if (Input.GetMouseButtonUp(0)) //松开鼠标
            {               
                MouseUpEvent();
            }
        }

        if (!getAttached)//吸附到指定位置
        {
            AttachToPos();
        }
    }

    bool OnMouseDownInMask()
    {
        Vector2 hitPos = camer.ScreenToWorldPoint(Input.mousePosition);
        //如果没点中底片或者主角现在没有静止就返回
        if (IsInRectangle(hitPos))
        {
            firstHitPos = hitPos;
            startPos = maskTransform.position;
        }
        if (!IsInRectangle(hitPos) || playerAction.CurrentState != PlayerState.Idel || IsInRectangle(player.PlayerPos))
        {
            return false;
        }
        //否则判定为点中了
        hitted = true;
        firstHitPos = hitPos;
        return true;
    }

    void HittedEvent()
    {
        Vector2 dragOffoset = GetDragOffoset();
        //如果有拖动轨迹就不能让人物走
        if (!hasDrag && dragOffoset != Vector2.zero)
        {
            hasDrag = true;
        }
        Vector2 maskAboutToPos = (Vector2)maskTransform.position + dragOffoset;
        if (IfIntersectionWithPlayer(maskAboutToPos))
        {
            dragOffoset = MathCalulate.GetOffoset(player.PlayerContour,GetOutMaskContour(),dragOffoset);
        }

        maskTransform.Translate(dragOffoset);
    }

    void MouseUpEvent()
    {
        hitted = false;
        //当松开点击底片位置不是最开始的位置则吸附
        if ((Vector2)maskTransform.position != startPos)
        {
            getAttached = false;

            Vector2 halfPos = MathCalulate.GetHalfVector2(maskTransform.position);
            attachPos = new Vector3(halfPos.x, halfPos.y, -1);
            attachPos += (Vector3)MathCalulate.UpdateMaskPosOffoset(player.PlayerContour, GetOutMaskContour(attachPos));
        }
    }

    void AttachToPos()
    {
        Vector3 pos = maskTransform.position;
        maskTransform.position = Vector3.MoveTowards(pos, attachPos, Time.deltaTime * attachSpeed);
        //已经吸附到指定位置
        if (maskTransform.position == attachPos)
        {
            getAttached = true;
            maskCollider.UpdateLandformCollider(GetOutMaskContour());
        }
    }

    public Vector2 GetDragOffoset()
    {
        Vector2 newHitPos = camer.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dragOffoset = newHitPos - firstHitPos;
        firstHitPos = newHitPos;
        return dragOffoset;
    }
    #endregion

    #region 还原底片的一些标志位，外部调用
    public void RevertDragMark()
    {
        hasDrag = false;
    }
    #endregion

}

