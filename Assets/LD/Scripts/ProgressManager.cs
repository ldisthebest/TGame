using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressManager : MonoBehaviour {

    [SerializeField]
    TitleParticleControl title;

    [SerializeField]
    GameObject menu;

    [SerializeField]
    Image[] textGuide;

    [SerializeField]
    string[] textImagePath;

    [SerializeField]
    Vector2[] playerTargetPos;

    int textIndex;

    Animation anima;

    PlayerController2D player;

    LevelManager levelmanager;

    Mask mask;

	
	void Awake () {
        anima = GetComponent<Animation>();
        player = GameObject.FindWithTag("Player").GetComponent<PlayerController2D>();
        mask = GameObject.FindWithTag("Mask").GetComponent<Mask>();
        levelmanager = GetComponent<LevelManager>();
        //player.canPlayerControl = false;

        //player.PassLevelEvent += PassLevel;
        //player.OnMoveEvent += ShowMask;
        player.EndMoveEvent += EventOnLevel3;
        //Time.timeScale = 3;
    }
	
	
	void ShowText(int index)
    {
        textGuide[index].gameObject.SetActive(true);
        textGuide[index].GetComponent<Animation>().Play("FadeUp");
        Destroy(textGuide[index].gameObject, 7);
    }


    public void StartGame()
    {
        title.Action();
        Destroy(title.gameObject,10);
        anima.Play("ViewDown");
        Destroy(menu);
        StartCoroutine(EventOnLevel0());
        
    }

    public void ExitGame()
    {
        Application.Quit();
    }


    IEnumerator EventOnLevel0()
    {
        yield return new WaitForSeconds(2.5f);
        levelmanager.SnowMoveOutParent();
        ShowText(0);
       
        textIndex++;
    }


    //void ShowMask(Vector2 playerPos)
    //{
    //    if(playerPos.x > playerTargetPos[1].x)
    //    {
    //        player.OnMoveEvent -= ShowMask;
    //        player.StopMove();
    //        mask.MoveToNewLevel();
    //        ShowText(1);
    //        textIndex++;
    //        StartCoroutine(PLayerMoveAfterTextOver());
    //    }
       
    //}

    //IEnumerator PLayerMoveAfterTextOver()
    //{
    //    yield return new WaitForSeconds(4);
    //    player.playerPause = false;
    //    player.AutoMove(playerTargetPos[2]);

    //}

    void EventOnLevel3(Vector2 playerPos)
    {
        if (textIndex == 0) return;
        if(playerPos.x == playerTargetPos[textIndex-1].x)
        {
            //player.EndMoveEvent -= EventOnLevel3;
            ShowText(textIndex);
            textIndex++;
            if(textIndex == textGuide.Length)
            {
                player.EndMoveEvent -= EventOnLevel3;
            }
        }
    }
}
