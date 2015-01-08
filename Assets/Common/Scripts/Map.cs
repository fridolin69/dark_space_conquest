using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


/*
 * GameObject
 * Represents (and creates) a Map. Implements the observer pattern, so that observers can get notified about map changes (I_MapObserver.cs)
 * Contains syncronisation logic to build up the same map for all players.
 * If new players are entering the lobby, the map will be synchronised to them as well (observes the PlayerList to do that)
 */


[RequireComponent(typeof(SpriteRenderer))]
public class Map : MonoBehaviour, I_PlayerListObserver {
    
    public GameObject planetPrefab;
    public TextAsset mapData;
    public const int NUMBER_OF_MAPS = 1;        //Number of available maps
    

    PlayerList playerList;      //The list in this object is being observed
    
    List<PlanetEntity> planets = new List<PlanetEntity>();       //All planets on this map. (The Planet-GameObjects are childs of this one. This list allows easier access to the Entity-part) 
    
    public Texture2D mapBackgroundTexture;
    public const float MAP_BORDER = 8;          //Additional border on each side of the map which is added to the mapBounds
    Transform mapBackground;
    float backgroundRatio;
    SpriteRenderer backgroundRenderer;

    System.Random random;
    
    static List<string> predefinedPlanetNames = new List<string> {  "Aarseth", "Abdulla", "Adriana", "Asphaug", "Balaton", "Behaim", "Briggs", "Byrd", "Chaubal", "Cipolla", "Cyrus", "Decatur", "Dimpna", "Dumont", 
                                                                    "Echnaton", "Elbrus", "Elisa", "Erminia", "Fanynka", "Figneria", "Frobel", "Galinskij", "Ganguly", "Giocasilli", "Granule", "Hanakusa", "Harada", 
                                                                    "Hjorter", "Humecronyn", "Iglika", "Ikaunieks", "Isoda", "Jansky", "Kabtamu", "Kalinin", "Koikeda", "Landoni", "Lebedev", "Licitra", "Lyubov", 
                                                                    "Miknaitis", "Namba", "Orchiston", "Pandion", "Penttila", "Quero", "Radmall", "Ruetsch", "Serra", "Shustov", "Siurana", "Smaklosa", "Szalay", 
                                                                    "Tenmu", "Tietjen", "Trombka", "Tytgat", "Velichko", "Vulpius", "Wupatki", "Xanthus", "Yarilo", "Zajonc", "Zeissia", "Zykina" };

    //Observer pattern:
    List<I_MapObserver> mapObserver = new List<I_MapObserver>();       //Observers, that need to get notified if something on the map changed
    public Rect mapBounds { get; private set;}


    public Map() {
        mapBounds = new Rect(0, 0, 0, 0);
        random = new System.Random();
    }

    public Vector2 GetMapSize() {
        return new Vector2(mapBounds.xMax - mapBounds.xMin, mapBounds.yMax - mapBounds.yMin);
    }
    
    void Awake() {
        playerList = GameObject.Find("PlayerList").GetComponent<PlayerList>();
        if (playerList == null) {
            throw new MissingComponentException("Unable to find PlayerList.");
        }
        playerList.AddObserver(this);

        mapBackground = transform.Find("MapBackground");
        if (mapBackground == null) {
            throw new MissingComponentException("Unable to find MapBackground.");
        }


        backgroundRenderer = mapBackground.GetComponent<SpriteRenderer>();
        if (backgroundRenderer == null) {
            throw new MissingComponentException("Unable to find SpriteRenderer on MapBackground.");
        }

        Sprite sprite = Sprite.Create(mapBackgroundTexture, new Rect(0, 0, mapBackgroundTexture.width, mapBackgroundTexture.height), new Vector2(0, 0), 1f);    //origin (pivot) = corner
        backgroundRenderer.sprite = sprite;
        backgroundRatio = mapBackgroundTexture.width / mapBackgroundTexture.height;
    }

