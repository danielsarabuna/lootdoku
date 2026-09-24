using System.Collections.Generic;
using System.Threading;
using App.Domain.Repositories;
using App.Domain.Score;
using Cysharp.Threading.Tasks;

namespace App.Application.Score
{
    public sealed class HighScoreService
    {
        private const int TopScoresCount = 3;
        private readonly IHighScoreRepository _repository;
        private List<HighScoreEntry> _topScores = new();

        public IReadOnlyList<HighScoreEntry> TopScores => _topScores;
        public int BestScore => _topScores.Count > 0 ? _topScores[0].score : 0;

        public HighScoreService(IHighScoreRepository repository) => _repository = repository;

        public async UniTask<IReadOnlyList<HighScoreEntry>> LoadTopScoresAsync(CancellationToken ct = default)
        {
            var loaded = await _repository.LoadTopScoresAsync(TopScoresCount, ct);
            _topScores = new List<HighScoreEntry>(loaded);
            return _topScores;
        }

        public void RecordScore(int score)
        {
            if (score <= 0) return;

            var entry = new HighScoreEntry(score);
            UpdateTopScores(entry);
            _repository.SaveScore(entry);
        }

        private void UpdateTopScores(HighScoreEntry entry)
        {
            _topScores.Add(entry);
            _topScores.Sort();
            if (_topScores.Count > TopScoresCount)
                _topScores.RemoveAt(_topScores.Count - 1);
        }
    }
}