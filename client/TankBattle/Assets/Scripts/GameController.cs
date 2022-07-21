using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    int m_roomID, m_localPlayerId;
    bool m_isGameOver, isGameStarted;
    float m_timeRemain= 300;
    UIManager m_ui;
    public HPItem hp1;
    public BulletLv2Item bullet1;
    public Player1 player1;
    public Player2  player2;
    private string items;
   
    string m_itemState = "111111";
    // Start is called before the first frame update
    void Start()
    {
        m_ui = FindObjectOfType<UIManager>();
        
        m_ui.ShowHomeGUI(true);
        
    }

    // Update is called once per frame
    void Update()
    {
        
        m_ui.SetTimeText(m_timeRemain);
        if(isGameStarted){
            RenderGame();
        }
        
        
    }

    private void LateUpdate() {
        
    }

    private void FixedUpdate() {
        
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
        isGameStarted = true;
        m_ui.ShowHomeGUI(false);
        m_ui.ShowGamePlayGUI(true);
        // player1 = FindObjectOfType<Player1>() ;
        // player2 = FindObjectOfType<Player2>() ;
        player1 = Instantiate(player1, new Vector2(-7.5f, -3.5f), Quaternion.Euler(0,0,0)) ;
        player2 = Instantiate(player2, new Vector2(7.5f, 3.5f), Quaternion.Euler(0,0,180)) ;
        
        if (m_localPlayerId == 1) {
            player1.setLocal(true);
            player2.setLocal(false);
        } else {
            player1.setLocal(false);
            player2.setLocal(true);
        }
        Debug.Log($"Player1: {player1.getLocal()}. Player 2: {player2.getLocal()}");
        renderItems("111111");
    }


    public void RenderGame(){
        // m_timeRemain -= Time.deltaTime;
        // if(true){
        //     // renderItems("111111");
        // }
        
        renderEnemy();
        
        
    }

    public void  renderItems(string itemState){
        // Vector2 pos1 = new Vector2(Random.Range(-8.6f, 8.6f), Random.Range(-4.7f, 3.52f));
        // // if(hp1){
        //     Instantiate(hp1, pos1, Quaternion.identity);
        if(itemState[0] == '1')
            Instantiate(hp1, new Vector2(7.49f, -3.13f), Quaternion.identity);
        if(itemState[1] == '1')
            Instantiate(hp1, new Vector2(-0.72f, -3.56f), Quaternion.identity);
        if(itemState[2] == '1')
            Instantiate(hp1, new Vector2(-7.93f, -1.29f), Quaternion.identity);

        if(itemState[3] == '1')
            Instantiate(bullet1, new Vector2(-7.57f, 3.07f), Quaternion.identity);
        if(itemState[4] == '1')
            Instantiate(bullet1, new Vector2(8.05f, -1.24f), Quaternion.identity);
        if(itemState[5] == '1')
            Instantiate(bullet1, new Vector2(0.16f, 2.54f), Quaternion.identity);
    }

    public void renderEnemy(){
        // enemy = FindObjectOfType<Enemy>();
        
        // Instantiate(player1, new Vector2(posX1, posY1), Quaternion.Euler(0,0,rotation1));
        // Vector2 pos1 = new Vector2(Random.Range(-8.6f, 8.6f), Random.Range(-4.7f, 3.52f));

        // Vector2 posEnemy = new Vector2(posX2, posY2);
        //  Enemy clone =    Instantiate(enemy, pos1, Quaternion.Euler(0,0,Random.Range(-180,180))) ;

        // clone.destroy();
        // yield return new WaitForSeconds(3);
        // Destroy(clone,0.1f);
        // if (m_timeRemain < 299) {
            if (player1.getLocal()) {
                player2.moveNewPos();
                
                StartCoroutine(Player2Shooting());
                    // player2.Shoot();
                    // player2.setShootingStatus(0);
                    // Debug.Log("Player 2 ShoottttttttttttTTTT~");
                
            }

                // Debug.Log($"{player2.}");
            else {
                player1.moveNewPos();
                StartCoroutine(Player1Shooting());
            }
            // enemy.moveNewPos(Random.Range(-8.6f, 8.6f), Random.Range(-4.7f, 3.52f) , Random.Range(-180,180));
        // }

        
    }

    IEnumerator Player2Shooting() {
        while (player2.getNumShooting() > 0) {
            player2.Shoot();
            yield return null;
        }
    }

    IEnumerator Player1Shooting() {
        while (player1.getNumShooting() > 0) {
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
    public Player1 getPlayer(){
        return player1;
    }


    public void QuitGame(){
        Application.Quit();
        Debug.Log("aaaaaaaaaaaaaaaaaaaaaaaaaaa");
    }

}
