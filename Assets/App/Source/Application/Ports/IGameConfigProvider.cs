using System.Collections.Generic;
using System.Threading;
using App.Domain.Figures;
using Cysharp.Threading.Tasks;

namespace App.Application.Ports
{
    public interface IGameConfigProvider
    {
        int BoardWidth { get; }
        int BoardHeight { get; }
        int PointsPerCell { get; }
        IReadOnlyList<Figure> AvailableFiguresPool { get; }
        UniTask InitializeAsync(CancellationToken ct = default);
    }
}