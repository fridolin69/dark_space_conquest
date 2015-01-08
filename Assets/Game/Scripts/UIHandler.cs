using System;
using UnityEngine;
using System.Collections.Generic;
using System.Security.Policy;
using UnityEngine.UI;

public class UIHandler : MonoBehaviour, I_PlayerListObserver, I_StateSynchronisationObserver
{

    public Canvas UiCanvas;

    public GameObject TopPanel;
    private TopPanel _topPanelSc;

    public GameObject PlanetOverviewPanel;
    private PlanetOverviewPanel _planetOverviewPanelSc;

    public GameObject PlanetOverviewLockedPanel;
    private PlanetOverviewLockedPanel _planetOverviewLockedPanelSc;

    public GameObject SendShipsPanel;
    private SendShipsPanel _sendShipsPanelSc;

    public GameObject UpgradePanel;
    private UpgradePanel _upgradePanelSc;

    public GameObject ReadyPanel;
    private ReadyPanel _readyPanelSc;

    public GameObject EventOnPlanetPanel;
    private EventOnPlanetPanel _eventOnPlanetPanelSc;
    

    public GameObject ShipDetailPanel;
    private ShipDetailPanel _shipDetailPanelSc;

    public GameObject ExitGameDialog;

    public GameObject Synchorniser;
    private StateSynchronisation _synchroniserSc;
    private ShipMovementHandler _shipMovementHandlerSc;

    private float FlashSpeed = 18;
    //private Color FlashColor = new Color(200, 0f, 0f, 45);
    public Image AlertImage;

    public GameObject PlayerReadyInfoPrefab;
    public const float PLAYER_READY_INFO_PADDING = 30;

    private static PlanetEntity _activePlanet;
    private static PlanetEntity _highlightedPlanet; // when ship is choosen the destination planet is highlighted

    private StateSynchronisation _stateSynchronisation;
    private Map _map;
    private PlayerList _playerList;
    Transform[] playerInfoObjects;
    Toggle[] playerInfoToggles;
    public Toggle IsReadyToggle;

    private NetEnvironment _networkHelper;

    /// fade day
    public Image DayChangeImage;
    private const float DayChangeSpeed = 1.0f;
    private const float NightDuration = 1.2f; //raise this number to have longer nights depends on daychangespeed too
    private readonly Color _myBlack = new Color(0, 0, 0, 5);
    private bool _fadeToBlack;
    private bool _fadeToClear;
    private float _alpha;


    public enum InputStateE
    {
        PlanetOverview, SendShips, UpgradeBuilding, ReadyForNextRound, DisplayShipDetails
    }

    public InputStateE InputState { get; set; }


    public void Awake()
    {
        _networkHelper = GameObject.Find("Networking").GetComponent<NetEnvironment>();
        _stateSynchronisation = GameObject.Find("Synchroniser").GetComponent<StateSynchronisation>();

        _topPanelSc = TopPanel.GetComponent<TopPanel>();
        _planetOverviewPanelSc = PlanetOverviewPanel.GetComponent<PlanetOverviewPanel>();
        _planetOverviewLockedPanelSc = PlanetOverviewLockedPanel.GetComponent<PlanetOverviewLockedPanel>();
        _sendShipsPanelSc = SendShipsPanel.GetComponent<SendShipsPanel>();
        _upgradePanelSc = UpgradePanel.GetComponent<UpgradePanel>();
        _readyPanelSc = ReadyPanel.GetComponent<ReadyPanel>();
        _eventOnPlanetPanelSc = EventOnPlanetPanel.GetComponent<EventOnPlanetPanel>();
        _map = GameObject.Find("Map").GetComponent<Map>();
        _playerList = GameObject.Find("PlayerList").GetComponent<PlayerList>();
        _shipMovementHandlerSc = Synchorniser.GetComponent<ShipMovementHandler>();
        _synchroniserSc = Synchorniser.GetComponent<StateSynchronisation>();
        _shipDetailPanelSc = ShipDetailPanel.GetComponent<ShipDetailPanel>();
        if (_topPanelSc == null
            || _planetOverviewPanelSc == null
            || _planetOverviewLockedPanelSc == null
            || _sendShipsPanelSc == null
            || _upgradePanelSc == null
            || _readyPanelSc == null
            || _eventOnPlanetPanelSc == null
            || _map == null
            || _playerList == null
            || _shipMovementHandlerSc == null
            || _synchroniserSc == null
            || _shipDetailPanelSc == null
            || _networkHelper == null
            || _stateSynchronisation == null)
        {
            throw new MissingComponentException("Failed to find one of the Components in UiHandler");
        }
        ClickedOnEmptySpace();
        _playerList.AddObserver(this);
        _synchroniserSc.AddObserver(this);
    }

    

