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

    List<Rectangle> insideBox;

    List<Rectangle> outsideBox;

    List<GameObject> colliderSoldiers;

    Object colliderSoldier;

    #endregion

    #region 初始化或者更新碰撞体列表

    public void InitWorldColliders(Rectangle maskRectangle)
    {
        colliderSoldier = Resources.Load(colliderSoldierPath, typeof(GameObject));
        colliderSoldiers = new List<GameObject>();
        insideBox = new List<Rectangle>();
        outsideBox = new List<Rectangle>();

        InitRectangle();

        UpdateLandformCollider(maskRectangle);
    }

    void InitRectangle()
    {
        for (int i = 0; i < insideLandform.Count; i++)
        {
            Bounds colliderBounds = insideLandform[i].bounds;
            Rectangle rect;
            rect.minX = colliderBounds.min.x;
            rect.minY = colliderBounds.min.y;
            rect.maxX = colliderBounds.max.x;
            rect.maxY = colliderBounds.max.y;
            insideLandform[i].enabled = false;//游戏开始先禁用底片世界的碰撞体
            insideBox.Add(rect);
        }
        for (int i = 0; i < outsideLandform.Count; i++)
        {
            Bounds bounds = outsideLandform[i].bounds;
            Rectangle rect;
            rect.minX = bounds.min.x;
            rect.maxX = bounds.max.x;
            rect.minY = bounds.min.y;
            rect.maxY = bounds.max.y;
            outsideBox.Add(rect);
        }
    }
     
    #endregion

    #region 切割碰撞体逻辑

    public void UpdateLandformCollider(Rectangle maskRectangle)
    {
        ClearColliderSoldiers();
        for (int i = 0; i < insideBox.Count; i++)
        {
            Rectangle? ConvergenceRect = MathCalulate.ConvergenceRectangle(insideBox[i], maskRectangle);
            if (ConvergenceRect != null && !MathCalulate.ifRectCanIgnore(ConvergenceRect.Value))//如果相交生成哨兵碰撞体
            {
                SetInsideCollider(insideLandform[i], insideBox[i], ConvergenceRect.Value);
            }
        }

        for(int i = 0; i< outsideBox.Count;i++)
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

    #region 更新碰撞体列表

    public void UpdateColliderList(Transform level)
    {
        Transform outsideParent = level.GetChild(0).GetChild(0);
        if(level.GetChild(0).childCount > 1)
        {
            Transform insideParent = level.GetChild(0).GetChild(1);
            InitColliderList(insideParent, insideLandform);
        }    
        InitColliderList(outsideParent, outsideLandform);      
        insideBox.Clear();
        outsideBox.Clear();
        ClearColliderSoldiers();
        InitRectangle();
    }

    void InitColliderList(Transform parent,List<BoxCollider2D> landform)
    {
        landform.Clear();
        for (int i = 0; i < parent.childCount; i++)
        {
            landform.Add(parent.GetChild(i).GetComponent<BoxCollider2D>());
        }
    }
    #endregion

}
