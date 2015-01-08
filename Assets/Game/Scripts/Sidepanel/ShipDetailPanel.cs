using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class ShipDetailPanel : MonoBehaviour {

    //public Image ShipImage;
    public Text ShipCount;
    public Text ArrivalDay;
    public Text Destination;
    public Text Source;

    public void UpdatePanel(Ship ship )
    {
        ShipCount.text = ship.ShipMovement.ShipCount.ToString();
        ArrivalDay.text = "Day " + ship.ShipMovement.ArrivalDay + "(+" + (ship.ShipMovement.ArrivalDay - StateManager.CurrentDay) + ")";
        Destination.text = ship.ShipMovement.Destination.planetName;
        Source.text = ship.ShipMovement.Source.planetName;
    }
}
