using System;
using System.IO;
using UnityApp = UnityEngine.Application;
using App.Application.Board.Commands;
using App.Application.Board.Queries;
using App.Application.Board.Requests;
using App.Application.Figures.Commands;
using App.Application.Figures.Requests;
using App.Application.GameLoop.Commands;
using App.Application.GameLoop.Requests;
using App.Application.Ports;
using App.Application.Score;
using App.Domain.Board;
using App.Domain.Figures;
using App.Domain.GameLoop;
using App.Domain.Repositories;
using App.Domain.Score;
using App.Infrastructure.Audio;
using App.Infrastructure.Config;
using App.Infrastructure.Config.ScriptableObjects;
using App.Infrastructure.Input;
using App.Infrastructure.Logging;
using App.Infrastructure.Persistence;
using App.Infrastructure.Resources;
using App.Presentation.GameOverScreen;
using App.Presentation.GameplayScreen;
using App.Presentation.LoadingScreen;
using App.Presentation.PauseScreen;
using App.Presentation.VFX;
using MessagePipe;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace App.Composition
{
    public sealed class GameLifetimeScope : LifetimeScope
    {
        [Header("Configuration")] [SerializeField]
        private GameSettingsSO _settings;

        [Header("Scene Views")] [SerializeField]
        private LoadingScreenView _loadingScreenView;

        [SerializeField] private GameplayScreenView _gameplayScreenView;
        [SerializeField] private PauseScreenView _pauseScreenView;
        [SerializeField] private GameOverScreenView _gameOverScreenView;
        [SerializeField] private CellBurnVfxPlayer _cellBurnVfxPlayer;

        protected override void Awake()
        {
            EnforceTargetFrameRate();
            base.Awake();
        }

        protected override void Configure(IContainerBuilder builder)
        {
            var options = builder.RegisterMessagePipe();
            builder.RegisterMessageBroker<FigurePlacedEvent>(options);
            builder.RegisterMessageBroker<FigureRotatedEvent>(options);
            builder.RegisterMessageBroker<LinesClearedEvent>(options);
            builder.RegisterMessageBroker<FiguresDealtEvent>(options);
            builder.RegisterMessageBroker<ScoreUpdatedEvent>(options);
            builder.RegisterMessageBroker<GameOverEvent>(options);
            builder.RegisterMessageBroker<GameStateChangedEvent>(options);

            RegisterHandler<CanPlaceFigureRequest, CanPlaceFigureResult, CanPlaceFigureQueryHandler>(builder);
            RegisterHandler<PlaceFigureRequest, PlaceFigureResult, PlaceFigureCommandHandler>(builder);
            RegisterHandler<GetBoardGridRequest, BoardGridDto, GetBoardGridQueryHandler>(builder);

            RegisterHandler<RotateFigureRequest, RotateFigureResult, RotateFigureCommandHandler>(builder);

            RegisterHandler<StartNewGameRequest, StartNewGameResult, StartNewGameCommandHandler>(builder);
            RegisterHandler<TogglePauseRequest, TogglePauseResult, TogglePauseCommandHandler>(builder);

            RegisterAsyncHandler<InitializeGameRequest, InitializeGameResult, InitializeGameCommandHandler>(builder);
            RegisterHandler<RestoreGameRequest, RestoreGameResult, RestoreGameCommandHandler>(builder);

            var gameStateFilePath = Path.Combine(UnityApp.persistentDataPath, "gamestate.json");
            var highScoresFilePath = Path.Combine(UnityApp.persistentDataPath, "highscores.json");

            builder.Register<LogService>(Lifetime.Singleton).As<ILogService>();
            builder.Register(
                resolver => new FileHighScoreRepository(highScoresFilePath, resolver.Resolve<ILogService>()),
                Lifetime.Singleton).As<IHighScoreRepository>();
            builder.Register(resolver => new ResourceService(resolver.Resolve<ILogService>()), Lifetime.Singleton)
                .As<IResourceService>().As<IDisposable>();
            builder.Register(resolver =>
            {
                var res = resolver.Resolve<IResourceService>();
                return new ScriptableObjectConfigProvider(_settings, res);
            }, Lifetime.Singleton).As<IGameConfigProvider>();
            builder.Register(resolver =>
            {
                var res = resolver.Resolve<IResourceService>();
                var logger = resolver.Resolve<ILogService>();
                return new AudioService(gameObject, res, logger);
            }, Lifetime.Singleton).As<IAudioService>().As<IDisposable>();

            builder.Register<InputSystemService>(Lifetime.Singleton)
                .As<IInputService>()
                .As<IInitializable>()
                .As<IDisposable>();

            builder.Register(resolver =>
            {
                var cfg = resolver.Resolve<IGameConfigProvider>();
                var logger = resolver.Resolve<ILogService>();
                return new FileGameSessionRepository(gameStateFilePath, cfg, logger);
            }, Lifetime.Singleton).As<IGameSessionRepository>();

            builder.Register(resolver =>
            {
                var repo = resolver.Resolve<IGameSessionRepository>();
                return repo.GetActiveSession();
            }, Lifetime.Singleton);

            builder.Register(resolver => resolver.Resolve<GameSession>().Score, Lifetime.Singleton);

            builder.Register<PlacementValidator>(Lifetime.Singleton).As<IPlacementValidator>();
            builder.Register<LineClearRule>(Lifetime.Singleton).As<ILineClearRule>();
            builder.Register<GameOverDetector>(Lifetime.Singleton).As<IGameOverDetector>();

            builder.Register<HighScoreService>(Lifetime.Singleton);

            builder.RegisterComponent(_loadingScreenView);
            builder.RegisterComponent(_gameplayScreenView);
            builder.RegisterComponent(_pauseScreenView);
            builder.RegisterComponent(_gameOverScreenView);
            builder.RegisterComponent(_cellBurnVfxPlayer).AsImplementedInterfaces();

            builder.RegisterEntryPoint<LoadingScreenPresenter>();
            builder.RegisterEntryPoint<GameplayScreenPresenter>();
            builder.RegisterEntryPoint<PauseScreenPresenter>();
            builder.RegisterEntryPoint<GameOverScreenPresenter>();
        }

        private void RegisterHandler<TRequest, TResponse, THandler>(IContainerBuilder builder)
            where THandler : class, IRequestHandler<TRequest, TResponse>
        {
            builder.Register<THandler>(Lifetime.Singleton)
                .As<IRequestHandler<TRequest, TResponse>>();
        }

        private void RegisterAsyncHandler<TRequest, TResponse, THandler>(IContainerBuilder builder)
            where THandler : class, IAsyncRequestHandler<TRequest, TResponse>
        {
            builder.Register<THandler>(Lifetime.Singleton)
                .As<IAsyncRequestHandler<TRequest, TResponse>>();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnforceTargetFrameRate()
        {
            QualitySettings.vSyncCount = 0;
            UnityApp.targetFrameRate = 60;
        }
    }
}