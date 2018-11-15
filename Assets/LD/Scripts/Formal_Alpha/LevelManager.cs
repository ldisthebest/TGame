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
    int[] cameraPosX;

    [SerializeField]
    Vector3[] maskSize;

    [SerializeField]
    Vector2[] playerbBeginPos;

    [SerializeField]
    Vector2[] passPos;

    [SerializeField]
    int moveSpeed;

    [SerializeField]
    Transform player;

    PlayerController2D controller;

    bool cameraMove;

    bool playerMove;

    int currentLevel;

    void Awake()
    {
        cameraMove = false;
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
            }
        }

        if(playerMove)
        {

        }
    }

    void SetPlayerAndMask()
    {
       // player.position = Vector2.MoveTowards(player.position, playerbBeginPos[currentLevel]);
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
