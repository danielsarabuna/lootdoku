using System;
using System.Collections.Generic;
using App.Domain.Board;
using App.Domain.Common;
using App.Domain.Figures;
using App.Domain.Repositories;
using App.Domain.Score;

namespace App.Domain.GameLoop
{
    public sealed class GameSession
    {
        private const int TrayCapacity = SlotIndex.maxCapacity;

        private readonly Figure[] _slots = new Figure[TrayCapacity];
        private readonly List<IDomainEvent> _domainEvents = new();
        private readonly int[] _slotRotations = new int[TrayCapacity];
        private int _seed;
        private int _dealStep;
        private bool _hasDealtCurrentStep;

        public BoardGrid Board { get; private set; }
        public ScoreModel Score { get; }
        public GameState CurrentState { get; private set; }

        public IReadOnlyList<Figure> Slots => _slots;

        public bool IsGameOver => CurrentState == GameState.GameOver;

        private bool IsHandEmpty
        {
            get
            {
                for (var i = 0; i < TrayCapacity; i++)
                    if (_slots[i] is not null)
                        return false;
                return true;
            }
        }

        private IReadOnlyList<Figure> RemainingFigures
        {
            get
            {
                var remaining = new List<Figure>();
                for (var i = 0; i < TrayCapacity; i++)
                    if (_slots[i] is not null)
                        remaining.Add(_slots[i]);
                return remaining;
            }
        }

        public GameSession(
            BoardGrid board,
            ScoreModel score,
            GameState initialState = GameState.Playing,
            int initialSeed = 12345)
        {
            Board = board;
            Score = score;
            CurrentState = initialState;
            _seed = initialSeed;
            _dealStep = 0;
            _hasDealtCurrentStep = false;
        }

        public static GameSession CreateNew(int boardWidth, int boardHeight, int seed = 12345)
        {
            var board = new BoardGrid(boardWidth, boardHeight);
            return new GameSession(board, new ScoreModel(), GameState.Playing, seed);
        }

        public void StartNewGame(int seed, IReadOnlyList<Figure> pool, int? boardWidth = null, int? boardHeight = null)
        {
            _seed = seed;
            _dealStep = 0;
            _hasDealtCurrentStep = false;

            if (boardWidth.HasValue && boardHeight.HasValue && boardWidth.Value > 0 && boardHeight.Value > 0 &&
                (Board.Width != boardWidth.Value || Board.Height != boardHeight.Value))
                Board = new BoardGrid(boardWidth.Value, boardHeight.Value);
            else
                Board.Clear();

            Score.Reset();
            Array.Clear(_slotRotations, 0, TrayCapacity);
            CurrentState = GameState.Playing;

            AddDomainEvent(new GameStateChangedEvent(GameState.Playing));
            AddDomainEvent(new ScoreUpdatedEvent(Score.CurrentScore, Score.BestScore));

            if (pool is not null && pool.Count > 0)
                DealNewFigures(pool);
        }

        private bool PlaceFigure(
            Figure figure,
            int slotIndex,
            BoardCoordinate origin,
            IPlacementValidator validator,
            ILineClearRule clearRule,
            int pointsPerCell)
        {
            if (figure is null) return false;
            if (CurrentState == GameState.GameOver || CurrentState == GameState.Paused) return false;
            if (!validator.TryPlace(Board, figure, origin)) return false;

            if (CurrentState == GameState.Initializing)
                CurrentState = GameState.Playing;

            AddDomainEvent(new FigurePlacedEvent(slotIndex, figure, origin));

            var clearResult = clearRule.CheckAndClear(Board);

            if (clearResult.HasCleared)
            {
                var clearPoints = ScoreCalculator.CalculateClearPoints(clearResult, pointsPerCell);
                Score.AddPoints(clearPoints);
                AddDomainEvent(new LinesClearedEvent(clearResult));
            }

            AddDomainEvent(new ScoreUpdatedEvent(Score.CurrentScore, Score.BestScore));

            return true;
        }

