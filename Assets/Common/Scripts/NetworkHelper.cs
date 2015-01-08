using UnityEngine;
using System.Collections;

public class NetworkHelper : MonoBehaviour
{

    /*~~ event handler delegates ~~*/
    public delegate void SystemMessageDelegate(string message);
    SystemMessageDelegate SystemMessage = Debug.Log;						//Handler-function for system messages (should usually be printed to the chat window)
    public delegate void NetworkingErrorDelegate(string errorMessage);
    NetworkingErrorDelegate NetworkingError = Debug.LogError;    			//Is called, if a networking error appears    
    
    public delegate void OnDisconnectedDelegate(string message);
    OnDisconnectedDelegate OnDisconnected = Debug.Log;    			        //Is called, if the network is disconnected
    /*public delegate void OnLobbyJoinedDelegate();
    OnLobbyJoinedDelegate OnLobbyJoined = null;    						    //Is called, if the server or host has entered the lobby (next step: tell all other members who we are)
    */
    public delegate void HostListUpdatedDelegate();
    HostListUpdatedDelegate HostListUpdated = null;    						//Is called, if the hostlist got updated

    
    
    
    
    /*~~ server ~~*/
    public string LobbyName { get; private set; }

    /*~~ client ~~*/
    private HostData[] hostList;

    /*~~  ----  ~~*/

    


    //Initialise class according to settings
        void Awake(){
            Initialise();
        }




    //Sets the master-server properties according to the settings
        public void Initialise()
        {
            if (!Settings.UseUnityMasterServer)
            {
                MasterServer.ipAddress = Settings.MasterServerUrl;
                MasterServer.port = Settings.MasterServerPort;
                Network.natFacilitatorIP = Settings.MasterServerUrl;
                Network.natFacilitatorPort = 50005;//Settings.MasterServerPort;
            }
            else
            {
                MasterServer.ipAddress = "0.0.0.0";
                MasterServer.port = 23466;
                Network.natFacilitatorIP = "0.0.0.0";
                Network.natFacilitatorPort = 50005;//23466;
                //TODO: Debug.Log("not implemented yet: reset MasterServer to unity (the master server port might be an issue)");
            }
        }


    //Sets the message handler for system messages
        public void SetSystemMessageHandler(SystemMessageDelegate _SystemMessage){
            if (_SystemMessage == null){
                SystemMessage = Debug.Log;
            }else{
                SystemMessage = _SystemMessage;
            }
        }
    //Sets the handler for networking issues
        public void SetNetworkingErrorHandler(NetworkingErrorDelegate _NetworkingError){
            if (_NetworkingError == null){
                NetworkingError = Debug.LogError;
            }else{
                NetworkingError = _NetworkingError;
            }
        }
    //Sets the handler for networking issues
        public void SetHostListUpdatedHandler(HostListUpdatedDelegate _HostListUpdated){
            HostListUpdated = _HostListUpdated;
        }

    //Sets the handler for networking issues
        public void SetOnDisconnectedHandler(OnDisconnectedDelegate _OnDisconnected){
            OnDisconnected = _OnDisconnected;
        }

    //Sets the handler for if the server/client entered the lobby
        /*public void SetOnLobbyJoinedHandler(OnLobbyJoinedDelegate _OnLobbyJoined){
            OnLobbyJoined = _OnLobbyJoined;
        }*/





    //Server: Starts a server
        public void StartServer(bool isPrivate){
            LobbyName = GetRandomLobbyName(6);
           
            SystemMessage("Creating Lobby " + LobbyName + " \nmax. players = " + Settings.MAX_PLAYERS + " \nport = " + Settings.PORT_NO + " \nmaster server = " + Settings.GetMasterServerStringNorm());
            if (isPrivate){
                LobbyName = "priv_" + LobbyName;
            }
            NetworkConnectionError err = Network.InitializeServer(Settings.MAX_PLAYERS -1, Settings.PORT_NO, /**/false/*/!Network.HavePublicAddress()/**/);		//NAT Punchthrough ; the HavePublicAddress() seems to cause problems, when trying to connect to a local server (tested in the FH)
            if (err != NetworkConnectionError.NoError){
                NetworkingError(err.ToString());
                return;
            }
            MasterServer.RegisterHost(Settings.GAME_NAME, LobbyName, "-" + Settings.PlayerName);
        }

