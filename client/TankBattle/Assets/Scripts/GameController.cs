using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Text;
public class GameController : MonoBehaviour
{   
    int m_roomID, m_localPlayerId;
    bool m_isGameOver, isGameStarted;
    float m_timeRemain= 300;
    UIManager m_ui;
    public GameObject hp1;
    public GameObject bullet1;
    // public Transform gameObjectsCanvas;
    public GameObject player1Prefab;
    public GameObject  player2Prefab;
  
    private Player1 player1;
    private Player2 player2;

    private string items;
    private GameObject[] itemsObj = new GameObject[6];
    private GameObject player1Object, player2Object;
    string m_itemState = "111111";
    // Start is called before the first frame update
    void Start()
    {
        m_ui = FindObjectOfType<UIManager>();
        
        m_ui.ShowConnectionGUI(true);
        // StartGame();
        
    }

    // Update is called once per frame
    void Update()
    {
    
        
        
        
    }

    private void LateUpdate() {
        
    }

    private void FixedUpdate() {
        m_ui.SetTimeText(m_timeRemain);
        if(isGameStarted){
            RenderGame();
        }
    }

    public void setGameStart() {
        StartCoroutine(setGameStatus());
    }

    IEnumerator setGameStatus() {
        yield return null;
        isGameStarted = true;
    }

    public void CreateRoom(){
        m_ui.ShowHomeGUI(false);
        NetworkController.instance.sendCreateRoomRequest();
        // m_ui.ShowCreateRoomGUI(true);
    }

    public void JoinRoom(){
        m_ui.ShowHomeGUI(false);
        m_ui.ShowJoinRoomGUI(true);
    }

    public void BackHome(){
        m_ui.ShowJoinRoomGUI(false);
        m_ui.ShowCreateRoomGUI(false);
        m_ui.ShowGamePlayGUI(false);
        m_ui.ShowHomeGUI(true);
    }

    public void BackHomeFromCreate(){
        m_ui.ShowJoinRoomGUI(false);
        m_ui.ShowCreateRoomGUI(false);
        m_ui.ShowGamePlayGUI(false);
        NetworkController.instance.sendCancelWaitingRequest();
    }

    public void StartGame(){

        m_ui.ShowHomeGUI(false);
        m_ui.SetInGameRoomIdText($"RoomID:{m_roomID}");
        m_ui.ShowGamePlayGUI(true);
        m_ui.ShowQuitGameBtn(true);

        // player1 = FindObjectOfType<Player1>() ;
        // player2 = FindObjectOfType<Player2>() ;
        
        renderNewGame();
        
        
    }


    public void RenderGame(){
        // m_timeRemain -= Time.deltaTime;
        // if(true){
        //     // renderItems("111111");
        // }
        updateItems();
        renderEnemy();
        
        
    }

    public void  renderNewItems(){
        // Vector2 pos1 = new Vector2(Random.Range(-8.6f, 8.6f), Random.Range(-4.7f, 3.52f));
        // // if(hp1){
        //     Instantiate(hp1, pos1, Quaternion.identity);
        // if(itemState[0] == '1')
        items = "111111";
        itemsObj[0] = (GameObject) Instantiate(hp1, new Vector2(7.49f, -3.13f), Quaternion.identity);
        // if(itemState[1] == '1')
        itemsObj[1] = (GameObject) Instantiate(hp1, new Vector2(-0.72f, -3.56f), Quaternion.identity);
        // if(itemState[2] == '1')
        itemsObj[2] = (GameObject) Instantiate(hp1, new Vector2(-7.93f, -0.29f), Quaternion.identity);

        // if(itemState[3] == '1')
        itemsObj[3] = (GameObject) Instantiate(bullet1, new Vector2(-7.57f, 3.07f), Quaternion.identity);
        // if(itemState[4] == '1')
        itemsObj[4] = (GameObject) Instantiate(bullet1, new Vector2(8.05f, -1.24f), Quaternion.identity);
        // if(itemState[5] == '1')
        itemsObj[5] = (GameObject) Instantiate(bullet1, new Vector2(0.16f, 2.54f), Quaternion.identity);
    }

    public void renderNewPlayers() {
        player1Object = (GameObject) Instantiate(player1Prefab);
        player2Object = (GameObject) Instantiate(player2Prefab);
        
        // player1Object.transform.SetParent(gameObjectsCanvas, false);
        // player2Object.transform.SetParent(gameObjectsCanvas, false);
        
        player1 = player1Object.GetComponent<Player1>();
        player2 = player2Object.GetComponent<Player2>();
        if (m_localPlayerId == 1) {
            Debug.Log("111111");
            player1.setLocal(true);
            player2.setLocal(false);
        } else {
            Debug.Log("222222");
            player1.setLocal(false);
            player2.setLocal(true);
        }
        Debug.Log($"Player1: {player1.getLocal()}. Player 2: {player2.getLocal()}");
    }
    public void renderEnemy(){
        
            if (player1.getLocal()) {
                player2.moveNewPos();
                
                StartCoroutine(Player2Shooting());
                
            }

            else {
                player1.moveNewPos();
                StartCoroutine(Player1Shooting());
            }
            
    }
    public void updateItems() {
        for(int i = 0; i < items.Length; i++) {
            if (items[i] == '0') {
                if (itemsObj[i] != null) {
                    Destroy(itemsObj[i]);
                    itemsObj[i] = null;
                }
            }
        }
    }
    public void renderNewGame() {
        renderNewPlayers();
        renderNewItems();
        setGameStart();
    }

    public void DestroyGameObjects() {
        for (int i = 0; i < itemsObj.Length; i++) {
            Destroy(itemsObj[i]);
            itemsObj[i] = null;
            items = "000000";
        }
        Destroy(player1Object);
        Destroy(player2Object);
        isGameStarted = false;

    }

    public void SendGetItem(string itemType, int instanceId) {
        for (int i = 0; i < itemsObj.Length; i++) {
            if (itemsObj[i] != null) {
                if (itemsObj[i].GetInstanceID() == instanceId) {
                    NetworkController.instance.sendMessage($"{itemType}:{i}");
                    StringBuilder itemsStr = new StringBuilder(items);

                    itemsStr[i] = '0';
                    items = itemsStr.ToString();
              
                    break;
                }
            }
        }
    }

    IEnumerator Player2Shooting() {
        while (player2.getNumShooting() > 0) {
            if (player2.getPowerElapsed() > 0)
                player2.ShootLv2();
            else 
                player2.Shoot();
            yield return null;
        }
    }

    IEnumerator Player1Shooting() {
        while (player1.getNumShooting() > 0) {
            if (player1.getPowerElapsed() > 0)
                player1.ShootLv2();
            else 
                player1.Shoot();
            yield return null;
        }
    }

    public void setGameInfo(int roomId, int playerId) {
        m_roomID = roomId;
        m_localPlayerId = playerId;
    }

    public void setGameState(float posX1, float posY1, float rot1, int hp1, int isShot1, int power1,
                            float posX2, float posY2, float rot2, int hp2, int isShot2, int power2,
                            string itemStr, int time) {
        player1.setState(posX1, posY1, rot1, hp1, isShot1, power1);
        player2.setState(posX2, posY2, rot2, hp2, isShot2, power2);
        
        items = itemStr;
        m_timeRemain = time;

    }


    public void QuitGame(){
        Application.Quit();
        Debug.Log("Quit Game");
    }

}
