using System;
using App.Application.GameLoop.Requests;
using App.Domain.GameLoop;
using MessagePipe;
using VContainer.Unity;

namespace App.Presentation.GameOverScreen
{
    public sealed class GameOverScreenPresenter : IInitializable, IDisposable
    {
        private readonly GameOverScreenView _view;
        private readonly IRequestHandler<StartNewGameRequest, StartNewGameResult> _startNewGameHandler;
        private readonly ISubscriber<GameOverEvent> _gameOverSub;
        private readonly DisposableBagBuilder _bag = DisposableBag.CreateBuilder();

        public GameOverScreenPresenter(
            GameOverScreenView view,
            IRequestHandler<StartNewGameRequest, StartNewGameResult> startNewGameHandler,
            ISubscriber<GameOverEvent> gameOverSub)
        {
            _view = view;
            _startNewGameHandler = startNewGameHandler;
            _gameOverSub = gameOverSub;
        }

        public void Initialize()
        {
            _view.BindRestartButton(() => _view.Hide(0.2F,
                () => _startNewGameHandler.Invoke(new StartNewGameRequest())));
            _gameOverSub.Subscribe(OnGameOver).AddTo(_bag);
        }

        public void Dispose() => _bag.Build().Dispose();

        private void OnGameOver(GameOverEvent e)
        {
            _view.DisplayResults(e.FinalScore, e.TopScores);
            _view.Show(0.4F);
        }
    }
}