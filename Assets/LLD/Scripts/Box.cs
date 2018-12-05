using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box : MonoBehaviour {

    #region 非序列化私有字段

    private bool isMove = false;

    private bool isDrop = false;

    private bool isShow = false;

    private Mask mask;

    private Transform theTransform;

    private BoxCollider2D theCollider;

    private Vector3 destination;

    PlayerController2D player;

    #endregion

    #region 序列化私有字段

    [SerializeField]
    private float dropSpeed;

    #endregion

    #region 公有字段

    [HideInInspector]
    public bool IsPush = false;

    public Transform Player;

    public BoxInteraction boxUI;

    #endregion

    #region 公有属性

    public bool IsMove
    {
        get { return isMove; }
        set
        {
            if(value == false)
                transform.position= MathCalulate.GetHalfVector2(transform.position);
            isMove = value;
        }
    }

    #endregion

    #region Mono

    // Use this for initialization
    void Awake () {

        boxUI.TheBox = this;

        mask = GameObject.FindWithTag("Mask").GetComponent<Mask>();

        player = GameObject.FindWithTag("Player").GetComponent<PlayerController2D>();

        theTransform = transform;

        destination = theTransform.position;

        theCollider = GetComponent<BoxCollider2D>();

        player.ShowUiEvent += ShowUI;

        player.HideUiEvent += HideUI;
    }
	
	// Update is called once per framek
	void Update () {

        //修改图层
        ChangeLayer();

        if (IsMove)
            return;

        //修改碰撞体
        ChangeColliderState();

        //掉落
        Drop();
	}
   
    #endregion

    #region 私有方法

    void ChangeLayer()
    {
        //修改箱子图层,箱子正在移动的时候是bothseen
        if (IsMove)
        {
            if (LayerMask.NameToLayer("bothSeen") != gameObject.layer)
                SetLayer("bothSeen");
            return;
        }
        else if (LayerMask.NameToLayer("bothSeen") == gameObject.layer)
        {
            if (mask.IsInRectangle(theTransform.position))
                SetLayer("Default");
            else
                SetLayer("mask");
        }
    }

    void ChangeColliderState()
    {
        if (theCollider.enabled == false && (mask.IsInRectangle(theTransform.position) == (gameObject.layer == LayerMask.NameToLayer("Default"))))
        {
            theCollider.enabled = true;
        }
        else if (theCollider.enabled == true && (mask.IsInRectangle(theTransform.position) != (gameObject.layer == LayerMask.NameToLayer("Default"))))
        {
            theCollider.enabled = false;
            HideUI();
        }
    }

    bool IsNearPlayer(Vector2 playerPos)
    {
        float distanceY = playerPos.y - theTransform.position.y;
        if (Mathf.Abs(playerPos.x - theTransform.position.x) < 1.5f && distanceY >= 0 && distanceY < 1)
        {
            return true;
        }
        return false;
    }

    void ShowUI(Vector2 playerPos)
    {
        if (!isShow && IsNearPlayer(playerPos))
        {
            bool condition1 = mask.IsInRectangle(Player.position) == (gameObject.layer == LayerMask.NameToLayer("Default")) && theCollider.enabled == true;
            bool condition2 = mask.IfPointAtBorderX(Player.position) && theCollider.enabled == true;
            if(condition1 || condition2)
            {
                isShow = true;
                //后期考虑使用对象池,进行动态分配，现阶段每个箱子一套UI
                boxUI.gameObject.SetActive(true);
            }
        }
        //else if (isShow && !IsNearPlayer(playerPos))
        //{
        //    HideUI();
        //}
    }

    void Drop()
    {
        if (isDrop)
        {
            if (Mathf.Abs(destination.y - theTransform.position.y) > 0.1f)
                theTransform.Translate((destination - theTransform.position).normalized * dropSpeed * Time.deltaTime);
            else
            {
                theTransform.position = MathCalulate.GetHalfVector2(theTransform.position);
                isDrop = false;
            }
        }
    }

    //void ShowUI()
    //{
    //    isShow = true;
    //    //后期考虑使用对象池,进行动态分配，现阶段每个箱子一套UI
    //    boxUI.gameObject.SetActive(true);
    //}

    void HideUI()
    {
        if(isShow)
        {
            isShow = false;
            boxUI.gameObject.SetActive(false);
        }       
    }

    void SetLayer(string str)
    {
        gameObject.layer = LayerMask.NameToLayer(str);
        boxUI.SetLayer(str);       
    }

    #endregion

    #region 公有方法

    public void DropCheck()
    {
        //向下检测起始点应该在箱子下沿之外，当前位置向下加0.5是箱子下沿
        Vector2 origin = (Vector2)theTransform.position + Vector2.down * 0.6f;
        RaycastHit2D ray = Physics2D.Raycast(origin, Vector2.down);
        if (!ray)
        {
            isDrop = true;
            //无限深，箱子掉出屏幕外，需要销毁，待优化
            destination = (Vector2)theTransform.position + Vector2.down * 10f;
        }
        else if (Mathf.Abs(ray.point.y - theTransform.position.y) >= 1f)
        {
            isDrop = true;
            destination = MathCalulate.GetHalfVector2(ray.point + Vector2.up * 0.1f);
        }
    }

    public void SetColliderActive(bool colliderState)
    {
        theCollider.enabled = colliderState;
    }
    
    public Vector2 GetBoxPos()
    {
        return theTransform.position;
    }
    #endregion
}
