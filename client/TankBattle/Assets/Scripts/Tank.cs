using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tank : MonoBehaviour
{
    private float moveSpeed = 4;
    float xDirection, yDirection;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        xDirection = Input.GetAxisRaw("Horizontal");
        yDirection = Input.GetAxisRaw("Vertical");

        float xmoveStep = moveSpeed*xDirection*Time.deltaTime;
        float ymoveStep = moveSpeed*yDirection*Time.deltaTime;

        transform.position += new Vector3(xmoveStep, ymoveStep,0);
    }
}
