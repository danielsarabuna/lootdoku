using System;
using App.Domain.Board;
using App.Presentation.Board;
using MessagePipe;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace App.Presentation.VFX
{
    public sealed class CellBurnVfxPlayer : MonoBehaviour, IInitializable, IDisposable
    {
        [SerializeField] private BoardView _boardView;
        [SerializeField] private ParticleSystem _particlePrefab;

        private ISubscriber<LinesClearedEvent> _clearedSub;
        private readonly DisposableBagBuilder _bag = DisposableBag.CreateBuilder();

        [Inject]
        public void Construct(ISubscriber<LinesClearedEvent> clearedSub)
        {
            _clearedSub = clearedSub;
        }

        public void Initialize()
        {
            _clearedSub?.Subscribe(OnLinesCleared).AddTo(_bag);
        }

        public void Dispose() => _bag.Build().Dispose();

        private void OnLinesCleared(LinesClearedEvent e)
        {
            foreach (var coord in e.Result.ClearedCoordinates)
            {
                var pos = _boardView.GetCellWorldPosition(coord);
                var ps = Instantiate(_particlePrefab, pos, Quaternion.identity, transform);
                Destroy(ps.gameObject, 1.5F);
            }
        }
    }
}