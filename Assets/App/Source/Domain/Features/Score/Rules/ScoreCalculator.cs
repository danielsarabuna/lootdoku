using App.Domain.Board;

namespace App.Domain.Score
{
    public static class ScoreCalculator
    {
        public static int CalculateClearPoints(ClearResult clearResult, int pointsPerCell)
        {
            if (clearResult is null || !clearResult.HasCleared || pointsPerCell <= 0) return 0;

            return clearResult.ClearedCellCount * pointsPerCell;
        }
    }
}