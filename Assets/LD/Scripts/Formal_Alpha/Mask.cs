using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mask : MonoBehaviour {
    [SerializeField]
    float halfWidth;
    [SerializeField]
    float halfHeight;

    [SerializeField]
    float attachSpeed;

    public Rectangle maskContour;

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
        GetMaskContour();
        maskCollider = GetComponent<MaskCollider>();

        hasDrag = false;
        //test

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

    public Rectangle GetMaskContour()
    {
        maskContour.minX = GetMinX();
        maskContour.minY = GetMinY();
        maskContour.maxX = GetMaxX();
        maskContour.maxY = GetMaxY();
        return maskContour;
    }

    public bool IsInRectangle(Vector2 pos)
    {
        float x = pos.x;
        float y = pos.y;
        if(x >= GetMinX() && x <= GetMaxX() && y>= GetMinY() && y <= GetMaxY())
        {
            return true;
        }
        return false;
    }

    bool IfIntersectionWithPlayer(Vector2 pos)
    {
        Rectangle maskAboutRect;
        maskAboutRect.minX = pos.x - halfWidth;
        maskAboutRect.maxX = pos.x + halfWidth;
        maskAboutRect.minY = pos.y - halfHeight;
        maskAboutRect.maxY = pos.y + halfWidth;

        if (MathCalulate.ConvergenceRectangle(maskAboutRect, player.GetPlayerContour()) != null)
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
                maskCollider.UpdateLandformCollider(GetMaskContour());
            }
        }

    }
}

