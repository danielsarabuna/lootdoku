using System.Collections.Generic;
using System.Threading;
using App.Application.Ports;
using App.Domain.Figures;
using App.Infrastructure.Config.ScriptableObjects;
using Cysharp.Threading.Tasks;

namespace App.Infrastructure.Config
{
    public sealed class ScriptableObjectConfigProvider : IGameConfigProvider
    {
        private readonly GameSettingsSO _settings;
        private readonly IResourceService _resourceService;
        private readonly List<FigureDataSO> _figureDataList;
        private IReadOnlyList<Figure> _cachedFigures;
        private int _cachedWidth = -1;
        private int _cachedHeight = -1;

        public int BoardWidth => _settings != null ? _settings.Width : 8;
        public int BoardHeight => _settings != null ? _settings.Height : 8;
        public int PointsPerCell => _settings != null ? _settings.PointsPerCell : 10;

        public IReadOnlyList<Figure> AvailableFiguresPool
        {
            get
            {
                var w = BoardWidth;
                var h = BoardHeight;
                if (_cachedFigures is null || _cachedFigures.Count == 0 || _cachedWidth != w || _cachedHeight != h)
                {
                    _cachedWidth = w;
                    _cachedHeight = h;
                    var list = new List<Figure>();
                    if (_figureDataList != null)
                    {
                        foreach (var figSo in _figureDataList)
                        {
                            if (figSo != null)
                            {
                                var fig = figSo.ToDomainFigure();
                                if (fig.CanFitOnBoard(w, h))
                                {
                                    if (fig.Shape.BoundingWidth > w || fig.Shape.BoundingHeight > h)
                                        fig = fig.RotateClockwise();
                                    list.Add(fig);
                                }
                            }
                        }
                    }

                    list.Sort((a, b) => string.CompareOrdinal(a.Id, b.Id));
                    _cachedFigures = list;
                }

                return _cachedFigures;
            }
        }

        public ScriptableObjectConfigProvider(GameSettingsSO settings, IResourceService resourceService)
        {
            _settings = settings;
            _resourceService = resourceService;
            _figureDataList = new List<FigureDataSO>();
        }

        public async UniTask InitializeAsync(CancellationToken ct = default)
        {
            if (_resourceService != null)
            {
                var loaded = await _resourceService.LoadAssetsByLabelAsync<FigureDataSO>("Figure", ct);
                if (loaded != null && loaded.Count > 0)
                    SetFigures(loaded);
            }
        }

        private void SetFigures(IEnumerable<FigureDataSO> figures)
        {
            _figureDataList.Clear();
            if (figures != null)
            {
                _figureDataList.AddRange(figures);
                _figureDataList.Sort((a, b) =>
                    string.CompareOrdinal(a != null ? a.name : string.Empty, b != null ? b.name : string.Empty));
            }

            InvalidateCache();
        }

        private void InvalidateCache() => _cachedFigures = null;
    }
}