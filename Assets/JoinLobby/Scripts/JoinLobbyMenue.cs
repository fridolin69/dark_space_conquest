using UnityEngine;
using System.Collections;

/*
		GameObject ob = Instantiate (GameHostPrefab, new Vector3 (0, 0, 0), Quaternion.identity) as GameObject;
		ob.GetComponent<GameHost>().Initialize(5);

		ob = Instantiate (GameHostPrefab, new Vector3 (0, 0, 0), Quaternion.identity) as GameObject;
		ob.GetComponent<GameHost>().Initialize(9);
		ob.transform.position = new Vector3 (20, 20, 0);
		ob.transform.localScale = new Vector3(2.5f, 2.5f, 1f);
*/

public class JoinLobbyMenue : MonoBehaviour {

	GuiHelper guiHelper;
	NetworkHelper networkHelper;	//For "low-level" network tasks

	Vector2 hostListScrollPosition;


    private string directConnectLobbyName = "";
	/*~~ STATUS ~~*/
	/*~~  ----  ~~*/


	void Start () {
		guiHelper = GameObject.Find ("Menue").GetComponent<GuiHelper>();
		if (guiHelper == null) {
			throw new MissingComponentException ("Unable to find GuiHelper.");
		}

		//Initialize the network helper for hardcore lowlevel-commands  (not really, but sounds better)
			networkHelper = (NetworkHelper)gameObject.AddComponent(typeof(NetworkHelper));
			networkHelper.SetNetworkingErrorHandler (NetworkingError);
            networkHelper.SetOnDisconnectedHandler (OnDisconnected);

		StartCoroutine("PollHostList");
	}


	//Coroutine, to poll the hostList every x seconds
	private IEnumerator PollHostList(){
		for(;;){
			networkHelper.RefreshHostList();
			yield return new WaitForSeconds(Settings.LOBBY_REFRESH_RATE); 	//wait some time, before we poll again
		}
	}


	void OnGUI () {
		guiHelper.Prepare();
        //int fontSize = (int)GuiHelper.X(GuiHelper.Y(45, 10, 15), 2.5f, 5f);
        int fontSize = (int)GuiHelper.Y(GuiHelper.X(45, 2.5f, 5f), 1.8f, 10f);
        GUI.skin.label.fontSize = fontSize/2;
	    GUI.skin.textField.fontSize = fontSize/2;
        GUI.skin.button.fontSize = fontSize/2;

		guiHelper.TitleText ("Join Game");

        if (guiHelper.ExitButton("Back") || Input.GetKeyDown(KeyCode.Escape))
        {
			networkHelper.Disconnect();
			ApplicationModel.EnterMainMenue();
		}

        HostData[] hostList = networkHelper.GetHostList();

        GUI.Label(new Rect(guiHelper.GetWindowPadding(), guiHelper.GetTitleSpace(), GuiHelper.XtoPx(30), guiHelper.SmallElemHeight), "Direct connect:");
        directConnectLobbyName = GUI.TextField(new Rect(GuiHelper.XtoPx(30), guiHelper.GetTitleSpace()-4, GuiHelper.XtoPx(25), guiHelper.SmallElemHeight+8), directConnectLobbyName, 6).ToUpper();
        HostData direct = null;
        if (hostList != null && directConnectLobbyName.Length >= 6) {
            for (int i = 0; i < hostList.Length; i++) {
                if (hostList[i].gameName == directConnectLobbyName || hostList[i].gameName == "priv_" + directConnectLobbyName) {
                    direct = hostList[i]; break;
                }
            }
            if (direct != null && GUI.Button(new Rect(GuiHelper.XtoPx(60), guiHelper.GetTitleSpace(), GuiHelper.XtoPx(20), guiHelper.SmallElemHeight), "Join")) {
                ApplicationModel.EnterLobbyAsClient(direct);
            }
        }

	    GUI.skin.label.fontSize *= 2;
        GUI.skin.button.fontSize *= 2;

        GUILayout.BeginArea(new Rect(0, guiHelper.GetTitleSpace() + guiHelper.SmallElemHeight + guiHelper.BigElemSpacing, Screen.width, Screen.height - guiHelper.GetTitleSpace() - guiHelper.GetExitButtonSpace() - guiHelper.SmallElemHeight - guiHelper.BigElemSpacing));

	    int onlinePlayers = 0;
            if (hostList != null){
                hostListScrollPosition = GUILayout.BeginScrollView(hostListScrollPosition);

                for (int i = 0; i < hostList.Length; i++) {
                    onlinePlayers += hostList[i].connectedPlayers;
                    if (hostList[i].gameName.StartsWith("priv_")){
                        continue;
                    }
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(GuiHelper.XtoPx(20));
                    GUILayout.Label(hostList[i].comment.Substring(1));
                    GUILayout.EndHorizontal();

                    Rect region = GUILayoutUtility.GetLastRect();
                    Rect smallRegion = new Rect(region);
                    smallRegion.width /= 3.5f;
                    GUI.Label(smallRegion, hostList[i].gameName);

                    smallRegion.width = region.width / 5;
                    smallRegion.x = region.x + smallRegion.width * 1.35f;
                    GUI.Label(smallRegion, hostList[i].connectedPlayers + "/" + hostList[i].playerLimit);

                    smallRegion.x = region.x + smallRegion.width * 4f;
                    if (GUI.Button(smallRegion, "Join")){
                        ApplicationModel.EnterLobbyAsClient(hostList[i]);
                    }
                    GUILayout.Space(10);
                }

				GUILayout.EndScrollView();
            }
		GUILayout.EndArea();
        GUI.skin.label.fontSize /= 2;
        GUI.Label(new Rect((Screen.width - GuiHelper.XtoPx(50)) / 2, Screen.height - guiHelper.SmallElemHeight - GuiHelper.YtoPx(4), GuiHelper.XtoPx(50), guiHelper.SmallElemHeight), onlinePlayers + " players ingame");
        GUI.skin.label.fontSize *= 2;
	}

	//Is called, when a networking issue appears
	void NetworkingError(string errorMessage){
		networkHelper.Disconnect();
		ApplicationModel.EnterMainMenue("Networking Error \n"+errorMessage);
	}



     void OnDisconnected(string message){
        ApplicationModel.EnterMainMenue(message);
     }
}





