using App.Application.GameLoop.Requests;
using App.Application.Ports;
using App.Domain.GameLoop;
using App.Domain.Repositories;
using MessagePipe;

namespace App.Application.GameLoop.Commands
{
    public sealed class TogglePauseCommandHandler : IRequestHandler<TogglePauseRequest, TogglePauseResult>
    {
        private readonly IGameSessionRepository _sessionRepository;
        private readonly IPublisher<GameStateChangedEvent> _statePublisher;
        private readonly IAudioService _audioService;

        public TogglePauseCommandHandler(
            IGameSessionRepository sessionRepository,
            IPublisher<GameStateChangedEvent> statePublisher,
            IAudioService audioService)
        {
            _sessionRepository = sessionRepository;
            _statePublisher = statePublisher;
            _audioService = audioService;
        }

        public TogglePauseResult Invoke(TogglePauseRequest request)
        {
            var session = _sessionRepository.GetActiveSession();
            if (!session.TogglePause(out var newState)) return new TogglePauseResult();
            _audioService.SetPauseDucking(newState == GameState.Paused);
            _sessionRepository.Save(session);
            session.PullDomainEvents();
            _statePublisher.Publish(new GameStateChangedEvent(session.CurrentState));

            return new TogglePauseResult();
        }
    }
}