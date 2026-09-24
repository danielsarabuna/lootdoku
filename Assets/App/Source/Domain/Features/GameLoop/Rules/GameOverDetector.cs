using System.Collections.Generic;
using App.Domain.Board;
using App.Domain.Figures;

namespace App.Domain.GameLoop
{
    public interface IGameOverDetector
    {
        bool IsGameOver(BoardGrid board, IReadOnlyList<Figure> availableFigures, bool allowRotations = true);
    }

    public sealed class GameOverDetector : IGameOverDetector
    {
        private readonly IPlacementValidator _placementValidator;

        public GameOverDetector(IPlacementValidator placementValidator) => _placementValidator = placementValidator;

        public bool IsGameOver(BoardGrid board, IReadOnlyList<Figure> availableFigures, bool allowRotations = true)
        {
            if (board is null || availableFigures is null || availableFigures.Count == 0) return false;

            foreach (var figure in availableFigures)
            {
                if (figure is null) continue;
                if (CanFigureFitAnywhere(board, figure, allowRotations)) return false;
            }

            return true;
        }

        private bool CanFigureFitAnywhere(BoardGrid board, Figure figure, bool allowRotations)
        {
            var rotationCount = allowRotations ? 4 : 1;
            var current = figure;

            for (var r = 0; r < rotationCount; r++)
            {
                for (var x = 0; x < board.Width; x++)
                {
                    for (var y = 0; y < board.Height; y++)
                    {
                        var coord = new BoardCoordinate(x, y);
                        if (_placementValidator.CanPlace(board, current, coord)) return true;
                    }
                }

                current = current.RotateClockwise();
            }

            return false;
        }
    }
}