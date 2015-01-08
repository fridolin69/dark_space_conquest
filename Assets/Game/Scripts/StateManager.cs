using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class StateManager : MonoBehaviour {
    StateSynchronisation stateSynchronisation;

    GameState lastState;


    //Map map;
    public Toggle IsReadyToggle;
    public Text DayCounter;

    GameState_Initialising gameState_Initialising;
    GameState_ShipProduction gameState_ShipProduction;
    GameState_FightEvaluation gameState_FightEvaluation;

    ShipMovementHandler shipMovementHandler;
    UIHandler uiHandler;

    public static int CurrentDay { get; private set; }     //The current day; needed to find out when shipMovements arive. set automatically when a new day/round begins (within this class).



	// Use this for initialization
	void Awake () {
        //map = GameObject.Find("Map").GetComponent<Map>();
        //if (map == null) {
        //    throw new MissingComponentException("Unable to find Map.");
        //}

        shipMovementHandler = GameObject.Find("Synchroniser").GetComponent<ShipMovementHandler>();
        if (shipMovementHandler == null) {
            throw new MissingComponentException("Unable to find ShipMovementHandler.");
        }
        uiHandler = GameObject.Find("UIHandler").GetComponent<UIHandler>();
        if (uiHandler == null) {
            throw new MissingComponentException("Unable to find UIHandler.");
        }

        stateSynchronisation = GameObject.Find("Synchroniser").GetComponent<StateSynchronisation>();
        if (stateSynchronisation == null) {
            throw new MissingComponentException("Unable to find StateSynchronisation.");
        }
        lastState = GameState.CleanUp;     //there was nothing before this state, but we cant set it to initialising. CleanUp is the only state which makes sense (maybe there was already a game)

        gameState_Initialising = new GameState_Initialising();
        gameState_ShipProduction = new GameState_ShipProduction();
        gameState_FightEvaluation = new GameState_FightEvaluation();

        StateManager.CurrentDay = 1;
    }



    bool lastIsReadyResult;         //Is needed within the Update()-method, in order to check if the isReady-state changed since the last frame (yes: send a new RPC)
    bool changeGameStateRequestSent;    //Is set to true as soon as the changeGameStateRequest has been sent (in order to avoid additional RPC-calls)



	// Update is called once per frame.
    // Cares about calling the correct state-implementation and state-changes (the state is switched via RPCs in "StateSynchronisation.cs"
	void Update () {
        GameState gameState = stateSynchronisation.gameState;
        bool firstFrameOfState = (lastState != gameState);
        if (firstFrameOfState) { 
            changeGameStateRequestSent = false;
            IsReadyToggle.isOn = false;
            lastIsReadyResult = false;
        }
        bool isReady;


        //Has to set isReady true if everything is completed (isReady)
        //Note that Update() can be called again even after returning true, since the server might have to wait for other players
        //Note that the gameState can also change if the function returns false (Only possible, if it returned true in a previous frame and the false-request wasn't handled yet)
        switch (gameState) {
            case GameState.Initialising:    gameState_Initialising.Update(firstFrameOfState);
                                            isReady = true; 
                                            break;
            case GameState.FightEvaluation: //DayCounter.text = "Night: " + StateManager.CurrentDay + " - Conquer"; 
                                            gameState_FightEvaluation.Update(firstFrameOfState);
                                            //if (firstFrameOfState) {
                                            //    uiHandler.OnNextDayHandler();
                                            //}
                                            isReady = true;//IsReadyToggle.isOn;
                                            break;
            case GameState.UserInteraction: //DayCounter.text = "Day: " + StateManager.CurrentDay + " - Command"; 
                                            isReady = IsReadyToggle.isOn;
                                            break;
            case GameState.ShipProduction:  gameState_ShipProduction.Update(firstFrameOfState);
                                            isReady = true; 
                                            break;
            case GameState.GlobalEvents:    //Do some cleanups for the next day:
                                            {   if (firstFrameOfState){
                                                    //map.ClearEvaluationEvents();
                                                    ++StateManager.CurrentDay;
                                                    uiHandler.InitiateDayFading();
                                                    shipMovementHandler.UpdateGraphicalShipMovements();
                                                    DayCounter.text = "Day: " + StateManager.CurrentDay;
                                                } 
                                                isReady = true;
                                                Debug.LogWarning("Global events not implemented yet");
                                            } break;
            case GameState.CleanUp:         throw new UnityException("Not implemented yet");
            default: throw new UnityException("Invalid GameState");
        }

        if (isReady != lastIsReadyResult || (firstFrameOfState && isReady)) {           //The isReady-flag has to be changed on all clients
            stateSynchronisation.SetReadyRequest(isReady);
        }

        if (Network.isServer && !changeGameStateRequestSent && stateSynchronisation.AreAllPlayersReady()) {          //Change the gameState
            GameState nextGameState;
            switch (gameState) {
                case GameState.Initialising:    nextGameState = GameState.FightEvaluation; break;
                case GameState.FightEvaluation: nextGameState = GameState.UserInteraction; break;
                case GameState.UserInteraction: nextGameState = GameState.ShipProduction; break;
                case GameState.ShipProduction:  nextGameState = GameState.GlobalEvents; break;
                case GameState.GlobalEvents:    //Todo: check for end-situation here
                                                nextGameState = GameState.FightEvaluation;
                                                break;
                case GameState.CleanUp:         throw new UnityException("Not implemented yet"); //break;
                default: throw new UnityException("Invalid GameState");
            }
            stateSynchronisation.ChangeGameStateRequest(nextGameState);
            changeGameStateRequestSent = true;
        }

        lastIsReadyResult = isReady;
        lastState = gameState;
	}

//    private void AlertNotReadyUsers(){
//        stateSynchronisation.AlertNotReadyUsers();
//    }
}
