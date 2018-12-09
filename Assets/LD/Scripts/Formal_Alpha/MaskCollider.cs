using System.Collections.Generic;
using UnityEngine;

public class MaskCollider : MonoBehaviour {

    #region 常量
    const string colliderSoldierPath = "Prefabs/OutsideCollider";
    #endregion

    #region 序列化的私有字段

    [SerializeField]
    List<BoxCollider2D> insideLandform;

    [SerializeField]
    List<BoxCollider2D> outsideLandform;

    #endregion

    #region 非序列化的私有字段

    Rectangle[] insideBox;

    Rectangle[] outsideBox;

    List<GameObject> colliderSoldiers;

    Object colliderSoldier;

    #endregion

    #region 初始化,外部调用

    public void InitWorldColliders(Rectangle maskRectangle)
    {
        colliderSoldier = Resources.Load(colliderSoldierPath, typeof(GameObject));
        insideBox = new Rectangle[insideLandform.Count];
        outsideBox = new Rectangle[outsideLandform.Count];
        colliderSoldiers = new List<GameObject>();
        InitRectangle();
        //首先隐藏底片世界的碰撞体
        for (int i = 0; i < insideBox.Length; i++)
        {
            insideLandform[i].enabled = false;
        }
        UpdateLandformCollider(maskRectangle);
    }

    void InitRectangle()
    {
        for (int i = 0; i < insideBox.Length; i++)
        {
            Bounds colliderBounds = insideLandform[i].bounds;
            insideBox[i].minX = colliderBounds.min.x;
            insideBox[i].minY = colliderBounds.min.y;
            insideBox[i].maxX = colliderBounds.max.x;
            insideBox[i].maxY = colliderBounds.max.y;
            insideLandform[i].enabled = false;//游戏开始先禁用底片世界的碰撞体
        }
        for (int i = 0; i < outsideBox.Length; i++)
        {
            Bounds bounds = outsideLandform[i].bounds;
            outsideBox[i].minX = bounds.min.x;
            outsideBox[i].maxX = bounds.max.x;
            outsideBox[i].minY = bounds.min.y;
            outsideBox[i].maxY = bounds.max.y;
        }
    }
     
    #endregion

    #region 切割碰撞体逻辑

    public void UpdateLandformCollider(Rectangle maskRectangle)
    {
        ClearColliderSoldiers();
        for (int i = 0; i < insideBox.Length; i++)
        {
            Rectangle? ConvergenceRect = MathCalulate.ConvergenceRectangle(insideBox[i], maskRectangle);
            if (ConvergenceRect != null && !MathCalulate.ifRectCanIgnore(ConvergenceRect.Value))//如果相交生成哨兵碰撞体
            {
                SetInsideCollider(insideLandform[i], insideBox[i], ConvergenceRect.Value);
            }
        }

        for(int i = 0; i< outsideBox.Length;i++)
        {
            SetOutsideColliderPos(outsideLandform[i], outsideBox[i], maskRectangle);
        }
    }

    //清空哨兵碰撞体和容器
    void ClearColliderSoldiers()
    {
        for (int i = 0; i < colliderSoldiers.Count; i++)
        {
            Destroy(colliderSoldiers[i]);
        }
        colliderSoldiers.Clear();
    }

    /// <param name="collider">碰撞体物体</param>
    /// <param name="colliderRect">碰撞体对应的初始矩形信息</param>
    /// <param name="rect">将碰撞体大小设置成的目标矩形</param>
    void SetInsideCollider(BoxCollider2D collider,Rectangle colliderRect,Rectangle rect)
    {
        SetSoldierCollider(rect);
    }

    void SetOutsideColliderPos(BoxCollider2D landformCollider,Rectangle colliderRect, Rectangle maskRectangle)
    {
        Rectangle? ifConvergence = MathCalulate.ConvergenceRectangle(colliderRect, maskRectangle);
        if(ifConvergence != null) //相交
        {
            landformCollider.enabled = false;
            Rectangle convergenceRect = ifConvergence.Value;
            List<Rectangle> soldierRects = MathCalulate.GetColliderSoldierArea(colliderRect, convergenceRect);

            for(int i =0;i<soldierRects.Count;i++)
            {
                if(!MathCalulate.ifRectCanIgnore(soldierRects[i]))
                {
                    SetSoldierCollider(soldierRects[i]);
                }
               
            }
        }
        else
        {
            landformCollider.enabled = true;
        }
    }

    void SetSoldierCollider(Rectangle rect)
    {
        GameObject soldier = Instantiate(colliderSoldier, Vector3.zero, Quaternion.identity) as GameObject;
        soldier.transform.position = new Vector2((rect.minX + rect.maxX) / 2, (rect.minY + rect.maxY) / 2);
        soldier.transform.localScale = new Vector2(rect.maxX - rect.minX,rect.maxY-rect.minY);
        colliderSoldiers.Add(soldier);
    }

    #endregion


}
