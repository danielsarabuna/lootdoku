using System;
using App.Application.GameLoop.Requests;
using App.Application.Ports;
using App.Domain.GameLoop;
using MessagePipe;
using VContainer.Unity;

namespace App.Presentation.PauseScreen
{
    public sealed class PauseScreenPresenter : IInitializable, IDisposable
    {
        private readonly PauseScreenView _view;
        private readonly IRequestHandler<TogglePauseRequest, TogglePauseResult> _togglePauseHandler;
        private readonly IRequestHandler<StartNewGameRequest, StartNewGameResult> _startNewGameHandler;
        private readonly IInputService _inputService;
        private readonly ISubscriber<GameStateChangedEvent> _stateSub;
        private readonly DisposableBagBuilder _bag = DisposableBag.CreateBuilder();

        public PauseScreenPresenter(
            PauseScreenView view,
            IRequestHandler<TogglePauseRequest, TogglePauseResult> togglePauseHandler,
            IRequestHandler<StartNewGameRequest, StartNewGameResult> startNewGameHandler,
            IInputService inputService,
            ISubscriber<GameStateChangedEvent> stateSub)
        {
            _view = view;
            _togglePauseHandler = togglePauseHandler;
            _startNewGameHandler = startNewGameHandler;
            _inputService = inputService;
            _stateSub = stateSub;
        }

        public void Initialize()
        {
            _view.BindButtons(
                onResume: () => _togglePauseHandler.Invoke(new TogglePauseRequest()),
                onRestart: () =>
                {
                    _view.Hide(0.2F, () =>
                    {
                        _togglePauseHandler.Invoke(new TogglePauseRequest());
                        _startNewGameHandler.Invoke(new StartNewGameRequest());
                    });
                });

            _inputService.OnPauseHotkeyTriggered += HandlePauseHotkey;

            _stateSub.Subscribe(OnGameStateChanged).AddTo(_bag);
        }

        public void Dispose()
        {
            _inputService.OnPauseHotkeyTriggered -= HandlePauseHotkey;

            _bag.Build().Dispose();
        }

        private void HandlePauseHotkey() => _togglePauseHandler.Invoke(new TogglePauseRequest());

        private void OnGameStateChanged(GameStateChangedEvent e)
        {
            switch (e.NewState)
            {
                case GameState.Paused:
                    _view.Show();
                    break;
                case GameState.Playing:
                    _view.Hide();
                    break;
            }
        }
    }
}