using UnityEngine;
using System.Collections;


/**
 *  Note: Player != Character
 *      Each player gets a unique character. A character consists out of a sprite and a color.
 *      Both sprite and color are used in the lobby and the game to easily identify the player.
 *      If the program has less characters avaiable than there are players in the game, the first
 *      character (idx 0) might be used multiple times.
 *
 *  The character (int) is defined in the player-object. This class can be used to asign other (not-occupied)
 *      values, or to get the color of a specified character.
 */
using System.Collections.Generic;






public class Characters {
    
    PlayerList playerList;                      //Needed, to be able to return a random character which is not occupied yet
    static public int MAX_CHARACTERS = 10;      //Number of predefined characters in the program

    public Characters(PlayerList playerList){
        this.playerList = playerList;
    }
    public Characters(){
        playerList = GameObject.Find ("PlayerList").GetComponent<PlayerList>();
        if (playerList == null) {
            throw new MissingComponentException ("Unable to find PlayerList.");
        }      
    }
    
      
    //Returns the color of a given player character    
        static public Color GetCharacterColor(int character){
            switch(character){
                case 0: return Color.red; 
                case 1: return Color.gray; 
                case 2: return new Color(159/255f, 85/255f, 61/255f); //dark red
                case 3: return new Color(1, 107/255f, 3/255f);      //orange
                case 4: return Color.blue; 
                case 5: return Color.green; 
                case 6: return Color.magenta; 
                case 7: return new Color(54/255f, 174/255f, 160/255f);  //cyan
                case 8: return new Color(159/255f, 156/255f, 102/255f); //brown
                case 9: return new Color(160/255f, 108/255f, 163/255f); //purple
                default: Debug.LogError("Invalid character - There are only "+MAX_CHARACTERS+" avaiable, but the "+character+". color has been requested."); //starts with 0
                         return Color.white;
            }
        }

   //Returns the color of a given player    
        static public Color GetCharacterColor(Player player) {
            return GetCharacterColor(player.character);
        }

    ////Returns the sprite of a given player character  
    //    static public Sprite GetCharacterSprite(int character) {
    //        float width = 1 / MAX_CHARACTERS;
    //        float x = character * width;
    //        return Sprite.Create(characterSprites, new Rect(x, 0, width, 1), new Vector2(0, 0));
    //    }

    ////Returns the sprite of a given player    
    //    static public Sprite GetCharacterSprite(Player player) {
    //        return GetCharacterSprite(player.character);
    //    }


    
    //Returns a character which hasn't been used yet. If there are no unused characters, the first char (0) is returned
        public int GetUnoccupiedCharacter(int startChar = 0){
            int playerCount = playerList.GetPlayerCount();
            for(int c = startChar+1; c< startChar+MAX_CHARACTERS; c++){   //Iterate through all possible characters, but not the current one
                bool used = false;
                for (int i = 0; i < playerCount; ++i) {
                    Player mate = playerList.GetPlayerByIndex(i);
                    if (mate.character == (c % MAX_CHARACTERS)) { //character already used
                        used = true;
                        break;
                    }
                }
                if(!used){
                    return c % MAX_CHARACTERS;
                }
            }
            Debug.LogWarning("Unable to asign a character to a player - not enough characters predefined.");
            return 0;
        }
    
    //Asigns a new (not-occupied) character to the given Player
        public void SetNextCharacterToPlayer(Player player){        
            player.character = GetUnoccupiedCharacter(player.character);
        }
    
    
    //checks, if a character is already used
        public bool isOccupied(int character){
            int playerCount = playerList.GetPlayerCount();            
            for (int i = 0; i < playerCount; ++i) {
                Player mate = playerList.GetPlayerByIndex(i);
                if(mate.character == character){ //character already used
                    return true;
                }
            }
            return false;
        }
}
