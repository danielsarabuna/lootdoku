using System;
using System.Threading;
using App.Application.GameLoop.Requests;
using App.Application.Ports;
using App.Application.Score;
using App.Domain.GameLoop;
using App.Domain.Score;
using Cysharp.Threading.Tasks;
using MessagePipe;

namespace App.Application.GameLoop.Commands
{
    public sealed class InitializeGameCommandHandler : IAsyncRequestHandler<InitializeGameRequest, InitializeGameResult>
    {
        private readonly HighScoreService _highScoreService;
        private readonly ScoreModel _scoreModel;
        private readonly IAudioService _audioService;
        private readonly IResourceService _resourceService;
        private readonly IPublisher<ScoreUpdatedEvent> _scorePublisher;
        private readonly IPublisher<GameStateChangedEvent> _statePublisher;
        private readonly IGameConfigProvider _configProvider;

        public InitializeGameCommandHandler(
            HighScoreService highScoreService,
            ScoreModel scoreModel,
            IAudioService audioService,
            IPublisher<ScoreUpdatedEvent> scorePublisher,
            IPublisher<GameStateChangedEvent> statePublisher,
            IResourceService resourceService,
            IGameConfigProvider configProvider)
        {
            _highScoreService = highScoreService;
            _scoreModel = scoreModel;
            _audioService = audioService;
            _resourceService = resourceService;
            _configProvider = configProvider;
            _scorePublisher = scorePublisher;
            _statePublisher = statePublisher;
        }

        public async UniTask<InitializeGameResult> InvokeAsync(InitializeGameRequest request,
            CancellationToken cancellationToken = default)
        {
            var progress = request.Progress;
            _statePublisher.Publish(new GameStateChangedEvent(GameState.Initializing));
            progress?.Report("Loading assets and resources...");

            await _resourceService.InitializeAsync(cancellationToken);
            progress?.Report("Loading configurations...");

            await _configProvider.InitializeAsync(cancellationToken);
            if (_configProvider.AvailableFiguresPool.Count == 0)
                throw new InvalidOperationException("No playable figures were loaded.");
            progress?.Report("Initializing audio...");

            await _audioService.InitializeAsync(cancellationToken);
            progress?.Report("Loading high scores...");

            await _highScoreService.LoadTopScoresAsync(cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            _scoreModel.SetBestScore(_highScoreService.BestScore);

            _scorePublisher.Publish(new ScoreUpdatedEvent(_scoreModel.CurrentScore, _scoreModel.BestScore));
            _audioService.PlayMusic();

            progress?.Report("Ready!");
            return InitializeGameResult.Succeeded;
        }
    }
}