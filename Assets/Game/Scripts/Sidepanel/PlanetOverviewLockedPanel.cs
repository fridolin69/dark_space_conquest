using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlanetOverviewLockedPanel : MonoBehaviour {


    public Text Owner;

	// Update is called once per frame
    public void UpdatePlanet(PlanetEntity planet)
    {
        if (planet.owner != null){
            Owner.text = "Owner: " + planet.owner.name;
        } else {
            Owner.text = "Owner: Neutral";
        }
    }
}
