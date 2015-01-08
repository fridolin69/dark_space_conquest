using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;



/*
 * Entity.
 * Represents a connected Player.
 * Doesn't contain any special Logic.
 */




//This enum is only important for the SERVER - the server needs to know the sync-status of each connected player.
public enum PlayerStatus {
    Idle,               //The player is connected but NOT SYNCHRNISED yet
    PlayerListSync,     //The playerList on the player is not up-to-date - it is currently being synchronised
    Synchronised        //Ready - the playerList on the player is up-to-date
};




public class Player{
	public const int MAX_PLAYER_NAME_LENGTH = 16;

    public PlayerStatus playerStatus = PlayerStatus.Idle;           //This field is set/important for the SERVER ONLY - the server needs to know the sync-status of each connected player.

    public NetworkPlayer networkPlayer { get; private set; }        //The network representation of this character
   
    public int character {get; set; }                               //The character of this player (character = image+color that represent a player. The class "Character.cs" can be used to get the color & co)
   
    private string _name = "New Player";
	public string name {                                            //The player name
        get { return _name; }
		set{
			if(value.Length <= MAX_PLAYER_NAME_LENGTH){
                this._name = value;
			}else{
				Debug.LogError("Unable to set Player name: name too long");
			}
		}
	}


    //Needed inside the game: If the player is ready for a gameState change. Handled by the "StateSynchronisation.cs" script with the help of the "PlayerList.cs" script
        public bool isReady { get; set; }
    
    
    public Player(int character, string name, NetworkPlayer networkPlayer) {
        this.character = character;
        this.name = name;
        this.networkPlayer = networkPlayer;
        this.isReady = false;
    }

    public Color GetColor() {
        return Characters.GetCharacterColor(character);
    }

    public override string ToString() {
        return "Player \"" + name + "\", char " + character + ", networkPlayer " + networkPlayer;
    }
}
