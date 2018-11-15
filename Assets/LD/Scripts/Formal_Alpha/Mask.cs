using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mask : MonoBehaviour {
    [SerializeField]
    float halfWidth;
    [SerializeField]
    float halfHeight;

    [SerializeField]
    float outHalfWidth, outHalfHeight;

    [SerializeField]
    float attachSpeed;

    public Rectangle maskContour;

    public Rectangle outMaskContour;

    Vector3 attachPos;

    bool getAttached;

    bool hitted;

    Camera camer;

    [SerializeField]
    PlayerAction playerAction;
    PlayerController2D player;

    Transform maskTransform;

    Vector2 firstHitPos;

    MaskCollider maskCollider;

    [HideInInspector]
    public bool hasDrag;

    void Awake()
    {
        maskTransform = transform;
        player = playerAction.GetComponent<PlayerController2D>();
        hitted = false;
        getAttached = true;
        camer = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        maskCollider = GetComponent<MaskCollider>();

        hasDrag = false;
        InitMask();
        //test

    }


    void InitMask()
    {
        Vector2 halfPos = MathCalulate.GetHalfVector2(maskTransform.position);
        maskTransform.position = new Vector3(halfPos.x, halfPos.y, -1);
    }
    public float GetMinX()
    {
        return maskTransform.position.x - halfWidth;
    }

    public float GetMaxX()
    {
        return maskTransform.position.x + halfWidth;
    }

    public float GetMinY()
    {
        return maskTransform.position.y - halfHeight;
    }

    public float GetMaxY()
    {
        return maskTransform.position.y + halfHeight;
    }

    public float GetOutMinX()
    {
        return maskTransform.position.x - outHalfWidth;
    }

    public float GetOutMaxX()
    {
        return maskTransform.position.x + outHalfWidth;
    }


    public float GetOutMinY()
    {
        return maskTransform.position.y - outHalfHeight;
    }

    public float GetOutMaxY()
    {
        return maskTransform.position.y + outHalfHeight;
    }
    public Rectangle GetOutMaskContour()
    {
        maskContour.minX = GetOutMinX();
        maskContour.minY = GetOutMinY();
        maskContour.maxX = GetOutMaxX();
        maskContour.maxY = GetOutMaxY();
        return maskContour;
    }

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

        if (MathCalulate.ConvergenceRectangle(maskAboutRect, player.GetPlayerContour()) != null)
        {
            return true;
        }
        return false;
    }

    public bool IfPointAtBorderY(Vector2 point)
    {
        if(point.x >= GetMinX() && point.x <= GetMaxX())
        {
            if(point.y == GetMinY() || point.y == GetMaxY())
            {
                return true;
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

    public bool IfPosJustOnBorderTop(Vector2 point)
    {
        if(point.x <= GetMaxX() && point.x >= GetMinX() && point.y - GetMaxY() == 1)
        {
            return true;
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

    void Update()
    {

        if (Input.GetMouseButtonDown(0))
        {

            firstHitPos = camer.ScreenToWorldPoint(Input.mousePosition);
            if (!IsInRectangle(firstHitPos) || playerAction.CurrentState != PlayerState.Idel)
            {
                // dragOffoset = Vector2.zero;
                return;
            }

            hitted = true;
        }
        if(hitted)
        {
            Vector2 newHitPos = camer.ScreenToWorldPoint(Input.mousePosition);
            Vector2 dragOffoset = newHitPos - firstHitPos;
            if(dragOffoset != Vector2.zero)
            {
                hasDrag = true;
            }
            Vector2 maskAboutToPos = (Vector2)maskTransform.position + dragOffoset;
            if (IfIntersectionWithPlayer(maskAboutToPos))
            {
                dragOffoset = Vector2.zero;
            }

            maskTransform.Translate(dragOffoset);
            firstHitPos = newHitPos;

            if(Input.GetMouseButtonUp(0)) //松开鼠标
            {
                hitted = false;
                if (hasDrag)
                {
                    getAttached = false;
                    Vector2 halfPos = MathCalulate.GetHalfVector2(maskTransform.position);
                    attachPos = new Vector3(halfPos.x, halfPos.y, -1);

                    if(IfIntersectionWithPlayer(attachPos))
                    {
                        if(!IfIntersectionWithPlayer(attachPos + Vector3.right))
                        {
                            attachPos += Vector3.right;
                        }
                        else if(!IfIntersectionWithPlayer(attachPos - Vector3.right))
                        {
                            attachPos -= Vector3.right;
                        }
                        else if(!IfIntersectionWithPlayer(attachPos + Vector3.up))
                        {
                            attachPos += Vector3.up;
                        }
                        else if(!IfIntersectionWithPlayer(attachPos - Vector3.up))
                        {
                            attachPos -= Vector3.up;
                        }

                    }
                }             
            }
        }

        if (!getAttached)//吸附到指定位置
        {
            Vector3 pos = maskTransform.position;
            maskTransform.position = Vector3.MoveTowards(pos, attachPos, Time.deltaTime * attachSpeed);
            if (maskTransform.position == attachPos)
            {
                hasDrag = false;
                getAttached = true;
                maskCollider.UpdateLandformCollider(GetOutMaskContour());
            }
        }

    }
}

