﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box : MonoBehaviour {

    private bool isShow = false;
    [HideInInspector]
    public bool IsMove = false;
    [HideInInspector]
    public bool IsPush = false;

    public Transform Player;
    public BoxInteraction boxUI;

    private bool isDrop = false;
    private Vector3 destination;
    [SerializeField]
    private float dropSpeed;

    // Use this for initialization
    void Start () {
        //boxUI = GetComponentInChildren<BoxInteraction>();
        boxUI.TheBox = this;
        destination = transform.position;
	}
	
	// Update is called once per framek
	void Update () {

        if (IsMove)
        {
            if (LayerMask.NameToLayer("bothSeen") != gameObject.layer)
                SetLayer("bothSeen");            
            return;
        }
        else if (LayerMask.NameToLayer("bothSeen") == gameObject.layer)
        {
            if (Player.GetComponent<PlayerController2D>().mask.IsInRectangle(transform.position))
                SetLayer("Default");
            else
                SetLayer("mask");
        }

        if (!isShow && Mathf.Abs(Player.position.x-transform.position.x)<1.1f && Mathf.Abs(Player.position.y-transform.position.y)<0.2f)
        {
            if (Player.GetComponent<PlayerController2D>().mask.IsInRectangle(Player.position)==(gameObject.layer==LayerMask.NameToLayer("Default")) )
            ShowUI();
        }
        else if(isShow &&( Mathf.Abs(Player.position.x - transform.position.x) >= 1.1f || Mathf.Abs(Player.position.y - transform.position.y) >= 0.2f))
        {
            HideUI();
        }

        if(isDrop)
        {
            if (Mathf.Abs(destination.y-transform.position.y)>0.1f)
                transform.Translate((destination - transform.position).normalized * dropSpeed * Time.deltaTime);
            else
            {
                transform.position = MathCalulate.GetHalfVector2(transform.position);
                isDrop = false;
            }
        }
        
	}

    void ShowUI()
    {
        isShow = true;
        //后期考虑使用对象池,进行动态分配，现阶段每个箱子一套UI
        boxUI.gameObject.SetActive(true);
    }

    void HideUI()
    {
        isShow = false;
        boxUI.gameObject.SetActive(false);
    }

    public void Drop()
    {
        //向下检测起始点应该在箱子下沿之外，当前位置向下加0.5是箱子下沿
        Vector2 origin = (Vector2)transform.position + Vector2.down * 0.6f;
        RaycastHit2D ray = Physics2D.Raycast(origin,Vector2.down);
        if(!ray)
        {
            isDrop = true;
            //无限深，箱子掉出屏幕外，需要销毁，待优化
            destination = (Vector2)transform.position + Vector2.down * 10f;
        }
        else if(Mathf.Abs(ray.point.y-transform.position.y)>=1f)
        {
            isDrop = true;
            destination = MathCalulate.GetHalfVector2(ray.point+Vector2.up*0.1f);
        }
    }

    void SetLayer(string str)
    {
        gameObject.layer = LayerMask.NameToLayer(str);
        boxUI.transform.parent.gameObject.layer = LayerMask.NameToLayer(str);
        boxUI.gameObject.layer = LayerMask.NameToLayer(str);
        RectTransform[] children = boxUI.GetComponentsInChildren<RectTransform>();
        foreach (RectTransform r in children)
        {
            r.gameObject.layer = LayerMask.NameToLayer(str);
        }
    }

}
