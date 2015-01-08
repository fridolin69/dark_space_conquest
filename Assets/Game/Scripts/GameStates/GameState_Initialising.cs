using UnityEngine;
using System.Collections;


/* Helper.
 * GameState - state
 * Cares about initialising everything
 */

public class GameState_Initialising {
    Map map;

    public GameState_Initialising() {
        map = GameObject.Find("Map").GetComponent<Map>();
        if (map == null) {
            throw new MissingComponentException("Unable to find Map.");
        }
    }

    public void Update(bool firstFrameOfState) {
        if (!firstFrameOfState) {   //Already finished
            return;
        }

        int planetCount = map.GetPlanetCount();
        for (int i = 0; i < planetCount; ++i) {
            map.GetPlanetByIndex(i).alwaysShowFlag = false;     //startplanets don't show a white flag if they're neutral
        }
        return;
    }
}
