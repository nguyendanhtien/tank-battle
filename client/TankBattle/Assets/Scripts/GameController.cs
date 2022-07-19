using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    int m_roomID;
    bool m_isGameOver, isGameStarted;
    float m_timeRemain= 300;
    UIManager m_ui;
    public HPItem hp1;
    public BulletLv2Item bullet1;
    public Tank player1;
    public Enemy  enemy;

    string itemState;
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
        
        
        
    }

    private void LateUpdate() {
        
    }

    private void FixedUpdate() {
        if(isGameStarted){
            RenderGame();
        }
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
        player1 = FindObjectOfType<Tank>();
        enemy = Instantiate(enemy, new Vector2(7.5f, 3.5f), Quaternion.Euler(0,0,90)) ;
    }


    public void RenderGame(){
        m_timeRemain -= Time.deltaTime;
        if(true){
            renderItems("111111");
        }
        
        renderEnemy( 7.25f,2.35f,180);
        
        
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
            Instantiate(bullet1, new Vector2(0.16f, 0.54f), Quaternion.identity);
    }

    public void renderEnemy( float posX2, float posY2, float rotation2){
        // enemy = FindObjectOfType<Enemy>();
        
        // Instantiate(player1, new Vector2(posX1, posY1), Quaternion.Euler(0,0,rotation1));
        // Vector2 pos1 = new Vector2(Random.Range(-8.6f, 8.6f), Random.Range(-4.7f, 3.52f));

        // Vector2 posEnemy = new Vector2(posX2, posY2);
        //  Enemy clone =    Instantiate(enemy, pos1, Quaternion.Euler(0,0,Random.Range(-180,180))) ;

        // clone.destroy();
        // yield return new WaitForSeconds(3);
        // Destroy(clone,0.1f);


        enemy.moveNewPos(Random.Range(-8.6f, 8.6f), Random.Range(-4.7f, 3.52f) , Random.Range(-180,180));

        
    }

    public Tank getPlayer(){
        return player1;
    }

}
