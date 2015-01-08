using UnityEngine;
using System.Collections;

/*
 * GameObject
 * Is part of the "PlayerView.cs" and represents exacly one connected (and registered) Player.
 * If the user clicks on its character (change it), the RPC-function in the PlayerView is called.
 */

public class PlayerInfoScript : MonoBehaviour {

    public Texture2D characterSprites;    
    PlayerView playerView;
    Player player;
    
    	
        
    public void Initialise(PlayerView playerView){
        this.playerView = playerView; 
    }    
        
        
    public void SetPlayer(Player player){
        this.player = player; 
        UpdateGameObject();
    }
        
    void OnMouseDown(){
        print("ON_MOUSE_DOWN");
        playerView.ChangePlayerCharacterUserRequest(player);     
    }
    
    //Updates the appearence of this game object according to the player
        public void UpdateGameObject(){
            TextMesh playerName = GetComponentInChildren<TextMesh>();
            SpriteRenderer playerCharacter = transform.Find("PlayerCharacter").GetComponent<SpriteRenderer>();

            playerName.text = player.name;                              //Playername
            playerName.color = Characters.GetCharacterColor(player);    //Textcolor

            float width = characterSprites.width / Characters.MAX_CHARACTERS;
            float x = player.character* width;
            playerCharacter.sprite = Sprite.Create(characterSprites, new Rect(x, 0, width, characterSprites.height), new Vector2(0, 0));
        }
}
