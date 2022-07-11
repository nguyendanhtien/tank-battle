using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    int m_roomID;
    bool m_isGameOver;
    float m_timeRemain= 300;
    UIManager m_ui;
    Tank player1;
    // Start is called before the first frame update
    void Start()
    {
        m_ui = FindObjectOfType<UIManager>();
        m_ui.ShowHomeGUI(true);
    }

    // Update is called once per frame
    void Update()
    {
        m_timeRemain -= Time.deltaTime;
        m_ui.SetTimeText(m_timeRemain);
    }


    public void CreateRoom(){
        m_ui.ShowHomeGUI(false);
        int roomId = NetworkController.instance.sendCreateRoomRequest();
        Debug.Log($"OUT: {roomId}");
        m_ui.ShowCreateRoomGUI(true);
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

    public void StartGame(){
        m_ui.ShowHomeGUI(false);
        m_ui.ShowGamePlayGUI(true);
        player1 = FindObjectOfType<Tank>();

    }

    public Tank getPlayer(){
        return player1;
    }

}