    void OnDestroy() {
        playerList.RemoveObserver(this);
    }

    

    //Server: Initialises the Map; Client: Does nothing
        void Start() {
            if (Network.isServer) {
                buildMap(0);
            }
        }

    //Adds an observer, which is notified whenever the map changes
        public void AddObserver(I_MapObserver observer) {
            mapObserver.Add(observer);
        }

    //Removes an obersver
        public void RemoveObserver(I_MapObserver observer) {
            mapObserver.Remove(observer);
        }
    //Internal: Informs all observers about a specific event
        void InformObservers(MapEventType mapEventType, PlanetEntity planet) {
            foreach (I_MapObserver observer in mapObserver) {
                observer.OnMapEvent(mapEventType, planet);
            }
        }

    //Server+Client: Delete all planets and clear the map. Called by the Server.
        [RPC]
        void ClearMap() {
            print("clear map not implemented yet");
            startPlanetCount = 0;

            InformObservers(MapEventType.MapCleared, null);
        }
   

    //Server+Client: Add a planet to the map. Called by the Server.
    //      (If the planet is a start planet: the flag must be visible all the time in the lobby)
        [RPC]
        void AddPlanet(int planetID, bool isStartPlanet, string planetName, float positionX, float positionY, float diameter, int planetSprite, int ships, int hangarSize, int factorySpeed) {
            foreach (PlanetEntity otherPlanet in planets) {
                if (otherPlanet.planetID == planetID) {          //ID's must be unique
                    throw new UnityException("Unable to create planet. There is already a planet with the ID " + planetID);
                }
            }
            Debug.Log("Creating planet " + planetID + " \"" + planetName + "\"");

            GameObject planet = Instantiate(planetPrefab) as GameObject;        //Create new planet
            planet.transform.parent = this.transform;                           //set as child of the map
            PlanetEntity entity = planet.GetComponent<PlanetEntity>();
            entity.Initialise(planetID, planetName, new Vector2(positionX, positionY), diameter, planetSprite, ships, hangarSize, factorySpeed, isStartPlanet, isStartPlanet);            
            planets.Add(entity);
            if (isStartPlanet) {
                ++startPlanetCount;
            }
           
            //Update mapBounds:
                Rect newMapBounds = mapBounds;
                if (entity.position.x - diameter / 2 - MAP_BORDER < mapBounds.xMin) {
                    newMapBounds.xMin = entity.position.x - diameter / 2 - MAP_BORDER;
                }
                if (entity.position.y - diameter / 2 - MAP_BORDER < mapBounds.yMin) {
                    newMapBounds.yMin = entity.position.y - diameter / 2 - MAP_BORDER;
                }
                if (entity.position.x + diameter / 2 + MAP_BORDER > mapBounds.xMax) {
                    newMapBounds.xMax = entity.position.x + diameter / 2 + MAP_BORDER;
                }
                if (entity.position.y + diameter / 2 + MAP_BORDER > mapBounds.yMax) {
                    newMapBounds.yMax = entity.position.y + diameter / 2 + MAP_BORDER;
                }
                mapBounds = newMapBounds;            

                //Update the mapBackground:                
                    //mapBackground.transform.localPosition = new Vector3(mapBounds.xMin + mapBounds.width / 2, mapBounds.yMin + mapBounds.height / 2, 0);       for center-pivot
                    mapBackground.transform.localPosition = new Vector3(mapBounds.xMin, mapBounds.yMin, 0);
                    Vector2 scaling = new Vector2(mapBounds.width / mapBackgroundTexture.width, mapBounds.height / mapBackgroundTexture.height);    //scaling of the background to be as big as the map
                                            
                    //Update the textureRect according to the scaling-ratio:
                        float scaleRatio = scaling.x / scaling.y;
                        Rect textureRect;
                        if (scaleRatio > backgroundRatio) {     //Clip top/bottom
                            float height = mapBackgroundTexture.height / scaleRatio;
                            scaling.y = scaling.x;
                            textureRect = new Rect(0, (mapBackgroundTexture.height-height)/2, mapBackgroundTexture.width, height);
                        } else {                                //Clip left/right
                            float width = mapBackgroundTexture.width * scaleRatio;
                            scaling.x = scaling.y;
                            textureRect = new Rect((mapBackgroundTexture.width - width) / 2, 0, width, mapBackgroundTexture.height);
                        }
                        mapBackground.transform.localScale = new Vector3(scaling.x, scaling.y, 1);
       
                    Sprite sprite = Sprite.Create(mapBackgroundTexture, textureRect, new Vector2(0, 0), 1);    //origin (pivot) = corner
                    backgroundRenderer.sprite = sprite;

            InformObservers(MapEventType.PlanetAdded, entity);
        }

