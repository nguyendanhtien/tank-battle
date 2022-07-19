using System;
using System.IO;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class NetworkController : MonoBehaviour
{
  
    [SerializeField]
    private string ipServer;
    private const int BUFFER_SIZE=1024;
    private const int PORT_NUMBER=5552;
    private ASCIIEncoding encoding= new ASCIIEncoding();
    private Stream stream;

    private Thread waitServerResponseThread, waitGameStartSignalThread;
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

    public void sendMessage(string message) {
        StringWriter strWriter = new StringWriter();
        strWriter.Write(message);
        string strData = strWriter.ToString();
        
        byte[] data=encoding.GetBytes(strData);
        stream.Write(data,0,data.Length);
        Debug.Log($"SEND: {strData}");
    }


    public void sendMoveData(float x, float y, float r) {
        sendMessage($"MOVE:{x},{y},{r}");
    }

    public void sendCreateRoomRequest() {
        sendMessage("CREA");
        
        // Waiting for data
        int roomId = 0;
        StartCoroutine(GetRoomId((roomIdFromServer) => {
            roomId = roomIdFromServer;
            LogMessageFromServer(roomId.ToString());
            m_ui.ShowHomeGUI(false);
            m_ui.ShowCreateRoomGUI(true, roomId);
            // Debug.Log("Display");
            waitServerResponseThread = new Thread(GetMatchResponseFunc);
            waitServerResponseThread.IsBackground = true;
            waitServerResponseThread.Start();
    
        }));
    }

    public void sendCancelWaitingRequest() {
        sendMessage("DROP");
        // Waiting for data
        StartCoroutine(GetServerMessage((message) => {
            LogMessageFromServer(message);
            m_ui.ShowHomeGUI(true);
        }));
    }

    public void sendPlayRandomRequest() {
        int roomId, playerId;
        sendMessage("RAND");
        // Waiting for data
        StartCoroutine(GetRandomPlayResponse((hasRoom, match_room_id, player_id) => {
            if (hasRoom) {
                roomId = match_room_id;
                playerId = player_id;
                Debug.Log($"Joined room {roomId} with player Id {playerId}");
            } else {
                roomId = match_room_id;
                m_ui.ShowHomeGUI(false);
                m_ui.ShowCreateRoomGUI(true, roomId);
                Debug.Log($"No room available. Created room {roomId}.");
                waitServerResponseThread = new Thread(GetMatchResponseFunc);
                waitServerResponseThread.IsBackground = true;
                waitServerResponseThread.Start();
            }
        }));
    }

    public void joinRoom() {
        int roomId = m_ui.GetRoomId();
        if (roomId >= 0) {
            sendJoinRoomRequest(roomId);
        }
    }

    public void sendJoinRoomRequest(int room_id) {
        sendMessage($"JOIN:{room_id}");
        int roomId, playerId;
        StartCoroutine(GetJoinResponse((isSuccess, match_room_id, player_id) => {
            if (isSuccess) {
                roomId = match_room_id;
                playerId = player_id;
                Debug.Log($"Joined room {roomId} with player Id {playerId}");
                m_ui.ShowDialogGUI(true, $"Match room {roomId} successfully.\nDo you want to play now ?");
                waitGameStartSignalThread = new Thread(GetOpponentResponse);
                waitGameStartSignalThread.IsBackground = true;
                waitGameStartSignalThread.Start();
            } else {
                Debug.Log($"Join room {room_id} fail");
            }
        }));
    }

    public void SendAcceptPlayGame() {
        sendMessage("PLAY:1");
        
        // Waiting for data
    
        m_ui.ShowDialogGUI(false);
        // Debug.Log("Display");
        
    
      
    }

    public void SendRefusePlayGame() {
        sendMessage("PLAY:0");
        
        // Waiting for data
    
        m_ui.ShowDialogGUI(false);
        // Debug.Log("Display");
        m_ui.ShowHomeGUI(true);
        m_ui.ShowCreateRoomGUI(false);
        m_ui.ShowJoinRoomGUI(false);
    
      
    }

    public void GetOpponentResponse() {
        Debug.Log("Thread listen");
        byte[] data = new byte[BUFFER_SIZE];
        stream.Read(data,0,BUFFER_SIZE);
        string strData = encoding.GetString(data);
        LogMessageFromServer(strData);
        // Read data
        StringReader strReader = new StringReader(strData.Substring(0, 4));
        string messageType = strReader.ReadLine();
        if (messageType.Equals("GAME")) {
            strReader = new StringReader(strData.Substring(5));
            // Process room id
            int isAnotherAccept = Int16.Parse(strReader.ReadLine());
        
            if (isAnotherAccept == 1) {
                Debug.Log("Opponent player accepts to play with you. GAME START");
            } else {
                Debug.Log("Opponent player refuses to play with you. BACK TO HOME SCREEN");
                MainThread.singleton.AddJob(() => {
                    m_ui.ShowHomeGUI(true);
                    m_ui.ShowCreateRoomGUI(false);
                    m_ui.ShowJoinRoomGUI(false);
                    m_ui.ShowDialogGUI(false);
                });
            }
        }
            
        waitGameStartSignalThread.Abort();
        waitGameStartSignalThread.Join();
    }

    public void GetMatchResponseFunc() {
        byte[] data = new byte[BUFFER_SIZE];
        
        stream.Read(data,0,BUFFER_SIZE);
        
        string strData = encoding.GetString(data);
        LogMessageFromServer(strData);
        // Read data
        StringReader strReader = new StringReader(strData.Substring(0, 4));
        string messageType = strReader.ReadLine();
        // Process room id
        if (messageType.Equals("MATC")) {

            strReader = new StringReader(strData.Substring(5));
            string str = strReader.ReadLine();
            String[] seperator = {":", "-"};
            String[] arr = str.Split(seperator, 2,
                StringSplitOptions.RemoveEmptyEntries);
            int roomId = Int16.Parse(arr[0]);
            int playerId = Int16.Parse(arr[1]);
            
            Debug.Log($"SERVER: Match room {roomId} successfully. Player ID: {playerId}");
            MainThread.singleton.AddJob(() => {
                Debug.Log($"Another player joined your room {roomId}. Your player Id: {playerId}");
                m_ui.ShowDialogGUI(true, $"Match room {roomId} successfully.\nDo you want to play now ?");
            });
        }
            
        
        waitGameStartSignalThread = new Thread(GetOpponentResponse);
        waitGameStartSignalThread.IsBackground = true;
        waitGameStartSignalThread.Start();

        waitServerResponseThread.Abort();
        waitServerResponseThread.Join();

    }
    

    IEnumerator GetRandomPlayResponse(System.Action<bool, int, int> callbackOnFinish) {
        yield return new WaitForSeconds(Time.deltaTime);
        // Receive data
        byte[] data = new byte[BUFFER_SIZE];
        stream.Read(data,0,BUFFER_SIZE);
        string strData = encoding.GetString(data);
        LogMessageFromServer(strData);
        // Read data
        StringReader strReader = new StringReader(strData.Substring(0, 4));
        string messageType = strReader.ReadLine();
        // Process room id
        if (messageType.Equals("MATC")) {

            strReader = new StringReader(strData.Substring(5));
            string str = strReader.ReadLine();
            String[] seperator = {":", "-"};
            String[] arr = str.Split(seperator, 2,
                   StringSplitOptions.RemoveEmptyEntries);
            int roomId = Int16.Parse(arr[0]);
            int playerId = Int16.Parse(arr[1]);
            
            Debug.Log($"SERVER: Match room {roomId} successfully. Player ID: {playerId}");
            callbackOnFinish(true, roomId, playerId);
        } else if (messageType.Equals("CREA")) {
            strReader = new StringReader(strData.Substring(5));
            // Process room id
            int roomId = Int16.Parse(strReader.ReadLine());
            int playerId = 1;
            Debug.Log($"SERVER: No available room. Created room {roomId}.");
            callbackOnFinish(false, roomId, playerId);
        }
        
        
    }

    IEnumerator GetJoinResponse(System.Action<bool, int, int> callbackOnFinish) {
        yield return new WaitForSeconds(Time.deltaTime);
      
        byte[] data = new byte[BUFFER_SIZE];
        stream.Read(data,0,BUFFER_SIZE);
        string strData = encoding.GetString(data);
        LogMessageFromServer(strData);
        // Read data
        StringReader strReader = new StringReader(strData.Substring(0, 4));
        // Process room id
        if (strReader.ReadLine().Equals("MATC")) {

            strReader = new StringReader(strData.Substring(5));
            string str = strReader.ReadLine();
            String[] seperator = {":", "-"};
            String[] arr = str.Split(seperator, 2,
                   StringSplitOptions.RemoveEmptyEntries);
            int roomId = Int16.Parse(arr[0]);
            int playerId = Int16.Parse(arr[1]);

            Debug.Log($"SERVER: Match room {roomId} successfully. Player ID: {playerId}");
            callbackOnFinish(true, roomId, playerId);
        } else {
            Debug.Log($"SERVER: Join room fail");
            callbackOnFinish(false, -1 , -1);
        }
        
    }

    IEnumerator GetRoomId(System.Action<int> callbackOnFinish) {
        yield return new WaitForSeconds(Time.deltaTime);
        
        byte[] data = new byte[BUFFER_SIZE];
        stream.Read(data,0,BUFFER_SIZE);
        string strData = encoding.GetString(data);
        LogMessageFromServer(strData);
        // Read data
        StringReader strReader = new StringReader(strData.Substring(5));
        // Process room id
        int roomId = Int16.Parse(strReader.ReadLine());
        Debug.Log($"Server: {roomId}");
        callbackOnFinish(roomId);
        
    }

    IEnumerator GetServerMessage(System.Action<string> callbackOnFinish) {
        yield return new WaitForSeconds(Time.deltaTime);
        // Receive data
        
        byte[] data = new byte[BUFFER_SIZE];

        
        stream.Read(data,0,BUFFER_SIZE);
        string strData = encoding.GetString(data);
        callbackOnFinish(strData);
        
    }


    public void LogMessageFromServer(string message) {
        Debug.Log($"Server: {message}");
    }
}
