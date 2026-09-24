namespace App.Domain.Board
{
    public sealed class BoardGrid
    {
        private readonly CellState[,] _cells;
        private readonly int[,] _cellColors;

        public int Width { get; }
        public int Height { get; }

        public BoardGrid(int width, int height)
        {
            Width = width;
            Height = height;
            _cells = new CellState[width, height];
            _cellColors = new int[width, height];
        }

        private bool IsInside(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;

        private bool TryGetCell(int x, int y, out CellState state)
        {
            if (IsInside(x, y))
            {
                state = _cells[x, y];
                return true;
            }

            state = CellState.Empty;
            return false;
        }

        public bool TryGetCell(BoardCoordinate coord, out CellState state) => TryGetCell(coord.X, coord.Y, out state);

        private CellState GetCell(int x, int y) => TryGetCell(x, y, out var state) ? state : CellState.Empty;

        public int GetCellColor(int x, int y) => TryGetCellColor(x, y, out var color) ? color : 0;

        public bool IsEmpty(int x, int y) => GetCell(x, y) == CellState.Empty;

        public void SetCell(int x, int y, CellState state, int colorIndex = 0)
        {
            if (!IsInside(x, y)) return;
            _cells[x, y] = state;
            _cellColors[x, y] = state == CellState.Empty ? 0 : colorIndex;
        }

        public void SetCell(BoardCoordinate coord, CellState state, int colorIndex = 0) =>
            SetCell(coord.X, coord.Y, state, colorIndex);

        public void Clear()
        {
            for (var x = 0; x < Width; x++)
            {
                for (var y = 0; y < Height; y++)
                {
                    _cells[x, y] = CellState.Empty;
                    _cellColors[x, y] = 0;
                }
            }
        }

        private bool TryGetCellColor(int x, int y, out int colorIndex)
        {
            if (IsInside(x, y))
            {
                colorIndex = _cellColors[x, y];
                return true;
            }

            colorIndex = 0;
            return false;
        }
    }
}