    //Server+Client: Change ownership of planet; Called by the server.
    //Note the server can also change a planet's ownership without performing an RPC (when a player joins. this behaviour can be changed if neccessary)
        [RPC]
        void SetPlanetOwner(int planetID, NetworkPlayer owner) {
            PlanetEntity planet = GetPlanetById(planetID);
            Player player = playerList.GetPlayer(owner);    
            if (planetID < 0 || planet == null) {
                throw new UnityException("Unable to set planet ownership. Planet " + planetID + " not found.\n" + ToString());
            }
            if(player == null){
                throw new UnityException("Unable to set planet ownership. Player " + owner + " not found.\n" + playerList.ToString());
            }
            planet.owner = player;
            InformObservers(MapEventType.PlanetOwnerChanged, planet);
        }
        [RPC]
        void RemovePlanetOwner(int planetID) {
            PlanetEntity planet = GetPlanetById(planetID);
            if (planetID < 0 || planet == null) {
                throw new UnityException("Unable to set planet ownership. Planet " + planetID + " not found.\n" + ToString());
            }
            planet.owner = null;
            InformObservers(MapEventType.PlanetOwnerChanged, null);
        }
    //Server+Client: Upgrade the factory on a specific planet; Called by client or server (the planet owner)
        [RPC]
        void UpgradePlanetFactory(int planetID, NetworkPlayer owner) {
            PlanetEntity planet = GetPlanetById(planetID);
            if (planetID < 0 || planet == null) {
                throw new UnityException("Unable to upgrade planet factory. Planet " + planetID + " not found.\n" + ToString());
            }
            if (planet.owner == null || planet.owner.networkPlayer != owner) {
                throw new UnityException("The planet's Factory can't be upgraded: The RPC sender isn't the owner of " + planet.ToString());
            }
            if (planet.GetFactoryUpgradeCosts() > planet.ships) {
                throw new UnityException("The planet can't afford a Factory upgrade of " + planet.ToString());
            }
            planet.ships -= planet.GetFactoryUpgradeCosts();
            planet.factorySpeed = planet.GetNextFactoryUpgrade();
            InformObservers(MapEventType.PlanetUpgrade, planet);
        }
    //Server+Client: Upgrade the hangar on a specific planet; Called by client or server (the planet owner)
        [RPC]
        void UpgradePlanetHangar(int planetID, NetworkPlayer owner) {
            PlanetEntity planet = GetPlanetById(planetID);
            if (planetID < 0 || planet == null) {
                throw new UnityException("Unable to upgrade planet hangar. Planet " + planetID + " not found.\n" + ToString());
            }
//<<<<<<< HEAD
            //if (planet.owner.networkPlayer != Network.player)
            //{ ich hoff das war das richtige wenn ned sorry ^^
            if (planet.owner == null || planet.owner.networkPlayer != owner) {
                throw new UnityException("The planet's Hangar can't be upgraded: The RPC sender isn't the owner of " + planet.ToString());
            }
            if (planet.GetHangarUpgradeCosts() > planet.ships) {
                throw new UnityException("The planet can't afford a Hangar upgrade of" + planet.ToString());
            }            
            planet.ships -= planet.GetHangarUpgradeCosts();
            planet.hangarSize = planet.GetNextHangarUpgrade();
            InformObservers(MapEventType.PlanetUpgrade, planet);
        }


