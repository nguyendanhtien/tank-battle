using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player2 : Player1 
{
    
    
    // Start is called before the first frame update
    void Start()
    {
        m_attack= 1;
        m_hp = 50;
        powerEllapse = 0;
        posX = 7.5f;
        posY = 3.5f;
        rot = 180f;
    }

    
}