    public void Start()
    {
        int playerListCount = _playerList.GetPlayerCount();
        if (playerListCount > 12)
        {
            throw new UnityException("Unable to setup UI: The number of players is too high. The ReadyPanel only supports up to 12 players.");
        }
        playerInfoObjects = new Transform[playerListCount];
        playerInfoToggles = new Toggle[playerListCount];
        for (int i = 0; i < playerListCount; ++i)
        {
            playerInfoObjects[i] = ReadyPanel.transform.Find("PlayerReadyInfo" + (i + 1));
            playerInfoObjects[i].Find("Label").GetComponent<Text>().text = _playerList.GetPlayerByIndex(i).name;
            playerInfoObjects[i].Find("Background").GetComponent<UnityEngine.UI.Image>().color = _playerList.GetPlayerByIndex(i).GetColor();

            playerInfoToggles[i] = playerInfoObjects[i].gameObject.GetComponent<Toggle>();
        }
        for (int i = playerListCount; i < 12; ++i)
        {
            Destroy(ReadyPanel.transform.Find("PlayerReadyInfo" + (i + 1)).gameObject);
        }
        //Object playerReadyInfo = GameObject.FindGameObjectsWithTag("playerReadyInfo");        
        //foreach (Object respawn in respawns) {
        //    Instantiate(respawnPrefab, respawn.transform.position, respawn.transform.rotation) as GameObject;
        //}
    }




    void OnDestroy()
    {
        _playerList.RemoveObserver(this);
    }


    public void InitiateDayFading() {
        _fadeToBlack = true;
        _fadeToClear = false;
    }


    //public void OnNextDayHandler(){
    //    if (_activePlanet == null){
    //        ClickedOnEmptySpace();
    //    }else{
    //        PlanetClicked(_activePlanet);
    //    }
    //}

//gets called when planet is clicked (from planetentity itself)
    public void PlanetClicked(PlanetEntity planet)
    {
        int eventsAvailable = planet.GetEvaluationEventCount();
        if (eventsAvailable > 0 && InputState != InputStateE.SendShips && InputState != InputStateE.UpgradeBuilding)
        {
            //show eval panel
            DisableAllPanels();
            DisablePlanetOutlines();
            _activePlanet = planet;

            planet.SetOutline(Color.green);
            _eventOnPlanetPanelSc.UpdatePanel(planet);
            EventOnPlanetPanel.SetActive(true);
            return;
        }
        
        if (InputState == InputStateE.SendShips){
            _sendShipsPanelSc.UpdateTargetPlanet(planet);
            return;
        }
        
        UpdatePlanetInfo(planet);
    }
    private void UpdatePlanetInfo(PlanetEntity planet)
    {
        if (planet == null) { return; } //save me from myself.. should probably be an assert.. :O
        InputState = InputStateE.PlanetOverview;
        DisablePlanetOutlines();

        _activePlanet = planet;

        _topPanelSc.UpdatePlanet(planet);
        planet.SetOutline(Color.grey);
        // if null it's neutral and no info is to displayed other than name and planetsprite
        // same as if it is not null and I am not the owner
        if (planet.owner == null || (planet.owner != null && planet.owner.networkPlayer != Network.player))
        {
            //Dieser planet gehört nicht mir
            //InputState = InputStateE.NotMyPlanet;
            DisableAllPanels();
            PlanetOverviewLockedPanel.SetActive(true);
            _planetOverviewLockedPanelSc.UpdatePlanet(_activePlanet);
        }
        else
        {
            _planetOverviewPanelSc.UpdatePlanetInfo(planet);
            DisableAllPanels();
            PlanetOverviewPanel.SetActive(true);
        }

    }

