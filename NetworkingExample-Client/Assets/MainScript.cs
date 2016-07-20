using UnityEngine;

public class MainScript : MonoBehaviour {
	// GUI Vars
	public float GUIItemHeight;
	public float GUIItemWidth;
	public float GUIItemPadding;
	public GUIStyle ButtonStyle;
	public GUIStyle LabelStyle;

	// Networking
	ClientControl _client;
	Control _control;

	// Data
	float _data;

	void Start() {
		GameObject g = GameObject.Find("Client");
		_client = g.GetComponent<ClientControl>();
		_control = g.GetComponent<Control>();

		_data = 0f;
	}
	
	void Update() {
		if (_client.GetOutput().IndexOf((char)21) > 0) {
			// Disconnect
			_control.Disconnect();
		}
	}

	void OnGUI() {
		if (_control == null || !_control.GotIP) return;

		GUI.Label(new Rect(Screen.width / 2f - GUIItemWidth * 1.5f - GUIItemPadding,
						   Screen.height / 2f - GUIItemHeight - GUIItemPadding / 2f,
						   GUIItemWidth, GUIItemHeight),
				  "Penguins: " + _data.ToString("N"), LabelStyle);

		_data = GUI.HorizontalSlider(new Rect(Screen.width / 2f - GUIItemWidth / 2f,
											  Screen.height / 2f - GUIItemHeight / 2f - GUIItemPadding / 2f - 8f,
											  GUIItemWidth * 2f + GUIItemPadding, GUIItemHeight),
									 _data, 0f, 300f);

		if (GUI.Button(new Rect(Screen.width / 2f - GUIItemWidth / 2f,
								Screen.height / 2f + GUIItemPadding / 2f,
								GUIItemWidth, GUIItemHeight),
					   "Send", ButtonStyle)) {
			SendData();
		}
	}

	void SendData() {
		Debug.Log("Sending data");

		// Packet format: @DataName|Len=Number
		string dataNumber = _data.ToString("N");
		_client.Send("@Penguins|" + dataNumber.Length + "=" + dataNumber);
	}
}
