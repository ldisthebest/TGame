using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaskCollider : MonoBehaviour {

    const string colliderSoldierPath = "Prefabs/OutsideCollider";

    [SerializeField]
    List<BoxCollider2D> insideLandform;
    [SerializeField]
    List<BoxCollider2D> outsideLandform;

    Rectangle[] insideBox;
    Rectangle[] outsideBox;

    Object colliderSoldier;
    List<GameObject> colliderSoldiers; 
    void Awake()
    {
        insideBox = new Rectangle[insideLandform.Count];
        outsideBox = new Rectangle[outsideLandform.Count];
        colliderSoldiers = new List<GameObject>();
        InitRectangle();
        colliderSoldier = Resources.Load(colliderSoldierPath, typeof(GameObject));
    }

    void ClearColliderSoldiers()
    {
        for(int i = 0;i<colliderSoldiers.Count;i++)
        {
            Destroy(colliderSoldiers[i]);
        }
        colliderSoldiers.Clear();
    }

    void InitRectangle()
    {
        for(int i = 0;i<insideBox.Length;i++)
        {
            Bounds colliderBounds = insideLandform[i].bounds;
            insideBox[i].minX = colliderBounds.min.x;
            insideBox[i].minY = colliderBounds.min.y;
            insideBox[i].maxX = colliderBounds.max.x;
            insideBox[i].maxY = colliderBounds.max.y;
            insideLandform[i].enabled = false;//游戏开始先禁用底片世界的碰撞体
        }
        for(int i= 0;i<outsideBox.Length;i++)
        {
            Bounds bounds = outsideLandform[i].bounds;
            outsideBox[i].minX = bounds.min.x;
            outsideBox[i].maxX = bounds.max.x;
            outsideBox[i].minY = bounds.min.y;
            outsideBox[i].maxY = bounds.max.y;
        }
    }

    public void UpdateLandformCollider(Rectangle maskRectangle)
    {
        ClearColliderSoldiers();
        for (int i = 0; i < insideBox.Length; i++)
        {
            Rectangle? ConvergenceRect = MathCalulate.ConvergenceRectangle(insideBox[i], maskRectangle);
            if (ConvergenceRect != null)//如果相交改变碰撞体位置和大小
            {
                Rectangle rect = ConvergenceRect.Value;
                SetColliderBounds(insideLandform[i], insideBox[i], rect);
            }
            else
            {
                insideLandform[i].enabled = false; //否则隐藏底片世界的碰撞体
            }
        }

        for(int i = 0; i< outsideBox.Length;i++)
        {
            SetOutsideColliderPos(outsideLandform[i], outsideBox[i], maskRectangle);
        }
    }



    void SetColliderBounds(BoxCollider2D collider,Rectangle colliderRect,Rectangle rect)
    {
        collider.enabled = true;
        Vector2 landformScale = collider.transform.localScale;
        //化简得到
        float offosetX = (rect.minX + rect.maxX - colliderRect.minX - colliderRect.maxX) / landformScale.x / 2;
        float offosetY = (rect.minY + rect.maxY - colliderRect.minY - colliderRect.maxY) / landformScale.y / 2;
        collider.offset = new Vector2(offosetX, offosetY);

        float sizeX = collider.size.x;
        sizeX *= (rect.maxX - rect.minX) / 2 / collider.bounds.extents.x;
        float sizeY = collider.size.y;
        sizeY *= (rect.maxY - rect.minY) / 2 / collider.bounds.extents.y;
        collider.size = new Vector2(sizeX, sizeY);
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
                SetSoldierCollider(soldierRects[i]);
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

//    void Update()
//    {
//#if UNITY_EDITOR
//        //for (int i = 0; i < insideBox.Length; i++)
//        //{
//        //    Debug.DrawLine(new Vector2(insideBox[i].minX, insideBox[i].minY), new Vector2(insideBox[i].minX, insideBox[i].maxY), Color.red);
//        //    Debug.DrawLine(new Vector2(insideBox[i].minX, insideBox[i].maxY), new Vector2(insideBox[i].maxX, insideBox[i].maxY), Color.red);
//        //    Debug.DrawLine(new Vector2(insideBox[i].maxX, insideBox[i].maxY), new Vector2(insideBox[i].maxX, insideBox[i].minY), Color.red);
//        //    Debug.DrawLine(new Vector2(insideBox[i].maxX, insideBox[i].minY), new Vector2(insideBox[i].minX, insideBox[i].minY), Color.red);
//        //}

//        Rectangle maskRectangle = GetComponent<Mask>().GetMaskContour();
//        Rectangle? rect = MathCalulate.ConvergenceRectangle(insideBox[0], maskRectangle);
//        if(rect != null)
//        {
//            Debug.DrawLine(new Vector2(rect.Value.minX, rect.Value.minY), new Vector2(rect.Value.minX, rect.Value.maxY), Color.red);
//            Debug.DrawLine(new Vector2(rect.Value.minX, rect.Value.maxY), new Vector2(rect.Value.maxX, rect.Value.maxY), Color.red);
//            Debug.DrawLine(new Vector2(rect.Value.maxX, rect.Value.maxY), new Vector2(rect.Value.maxX, rect.Value.minY), Color.red);
//            Debug.DrawLine(new Vector2(rect.Value.maxX, rect.Value.minY), new Vector2(rect.Value.minX, rect.Value.minY), Color.red);
//        }


//#endif
//    }
}