    public void ShowUpgradeFactoryPanel()
    {

        _upgradePanelSc.UpdatePanel(_activePlanet, global::UpgradePanel.Type.Factory);
        ShowUpgradePanel();
        InputState = InputStateE.UpgradeBuilding;
    }
    public void ShowUpgradeHangarPanel()
    {
        _upgradePanelSc.UpdatePanel(_activePlanet, global::UpgradePanel.Type.Hangar);
        ShowUpgradePanel();
        InputState = InputStateE.UpgradeBuilding;
    }

    private void ShowUpgradePanel()
    {
        DisableAllPanels();
        UpgradePanel.SetActive(true);

    }

    public void DismissEventsOnPlanet()
    {
        _activePlanet.ClearEvaluationEvents();
        DisableAllPanels();
        DisablePlanetOutlines();

        UpdatePlanetInfo(_activePlanet);
        InputState = InputStateE.PlanetOverview;
    }

    public void BuyUpgrade()
    {
        if (_upgradePanelSc._type == global::UpgradePanel.Type.Factory)
        {
            _map.UpgradeFactoryRequest(_activePlanet.planetID);
        }
        else if (_upgradePanelSc._type == global::UpgradePanel.Type.Hangar)
        {
            _map.UpgradeHangarRequest(_activePlanet.planetID);
        }
        Debug.Log("Upgrading the : " + _upgradePanelSc._type.ToString());
        PlanetClicked(_activePlanet);
        InputState = InputStateE.PlanetOverview;
    }

    public void AbortUpgradePanel()
    {
        DisableAllPanels();
        PlanetOverviewPanel.SetActive(true);
        InputState = InputStateE.PlanetOverview;
        DisablePlanetOutlines();

        UpdatePlanetInfo(_activePlanet);
    }

    public void ShowShipDetailPanel(Ship ship)
    {
        DisablePlanetOutlines();
        DisableAllPanels();
        ShipDetailPanel.SetActive(true);
        _shipDetailPanelSc.UpdatePanel(ship);
        _highlightedPlanet = ship.ShipMovement.Destination;
        _highlightedPlanet.SetOutline(Color.red);
        InputState = InputStateE.DisplayShipDetails;
        //show shippanel display info

    }

    public void SendShips()
    {
        _shipMovementHandlerSc.AddNewShipMovementRequest(
                     _sendShipsPanelSc.Slidervalue
                     , _sendShipsPanelSc.SourcePlanet
                     , _sendShipsPanelSc.TargetPlanet);
        InputState = InputStateE.PlanetOverview;
        UpdatePlanetInfo(_activePlanet);

    }

    // show sendship panel
    public void ShowSendShipsPanel()
    {

        DisableAllPanels();
        SendShipsPanel.SetActive(true);
        _sendShipsPanelSc.UpdatePanel(_activePlanet);
        InputState = InputStateE.SendShips;

    }

    // go back to the overview
    public void AbortSendShips()
    {
        DisablePlanetOutlines();
        DisableAllPanels();
        PlanetOverviewPanel.SetActive(true);
        InputState = InputStateE.PlanetOverview;

    }

    // reset to playeroverview
    public void ClickedOnEmptySpace(Boolean force = false)
    {
        if (!force) // don't force panel switch (sendships will return)
        {
            if (InputState == InputStateE.SendShips) { return;   }
        }

        DisablePlanetOutlines();

        Debug.Log("click in empty space");

        DisableAllPanels(false);
        ReadyPanel.SetActive(true);
        InputState = InputStateE.ReadyForNextRound;
    }



    private void DisablePlanetOutlines()
    {
        if (_activePlanet != null)
        {
            _activePlanet.DisableOutline();
        }
        else if (_sendShipsPanelSc.SourcePlanet != null)
        {
            _sendShipsPanelSc.SourcePlanet.DisableOutline();
        }
        if (_sendShipsPanelSc.TargetPlanet != null)
        {
            _sendShipsPanelSc.TargetPlanet.DisableOutline();
        }
        if (_highlightedPlanet != null)
        {
            _highlightedPlanet.DisableOutline();
        }
    }



