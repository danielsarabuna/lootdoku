using App.Application.Board.Requests;
using App.Domain.Repositories;
using MessagePipe;

namespace App.Application.Board.Queries
{
    public sealed class GetBoardGridQueryHandler : IRequestHandler<GetBoardGridRequest, BoardGridDto>
    {
        private readonly IGameSessionRepository _sessionRepository;

        public GetBoardGridQueryHandler(IGameSessionRepository sessionRepository)
        {
            _sessionRepository = sessionRepository;
        }

        public BoardGridDto Invoke(GetBoardGridRequest request)
        {
            var board = _sessionRepository.GetActiveSession()?.Board;
            if (board is null)
                return new(0, 0, new bool[0, 0]);

            var occupied = new bool[board.Width, board.Height];
            for (var x = 0; x < board.Width; x++)
            {
                for (var y = 0; y < board.Height; y++)
                {
                    occupied[x, y] = !board.IsEmpty(x, y);
                }
            }

            return new(board.Width, board.Height, occupied);
        }
    }
}