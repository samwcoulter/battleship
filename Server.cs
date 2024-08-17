using System.Net.Sockets;
using System.Collections.Generic;

public class Server
{
    private TcpListener _listener = null;
    private const int LISTEN_PORT = 9001;
    private List<TcpClient> _clients = new();
    private bool _running = true;

    public Server()
    {
        _listener = TcpListener.Create(LISTEN_PORT);
        // _listener.Start();
    }

    async void Listen()
    {
        _listener.Start();
        while (_running)
        {
            TcpClient client = await _listener.AcceptTcpClientAsync();
        }
    }

    ~Server()
    {
        _listener.Stop();
    }
}
