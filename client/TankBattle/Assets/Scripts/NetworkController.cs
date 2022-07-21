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
  
    private string ipServer;
    private const int BUFFER_SIZE=1024;
    private const int PORT_NUMBER=5552;
    private ASCIIEncoding encoding= new ASCIIEncoding();
    private Stream stream;

    private Thread waitServerResponseThread, waitGameStartSignalThread, ingameThread;
    private int inGameRoomId, inGamePlayerId;
    public static NetworkController instance;
    GameController gameController;
    UIManager m_ui;
    // Enemy enemy;


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
        gameController = FindObjectOfType<GameController>();
        // enemy = FindObjectOfType<Enemy>();
        
    }

    public void ConnectServer() {
        try
        {
            IPAddress address = IPAddress.Parse(m_ui.getIpServer());
            TcpClient client = new TcpClient();
            client.Connect(address, PORT_NUMBER);
            stream = client.GetStream();
            StartCoroutine(GetServerMessage((message) => {
                LogMessageFromServer(message);
            }));
            Debug.Log("Connected");
            m_ui.ShowHomeGUI(true);
        }
        catch (Exception ex)
        {
            Debug.Log("Error: " + ex);
            m_ui.ShowPopUpGUI(true, "Connection failed");
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

    public void sendShootMessage() {
        sendMessage("SHOT");
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
        // StartCoroutine(GetServerMessage((message) => {
        //     LogMessageFromServer(message);
        //     m_ui.ShowHomeGUI(true);
        // }));
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
                m_ui.ShowDialogGUI(true, $"Match room {roomId} successfully.\nDo you want to play now ?");
                inGameRoomId = roomId;
                inGamePlayerId = playerId;
                waitGameStartSignalThread = new Thread(GetOpponentResponse);
                waitGameStartSignalThread.IsBackground = true;
                waitGameStartSignalThread.Start();
            } else {
                roomId = match_room_id;
                m_ui.ShowHomeGUI(false);
                m_ui.ShowCreateRoomGUI(true, roomId);
                m_ui.ShowPopUpGUI(true, $"No room available. Created room {roomId}.");
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
                m_ui.ShowWaitingGUI(true);
                inGameRoomId = roomId;
                inGamePlayerId = playerId;
                waitGameStartSignalThread = new Thread(GetOpponentResponse);
                waitGameStartSignalThread.IsBackground = true;
                waitGameStartSignalThread.Start();
            } else {
                Debug.Log($"Join room {room_id} fail");
                m_ui.ShowPopUpGUI(true, $"Room {room_id} is not available.");
            }
        }));
    }

    public void SendAcceptPlayGame() {
        sendMessage("PLAY:1");
        Debug.Log("AA");
        // Waiting for data
    
        m_ui.ShowDialogGUI(false);
        // Debug.Log("Display");
        // waitServerResponseThread.Abort();
        // waitServerResponseThread.Join();
        
        
    
      
    }

    public void SendQuitGame() {
        sendMessage("QUIT");
        ingameThread.Abort();
        waitGameStartSignalThread.Abort();
        waitServerResponseThread.Abort();
        m_ui.ShowHomeGUI(true);
        m_ui.ShowGamePlayGUI(false);
    }

    public void SendAcceptContinuePlayGame() {
        sendMessage("CONT:1");
        Debug.Log("BB");
        // Waiting for data
    
        m_ui.ShowContinueDialogGUI(false);
        m_ui.ShowWaitingGUI(true);
      
    }

    public void SendRefuseContinuePlayGame() {
        sendMessage("CONT:0");
        
        // Waiting for data
    
        m_ui.ShowContinueDialogGUI(false);      
    }


    public void SendRefusePlayGame() {
        sendMessage("PLAY:0");
        
        // Waiting for data
    
        m_ui.ShowDialogGUI(false);
        // Debug.Log("Display");
        m_ui.ShowHomeGUI(true);
        m_ui.ShowWaitingGUI(false);
        m_ui.ShowCreateRoomGUI(false);
        m_ui.ShowJoinRoomGUI(false);
        waitServerResponseThread.Abort();
        waitServerResponseThread.Join();
        
        waitGameStartSignalThread.Abort();
        waitGameStartSignalThread.Join();
      
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
                MainThread.singleton.AddJob(() => {
                    m_ui.ShowDialogGUI(false);
                    m_ui.ShowWaitingGUI(false);
                    m_ui.ShowJoinRoomGUI(false);
                    gameController.setGameInfo(inGameRoomId, inGamePlayerId);
                    gameController.StartGame();
                    ingameThread = new Thread(GetGameState);
                    ingameThread.IsBackground = true;
                    ingameThread.Start();
                });
                
            } else {
                Debug.Log("Opponent player refuses to play with you. BACK TO HOME SCREEN");
                MainThread.singleton.AddJob(() => {
                    m_ui.ShowHomeGUI(true);
                    m_ui.ShowCreateRoomGUI(false);
                    m_ui.ShowJoinRoomGUI(false);
                    m_ui.ShowDialogGUI(false);
                    m_ui.ShowWaitingGUI(false);
                    m_ui.ShowPopUpGUI(true, "Opponent refuses to play");
                });
            }
        }
            
        waitGameStartSignalThread.Abort();
        waitGameStartSignalThread.Join();
    }

    public void GetMatchResponseFunc() {
        byte[] data = new byte[BUFFER_SIZE];
        lock(stream) {

            stream.Read(data,0,BUFFER_SIZE);
        }
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
                m_ui.ShowCreateRoomGUI(false);
                m_ui.ShowJoinRoomGUI(false);
                m_ui.ShowWaitingGUI(true);
                m_ui.ShowDialogGUI(true, $"Match room {roomId} successfully.\nDo you want to play now ?");
            });
            inGameRoomId = roomId;
            inGamePlayerId = playerId;
            

            waitGameStartSignalThread = new Thread(GetOpponentResponse);
            waitGameStartSignalThread.IsBackground = true;
            waitGameStartSignalThread.Start();
        } else if (messageType.Equals("DROP")) {
            MainThread.singleton.AddJob(() => {
                m_ui.ShowHomeGUI(true);
                m_ui.ShowCreateRoomGUI(false);
            });
        }
    
            
        
        

        waitServerResponseThread.Abort();
        waitServerResponseThread.Join();

    }
    public void DeserializedGameStateMessage(string gameStateMsg) {
        String[] seperator = {":", "|", "~", ","};
        String[] arr = gameStateMsg.Split(seperator, 19,
                StringSplitOptions.RemoveEmptyEntries);
        // for (int i = 0; i < arr.Length; i++) {
        //     Debug.Log(arr[i]);
        // }
        // Debug.Log(arr.Length);
        float posX1, posY1, rot1, posX2, posY2, rot2;
        int hp1, hp2, isShot1, isShot2, power1, power2, time;
        posX1 = float.Parse(arr[2]);
        posY1 = float.Parse(arr[3]);
        rot1 = float.Parse(arr[4]);
        posX2 = float.Parse(arr[9]);
        posY2 = float.Parse(arr[10]);
        rot2 = float.Parse(arr[11]);
        hp1 = Int16.Parse(arr[5]);
        hp2 = Int16.Parse(arr[12]);
        isShot1 = Int16.Parse(arr[6]);
        isShot2 = Int16.Parse(arr[13]);
        string items = arr[16];
        time = Int16.Parse(arr[18]);
        power1 = Int16.Parse(arr[7]);
        power2 = Int16.Parse(arr[14]);
        // LogMessageFromServer(gameStateMsg);
        MainThread.singleton.AddJob(() => {
                gameController.setGameState(posX1, posY1, rot1, hp1, isShot1, power1,
                                            posX2, posY2, rot2, hp2, isShot2, power2,
                                            items, time);
        });
    }
    public void GetGameState() {

        waitGameStartSignalThread.Abort();
        waitGameStartSignalThread.Join();

        byte[] data = new byte[BUFFER_SIZE];
        string strData = "";
        string headSegment = "", lastSegment = "";
        bool isGameEnd = false;
        bool isGameOver = false;

        while (!isGameEnd) {
            if (stream.Read(data,0,BUFFER_SIZE) <= 0) {
                Debug.Log("Server terminated");
                break;
            }
            strData = encoding.GetString(data);
            String[] seperator = {"\n"};
            String[] streamQueue = strData.Split(seperator, 1000,
                    StringSplitOptions.RemoveEmptyEntries);
            headSegment = streamQueue[0];
            string messageType = "";
            StringReader strReader;
            try {
                strReader = new StringReader(headSegment.Substring(0, 4));
                messageType = strReader.ReadLine();
            } catch {
                Debug.Log("ERRR1");
                Debug.Log(data);
            }
            if (!messageType.Equals("STAT") && !messageType.Equals("ENDS") && !messageType.Equals("HOME") && messageType.Equals("CONT")) {
                
                streamQueue[0] = lastSegment + headSegment;
            } 
            for (int i = 0; i < streamQueue.Length - 1; i++) {
                string dataStream = streamQueue[i];
                // if (dataStream[dataStream.Length - 1] != ) {
                //     continue;
                // }
                // LogMessageFromServer(dataStream);
                try {
                    strReader = new StringReader(dataStream.Substring(0, 4));
                    messageType = strReader.ReadLine();
                } catch {
                    Debug.Log("ERRR");
                    Debug.Log(data);
                }
                if (messageType.Equals("REPL")) {
                    
                    MainThread.singleton.AddJob(() => {
                        m_ui.ShowWaitingGUI(false);
                    });

                // Process room id
                } else if (messageType.Equals("STAT")) {
                    
                    try {
                        DeserializedGameStateMessage(dataStream);
                    } catch {
                        Debug.Log($"ERR2: {dataStream}");
                    }
                } else if (messageType.Equals("ENDS")) {
                    strReader = new StringReader(dataStream.Substring(5, 4));
                    string gameResult = strReader.ReadLine();
                    
                    if (gameResult.Equals("WINS")) {
                        MainThread.singleton.AddJob(() => {
                                m_ui.ShowPopUpGUI(true, "You Win!");
                        });
                    } else if (gameResult.Equals("LOSE")) {
                        MainThread.singleton.AddJob(() => {
                                m_ui.ShowPopUpGUI(true, "You Lose!");
                        });
                    } else if (gameResult.Equals("DRAW")) {
                        MainThread.singleton.AddJob(() => {
                                m_ui.ShowPopUpGUI(true, "Game Draw!");
                        });
                    } else if (gameResult.Equals("WINA")) {
                        isGameEnd = true;
                        MainThread.singleton.AddJob(() => {
                                m_ui.ShowPopUpGUI(true, "Opponent exits game. You Win!");
                                m_ui.ShowGamePlayGUI(false);
                                m_ui.ShowHomeGUI(true);
                        });
                    }

                } else if (messageType.Equals("CONT")) {
                        MainThread.singleton.AddJob(() => {
                                m_ui.ShowContinueDialogGUI(true, "Do you want to continue playing?");
                        });
                    
                } else if (messageType.Equals("HOME")) {
                    isGameEnd = true;
                    MainThread.singleton.AddJob(() => {
                            m_ui.ShowPopUpGUI(true, "A player does not want to continue playing");
                            m_ui.ShowDialogGUI(false);
                            m_ui.ShowWaitingGUI(false);
                            m_ui.ShowContinueDialogGUI(false);
                            m_ui.ShowGamePlayGUI(false);
                            m_ui.ShowHomeGUI(true);
                    });
                } else {
                    Debug.Log($"NERR: {dataStream}");
                }
            } 
            data = new byte[BUFFER_SIZE];
            lastSegment = streamQueue[streamQueue.Length - 1];
            try {
                DeserializedGameStateMessage(lastSegment);
            } catch {
                continue;
            } 
            
        }
        Debug.Log("In Game Thread terminated");
        ingameThread.Abort();
        ingameThread.Join();
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

        lock(stream) {
            stream.Read(data,0,BUFFER_SIZE);
        }
        string strData = encoding.GetString(data);
        callbackOnFinish(strData);
        
    }


    public void LogMessageFromServer(string message) {
        Debug.Log($"Server: {message}");
    }
}