        public bool PlaceFigure(
            SlotIndex slotIndex,
            BoardCoordinate origin,
            IPlacementValidator validator,
            ILineClearRule clearRule,
            IGameOverDetector gameOverDetector,
            int pointsPerCell,
            IReadOnlyList<Figure> figurePool)
        {
            var figure = _slots[slotIndex.Value];
            if (figure is null) return false;

            var success = PlaceFigure(
                figure,
                slotIndex.Value,
                origin,
                validator,
                clearRule,
                pointsPerCell);

            if (!success) return false;

            _slots[slotIndex.Value] = null;
            _slotRotations[slotIndex.Value] = 0;

            if (IsHandEmpty && figurePool is not null && figurePool.Count > 0)
                DealNewFigures(figurePool);

            var isGameOver = gameOverDetector is not null &&
                             gameOverDetector.IsGameOver(Board, RemainingFigures, allowRotations: true);
            if (isGameOver)
                RecordGameOver(Score.CurrentScore, Array.Empty<HighScoreEntry>());

            return true;
        }

        public Figure RotateFigure(SlotIndex slotIndex)
        {
            if (CurrentState == GameState.GameOver || CurrentState == GameState.Paused) return null;

            var idx = slotIndex.Value;
            if (_slots[idx] is null) return null;

            _slots[idx] = _slots[idx].RotateClockwise();
            _slotRotations[idx] = (_slotRotations[idx] + 1) % 4;
            AddDomainEvent(new FigureRotatedEvent(idx, _slots[idx]));
            return _slots[idx];
        }

        public GameSaveData ToSaveData()
        {
            var data = new GameSaveData
            {
                seed = _seed,
                dealStep = _dealStep,
                currentScore = Score.CurrentScore,
                bestScore = Score.BestScore,
                boardWidth = Board.Width,
                boardHeight = Board.Height,
                isActive = CurrentState != GameState.GameOver
            };

            for (var x = 0; x < Board.Width; x++)
            {
                for (var y = 0; y < Board.Height; y++)
                    if (!Board.IsEmpty(x, y))
                        data.occupiedCells.Add(new OccupiedCellSaveData(x, y, Board.GetCellColor(x, y)));
            }

            for (var i = 0; i < TrayCapacity; i++)
            {
                var fig = _slots[i];
                var isConsumed = fig is null;
                var figureId = fig?.Id ?? string.Empty;
                data.traySlots.Add(new TraySlotSaveData(isConsumed, _slotRotations[i], figureId));
            }

            return data;
        }

