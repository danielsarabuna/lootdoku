using System;
using System.Collections.Generic;
using App.Application.GameLoop.Requests;
using App.Application.Ports;
using App.Domain.Figures;
using App.Domain.GameLoop;
using App.Domain.Repositories;
using App.Domain.Score;
using MessagePipe;

namespace App.Application.GameLoop.Commands
{
    public sealed class StartNewGameCommandHandler : IRequestHandler<StartNewGameRequest, StartNewGameResult>
    {
        private readonly IGameSessionRepository _sessionRepository;
        private readonly IGameConfigProvider _configProvider;
        private readonly IPublisher<FiguresDealtEvent> _dealtPublisher;
        private readonly IPublisher<ScoreUpdatedEvent> _scorePublisher;
        private readonly IPublisher<GameStateChangedEvent> _statePublisher;

        public StartNewGameCommandHandler(
            IGameSessionRepository sessionRepository,
            IGameConfigProvider configProvider,
            IPublisher<FiguresDealtEvent> dealtPublisher,
            IPublisher<ScoreUpdatedEvent> scorePublisher,
            IPublisher<GameStateChangedEvent> statePublisher)
        {
            _sessionRepository = sessionRepository;
            _configProvider = configProvider;
            _dealtPublisher = dealtPublisher;
            _scorePublisher = scorePublisher;
            _statePublisher = statePublisher;
        }

        public StartNewGameResult Invoke(StartNewGameRequest request)
        {
            var gameSeed = new Random().Next(1, int.MaxValue);

            var session = _sessionRepository.GetActiveSession();
            var width = _configProvider.BoardWidth;
            var height = _configProvider.BoardHeight;
            session.StartNewGame(gameSeed, _configProvider.AvailableFiguresPool, width, height);

            _sessionRepository.Save(session);
            session.PullDomainEvents();

            _dealtPublisher.Publish(new FiguresDealtEvent(new List<Figure>(session.Slots)));
            _scorePublisher.Publish(new ScoreUpdatedEvent(session.Score.CurrentScore, session.Score.BestScore));
            _statePublisher.Publish(new GameStateChangedEvent(GameState.Playing));

            return new StartNewGameResult();
        }
    }
}