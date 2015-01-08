using UnityEngine;
using System.Collections;

public interface I_GameState {
    bool Update(bool firstFrameOfState);        //Is called for each frame. Returns true if the state finished and the next gameState should be started (isReady)
                                                //NOTE: returning true doesn't guarantee, that the gameState is really changed - maybe the server needs to wait for other players too
}