        public void RestoreFromSave(GameSaveData save, IReadOnlyList<Figure> figurePool)
        {
            if (save is null) return;

            _seed = save.seed;
            _dealStep = save.dealStep;
            _hasDealtCurrentStep = true;
            CurrentState = GameState.Playing;

            Board.Clear();
            if (save.occupiedCells is not null)
                foreach (var c in save.occupiedCells)
                    Board.SetCell(c.x, c.y, CellState.Occupied, c.colorIndex);

            Score.Reset();
            Score.AddPoints(save.currentScore);
            Score.SetBestScore(save.bestScore);

            if (figurePool is not null && figurePool.Count > 0)
            {
                var effectivePool = FilterPoolByBoard(figurePool, Board.Width, Board.Height);
                var stepFigures = PredictableRandom.GenerateStepFigures(_seed, _dealStep, effectivePool, TrayCapacity);

                for (var i = 0; i < TrayCapacity; i++)
                {
                    var slotSave = (save.traySlots is not null && i < save.traySlots.Count) ? save.traySlots[i] : null;
                    if (slotSave is not null && slotSave.isConsumed)
                    {
                        _slots[i] = null;
                        _slotRotations[i] = 0;
                    }
                    else
                    {
                        Figure fig = null;
                        if (slotSave is not null && !string.IsNullOrEmpty(slotSave.figureId))
                        {
                            for (var p = 0; p < effectivePool.Count; p++)
                            {
                                if (!string.Equals(effectivePool[p].Id, slotSave.figureId, StringComparison.Ordinal))
                                    continue;
                                var tmpl = effectivePool[p];
                                fig = new Figure(tmpl.Id, tmpl.Name, tmpl.Shape, RotationAngle.Deg0,
                                    tmpl.ColorIndex);
                                break;
                            }
                        }

                        if (fig is null && stepFigures is not null && i < stepFigures.Count)
                            fig = stepFigures[i];

                        if (fig is not null)
                        {
                            var rotations = slotSave is not null ? (slotSave.rotationCount % 4) : 0;
                            for (var r = 0; r < rotations; r++)
                                fig = fig.RotateClockwise();
                            _slots[i] = fig;
                            _slotRotations[i] = rotations;
                        }
                        else
                        {
                            _slots[i] = null;
                            _slotRotations[i] = 0;
                        }
                    }
                }

                if (IsHandEmpty)
                    DealNewFigures(effectivePool);
                else
                    AddDomainEvent(new FiguresDealtEvent(new List<Figure>(_slots)));
            }

            AddDomainEvent(new ScoreUpdatedEvent(Score.CurrentScore, Score.BestScore));
            AddDomainEvent(new GameStateChangedEvent(GameState.Playing));
        }

        public bool TogglePause(out GameState resultingState)
        {
            if (CurrentState is GameState.Playing or GameState.Paused)
            {
                CurrentState = CurrentState == GameState.Playing ? GameState.Paused : GameState.Playing;
                resultingState = CurrentState;
                AddDomainEvent(new GameStateChangedEvent(CurrentState));
                return true;
            }

            resultingState = CurrentState;
            return false;
        }

        public IReadOnlyList<IDomainEvent> PullDomainEvents()
        {
            var events = _domainEvents.ToArray();
            _domainEvents.Clear();
            return events;
        }

        private void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

        private void RecordGameOver(int finalScore, IReadOnlyList<HighScoreEntry> topScores)
        {
            CurrentState = GameState.GameOver;
            AddDomainEvent(new GameOverEvent(finalScore, topScores));
            AddDomainEvent(new GameStateChangedEvent(GameState.GameOver));
        }

        private IReadOnlyList<Figure> DealNewFigures(IReadOnlyList<Figure> pool)
        {
            if (pool is null || pool.Count == 0)
                throw new InvalidOperationException("Cannot deal figures from an empty pool.");

            var effectivePool = FilterPoolByBoard(pool, Board.Width, Board.Height);

            if (_hasDealtCurrentStep)
                _dealStep++;

            var generated = PredictableRandom.GenerateStepFigures(_seed, _dealStep, effectivePool, TrayCapacity);
            for (var i = 0; i < TrayCapacity; i++)
            {
                _slots[i] = generated[i];
                _slotRotations[i] = 0;
            }

            _hasDealtCurrentStep = true;
            var dealtList = new List<Figure>(_slots);

            AddDomainEvent(new FiguresDealtEvent(dealtList));
            return dealtList;
        }

        private static IReadOnlyList<Figure> FilterPoolByBoard(IReadOnlyList<Figure> pool, int width, int height)
        {
            if (pool is null || pool.Count == 0 || width <= 0 || height <= 0) return pool;

            var filtered = new List<Figure>();
            foreach (var f in pool)
            {
                if (f is null || !f.CanFitOnBoard(width, height)) continue;
                var fig = f;
                if (fig.Shape.BoundingWidth > width || fig.Shape.BoundingHeight > height)
                    fig = fig.RotateClockwise();
                filtered.Add(fig);
            }

            filtered.Sort((a, b) => string.CompareOrdinal(a.Id, b.Id));
            return filtered.Count > 0 ? filtered : pool;
        }
    }
}