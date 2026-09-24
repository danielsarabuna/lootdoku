using App.Domain.Figures;

namespace App.Domain.Board
{
    public interface IPlacementValidator
    {
        bool CanPlace(BoardGrid board, Figure figure, BoardCoordinate origin);
        bool TryPlace(BoardGrid board, Figure figure, BoardCoordinate origin);
    }

    public sealed class PlacementValidator : IPlacementValidator
    {
        public bool CanPlace(BoardGrid board, Figure figure, BoardCoordinate origin)
        {
            if (board is null || figure is null) return false;

            foreach (var offset in figure.Shape.Coordinates)
            {
                var target = origin + offset;
                if (!board.TryGetCell(target, out var state) || state != CellState.Empty) return false;
            }

            return true;
        }

        public bool TryPlace(BoardGrid board, Figure figure, BoardCoordinate origin)
        {
            if (!CanPlace(board, figure, origin)) return false;

            foreach (var offset in figure.Shape.Coordinates)
            {
                var target = origin + offset;
                board.SetCell(target, CellState.Occupied, figure.ColorIndex);
            }

            return true;
        }
    }
}