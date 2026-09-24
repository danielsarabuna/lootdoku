using System.Collections.Generic;

namespace App.Domain.Board
{
    public interface ILineClearRule
    {
        ClearResult CheckAndClear(BoardGrid board);
    }

    public sealed class LineClearRule : ILineClearRule
    {
        public ClearResult CheckAndClear(BoardGrid board)
        {
            if (board is null) return ClearResult.Empty;

            var clearedCoordinates = new HashSet<BoardCoordinate>();

            for (var y = 0; y < board.Height; y++)
            {
                var fullRow = true;
                for (var x = 0; x < board.Width; x++)
                    if (board.IsEmpty(x, y))
                    {
                        fullRow = false;
                        break;
                    }

                if (fullRow)
                {
                    for (var x = 0; x < board.Width; x++)
                        clearedCoordinates.Add(new BoardCoordinate(x, y));
                }
            }

            for (var x = 0; x < board.Width; x++)
            {
                var fullCol = true;
                for (var y = 0; y < board.Height; y++)
                    if (board.IsEmpty(x, y))
                    {
                        fullCol = false;
                        break;
                    }

                if (fullCol)
                {
                    for (var y = 0; y < board.Height; y++)
                        clearedCoordinates.Add(new BoardCoordinate(x, y));
                }
            }

            foreach (var coord in clearedCoordinates)
                board.SetCell(coord, CellState.Empty);

            return new ClearResult(new List<BoardCoordinate>(clearedCoordinates));
        }
    }
}