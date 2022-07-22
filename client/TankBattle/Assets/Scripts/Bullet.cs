using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed;
    public float timetoDestroy;
    Rigidbody2D m_rb;

    void Awake()
    {
        m_rb = GetComponent<Rigidbody2D>();
    }

    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, timetoDestroy);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        m_rb.AddForce(m_rb.transform.up* speed);
        // if(transform.position.y >160){
        //     Destroy(gameObject);
        // }
    }

    private void OnCollisionEnter2D(Collision2D col) {
        if( col.gameObject.CompareTag("StaticObject")) {
            // Debug.Log("Bullet Hit Obstacle");
            Destroy(gameObject);
        }
        if(col.gameObject.CompareTag("hpItem")) {
            Physics2D.IgnoreCollision(col.gameObject.GetComponent<BoxCollider2D>(), GetComponent<CircleCollider2D>());
        }

        if(col.gameObject.CompareTag("powerItem")) {
            Physics2D.IgnoreCollision(col.gameObject.GetComponent<BoxCollider2D>(), GetComponent<CircleCollider2D>());
        }
    }
}
