using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlanetOverviewPanel : MonoBehaviour
{

    public Text FactoryCount;
    public GameObject UpgradeFactoryButton;

    public Text ShipsCount;
    public GameObject UpgradeHangarButton;

    public UnityEngine.UI.Button SendShipsButton;

    public void UpdatePlanetInfo(PlanetEntity planet, bool activateUpgradeButtons = true)
    {
        ShipsCount.text = planet.ships + "/" + planet.hangarSize;
        FactoryCount.text = "+" + planet.factorySpeed;
        UpgradeFactoryButton.SetActive(activateUpgradeButtons);
        UpgradeHangarButton.SetActive(activateUpgradeButtons);

        if (planet.ships == 0)
        {
            SendShipsButton.interactable = false;
        }
        else
        {
            SendShipsButton.interactable = true;
        }
    }
}