using UnityEngine;
using System.Collections;


public enum EvaluationEventType {
    Supply,           //A planet received supply ships
    AttackedPlanet,   //The player attacked another planet
    GotAttacked,      //A planet of the player got attacked by another planet
    AttackViewer      //Another player attacked another planet and won - the ownership changed.
};



public enum EvaluationEventOutcome {
    Success,        //Good for the player
    Neutral,        //The planet got neutral. Neither good, nor bad
    Lost,           //Bad for the player
};




/*

Wenn man verstärkung bekommt (von sich selbst):     EvaluationEventType = Supply
    You received
     +134 ships                                 usedShips-lostShips

Nur, Wenn der Hangar voll is:                       EvaluationEventOutcome = Lost
    Hangar Full!
    6 ships lost                                lostShips

Immer sichtbar:
 250 ships available                            shipsOnPlanet












Wenn man selber jemand anderen angegriffen hat:          evaluationEventType = AttackedPlanet
     You attacked
      playername                                otherPlayer.  Wenn null, dann den Text "a neutral planet" ausgeben
         with
       150 ships                                usedShips

Wenn man verloren hat ODER ein neutraler planet neutral bleibt:         EvaluationEventOutcome = Lost
         Lost!
There are no survivors.

Wenn der Planet neutral wurde:                    EvaluationEventOutcome = Neutral
         Lost!
The planet is now neutral

Wenn der Planet erobert wurde:                  evaluationEventOutcome = success
        Victory!
   80 ships survived.                         shipsOnPlanet











Wenn man angefriffen wird und überlebt hat:     evaluationEventType = GotAttacked, EvaluationEventOutcome = Success
      playername                             otherPlayer
     attacked with
       200 ships                              usedShips

       Survived!
  12 ships remaining                          shipsOnPlanet








Wenn man angefriffen wird und der Planet neutral wurde:       evaluationEventType = GotAttacked, EvaluationEventOutcome = Neutral
      playername                              otherPlayer
     attacked with
       200 ships                              usedShips

         Lost!
The planet is now neutral.



Wenn man angegriffen wird, und der Planet jetzt dem Gegner gehört:      evaluationEventType = GotAttacked, evaluationEventOutcome = Lost
      playername                            otherPlayer
       attacked

        Lost!
There are no survivors.




Wenn ein anderer Spieler einen anderen Planeten angegriffen hat, und gewonnen hat:              evaluationEventType = AttackViewer, evaluationEventOutcome = Lost
  playername                              otherPlayer
   attacked
  playername                              otherAttackedPlayer  (Wenn null: den string "neutral planet" ausgeben)
   and won.


Wenn ein anderer Spieler einen anderen Planeten angegriffen hat, und der jetzt neutral ist:      evaluationEventType = AttackViewer, evaluationEventOutcome = Neutral;
  playername                              otherPlayer
   attacked
  playername                            otherAttackedPlayer

The planet is now neutral.


*/



public class EvaluationEvent {
    
    //private ShipMovement shipMovement;                              //The shipMovement, that caused the event
    public EvaluationEventType evaluationEventType { get; private set; }
    public EvaluationEventOutcome evaluationEventOutcome { get; private set; }


    public Player otherPlayer;                                      //The other player which is involved (in case of a supply: null)
    public Player otherAttackedPlayer;                              //Only set in the EventType "AttackViewer"
    public int usedShips { get; private set; }                      //The number of used ships to get the outcome
    public int shipsOnPlanet { get; private set; }                  //The number of ships, that are on the planet AFTER the event
    public int lostShips { get; private set; }                      //The number of ships that have been lost
    
    public bool isRelevantForPlayer { get; private set; }           //Is true, if the event is relevant for the current player (if the player should see the information)



