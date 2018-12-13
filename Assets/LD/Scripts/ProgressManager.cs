using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressManager : MonoBehaviour {

    [SerializeField]
    GameObject Menu;

    [SerializeField]
    Image[] textGuide;

    [SerializeField]
    string[] textImagePath;

    [SerializeField]
    Vector2[] playerTargetPos;

    int textIndex;

    Animation anima;

    PlayerController2D player;

    Mask mask;

	
	void Awake () {
        anima = GetComponent<Animation>();
        player = GameObject.FindWithTag("Player").GetComponent<PlayerController2D>();
        mask = GameObject.FindWithTag("Mask").GetComponent<Mask>();     
        player.canPlayerControl = false;

        //player.PassLevelEvent += PassLevel;
        player.OnMoveEvent += ShowMask;
        player.EndMoveEvent += EventOnLevel3;
        Time.timeScale = 3;
    }
	
	
	void ShowText(int index)
    {
        textGuide[index].gameObject.SetActive(true);
        textGuide[index].GetComponent<Animation>().Play("FadeUp");
        Destroy(textGuide[index].gameObject, 7);
    }


    public void StartGame()
    {
        Destroy(Menu);
        anima.Play("ViewDown");
        StartCoroutine(EventOnLevel0());
        
    }

    public void ExitGame()
    {
        Application.Quit();
    }


    IEnumerator EventOnLevel0()
    {
        yield return new WaitForSeconds(2.5f);             
        ShowText(0);
        yield return new WaitForSeconds(6);
        
        player.AutoMove(playerTargetPos[0]);
        textIndex++;
    }

    void PassLevel(Vector2 playerPos)
    {
        //HideText();
        //player.PassLevelEvent -= PassLevel;
    }

    void ShowMask(Vector2 playerPos)
    {
        if(playerPos.x > playerTargetPos[1].x)
        {
            player.OnMoveEvent -= ShowMask;
            player.StopMove();
            mask.MoveToNewLevel();
            ShowText(1);
            textIndex++;
            StartCoroutine(PLayerMoveAfterTextOver());
        }
       
    }

    IEnumerator PLayerMoveAfterTextOver()
    {
        yield return new WaitForSeconds(4);
        player.playerPause = false;
        player.AutoMove(playerTargetPos[2]);

    }

    void EventOnLevel3(Vector2 playerPos)
    {
        if(playerPos == playerTargetPos[3])
        {
            player.EndMoveEvent -= EventOnLevel3;
            ShowText(2);
        }
    }
}
