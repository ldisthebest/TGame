using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DragonBones;

public class DragonTest : MonoBehaviour {

    // Use this for initialization
    UnityArmatureComponent anima;
	void Start () {
        anima = GetComponent<UnityArmatureComponent>();
        //Debug.Log(anima.animationName);

    }
	
	// Update is called once per frame
	void Update () {
		if(Input.GetMouseButtonDown(1))
        {
            //Debug.Log(anima.animation.lastAnimationName);
            if (anima.animation.lastAnimationName != "走")
            {
                anima.animation.timeScale = 2;
                anima.animation.Play("走");
                Debug.Log("haha");
            
              }
            
            
        }
	}
}
