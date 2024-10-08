using Godot;
using System.Collections.Generic;

public partial class Board : GridContainer
{
    public const int GRID_SIZE = 5;

    private List<Cell> _cells = new();

    [Export]
    public bool IsClickable { get; set; } = true;

    
    [Signal]
    public delegate void OnClickEventHandler(int x, int y);

    public override void _Ready()
    {
        Columns = GRID_SIZE;
        for (int column = 0; column < GRID_SIZE; column++)
        {
            for (int row = 0; row < GRID_SIZE; row++)
            {
                Cell cell = new();
                int x = column;
                int y = row;
                cell.Pressed += () => this.OnCellPressed(x, y);
                // cell.Size = new Vector2(25.0f, 25.0f);
                AddChild(cell);
                _cells.Add(cell);
            }
        }
    }

    public override void _Process(double delta)
    {
    }

    public void ApplyState()
    {
    }

    public void OnCellPressed(int x, int y)
    {
        if (IsClickable) {
            // GetCell(x,y).Status = Cell.State.Hit;
            EmitSignal(SignalName.OnClick, x, y);
            GetCell(x,y);
        }
    }

    public Cell GetCell(int x, int y)
    {
        return _cells[x*GRID_SIZE+y];
    }

    public void SetState(PlayerState state)
    {
        List<Cell.State> l = state.All();
        for (int i = 0; i < l.Count; i++ )
        {
            _cells[i].Status = l[i];
            if (_cells[i].Status != Cell.State.Empty)
            {
                _cells[i].Disabled = true;
            }
        }
    }
}
