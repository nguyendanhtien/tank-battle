using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class UIManager : MonoBehaviour
{
    public Text RoomIDText, hpText, atkText, timeText, inputRoomIdText, dialogText, popUpText;
    public GameObject HomeGUI, createRoomGUI, joinRoomGUI, GamePlayGUI, DialogGUI, PopUpGUI;


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

    public void SetDialogText(string txt){
        if(dialogText){
            dialogText.text = txt;
        }
    }

    public void SetPopUpText(string txt){
        if(popUpText){
            popUpText.text = txt;
        }
    }


    public int GetRoomId() {
        if (inputRoomIdText.text != "") {
            try {
                int roomId = Int32.Parse(inputRoomIdText.text);
                if (roomId < 0) {
                    Debug.Log("Input must be a positive integer");
                    return -2;
                }

                return roomId;      
            } catch {
                Debug.Log("Input must be a positive integer");
                return -2;
            }
              
        }
        Debug.Log("No input");
        return -1;
        
    }
    public void ShowHomeGUI(bool isShow){
        if(HomeGUI)
            HomeGUI.SetActive(isShow);
        
    }

    public void ShowDialogGUI(bool isShow){
        if(DialogGUI)
            DialogGUI.SetActive(isShow);
    }

    public void ShowDialogGUI(bool isShow, string message){
        if(DialogGUI) {
            DialogGUI.SetActive(isShow);
            SetDialogText(message);
        }
    }

    public void ConfirmNotification() {
        ShowPopUpGUI(false);
    }

    public void ShowPopUpGUI(bool isShow){
        if(PopUpGUI)
            PopUpGUI.SetActive(isShow);
    }

    public void ShowPopUpGUI(bool isShow, string message){
        if(PopUpGUI) {
            PopUpGUI.SetActive(isShow);
            SetPopUpText(message);
        }
    }

    public void ShowCreateRoomGUI(bool isShow){
        if(createRoomGUI)
            createRoomGUI.SetActive(isShow);
    }

    public void ShowCreateRoomGUI(bool isShow, int roomId){
        if(createRoomGUI) {
            createRoomGUI.SetActive(isShow);
            SetRoomIDText($"Your room ID is: {roomId}");
        }
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
