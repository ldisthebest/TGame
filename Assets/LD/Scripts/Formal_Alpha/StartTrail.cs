using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartTrail : MonoBehaviour {

    Transform[] childStar;
    SpriteRenderer[] trailRender;
 
    [SerializeField]
    int[] rotateSpeed;

    [SerializeField]
    float[] trailFadeSpeed;

    bool[] fadeUp;

    private void Awake()
    {
        childStar = new Transform[3];
        trailRender = new SpriteRenderer[3];
        fadeUp = new bool[3];
        for (int i =0;i<3;i++)
        {
            childStar[i] = transform.GetChild(i);
            trailRender[i] = childStar[i].GetChild(0).GetComponent<SpriteRenderer>();
            fadeUp[i] = false;
        }      


    }
    void LateUpdate () {
		for(int i = 0; i < 3; i++)
        {
            childStar[i].Rotate(0, 0, Time.deltaTime * rotateSpeed[i]);
            UpdateFade(trailRender[i], trailFadeSpeed[i],ref fadeUp[i]);
        }
	}

    void UpdateFade(SpriteRenderer render,float speed,ref bool fadeUp)
    {

        if(fadeUp)
        {
           
            render.color += new Color(0, 0, 0, Time.deltaTime * speed/10);
            if (render.color.a >= 1)
            {
                render.color = new Color(1, 1, 1, 1);
                fadeUp = false;
            }
        }
        else
        {
            render.color -= new Color(0, 0, 0, Time.deltaTime * speed/10);
            if(render.color.a <= 0)
            {
                render.color = new Color(1, 1, 1, 0);
                fadeUp = true;
            }
        }
        
    }
}
