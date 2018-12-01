using UnityEngine;


public enum Direction
{
    left,
    right
}

public class BoxInteraction : MonoBehaviour {

    #region 非序列化的私有字段

    private GameObject left;

    private GameObject right;

    private GameObject stop;

    private Vector3 distance;

    private Transform theTrans;

    private Transform player;

    private PlayerController2D playerController;

    #endregion

    #region 序列化的私有字段

    [SerializeField]
    private Transform boxTrans;

    #endregion

    #region 公有字段

    [HideInInspector]
    public Box TheBox;

    #endregion

    #region Mono
    // Use this for initialization
    void Start()
    {
        /*完成所有变量初始化*/        
        boxTrans = TheBox.transform;

        distance = new Vector3(-4.5f, -0.5f, 1.0f);

        theTrans.position = boxTrans.position - distance;

        player = TheBox.Player;

        playerController = player.GetComponent<PlayerController2D>();
    }

    private void Awake()
    {
        theTrans = transform;
        right = theTrans.Find("Right").gameObject; 
        left = theTrans.Find("Left").gameObject;
        stop = theTrans.Find("Stop").gameObject;
    }

    // Update is called once per frame
    void Update () {

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
        ChangeUIState(false);
        playerController.TheBox = TheBox;
        TheBox.IsMove = true;

        //判断推还是拉，并将结果存储到箱子里
        TheBox.IsPush = player.position.x < boxTrans.position.x ? false : true;

        /*调整主角方向为朝向箱子，必须要在箱子设为主角子物体之前*/
        //下面这一行是设置主角朝向箱子的方向
        //float toward = TheBox.IsPush ? player.position.x - 10 : player.position.x + 10;
        //这一行是设置主角方向与运动方向一致
        float toward = player.position.x - 10;
        playerController.SetPlayerTowards(toward);       

        boxTrans.SetParent(TheBox.Player);
        playerController.ChangeSpeed(PlayerState.Push);
        if(playerController.CalculateWithBox(Direction.left))
        {
            if (TheBox.IsPush)
                playerController.playerAction.SetPlayerAnimation(PlayerState.Push);
            else
                playerController.playerAction.SetPlayerAnimation(PlayerState.Pull);
        }
    }

    public void RightMove()
    {
        ChangeUIState(false);
        playerController.TheBox = TheBox;
        TheBox.IsMove = true;
        //判断推还是拉，并将结果存储到箱子里
        TheBox.IsPush = player.position.x < boxTrans.position.x ? true : false;

        /*调整主角方向为朝向箱子，必须要在箱子设为主角子物体之前*/
        //下面这一行是设置主角朝向箱子的方向
        //float toward = TheBox.IsPush ? player.position.x + 10 : player.position.x - 10;
        //这一行是设置主角方向与运动方向一致
        float toward = player.position.x + 10;
        playerController.SetPlayerTowards(toward);

        boxTrans.SetParent(TheBox.Player);
        playerController.ChangeSpeed(PlayerState.Push);
        if (playerController.CalculateWithBox(Direction.right))
        {
            if (TheBox.IsPush)
                playerController.playerAction.SetPlayerAnimation(PlayerState.Push);
            else
                playerController.playerAction.SetPlayerAnimation(PlayerState.Pull);
        }
    }

    public void EndMove()
    {
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
        stop.SetActive(!directionButton);
    }
    #endregion

    #region 设置layer
    public void SetLayer(string layerName)
    {
        gameObject.layer = LayerMask.NameToLayer(layerName);
        for (int i = 0; i < theTrans.childCount; i++)
        {
            theTrans.GetChild(i).gameObject.layer = LayerMask.NameToLayer(layerName);
        }
    }
    #endregion
}
