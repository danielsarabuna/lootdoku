using System;
using System.Collections.Generic;
using App.Domain.Common;
using App.Domain.Score;

namespace App.Domain.GameLoop
{
    public readonly struct GameOverEvent : IDomainEvent
    {
        public int FinalScore { get; }
        public IReadOnlyList<HighScoreEntry> TopScores { get; }

        public GameOverEvent(int finalScore, IReadOnlyList<HighScoreEntry> topScores)
        {
            FinalScore = finalScore;
            TopScores = topScores ?? Array.Empty<HighScoreEntry>();
        }
    }
}