using UnityEngine;
using System.Collections;
using System.Security.Permissions;
using UnityEngine.UI;

public class SendShipsPanel : MonoBehaviour
{

    public Text ShipsCount;
    public Text TargetPlanetName;
    public Text TravelTimeText;

    public Slider Slider;
    public Text SliderValueText;
    public Text SliderValueTextRemaining;
    public UnityEngine.UI.Button SendButton;

    public PlanetEntity TargetPlanet { get; private set; }
    public PlanetEntity SourcePlanet { get; private set; }
    public int Slidervalue { get; private set; }
    


    public void UpdatePanel(PlanetEntity planet)
    {
        SourcePlanet = planet;
        TargetPlanetName.text = "-------------";
        TravelTimeText.text = "--- Days";

        ShipsCount.text = planet.ships + "/" + planet.hangarSize;
        Slider.minValue = 1;
        Slider.maxValue = planet.ships;
        Slider.value = (int)(planet.ships/2);
        SliderValueChanged();
        
        SendButton.interactable = false;
     
        if (planet.ships == 0)
        {
            Slider.minValue = 0;
        }
        

    }

    public void UpdateTargetPlanet(PlanetEntity target)
    {
        if (SourcePlanet.ships == 0 || target == SourcePlanet)
        {
            return;
        }
        if (TargetPlanet != null )
        {
            TargetPlanet.DisableOutline();
        }
        SendButton.interactable = true;
        target.SetOutline(Color.red);
        TargetPlanet = target;
        TargetPlanetName.text = target.planetName;
        TravelTimeText.text = target.GetTravelTime(SourcePlanet) + " Days";
    }



    public void SliderValueChanged()
    {
        Slidervalue = (int) Mathf.Floor(Slider.value);
        if (Slidervalue == 0 && SourcePlanet.ships == 1){
            Slidervalue = 1;
        }
        SliderValueText.text = Slidervalue.ToString();
        SliderValueTextRemaining.text = "/  " + (SourcePlanet.ships - Slidervalue);
    }

}
