using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


public class MainMenue : MonoBehaviour {
    
	GuiHelper guiHelper;
    private bool wasDownLastFrame = false;


    public static string GAME_TITLE = "Dark Space Conquest";

	/*~~ STATUS ~~*/
		enum Menue{Main, Settings};
		Menue menue = Menue.Main;
		private string MainMenueMessage = null;		//If set, this message is displayed and needs to be confirmed, before the main menue is shown
	/*~~  ----  ~~*/


	void Start () {
		guiHelper = GameObject.Find ("Menue").GetComponent<GuiHelper>();
		if (guiHelper == null) {
			throw new MissingComponentException ("Unable to find GuiHelper.");
		}
		MainMenueMessage = ApplicationModel.MainMenueMessage; 
	}
	

	void OnGUI () {
        //if (Screen.fullScreen){         //THIS is a fix for a Unity bug, which happens to appear sometimes - the f***ing game simply always starts in fullscreen - Todo: remove this code
        //    Screen.fullScreen = false;
        //}
        
		guiHelper.Prepare();

        if (MainMenueMessage == "") {
            MainMenueMessage = null;
        } 
		if(MainMenueMessage != null){
			RenderMenueMessage();
		}else{
			switch (menue) {
				case Menue.Main: RenderMainMenue(); break;
				case Menue.Settings: RenderSettingsMenue(); break;
				/*default:	menue = Menue.Main;
							Debug.LogError("Invalid menue-value -going back to main menue.");*/
			}
		}

        
		
	}

	//Can be an error message, why the lobby/game was closed/left
	void RenderMenueMessage(){
		guiHelper.TitleText (GAME_TITLE);

		var centeredStyle = GUI.skin.GetStyle("Label");
		centeredStyle.alignment = TextAnchor.MiddleCenter;

		GUI.Label (new Rect (0, 0, Screen.width, Screen.height), ApplicationModel.MainMenueMessage, centeredStyle);

        if (guiHelper.ExitButton("OK") || Input.GetKeyDown(KeyCode.Escape)) // for escape an android back button
        {
            wasDownLastFrame = true;
			MainMenueMessage = null;	//The MainMenueMessage has been confirmed -> don't show it anymore
		}
	}
	
	void RenderMainMenue(){
		guiHelper.TitleText (GAME_TITLE);

		float YPos = guiHelper.GetTitleSpace() + 10.0f;
        if (GUI.Button(guiHelper.BigCenterElemRect(YPos), "Create Lobby")) {
            ApplicationModel.EnterLobbyAsServer();
        }
        if (GUI.Button(guiHelper.BigCenterElemRect(YPos += guiHelper.BigElemHeight + guiHelper.BigElemSpacing), "Create private Lobby")) {
            ApplicationModel.EnterLobbyAsPrivateServer();
        }
		if (GUI.Button (guiHelper.BigCenterElemRect(YPos+=guiHelper.BigElemHeight+guiHelper.BigElemSpacing), "Join Game")) {
			ApplicationModel.EnterLobbyJoiner();
		}
		if(	GUI.Button(guiHelper.BigCenterElemRect(YPos+=guiHelper.BigElemHeight+guiHelper.BigElemSpacing), "Settings") ){
			menue = Menue.Settings;
		}
        if (GUI.Button(guiHelper.BigCenterElemRect(YPos += guiHelper.BigElemHeight + guiHelper.BigElemSpacing), "Quit") || (Input.GetKeyDown(KeyCode.Escape) && !wasDownLastFrame)) {
            Application.Quit();
        }
	    if (!Input.GetKeyDown(KeyCode.Escape)){
	        wasDownLastFrame = false;
	    }
	}

	void RenderSettingsMenue(){
		guiHelper.TitleText ("Settings");

        
        if (guiHelper.ExitButton("Back") || Input.GetKeyDown(KeyCode.Escape)){
			Settings.SetMasterServer( Settings.GetMasterServerStringNorm() );	//Normalise the masterServerString (for better UX)
			menue = Menue.Main;
            wasDownLastFrame = true;
        }

		float LeftX = Screen.width / 2 - guiHelper.ElemWidth;
		float RightX = Screen.width / 2;
		float YPos = guiHelper.GetTitleSpace();
		float RndBtnWidth = guiHelper.ElemWidth * 0.15f;//GuiHelper.X (60, 5, 20);


		GUI.Label(guiHelper.ElemRect(LeftX, YPos), "Player Name");
		Settings.SetPlayerName( GUI.TextField(new Rect(RightX, YPos, guiHelper.ElemWidth - RndBtnWidth, guiHelper.ElemHeight), Settings.PlayerName, Player.MAX_PLAYER_NAME_LENGTH) );

		if(GUI.Button(new Rect(RightX + guiHelper.ElemWidth - RndBtnWidth, YPos+guiHelper.ElemHeight*0.2f, RndBtnWidth, guiHelper.ElemHeight * 0.63f), "")){
			string newName;
			do{
				newName = Settings.GetRandomPlayerName();
			}while(newName == Settings.PlayerName);
			Settings.SetPlayerName( newName );
		}

		GUI.Label(guiHelper.ElemRect(LeftX, YPos+=guiHelper.ElemHeight+guiHelper.ElemSpacing), "Master Server");
		Settings.SetMasterServer( GUI.TextField(guiHelper.ElemRect(RightX, YPos), Settings.GetMasterServerString(), 64) );

		if( GUI.Button(guiHelper.SmallElemRect(RightX, YPos+=guiHelper.ElemHeight+guiHelper.ElemSpacing/2), "Local", guiHelper.SmallButtonStyle ) ){
			Settings.SetLocalMasterServer();
		}
		if( GUI.Button(guiHelper.SmallElemRect(RightX + guiHelper.ElemWidth/2, YPos), "Unity", guiHelper.SmallButtonStyle) ){
			Settings.SetUnityMasterServer();
		}

	}

}
