using UnityEngine;
using System.Net;
using System.Net.Sockets;

public class Control : MonoBehaviour {
	// Variables to configure
	public int Port;

	// GUI Vars
	public float GUIItemHeight;
	public float GUIItemWidth;
	public float GUIItemPadding;
	public GUIStyle AlertStyle;
	public GUIStyle ButtonStyle;
	public GUIStyle LabelStyle;
	public GUIStyle TextBoxStyle;

	// Static (only one instance can/should be created)
	static Control _instance;

	// Member vars
	public bool IsServer;
	bool _gotIP;
	bool _failedConnection;
	string _savedIP;
	string _logMessage;
	string _serverIP;
	Socket _socket;

	static Control Instance {
		get {
			if (_instance == null) {
				_instance = (Control)FindObjectOfType(typeof(Control));
			}
			return _instance;
		}
	}

	public static Socket Socket {
		get {
			return _instance._socket;
		}
	}

	public bool GotIP {
		get {
			return _gotIP;
		}
	}

	void Start() {
		// Init
		_gotIP = false;
		_savedIP = PlayerPrefs.HasKey("SavedIP") ? PlayerPrefs.GetString("SavedIP") : "";
		_logMessage = "Awake";
		_serverIP = "";

		// Register logging capabilities normally not available
		Application.logMessageReceived += OnLog;
	}

	void OnApplicationQuit() {
		Application.logMessageReceived -= OnLog;
		Disconnect();
	}

	public bool Connect(IPAddress ip, int port) {
		Debug.Log("Connecting to " + ip + " on port " + port);
		_failedConnection = false;

		_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		try {
			_socket.Connect(new IPEndPoint(ip, port));
		} catch (SocketException) {
			// Allow code to precede to connection failed code
		}

		if (!_socket.Connected) {
			Debug.LogError("Failed to connect to " + ip + " on port " + port);
			_socket = null;
			_failedConnection = true;
			return false;
		}

		gameObject.SendMessage("OnClientStarted", _socket);
		PlayerPrefs.SetString("SavedIP", _serverIP);
		PlayerPrefs.Save();
		_gotIP = true;

		return true;
	}

	public void Disconnect() {
		_gotIP = false;
		_socket = null;
	}

	void OnGUI() {
		if (_gotIP) return;

		GUI.Label(new Rect(Screen.width / 2f - GUIItemWidth - GUIItemPadding / 2f,
							Screen.height / 2f - GUIItemHeight - GUIItemPadding / 2f,
							GUIItemWidth, GUIItemHeight),
					"IP Address: ", LabelStyle);
		_serverIP = GUI.TextArea(new Rect(Screen.width / 2f + GUIItemPadding / 2f,
											Screen.height / 2f - GUIItemHeight - GUIItemPadding / 2f,
											GUIItemWidth, GUIItemHeight),
									_serverIP, TextBoxStyle);

		string buttonOutput = "";
		if (_serverIP.Length > 0 || _savedIP.Length == 0) {
			buttonOutput = "Connect";
		} else {
			buttonOutput = _savedIP;
		}

		if (GUI.Button(new Rect(Screen.width / 2f - GUIItemWidth / 2f,
								Screen.height / 2f + GUIItemPadding / 2f,
								GUIItemWidth, GUIItemHeight),
						buttonOutput, ButtonStyle)) {
			if (_serverIP.Length > 0) {
				Connect(IPAddress.Parse(_serverIP), Port);
			} else if (_savedIP.Length > 0) {
				_serverIP = _savedIP;
				Connect(IPAddress.Parse(_savedIP), Port);
			}
		}

		if (_failedConnection) {
			GUI.Label(new Rect(Screen.width / 2f - GUIItemWidth * 2f,
								Screen.height / 2f + GUIItemPadding * 2f + GUIItemHeight,
								GUIItemWidth * 4, GUIItemHeight),
						"Failed to connect!", AlertStyle);
		}
	}

	void OnLog(string message, string callStack, LogType type) {
		_logMessage = message + "\n" + _logMessage;
	}
}
