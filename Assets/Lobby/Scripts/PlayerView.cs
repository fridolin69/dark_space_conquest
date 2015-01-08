using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/*
 *  GameObject
 *  Represents the output, which shows all connected players.
 *  Each entry (one for each connected and registered Player) is represented by "PlayerInfoScript.cs" *  
 *  Contains RPC synchronisation logic to change the Players' characters (sprite and color). The RPC-functions are called by the "PlayerInfoScript.cs" whenever the player clicks on its character in the GUI
 */



//Responsible for changing the player's character and for displaying a list of connected (and registered) players
public class PlayerView : MonoBehaviour, I_PlayerListObserver {

    PlayerList playerList;      //The list in this object is being observed
    Characters characters;

    public GameObject playerInfoPrefab;
    public const float PLAYER_VIEW_PREVIEW_PADDING = 3;     //space between preview and playerView


	void Awake () {
		playerList = GameObject.Find ("PlayerList").GetComponent<PlayerList>();
		if (playerList == null) {
			throw new MissingComponentException ("Unable to find PlayerList.");
		}
        playerList.AddObserver(this);
        characters = new Characters(playerList);
	}

    void OnDestroy() {
        playerList.RemoveObserver(this);
    }
	
    //Gets called by the playerList, whenever a new player enters the lobby, or an old player leaves it (observer function)
    public void OnPlayerListChanged(PlayerListEventType eventType, Player player) {
        if (eventType == PlayerListEventType.PlayerUpdatedPlayerList) {     //The playerList itself didn't change. So there's nothing todo.
            return;
        } 
        if (eventType == PlayerListEventType.PlayerChangedCharacter) {    //The number of elements is the same, we only need to change the character
            UpdateGameObjects();  
            return;
        }

        int playerListCount = playerList.GetPlayerCount();          

        //Update the number of playerInfo-Objects according to the number of players:
            int count = transform.childCount;

            for (int i = count; i > playerListCount; i--) {   //Delete some elements
                GameObject.Destroy(transform.GetChild(i - 1).gameObject);
            }
            for (int i = count; i < playerListCount; i++) {   //Create some new elements
                GameObject playerInfo = Instantiate(playerInfoPrefab) as GameObject;        //Create new prefab
                playerInfo.transform.parent = this.transform;                               //set as child of this object
                playerInfo.transform.Translate(0, -i * 6 - PLAYER_VIEW_PREVIEW_PADDING, 0);                            //put at right position
                playerInfo.GetComponent<PlayerInfoScript>().Initialise(this);
            }
            //Update all playerInfo-objects
            int idx = 0;
            foreach (Transform child in transform) {    //Destroy all exsting elements
                PlayerInfoScript script = child.gameObject.GetComponent<PlayerInfoScript>();
                if (playerListCount < idx + 1) {
                    Debug.LogWarning("PlayerView: OnPlayerListChanged() might have run into problems. This has probably be caused by a server which is shutting down");
                    return;
                }
                script.SetPlayer(playerList.GetPlayerByIndex(idx));
                idx++;
            }
        }
    
    //Updates the appearence of all playerInfos according to the player-object
        public void UpdateGameObjects(){
            foreach (Transform child in transform) {    //Destroy all exsting elements
                child.GetComponent<PlayerInfoScript>().UpdateGameObject();
            }
        }
    
               
        
   
    //Server+Client: The user clicked a button to change a character   
        public void ChangePlayerCharacterUserRequest(Player player){
            if(Network.isServer){
                int character = characters.GetUnoccupiedCharacter(player.character);
                playerList.PlayerChangedCharacter(player, character);
                networkView.RPC("ChangePlayerCharacter", RPCMode.Others, player.networkPlayer, character);
            } else {
                networkView.RPC("ChangePlayerCharacterRequest", RPCMode.Server, player.networkPlayer); 
            }
        }
                               
    //Server: A player wants to change its character (is called within the playerInfoScript)
        [RPC]
        void ChangePlayerCharacterRequest(NetworkPlayer player, NetworkMessageInfo info){
            if(!Network.isServer){
                return;
            }           
            if(player != info.sender){   //Don't change other players´ color!
                print ("FORBID - DON'T CHANGE OTHER PLAYERS' COLOR");
                return;
            }
            Player pl = playerList.GetPlayer(player);
            playerList.PlayerChangedCharacter(pl, characters.GetUnoccupiedCharacter(pl.character));     //Set the character on the server (don't do it in the RPC - this can lead to concurrency problems if several players try to change its character at once)  
            networkView.RPC("ChangePlayerCharacter", RPCMode.Others, player, pl.character);
            UpdateGameObjects();                 
        }
	    
    //Clients: change the character of a player (The server changed it already
        [RPC]
        void ChangePlayerCharacter(NetworkPlayer player, int character, NetworkMessageInfo info){
            playerList.PlayerChangedCharacter(player, character);
        }
        
}
