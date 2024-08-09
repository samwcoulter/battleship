using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

public class PlayerState
{
    private List<Cell.State> _cells;
    private const int NUM_SHIPS = 2;

    public PlayerState()
    {
        _cells = new();
        for (int i = 0; i < Board.GRID_SIZE * Board.GRID_SIZE; i++)
        {
            _cells.Add(Cell.State.Empty);
        }

        System.Random rng = new();
        int placedShip = 0;
        while (placedShip < NUM_SHIPS)
        {
            int x = rng.Next(Board.GRID_SIZE);
            int y = rng.Next(Board.GRID_SIZE);
            List<List<(int, int)>> coords = new()
            {
                new List<(int, int)> {(x, y),(x+1,y),(x+2,y)},
                new List<(int, int)> {(x, y),(x-1,y),(x-2,y)},
                new List<(int, int)> {(x, y),(x,y+1),(x,y+2)},
                new List<(int, int)> {(x, y),(x,y-1),(x,y-2)},
            };

            coords = coords.OrderBy(_ => rng.Next()).ToList();

            foreach (List<(int, int)> l in coords)
            {
                bool conflict = false;
                foreach ((int, int) c in l)
                {
                    if (!(c.Item1 >= 0 && c.Item1 < Board.GRID_SIZE && c.Item2 >= 0 && c.Item2 < Board.GRID_SIZE && GetCell(c.Item1, c.Item2) == Cell.State.Empty))
                    {
                        conflict = true;
                        break;
                    }
                }
                if (conflict == false) 
                {
                    foreach ((int, int) c in l)
                    {
                        SetCell(c.Item1, c.Item2, Cell.State.Friendly);
                        Health++;
                    }
                    placedShip++;
                    break;
                }
            }
        }
    }

    public int Health { get; set; } = 0;

    public Cell.State GetCell(int x, int y)
    {
        return _cells[x*Board.GRID_SIZE+y];
    }
    
    public void SetCell(int x, int y, Cell.State state)
    {
        _cells[x*Board.GRID_SIZE+y] = state;
    }

    public List<Cell.State> All()
    {
        return _cells;
    }

    public string Serialize()
    {
        return System.Text.Json.JsonSerializer.Serialize(_cells);
    }

    public string SerializeSecret()
    {
        List<Cell.State> secretCells = JsonSerializer.Deserialize<List<Cell.State>>(JsonSerializer.Serialize(_cells));
        for (int i = 0; i < secretCells.Count(); i++)
        {
            if (secretCells[i] == Cell.State.Friendly)
            {
                secretCells[i] = Cell.State.Empty;
            }
        }
        return System.Text.Json.JsonSerializer.Serialize(secretCells);
    }

    public void Deserialize(string data)
    {
        _cells = System.Text.Json.JsonSerializer.Deserialize<List<Cell.State>>(data);
    }
}
