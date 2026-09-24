namespace App.Domain.Figures
{
    public sealed class Figure
    {
        public string Id { get; }
        public string Name { get; }
        public FigureShape Shape { get; }
        public int ColorIndex { get; }
        private RotationAngle Rotation { get; }

        public Figure(string id, string name, FigureShape shape, RotationAngle rotation = RotationAngle.Deg0,
            int colorIndex = 0)
        {
            Id = id;
            Name = name;
            Shape = shape;
            Rotation = rotation;
            ColorIndex = colorIndex;
        }

        public Figure RotateClockwise() => new(Id, Name, Shape.RotateClockwise(), Rotation.NextClockwise(), ColorIndex);

        public bool CanFitOnBoard(int boardWidth, int boardHeight) => Shape.CanFitOnBoard(boardWidth, boardHeight);
    }
}