using UnityEngine;
using System.Collections;


public enum MapEventType { PlanetAdded, MapCleared, PlanetOwnerChanged, PlanetUpgrade };

public interface I_MapObserver {
    void OnMapEvent(MapEventType mapEventType, PlanetEntity planet);
}
