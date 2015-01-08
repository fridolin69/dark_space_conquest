using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/*
 * Entity
 * Contains all Planetdata. Note that this script requires to be part of a GameObject.
 * Setting the position and diameter directly affects the GameObject properties
 */


[RequireComponent(typeof(SpriteRenderer))]
public class PlanetEntity : MonoBehaviour
{

    public Texture2D planetSprites;         //The texture to be used
    public static readonly Vector2 planetSpriteCount = new Vector2(4, 4);
    public const float OUTLINE_THICKNESS = 1;     //The outline thickness
    public const float UPGRADE_FACTOR = 1.5f;       //Multiplication factor for upgrade steps (both hangar + factory)
    public const int UPGRADE_STEP = 5;              //Upgrade steps: the different upgrade-levels are always "modulo UPGRADE_STEP == 0". --> if step is 5, the different levels can only be 5, 10, 15, 20, ...  (Note: the fist level is loaded from the mapfile and can be anything)
    public const int FACTORY_UPGRADE_COSTS = 5;     //The number of ships that are required to increase the speed by 1.
    public const int HANGAR_UPGRADE_COSTS = 1;      //The number of ships that are required to increase the hangar by 1.

    public const float TRAVEL_SPEED = 15.0f;           //Distance, that can be traveled within a day

    SpriteRenderer spriteRenderer;
    SpriteRenderer outlineRenderer;
    SpriteRenderer flagRenderer;
    SpriteRenderer eventRenderer;


    List<EvaluationEvent> evaluationEvents = new List<EvaluationEvent>();   //Needed during the FightEvaluation GameState

    PlanetEntity(){
        this.planetID = -1;                     //Not yet initialised
        this.planetName = "TrES-4b";
        //this.owner = null;                    //I'm not allowed to set the position in the constructor, and therefore can't use "Initialise()" - I can't tell you how much I hate Unit right now
        //this.position = position;             //I'm not allowed to set the position either
        //this.diameter = 1;                    //I'll initialise them in the Awake()-method
        this.ships = 100;
        this.hangarSize = 150;
        this.factorySpeed = 10;
        //this.alwaysShowFlag = false;
    }

    void Awake(){
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            throw new MissingComponentException("Unable to find SpriteRenderer on Planet.");
        }

        planetSpriteNo = GetRandomPlanetSpriteNo();

        outlineRenderer = transform.Find("Outline").GetComponent<SpriteRenderer>();
        if (outlineRenderer == null)
        {
            throw new MissingComponentException("Unable to find SpriteRenderer for Outline.");
        }
        flagRenderer = transform.Find("Flag").GetComponent<SpriteRenderer>();
        if (flagRenderer == null)
        {
            throw new MissingComponentException("Unable to find SpriteRenderer for Flag.");
        }
        eventRenderer = transform.Find("EventIcon").GetComponent<SpriteRenderer>();
        if (eventRenderer == null) {
            throw new MissingComponentException("Unable to find SpriteRenderer for Event Icon.");
        }

