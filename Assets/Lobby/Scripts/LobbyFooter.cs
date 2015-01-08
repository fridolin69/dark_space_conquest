using UnityEngine;
using System.Collections;


/*
 * GameObject
 * Bottom GUI-Content in the lobby. Contains "Leave"/"Close" Lobby and "Start Game" functionality
 * ALSO responsible for switching to the game-scene (Synchronises all Players, so that they switch simultaneously, ...).
 */
public class LobbyFooter : MonoBehaviour {

	GuiHelper guiHelper;
	NetEnvironment netEnvironment;
    PlayerList playerList;              //Required to check if the game can be started
    EnterGameSynchroniser enterGameSynchroniser;                //Required to leave the Lobby and enter the game when the user (=server) requires it

	void Awake () {		
		guiHelper = GetComponent<GuiHelper>();
		if (guiHelper == null) {
			throw new MissingComponentException ("Unable to find GuiHelper.");
		}
		netEnvironment = GameObject.Find ("Networking").GetComponent<NetEnvironment>();
		if (netEnvironment == null) {
			throw new MissingComponentException ("Unable to find NetEnvironment.");
		}
        playerList = GameObject.Find("PlayerList").GetComponent<PlayerList>();
        if (playerList == null) {
            throw new MissingComponentException("Unable to find PlayerList.");
        }
        enterGameSynchroniser = GameObject.Find("Synchroniser").GetComponent<EnterGameSynchroniser>();
        if (enterGameSynchroniser == null) {
            throw new MissingComponentException("Unable to find EnterGameSynchroniser.");
        }
	}
	

	void OnGUI () {
		guiHelper.Prepare();
        if (guiHelper.ExitButton(Network.isServer ? "Close Lobby" : "Leave Lobby") || Input.GetKeyDown(KeyCode.Escape) )
        {
			netEnvironment.ShutdownNetwork();
		}
        if(Network.isServer){                               //Only the server can start the game
            //if (playerList.GetPlayerCount() > 1) {          //Only join the game with more than one player
                if (guiHelper.NextButton("Start Game")) {
                    enterGameSynchroniser.EnterGame();                  //Initiate an EnterGameSynchroniser procedure on all players
                }
            //}            
        }
	}





}
