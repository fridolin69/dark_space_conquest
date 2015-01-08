using UnityEngine;
using System.Collections;

public class LobbyHeader : MonoBehaviour {
	
	GuiHelper guiHelper;


	
	public string LobbyName { get; set; }
	
	// Use this for initialization
	void Start () {
		guiHelper = GetComponent<GuiHelper>();
		if (guiHelper == null) {
			throw new MissingComponentException ("Unable to find GuiHelper.");
		}
	}


	// Update is called once per frame
	void OnGUI () {	
		guiHelper.Prepare();	
	/*	GUI.skin = skin;
		position = new Rect (0, 0, Screen.width, GuiHelper.Y (35, 5, 10) );
		das positionieren eventuell int einer anderen klasse durchführen...


		GUI.TextArea(position, "LOBBY HEADER");*/
	}
	
	







}
