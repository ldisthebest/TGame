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
	


    public void Action()
    {
        particleSystem.Play();
        Destroy(title, 0.05f);
    }
}
