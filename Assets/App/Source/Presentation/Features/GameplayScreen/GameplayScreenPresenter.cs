using System;
using App.Application.Board.Requests;
using App.Application.Figures.Requests;
using App.Application.GameLoop.Requests;
using App.Application.Ports;
using App.Domain.Board;
using App.Domain.Figures;
using App.Domain.GameLoop;
using App.Domain.Score;
using MessagePipe;
using VContainer.Unity;

namespace App.Presentation.GameplayScreen
{
    public sealed class GameplayScreenPresenter : IInitializable, IDisposable
    {
        private readonly GameplayScreenView _view;
        private readonly IRequestHandler<CanPlaceFigureRequest, CanPlaceFigureResult> _canPlaceHandler;
        private readonly IRequestHandler<PlaceFigureRequest, PlaceFigureResult> _placeFigureHandler;
        private readonly IRequestHandler<RotateFigureRequest, RotateFigureResult> _rotateFigureHandler;
        private readonly IRequestHandler<TogglePauseRequest, TogglePauseResult> _togglePauseHandler;
        private readonly IRequestHandler<GetBoardGridRequest, BoardGridDto> _getBoardGridHandler;
        private readonly IGameConfigProvider _configProvider;
        private readonly IAudioService _audioService;
        private readonly IInputService _inputService;

        private readonly ISubscriber<FigurePlacedEvent> _placedSub;
        private readonly ISubscriber<LinesClearedEvent> _clearedSub;
        private readonly ISubscriber<FiguresDealtEvent> _dealtSub;
        private readonly ISubscriber<FigureRotatedEvent> _rotatedSub;
        private readonly ISubscriber<ScoreUpdatedEvent> _scoreSub;
        private readonly ISubscriber<GameStateChangedEvent> _stateSub;

        private readonly DisposableBagBuilder _bag = DisposableBag.CreateBuilder();

        public GameplayScreenPresenter(
            GameplayScreenView view,
            IRequestHandler<CanPlaceFigureRequest, CanPlaceFigureResult> canPlaceHandler,
            IRequestHandler<PlaceFigureRequest, PlaceFigureResult> placeFigureHandler,
            IRequestHandler<RotateFigureRequest, RotateFigureResult> rotateFigureHandler,
            IRequestHandler<TogglePauseRequest, TogglePauseResult> togglePauseHandler,
            IRequestHandler<GetBoardGridRequest, BoardGridDto> getBoardGridHandler,
            IGameConfigProvider configProvider,
            IAudioService audioService,
            IInputService inputService,
            ISubscriber<FigurePlacedEvent> placedSub,
            ISubscriber<LinesClearedEvent> clearedSub,
            ISubscriber<FiguresDealtEvent> dealtSub,
            ISubscriber<FigureRotatedEvent> rotatedSub,
            ISubscriber<ScoreUpdatedEvent> scoreSub,
            ISubscriber<GameStateChangedEvent> stateSub)
        {
            _view = view;
            _canPlaceHandler = canPlaceHandler;
            _placeFigureHandler = placeFigureHandler;
            _rotateFigureHandler = rotateFigureHandler;
            _togglePauseHandler = togglePauseHandler;
            _getBoardGridHandler = getBoardGridHandler;
            _configProvider = configProvider;
            _audioService = audioService;
            _inputService = inputService;

            _placedSub = placedSub;
            _clearedSub = clearedSub;
            _dealtSub = dealtSub;
            _rotatedSub = rotatedSub;
            _scoreSub = scoreSub;
            _stateSub = stateSub;
        }

