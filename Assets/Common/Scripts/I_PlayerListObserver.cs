using System.Collections;
using UnityEngine;



public enum PlayerListEventType { 
    PlayerJoined,                   //A player registered and joined the lobby
    PlayerLeft,                     //A registered player left the Lobby
    PlayerUpdatedPlayerList,        //The playerList on a specific player is now up-to-date and synchronised
    PlayerChangedCharacter          //A player changed its character
};

public interface I_PlayerListObserver {
    void OnPlayerListChanged(PlayerListEventType eventType, Player player);
}
