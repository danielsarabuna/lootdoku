using System.Collections.Generic;
using System.Threading;
using App.Domain.Score;
using Cysharp.Threading.Tasks;

namespace App.Domain.Repositories
{
    public interface IHighScoreRepository
    {
        UniTask<IReadOnlyList<HighScoreEntry>> LoadTopScoresAsync(int count, CancellationToken ct = default);
        void SaveScore(HighScoreEntry entry);
    }
}