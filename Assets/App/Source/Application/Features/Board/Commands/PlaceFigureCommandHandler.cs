using System;
using App.Application.Board.Requests;
using App.Application.Ports;
using App.Application.Score;
using App.Domain.Board;
using App.Domain.Figures;
using App.Domain.GameLoop;
using App.Domain.Repositories;
using App.Domain.Score;
using MessagePipe;

namespace App.Application.Board.Commands
{
    public sealed class PlaceFigureCommandHandler : IRequestHandler<PlaceFigureRequest, PlaceFigureResult>
    {
        private readonly IGameSessionRepository _sessionRepository;
        private readonly IGameConfigProvider _configProvider;
        private readonly IPlacementValidator _placementValidator;
        private readonly ILineClearRule _lineClearRule;
        private readonly IGameOverDetector _gameOverDetector;
        private readonly IAudioService _audioService;
        private readonly HighScoreService _highScoreService;

        private readonly IPublisher<FigurePlacedEvent> _placedPublisher;
        private readonly IPublisher<LinesClearedEvent> _clearedPublisher;
        private readonly IPublisher<FiguresDealtEvent> _dealtPublisher;
        private readonly IPublisher<ScoreUpdatedEvent> _scorePublisher;
        private readonly IPublisher<GameOverEvent> _gameOverPublisher;
        private readonly IPublisher<GameStateChangedEvent> _statePublisher;

        public PlaceFigureCommandHandler(
            IGameSessionRepository sessionRepository,
            IGameConfigProvider configProvider,
            IPlacementValidator placementValidator,
            ILineClearRule lineClearRule,
            IGameOverDetector gameOverDetector,
            IAudioService audioService,
            HighScoreService highScoreService,
            IPublisher<FigurePlacedEvent> placedPublisher,
            IPublisher<LinesClearedEvent> clearedPublisher,
            IPublisher<FiguresDealtEvent> dealtPublisher,
            IPublisher<ScoreUpdatedEvent> scorePublisher,
            IPublisher<GameOverEvent> gameOverPublisher,
            IPublisher<GameStateChangedEvent> statePublisher)
        {
            _sessionRepository = sessionRepository;
            _configProvider = configProvider;
            _placementValidator = placementValidator;
            _lineClearRule = lineClearRule;
            _gameOverDetector = gameOverDetector;
            _audioService = audioService;
            _highScoreService = highScoreService;
            _placedPublisher = placedPublisher;
            _clearedPublisher = clearedPublisher;
            _dealtPublisher = dealtPublisher;
            _scorePublisher = scorePublisher;
            _gameOverPublisher = gameOverPublisher;
            _statePublisher = statePublisher;
        }

        public PlaceFigureResult Invoke(PlaceFigureRequest request)
        {
            if (!SlotIndex.TryCreate(request.SlotIndex, out var slotIndex))
                return PlaceFigureResult.Failed;

            var session = _sessionRepository.GetActiveSession();
            var pointsPerCell = _configProvider.PointsPerCell;
            var figuresPool = _configProvider.AvailableFiguresPool;

            var success = session.PlaceFigure(
                slotIndex,
                request.Origin,
                _placementValidator,
                _lineClearRule,
                _gameOverDetector,
                pointsPerCell,
                figuresPool);

            if (!success)
                return PlaceFigureResult.Failed;

            if (session.IsGameOver)
                _highScoreService?.RecordScore(session.Score.CurrentScore);

            _sessionRepository.Save(session);

            var events = session.PullDomainEvents();
            var hasCleared = false;
            for (var i = 0; i < events.Count; i++)
            {
                var evt = events[i];
                switch (evt)
                {
                    case FigurePlacedEvent placed:
                        _placedPublisher.Publish(placed);
                        break;
                    case LinesClearedEvent cleared:
                        hasCleared = true;
                        _clearedPublisher.Publish(cleared);
                        break;
                    case FiguresDealtEvent dealt:
                        _dealtPublisher.Publish(dealt);
                        break;
                    case ScoreUpdatedEvent score:
                        _scorePublisher.Publish(score);
                        break;
                    case GameOverEvent:
                        var topScores = _highScoreService?.TopScores ?? Array.Empty<HighScoreEntry>();
                        _gameOverPublisher.Publish(new GameOverEvent(session.Score.CurrentScore, topScores));
                        break;
                    case GameStateChangedEvent state:
                        _statePublisher.Publish(state);
                        break;
                }
            }

            var sfxType = hasCleared ? AudioSfxType.DestroyCells : AudioSfxType.Place;
            _audioService.PlaySfx(sfxType);

            return PlaceFigureResult.Succeeded;
        }
    }
}