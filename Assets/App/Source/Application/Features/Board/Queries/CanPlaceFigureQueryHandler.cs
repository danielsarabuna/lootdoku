using App.Application.Board.Requests;
using App.Domain.Board;
using App.Domain.Repositories;
using MessagePipe;

namespace App.Application.Board.Queries
{
    public sealed class CanPlaceFigureQueryHandler : IRequestHandler<CanPlaceFigureRequest, CanPlaceFigureResult>
    {
        private readonly IPlacementValidator _placementValidator;
        private readonly IGameSessionRepository _sessionRepository;

        public CanPlaceFigureQueryHandler(
            IPlacementValidator placementValidator,
            IGameSessionRepository sessionRepository)
        {
            _placementValidator = placementValidator;
            _sessionRepository = sessionRepository;
        }

        public CanPlaceFigureResult Invoke(CanPlaceFigureRequest request)
        {
            if (request.Figure is null) return CanPlaceFigureResult.Denied;

            var board = _sessionRepository.GetActiveSession()?.Board;
            if (board is null) return CanPlaceFigureResult.Denied;

            return _placementValidator.CanPlace(board, request.Figure, request.Origin)
                ? CanPlaceFigureResult.Allowed
                : CanPlaceFigureResult.Denied;
        }
    }
}