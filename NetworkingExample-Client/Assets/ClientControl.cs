using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class ClientControl : MonoBehaviour {
	// For receiving
	string _output = ""; // Data received
	string _outputBuffer = ""; // Data in process of being received

	// For sending
	string _input = ""; // Data to send
	bool _send; // Set to true when ready to send

	public Socket Socket;
	bool _serverRejected;

	void Start() {
	}

	void OnClientStarted(Socket socket) {
		Debug.Log("Client started");
		Socket = socket;
		SocketRead.Begin(Socket, OnReceive, OnError);
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

	void OnError(SocketRead read, System.Exception exception) {
		Debug.LogError("Error: " + exception);
	}

	public string GetOutput() {
		string temp = _output;
		_output = "";
		return temp;
	}

	public void Send(string input) {
		_input = input;
		_send = _input.Length > 0;

		if (Socket == null) Debug.Log("Socket is null");
	}

	void Update() {
		if (Socket == null) return;

		// Send your Input data here
		if (_input.Length == 0) {
			_send = false;
		}
		if (!_send) return;

		// Input data is limited to the max buffer size
		int sendableLength = _input.Length < SocketRead.BufferSize ? _input.Length : SocketRead.BufferSize;

		// Trim input to buffer size
		string toSend = _input.Substring(0, sendableLength);
		_input = _input.Substring(sendableLength);

		// Send to server
		try {
			Socket.Send(Encoding.ASCII.GetBytes(toSend));
		} catch (SocketException e) {
			Debug.LogError(e.ToString());
			// Can no longer use this server remove it
			Socket = null;
		}
	}
}
