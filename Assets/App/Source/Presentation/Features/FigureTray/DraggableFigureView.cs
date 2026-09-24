using System;
using System.Collections.Generic;
using App.Domain.Board;
using App.Domain.Figures;
using App.Presentation.Board;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace App.Presentation.FigureTray
{
    public sealed class DraggableFigureView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler,
        IPointerDownHandler
    {
        [Header("Settings")] [SerializeField] private float _cellSize = 48F;
        [SerializeField] private float _dragScale = 1.1F;
        [SerializeField] private Vector2 _dragPointerOffset = new Vector2(0F, 90F);

        [Header("Prefab")] [SerializeField] private FigureBlockCellView _blockCellPrefab;

        private Figure _figure;
        private BoardView _boardView;
        private Func<Figure, BoardCoordinate, bool> _canPlaceValidator;
        private Func<int, BoardCoordinate, bool> _onDropOnBoard;
        private Action _onDragStarted;

        private RectTransform _rectTransform;
        private CanvasGroup _canvasGroup;
        private Canvas _canvas;
        private Transform _originalParent;
        private Vector2 _lastPointerPosition;
        private Tween _moveTween;
        private Tween _scaleTween;

        private readonly List<FigureBlockCellView> _spawnedCells = new();

        public int SlotIndex { get; private set; }
        public bool IsDragging { get; private set; }

        public void Bind(
            int slotIndex,
            Figure figure,
            BoardView boardView,
            Func<Figure, BoardCoordinate, bool> canPlaceValidator,
            Func<int, BoardCoordinate, bool> onDropOnBoard,
            Action onDragStarted = null)
        {
            SlotIndex = slotIndex;
            _figure = figure;
            _boardView = boardView;
            _canPlaceValidator = canPlaceValidator;
            _onDropOnBoard = onDropOnBoard;
            _onDragStarted = onDragStarted;

            _originalParent = transform.parent;

            RebuildVisuals();
        }

        public void UpdateFigure(Figure newFigure)
        {
            _figure = newFigure;
            RebuildVisuals();

            if (IsDragging && _boardView != null)
            {
                var pointerPos = GetPointerScreenPosition();
                UpdateDragPosition(pointerPos);
                UpdateDragPreview(pointerPos);
            }
        }

        public void ReturnToSlot()
        {
            IsDragging = false;
            if (_originalParent != null)
                transform.SetParent(_originalParent, true);

            if (_canvasGroup != null)
                _canvasGroup.blocksRaycasts = true;

            _scaleTween?.Kill();
            _scaleTween = transform.DOScale(Vector3.one, 0.2F);

            _moveTween?.Kill();
            _moveTween = transform.DOLocalMove(Vector3.zero, 0.2F)
                .SetEase(Ease.OutQuad);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_figure is null) return;
            if (_canvas == null) _canvas = GetComponentInParent<Canvas>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_figure is null) return;

            IsDragging = true;
            _lastPointerPosition = eventData.position;
            _moveTween?.Kill();
            _scaleTween?.Kill();

            if (_canvasGroup != null)
                _canvasGroup.blocksRaycasts = false;

            _onDragStarted?.Invoke();

            if (_canvas == null) _canvas = GetComponentInParent<Canvas>();
            if (_canvas != null)
            {
                transform.SetParent(_canvas.transform, true);
                transform.SetAsLastSibling();
            }

            var boardCellSize = _boardView != null ? _boardView.CellSize.x : 88F;
            var targetScale = Mathf.Max(1.0F, boardCellSize / _cellSize * _dragScale);
            _scaleTween = transform.DOScale(Vector3.one * targetScale, 0.15F);

            UpdateDragPosition(eventData.position);
            UpdateDragPreview(eventData.position);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!IsDragging) return;

            _lastPointerPosition = eventData.position;
            UpdateDragPosition(eventData.position);
            UpdateDragPreview(eventData.position);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!IsDragging) return;
            ExecuteDrop(eventData.position);
        }

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvas = GetComponentInParent<Canvas>();

            var image = GetComponent<Image>();
            if (image == null)
            {
                image = gameObject.AddComponent<Image>();
                image.color = Color.clear;
                image.raycastTarget = true;
            }
        }

        private void Update()
        {
            if (!IsDragging) return;

            var pointerPos = GetPointerScreenPosition();
            if (pointerPos != _lastPointerPosition)
            {
                _lastPointerPosition = pointerPos;
                UpdateDragPosition(pointerPos);
                UpdateDragPreview(pointerPos);
            }
        }

        private void RebuildVisuals()
        {
            foreach (var cell in _spawnedCells)
                if (cell != null)
                    Destroy(cell.gameObject);
            _spawnedCells.Clear();

            if (_figure is null) return;

            var shape = _figure.Shape;
            var widthOffset = (shape.BoundingWidth - 1) * _cellSize * 0.5F;
            var heightOffset = (shape.BoundingHeight - 1) * _cellSize * 0.5F;

            foreach (var coord in shape.Coordinates)
            {
                var cell = Instantiate(_blockCellPrefab, transform);
                if (cell.transform is RectTransform rt)
                {
                    var posX = (coord.X * _cellSize) - widthOffset;
                    var posY = heightOffset - (coord.Y * _cellSize);

                    rt.anchoredPosition = new Vector2(posX, posY);
                    rt.sizeDelta = new Vector2(_cellSize, _cellSize);
                }

                cell.SetImageToDefault();
                _spawnedCells.Add(cell);
            }
        }

        private void ExecuteDrop(Vector2 screenPosition)
        {
            if (!IsDragging) return;

            _scaleTween?.Kill();
            _boardView?.ClearPreview();

            if (_canvasGroup != null)
                _canvasGroup.blocksRaycasts = true;

            var placed = false;
            if (TryFindDropOrigin(screenPosition, out var origin))
            {
                var isValid = _canPlaceValidator == null || _canPlaceValidator(_figure, origin);
                if (isValid && _onDropOnBoard != null)
                    placed = _onDropOnBoard(SlotIndex, origin);
            }

            IsDragging = false;

            if (!placed)
                ReturnToSlot();
        }

        private Vector2 GetPointerScreenPosition()
        {
            if (Pointer.current != null) return Pointer.current.position.ReadValue();

            if (Mouse.current != null) return Mouse.current.position.ReadValue();

            if (Touchscreen.current == null) return _lastPointerPosition;
            var primaryTouch = Touchscreen.current.primaryTouch;
            return primaryTouch.isInProgress ? primaryTouch.position.ReadValue() : _lastPointerPosition;
        }

        private Vector2 GetDragOffset() => _dragPointerOffset;

        private Camera GetCanvasCamera()
        {
            if (_canvas is null) return null;
            return _canvas.rootCanvas != null ? _canvas.rootCanvas.worldCamera : _canvas.worldCamera;
        }

        private void UpdateDragPosition(Vector2 screenPosition)
        {
            var targetScreenPos = screenPosition + GetDragOffset();
            var canvasRect = _canvas != null ? _canvas.transform as RectTransform : _rectTransform;
            var cam = GetCanvasCamera();
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                    canvasRect, targetScreenPos, cam, out var worldPoint))
                transform.position = worldPoint;
        }

        private void UpdateDragPreview(Vector2 screenPosition)
        {
            if (_boardView == null || _figure == null) return;

            if (TryFindDropOrigin(screenPosition, out var origin))
            {
                var isValid = _canPlaceValidator != null && _canPlaceValidator(_figure, origin);
                _boardView.ShowPreview(_figure, origin, isValid);
            }
            else _boardView.ClearPreview();
        }

        private bool TryFindDropOrigin(Vector2 screenPosition, out BoardCoordinate origin)
        {
            origin = default;
            if (_boardView == null || _figure == null) return false;

            UpdateDragPosition(screenPosition);

            if (_boardView.TryGetOriginForShape(transform.position, _figure.Shape, out origin))
                return true;

            var targetScreenPos = screenPosition + GetDragOffset();
            var cam = GetCanvasCamera();
            if (!_boardView.TryGetCoordinateAtScreenPosition(targetScreenPos, cam, out var centerHoverCoord))
                return false;
            var shape = _figure.Shape;
            var centerOffsetX = (shape.BoundingWidth - 1) * 0.5F;
            var centerOffsetY = (shape.BoundingHeight - 1) * 0.5F;

            var originX = Mathf.FloorToInt(centerHoverCoord.X - centerOffsetX + 0.5F);
            var originY = Mathf.FloorToInt(centerHoverCoord.Y - centerOffsetY + 0.5F);

            foreach (var offset in shape.Coordinates)
            {
                var tx = originX + offset.X;
                var ty = originY + offset.Y;
                if (tx < 0 || tx >= _boardView.Width || ty < 0 || ty >= _boardView.Height)
                    return false;
            }

            origin = new BoardCoordinate(originX, originY);
            return true;
        }

        private void OnDestroy()
        {
            _moveTween?.Kill();
            _scaleTween?.Kill();
        }
    }
}