    private void DisableAllPanels(bool activateTopPanel = true)
    {
        TopPanel.SetActive(activateTopPanel);
        PlanetOverviewPanel.SetActive(false);
        PlanetOverviewLockedPanel.SetActive(false);
        SendShipsPanel.SetActive(false);
        UpgradePanel.SetActive(false);
        ReadyPanel.SetActive(false);
        EventOnPlanetPanel.SetActive(false);
        ShipDetailPanel.SetActive(false);
    }



    //The user clicked the alert-button
    public void OnAlertUser(){
        _stateSynchronisation.AlertNotReadyUsers();
    }

    public void AlertUser(Color flashColor){
        AlertImage.color = flashColor;
    }



    void Update()
    {
        AlertImage.color = Color.Lerp(AlertImage.color, Color.clear, FlashSpeed*Time.smoothDeltaTime);
        if (_fadeToBlack)
        {
            _alpha += DayChangeSpeed*Time.smoothDeltaTime;
            DayChangeImage.color =new  Color(0,0,0,_alpha);
            if (DayChangeImage.color.a > NightDuration) 
            {
                DayChangeImage.color = _myBlack;
                _fadeToBlack = false;
                _fadeToClear = true;
            }
        }
        if (_fadeToClear)
        {
            _alpha -= DayChangeSpeed * Time.smoothDeltaTime;
            DayChangeImage.color = new Color(0, 0, 0, _alpha);
            if (DayChangeImage.color.a < 0.1f)
            {
                DayChangeImage.color = Color.clear;
                _fadeToBlack = false;
                _fadeToClear = false;
            }
        }
    }






    // I_StateSynchronisationObserver functions
    public void OnPlayerReadyChanged(Player player, GameState gameState){
        if (gameState != GameState.UserInteraction /*&& gameState != GameState.FightEvaluation*/)
        { return; }
        for (int i = 0; i < _playerList.GetPlayerCount(); ++i){
            //if (_playerList.GetPlayerByIndex(i) == player){
                playerInfoToggles[i].isOn = _playerList.GetPlayerByIndex(i).isReady;
            //}
        }
    }



    public void ShowEndGameDialog()
    {
        if (InputState == InputStateE.ReadyForNextRound)
        {
            ExitGameDialog.SetActive(!ExitGameDialog.activeSelf);

        }
        else
        {
            ClickedOnEmptySpace(true);
        }
    }

    public void QuitToMenu()
    {
        _networkHelper.ShutdownNetwork();
    }

    public void CancelEndGameDialog()
    {
        ExitGameDialog.SetActive(false);
    }



    ///////////////////////////////////////////////////////////////////////////
    ///     General
    ///////////////////////////////////////////////////////////////////////////


    //Gets called by the playerList, whenever a new player enters the lobby, or an old player leaves it (observer function)
    public void OnPlayerListChanged(PlayerListEventType eventType, Player player)
    {
        if (eventType == PlayerListEventType.PlayerUpdatedPlayerList)
        {     //The playerList itself didn't change. So there's nothing todo.
            return;
        }
        if (eventType == PlayerListEventType.PlayerJoined)
        {      //The number of elements is the same, we only need to change the character
            throw new UnityException("A player can't join during a running game.");
        }
        if (eventType == PlayerListEventType.PlayerChangedCharacter)
        {      //The number of elements is the same, we only need to change the character
            throw new UnityException("A player can't change its character during a running game.");
        }

        if (eventType == PlayerListEventType.PlayerLeft)
        {
            int playerListCount = _playerList.GetPlayerCount();
            Destroy(ReadyPanel.transform.Find("PlayerReadyInfo" + (playerListCount + 1)).gameObject);
            for (int i = 0; i < playerListCount; ++i)
            {
                playerInfoObjects[i].Find("Label").GetComponent<Text>().text = _playerList.GetPlayerByIndex(i).name;
                playerInfoObjects[i].Find("Background").GetComponent<UnityEngine.UI.Image>().color = _playerList.GetPlayerByIndex(i).GetColor();
            }
        }
    }
}
