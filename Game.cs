using Godot;
using System.Collections.Generic;
using Stateless;

public partial class Game : Node
{
    public enum State
    {
        NotStarted,
        PlayerTurn,
        End
    }

    public enum Command
    {
        Start,
        NextTurn,
        End
    }

    private struct Player
    {
        public long id;
        public PlayerState state = new();

        public Player(long playerId)
        {
            id = playerId;
        }
    }

    private const int PORT = 9999;
    private ENetMultiplayerPeer _peer = new(); 
    private CanvasLayer _mainMenu;
    private CenterContainer _game;
    private List<Player> _players = new();
    private Board _enemy;
    private Board _self;
    private Label _health;
    private Label _turnLabel;
    private int _currentPlayer = 0;
    private StateMachine<State, Command> _machine;
    private StateMachine<State, Command>.TriggerWithParameters<int> _turnTrigger;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _mainMenu = GetNode<CanvasLayer>("MainMenu");
        _game = GetNode<CenterContainer>("Game");
        _mainMenu.Show();
        _game.Hide();

        _machine = new StateMachine<State, Command>(State.NotStarted);
        _turnTrigger = _machine.SetTriggerParameters<int>(Command.NextTurn);

        _machine.Configure(State.NotStarted).Permit(Command.Start, State.PlayerTurn);
        _machine.Configure(State.PlayerTurn)
            .OnEntryFrom(_turnTrigger, player => OnStartTurn(player))
            .PermitReentry(Command.NextTurn)
            .Permit(Command.End, State.End);

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
        PackedScene pScene =  GD.Load<PackedScene>("res://Player.tscn");
        _game.AddChild(pScene.Instantiate());
        PlayerScene playerScene = _game.GetNode<PlayerScene>("Player");
        _self = playerScene.Self();
        _enemy = playerScene.Enemy();
        _health = playerScene.HealthLabel();
        _turnLabel = playerScene.TurnLabel();
        _peer.CreateClient("localhost", PORT);
        Multiplayer.ConnectedToServer += OnServerConnected;
        Multiplayer.MultiplayerPeer = _peer;
        _mainMenu.Hide();
        _game.Show();
    }


    public void OnPlayerConnected(long id)
    {
        GD.Print($"Player connected {Multiplayer.GetUniqueId()} - {id}");
        _players.Add(new Player(id));
        if (_players.Count == 2)
        {
            System.Random rng = new();
            _currentPlayer = rng.Next(2);
            _machine.Fire(Command.Start);
            _machine.Fire(_turnTrigger, _currentPlayer);
        }
    }

    public void OnServerConnected()
    {
        GD.Print($"Server connected {Multiplayer.GetUniqueId()}");
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void SendPlayerState(string state)
    {
        GD.Print($"Hello from server {Multiplayer.GetUniqueId()}");
        PlayerState p = new();
        p.Deserialize(state);
        _self.SetState(p);
        _health.Text = p.Health.ToString();
    }

    public void OnStartTurn(int playerIndex)
    {
        foreach (Player p in _players)
        {
            RpcId(p.id, nameof(SendPlayerState), p.state.Serialize());
        }
        Player activePlayer = _players[_currentPlayer];
        RpcId(activePlayer.id, nameof(StartTurn));
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void StartTurn()
    {
        _enemy.IsClickable = true;
        _turnLabel.Text = "Your turn!";
    }
    private int NextTurn(int turn)
    {
        return turn + 2 % 2;
    }
}
