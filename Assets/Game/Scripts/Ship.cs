using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class Ship : MonoBehaviour {

    public ShipMovement ShipMovement { get; set; }
    public UIHandler UiHandler { private get; set; }

    public void SingleTouchClick()
    {
        Debug.Log("clicked ship: ");
        UiHandler.ShowShipDetailPanel(this);
    }
}
