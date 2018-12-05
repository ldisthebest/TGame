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

    private GameObject left;

    private GameObject right;

    private Transform theTrans;

    private Transform player;

    private PlayerController2D playerController;

    private Transform boxTrans;

    #endregion

    #region 公有字段

    [HideInInspector]
    public Box TheBox;

    #endregion

    #region Mono
    public void Init(Box box)
    {
        TheBox = box;

        boxTrans = box.transform;

        player = TheBox.Player;

        playerController = player.GetComponent<PlayerController2D>();

        theTrans = transform;

        right = transform.Find("Right").gameObject;

        left = transform.Find("Left").gameObject;

        transform.SetParent(GameObject.FindWithTag("WorldCanvas").transform);

        theTrans.position = boxTrans.position - distance;
    }

    private void OnEnable()
    {
        ChangeUIState(true);
    }
    #endregion

    #region ClickEvent
    public void LeftMove()
    {
        //判断推还是拉，并将结果存储到箱子里
        TheBox.IsPush = player.position.x < boxTrans.position.x ? false : true;

        playerController.TheBox = TheBox;

        if (!playerController.CalculateWithBox(Direction.left,TheBox.GetBoxPos()))
        {
            return;
        }
        ChangeUIState(false);
        
        TheBox.IsMove = true;

        playerController.SetPlayerTowards(Direction.left);       

        boxTrans.SetParent(TheBox.Player);
        playerController.ChangeSpeed(PlayerState.Push);
        if (TheBox.IsPush)
            playerController.playerAction.SetPlayerAnimation(PlayerState.Push);
        else
            playerController.playerAction.SetPlayerAnimation(PlayerState.Pull);
    }

    public void RightMove()
    {
        //判断推还是拉，并将结果存储到箱子里
        TheBox.IsPush = player.position.x < boxTrans.position.x ? true : false;
        playerController.TheBox = TheBox;

        if (!playerController.CalculateWithBox(Direction.right,TheBox.GetBoxPos()))
        {
            return;
        }
        ChangeUIState(false);
       
        TheBox.IsMove = true;
        

        playerController.SetPlayerTowards(Direction.right);

        boxTrans.SetParent(TheBox.Player);
        playerController.ChangeSpeed(PlayerState.Push);

        if (TheBox.IsPush)
            playerController.playerAction.SetPlayerAnimation(PlayerState.Push);
        else
            playerController.playerAction.SetPlayerAnimation(PlayerState.Pull);

    }

    public void EndMove()
    {
        theTrans.position = boxTrans.position - distance;
        ChangeUIState(true);
        TheBox.IsMove = false;
        boxTrans.SetParent(null);
        playerController.TheBox = null;
        playerController.ChangeSpeed(PlayerState.Run);
        TheBox.DropCheck();
    }

    void ChangeUIState(bool directionButton)
    {
        right.SetActive(directionButton);
        left.SetActive(directionButton);
    }


    #endregion

    #region 设置一些属性，外部调用
    public void SetLayer(string layerName)
    {
        gameObject.layer = LayerMask.NameToLayer(layerName);
        for (int i = 0; i < theTrans.childCount; i++)
        {
            theTrans.GetChild(i).gameObject.layer = LayerMask.NameToLayer(layerName);
        }
    }

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
