using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletLv2Item : MonoBehaviour
{
    GameController m_gc;
    // Start is called before the first frame update
    void Start()
    {
        m_gc = FindObjectOfType<GameController>();
    }


    // void OnTriggerEnter2D(Collider2D col)
    // {
    //     if(col.CompareTag("Player")){
    //         Destroy(gameObject);
    //         // Debug.Log("hitted");
    //         // m_gc.getPlayer().EatBulletItem();
    //     }
    // }

    // Update is called once per frame
    void Update()
    {
        
    }
}
