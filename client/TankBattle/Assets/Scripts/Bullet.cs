using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bullet : MonoBehaviour
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
    void Update()
    {
        m_rb.AddForce(m_rb.transform.up* speed);
    }
}
