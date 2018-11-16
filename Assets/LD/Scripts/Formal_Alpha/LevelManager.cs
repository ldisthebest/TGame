using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour {

    private static LevelManager instance;

    public static LevelManager Instance
    {
        get
        {
            return instance;
        }
    }

    [SerializeField]
    Transform cameras;

    [SerializeField]
    Transform mask;

    [SerializeField]
    int[] cameraPosX;

    [SerializeField]
    Vector3[] maskSize;

    [SerializeField]
    Vector2[] playerBeginPos; 

    [SerializeField]
    Vector2[] passPos;

    [SerializeField]
    int moveSpeed;

    [SerializeField]
    int playerSpeed;

    [SerializeField]
    Transform player;

    PlayerController2D controller;

    bool cameraMove;

    bool playerMove;

    bool maskMove;

    int currentLevel;

    void Awake()
    {
        cameraMove = false;
        maskMove = false;
        currentLevel = 0;
        instance = this;
        controller = player.GetComponent<PlayerController2D>();
    }

    void Update()   
    {
        if(cameraMove)
        {
            cameras.position = Vector2.MoveTowards(cameras.position, new Vector2(cameraPosX[currentLevel], 0), Time.deltaTime * moveSpeed);
            if (cameras.position.x == cameraPosX[currentLevel])
            {
                cameraMove = false;
                playerMove = true;
                
                controller.GetComponent<PlayerAction>().SetPlayerAnimation(PlayerState.Run);
                controller.enabled = false;
            }
        }

        if(playerMove)
        {
            SetPlayer();
        }

        if(maskMove)
        {
            SetMask();
        }
    }

    void SetMask()
    {
       
        mask.position = Vector3.MoveTowards(mask.position, new Vector3(cameraPosX[currentLevel] + 6.5f, 3.5f, -1), Time.deltaTime * moveSpeed);
        if (mask.position == new Vector3(cameraPosX[currentLevel] + 6.5f, 3.5f, -1))
        {
            controller.enabled = true;
            mask.GetComponent<Mask>().enabled = true;
            maskMove = false;
        }
    }

    void SetPlayer()
    {
        
        player.position = Vector2.MoveTowards(player.position, playerBeginPos[currentLevel],Time.deltaTime* playerSpeed);
        
        if ((Vector2)player.position == playerBeginPos[currentLevel])
        {
            //mask.position = new Vector3(cameraPosX[currentLevel] + 6.5f, 3.5f,-1);
            controller.GetComponent<PlayerAction>().SetPlayerAnimation(PlayerState.Idel);
            
            playerMove = false;
            maskMove = true;
            mask.position = new Vector3(cameraPosX[currentLevel] + 12.5f, 9.5f, -1);
            mask.GetComponent<Mask>().enabled = false;
        }
    }
    public void CanPassLevel()
    {
        if((Vector2)player.position == passPos[currentLevel])
        {
            if(currentLevel == passPos.Length - 1)
            {
                GameOver();
                return;
            }
            cameraMove = true;
            currentLevel++;
        }
    }

    void GameOver()
    {

    }

}
