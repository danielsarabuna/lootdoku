using App.Domain.GameLoop;

namespace App.Domain.Repositories
{
    public interface IGameSessionRepository
    {
        bool HasActiveSave { get; }
        GameSession GetActiveSession();
        void Save(GameSession session);
    }
}