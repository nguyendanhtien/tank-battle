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

    UIManager m_ui;

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
        m_ui = FindObjectOfType<UIManager>();
        try
        {
            IPAddress address = IPAddress.Parse(ipServer);
            TcpClient client = new TcpClient();
            client.Connect(address, PORT_NUMBER);
            stream = client.GetStream();
            StartCoroutine(GetServerMessage((message) => {
                LogMessageFromServer(message);
            }));
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

    public void sendCreateRoomRequest() {
        StringWriter strWriter = new StringWriter();
        strWriter.Write("CREA");
        string strData = strWriter.ToString();
        Debug.Log(strData);
        byte[] data=encoding.GetBytes(strData);
        stream.Write(data,0,data.Length);
        // Waiting for data
        int roomId = 0;
        StartCoroutine(GetRoomId((roomIdFromServer) => {
            roomId = roomIdFromServer;
            LogMessageFromServer(roomId.ToString());
            m_ui.ShowCreateRoomGUI(true, roomId);
        }));
    }

    public void sendCancelWaitingRequest() {
        StringWriter strWriter = new StringWriter();
        strWriter.Write("DROP");
        string strData = strWriter.ToString();
        Debug.Log(strData);
        byte[] data=encoding.GetBytes(strData);
        stream.Write(data,0,data.Length);
        // Waiting for data
        StartCoroutine(GetServerMessage((message) => {
            LogMessageFromServer(message);
            m_ui.ShowHomeGUI(true);
        }));
    }

    public void sendJoinRoomRequest(int roomId) {
        StringWriter strWriter = new StringWriter();
        strWriter.Write($"JOIN:{roomId}");
        string strData = strWriter.ToString();
        Debug.Log(strData);
        byte[] data=encoding.GetBytes(strData);
        stream.Write(data,0,data.Length);
    }

    IEnumerator GetRoomId(System.Action<int> callbackOnFinish) {
        yield return new WaitForSeconds(Time.deltaTime);
        // Receive data
        byte[] data = new byte[BUFFER_SIZE];
        stream.Read(data,0,BUFFER_SIZE);
        string strData = encoding.GetString(data);
        // Read data
        StringReader strReader = new StringReader(strData.Substring(5));
        // Process room id
        int roomId = Int16.Parse(strReader.ReadLine());
        // Debug.Log($"Server: {roomId}");
        callbackOnFinish(roomId);
        
    }

    IEnumerator GetServerMessage(System.Action<string> callbackOnFinish) {
        yield return new WaitForSeconds(Time.deltaTime);
        // Receive data
        byte[] data = new byte[BUFFER_SIZE];
        stream.Read(data,0,BUFFER_SIZE);
        string strData = encoding.GetString(data);
        // Read data
        StringReader strReader = new StringReader(strData.Substring(5));
        callbackOnFinish(strData);
        
    }

    public void LogMessageFromServer(string message) {
        Debug.Log($"Server: {message}");
    }
}
