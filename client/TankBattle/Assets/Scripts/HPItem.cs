using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPItem : MonoBehaviour
{

    GameController m_gc;

    // Start is called before the first frame update
    void Start()
    {
        m_gc = FindObjectOfType<GameController>();

        
    }

    void Update(){
        // Debug.Log(transform.position.x +"-"+transform.position.y);
    }

    /// <summary>
    /// Sent when an incoming collider makes contact with this object's
    /// collider (2D physics only).
    /// </summary>
    /// <param name="other">The Collision2D data associated with this collision.</param>
    void OnTriggerEnter2D(Collider2D col)
    {
        if(col.CompareTag("Player")){
            Destroy(gameObject);
            Debug.Log("hitted");
            m_gc.getPlayer().IncreaseHP(5);
        }
    }

    // Update is called once per frame
}
