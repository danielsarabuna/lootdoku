using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using App.Application.Ports;
using App.Domain.Repositories;
using App.Domain.Score;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace App.Infrastructure.Persistence
{
    [Serializable]
    public sealed class HighScoreDataWrapper
    {
        public List<HighScoreEntry> entries = new();
    }

    public sealed class FileHighScoreRepository : IHighScoreRepository
    {
        private readonly string _filePath;
        private readonly ILogService _logger;

        public FileHighScoreRepository(string filePath, ILogService logger)
        {
            _filePath = !string.IsNullOrEmpty(filePath)
                ? filePath
                : throw new ArgumentException("File path must not be null or empty.", nameof(filePath));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public UniTask<IReadOnlyList<HighScoreEntry>> LoadTopScoresAsync(int count, CancellationToken ct = default)
        {
            try
            {
                if (!File.Exists(_filePath))
                    return UniTask.FromResult<IReadOnlyList<HighScoreEntry>>(Array.Empty<HighScoreEntry>());

                var json = File.ReadAllText(_filePath);
                var wrapper = JsonUtility.FromJson<HighScoreDataWrapper>(json);
                if (wrapper?.entries is null)
                    return UniTask.FromResult<IReadOnlyList<HighScoreEntry>>(Array.Empty<HighScoreEntry>());

                wrapper.entries.Sort();
                IReadOnlyList<HighScoreEntry> result = wrapper.entries.Take(count).ToList();
                return UniTask.FromResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Could not load high scores: {ex.Message}");
                return UniTask.FromResult<IReadOnlyList<HighScoreEntry>>(Array.Empty<HighScoreEntry>());
            }
        }

        public void SaveScore(HighScoreEntry entry)
        {
            if (entry is null) return;

            try
            {
                var list = new List<HighScoreEntry>();
                if (File.Exists(_filePath))
                {
                    var existingJson = File.ReadAllText(_filePath);
                    var existingWrapper = JsonUtility.FromJson<HighScoreDataWrapper>(existingJson);
                    if (existingWrapper?.entries is not null)
                        list.AddRange(existingWrapper.entries);
                }

                list.Add(entry);
                list.Sort();

                var topList = list.Take(3).ToList();
                var wrapper = new HighScoreDataWrapper { entries = topList };
                var newJson = JsonUtility.ToJson(wrapper, prettyPrint: true);

                var dir = Path.GetDirectoryName(_filePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                File.WriteAllText(_filePath, newJson);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Could not save high score: {ex.Message}");
            }
        }
    }
}