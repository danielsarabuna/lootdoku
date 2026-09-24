using App.Domain.Common;

namespace App.Domain.GameLoop
{
    public readonly struct GameStateChangedEvent : IDomainEvent
    {
        public GameState NewState { get; }

        public GameStateChangedEvent(GameState newState)
        {
            NewState = newState;
        }
    }
}