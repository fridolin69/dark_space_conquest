using UnityEngine;
using System.Collections;



/*
 * Entity
 * Prepares the Networking and contains functionality to start/connect to a server
 */

public class NetEnvironment : MonoBehaviour {


    NetworkHelper networkHelper;	//For "low-level" network tasks
   

	    void Start () {
		    Chat chat = GameObject.Find ("GUI").GetComponent<Chat>();			//For system- and debug messages
            
            //Initialize the networkHelper to perform lowlevel-commands  (not really, but sounds good)
			    networkHelper = (NetworkHelper)gameObject.AddComponent(typeof(NetworkHelper));
			    if (chat == null) {
				    Debug.LogWarning ("Unable to find Chat. System messages will be Logged.");
			    }else{
				    networkHelper.SetSystemMessageHandler ( chat.SystemMessage );
			    }
                networkHelper.SetNetworkingErrorHandler(NetworkingError);
                networkHelper.SetOnDisconnectedHandler(OnDisconnected);
                //networkHelper.SetOnLobbyJoinedHandler(OnLobbyJoined);
			
		    //Start server or client, depending on how the lobby was entered
			    StartupNetwork ();
	    }	    
	    void StartupNetwork(){
		    if(!Settings.UseUnityMasterServer){
			    MasterServer.ipAddress = Settings.MasterServerUrl;
			    MasterServer.port = Settings.MasterServerPort;
			    Network.natFacilitatorIP = Settings.MasterServerUrl;
		        Network.natFacilitatorPort = 50005; //Settings.MasterServerPort;
		    }			
		
		    if (ApplicationModel.lobbyEnteredAs == LobbyEnteredAs.PublicServer || ApplicationModel.lobbyEnteredAs == LobbyEnteredAs.PrivateServer) {
                networkHelper.StartServer( ApplicationModel.lobbyEnteredAs == LobbyEnteredAs.PrivateServer );
		    }else{
                networkHelper.ConnectToServer(ApplicationModel.HostToUse);
		    }
	    }
    

	//Leaves the network immediately, and goes to the main menue
	    public void ShutdownNetwork(){
		    networkHelper.Disconnect();
		    ApplicationModel.EnterMainMenue();
	    }


	//Is called, when a networking issue appears
	    void NetworkingError(string errorMessage){
		    networkHelper.Disconnect();
		    ApplicationModel.EnterMainMenue("Networking Error \n"+errorMessage);
	    }

    //Is called, if the connection got closed
        void OnDisconnected(string message){
            ApplicationModel.EnterMainMenue(message);
        }
           


}
