using UnityEngine;
using System.Collections;
using System.Collections.Generic;



/*
 *  Entity
 *  Holds information about all connected (and registered) Players
 *  Allows new players to join (RPC synchronisation logic embedded), allows players to change their color (the function is called by PlayerView.cs)
 */


public class PlayerList : MonoBehaviour {

    Chat chat;                                          //For system- and debug messages, as well as chat messages from the client     
    Characters characters;                              //Helper, to asign characters to Player-Objects

    public const string PLAYER_LIST_SYNC_ACK = "PLAYER_LIST_SYNC_ACK";

    int playerListVersion = 0;                          //This number is incremented everytime the playerList changes. Required for correct synchronisation. If the playerList is synchronised, this number is the same everywhere
    List<Player> playerList = new List<Player>();       //All connected and registered Players

    //Observer pattern:
    List<I_PlayerListObserver> PlayerListObserver = new List<I_PlayerListObserver>();       //Observers, that need to get notified if the client list changes
    




    void Awake (){
        chat = GameObject.Find("GUI").GetComponent<Chat>();			//For system- and debug messages, as well as chat messages from the client
        if (chat == null) {
            throw new UnityException("Unable to find chat");
        }
        characters = new Characters(this);
    }


    //Server: Lobby joined; Called by Unity
        void OnServerInitialized() {
			networkView.RPC ("PlayerEnteredLobby", RPCMode.All, Settings.PlayerName, characters.GetUnoccupiedCharacter(), Network.player);	//Don't buffer. new clients will be updated as soon as they join and tell us their name
        }

    //Client: Lobby joined; Called by Unity
        void OnConnectedToServer() {
            networkView.RPC("TellServerOurName", RPCMode.Server, Settings.PlayerName);
        }
     
    //Server: a new client joined (it will not be added to the list until we know its name); Called by Unity
        void OnPlayerConnected(NetworkPlayer unknownPlayer) {
            chat.SystemMessage("Player connected from: " + unknownPlayer.ipAddress + ":" + unknownPlayer.port);
        }

    //Server: A client registered itself and tells the server its name. The PlayerList now needs to be synchronised
        [RPC]
        void TellServerOurName(string playerName, NetworkMessageInfo info) {
            //Update the new client, so that he knows who else is in the lobby:
            foreach (Player entry in playerList) {
                entry.playerStatus = PlayerStatus.PlayerListSync;
                networkView.RPC("PlayerEnteredLobby", info.sender, entry.name, entry.character, entry.networkPlayer);
            }
            //Update the client itself and all others about the new player:
            networkView.RPC("PlayerEnteredLobby", RPCMode.All, playerName, characters.GetUnoccupiedCharacter(1), info.sender);
            //Request a synchronisation confirmation:           
            networkView.RPC("PlayerListSynced", RPCMode.All, ++playerListVersion);
        }
    //Server+Client: The server finished synchronising the playerList. The server now needs a confirmation
        [RPC]
        void PlayerListSynced(int playerListVersion, NetworkMessageInfo info){
            this.playerListVersion = playerListVersion;
            Debug.Log("The playerlist has been synchronised to version "+playerListVersion+".");
            networkView.RPC("PlayerListSyncedConfirmation", RPCMode.Server, playerListVersion);
        }
    //Server: A client confirms a successfull playerList synchronisation (The client also tells us his current version)
        [RPC]
        void PlayerListSyncedConfirmation(int playerListVersion, NetworkMessageInfo info){
            if (playerListVersion < this.playerListVersion) {   //The client confirmed an old playerList. Another synchronisation is already in progress.
                return;
            }
            Player player = GetPlayer(info.sender);
            Debug.Log("Confirmation: The playerList on client/player \"" + player.ToString() + "\" has been synchronised to version " + playerListVersion + ".");
            player.playerStatus = PlayerStatus.Synchronised;
            InformObserversAboutEvent(PlayerListEventType.PlayerUpdatedPlayerList, player);
        }
  
    //Server+Client: A registered player should be added to the playerList (called by the server) - the playerList is currently being synchronised...
        [RPC]
        void PlayerEnteredLobby(string name, int character, NetworkPlayer networkPlayer) {
            if (GetPlayer(networkPlayer) != null) { //Ensure that no player is inserted twice  (This can happen, if another client joins/registeres BEFORE this client has sent his name. The server then would inform this client about all currently connected players, including the one we already know
                return;
            }
            Player player = new Player(character, name, networkPlayer);
            playerList.Add(player);
            chat.SystemMessage(player.name + " joined the lobby", Characters.GetCharacterColor(character));
            InformObserversAboutEvent(PlayerListEventType.PlayerJoined, player);
        }


