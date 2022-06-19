using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    int m_hp, m_attack, m_roomID;
    bool m_isGameOver;
    UIManager m_ui;
    // Start is called before the first frame update
    void Start()
    {
        m_ui = FindObjectOfType<UIManager>();
        m_ui.ShowHomeGUI(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void CreateRoom(){
        m_ui.ShowHomeGUI(false);
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
    }

}
