using App.Application.Figures.Requests;
using App.Application.Ports;
using App.Domain.Figures;
using App.Domain.Repositories;
using MessagePipe;

namespace App.Application.Figures.Commands
{
    public sealed class RotateFigureCommandHandler : IRequestHandler<RotateFigureRequest, RotateFigureResult>
    {
        private readonly IGameSessionRepository _sessionRepository;
        private readonly IPublisher<FigureRotatedEvent> _rotatedPublisher;
        private readonly IAudioService _audioService;

        public RotateFigureCommandHandler(
            IGameSessionRepository sessionRepository,
            IPublisher<FigureRotatedEvent> rotatedPublisher,
            IAudioService audioService)
        {
            _sessionRepository = sessionRepository;
            _rotatedPublisher = rotatedPublisher;
            _audioService = audioService;
        }

        public RotateFigureResult Invoke(RotateFigureRequest request)
        {
            if (!SlotIndex.TryCreate(request.SlotIndex, out var slotIndex))
                return RotateFigureResult.Failed;

            var session = _sessionRepository.GetActiveSession();
            var rotated = session.RotateFigure(slotIndex);
            if (rotated == null) return RotateFigureResult.Failed;
            _sessionRepository.Save(session);
            session.PullDomainEvents();
            _rotatedPublisher.Publish(new FigureRotatedEvent(slotIndex.Value, rotated));
            _audioService.PlaySfx(AudioSfxType.Rotate);
            return RotateFigureResult.Succeeded;
        }
    }
}