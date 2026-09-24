using App.Domain.Common;
using App.Domain.Figures;

namespace App.Domain.Board
{
    public readonly struct FigurePlacedEvent : IDomainEvent
    {
        public int SlotIndex { get; }
        public Figure Figure { get; }
        public BoardCoordinate Coordinate { get; }

        public FigurePlacedEvent(int slotIndex, Figure figure, BoardCoordinate coordinate)
        {
            SlotIndex = slotIndex;
            Figure = figure;
            Coordinate = coordinate;
        }
    }
}