using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxInteraction : MonoBehaviour {

    [HideInInspector]
    public Box TheBox;

    public GameObject Left;
    public GameObject Right;
    public GameObject Stop;

    private Vector3 distance;

    private Transform theTrans;

	// Use this for initialization
	void Start () {

        theTrans = transform;

        distance = new Vector3(-4.5f, -0.5f, 1.0f);
        
        theTrans.position = TheBox.transform.position - distance;
	}
	
	// Update is called once per frame
	void Update () {

        theTrans.position = TheBox.transform.position - distance;

	}

    private void OnEnable()
    {
        ChangeUIState(true);
    }

    public void LeftMove()
    {
        Transform player = TheBox.Player.transform;
        ChangeUIState(false);
        TheBox.IsMove = true;

        //判断推还是拉，并将结果存储到箱子里
        TheBox.IsPush = player.position.x < TheBox.transform.position.x ? false : true;

        /*调整主角方向为朝向箱子，必须要在箱子设为主角子物体之前*/
        //下面这一行是设置主角朝向箱子的方向
        //float toward = TheBox.IsPush ? player.position.x - 10 : player.position.x + 10;
        //这一行是设置主角方向与运动方向一致
        float toward = player.position.x - 10;
        TheBox.Player.GetComponent<PlayerController2D>().SetPlayerTowards(toward);

        TheBox.transform.SetParent(TheBox.Player);
        TheBox.Player.GetComponent<PlayerController2D>().CalculateWithBox(false);
    }

    public void RightMove()
    {
        Transform player = TheBox.Player.transform;
        ChangeUIState(false);
        TheBox.IsMove = true;
        //判断推还是拉，并将结果存储到箱子里
        TheBox.IsPush = player.position.x < TheBox.transform.position.x ? true : false;

        /*调整主角方向为朝向箱子，必须要在箱子设为主角子物体之前*/
        //下面这一行是设置主角朝向箱子的方向
        //float toward = TheBox.IsPush ? player.position.x + 10 : player.position.x - 10;
        //这一行是设置主角方向与运动方向一致
        float toward = player.position.x + 10;
        TheBox.Player.GetComponent<PlayerController2D>().SetPlayerTowards(toward);

        TheBox.transform.SetParent(TheBox.Player);
        TheBox.Player.GetComponent<PlayerController2D>().CalculateWithBox(true);
    }

    public void EndMove()
    {
        ChangeUIState(true);
        TheBox.IsMove = false;
        TheBox.transform.SetParent(null);
        TheBox.Drop();
    }

    void ChangeUIState(bool directionButton)
    {
        Right.SetActive(directionButton);
        Left.SetActive(directionButton);
        Stop.SetActive(!directionButton);
    }
}
