using UnityEngine;

public enum Direction
{
    left,
    right
}

public class BoxInteraction : MonoBehaviour {

    #region 静态变量
    private static Vector3 distance = new Vector3(-4.5f, -0.5f, 1.0f);
    #endregion

    #region 非序列化的私有字段
    private Transform theTrans;

    Transform boxTrans;

    Box TheBox;
    #endregion

    #region 初始化，外部调用
    public void Init(Box box)
    {
        TheBox = box;

        boxTrans = box.transform;

        theTrans = transform;

        transform.SetParent(GameObject.FindWithTag("WorldCanvas").transform);

        theTrans.position = boxTrans.position - distance;
    }
    #endregion

    #region ClickEvent

    public void LeftButtonClickEvent()
    {
        if(TheBox.CanBoxLeftMove())
        {
            SetUI(false);
        }       
    }

    public void RightButtonClickEvent()
    {
        if(TheBox.CanBoxRightMove())
        {
            SetUI(false);
        }    

    }

    #endregion

    #region 设置一些属性
    public void SetUI(bool active)
    {
        if(active)
        {
            theTrans.position = boxTrans.position - distance;
        }
        gameObject.SetActive(active);
    }

    #endregion
}
