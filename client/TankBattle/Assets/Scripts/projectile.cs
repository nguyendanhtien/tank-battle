using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class projectile : MonoBehaviour
{
    public float speed;
    public float timetoDestroy;
    Rigidbody2D m_rb;

     /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        m_rb = GetComponent<Rigidbody2D>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
        // m_rb.AddForce(m_rb.transform.forward * speed);
        Destroy(gameObject, timetoDestroy);
    }

    // Update is called once per frame
    void Update()
    {
        // m_rb.velocity = Vector2.up * speed;
        m_rb.AddForce(m_rb.transform.up* speed);
    }
}
