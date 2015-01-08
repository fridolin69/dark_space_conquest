using System;
using UnityEngine;
using System.Collections.Generic;



/*
 * Entity
 * Stores and synchronises all shipMovements
 */

public class ShipMovementHandler : MonoBehaviour
{

    public GameObject GraphicalShipMovementPrefab;
    public GameObject UiHandlerGameObject;
    private UIHandler _uiHandlerSc;

    //Observer pattern:
    private readonly List<I_ShipMovementObserver> _shipMovementObserver = new List<I_ShipMovementObserver>();       //Observers, that need to get notified if the client list changes
    private Map _map;
    private readonly List<ShipMovement> _shipMovements = new List<ShipMovement>();            //Contains all shipMovements that are still traveling to their target planet
    private readonly List<ShipMovement> _shipsToMoveInUpdate = new List<ShipMovement>();
    private static float ANIMATIONS_SPEED = 1.5f;







    //Adds an observer, which is notified whenever the playerlist changes
    public void AddObserver(I_ShipMovementObserver observer)
    {
        _shipMovementObserver.Add(observer);
    }
    //Removes an obersver
    public void RemoveObserver(I_ShipMovementObserver observer)
    {
        _shipMovementObserver.Remove(observer);
    }
    //Internal Helper. Informs all observers about a specific event
    void InformObserversAboutEvent(ShipMovementEventType eventType, ShipMovement shipMovement)
    {
        foreach (I_ShipMovementObserver observer in _shipMovementObserver)
        {
            observer.OnShipMovementEvent(eventType, shipMovement);
        }
    }





    void Awake()
    {
        _map = GameObject.Find("Map").GetComponent<Map>();
        if (_map == null)
        {
            throw new MissingComponentException("Unable to find Map.");
        }


        _uiHandlerSc = UiHandlerGameObject.GetComponent<UIHandler>();
        if (_uiHandlerSc == null)
        {
            throw new MissingComponentException("Uihandler is missing");
        }

    }

    void FixedUpdate()
    {
        for (int i = _shipsToMoveInUpdate.Count - 1; i >= 0; --i)
        {
            if (_shipsToMoveInUpdate[i].GraphicalOutput == null)
            {           //TODO: Simon, das musst du fixen. Das kann man ja nicht so lassen...
                _shipsToMoveInUpdate.RemoveAt(i);
                continue;
            }
            _shipsToMoveInUpdate[i].GraphicalOutput.transform.localPosition = Vector3.Lerp(_shipsToMoveInUpdate[i].GraphicalOutput.transform.localPosition,
                                                                        _shipsToMoveInUpdate[i].TempMoveToPosition, ANIMATIONS_SPEED * Time.smoothDeltaTime);

            //todo figure out how this should work
            float diff = Mathf.Abs(_shipsToMoveInUpdate[i].GraphicalOutput.transform.localPosition.sqrMagnitude - _shipsToMoveInUpdate[i].Destination.position.sqrMagnitude);
            if (diff < 1)  // Close enough
            {
                if (_shipsToMoveInUpdate[i].ArrivalDay <= StateManager.CurrentDay)
                {
                    Destroy(_shipsToMoveInUpdate[i].GraphicalOutput);
                }
                _shipsToMoveInUpdate.RemoveAt(i);
            }
        }
    }