    //Gets called, when the PlayerList changes (The map of new players need to be updated)
        public void OnPlayerListChanged(PlayerListEventType eventType, Player player) {
            switch (eventType) {
                case PlayerListEventType.PlayerJoined:              //Update the map on the new player and give the player a startplanet:
                    if (!Network.isServer || player.networkPlayer == Network.player) { break; }     //The server only informs clients.
                    networkView.RPC("ClearMap", player.networkPlayer);
                    bool startPlanetFound = false;
                    foreach (PlanetEntity entity in planets) {
                        networkView.RPC("AddPlanet", player.networkPlayer, entity.planetID, entity.isStartPlanet, entity.planetName, entity.position.x, entity.position.y, entity.diameter, entity.planetSpriteNo, entity.ships, entity.hangarSize, entity.factorySpeed);
                        if (!startPlanetFound && entity.isStartPlanet && entity.owner == null) {
                            entity.owner = player;
                            InformObservers(MapEventType.PlanetOwnerChanged, entity);
                            for (int i = 0; i < playerList.GetPlayerCount(); ++i) {
                                Player other = playerList.GetPlayerByIndex(i);
                                if (other.playerStatus == PlayerStatus.Synchronised && other.networkPlayer != Network.player) {      //The player is up-to-date and knows everyone --> inform him about the new owner
                                    networkView.RPC("SetPlanetOwner", other.networkPlayer, entity.planetID, player.networkPlayer);
                                }
                            }                                
                            startPlanetFound = true;
                        }
                    }
                    if (!startPlanetFound) {
                        Debug.LogWarning("The game is overfull because of a too small map. Not every player has a start planet.");
                    }
                    break;
                case PlayerListEventType.PlayerLeft:
                    if (!Network.isServer) { break; }               //not sure `bout this though
                    foreach (PlanetEntity entity in planets) {
                        if (entity.owner == player) {
                            networkView.RPC("RemovePlanetOwner", RPCMode.All, entity.planetID);  //Remove the ownership on all clients
                        }
                    }
                    break;
                case PlayerListEventType.PlayerUpdatedPlayerList:   //The playerList on a connected player is not up-to-date: we are now allowed to inform the player about planet-ownerships
                    foreach (PlanetEntity entity in planets) {
                        if (entity.owner != null) {
                            networkView.RPC("SetPlanetOwner", player.networkPlayer, entity.planetID, entity.owner.networkPlayer);
                        }
                    }                    
                    break;
                case PlayerListEventType.PlayerChangedCharacter:    //Change the color of all planets who are owned by this player
                    foreach (PlanetEntity entity in planets) {
                        if (entity.owner == player) {
                            entity.OwnerChangedItsCharacter();
                        }
                    }
                    break;
                default:
                    Debug.LogError("Unkown PlayerList Event");
                    break;
            }
        }
         
    //Planet name management:
        bool IsPlanetNameOccupied(string planetName) {
            foreach(PlanetEntity entity in planets){
                if(entity.name == planetName){
                    return true;
                }
            }
            return false;
        }
         string GetRandomPlanetName() {
            string planetName;
            do {
                planetName = predefinedPlanetNames[UnityEngine.Random.Range(0, predefinedPlanetNames.Count)];
            } while (IsPlanetNameOccupied(planetName));
            return planetName;
        }

    public int startPlanetCount { get; private set; }  //The number of startplanets that exist. This limits the max. amount of players
        

    //Creates a specific Map
         private enum MapSection { None, StartPlanet, Planet, Sun, End};

