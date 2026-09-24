using System;
using System.Threading;
using App.Application.GameLoop.Requests;
using App.Application.Ports;
using Cysharp.Threading.Tasks;
using MessagePipe;
using VContainer.Unity;

namespace App.Presentation.LoadingScreen
{
    public sealed class LoadingScreenPresenter : IAsyncStartable
    {
        private readonly LoadingScreenView _view;
        private readonly IAsyncRequestHandler<InitializeGameRequest, InitializeGameResult> _initializeHandler;
        private readonly IRequestHandler<StartNewGameRequest, StartNewGameResult> _startNewGameHandler;
        private readonly IRequestHandler<RestoreGameRequest, RestoreGameResult> _restoreGameHandler;
        private readonly ILogService _logger;

        public LoadingScreenPresenter(
            LoadingScreenView view,
            IAsyncRequestHandler<InitializeGameRequest, InitializeGameResult> initializeHandler,
            IRequestHandler<StartNewGameRequest, StartNewGameResult> startNewGameHandler,
            IRequestHandler<RestoreGameRequest, RestoreGameResult> restoreGameHandler,
            ILogService logger)
        {
            _view = view;
            _initializeHandler = initializeHandler;
            _startNewGameHandler = startNewGameHandler;
            _restoreGameHandler = restoreGameHandler;
            _logger = logger;
        }

        public async UniTask StartAsync(CancellationToken cancellation)
        {
            if (_view == null || cancellation.IsCancellationRequested) return;

            _view.ShowInstant();
            _view.SetProgress("Initializing...");

            try
            {
                _logger?.Log("Loading game...");
                cancellation.ThrowIfCancellationRequested();
                var progressReporter = Progress.Create<string>(_view.SetProgress);

                var initResult = await _initializeHandler.InvokeAsync(new InitializeGameRequest(progressReporter),
                    cancellation);

                cancellation.ThrowIfCancellationRequested();
                if (!initResult.Success)
                    throw new InvalidOperationException("Game initialization reported failure.");

                _view.SetProgress("Ready!");
                await UniTask.Delay(TimeSpan.FromSeconds(1F), cancellationToken: cancellation);
                cancellation.ThrowIfCancellationRequested();

                var restored = _restoreGameHandler.Invoke(new RestoreGameRequest()).Restored;
                if (!restored)
                    _startNewGameHandler.Invoke(new StartNewGameRequest());

                _view.HideTwoStep();
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Could not start game: {ex}");
                if (!cancellation.IsCancellationRequested && _view != null)
                    _view.SetProgress("Unable to load the game. Please restart.");
            }
        }
    }
}