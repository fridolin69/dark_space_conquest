using UnityEngine;
using System.Collections;


/*
 * This class is used to leave the Lobby and enter the Game Scene on all connected (and registered) Players simultaneously
 */


//[RequireComponent(typeof(NetworkView))]
public class EnterGameSynchroniser : MonoBehaviour {



    //Client + Server: leave the lobby and enter the game. Called by the server
        [RPC]
        void EnterGameNow() {
            if (Application.loadedLevelName != "Lobby") {
                throw new UnityException("The game can only be entered through the Lobby");
            }
            GameObject networking = GameObject.Find("Networking");      //This object must be available in the Game Scene too
            if (networking == null) {
                throw new MissingComponentException("Unable to find Networking GameObject.");
            }

            Object.DontDestroyOnLoad(networking);
            print("Entering Game...");
            Application.LoadLevel("Game");
        }





    //Server: The user pressed the "Enter game" button:
        public void EnterGame() {
            if (Network.isClient) {
                throw new UnityException("Clients are not allowed to start the Game");
            }
            //MasterServer.UnregisterHost();
            networkView.RPC("EnterGameNow", RPCMode.All); 
        }


}
