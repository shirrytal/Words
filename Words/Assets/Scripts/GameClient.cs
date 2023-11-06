using System.Net.WebSockets;
using System.Threading;
using System;
using System.Threading.Tasks;
using System.Text;

public class GameClient {

    private readonly ClientWebSocket socket;
    private readonly CancellationToken c_token;
    private bool isConnected = false;
    public GameClient()
    {
        socket = new ClientWebSocket();
    }

    public bool IsConnected() { return isConnected; }

    public Task ReceiveMessage(out ArraySegment<byte> segment) {
        byte[] buffer = new byte[1024];  
        segment = new ArraySegment<byte>(buffer);
        return socket.ReceiveAsync(segment, c_token);    
    }

    public Task SendMessage(string message_content) {
        if (socket == null || socket.State != WebSocketState.Open)
        {
            throw new InvalidOperationException("Connection is not open.");
        }

        // Encode the string message to a byte array.
        var bytes = Encoding.UTF8.GetBytes(message_content);

        var arraySegment = new ArraySegment<byte>(bytes);

        // Send the data over the WebSocket.
        return socket.SendAsync(
            arraySegment,
            WebSocketMessageType.Text,
            endOfMessage: true,
            cancellationToken: c_token
        );
    }

    public Task Connect() {
       return socket.ConnectAsync(new Uri("ws://127.0.0.1:8000/ws/game/"), c_token)
            .ContinueWith(task => { if (task.IsCompletedSuccessfully) isConnected = true; return task; });
    }

    public Task Disconnect(WebSocketCloseStatus status = WebSocketCloseStatus.NormalClosure, string desc = "Connection end") {
        return  socket.CloseAsync(status, desc, c_token);
    }
}