        this.owner = null;
        this.position = new Vector2(0, 0);
        this.diameter = 1;
        this.isStartPlanet = false;
        this.alwaysShowFlag = false;
    }

    //Initialises the planet (it will be neutral)
        public void Initialise(int planetID, string planetName, Vector2 position, float diameter, int planetSpriteNo, int ships, int hangarSize, int factorySpeed, bool isStartPlanet, bool alwaysShowFlag){
            if (planetID < 0){
                throw new UnityException("Invalid planet id. ID must be >= 0");
            }
            this.planetID = planetID;
            this.planetName = planetName;
            this.owner = null;
            this.position = position;
            this.diameter = diameter;
            this.planetSpriteNo = planetSpriteNo;
            this.ships = ships;
            this.hangarSize = hangarSize;
            this.factorySpeed = factorySpeed;
            this.isStartPlanet = isStartPlanet;
            this.alwaysShowFlag = alwaysShowFlag;
        }


    public bool isStartPlanet { get; set; }

    //Every planet on a map has a unique id, which is the same at all players. Can be used to identify a planet
    //Unique id's start at 0. -1 means that the planet has not been initialised yet.
        public int planetID { get; private set; }

    public string planetName { get; private set; }

    public Vector2 position{
        get { return transform.localPosition; }
        set { transform.localPosition = value; }
    }


    public Sprite planetSprite { get; private set; }
    int _planetSpriteNo;
    public int planetSpriteNo{
        get { return _planetSpriteNo; }
        set
        {
            if (planetSpriteCount.x == 0 || planetSpriteCount.y == 0)
            {
                throw new UnityException("Invalid sprite count");
            }
            Vector2 spriteToUse = new Vector2(value % (int)planetSpriteCount.x, value / (int)planetSpriteCount.x);
            //if (planetID >= 0) {
            //    Debug.Log("Using sprite " + spriteToUse.ToString() + " for " + planetID + " \"" + planetName + "\"");
            //}
            if (spriteToUse.x >= planetSpriteCount.x || spriteToUse.y >= planetSpriteCount.y)
            {
                spriteToUse = new Vector2(0, 0);
                Debug.LogError("Planet Sprite does not exist - using default sprite instead");
            }
            Rect textureRect = new Rect((planetSprites.width / planetSpriteCount.x) * spriteToUse.x, (planetSprites.height / planetSpriteCount.y) * spriteToUse.y,
                                         planetSprites.width / planetSpriteCount.x, planetSprites.height / planetSpriteCount.y);
            planetSprite = Sprite.Create(planetSprites, textureRect, new Vector2(0.5f, 0.5f), planetSprites.width / planetSpriteCount.y);    //origin (pivot) = center;
            spriteRenderer.sprite = planetSprite;
            _planetSpriteNo = (int)(spriteToUse.y * planetSpriteCount.x + spriteToUse.x);
        }
    }


    static public int GetRandomPlanetSpriteNo(){
        Vector2 random = new Vector2(Random.Range(0, (int)planetSpriteCount.x), Random.Range(0, (int)planetSpriteCount.y));
        return (int)(random.y * planetSpriteCount.x + random.x);
    }




    public float diameter {
        get { return transform.localScale.x; }
        set {
            transform.localScale = new Vector3(value, value, 1);
            //Smaller planets need a larger outline (in comparison with the planet), to let it appear as thick as for big planets:
            float scale = (value + OUTLINE_THICKNESS) / value;
            outlineRenderer.transform.localScale = new Vector3(scale, scale, 1);
            scale = (value + 5) / Mathf.Pow(value, 1.2f) * 0.01f;
            flagRenderer.transform.localScale = new Vector3(scale, scale, 1);
            flagRenderer.transform.localPosition = new Vector3(diameter * scale, diameter * scale, 0);
            scale *= 0.8f * Random.Range(0.9f, 1.1f);
            //eventRenderer.transform.localScale = new Vector3(scale, scale, 1);
            //eventRenderer.transform.localPosition = new Vector3(0, diameter * scale, 0);
            //eventRenderer.transform.rotation = Quaternion.Euler( new Vector3(0, 0, Random.Range(2, 15)) );
            SetEventRendererScale(1);
            SetRandomEventRendererRotation();
        }
    }


    void SetEventRendererScale(float scale) {
        //float scale = (diameter + OUTLINE_THICKNESS) / diameter;        
        //scale *= 0.8f;
        scale = 0.1f * scale / diameter;
        eventRenderer.transform.localScale = new Vector3(scale, scale, 1);
        eventRenderer.transform.localPosition = new Vector3(0, diameter * scale, 0);
    }
    float GetEventRendererScale() {
        return eventRenderer.transform.localScale.x * diameter / 0.1f;
    }

    void SetRandomEventRendererRotation() {
        eventRenderer.transform.rotation = Quaternion.Euler(new Vector3(0, 0, Random.Range(2, 15)));
    }




    int _ships;
    public int ships{
        get { return _ships; }
        set {
            if (ships < 0) { throw new UnityException("Invalid ship amount"); }
            _ships = value;
        }
    }

    int _hangarSize;
    public int hangarSize{
        get { return _hangarSize; }
        set {
            if (hangarSize < 0) { throw new UnityException("Invalid hangar size"); }
            _hangarSize = value;
        }
    }

    int _factorySpeed;
    public int factorySpeed {
        get { return _factorySpeed; }
        set {
            if (factorySpeed < 0) { throw new UnityException("Invalid factory speed"); }
            _factorySpeed = value;
        }
    }

    //Upgrading costs and steps:
        public int GetFactoryUpgradeCosts() {
            int upgradeAmount = GetNextFactoryUpgrade() - factorySpeed;
            return upgradeAmount * FACTORY_UPGRADE_COSTS;
        }
        public int GetNextFactoryUpgrade() {
            int next = Mathf.CeilToInt(factorySpeed * UPGRADE_FACTOR);
            next += UPGRADE_STEP - (next % UPGRADE_STEP);
            return next;
        }
        public int GetHangarUpgradeCosts() {
            int upgradeAmount = GetNextHangarUpgrade() - hangarSize;
            return upgradeAmount * HANGAR_UPGRADE_COSTS;
        }
        public int GetNextHangarUpgrade() {
            int next = Mathf.CeilToInt(hangarSize * UPGRADE_FACTOR);
            next += UPGRADE_STEP - (next % UPGRADE_STEP);
            return next;
        }
        

    bool _alwaysShowFlag;
    public bool alwaysShowFlag {
        get{
            return _alwaysShowFlag;
        }
        set{
            _alwaysShowFlag = value;
            if (owner != null){      //Flag doesn't influence anything
                return;
            }
            if (alwaysShowFlag){     //disable flag
                flagRenderer.color = Color.white;
                flagRenderer.enabled = true;
            }else{
                flagRenderer.enabled = false;
            }
        }
    }

    Player _owner;
    public Player owner{           //The owner of the planet. Neutral = null
        get { return _owner; }
        set {
            if (value == _owner) { return; }
            Debug.Log("Changing owner of planet " + planetID + " \"" + planetName + "\": Instead of " + ((_owner == null) ? "noone" : _owner.ToString()) + ", it's now " + ((value == null) ? "noone" : value.ToString()));
            _owner = value;
            if (_owner == null && !alwaysShowFlag)
            {
                flagRenderer.enabled = false;
                return;
            }
            if (owner == null)
            {
                flagRenderer.color = Color.white;
            }
            else
            {
                flagRenderer.color = _owner.GetColor();
            }
            flagRenderer.enabled = true;
        }
    }

    //This function is called everytime the planet-owner changed its character. Is called by the Map.cs, which is an observer of the PlayerList.
        public void OwnerChangedItsCharacter(){
            if (owner == null){
                throw new UnityException("A player which doesn't exist can't change its character");
            }
            flagRenderer.color = owner.GetColor();
        }





    //Enables the outline and sets a specific color 
        public void SetOutline(Color color){
            outlineRenderer.color = color;
            outlineRenderer.enabled = true;
        }
    //Disables the outline, so that it is not visible anymore
        public void DisableOutline(){
            outlineRenderer.enabled = false;
        }
    //Reutrns the color of the outline. If the outline is disabled, the last used color is returned
        public Color GetOutlineColor(){
            return outlineRenderer.color;
        }
    //Returns true if the outline is visible/enabled
        public bool IsOutlineEnabled(){
            return outlineRenderer.enabled;
        }





    public void AddEvaluationEvent(EvaluationEvent evaluationEvent) {
        evaluationEvents.Add(evaluationEvent);
        eventRenderer.enabled = true;


        EvaluationEventOutcome iconColor = evaluationEvent.positiveEvent;
        float importanceScale = Mathf.Lerp(0.4f, 1.1f, 0);//evaluationEvent.importance/100);

        if (importanceScale > GetEventRendererScale()) {
            switch (iconColor) {
                case EvaluationEventOutcome.Success: eventRenderer.color = Color.green;    break;
                case EvaluationEventOutcome.Neutral: eventRenderer.color = Color.yellow;   break;
                case EvaluationEventOutcome.Lost:    eventRenderer.color = Color.red;      break;
            }
            SetEventRendererScale(importanceScale);
            SetRandomEventRendererRotation();
        }
    }
    public void ClearEvaluationEvents() {
        evaluationEvents.Clear();
        eventRenderer.enabled = false;
        eventRenderer.color = Color.blue;       //for debugging purposes
        SetEventRendererScale(0);
    }
    public int GetEvaluationEventCount() {
        return evaluationEvents.Count;
    }
    public EvaluationEvent GetEvaluationEventByIndex(int index) {
        return evaluationEvents[index];
    }

    //public void RemoveEvaluationEventByIndex(int index)
    //{
    //    evaluationEvents.RemoveAt(index);
    //    if (evaluationEvents.Count == 0)
    //    {
    //        eventRenderer.enabled = false;
    //    }
    //}





















    public override string ToString(){
        Vector2 position = this.position;
        float diameter = this.diameter;
        return "Planet " + planetID + " \"" + planetName + "\" at " + position.ToString() + " belongs to " + ((owner == null) ? "noone" : owner.name) + " , diameter = " + diameter + " has: " + ships + " (+" + factorySpeed + " á turn) ships, with hangar of size " + hangarSize;
    }



    //Returns the distance of this planet to any other position (Note: maybe you wanna use SurfaveDistance instead?)
        public float GetDistance(Vector2 ToPosition)
        {
           return  ( ToPosition - this.position).magnitude;
        }
    //Returns the distance of this planet to any other position
    //NOTE: Returns the distance to the planet's surface, not it's center
        public float GetSurfaceDistance(Vector2 position){
            return GetDistance(position) - diameter / 2;
        }

    //Returns the distance between two planets (their surfaces, not their centers)
        public float GetSurfaceDistance(PlanetEntity otherPlanet)
        {
            return GetDistance(otherPlanet.position) - (otherPlanet.diameter + diameter) / 2;
        }

        public int GetTravelTime(PlanetEntity otherPlanet) {
            return GetTravelTime( GetSurfaceDistance(otherPlanet) );
        }

        public int GetTravelTime(float distance) {
            return Mathf.RoundToInt(distance / TRAVEL_SPEED);
        }






    private UIHandler _uiHandler;
    public void InitForGame()
    {
        _uiHandler = GameObject.Find("UIHandler").GetComponent<UIHandler>();
    }

    public void SingleTouchClick(){
        if (_uiHandler == null){
            InitForGame();
        }
        _uiHandler.PlanetClicked(this);
        Debug.Log("clicked planet: " + this.ToString());
    }
}