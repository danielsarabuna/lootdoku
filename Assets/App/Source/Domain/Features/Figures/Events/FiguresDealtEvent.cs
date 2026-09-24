using System;
using System.Collections.Generic;
using App.Domain.Common;

namespace App.Domain.Figures
{
    public readonly struct FiguresDealtEvent : IDomainEvent
    {
        public IReadOnlyList<Figure> Figures { get; }

        public FiguresDealtEvent(IReadOnlyList<Figure> figures)
        {
            Figures = figures ?? Array.Empty<Figure>();
        }
    }
}