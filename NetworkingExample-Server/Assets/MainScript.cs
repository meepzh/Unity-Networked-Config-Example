using UnityEngine;

public class MainScript : MonoBehaviour {
	// Networking
	ServerControl _server;
	Control _control;
	string _ipAddress;

	// Output
	public float GUIItemHeight;
	public float GUIItemWidth;
	public GUIStyle OutputStyle;
	string _incomingData;
	string _outputString;
	bool _updatedOutput;

	// Use this for initialization
	void Start() {
		_incomingData = "";

		GameObject g = GameObject.Find("Server");
		_server = g.GetComponent<ServerControl>();
		_control = g.GetComponent<Control>();
		_ipAddress = _control.IP.ToString();
	}
	
	// Update is called once per frame
	void Update() {
		if (_server.HadClients) {
			if (!_updatedOutput) {
				_outputString = "Connected!";
			}

			// Get new data
			_incomingData = _server.GetOutput();

			ParseIncomingData();
		} else {
			// Show the IP you need to connect to
			_outputString = _ipAddress;
			_updatedOutput = false;
		}
	}

	void OnGUI() {
		GUI.Box(new Rect(Screen.width / 2f - GUIItemWidth / 2f,
						 Screen.height / 2f - GUIItemHeight / 2f,
						 GUIItemWidth, GUIItemHeight),
				_outputString, OutputStyle);
	}

	void ParseIncomingData() {
		// Foreach data point
		while (_incomingData.Length > 0) {
			Debug.Log("Parsing data packet");

			// Expected data format: @DataName|Len=Number
			int startIndex = _incomingData.IndexOf('@');
			int dividerIndex = _incomingData.IndexOf('|');
			int equalsIndex = _incomingData.IndexOf('=');

			// Clear invalid data
			if (equalsIndex <= startIndex) {
				Debug.Log("Invalid data: " + _incomingData);

				// Clear everything up to the nearest start character following the current one
				int newStartIndex = _incomingData.IndexOf('@', startIndex + 1);
				if (newStartIndex < 0) {
					// None available, clear all
					_incomingData = "";
				} else {
					_incomingData = _incomingData.Substring(newStartIndex);
				}
				continue;
			}

			// Parse initial data
			string dataName = _incomingData.Substring(startIndex + 1, dividerIndex - startIndex - 1);
			int length = int.Parse(_incomingData.Substring(dividerIndex + 1, equalsIndex - dividerIndex - 1));

			if (_incomingData.Length <= equalsIndex + length) {
				// Length is invalid, clear all
				Debug.Log("Invalid length " + length + " for input: " + _incomingData);
				_incomingData = "";
				continue;
			}

			// Parse
			float data = float.Parse(_incomingData.Substring(equalsIndex + 1, length));

			// Output
			_outputString = dataName + " is now " + data.ToString("N") + "!";
			_updatedOutput = true;

			// Remove data from buffer
			_incomingData = _incomingData.Substring(equalsIndex + length + 1);
		}
	}
}