    //Server+Client: Generates a new Shipmovement; Called by Server or Client (the player who sent the ships)
    [RPC]
    void AddNewShipMovement(int shipCount, int sourcePlanetId, int destinationPlanetId)
    {
        PlanetEntity source = _map.GetPlanetById(sourcePlanetId);
        PlanetEntity destination = _map.GetPlanetById(destinationPlanetId);
        if (sourcePlanetId < 0 || source == null || destinationPlanetId < 0 || destination == null)
        {
            throw new UnityException("Unable to generate shipmovement. Planet(s) can't be found. Source: " + sourcePlanetId + " Destination: " + destinationPlanetId);
        }
        if (source.ships < shipCount)
        {
            throw new UnityException("Unable to generate shipmovement. The planet can't afford " + shipCount + " ships: " + source.ToString());
        }

        int currentDay = StateManager.CurrentDay;
        Vector3 toDirection = (destination.position - source.position);
        GameObject graphicalOutput = null;
        if (source.owner.networkPlayer == Network.player)
        {

            graphicalOutput = Instantiate(GraphicalShipMovementPrefab) as GameObject;        //Create new graphical representation for the shipMovement

            if (graphicalOutput == null)
            {
                throw new MissingComponentException("Could not instantiate the Spaceship");
            }

            graphicalOutput.transform.parent = _map.transform;
            graphicalOutput.transform.localPosition = source.position;



            GameObject ship = graphicalOutput.transform.FindChild("ship").gameObject;
            TextMesh textMesh = graphicalOutput.transform.FindChild("text").GetComponent<TextMesh>();
            CircleCollider2D collider = graphicalOutput.transform.GetComponent<CircleCollider2D>();
            SpriteRenderer textBackground = graphicalOutput.transform.FindChild("text").
                                        GetComponentInChildren<SpriteRenderer>();

            textMesh.text = shipCount.ToString();

            float textBackgroundScale = 13;
            if (shipCount > 9)
            {
                textBackgroundScale = 23;
            }else if (shipCount > 99)
            {
                textBackgroundScale = 33;
            }else if (shipCount > 999)
            {
                textBackgroundScale = 43;
            }
            
            textBackground.transform.localScale = new Vector3(textBackgroundScale, 18,1);
            
            ship.transform.rotation = Quaternion.FromToRotation(Vector3.up, toDirection.normalized);

            float percent = Mathf.Min(1, shipCount / 500.0f); // von 0 - 500 wird scaliert auf max 4 
            float scale = 3 * percent + 1;
            collider.radius = scale;
            Vector3 scaleVector = new Vector3(scale, scale, 1); // can be from 1 to 4 in each direction
            ship.transform.localScale = scaleVector;
        }


        ShipMovement shipMovement = new ShipMovement(currentDay, source.owner, shipCount, source, destination, graphicalOutput);
        shipMovement.DistanceBetweenPlanets = toDirection.magnitude;
        shipMovement.TempMoveToPosition = Vector3.MoveTowards(source.position, destination.position,
            (shipMovement.DistanceBetweenPlanets / shipMovement.TravelDays) * 0.5f);

        if (graphicalOutput != null)
        {
            Ship shipSc = graphicalOutput.GetComponent<Ship>();
            shipSc.ShipMovement = shipMovement;
            shipSc.UiHandler = _uiHandlerSc;
            _shipsToMoveInUpdate.Add(shipMovement);
        }
        _shipMovements.Add(shipMovement);
        source.ships -= shipCount;
        InformObserversAboutEvent(ShipMovementEventType.NewShipMovement, shipMovement);
    }




    //Sends new ships from one planet to another
    public void AddNewShipMovementRequest(int shipCount, PlanetEntity source, PlanetEntity destination)
    {
        networkView.RPC("AddNewShipMovement", RPCMode.All, shipCount, source.planetID, destination.planetID);
    }

    public int GetShipMovementCount()
    {
        return _shipMovements.Count;
    }

    public ShipMovement GetShipMovementByIndex(int index)
    {
        return _shipMovements[index];
    }


    //Removes all ShipMovements from the internal storage and returns them, so that they can be processed
    public List<ShipMovement> GetAndRemoveTodaysShipMovements()
    {
        int day = StateManager.CurrentDay;
        List<ShipMovement> extractedMovements = new List<ShipMovement>();
        for (int i = _shipMovements.Count - 1; i >= 0; --i)
        {
            if (_shipMovements[i].ArrivalDay <= day)
            {
                extractedMovements.Add(_shipMovements[i]);
                _shipMovements.RemoveAt(i);
            }
        }
        return extractedMovements;
    }

    public void UpdateGraphicalShipMovements()
    {    //called after each round/day by the StateManager.cs script
        foreach (var ship in _shipMovements)
        {
            if (ship.GraphicalOutput == null) { continue; }
            int daysRemaining = ship.ArrivalDay - StateManager.CurrentDay;
            int daysTraveled = ship.TravelDays - daysRemaining;
            ship.TempMoveToPosition
                = Vector3.MoveTowards(ship.Source.position, ship.Destination.position,
                    (ship.DistanceBetweenPlanets / ship.TravelDays) * daysTraveled);
            _shipsToMoveInUpdate.Add(ship);
        }
    }
}
