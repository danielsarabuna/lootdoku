using App.Domain.Common;

namespace App.Domain.Figures
{
    public readonly struct FigureRotatedEvent : IDomainEvent
    {
        public int SlotIndex { get; }
        public Figure Figure { get; }

        public FigureRotatedEvent(int slotIndex, Figure figure)
        {
            SlotIndex = slotIndex;
            Figure = figure;
        }
    }
}