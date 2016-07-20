using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

public class Control : MonoBehaviour {
	// Variables to configure
	public int Port;

	// Constants
	const int ConnectionBacklog = 10;

	// Static (only one instance can/should be created)
	static Control _instance;

	// Member vars
	public bool IsServer;
	string _logMessage;
	Socket _socket;
	IPAddress _ip;
	List<Socket> _socketList;

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

	void Start() {
		// Init
		_logMessage = "Awake";
		_socketList = new List<Socket>();

		// Register logging capabilities normally not available
		Application.logMessageReceived += OnLog;

		if (!IsServer || !Host(Port)) return;
		gameObject.SendMessage("OnServerStarted");
	}

	void Update() {
		// Pass sockets to ServerControl
		if (_socketList.Count > 0) {
			gameObject.SendMessage("OnClientConnected", _socketList[0]);
			_socketList.RemoveAt(0);
		}
	}

	void OnApplicationQuit() {
		Application.logMessageReceived -= OnLog;
		Disconnect();
	}

	public bool Host(int port) {
		Debug.Log("Hosting on port " + port);
		_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

		try {
			_socket.Bind(new IPEndPoint(IP, port));
			_socket.Listen(ConnectionBacklog);
			_socket.BeginAccept(OnClientConnect, _socket);
		} catch (System.Exception e) {
			Debug.LogError("Exception when attempting to host (" + port + "): " + e);
			_socket = null;
			return false;
		}

		return true;
	}

	public void Disconnect() {
		if (_socket != null) {
			_socket.BeginDisconnect(false, OnEndHostComplete, _socket);
		}
	}

	void OnClientConnect(System.IAsyncResult result) {
		Debug.Log("Handling client connecting");

		try {
			_socketList.Add(_socket.EndAccept(result));
		} catch (System.Exception e) {
			Debug.LogError("Exception when accepting incoming connection: " + e);
		}

		try {
			_socket.BeginAccept(OnClientConnect, _socket);
		} catch (System.Exception e) {
			Debug.LogError("Exception when starting new accept process: " + e);
		}
	}

	void OnEndHostComplete(System.IAsyncResult result) {
		_socket = null;
	}

	// Get the IP address of this machine
	public IPAddress IP {
		get {
			if (_ip == null) {
				_ip = (
					from entry in Dns.GetHostEntry(Dns.GetHostName()).AddressList
					where entry.AddressFamily == AddressFamily.InterNetwork
					select entry
				).FirstOrDefault();
			}

			return _ip;
		}
	}

	void OnLog(string message, string callStack, LogType type) {
		_logMessage = message + "\n" + _logMessage;
	}
}
