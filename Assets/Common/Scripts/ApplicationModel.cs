using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/*
 * Helper
 * This (static) class must be used for scene Changes within the Game.
 * Exception: This class is NOT RESPONSIBLE for opening the Game-Scene. (The EnterGameSynchroniser.cs in the Lobby does this)
 *            Reason: The Game must only be entered from the Lobby, AND ALL PLAYERS MUST ENTER IT SIMULTANEOUSLY.
 *                    This requires synchronisation, and the ApplicationModel is not capable of synchronising that (as some gameobjects need to be moved from the Lobby to the Game and some other operations need to be performed too)
 */

public enum LobbyEnteredAs
{
    Client,
    PublicServer,
    PrivateServer
}


public sealed class ApplicationModel : MonoBehaviour {


	//Scene-Changes:
        static public LobbyEnteredAs lobbyEnteredAs { get; private set; } 		//Scene-change from main menue to lobby --> which button did the user click?
        static public HostData HostToUse { get; private set; } 			        //Scene-change from main menue to lobby --> which server the client should connect to
		static public string MainMenueMessage { get; private set; } 			//This message is displayed in the main menue (eg. for error messages)
	//~~~~~


	//Opens the main menue
	    public static void EnterMainMenue(){
		    MainMenueMessage = null;
		    Application.LoadLevel("MainMenue");
	    }

	//Opens the main menue and displays a message, that needs to be confirmed
	    public static void EnterMainMenue(string message){
		    MainMenueMessage = message;
		    Application.LoadLevel("MainMenue");
	    }

	//Opens the main menue and displays a message, that needs to be confirmed
	    public static void EnterLobbyJoiner(){
		    Application.LoadLevel("JoinLobby");
	    }

	//Opens the lobby, and starts a server
	    public static void EnterLobbyAsServer(){
            lobbyEnteredAs = LobbyEnteredAs.PublicServer;
		    Application.LoadLevel("Lobby");
	    }

    //Opens the lobby, and starts a private server
        public static void EnterLobbyAsPrivateServer() {
            lobbyEnteredAs = LobbyEnteredAs.PrivateServer;
            Application.LoadLevel("Lobby");
        }

	//Enters the lobby, and connects to a server
	    public static void EnterLobbyAsClient(HostData host){
            lobbyEnteredAs = LobbyEnteredAs.Client;
            HostToUse = host;
		    Application.LoadLevel("Lobby");
	    }

}
