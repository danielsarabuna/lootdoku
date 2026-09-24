using App.Domain.Common;

namespace App.Domain.Score
{
    public readonly struct ScoreUpdatedEvent : IDomainEvent
    {
        public int CurrentScore { get; }
        public int BestScore { get; }

        public ScoreUpdatedEvent(int currentScore, int bestScore)
        {
            CurrentScore = currentScore;
            BestScore = bestScore;
        }
    }
}