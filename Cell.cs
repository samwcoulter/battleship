using Godot;

public partial class Cell : Button
{
    public enum State
    {
        Empty,
        Friendly,
        Hit,
        Missed,
    }

    private readonly Texture2D GreySquare = (Texture2D)ResourceLoader.Load("res://assets/element_grey_square.png");
    private readonly Texture2D GreenSquare = (Texture2D)ResourceLoader.Load("res://assets/element_green_square.png");
    private readonly Texture2D BlueSquare = (Texture2D)ResourceLoader.Load("res://assets/element_blue_square.png");
    private readonly Texture2D RedSquare = (Texture2D)ResourceLoader.Load("res://assets/element_red_square.png");
    private readonly Texture2D YellowSquare = (Texture2D)ResourceLoader.Load("res://assets/element_yellow_square.png");

    private State _state;
    private TextureRect _tex;

    [Export]
    public State Status {
        get => _state;
        set {
            _state = value;
            if (_state == State.Empty) Icon = GreySquare;
            else if (_state == State.Friendly) Icon = BlueSquare;
            else if (_state == State.Hit) Icon = RedSquare;
            else if (_state == State.Missed) Icon = YellowSquare;
        }
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Status = State.Empty;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    public void OnHit()
    {
        Status = State.Hit;
    }

    public void OnMiss()
    {
        Status = State.Missed;
    }
}