    //Server: A player disconnected; Called by Unity
        void OnPlayerDisconnected(NetworkPlayer oldPlayer) {
            //Update each client about the lost player:
            foreach (Player entry in playerList) {
                entry.playerStatus = PlayerStatus.PlayerListSync;
            } 
            networkView.RPC("PlayerLeftLobby", RPCMode.All, oldPlayer);							//Don't buffer. new clients will be updated as soon as they join and tell us their name		         
            //Request a synchronisation confirmation:           
            networkView.RPC("PlayerListSynced", RPCMode.All, ++playerListVersion);
        }
    
    //Is called on server+client, everytime a player should be removed from the list (called by the server)
        [RPC]
        void PlayerLeftLobby(NetworkPlayer networkPlayer) {
            //Remove player from the server list
            if (Network.isServer) {
                chat.SystemMessage("Player disconnected from: " + networkPlayer.ipAddress + ":" + networkPlayer.port);
            }            
            Player player = GetPlayer(networkPlayer);
            if (player == null) {     //The player didn't register yet
                return;
            }
            chat.SystemMessage(player.name + " disconnected", Characters.GetCharacterColor(player.character));
            playerList.Remove(player);
            InformObserversAboutEvent(PlayerListEventType.PlayerLeft, player);
        }
    

    //Returns the name of a registered networkplayer (if the name is already known)
        public string GetPlayerName(NetworkPlayer player) {
            Player playerInfo = GetPlayer(player);
            if(playerInfo != null){
                return playerInfo.name;
            }
            return null;
        }

    //Returns the player information of a registered networkplayer (if the player is already known)
        public Player GetPlayer(NetworkPlayer player) {
            foreach (Player entry in playerList) {
                if (entry.networkPlayer == player) {
                    return entry;
                }
            }
            return null;
        }
    //Returns the player-object of the own player
        public Player GetPlayer() {            
            return GetPlayer(Network.player);
        }

    //Returns a specific player-object (together with getCount, this can be used for iterating outside the object)
        public Player GetPlayerByIndex(int index) {
            return playerList[index];
        }
    //Returns the number of connected players
        public int GetPlayerCount() {
            return playerList.Count;
        }
    
    //Server + Client: Changes the character of a player. This function is called by [RPC]PlayerView.ChangePlayerCharacter()  in PlayerView.cs.
        public void PlayerChangedCharacter(NetworkPlayer networkPlayer, int character) {
            PlayerChangedCharacter( GetPlayer(networkPlayer), character );
        }
        public void PlayerChangedCharacter(Player player, int character) {
            player.character = character;
            InformObserversAboutEvent(PlayerListEventType.PlayerChangedCharacter, player);
        }

    //Adds an observer, which is notified whenever the playerlist changes
        public void AddObserver(I_PlayerListObserver observer){
            PlayerListObserver.Add(observer);
        }

    //Removes an obersver
        public void RemoveObserver(I_PlayerListObserver observer){
            PlayerListObserver.Remove(observer);
        }
    //Internal Helper. Informs all observers about a specific event
        void InformObserversAboutEvent(PlayerListEventType eventType, Player player) {
            foreach (I_PlayerListObserver observer in PlayerListObserver) {
                observer.OnPlayerListChanged(eventType, player);
            }
        }
    //Returns the list with all players
        //public List<Player> GetPlayerList(){
        //    return playerList;
        //}

    //Returns true, if all players are ready
        public bool AreAllPlayersReady() {
            foreach (Player entry in playerList) {
                if (!entry.isReady) {
                    return false;
                }
            }
            return true;
        }
    //Sets every player to not-ready
        public void NoPlayerIsReady() {
            foreach (Player entry in playerList) {
                entry.isReady = false;
            }
        }







    public override string ToString(){
        string connectedPlayers = "Connected and registered players:\n";
        foreach (Player entry in playerList) {
            connectedPlayers += (entry.ToString() + ",\n");
        }
        return connectedPlayers.Remove (connectedPlayers.Length -2);    //Remove the last \n
    }
    
}






