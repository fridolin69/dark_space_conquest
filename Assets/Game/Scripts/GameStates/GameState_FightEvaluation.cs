using UnityEngine;
using System.Collections;
using System.Collections.Generic;



/* Helper.
 * GameState - state
 * Ships are arriving at planets. Planets change their owner. The fight evaluation is done by each player individually
 */


public class GameState_FightEvaluation {

    Map map;
    ShipMovementHandler shipMovementHandler;

    public GameState_FightEvaluation() {
        map = GameObject.Find("Map").GetComponent<Map>();
        if (map == null) {
            throw new MissingComponentException("Unable to find Map.");
        } 
        shipMovementHandler = GameObject.Find("Synchroniser").GetComponent<ShipMovementHandler>();
        if (shipMovementHandler == null) {
            throw new MissingComponentException("Unable to find ShipMovementHandler.");
        }
    }    

    public void Update(bool firstFrameOfState) {
        if (!firstFrameOfState) {   //Already finished
            return;
        }
        map.ClearEvaluationEvents();            //Clear any events that haven't been dismissed yet.


        List<ShipMovement> shipMovements = shipMovementHandler.GetAndRemoveTodaysShipMovements();
        Debug.Log("Fight evaluation for day " + StateManager.CurrentDay + ": " + shipMovements.Count + " ship movements need to get evaluated.");
       
        foreach(ShipMovement movement in shipMovements){            
            EvaluationEvent evaluationEvent = new EvaluationEvent(movement);
            if (evaluationEvent.isRelevantForPlayer) {
                movement.Destination.AddEvaluationEvent(evaluationEvent);
            }
        }

        
    }
    
}
