using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {


    Transform playerTransform;
    Vector2 targetPos;
    bool getDestination;
    [SerializeField]
    int lerpSpeed;
    
    void Awake()
    {
        playerTransform = transform;
        getDestination = true;
    }

    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            targetPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            getDestination = false;
            Debug.Log(targetPos);

        }
#if UNITY_EDITOR
        Debug.DrawLine(targetPos+Vector2.up*0.5f, targetPos + Vector2.down * 0.5f, Color.red);
        Debug.DrawLine(targetPos + Vector2.right * 0.5f, targetPos + Vector2.left * 0.5f, Color.red);
        
#endif
        //if (!getDestination)
        //{
        //    playerTransform.position = Vector2.MoveTowards(playerTransform.position, targetPos, lerpSpeed * Time.deltaTime);
        //    if((Vector2)playerTransform.position == targetPos)
        //    {
        //        getDestination = true;
        //    }
        //}


    }
}
