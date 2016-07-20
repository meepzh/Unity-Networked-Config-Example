using System.Net.Sockets;
using System;

public delegate void IncomingReadHandler(SocketRead read, byte[] data);
public delegate void IncomingReadErrorHandler(SocketRead read, Exception exception);

public class SocketRead {
	public const int BufferSize = 1024;

	Socket _socket;
	IncomingReadHandler _readHandler;
	IncomingReadErrorHandler _errorHandler;
	byte[] buffer = new byte[BufferSize];

	public Socket Socket {
		get {
			return _socket;
		}
	}

	SocketRead(Socket socket, IncomingReadHandler readHandler, IncomingReadErrorHandler errorHandler) {
		_socket = socket;
		_readHandler = readHandler;
		_errorHandler = errorHandler;

		BeginReceive();
	}

	void BeginReceive() {
		_socket.BeginReceive(buffer, 0, BufferSize, SocketFlags.None, OnReceive, this);
	}

	public static SocketRead Begin(Socket socket, IncomingReadHandler readHandler, IncomingReadErrorHandler errorHandler) {
		return new SocketRead(socket, readHandler, errorHandler);
	}

	void OnReceive(IAsyncResult result) {
		try {
			if (result.IsCompleted) {
				int bytesRead = _socket.EndReceive(result);

				if (bytesRead > 0) {
					byte[] read = new byte[bytesRead];
					Array.Copy(buffer, 0, read, 0, bytesRead);

					_readHandler(this, read);
					Begin(_socket, _readHandler, _errorHandler);
				} // else Disconnect
			}
		} catch (Exception e) {
			UnityEngine.Debug.LogError(e);
			if (_errorHandler != null) {
				_errorHandler(this, e);
			}
		}
	}
}
