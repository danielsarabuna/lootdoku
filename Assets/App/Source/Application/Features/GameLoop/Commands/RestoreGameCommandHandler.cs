using System.Collections.Generic;
using App.Application.GameLoop.Requests;
using App.Domain.Figures;
using App.Domain.GameLoop;
using App.Domain.Repositories;
using App.Domain.Score;
using MessagePipe;

namespace App.Application.GameLoop.Commands
{
    public sealed class RestoreGameCommandHandler : IRequestHandler<RestoreGameRequest, RestoreGameResult>
    {
        private readonly IGameSessionRepository _sessionRepository;
        private readonly IPublisher<ScoreUpdatedEvent> _scorePublisher;
        private readonly IPublisher<FiguresDealtEvent> _dealtPublisher;
        private readonly IPublisher<GameStateChangedEvent> _statePublisher;

        public RestoreGameCommandHandler(
            IGameSessionRepository sessionRepository,
            IPublisher<ScoreUpdatedEvent> scorePublisher,
            IPublisher<FiguresDealtEvent> dealtPublisher,
            IPublisher<GameStateChangedEvent> statePublisher)
        {
            _sessionRepository = sessionRepository;
            _scorePublisher = scorePublisher;
            _dealtPublisher = dealtPublisher;
            _statePublisher = statePublisher;
        }

        public RestoreGameResult Invoke(RestoreGameRequest request)
        {
            if (_sessionRepository is null || !_sessionRepository.HasActiveSave) return RestoreGameResult.Failed;

            var session = _sessionRepository.GetActiveSession();
            if (session is null || !_sessionRepository.HasActiveSave || session.CurrentState != GameState.Playing)
                return RestoreGameResult.Failed;

            session.PullDomainEvents();
            _scorePublisher.Publish(new ScoreUpdatedEvent(session.Score.CurrentScore, session.Score.BestScore));
            _dealtPublisher.Publish(new FiguresDealtEvent(new List<Figure>(session.Slots)));
            _statePublisher.Publish(new GameStateChangedEvent(session.CurrentState));

            return RestoreGameResult.Succeeded;
        }
    }
}