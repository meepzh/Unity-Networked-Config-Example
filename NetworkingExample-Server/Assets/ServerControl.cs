using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class ServerControl : MonoBehaviour {
	public List<Socket> Clients = new List<Socket>();

	// For receiving
	string _output = ""; // Data received
	string _outputBuffer = ""; // Data in process of being received
	
	// For sending
	string _input = ""; // Data to send
	bool _send; // Set to true when ready to send

	List<Socket> _disconnectedClients; // Temp list for removing clients
	bool _hadClients;

	public bool HadClients {
		get {
			return _hadClients;
		}
	}

	void Start() {
		_disconnectedClients = new List<Socket>();
	}

	void OnServerStarted() {
		Debug.Log("Server started");
	}

	void OnClientConnected(Socket client) {
		Debug.Log("Client attempting to connect");

		// We only allow one client at a time
		bool accept = Clients.Count == 0;
		if (!accept) {
			if (Clients.Count > 0) {
				// If client comes from same address, replace old
				IPEndPoint remoteEndPointOld = Clients[0].RemoteEndPoint as IPEndPoint;
				IPEndPoint remoteEndPointNew = client.RemoteEndPoint as IPEndPoint;
				accept = true;

				if (remoteEndPointOld != null && remoteEndPointNew != null) {
					if (!remoteEndPointOld.Address.Equals(remoteEndPointNew.Address)) {
						accept = false;
					}
				} else {
					accept = false;
				}

				if (accept) {
					Debug.Log("Old client rejected");
					Clients.RemoveAt(0);
				}
			}
			if (!accept) {
				Debug.Log("Client rejected");
				// Send client a shutdown code (this needs to be manually parsed)
				client.Send(Encoding.ASCII.GetBytes("" + (char)21));
			}
		}
		if (accept) {
			Debug.Log("Client accepted");
			Clients.Add(client);
			SocketRead.Begin(client, OnReceive, OnReceiveError);
		}
	}

	void OnReceive(SocketRead read, byte[] data) {
		_outputBuffer += Encoding.ASCII.GetString(data, 0, data.Length);
		bool holdBuffer = false; // Should we retain the buffer until we're ready?

		// If there's any extremely long data you're receiving, you might receive it in pieces
		// Check if the data is incomplete here

		if (holdBuffer) return;

		// Pass data in buffer out, clear buffer
		_output += _outputBuffer;
		_outputBuffer = "";
	}

	void OnReceiveError(SocketRead read, System.Exception exception) {
		Debug.LogError("Receive error: " + exception);
	}

	public string GetOutput() {
		string temp = _output;
		_output = "";
		return temp;
	}

	public void Send(string input) {
		_input = input;
		_send = _input.Length > 0;
	}

	void Update() {
		_hadClients = Clients.Count > 0;

		// Send your Input data here
		if (_input.Length == 0) {
			_send = false;
		}
		if (!_send) return;
		if (_hadClients) {
			// Input data is limited to the max buffer size
			int sendableLength = _input.Length < SocketRead.BufferSize ? _input.Length : SocketRead.BufferSize;

			// Trim input to buffer size
			string toSend = _input.Substring(0, sendableLength);
			_input = _input.Substring(sendableLength);

			// Send to each connected client
			foreach (Socket client in Clients) {
				try {
					client.Send(Encoding.ASCII.GetBytes(toSend));
				} catch (SocketException e) {
					Debug.LogError(e.ToString());
					// Can no longer use this client, queue it for removal
					// Cannot remove while in foreach
					_disconnectedClients.Add(client);
				}
			}

			// Remove errored clients
			if (_disconnectedClients.Count > 0) {
				foreach (Socket client in _disconnectedClients) {
					Clients.Remove(client);
					Debug.Log("Removed errored client");
				}
				_disconnectedClients.Clear();
			}
		} else {
			// If no clients, clear Input
			_input = "";
			_send = false;
		}
	}
}
