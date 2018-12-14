using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LevelManager : MonoBehaviour {

    enum LevelMode
    {
        release = 0,
        debug
    }

    #region 常量
    const float intervalTime = 0.01f;
    #endregion

    #region 序列化的字段
    [SerializeField]
    string[] levelPrefabPath;

    [SerializeField]
    Vector2[] cameraPos;

    [SerializeField]
    Vector2[] playerBeginPos;

    [SerializeField]
    Vector2[] playerPassPos;

    [SerializeField]
    Vector2[] maskBodySize;

    [SerializeField]
    Vector2[] maskBorderSize;

    [SerializeField]
    float[] inHalfSize;

    [SerializeField]
    float cameraMoveSpeed;

    [SerializeField]
    int currentLevel;

    [SerializeField]
    LevelMode levelmode;

    #endregion

    #region 非序列化的字段

    Camera mainCamera;

    PlayerController2D player;

    Mask mask;

    MaskCollider colliderManager;

    GameObject nowlevel,pastlevel;

    #endregion


    private void Start()
    {
        mainCamera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        player = GameObject.FindWithTag("Player").GetComponent<PlayerController2D>();
        mask = GameObject.FindWithTag("Mask").GetComponent<Mask>();
        colliderManager = mask.GetComponent<MaskCollider>();
        player.PassLevelEvent += PassLevel;

        
        player.SetInitialPos(playerBeginPos[currentLevel]);

        InitSceneItemPos();     
    }

    void InitSceneItemPos()
    {
        
        if (levelmode == LevelMode.debug)
        {
            transform.position = cameraPos[currentLevel];
            MathCalulate.UpdateScreeenRect(mainCamera);
            if (currentLevel != 0)
            {
                mask.SetMaskPosAtScreenVertex();
                //mask.SetMaskSize(Vector2.zero,Vector2.zero);
                mask.SetMaskSize(maskBodySize[currentLevel], maskBorderSize[currentLevel], inHalfSize[currentLevel]);
            }
        }
        
        if(levelmode == LevelMode.release)
        {
            LoadNextLevel();
            colliderManager.UpdateColliderList(nowlevel.transform);
        }

       
    }

    void PassLevel(Vector2 playerPos)
    {
        if(playerPos == playerPassPos[currentLevel])
        {
                     
            //到达章节最后一关
            if (currentLevel == levelPrefabPath.Length - 1)
            {
                Debug.Log("Game Over");
                return;
            }

            //过关
            currentLevel++;

            if(levelmode == LevelMode.release)
            {
                LoadNextLevel();
                colliderManager.UpdateColliderList(nowlevel.transform);
            }

            StartCoroutine(CameraMove());
            
        }
    }


    IEnumerator CameraMove()
    {      
        Vector2 targetPos = cameraPos[currentLevel];
        while ((Vector2)transform.position != cameraPos[currentLevel])
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPos, Time.deltaTime * cameraMoveSpeed);
            yield return new WaitForSeconds(intervalTime);
        }
        MathCalulate.UpdateScreeenRect(mainCamera);
        player.AutoMove(playerBeginPos[currentLevel]);
        if (currentLevel >= 2)
        {
            mask.SetMaskSize(maskBodySize[currentLevel],maskBorderSize[currentLevel],inHalfSize[currentLevel]);
            mask.MoveToNewLevel();
        }
       
        if(levelmode == LevelMode.release)
        {
            UnloadPastLevel();
        }
    }

    void LoadNextLevel()
    {
        pastlevel = nowlevel;
        GameObject level = Resources.Load<GameObject>(levelPrefabPath[currentLevel]);
        nowlevel = Instantiate(level, level.transform.position, Quaternion.identity);
        
    }

    void UnloadPastLevel()
    {
        if (pastlevel != null)
        {
            Destroy(pastlevel);
        }
    }

}
