using System;
using System.Collections.Generic;

namespace App.Domain.Board
{
    public sealed class ClearResult
    {
        public IReadOnlyList<BoardCoordinate> ClearedCoordinates { get; }
        public int ClearedCellCount => ClearedCoordinates.Count;
        public bool HasCleared => ClearedCellCount > 0;
        public static ClearResult Empty => new(Array.Empty<BoardCoordinate>());

        public ClearResult(IReadOnlyList<BoardCoordinate> clearedCoordinates)
        {
            ClearedCoordinates = clearedCoordinates ?? Array.Empty<BoardCoordinate>();
        }
    }
}