using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/*
 * GameObject(s)
 */

class ChatEntry{
	public string name {get; private set;}
	public string message {get; private set;}
    public Color nameColor {get; private set;}
    public Color messageColor {get; private set;}
    
    public ChatEntry(string name, string message, Color nameColor) : this(name, message, nameColor, nameColor){        ;
    }
    
    public ChatEntry(string name, string message, Color nameColor, Color messageColor){
        this.name = name;
        this.message = message;
        this.nameColor = nameColor;
        this.messageColor = messageColor;
    }
}



public class Chat : MonoBehaviour {

	GuiHelper guiHelper;
    PlayerList playerList;

    public int MAX_CHAT_ENTRIES = 50;   //wenn mehr nachrichten empfangen werden, werden die ältesten einträge gelöscht

	string messageInput = "";

	List<ChatEntry> chatEntries = new List<ChatEntry> ();
	Vector2 scrollPosition;

	void Start () {
		guiHelper = GameObject.Find ("GUI").GetComponent<GuiHelper>();
		if (guiHelper == null) {
			throw new MissingComponentException ("Unable to find GuiHelper.");
		}
		playerList = GameObject.Find ("PlayerList").GetComponent<PlayerList>();
        if (playerList == null) {
            throw new MissingComponentException ("Unable to find PlayerList.");
        }
	}

	void OnGUI () {
		guiHelper.Prepare();

		Rect chatArea = new Rect (guiHelper.GetWindowPadding(),
                                  guiHelper.GetWindowPadding(), 
                                  Screen.width / 2 - guiHelper.GetWindowPadding()*2, 
                                  Screen.height - guiHelper.GetExitButtonSpace() - guiHelper.ElemHeight - guiHelper.ElemSpacing );		
        Rect msgBoxArea = new Rect(guiHelper.GetWindowPadding(), 
                                   Screen.height - guiHelper.GetExitButtonSpace() - guiHelper.ElemHeight, 
                                   Screen.width / 2 - guiHelper.GetWindowPadding()*2 - guiHelper.SmallElemWidth/2, 
                                   guiHelper.ElemHeight);
        Rect msgSendBtnArea = new Rect(
                                   msgBoxArea.xMax + guiHelper.GetWindowPadding(),
                                   msgBoxArea.yMin + (guiHelper.ElemHeight - guiHelper.SmallElemHeight)/2, 
                                   guiHelper.SmallElemWidth/2 - guiHelper.GetWindowPadding(),
                                   guiHelper.SmallElemHeight);
        Rect prevMapBtnArea = new Rect(
                                   Screen.width - GuiHelper.XtoPx(13) - 25,
                                   Screen.height / 2 + 20,
                                   GuiHelper.XtoPx(13),
                                   GuiHelper.YtoPx(6));
        Rect nextMapBtnArea = new Rect(
                                   Screen.width - GuiHelper.XtoPx(13) - 25,
                                   prevMapBtnArea.y + prevMapBtnArea.height + 40,
                                   GuiHelper.XtoPx(13),
                                   GuiHelper.YtoPx(6));

		GUILayout.BeginArea (chatArea);
		scrollPosition = GUILayout.BeginScrollView (scrollPosition);

			//ChatEntry last = null;
			foreach(ChatEntry entry in chatEntries){
                string[] lines = entry.message.Split('\n');

                GUIStyle style = guiHelper.SmallLabelStyle;
                style.normal.textColor = entry.nameColor;
                //style.fontStyle = FontStyle.Bold;
                GUILayout.BeginHorizontal();
                    GUILayout.Label (entry.name+": ", style, GUILayout.Width(chatArea.width/3));
                    //GUILayout.Label (entry.name+": ", style);
                       
                    GUILayout.BeginVertical();
                        //style.fontStyle = FontStyle.Normal;
                        style.normal.textColor = entry.messageColor;
                        GUILayout.Label (lines[0], style);
                        for(int i=1; i<lines.Length; i++){
                            GUILayout.Label (/*"    "+*/lines[i], guiHelper.SmallLabelStyle);
                        } 
                    GUILayout.EndVertical();                    
                GUILayout.EndHorizontal();
                               
				GUILayout.Space(3);
			}

		GUILayout.EndScrollView ();
		GUILayout.EndArea ();

		GUI.SetNextControlName ("messageInput");
		messageInput = GUI.TextField (msgBoxArea, messageInput);

        GUI.Button(prevMapBtnArea, "<");
        GUI.Button(nextMapBtnArea, ">");

        if (GUI.Button(msgSendBtnArea, "↵") || (Event.current.isKey && Event.current.keyCode == KeyCode.Return && GUI.GetNameOfFocusedControl() == "messageInput"))
        {
            messageInput = messageInput.Trim();
            if (messageInput != "")
            {
                PostChatMessage(messageInput);
                messageInput = "";
            }
        }
	}











	// The user entered a chat-message --> send to all (the
	void PostChatMessage(string message){
        if(Network.isServer){
            /* spam filter goes here */
            networkView.RPC ("ChatMessage", RPCMode.All, message, Network.player);
        }
		networkView.RPC ("ClientsChatMessageRequest", RPCMode.Server, message);
	}


	// server: a client wants to post a chat message
	[RPC]
	void ClientsChatMessageRequest(string message, NetworkMessageInfo info){
        if(Network.isClient){
            return;
        }
		/* spam filter goes here */
		networkView.RPC ("ChatMessage", RPCMode.All, message, info.sender);
	}


	// Is called on server+client everytime a chat-message should be added
	[RPC]
	void ChatMessage(string message, NetworkPlayer networkPlayer){
        Player player = playerList.GetPlayer(networkPlayer);
        if(player == null){
            Debug.LogError("An unregistered player tried to write into the chat.");
            return;
        }
        AddMessage (new ChatEntry (player.name, message, Characters.GetCharacterColor(player)));
        scrollPosition.y = float.MaxValue;
	}

    
    
    public void SystemMessage(string message){
        SystemMessage(message, Color.yellow);
    }
    public void SystemMessage(string message, Color messageColor){
        AddMessage (new ChatEntry ("SYSTEM", message, Color.yellow, messageColor));
        scrollPosition.y = float.MaxValue;
    }

    void AddMessage(ChatEntry entry){
        chatEntries.Add (entry);
        if(chatEntries.Count > MAX_CHAT_ENTRIES){
            chatEntries.RemoveAt(0);
        }
    }
}
