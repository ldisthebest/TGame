using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawLine : MonoBehaviour {

    public Material mat;

	// Use this for initialization
	void Awake () {

        Debug.Log("begin");

        GL.PushMatrix();
        mat.SetPass(0);
        GL.LoadPixelMatrix();//设置pixelMatrix 
        GL.Color(Color.yellow);
        GL.Begin(GL.LINES);

        for(int i=-150;i<=50;i++)
        {
            GL.Vertex3(i, 5f, 0);
            GL.Vertex3(i, -5f, 0);
        }

        GL.End();
        GL.PopMatrix();

        Debug.Log("end");
	}

}
