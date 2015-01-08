using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;




//This script is completly static, and therefore can be reached everywhere.
//This script is a singleton
public sealed class Settings : MonoBehaviour {
	public const string GAME_NAME = "fridolinMajaSkitch"; 	//Unique GameName
	public const int PORT_NO = 25005;						//If several servers should be started on the same machine/device: use different ports
	public const int MAX_PLAYERS = 8;
	public const string ROOM_CHARS = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
	public const float LOBBY_REFRESH_RATE = 5.0f;			//How often the host-list should be retrieved. Value = delay between polling in seconds



	static public string PlayerName { get; private set; }

	static string MasterServerString;
	static public bool UseUnityMasterServer { get; private set; }		//true: the unity server is used; ignore the url and the port
	static public string MasterServerUrl { get; private set; }
	static public int MasterServerPort { get; private set; }
    
    static List<string> predefinedNames = new List<string>{ "Emil", "Mathias", "Magnus", "Jonas", "William", "Oliver", "Noah", "Tobias", "Adrian", "Elias", "Daniel", "Henrik", "Sebastian", "Lucas",
                                                            "Martin", "Andreas", "Benjamin", "Leon", "Sander", "Alexander", "Isak", "Liam", "Jakob", "Kristian", "Aksel", "Fredrik", "Julian", "Sondre", 
                                                            "Johannes", "Erik", "Marius", "Jonathan", "Filip", "Sigurd", "Håkon", "Lukas", "Markus", "Eirik", "Oscar", "Theodor", "Theo", "Mikkel", 
                                                            "Oskar", "Gabriel", "Kasper", "David", "Marcus", "Olav", "Even", "Herman", "Emma", "Nora", "Sofie", "Thea", "Ingrid", "Emilie", "Julie", 
                                                            "Mia", "Anna", "Ida", "Linnea", "Amalie", "Sara", "Maria", "Ella", "Leah", "Maja", "Tuva", "Frida", "Vilde", "Mathilde", "Sofia", "Marie", 
                                                            "Olivia", "Jenny", "Aurora", "Hanna", "Malin", "Elise", "Victoria", "Oda", "Selma", "Hedda", "Mari", "Eline", "Martine", "Mina", "Julia", 
                                                            "Pernille", "Andrea", "Mathea", "Alma", "Amanda", "Celine", "Tiril", "Mille", "Sarah", "Synne", "Isabella", "Hannah" };

	//Here is a private reference only this class can access
	private static Settings _instance;	
	//This is the public reference that other classes will use
	public static Settings instance{
		get{
			//If _instance hasn't been set yet, we grab it from the scene!
			//This will only happen the first time this reference is used.
			if(_instance == null){
				_instance = GameObject.FindObjectOfType<Settings>();
			}
			return _instance;
		}
	}



			
	// Load settings from storage
	void Start () {
		Settings.Load();
		Debug.Log("Loaded settings: "+PlayerName+"/"+UseUnityMasterServer+"/"+MasterServerUrl+"/"+MasterServerPort);
	}






	static void Load(){
		if(!PlayerPrefs.HasKey("PlayerName")){
			SetPlayerName( GetRandomPlayerName() );
		}else{
			PlayerName = PlayerPrefs.GetString("PlayerName");
		}

		if(!PlayerPrefs.HasKey("UseUnityMasterServer") || !PlayerPrefs.HasKey("MasterServerUrl") || !PlayerPrefs.HasKey("MasterServerPort")){
			SetUnityMasterServer();
		}else{
			UseUnityMasterServer = PlayerPrefs.GetInt("UseUnityMasterServer") != 0;
			MasterServerUrl = PlayerPrefs.GetString("MasterServerUrl");
			MasterServerPort = PlayerPrefs.GetInt("MasterServerPort");
		}
		MasterServerString = GetMasterServerStringNorm ();
	}
	
	static void Save(){
		PlayerPrefs.SetString("PlayerName", PlayerName);
		PlayerPrefs.SetInt("UseUnityMasterServer", UseUnityMasterServer?1:0);
		PlayerPrefs.SetString("MasterServerUrl", MasterServerUrl);
		PlayerPrefs.SetInt("MasterServerPort", MasterServerPort);
	}
		
	
	
	//value:  <serverUrl>:<serverPort>  -or-  "unity"
	static public void SetMasterServer(string value){
		MasterServerString = value;
		if (value.ToLower() == "unity") {
			SetUnityMasterServer();
			return;
		}
		UseUnityMasterServer = false;

		var idx = value.LastIndexOf (':');
		if (idx < 0) { //No port
			MasterServerUrl = value;
			MasterServerPort = 23466;
			Save();
			return;
		}

		int port = 0;
		try{
			port = Convert.ToInt32( value.Substring(idx+1) );
		}catch (FormatException){		//Invalid integer
			MasterServerUrl = value;
			MasterServerPort = 23466;
			Save();
			return;
		}
		
		if(port <= 0 || port >=65536){	//Invalid port
			MasterServerUrl = value;
			MasterServerPort = 23466;
			Save();
			return;
		}
		
		MasterServerPort = port;
		if(idx == 0){					//no url
			MasterServerUrl = "72.52.207.14";
		}else{
			MasterServerUrl = value.Substring(0, idx);
		}
		Save();
	}
	
	//returna:  <serverUrl>:<serverPort>, but not normalised (as the user has entered it) 
	static public string GetMasterServerString(){
		return MasterServerString;
	}


	//return:  <serverUrl>:<serverPort>   -or-   "unity"
	static public string GetMasterServerStringNorm(){
		if(UseUnityMasterServer){
			return "unity";
		}else{
			return MasterServerUrl + ":" + MasterServerPort;
		}
	}
	
	static public void SetUnityMasterServer(){
		UseUnityMasterServer = true;
		MasterServerUrl = "0.0.0.0";
		MasterServerPort = 0;
		MasterServerString = GetMasterServerStringNorm ();
		Save ();
	}
	
	static public void SetLocalMasterServer(){
		MasterServerUrl = "127.0.0.1";
		MasterServerPort = 23466;
		UseUnityMasterServer = false;
		MasterServerString = GetMasterServerStringNorm ();
		Save ();
	}



	static public string GetRandomPlayerName(){
		return predefinedNames[UnityEngine.Random.Range(0, predefinedNames.Count)];;
	}

	static public void SetPlayerName(string _PlayerName){
		PlayerName = _PlayerName;
		Save();
	}
}
