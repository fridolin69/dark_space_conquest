using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UpgradePanel : MonoBehaviour
{
    public enum Type
    {
        Factory, Hangar
    }

    public Text TitleText;
    public Text CurrentText;
    public Text UpgradeText;
    public Text CostText;
    public UnityEngine.UI.Button UpgradeButton;

    public Type _type { get; private set; }

    public void UpdatePanel(PlanetEntity planet, Type type)
    {
        //if (UpgradeButton == null)
        //{
        //    InitPanel();
        //}
        _type = type;
        UpgradeButton.interactable = true;
        CostText.color = Color.black;
        switch (type)
        {
            case Type.Factory:
                TitleText.text = "Upgrade Factory";
                CurrentText.text = "+" + planet.factorySpeed;
                UpgradeText.text = "+" + planet.GetNextFactoryUpgrade();
                CostText.text = planet.GetFactoryUpgradeCosts() + " ships";
                if (planet.ships < planet.GetFactoryUpgradeCosts())
                {
                    UpgradeButton.interactable = false;
                    CostText.color = Color.red;
                }
                break;

            case Type.Hangar:
                TitleText.text = "Upgrade Hangar";
                CurrentText.text = "" + planet.hangarSize;
                UpgradeText.text = "" + planet.GetNextHangarUpgrade();
                CostText.text = planet.GetHangarUpgradeCosts() +" ships";
                if (planet.ships < planet.GetHangarUpgradeCosts())
                {
                    UpgradeButton.interactable = false;
                    CostText.color = Color.red;
                }
                break;

            default:
                throw new ArgumentOutOfRangeException("type");
        }

    }

}
