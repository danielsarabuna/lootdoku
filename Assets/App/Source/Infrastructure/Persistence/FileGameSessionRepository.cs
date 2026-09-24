using System;
using System.IO;
using App.Application.Ports;
using App.Domain.Board;
using App.Domain.GameLoop;
using App.Domain.Repositories;
using App.Domain.Score;
using UnityEngine;

namespace App.Infrastructure.Persistence
{
    public sealed class FileGameSessionRepository : IGameSessionRepository
    {
        private readonly string _filePath;
        private readonly IGameConfigProvider _configProvider;
        private readonly ILogService _logger;
        private GameSession _activeSession;
        private GameSaveData _pendingSaveData;
        private bool _hasActiveSave;

        public bool HasActiveSave
        {
            get
            {
                if (_activeSession != null)
                    return _hasActiveSave;
                var save = LoadSaveData();
                return save != null && save.isActive;
            }
        }

        public FileGameSessionRepository(string filePath, IGameConfigProvider configProvider, ILogService logger)
        {
            _filePath = !string.IsNullOrEmpty(filePath)
                ? filePath
                : throw new ArgumentException("File path must not be null or empty.", nameof(filePath));
            _configProvider = configProvider ?? throw new ArgumentNullException(nameof(configProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public GameSession GetActiveSession()
        {
            if (_activeSession != null)
            {
                if (_hasActiveSave && _pendingSaveData != null)
                {
                    var pool = _configProvider?.AvailableFiguresPool;
                    if (pool != null && pool.Count > 0)
                    {
                        var saveToRestore = _pendingSaveData;
                        _pendingSaveData = null;
                        _activeSession.RestoreFromSave(saveToRestore, pool);
                    }
                }

                return _activeSession;
            }

            var save = LoadSaveData();
            if (save != null && save.isActive)
            {
                _hasActiveSave = true;
                var width = save.boardWidth > 0 ? save.boardWidth : (_configProvider?.BoardWidth ?? 8);
                var height = save.boardHeight > 0 ? save.boardHeight : (_configProvider?.BoardHeight ?? 8);
                var board = new BoardGrid(width, height);
                var score = new ScoreModel();
                var session = new GameSession(board, score, GameState.Playing, save.seed);

                var pool = _configProvider?.AvailableFiguresPool;
                if (pool != null && pool.Count > 0)
                {
                    _pendingSaveData = null;
                    session.RestoreFromSave(save, pool);
                }
                else
                {
                    session.RestoreFromSave(save, null);
                    _pendingSaveData = save;
                }

                _activeSession = session;
                return _activeSession;
            }

            _hasActiveSave = false;
            _pendingSaveData = null;
            var defaultWidth = _configProvider?.BoardWidth ?? 8;
            var defaultHeight = _configProvider?.BoardHeight ?? 8;
            _activeSession = GameSession.CreateNew(defaultWidth, defaultHeight);
            return _activeSession;
        }

        public void Save(GameSession session)
        {
            if (session is null) return;

            _activeSession = session;

            if (session.CurrentState == GameState.GameOver)
            {
                Clear();
                return;
            }

            _hasActiveSave = true;
            _pendingSaveData = null;

            try
            {
                var saveData = session.ToSaveData();
                var json = JsonUtility.ToJson(saveData, prettyPrint: true);
                var dir = Path.GetDirectoryName(_filePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                File.WriteAllText(_filePath, json);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Could not save game: {ex.Message}");
            }
        }

        public void Clear()
        {
            _hasActiveSave = false;
            _pendingSaveData = null;

            try
            {
                if (File.Exists(_filePath))
                    File.Delete(_filePath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Could not delete save: {ex.Message}");
            }
        }

        private GameSaveData LoadSaveData()
        {
            try
            {
                if (!File.Exists(_filePath)) return null;

                var json = File.ReadAllText(_filePath);
                return JsonUtility.FromJson<GameSaveData>(json);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Could not load save: {ex.Message}");
                return null;
            }
        }
    }
}