         public void buildMap(int mapNumber) {
            if (Network.isClient) {
                throw new UnityException("Clients are not allowed to build a Map.");
            }
            if (mapNumber >= NUMBER_OF_MAPS) {
                Debug.LogError("Unable to build map. Map number does not exist.");
                return;
            }
            networkView.RPC("ClearMap", RPCMode.All);
             
        //Read mapdata from file
            string mapDataStr = mapData.text;           //filecontent
            string[] lines = mapDataStr.Split(new string[] { "\r\n", "\n", "\r", "/", ":", ";" }, System.StringSplitOptions.None);
            
            MapSection mapSection = MapSection.None;        //What we are currently reading from the file
            int sectionLine = 0;                            //The lineNumber within the section that we are reading
            
            //Planet data to read:
                int planetID = 0;
                float positionX=0, positionY=0;
                float diameter=100;
                int ships=0, factorySpeed=0;
                int hangarSize=0;
            //For each line:
            for(int i=0; i<lines.Length && mapSection != MapSection.End; ++i){
                lines[i] = lines[i].Trim().ToLower(); 
                if(lines[i].Length == 0){   //Empty line
                    continue;
                }
                string line = "lineOrPart" + (i+1) + ": \"" + TruncateLongString(lines[i], 15) + "\"";     //For debugging purposes

                if(lines[i][0] == '#'){     //A new section starts
                    if (mapSection != MapSection.None) {        //Did we finish the previous section?
                        throw new UnityException("Unable to load map: Syntax Error on " + line + " - previous section not completed, missing some data.");
                    }
                    switch (lines[i]) {
                        case "#startplanet":    mapSection = MapSection.StartPlanet;    break;
                        case "#planet":         mapSection = MapSection.Planet;         break;
                        case "#sun":            mapSection = MapSection.Sun;            break;
                        case "#end":            mapSection = MapSection.End;            break;        //Skip remaining file
                        default: throw new UnityException("Unable to load map: Syntax Error on " + line + " - invalid section name.");
                    }
                    sectionLine = 0;
                    continue;
                }
                if (mapSection == MapSection.None) {            //There's data, but no section
                    throw new UnityException("Unable to load map: Syntax Error on " + line + " - no map section specified.");
                }
                //Split line into different parts
                string[] parts = lines[i].Split(new string[] { "x", ",", "+", " ", "\t", ";" }, System.StringSplitOptions.RemoveEmptyEntries);    //Different parts of one line
                for (int j = 0; j < parts.Length; ++j) {
                    parts[j] = parts[j].Trim();
                    //print("part in "+i+": " + parts[j]);
                }


                    switch (mapSection) {
                        case MapSection.StartPlanet:
                        case MapSection.Planet:
                            switch (sectionLine) {
                                case 0: //0x0, 5            position (x,y) + diameter
                                    if (parts.Length != 3) {
                                        throw new UnityException("Unable to load map: Syntax Error on " + line + " - invalid number of arguments (3 arguments required).");
                                    }
                                    positionX = (float)Convert.ToDouble(parts[0]);
                                    positionY = (float)Convert.ToDouble(parts[1]);
                                    diameter  = (float)Convert.ToDouble(parts[2]);
                                    if (diameter <= 0) {
                                        throw new UnityException("Unable to load map: Syntax Error on " + line + " - invalid diameter.");
                                    }
                                    break;
                                case 1: //100+10            ships + factorySpeed
                                    if (parts.Length != 2) {
                                        throw new UnityException("Unable to load map: Syntax Error on " + line + " - invalid number of arguments (2 arguments required).");
                                    }

                                    int shipsMIN, shipsMAX;
                                    int factoryMIN, factoryMAX;

                                    string[] shipParts = parts[0].Split('-');
                                    if (shipParts.Length == 2)
                                    {
                                        if (!int.TryParse(shipParts[0], out shipsMIN) || !int.TryParse(shipParts[1], out shipsMAX))
                                        {
                                            ships = -1;
                                        }
                                        else
                                        {
                                            ships = random.Next(shipsMIN, shipsMAX);
                                        }
                                    }
                                    else
                                    {
                                        ships = Convert.ToInt32(parts[0]);
                                    }

                                    string[] factoryParts = parts[1].Split('-');
                                    if(factoryParts.Length==2)
                                    {
                                        if (!int.TryParse(factoryParts[0], out factoryMIN) || !int.TryParse(factoryParts[1], out factoryMAX))
                                        {
                                            factorySpeed = -1;
                                        }
                                        else
                                        {
                                            factorySpeed = random.Next(factoryMIN, factoryMAX);
                                        }
                                    }
                                    else
                                    {
                                        factorySpeed = Convert.ToInt32(parts[1]);
                                    }
       
                                    if (ships < 0 || factorySpeed < 0) {
                                        throw new UnityException("Unable to load map: Syntax Error on " + line + " - invalid arguments.");
                                    }
                                    break;
                                case 2: //150               hangarSize
                                    if (parts.Length != 1) {
                                        throw new UnityException("Unable to load map: Syntax Error on " + line + " - invalid number of arguments (1 argument required).");
                                    }

                                    int hangarMIN, hangarMAX;

                                    string[] hangarParts = parts[0].Split('-');
                                    if (hangarParts.Length == 2)
                                    {
                                        if (!int.TryParse(hangarParts[0], out hangarMIN) || !int.TryParse(hangarParts[1], out hangarMAX))
                                        {
                                            hangarSize = -1;
                                        }
                                        else
                                        {
                                            hangarSize = random.Next(hangarMIN, hangarMAX);
                                        }
                                    }
                                    else
                                    {
                                        hangarSize = Convert.ToInt32(parts[0]);
                                    }

                                    if (hangarSize < 0) {
                                        throw new UnityException("Unable to load map: Syntax Error on " + line + " - invalid hangarSize.");
                                    }
                                    break;
                                default: throw new UnityException("Unable to load map: Error while parsing " + line + " - Invalid sectionLine.");
                            }
                            sectionLine++;
                            if (sectionLine == 3) {
                                string planetName = GetRandomPlanetName();
                                int planetSprite = PlanetEntity.GetRandomPlanetSpriteNo();
                                networkView.RPC("AddPlanet", RPCMode.All, planetID++, (mapSection == MapSection.StartPlanet), planetName, positionX, positionY, diameter, planetSprite, ships, hangarSize, factorySpeed);
                                mapSection = MapSection.None;
                            }
                            break;
                        default: throw new UnityException("Unable to load map: Error while parsing " + line + " - Invalid section.");
                    }
            }

            //Map is done - now give each connected player a startPlanet:

            int playerCount = playerList.GetPlayerCount();
            int playerIdx = 0;
            foreach (PlanetEntity entity in planets) {
                if (entity.isStartPlanet) {
                    entity.owner = playerList.GetPlayerByIndex(playerIdx++);
                    InformObservers(MapEventType.PlanetOwnerChanged, entity);
                    if (playerIdx >= playerCount) {     //Every player has a startPlanet
                        break;
                    }
                }
            }
            if (playerIdx < playerCount){
                Debug.LogWarning("The game is overfull because of a too small map. Not every player has a start planet.");
            }
         }


