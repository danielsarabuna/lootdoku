using App.Domain.Common;

namespace App.Domain.Board
{
    public readonly struct LinesClearedEvent : IDomainEvent
    {
        public ClearResult Result { get; }

        public LinesClearedEvent(ClearResult result)
        {
            Result = result;
        }
    }
}