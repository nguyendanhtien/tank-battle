using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Tank 
{
    // Start is called before the first frame update
    void Start()
    {
        id  = 2;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void destroy(){
        Destroy(gameObject);
    }

    public void moveNewPos(float posX, float posY, float rotation){
        transform.position = new Vector3(posX, posY, 0);
        // transform.position.x += posX;
        // transform.position.y += posY;
        transform.rotation = Quaternion.Euler(transform.rotation.x, transform.rotation.y, rotation);
        // transform.rotation.z += rotation;
    }
}