    //Truncates a string. If characters are removed, "..." is added at the end of the string
         public static string TruncateLongString(string str, int maxLength) {
             string shortStr = str.Substring(0, System.Math.Min(str.Length, maxLength));
             if (str.Length > maxLength) {
                 shortStr += "...";
             }
             return shortStr;
         }

//Positioning:
        float scaling = 1;
    //Sets the map on a specific position. 0x0 = lower left map border is at position 0x0 (usually center of camera)
    //Optianally sets the scaling (0 = don't change the scaling)
        public void SetPosition(Vector2 position, float scaling = 0) {
            if (scaling == 0) {             //Default-parameter: change nothing
                scaling = this.scaling;
            }
            this.scaling = scaling;
            transform.localScale = new Vector3(scaling, scaling, 1);
            transform.position = new Vector3(position.x - (mapBounds.xMin * scaling), position.y - (mapBounds.yMin * scaling), 0); 
        }
    //Sets the center of the map to a specific position. 0x0 = center of map is at position 0x0 (usually center of camera)
        //Optianally sets the scaling (0 = don't change the scaling)
        public void SetCenterPosition(Vector2 position, float scaling = 0) {
            if (scaling == 0) {             //Default-parameter: change nothing
                scaling = this.scaling;
            }
            Vector2 mapSize = GetMapSize();
            SetPosition(new Vector2(position.x - (mapSize.x * scaling / 2), position.y - (mapSize.y * scaling / 2)), scaling);
        }

