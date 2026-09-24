using System;

namespace App.Domain.Score
{
    public sealed class ScoreModel
    {
        public int CurrentScore { get; private set; }
        public int BestScore { get; private set; }

        public ScoreModel(int initialBestScore = 0)
        {
            BestScore = Math.Max(0, initialBestScore);
            CurrentScore = 0;
        }

        public void AddPoints(int points)
        {
            if (points <= 0) return;

            CurrentScore += points;
            if (CurrentScore > BestScore)
                BestScore = CurrentScore;
        }

        public void SetBestScore(int bestScore)
        {
            if (bestScore > BestScore)
                BestScore = bestScore;
        }

        public void Reset() => CurrentScore = 0;
    }
}