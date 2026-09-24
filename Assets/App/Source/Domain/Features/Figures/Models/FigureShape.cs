using System;
using System.Collections.Generic;
using System.Linq;
using App.Domain.Board;

namespace App.Domain.Figures
{
    public sealed class FigureShape
    {
        private const int MaxDimension = 5;

        public IReadOnlyList<BoardCoordinate> Coordinates { get; }
        public int BoundingWidth { get; }
        public int BoundingHeight { get; }
        public int CellCount => Coordinates.Count;

        public FigureShape(IEnumerable<BoardCoordinate> coordinates)
        {
            if (coordinates is null) throw new ArgumentNullException(nameof(coordinates));

            var rawList = coordinates.ToList();

            if (rawList.Count == 0)
            {
                Coordinates = Array.Empty<BoardCoordinate>();
                BoundingWidth = 0;
                BoundingHeight = 0;
                return;
            }

            var minX = rawList.Min(c => c.X);
            var maxX = rawList.Max(c => c.X);
            var minY = rawList.Min(c => c.Y);
            var maxY = rawList.Max(c => c.Y);

            var coords = new List<BoardCoordinate>();
            foreach (var c in rawList)
            {
                var normX = c.X - minX;
                var normY = c.Y - minY;
                if (normX is >= 0 and < MaxDimension && normY is >= 0 and < MaxDimension)
                    coords.Add(new BoardCoordinate(normX, normY));
            }

            Coordinates = coords;
            BoundingWidth = maxX - minX + 1;
            BoundingHeight = maxY - minY + 1;
        }

        public FigureShape RotateClockwise()
        {
            var rotated = new List<BoardCoordinate>();
            foreach (var c in Coordinates)
            {
                var newX = BoundingHeight - 1 - c.Y;
                var newY = c.X;
                rotated.Add(new BoardCoordinate(newX, newY));
            }

            return new FigureShape(rotated);
        }

        public bool CanFitOnBoard(int boardWidth, int boardHeight)
        {
            if (boardWidth <= 0 || boardHeight <= 0) return false;
            var fitsNormal = BoundingWidth <= boardWidth && BoundingHeight <= boardHeight;
            var fitsRotated = BoundingHeight <= boardWidth && BoundingWidth <= boardHeight;
            return fitsNormal || fitsRotated;
        }
    }
}