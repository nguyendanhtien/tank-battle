using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player2 : Player1 
{
    void Awake() {
        posX = 7.5f;
        posY = 2.8f;
        rot = 180f;
        m_rb = GetComponent<Rigidbody2D>();
    }
    
    // Start is called before the first frame update
    void Start()
    {
        m_gc = FindObjectOfType<GameController>();
        m_powerElapsed =  0;
        m_hp = 50;
        // posX = 7.5f;
        // posY = 3.5f;
        // rot = 180f;

        healthBar.SetMaxHealth(m_hp);
        manaBar.SetMaxMana(10);
    }

    
}
