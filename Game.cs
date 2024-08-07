using Godot;

public partial class Game : Node
{
    private const int PORT = 9999;
    private ENetMultiplayerPeer _peer = new(); 
    private CanvasLayer _mainMenu;
    private CenterContainer _game;
    private int _playersConnected = 0;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _mainMenu = GetNode<CanvasLayer>("MainMenu");
        _game = GetNode<CenterContainer>("Game");
        _mainMenu.Show();
        _game.Hide();

        GD.Print("Starting Game");
    }

    [Signal]
    public delegate void MySignalEventHandler();

    public override void _Input(InputEvent ev)
    {
        if (ev is InputEventKey evk)
        {
            if (evk.KeyLabel == Key.Q)
            {
                GetTree().Root.PropagateNotification((int)NotificationWMCloseRequest);
                GetTree().Quit();
            }
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    public void OnHostButton()
    {
        _peer.CreateServer(PORT);
        Multiplayer.PeerConnected += OnPlayerConnected;
        Multiplayer.MultiplayerPeer = _peer;
        _mainMenu.Hide();
        _game.Show();
    }

    public void OnJoinButton()
    {
        _peer.CreateClient("localhost", PORT);
        Multiplayer.ConnectedToServer += OnServerConnected;
        Multiplayer.MultiplayerPeer = _peer;
        _mainMenu.Hide();
        _game.Show();
    }

    public void OnPlayerConnected(long id)
    {
        GD.Print($"Player connected {Multiplayer.GetUniqueId()} - {id}");
        _playersConnected++;
        if (_playersConnected == 2)
        {
            Rpc(nameof(SendGameState));
        }
    }

    public void OnServerConnected()
    {
        GD.Print($"Server connected {Multiplayer.GetUniqueId()}");
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void SendGameState()
    {
        GD.Print($"Hello from server {Multiplayer.GetUniqueId()}");
    }
}
