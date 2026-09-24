using System;

namespace App.Domain.Score
{
    [Serializable]
    public sealed class HighScoreEntry : IComparable<HighScoreEntry>
    {
        public int score;
        public string dateUtc;

        public HighScoreEntry()
        {
        }

        public HighScoreEntry(int score, string dateUtc)
        {
            this.score = score;
            this.dateUtc = dateUtc;
        }

        public HighScoreEntry(int score) : this(score, DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm"))
        {
        }

        public int CompareTo(HighScoreEntry other)
        {
            if (other == null) return 1;
            var scoreCompare = other.score.CompareTo(score);
            return scoreCompare != 0 ? scoreCompare : string.Compare(dateUtc, other.dateUtc, StringComparison.Ordinal);
        }
    }
}