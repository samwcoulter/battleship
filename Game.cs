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
        _machine.Configure(State.End)
            .OnEntry(() => OnGameEnd());

        _machine.OnTransitioned(t => GD.Print($"OnTransitioned: {t.Source} -> {t.Destination} via {t.Trigger}({string.Join(", ",  t.Parameters)})"));

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
        _enemy.OnClick += OnEnemyCellClicked;
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
        GD.Print($"SendPlayerState {Multiplayer.GetUniqueId()}");
        GD.Print(state);
        PlayerState p = new();
        p.Deserialize(state);
        _self.SetState(p);
        _health.Text = p.Health.ToString();
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void SendPlayerHealth(int health)
    {
        GD.Print($"SendPlayerHealth {Multiplayer.GetUniqueId()}");
        _health.Text = health.ToString();
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void SendEnemyState(string state)
    {
        GD.Print($"SendEnemyState {Multiplayer.GetUniqueId()}");
        GD.Print(state);
        PlayerState p = new();
        p.Deserialize(state);
        _enemy.SetState(p);
    }

    public void SendState()
    {
        for (int i = 0; i < _players.Count; i++)
        {
            Player p = _players[i];
            Player e = _players[NextTurn(i)];
            RpcId(p.id, nameof(SendPlayerState), p.state.Serialize());
            RpcId(p.id, nameof(SendPlayerHealth), p.state.Health);
            RpcId(p.id, nameof(SendEnemyState), e.state.SerializeSecret());
        }
    }

    public void OnStartTurn(int playerIndex)
    {
        GD.Print($"OnStartTurn {playerIndex}");
        _currentPlayer = playerIndex;
        SendState();
        RpcId(_players[playerIndex].id, nameof(StartTurn));
    }

    public void OnGameEnd()
    {
        SendState();
        for (int i = 0; i < _players.Count; i++)
        {
            Player p = _players[i];
            if (p.state.Health > 0)
            {
                RpcId(p.id, nameof(Win));
            }
            else
            {
                RpcId(p.id, nameof(Lose));
            }
        }

    }

    public void OnEnemyCellClicked(int x, int y)
    {
        GD.Print($"On enemy Cell clicked {Multiplayer.GetUniqueId()}");
        _enemy.IsClickable = false;
        _turnLabel.Text = "Enemy Turn!";
        RpcId(1, nameof(AttackCell), new Vector2I(x, y));
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void StartTurn()
    {
        GD.Print($"Start Turn {Multiplayer.GetUniqueId()}");
        _enemy.IsClickable = true;
        _turnLabel.Text = "Your turn!";
    }
    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void Lose()
    {
        _enemy.IsClickable = false;
        _turnLabel.Text = "You Lose! :(";
    }
    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void Win()
    {
        _enemy.IsClickable = false;
        _turnLabel.Text = "You Win!!!!";
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void AttackCell(Vector2I cell)
    {
        GD.Print($"Attack Cell {Multiplayer.GetUniqueId()}");
        if (Multiplayer.IsServer())
        {
            long id = Multiplayer.GetRemoteSenderId();
            GD.Print($"Attack Cell From {id} Active({_currentPlayer}) Inactive({NextTurn(_currentPlayer)})");
            if (id != _players[_currentPlayer].id)
            {
                GD.Print($"Attack Cell ID IS WRONG");
                return;
            }

            PlayerState state = InactivePlayer().state;
            if (state.GetCell(cell.X, cell.Y) == Cell.State.Friendly)
            {
                state.SetCell(cell.X, cell.Y, Cell.State.Hit);
                state.Health--;
                if (state.Health <= 0)
                {
                    _machine.Fire(Command.End);
                    return;
                }
            }
            else
            {
                state.SetCell(cell.X, cell.Y, Cell.State.Missed);
            }
            _machine.Fire(_turnTrigger, NextTurn(_currentPlayer));
        }
    }

    private int NextTurn(int turn)
    {
        return turn == 0 ? 1 : 0;
    }

    private Player ActivePlayer()
    {
        return _players[_currentPlayer];
    }

    private Player InactivePlayer()
    {
        return _players[NextTurn(_currentPlayer)];
    }
}
