using App.Domain.Board;
using App.Domain.Figures;

namespace App.Application.Board.Requests
{
    public readonly struct CanPlaceFigureRequest
    {
        public Figure Figure { get; }
        public BoardCoordinate Origin { get; }

        public CanPlaceFigureRequest(Figure figure, BoardCoordinate origin)
        {
            Figure = figure;
            Origin = origin;
        }
    }

    public readonly struct CanPlaceFigureResult
    {
        public bool CanPlace { get; }
        public static CanPlaceFigureResult Allowed => new(true);
        public static CanPlaceFigureResult Denied => new(false);

        private CanPlaceFigureResult(bool canPlace)
        {
            CanPlace = canPlace;
        }
    }
}