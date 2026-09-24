namespace App.Application.Board.Requests
{
    public readonly struct GetBoardGridRequest
    {
    }

    public readonly struct BoardGridDto
    {
        private readonly bool[,] _occupiedMatrix;

        public int Width { get; }
        public int Height { get; }

        public BoardGridDto(int width, int height, bool[,] occupiedMatrix)
        {
            Width = width;
            Height = height;
            _occupiedMatrix = occupiedMatrix;
        }

        public bool IsEmpty(int x, int y)
        {
            if (_occupiedMatrix == null || x < 0 || y < 0 || x >= Width || y >= Height) return true;
            return !_occupiedMatrix[x, y];
        }
    }
}