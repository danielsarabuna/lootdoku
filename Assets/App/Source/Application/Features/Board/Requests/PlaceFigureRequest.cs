using App.Domain.Board;

namespace App.Application.Board.Requests
{
    public readonly struct PlaceFigureRequest
    {
        public int SlotIndex { get; }
        public BoardCoordinate Origin { get; }

        public PlaceFigureRequest(int slotIndex, BoardCoordinate origin)
        {
            SlotIndex = slotIndex;
            Origin = origin;
        }
    }

    public readonly struct PlaceFigureResult
    {
        public bool Success { get; }
        public static PlaceFigureResult Succeeded => new(true);
        public static PlaceFigureResult Failed => new(false);

        private PlaceFigureResult(bool success)
        {
            Success = success;
        }
    }
}