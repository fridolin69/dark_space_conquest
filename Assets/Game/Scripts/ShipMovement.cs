using UnityEngine;
using System.Collections;


/*
 * Entity.
 * Contains all information about a single shipmovement. One entity is generated for each "send ships" command
 */

public class ShipMovement {

    public Player Owner {get; private set;}
    public int ShipCount {get; private set;}
    public PlanetEntity Source {get; private set;}
    public PlanetEntity Destination {get; private set;}
    public GameObject GraphicalOutput {get; private set;}
    public float DistanceBetweenPlanets { get; set; }
    public int TravelDays {get; private set;}
    public int ArrivalDay {get; private set;}

    //public int 
    // used to move planes in update loop
    public Vector3 TempMoveToPosition { get; set; }


    public ShipMovement(int currentDay, Player owner, int shipCount, PlanetEntity source, PlanetEntity destination, GameObject graphicalOutput){
        this.Owner = owner;
        this.ShipCount = shipCount;
        this.Source = source;
        this.Destination = destination;
        this.GraphicalOutput = graphicalOutput;

        TravelDays = source.GetTravelTime(destination);
        ArrivalDay = currentDay + TravelDays;
    }
}
