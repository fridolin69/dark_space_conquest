using UnityEngine;
using System.Collections;


/* Helper.
 * GameState - state
 * All planets are producing ships. If a user upgraded a planet before, the modification is already included
 * Handled by every player on its own (not authoritative)
 */




public class GameState_ShipProduction {
    Map map;

    public GameState_ShipProduction() {
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
        int freeSpace;
        PlanetEntity planet;
        for (int i = 0; i < planetCount; ++i) {
            planet = map.GetPlanetByIndex(i);
            freeSpace = planet.hangarSize - planet.ships;
            if (freeSpace < 0) {
                freeSpace = 0;
            }
            if (freeSpace < planet.factorySpeed) {
                planet.ships += freeSpace;
            } else {
                planet.ships += planet.factorySpeed;
            }
        }
        return;
    }
}