        public void Initialize()
        {
            var boardGrid = _getBoardGridHandler.Invoke(new GetBoardGridRequest());
            var initWidth = boardGrid.Width > 0 ? boardGrid.Width : _configProvider.BoardWidth;
            var initHeight = boardGrid.Height > 0 ? boardGrid.Height : _configProvider.BoardHeight;
            _view.BoardView.Initialize(initWidth, initHeight);

            _view.FigureTrayView.Initialize(
                _view.BoardView,
                CanPlaceFigure,
                HandleFigureDropped,
                HandleRotateRequested,
                () => _audioService.PlaySfx(AudioSfxType.Take));

            _view.TopBarView.Bind(() => _togglePauseHandler.Invoke(new TogglePauseRequest()));

            _inputService.OnRotateHotkeyTriggered += HandleRotateHotkey;

            _placedSub.Subscribe(OnFigurePlaced).AddTo(_bag);
            _clearedSub.Subscribe(OnLinesCleared).AddTo(_bag);
            _dealtSub.Subscribe(OnFiguresDealt).AddTo(_bag);
            _rotatedSub.Subscribe(OnFigureRotated).AddTo(_bag);
            _scoreSub.Subscribe(OnScoreUpdated).AddTo(_bag);
            _stateSub.Subscribe(OnGameStateChanged).AddTo(_bag);
        }

        public void Dispose()
        {
            _inputService.OnRotateHotkeyTriggered -= HandleRotateHotkey;
            _bag.Build().Dispose();
        }

        private bool HandleFigureDropped(int slotIndex, BoardCoordinate origin) =>
            _placeFigureHandler.Invoke(new PlaceFigureRequest(slotIndex, origin)).Success;

        private void HandleRotateRequested(int slotIndex) =>
            _rotateFigureHandler.Invoke(new RotateFigureRequest(slotIndex));

        private void HandleRotateHotkey() => _view.FigureTrayView.RotateActiveOrLastSlot();

        private bool CanPlaceFigure(Figure figure, BoardCoordinate origin) =>
            _canPlaceHandler.Invoke(new CanPlaceFigureRequest(figure, origin)).CanPlace;

        private void OnFigurePlaced(FigurePlacedEvent e)
        {
            if (_view?.BoardView != null)
                foreach (var offset in e.Figure.Shape.Coordinates)
                    _view.BoardView.SetCellOccupied(e.Coordinate + offset);

            if (_view?.FigureTrayView != null)
                _view.FigureTrayView.ClearSlot(e.SlotIndex);
        }

        private void OnLinesCleared(LinesClearedEvent e) =>
            _view?.BoardView?.PlayClearAnimation(e.Result.ClearedCoordinates);

        private void OnFiguresDealt(FiguresDealtEvent e) => _view?.FigureTrayView?.PopulateTray(e.Figures);

        private void OnFigureRotated(FigureRotatedEvent e) =>
            _view?.FigureTrayView?.UpdateFigureAtSlot(e.SlotIndex, e.Figure);

        private void OnScoreUpdated(ScoreUpdatedEvent e) =>
            _view?.TopBarView?.ScoreView?.SetScore(e.CurrentScore, e.BestScore);

        private void OnGameStateChanged(GameStateChangedEvent e)
        {
            switch (e.NewState)
            {
                case GameState.Playing:
                    SyncBoardViewWithGrid();
                    _view?.Show();
                    break;
                case GameState.Paused:
                    _view?.FigureTrayView?.CancelActiveDrag();
                    _view?.BoardView?.ClearPreview();
                    break;
            }
        }

        private void SyncBoardViewWithGrid()
        {
            if (_view?.BoardView == null) return;

            var boardGrid = _getBoardGridHandler.Invoke(new GetBoardGridRequest());
            if (boardGrid.Width <= 0 || boardGrid.Height <= 0) return;

            if (_view.BoardView.Width != boardGrid.Width || _view.BoardView.Height != boardGrid.Height)
                _view.BoardView.Initialize(boardGrid.Width, boardGrid.Height);

            for (var x = 0; x < boardGrid.Width; x++)
            {
                for (var y = 0; y < boardGrid.Height; y++)
                {
                    var cell = _view.BoardView.GetCell(x, y);
                    if (cell is null) continue;

                    if (boardGrid.IsEmpty(x, y))
                    {
                        if (cell.IsOccupied)
                            cell.SetEmpty();
                    }
                    else cell.SetOccupied(animate: false);
                }
            }
        }
    }
}