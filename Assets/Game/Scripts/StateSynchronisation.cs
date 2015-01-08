using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

/*
 * Helper
 * Cares about state synchronisation between all clients.
 * State changes can be initiated by calling "ChangeGameStateRequest()" on the Server
 */


public enum GameState { Initialising        =0, 
                        UserInteraction     =1,     //The user (if still alive) can send ships and upgrade Planets
                                                    //All interactions are immediately sent to the server and then to all other players. No reverting possible.
                        ShipProduction      =2,     //All planets are producing ships. If a user upgraded a planet before, the modification is already included
                                                    //Handled by every player on its own (not authoritative)
                        FightEvaluation     =3,     //Ships are arriving at planets. Planets change their owner. The fight evaluation is done by each player individually
                        GlobalEvents        =4,     //The server initiates special events (sun erruptions,... )
                        /*optional * /MapSynchronisation, /**/     //The server updates the complete map, so that everything stays synchronised
                        CleanUp             =5      //The game is over, there is a winner
};



//Every player starts with the state "Initialising".
//
//  The server initiates state-changes and defines the next state.
//  For specific states, a player can inform the others to be "ready" or "not ready" - this allows the server to know when to switch the state. Other players can display it in the gui
//  After a state-Change, all players are set not-ready automatically by each player. the server doesn't initiate that
//



public class StateSynchronisation : MonoBehaviour {
    PlayerList playerList;
    UIHandler uiHandler;
    List<I_StateSynchronisationObserver> observers;

    public GameState gameState {get; private set;}

    //List<int> PlayerIsReady;

    
    public StateSynchronisation(){
        gameState = GameState.Initialising;
        this.observers = new List<I_StateSynchronisationObserver>();
    }

    void Awake() {
        playerList = GameObject.Find("PlayerList").GetComponent<PlayerList>();
        if (playerList == null) {
            throw new MissingComponentException("Unable to find PlayerList.");
        }
        uiHandler = GameObject.Find("UIHandler").GetComponent<UIHandler>();
        if (uiHandler == null) {
            throw new MissingComponentException("Unable to find UIHandler.");
        }
    }
    


    //Server + Client: A player is (not) ready for the next state change (Sent by both server+client)
    //Note: the GameState is also sent. If the current GameState!=validForGameState, the isReady-request is deprecated and must be ignored
        [RPC]
        void SetPlayerReady(bool isReady, int/*GameState*/ _validForGameState, NetworkPlayer networkPlayer) {
            GameState validForGameState = (GameState)_validForGameState;
            if (validForGameState != gameState) {
                Debug.LogWarning("Deprecated isReady RPC - ignoring request.");
                return;
            }
            Player player = playerList.GetPlayer(networkPlayer);
            Debug.Log("The player "+player.ToString()+" is "+(isReady?"":"not ")+"ready");
            player.isReady = isReady;

            PlayerReadyChanged(player);
        }    

    //Server + Client: Change the game State (Called by the server)
        [RPC]
        void ChangeGameState(int/*GameState*/ _nextState){
            GameState nextState = (GameState)_nextState;
            playerList.NoPlayerIsReady();   //No player is ready anymore (this doesn't need to be synchronised - everyone knows what to do)
            PlayerReadyChanged(null);
            gameState = nextState;
        }

    //Server + Client: We get alerted, because we aren't ready yet; Called by server+client
        [RPC]
        void GetAlerted(float r, float g, float b) {
            uiHandler.AlertUser(new Color(r,g,b));
        }

    //Server + Client: Alter all users, that aren't ready yet
        public void AlertNotReadyUsers(){
            int count = playerList.GetPlayerCount();
            Color color = playerList.GetPlayer().GetColor();
            for (int i = 0; i < count; ++i){
                Player player = playerList.GetPlayerByIndex(i);
                if (!player.isReady){
                    if (player.networkPlayer == Network.player) {   //Stupid you
                        GetAlerted(color.r, color.g, color.b);
                    } else {
                        networkView.RPC("GetAlerted", player.networkPlayer, color.r, color.g, color.b);
                    }
                    
                }
            }
        }




    //Server: the gameState should be changed
        public void ChangeGameStateRequest(GameState nextState){
            if(Network.isClient){
                throw new UnityException("Clients are not allowed to change the gameState.");
            }
            networkView.RPC("ChangeGameState", RPCMode.All, (int)nextState);
        }
	
    //Server+Client: The ready-flag should be changed
        public void SetReadyRequest(bool isReady){
            networkView.RPC("SetPlayerReady", RPCMode.All, isReady, (int)gameState, Network.player); //Send the current gameState. If the state changes before the request can be handled, it has to be ignored.
        }

        public void AddObserver(I_StateSynchronisationObserver observer)
        {
            this.observers.Add(observer);
        }

        public void RemoveObserver(I_StateSynchronisationObserver observer)
        {
            this.observers.Remove(observer);
        }

        private void PlayerReadyChanged(Player player){
            this.observers.ForEach(o => o.OnPlayerReadyChanged(player, gameState));
        }


    //Returns true, if a specific player is ready. If null is given, returns if the own instance is ready
        public bool IsPlayerReady(Player player = null) {
            return player.isReady;
        }
    //Returns true, if all players are ready
        public bool AreAllPlayersReady() {
            return playerList.AreAllPlayersReady();
        }

    //Avoid new players from connecting:
        void OnPlayerConnected(NetworkPlayer player) {
            Network.CloseConnection(player, true);
        }
}
