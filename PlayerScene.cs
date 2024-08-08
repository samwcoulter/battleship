using Godot;

public partial class PlayerScene : VBoxContainer
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

        public Board Self()
        {
            return GetNode<Board>("Self");
        }

        public Board Enemy()
        {
            return GetNode<Board>("Enemy");
        }
}
