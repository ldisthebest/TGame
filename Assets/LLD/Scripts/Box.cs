using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box : MonoBehaviour {

    #region 常量
    const string boxUiPath = "Prefabs/BoxUI";
    #endregion

    #region 非序列化私有字段

    private bool isShow = false;

    private Mask mask;

    private Transform theTransform;

    private BoxCollider2D theCollider;

    PlayerController2D player;

    Transform playerTransform;

    BoxInteraction boxUI;

    Transform parent;

    #endregion

    #region 序列化私有字段

    [SerializeField,Range(0,1)]
    private float gravity;

    [SerializeField]
    private float bottomOffoset;

    #endregion

    #region 公有字段

    [HideInInspector]
    public bool IsPush = false;
    #endregion

    #region Mono

    // Use this for initialization
    void Awake () {

        //生成对应的箱子UI
        GameObject boxUIObject = Resources.Load<GameObject>(boxUiPath);
        boxUI = Instantiate(boxUIObject, Vector3.zero, Quaternion.identity).GetComponent<BoxInteraction>();
        boxUI.Init(this);

        parent = transform.parent;

        mask = GameObject.FindWithTag("Mask").GetComponent<Mask>();

        player = GameObject.FindWithTag("Player").GetComponent<PlayerController2D>();

        playerTransform = player.transform;

        theTransform = transform;

        theCollider = GetComponent<BoxCollider2D>();

        player.ShowUiEvent += ShowUI;

        player.HideUiEvent += HideUI;

        mask.UpdateColliderEvent += ChangeColliderState;
    }

    void OnDestroy()
    {
        player.ShowUiEvent -= ShowUI;

        player.HideUiEvent -= HideUI;

        mask.UpdateColliderEvent -= ChangeColliderState;
        //if(boxUI.gameObject != null)
        //{
        //    Destroy(boxUI.gameObject);
        //}
    }

    #endregion

    #region 私有方法

    void ChangeColliderState(Rectangle maskRect)
    {
        if (theCollider.enabled == false && (MathCalulate.PosInRect(theTransform.position,maskRect) == (gameObject.layer == LayerMask.NameToLayer("Default"))))
        {
            SetColliderActive(true);
            ShowUI(player.PlayerCenter);
        }
        else if (theCollider.enabled == true && (MathCalulate.PosInRect(theTransform.position, maskRect) != (gameObject.layer == LayerMask.NameToLayer("Default"))))
        {
            SetColliderActive(false);
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
        if (!isShow && IsNearPlayer(playerPos) && theCollider.enabled)
        {
            isShow = true;
            boxUI.SetUI(true);
        }
    } 

    void HideUI()
    {
        if(isShow)
        {
            isShow = false;
            boxUI.SetUI(false);
        }       
    }

    bool ShouldDrop()
    {
        //向下检测起始点应该在箱子下沿之外
        Vector2 origin = (Vector2)theTransform.position + Vector2.down * (bottomOffoset + 0.1f);
        RaycastHit2D ray = Physics2D.Raycast(origin, Vector2.down);

        if (Mathf.Abs(ray.point.y - theTransform.position.y) >= 1f)
        {
            Vector2 destination = MathCalulate.GetHalfVector2(ray.point + Vector2.up * bottomOffoset);
            StartCoroutine(Drop(destination));
            return true;
        }
        return false;

    }

    IEnumerator Drop(Vector2 destination)
    {
        float dropSpeed = 0;
        while ((Vector2)theTransform.position != destination)
        {
            dropSpeed += gravity;
            theTransform.position = Vector2.MoveTowards(theTransform.position, destination, 0.02f * dropSpeed);
            yield return new WaitForSeconds(0.01f);
        }
    }

    void SetLayer(string str)
    {
        gameObject.layer = LayerMask.NameToLayer(str);
    }

    #endregion

    #region 公有方法

    public void SetColliderActive(bool colliderState)
    {
        theCollider.enabled = colliderState;
    } 

    public bool CanBoxLeftMove()
    {
        IsPush = playerTransform.position.x < theTransform.position.x ? false : true;

        player.TheBox = this;

        if (!player.CalculateWithBox(Direction.left, theTransform.position))
        {
            return false;
        }

        SetLayer("bothSeen");

        theTransform.SetParent(playerTransform);

        return true;
    }

    public bool CanBoxRightMove()
    {
        IsPush = playerTransform.position.x < theTransform.position.x ? true : false;

        player.TheBox = this;

        if (!player.CalculateWithBox(Direction.right, theTransform.position))
        {
            return false;
        }

        SetLayer("bothSeen");

        theTransform.SetParent(playerTransform);

        return true;
    }

    public void EndMove()
    {   
        //transform.position = MathCalulate.GetHalfVector2(transform.position);
        theTransform.SetParent(parent);
        if (mask.IsInRectangle(theTransform.position))
        {
            SetLayer("Default");
        }
        else
        {
            SetLayer("mask");
        }

        if(!ShouldDrop())
        {
            boxUI.SetUI(true);
        }
    }
    #endregion
}
