using System.Collections.Generic;

public class PlayerState
{
    private List<Cell.State> _cells;

    public PlayerState()
    {
        _cells = new();
        for (int i = 0; i < Board.GRID_SIZE * Board.GRID_SIZE; i++)
        {
            _cells.Add(Cell.State.Empty);
        }

        System.Random rng = new();
        while (true)
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

            bool conflict = false;
            foreach (List<(int, int)> l in coords)
            {
                foreach ((int, int) c in l)
                {
                    if (c.Item1 >= 0 && c.Item1 < Board.GRID_SIZE && c.Item2 >= 0 && c.Item2 < Board.GRID_SIZE)
                    {

                    }
                    else {
                        conflict = true;
                    }
                }
            }
        }
    }

    public Cell.State GetCell(int x, int y)
    {
        return _cells[x*Board.GRID_SIZE+y];
    }
}
