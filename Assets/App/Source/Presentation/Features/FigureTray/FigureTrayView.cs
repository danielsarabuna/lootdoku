using System;
using System.Collections.Generic;
using App.Domain.Board;
using App.Domain.Figures;
using App.Presentation.Board;
using UnityEngine;

namespace App.Presentation.FigureTray
{
    public sealed class FigureTrayView : MonoBehaviour
    {
        [Header("Slots")] [SerializeField] private FigureSlotView[] _slots = new FigureSlotView[3];

        [Header("Prefab")] [SerializeField] private DraggableFigureView _draggableFigurePrefab;

        private BoardView _boardView;
        private Func<Figure, BoardCoordinate, bool> _canPlaceValidator;
        private Func<int, BoardCoordinate, bool> _onFigureDropped;
        private Action<int> _onRotateRequested;
        private Action _onFigurePickedUp;

        private int _lastInteractedSlot = 0;

        private DraggableFigureView ActiveDraggedFigure
        {
            get
            {
                foreach (var slot in _slots)
                {
                    if (slot != null && slot.CurrentFigureView != null && slot.CurrentFigureView.IsDragging)
                    {
                        return slot.CurrentFigureView;
                    }
                }

                return null;
            }
        }

        public void Initialize(
            BoardView boardView,
            Func<Figure, BoardCoordinate, bool> canPlaceValidator,
            Func<int, BoardCoordinate, bool> onFigureDropped,
            Action<int> onRotateRequested,
            Action onFigurePickedUp = null)
        {
            _boardView = boardView;
            _canPlaceValidator = canPlaceValidator;
            _onFigureDropped = onFigureDropped;
            _onRotateRequested = onRotateRequested;
            _onFigurePickedUp = onFigurePickedUp;

            for (var i = 0; i < _slots.Length; i++)
                _slots[i].Initialize(i, HandleRotateClick);
        }

        public void PopulateTray(IReadOnlyList<Figure> figures)
        {
            for (var i = 0; i < _slots.Length; i++)
            {
                var slot = _slots[i];

                slot.ClearFigureView();

                if (i >= figures.Count || figures[i] == null) continue;
                var figView = Instantiate(_draggableFigurePrefab, slot.FigureContainer);
                if (figView.transform is RectTransform figRt)
                {
                    figRt.anchoredPosition = Vector2.zero;
                    figRt.localScale = Vector3.one;
                }

                var capturedIndex = i;

                figView.Bind(
                    capturedIndex,
                    figures[i],
                    _boardView,
                    _canPlaceValidator,
                    (slotIdx, origin) => _onFigureDropped != null && _onFigureDropped(slotIdx, origin),
                    () =>
                    {
                        _lastInteractedSlot = capturedIndex;
                        _onFigurePickedUp?.Invoke();
                    });

                slot.SetFigureView(figView);
            }
        }

        public void UpdateFigureAtSlot(int slotIndex, Figure updatedFigure)
        {
            if (slotIndex < 0 || slotIndex >= _slots.Length) return;

            var slot = _slots[slotIndex];
            if (slot != null && slot.CurrentFigureView != null) slot.CurrentFigureView.UpdateFigure(updatedFigure);
        }

        public void ClearSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _slots.Length) return;
            _slots[slotIndex]?.ClearFigureView();
        }

        public void RotateActiveOrLastSlot()
        {
            var active = ActiveDraggedFigure;
            if (active != null)
            {
                _onRotateRequested?.Invoke(active.SlotIndex);
                return;
            }

            if (_slots[_lastInteractedSlot] != null && _slots[_lastInteractedSlot].HasFigure)
            {
                _onRotateRequested?.Invoke(_lastInteractedSlot);
                return;
            }

            for (var i = 0; i < _slots.Length; i++)
            {
                if (_slots[i] == null || !_slots[i].HasFigure) continue;
                _lastInteractedSlot = i;
                _onRotateRequested?.Invoke(i);
                return;
            }
        }

        public void CancelActiveDrag()
        {
            var active = ActiveDraggedFigure;
            if (active == null) return;
            active.ReturnToSlot();
            _boardView?.ClearPreview();
        }

        private void HandleRotateClick(int slotIndex)
        {
            _lastInteractedSlot = slotIndex;
            _onRotateRequested?.Invoke(slotIndex);
        }
    }
}