        private string GetRandomLobbyName(int length){
            string lobbyName = "";
            for (int i = 0; i < length; i++)
            {
                int a = Random.Range(0, Settings.ROOM_CHARS.Length);
                lobbyName = lobbyName + Settings.ROOM_CHARS[a];
            }
            return lobbyName;
        }
    

    //Client: Connects to a server
        public void ConnectToServer(HostData host){
            if (host == null) {
                NetworkingError("Unkown host");
                return;
            }
            SystemMessage("Connecting to Lobby " + host.gameName + "...");
            NetworkConnectionError err = Network.Connect(host);
            if (err != NetworkConnectionError.NoError){
                NetworkingError(err.ToString());
                return;
            }
        }

    //Client: Finds open lobbies
        public void RefreshHostList(){
            MasterServer.RequestHostList(Settings.GAME_NAME);
        }
        public HostData[] GetHostList(){
            //todo: filter hosts here
            return hostList;
        }
      

    //Shuts down everything - closes the server or disconnects the client from the server
        public void Disconnect() {
            Network.Disconnect(100);		//timeout 100ms (to tell the server, that we disconnected) - otherwise, we might not be able to join to the same lobby, as the server thinks we are still there (see http://docs.unity3d.com/ScriptReference/Network.Disconnect.html)
            MasterServer.UnregisterHost();	//Unregister the server from the master server. does nothing, if we are not registered
            //SystemMessage("Disconnected");
        }










    //Server: Unable to connect to master server
        void OnFailedToConnectToMasterServer(NetworkConnectionError error) {
            NetworkingError(error.ToString());
        }
    //Server: Something happened
        void OnMasterServerEvent(MasterServerEvent msEvent) {
            if (msEvent == MasterServerEvent.RegistrationFailedGameName ||
                msEvent == MasterServerEvent.RegistrationFailedGameType ||
                msEvent == MasterServerEvent.RegistrationFailedNoServer) {
                NetworkingError(msEvent.ToString());
            } else if (msEvent == MasterServerEvent.RegistrationSucceeded) {
                //SystemMessage("Lobby registered");  //Can happen multiple times, if the server loses connection
            } else if (msEvent == MasterServerEvent.HostListReceived) {
                hostList = MasterServer.PollHostList();
                if (HostListUpdated != null) {
                    HostListUpdated();
                }
            } else {
                Debug.Log(msEvent.ToString());
            }
        }
    //Server: Don't allow new clients to join
        void ForbidNewClients() {
            if (!Network.isServer) {
                return;
            }
            Network.maxConnections = -1;
            MasterServer.UnregisterHost();
        }
    //Server: The server has been initialised and is ready (it is not yet registered at the master server)
        void OnServerInitialized() {
            SystemMessage("Lobby created");
        }
    //Server: A new client connected
        /*void OnPlayerConnected(NetworkPlayer player) {
            SystemMessage("Player connected");
        }*/

    //Server: A client disconnected
        /*void OnPlayerDisconnected(NetworkPlayer player) {
            SystemMessage("Player disconnected");
        }*/





    //Client: On connected to server
        void OnConnectedToServer() {
            SystemMessage("Connected to Lobby");
        }
        
    //Client: Connection to server lost
        void OnDisconnectedFromServer(NetworkDisconnection info) {
            if (Network.isServer) {
                OnDisconnected("Lobby closed");
            } else {
                if (info == NetworkDisconnection.LostConnection) {
                    OnDisconnected("Lost connection to the Host");
                } else {
                    OnDisconnected("The Host closed the connection");
                }
            }
        }
    //Client: Unable to connect to server
        void OnFailedToConnect(NetworkConnectionError error) {
            NetworkingError(error.ToString());
        }
}
