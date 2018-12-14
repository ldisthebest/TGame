using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleParticleControl : MonoBehaviour {

    [SerializeField]
    ParticleSystem particleSystem;
    [SerializeField]
    GameObject title;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
        if(Input.GetMouseButtonDown(0))
        {
            Action();
        }
	}


    private void Action()
    {
        particleSystem.Play();
        Destroy(title, 0.65f);
    }
}
