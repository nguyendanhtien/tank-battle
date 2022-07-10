using System;
using System.IO;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkController : MonoBehaviour
{
    [SerializeField]
    private string ipServer;
    private const int BUFFER_SIZE=1024;
    private const int PORT_NUMBER=5552;
    private ASCIIEncoding encoding= new ASCIIEncoding();
    private Stream stream;
    public static NetworkController instance;

    void Awake()
	{
		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy(gameObject);
		// DontDestroyOnLoad(gameObject);
	}

    // Start is called before the first frame update
    void Start()
    {
        try
        {
            IPAddress address = IPAddress.Parse(ipServer);
            TcpClient client = new TcpClient();
            client.Connect(address, PORT_NUMBER);
            stream = client.GetStream();
            Debug.Log("Connected");
        }
        catch (Exception ex)
        {
            Debug.Log("Error: " + ex);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void sendMoveData(float x, float y, float r) {
        StringWriter strWriter = new StringWriter();
        strWriter.Write($"MOVE:{x}-{y}-{r}");
        string strData = strWriter.ToString();
        Debug.Log(strData);
        byte[] data=encoding.GetBytes(strData);
        stream.Write(data,0,data.Length);
    }
}
