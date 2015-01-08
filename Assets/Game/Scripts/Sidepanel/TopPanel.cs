using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TopPanel : MonoBehaviour {


    //public static PlanetEntity planet;
    public Text Planetname;
    public Image PlanetPreview; 

    public void UpdatePlanet(PlanetEntity planet)
    {
        PlanetPreview.sprite = planet.planetSprite;
        Planetname.text = planet.planetName;
    }

}
