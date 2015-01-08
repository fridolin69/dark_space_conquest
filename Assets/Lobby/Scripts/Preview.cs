using UnityEngine;
using System.Collections;


/*
 * Helper
 * Responsible for the map preview in the GUI.
 * Cares about correct positioning/scaling of the map gameobject
 */
public class Preview : MonoBehaviour/*, I_MapObserver*/ {

    public const float PREVIEW_BORDER_PADDING = 3;     //space between preview and screen border

    Map map;

    void Awake() {
        map = GameObject.Find("Map").GetComponent<Map>();
        if (map == null) {
            throw new MissingComponentException("Unable to find Map.");
        }
    }
    
    //Position the map correctly (needs to be placed within update to provide correct placing when rotating/resizing) 
        void Update() {
            Vector2 mapSize = map.GetMapSize();
            if (mapSize.x <= 0 || mapSize.y <= 0) {     //No planets on map yet
                return;
            }
            float mapRatio = mapSize.x / mapSize.y;
 
            Vector2 space = new Vector2(Camera.main.aspect * Camera.main.orthographicSize - PREVIEW_BORDER_PADDING, Camera.main.orthographicSize - PREVIEW_BORDER_PADDING); //Available space (only works with orthographic cam)
            float spaceRatio = space.x / space.y;                        
                                     
            float scaling;
            if (mapRatio > spaceRatio) {    //Void at top/bottom
                scaling = space.x / mapSize.x;
            } else {                        //Void at left/right
                scaling = space.y / mapSize.y;
            }
            //0,0 = center of screen; position = center of right-upper screen quadrant
            map.SetCenterPosition(new Vector2(space.x/2, space.y/2), scaling);  
        }

}
