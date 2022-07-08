using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Text RoomIDText, hpText, atkText, timeText;
    public GameObject HomeGUI, createRoomGUI, joinRoomGUI, GamePlayGUI;


    public void SetRoomIDText(string txt){
        if(RoomIDText){
            RoomIDText.text = txt;
        }
    }

    public void SetTimeText(float num){
        if(timeText){
            if(num >=0){
                string txt = ((int)num/60) +":" +(int)(num - ((int)num/60 )*60);
                timeText.text = txt;
            } else timeText.text = "0:0";
        }
    }
    public void ShowHomeGUI(bool isShow){
        if(HomeGUI)
            HomeGUI.SetActive(isShow);
        
    }

    public void ShowCreateRoomGUI(bool isShow){
        if(createRoomGUI)
            createRoomGUI.SetActive(isShow);
    }

    public void ShowJoinRoomGUI(bool isShow){
        if(joinRoomGUI)
            joinRoomGUI.SetActive(isShow);
    }


    public void ShowGamePlayGUI(bool isShow){
        if(GamePlayGUI){
            GamePlayGUI.SetActive(isShow);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
