using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapNavigation : MonoBehaviour{

    private MapNavigation instance;
    public MapNavigation Instance
    {
        get
        {
            return instance;
        }
    }

    [SerializeField]
    List<FlatBoxStep> steps;

   // List<float> stepX = new List<float>();
    float[] stepX;

    void Awake()
    {
        instance = this;
        stepX = new float[steps.Count * 2];

        //test
        float[] a = { 77, 3, 23, 21, 12, 56, 12, 97, 88, 12, 44 };
        MathCalulate.BubbleSort(a);
        for(int i = 0;i<a.Length;i++)
        {
            Debug.Log(a[i]);
        }

        Debug.Log(MathCalulate.GetInsertIndexOfMin(a, 12));
    }

    

    void SortByStepX()
    {
        float[] x = new float[steps.Count*2];
        //for(int )
    }

    public Vector2 GetRightTargetPos(Vector2 hitPos)
    {
        return Vector2.zero;
    }
    
}
