using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class EventOnPlanetPanel : MonoBehaviour
{

    public Text EvaluationType;

    public GameObject SupplyPanel;
    public Text SupRecievedShips;
    public Text SupNrShipsAvailable;
    public Text SupHangarFull;
    public Text SupShipsLost;

    public GameObject AttackedPlanetPanel;
    public Text AttPlPlayername;
    public Text AttPlNrShips;
    public Text AttPlOutcome;
    public Text AttPlOutcomeText1;
    public Text AttPlOutcomeText2;
    public Text AttPlOutcomeText3;
    public Text AttPlOutcomeText4;


    public GameObject GotAttackedPanel;
    public Text GotAttPlayername;
    public Text GotAttNrShips;
    public Text GotAttAttacked;
    public Text GotAttOutcome;
    public Text GotAttOutcomeText1;
    public Text GotAttOutcomeText2;

    public GameObject AttackViewer;
    public Text AttViePlayer1;
    public Text AttViePlayer2;
    public Text AttVieOutcome;
    public Text AttVieOutcomeText1;
    public Text AttVieOutcomeText2;


    public UnityEngine.UI.Button PreviousButton;
    public UnityEngine.UI.Button NextButton;

    private PlanetEntity _planet;
    private int _eventIndex = 0;


    public void UpdatePanel(PlanetEntity planet)
    {
        _eventIndex = 0;
        _planet = planet;
        DisableAllPanels();
        UpdateLocalPanel();
        UpdateButtons();
    }

    private void UpdateLocalPanel()
    {

        DisableAllPanels();
        EvaluationEvent evaluationEvent = _planet.GetEvaluationEventByIndex(_eventIndex);

        switch (evaluationEvent.evaluationEventType)
        {
            case EvaluationEventType.Supply:
                UpdateSupplyPanel(evaluationEvent);
                SupplyPanel.SetActive(true);
                break;

            case EvaluationEventType.AttackedPlanet:
                UpdateAttackedPlanetPanel(evaluationEvent);
                AttackedPlanetPanel.SetActive(true);
                break;

            case EvaluationEventType.GotAttacked:
                UpdateGotAttackedPanel(evaluationEvent);
                GotAttackedPanel.SetActive(true);
                break;

            case EvaluationEventType.AttackViewer:
                UpdateAttackViewer(evaluationEvent);
                AttackViewer.SetActive(true);
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void UpdateSupplyPanel(EvaluationEvent evaluationEvent)
    {
        SupRecievedShips.text = "+" + (evaluationEvent.usedShips - evaluationEvent.lostShips) + " ships";
        SupNrShipsAvailable.text = evaluationEvent.shipsOnPlanet.ToString();

        switch (evaluationEvent.evaluationEventOutcome)
        {
            case EvaluationEventOutcome.Success:
                SupHangarFull.text = "";
                SupShipsLost.text = "";
                break;
            case EvaluationEventOutcome.Neutral:
                //gibts des?
                break;
            case EvaluationEventOutcome.Lost:
                SupHangarFull.text = "Hangar full!";
                SupShipsLost.text = evaluationEvent.lostShips + " ships lost";
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void UpdateAttackedPlanetPanel(EvaluationEvent evaluationEvent)
    {
        if (evaluationEvent.otherPlayer == null)
        {
            AttPlPlayername.text = "a neutral planet";
        }
        else
        {
            AttPlPlayername.text = evaluationEvent.otherPlayer.name;
        }
        AttPlNrShips.text = evaluationEvent.usedShips + " ships";

        AttPlOutcome.text = "Lost!";
        AttPlOutcome.color = new Color(172, 0, 0);
        AttPlOutcomeText3.text = "";
        AttPlOutcomeText4.text = "";
        switch (evaluationEvent.evaluationEventOutcome)
        {
            case EvaluationEventOutcome.Success:
                {
                    AttPlOutcome.text = "Victory!";
                    AttPlOutcome.color = new Color(0, 172, 0);
                    AttPlOutcomeText1.text = evaluationEvent.shipsOnPlanet.ToString();
                    AttPlOutcomeText2.text = "ships survived";
                    if (evaluationEvent.lostShips > 0)
                    {
                        AttPlOutcomeText3.text = "Hangar Full!";
                        AttPlOutcomeText4.text = evaluationEvent.lostShips + " ships couldn't land";
                    }
                } break;
            case EvaluationEventOutcome.Neutral:
                AttPlOutcomeText1.text = "The planet is now";
                AttPlOutcomeText2.text = "neutral";
                break;
            case EvaluationEventOutcome.Lost:
                AttPlOutcomeText1.text = "There are no";
                AttPlOutcomeText2.text = "survivors";
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void UpdateGotAttackedPanel(EvaluationEvent evaluationEvent)
    {
        GotAttPlayername.text = evaluationEvent.otherPlayer.name;
        GotAttAttacked.text = "attacked with";
        GotAttNrShips.text = evaluationEvent.usedShips + " ships";
        switch (evaluationEvent.evaluationEventOutcome)
        {
            case EvaluationEventOutcome.Success:
                {
                    GotAttOutcome.text = "Survived!";
                    GotAttOutcome.color = new Color(0, 172, 0);
                    GotAttOutcomeText1.text = evaluationEvent.shipsOnPlanet.ToString();
                    GotAttOutcomeText2.text = "ships remaining";
                } break;
            case EvaluationEventOutcome.Neutral:
                {
                    GotAttOutcome.text = "Lost!";
                    GotAttOutcome.color = new Color(172, 0, 0);
                    GotAttOutcomeText1.text = "The planet is";
                    GotAttOutcomeText2.text = "now neutral";
                } break;


            case EvaluationEventOutcome.Lost:
                {
                    GotAttAttacked.text = "attacked you";
                    GotAttNrShips.text = "";

                    GotAttOutcome.text = "Lost!";
                    GotAttOutcome.color = new Color(172, 0, 0);
                    GotAttOutcomeText1.text = "There are no";
                    GotAttOutcomeText2.text = "survivors";

                } break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void UpdateAttackViewer(EvaluationEvent evaluationEvent)
    {
        AttViePlayer1.text = evaluationEvent.otherPlayer.name;
        if (evaluationEvent.otherAttackedPlayer == null)
        {
            AttViePlayer2.text = "a neutral planet";
        }
        else
        {
            AttViePlayer2.text = evaluationEvent.otherAttackedPlayer.name;
        }

        switch (evaluationEvent.evaluationEventOutcome)
        {
            case EvaluationEventOutcome.Success:
                //why not?
                throw new NotImplementedException("not expected status");
            // wenn ein spieler einen anderen angreift und verliert bekommt man das nicht mit
            //break;

            case EvaluationEventOutcome.Neutral:
                AttVieOutcome.text = "";
                AttVieOutcomeText1.text = "The planet is now";
                AttVieOutcomeText2.text = "neutral";
                break;

            case EvaluationEventOutcome.Lost:
                AttVieOutcome.text = "and won";
                AttVieOutcomeText1.text = "";
                AttVieOutcomeText2.text = "";
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void OnPreviousButton()
    {
        --_eventIndex;
        UpdateButtons();
        UpdateLocalPanel();
    }

    public void OnNextButton()
    {
        ++_eventIndex;
        UpdateButtons();
        UpdateLocalPanel();
    }

    private void UpdateButtons()
    {
        int eventListCount = _planet.GetEvaluationEventCount();

        PreviousButton.interactable = true;
        NextButton.interactable = true;

        if (_eventIndex == 0)
        {
            PreviousButton.interactable = false;
        }
        if ((_eventIndex + 1) == eventListCount)
        {
            NextButton.interactable = false;
        }
    }

    private void DisableAllPanels()
    {
        SupplyPanel.SetActive(false);
        AttackedPlanetPanel.SetActive(false);
        GotAttackedPanel.SetActive(false);
        AttackViewer.SetActive(false);
    }




}