        public Vector2 GetPosition() {
            Vector3 position = transform.position;
            return new Vector2(position.x + mapBounds.xMin * scaling, position.y + mapBounds.yMin * scaling);
        }

        public Vector2 GetCenterPosition() {
            Vector2 position = GetPosition();
            Vector2 mapSize = GetMapSize();
            return new Vector2(position.x + (mapSize.x * scaling / 2), position.y + (mapSize.y * scaling / 2));
        }
        public float GetLocalScale() {
            return scaling;
        }








    //=======================================================================================================================================
    // GAME LOGIC:
    //=======================================================================================================================================

    //Returns the number of planets. Iterating over planets can be achieved by combining this function with "GetPlanetByIndex()"
         public int GetPlanetCount() {
             return planets.Count;
         }
         public PlanetEntity GetPlanetByIndex(int index) {
             return planets[index];
         }

    //Returns the diameter of the smallest unit (planet/sun/...); Returns -1 if the map is empty.
        public  float GetSmallestDiameter() {
             float diameter = -1;
             foreach (PlanetEntity entity in planets) {
                 if (entity.diameter < diameter || diameter < 0) {
                     diameter = entity.diameter;
                 }
             }
             return diameter;
         }

    //Returns a planet by its unique id
        public PlanetEntity GetPlanetById(int planetID) {
             foreach (PlanetEntity entity in planets) {
                 if (entity.planetID == planetID) {          //ID's must be unique
                     return entity;
                 }
             }
             return null;
         }

    //Returns the planet at the specified position (the position must be within the planet's surface)
        public PlanetEntity GetPlanetAtPosition(Vector2 position) {
             Debug.LogWarning("Are you sure that you wanna use that function? The Planet has a collider which is much more efficient than this function");
             foreach (PlanetEntity entity in planets) {
                 if (entity.GetSurfaceDistance(position) <= 0){
                     return entity;
                 }
             }
             return null;
         }


    //Requests a Factory to be upgraded. Note that the amount of ships must be > upgradeCosts, and that the player must be the owner of the planet
    //Note: this function should only be called, of the player is the actual player owner, and if the player can afford an upgrade. Otherwise, it creates unnecessary network traffic.
        public void UpgradeFactoryRequest(int planetID) {
            networkView.RPC("UpgradePlanetFactory", RPCMode.All, planetID, Network.player);
        }

    //Requests a Hangar to be upgraded. Note that the amount of ships must be > upgradeCosts, and that the player must be the owner of the planet
    //Note: this function should only be called, of the player is the actual player owner, and if the player can afford an upgrade. Otherwise, it creates unnecessary network traffic.
        public void UpgradeHangarRequest(int planetID) {
            networkView.RPC("UpgradePlanetHangar", RPCMode.All, planetID, Network.player);
        }

    //Clears the evaluation events on all planets
        public void ClearEvaluationEvents() {
            foreach (PlanetEntity entity in planets) {
                entity.ClearEvaluationEvents();
            }
        }



         public override string ToString() {
             string str = "This map has " + planets.Count + " planets:\n";
             foreach (PlanetEntity entity in planets) {
                 str += "    " + entity.ToString() + "\n";
             }
             return str;
         }
}
