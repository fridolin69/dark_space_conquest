using UnityEngine;
using System.Collections;


public enum ShipMovementEventType { 
    NewShipMovement
};

public interface I_ShipMovementObserver {
    void OnShipMovementEvent(ShipMovementEventType eventType, ShipMovement shipMovement);
}