    public EvaluationEventOutcome positiveEvent { get; private set; }   //defines the icon color of the planet-icon, that is used to show this event. (green/yellow/red)
    public float importance { get; private set; }                       //0: not important; 50: normal; 100: important;



//Evaluates an event (ship calculations, owner changes,...) and prepares the output to be displayed in the sidebar
    public EvaluationEvent(ShipMovement shipMovement) {
        if (shipMovement.Owner == shipMovement.Destination.owner) {     //Supply
            evaluationEventType = EvaluationEventType.Supply;
            otherPlayer = null;
            usedShips = shipMovement.ShipCount;

            int remainingSpace = shipMovement.Destination.hangarSize - shipMovement.Destination.ships;  //Remaining space on the destination
            if (remainingSpace < 0) { remainingSpace = 0; }

            if (remainingSpace < shipMovement.ShipCount) {          //Some ships will get lost
                evaluationEventOutcome = EvaluationEventOutcome.Lost;
                shipMovement.Destination.ships += remainingSpace;                                   //The first ships make the hangar full
                lostShips = Mathf.RoundToInt((shipMovement.ShipCount - remainingSpace) * 0.5f);     //All other ships will get a 50% 
                shipMovement.Destination.ships += shipMovement.ShipCount - remainingSpace - lostShips;
                importance = Mathf.Lerp(50, 100, Mathf.Min(1, lostShips/500) );
                positiveEvent = EvaluationEventOutcome.Lost;        //negative - red icon
            } else {
                evaluationEventOutcome = EvaluationEventOutcome.Success;
                shipMovement.Destination.ships += shipMovement.ShipCount;
                lostShips = 0;
                importance = 40;
                positiveEvent = EvaluationEventOutcome.Success;     //green icon   
                //importance = Mathf.Lerp(35, 65, Mathf.Min(1, shipMovement.ShipCount/) );
            }
            shipsOnPlanet = shipMovement.Destination.ships;
            isRelevantForPlayer = (shipMovement.Owner.networkPlayer == Network.player);
            Debug.Log("A supply of " + usedShips + " reached planet " + shipMovement.Destination.planetName + " - " + lostShips + " ships were lost due to full hangar");
            return;
        }

    //One person attacked another one:
        usedShips = shipMovement.ShipCount;
                        
        //The planet got neutral (wasn't neutral before):
        if (shipMovement.Destination.ships == shipMovement.ShipCount && shipMovement.Destination.owner != null) {       //The planet will become neutral
            evaluationEventOutcome = EvaluationEventOutcome.Neutral;
            shipMovement.Destination.ships = 0;
            bool wasNeutralBefore = (shipMovement.Destination.owner == null);   //If a neutral planet was attacked, nothing changed
            otherAttackedPlayer = shipMovement.Destination.owner;
            shipMovement.Destination.owner = null;            
            shipsOnPlanet = 0;              //No ships left
            lostShips = shipMovement.ShipCount;
            positiveEvent = EvaluationEventOutcome.Neutral;        //neutral - yellow flag; neither good, nor bad

            if (shipMovement.Owner.networkPlayer == Network.player) {       //We attacked another one
                evaluationEventType = EvaluationEventType.AttackedPlanet;
                isRelevantForPlayer = true;
                otherPlayer = otherAttackedPlayer;
                importance = 55;
                Debug.Log("You attacked the planet " + shipMovement.Destination.planetName + " with " + usedShips + " ships, and now it is neutral.");
            }else
            if (shipMovement.Destination.owner != null && shipMovement.Destination.owner.networkPlayer == Network.player) {     //We got attacked!
                evaluationEventType = EvaluationEventType.GotAttacked;
                isRelevantForPlayer = true;
                otherPlayer = shipMovement.Owner;
                importance = Mathf.Lerp(60, 100, Mathf.Min(1, lostShips / 500));
                Debug.Log("The planet " + shipMovement.Destination.planetName + " has been attacked with " + usedShips + " ships, and now it is neutral.");
            } else if(!wasNeutralBefore){
                evaluationEventType = EvaluationEventType.AttackViewer;     //we only watched
                evaluationEventOutcome = EvaluationEventOutcome.Neutral;
                isRelevantForPlayer = true;
                otherPlayer = shipMovement.Owner;
                
                importance = 15;
                Debug.Log(shipMovement.Owner.name + " attacked the planet " + shipMovement.Destination.planetName + ". The planet is now neutral.");
            } else {
                Debug.Log(shipMovement.Owner.name + " attacked the planet " + shipMovement.Destination.planetName + ". The planet stays neutral.");
            }
            return;
        }

        //The planet owner wins (or a neutral planet stays neutral):
        if (shipMovement.Destination.ships >= shipMovement.ShipCount) {                  
            shipMovement.Destination.ships -= shipMovement.ShipCount;     
            positiveEvent = EvaluationEventOutcome.Lost;        //if we attacked and lost: bad; if another player is attacking us: bad -> red icon

            if (shipMovement.Owner.networkPlayer == Network.player) {           //We attacked another planet without success
                evaluationEventType = EvaluationEventType.AttackedPlanet;
                evaluationEventOutcome = EvaluationEventOutcome.Lost;
                isRelevantForPlayer = true;
                otherPlayer = shipMovement.Destination.owner;
                shipsOnPlanet = -1;  //We must not know how many ships there are on that planet
                importance = Mathf.Lerp(30, 60, Mathf.Min(1, shipMovement.ShipCount / 500));
                Debug.Log("You attacked the planet " + shipMovement.Destination.planetName + " with " + usedShips + " ships. It failed. ("+shipMovement.Destination.ships+" ships remaining)");
            }else
            if (shipMovement.Destination.owner != null && shipMovement.Destination.owner.networkPlayer == Network.player) {     //We got attacked and survived
                evaluationEventType = EvaluationEventType.GotAttacked;
                evaluationEventOutcome = EvaluationEventOutcome.Success;        //Survived
                isRelevantForPlayer = true;
                otherPlayer = shipMovement.Owner;
                shipsOnPlanet = shipMovement.Destination.ships;
                importance = Mathf.Lerp(30, 80, Mathf.Min(1, shipMovement.ShipCount / 500));
                Debug.Log("The planet " + shipMovement.Destination.planetName + " has been attacked with " + usedShips + " ships. You survived.");
            } else {
                Debug.Log(shipMovement.Owner.name + " attacked the planet " + shipMovement.Destination.planetName + " and lost.");
            }
            return;
        }


        //The planet owner lost:
        shipMovement.Destination.ships = shipMovement.ShipCount - shipMovement.Destination.ships;
        otherAttackedPlayer = shipMovement.Destination.owner;
        shipMovement.Destination.owner = shipMovement.Owner;

        if (shipMovement.Destination.ships > shipMovement.Destination.hangarSize) {     //All ships couldn't land
            lostShips = Mathf.RoundToInt((shipMovement.Destination.ships - shipMovement.Destination.hangarSize) * 0.5f);     //50% of all ships that hadn't enough space lost
            shipMovement.Destination.ships -= lostShips;
        } else {
            lostShips = 0;
        }
        


        if (shipMovement.Owner.networkPlayer == Network.player) {           //We attacked another planet and won
            evaluationEventType = EvaluationEventType.AttackedPlanet;
            evaluationEventOutcome = EvaluationEventOutcome.Success;
            isRelevantForPlayer = true;
            otherPlayer = otherAttackedPlayer;
            shipsOnPlanet = shipMovement.Destination.ships;
            importance =80;
            positiveEvent = EvaluationEventOutcome.Success;        //green icon
            Debug.Log("You attacked the planet " + shipMovement.Destination.planetName + " with " + usedShips + " ships. You won with " + shipMovement.Destination.ships + " survivors.");
        }else
            if (otherAttackedPlayer != null && otherAttackedPlayer.networkPlayer == Network.player) {     //We got attacked and lost
            evaluationEventType = EvaluationEventType.GotAttacked;
            evaluationEventOutcome = EvaluationEventOutcome.Lost;       
            isRelevantForPlayer = true;
            otherPlayer = shipMovement.Owner;
            shipsOnPlanet = -1;                                             //We must not know with how many ships the oponent attacked
            importance = Mathf.Lerp(90, 100, Mathf.Min(1, shipMovement.ShipCount / 500));
            positiveEvent = EvaluationEventOutcome.Lost;        //negative - red icon
            Debug.Log("The planet " + shipMovement.Destination.planetName + " has been attacked and you lost. (The enemy had " + shipMovement.Destination.ships + " survivors).");
        } else {
            evaluationEventType = EvaluationEventType.AttackViewer;     //we only watched
            evaluationEventOutcome = EvaluationEventOutcome.Lost;
            isRelevantForPlayer = true;
            otherPlayer = shipMovement.Owner;
            importance = 25;
            positiveEvent = EvaluationEventOutcome.Lost;                //another player attacked - red icon (we could also make it yellow, but it would be strange
            Debug.Log(shipMovement.Owner.name + " attacked the planet " + shipMovement.Destination.planetName + " and won.");
        }
        
    }

}
