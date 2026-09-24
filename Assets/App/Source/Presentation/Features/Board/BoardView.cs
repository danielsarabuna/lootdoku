using System;
using System.Collections.Generic;
using App.Domain.Board;
using App.Domain.Figures;
using UnityEngine;
using UnityEngine.UI;

namespace App.Presentation.Board
{
    public sealed class BoardView : MonoBehaviour
    {
        [Header("References")] [SerializeField]
        private RectTransform _gridContainer;

        [SerializeField] private GridLayoutGroup _layoutGroup;
        [SerializeField] private CellView _cellPrefab;

        private int _width;
        private int _height;
        private CellView[,] _cells;
        private readonly List<BoardCoordinate> _activePreviewCoords = new();

        public int Width => _width;
        public int Height => _height;

        public Vector2 CellSize => _layoutGroup != null && _layoutGroup.cellSize.x > 0F
            ? _layoutGroup.cellSize
            : new Vector2(100F, 100F);

        public void Initialize(int width, int height, CellView cellPrefab = null)
        {
            _width = width;
            _height = height;
            if (cellPrefab != null) _cellPrefab = cellPrefab;

            if (_gridContainer == null) _gridContainer = GetComponent<RectTransform>();
            if (_layoutGroup == null) _layoutGroup = GetComponent<GridLayoutGroup>();

            ClearExistingCells();
            SetupLayout(width, height);
            CreateCells(width, height);
        }

        public CellView GetCell(int x, int y) => TryGetCell(x, y, out var cell) ? cell : null;

        public Vector3 GetCellWorldPosition(BoardCoordinate coord) =>
            TryGetCell(coord, out var cell) ? cell.transform.position : transform.position;

        public void SetCellOccupied(BoardCoordinate coord, bool animate = true)
        {
            if (TryGetCell(coord, out var cell))
                cell.SetOccupied(animate);
        }

        public void ShowPreview(Figure figure, BoardCoordinate origin, bool isValid)
        {
            ClearPreview();
            if (figure is null) return;

            foreach (var offset in figure.Shape.Coordinates)
            {
                var target = origin + offset;
                if (target.X < 0 || target.X >= _width || target.Y < 0 || target.Y >= _height)
                    return;
            }

            foreach (var offset in figure.Shape.Coordinates)
            {
                var target = origin + offset;
                if (TryGetCell(target, out var cell))
                {
                    cell.ShowPreview(isValid);
                    _activePreviewCoords.Add(target);
                }
            }
        }

        public void ClearPreview()
        {
            foreach (var coord in _activePreviewCoords)
                if (TryGetCell(coord, out var cell))
                    cell.ResetPreview();
            _activePreviewCoords.Clear();
        }

        public void PlayClearAnimation(IReadOnlyList<BoardCoordinate> coords, Action onComplete = null)
        {
            if (coords is null || coords.Count == 0)
            {
                onComplete?.Invoke();
                return;
            }

            var remaining = coords.Count;
            foreach (var coord in coords)
            {
                if (TryGetCell(coord, out var cell))
                    cell.PlayClearAnimation(() =>
                    {
                        remaining--;
                        if (remaining <= 0) onComplete?.Invoke();
                    });
                else
                {
                    remaining--;
                    if (remaining <= 0) onComplete?.Invoke();
                }
            }
        }

        public bool TryGetOriginForShape(Vector3 worldPosition, FigureShape shape, out BoardCoordinate origin)
        {
            origin = default;
            if (_cells is null || _width < 1 || _height < 1 || shape is null) return false;

            var p00 = _cells[0, 0].transform.position;
            var cellSize = CellSize;
            var spacing = _layoutGroup != null ? _layoutGroup.spacing : Vector2.zero;

            var stepX = 0F;
            if (_width >= 2 && _cells[1, 0] != null)
                stepX = _cells[1, 0].transform.position.x - p00.x;

            if (Mathf.Abs(stepX) < 0.0001F)
            {
                var localStep = cellSize.x + spacing.x;
                stepX = transform.TransformVector(new Vector3(localStep, 0F, 0F)).x;
                if (Mathf.Abs(stepX) < 0.0001F) stepX = localStep;
            }

            var stepY = 0F;
            if (_height >= 2 && _cells[0, 1] != null)
                stepY = p00.y - _cells[0, 1].transform.position.y;

            if (Mathf.Abs(stepY) < 0.0001F)
            {
                var localStep = cellSize.y + spacing.y;
                stepY = -transform.TransformVector(new Vector3(0F, -localStep, 0F)).y;
                if (Mathf.Abs(stepY) < 0.0001F) stepY = localStep;
            }

            if (Mathf.Abs(stepX) < 0.0001F || Mathf.Abs(stepY) < 0.0001F) return false;

            var centerOffsetX = (shape.BoundingWidth - 1) * 0.5F;
            var centerOffsetY = (shape.BoundingHeight - 1) * 0.5F;

            var originX = Mathf.FloorToInt((worldPosition.x - p00.x) / stepX - centerOffsetX + 0.5F);
            var originY = Mathf.FloorToInt((p00.y - worldPosition.y) / stepY - centerOffsetY + 0.5F);

            foreach (var offset in shape.Coordinates)
            {
                var tx = originX + offset.X;
                var ty = originY + offset.Y;
                if (tx < 0 || tx >= _width || ty < 0 || ty >= _height)
                    return false;
            }

            origin = new BoardCoordinate(originX, originY);
            return true;
        }

        public bool TryGetCoordinateAtScreenPosition(Vector2 screenPosition, Camera eventCamera,
            out BoardCoordinate coordinate)
        {
            coordinate = default;
            if (_cells is null || _width == 0 || _height == 0) return false;

            for (var x = 0; x < _width; x++)
            {
                for (var y = 0; y < _height; y++)
                {
                    var cell = _cells[x, y];
                    if (cell != null && cell.transform is RectTransform rect &&
                        RectTransformUtility.RectangleContainsScreenPoint(rect, screenPosition, eventCamera))
                    {
                        coordinate = new BoardCoordinate(x, y);
                        return true;
                    }
                }
            }

            var minDistanceSq = float.MaxValue;
            var found = false;
            var bestCoord = default(BoardCoordinate);

            for (var x = 0; x < _width; x++)
            {
                for (var y = 0; y < _height; y++)
                {
                    var cell = _cells[x, y];
                    if (cell != null)
                    {
                        var cellScreenPos =
                            RectTransformUtility.WorldToScreenPoint(eventCamera, cell.transform.position);
                        var distSq = (cellScreenPos - screenPosition).sqrMagnitude;
                        if (distSq < minDistanceSq)
                        {
                            minDistanceSq = distSq;
                            bestCoord = new BoardCoordinate(x, y);
                            found = true;
                        }
                    }
                }
            }

            if (!found || _cells[0, 0] == null || _cells[0, 0].transform is not RectTransform sampleRect) return false;
            var snapRadius = Mathf.Max(sampleRect.rect.width, sampleRect.rect.height) * 0.95F;
            if (!(minDistanceSq <= snapRadius * snapRadius)) return false;
            coordinate = bestCoord;
            return true;
        }

        private void SetupLayout(int width, int height)
        {
            if (_layoutGroup == null) return;

            _layoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            _layoutGroup.constraintCount = width;

            var cellSize = _layoutGroup.cellSize;
            if (cellSize.x <= 0F || cellSize.y <= 0F)
            {
                cellSize = new Vector2(100F, 100F);
                _layoutGroup.cellSize = cellSize;
            }

            var spacing = _layoutGroup.spacing;
            var padding = _layoutGroup.padding;

            var totalWidth = (width * cellSize.x) + (Mathf.Max(0, width - 1) * spacing.x) +
                             (padding?.horizontal ?? 0);
            var totalHeight = (height * cellSize.y) + (Mathf.Max(0, height - 1) * spacing.y) +
                              (padding?.vertical ?? 0);
            var targetSize = new Vector2(totalWidth, totalHeight);

            if (transform is RectTransform boardRt && boardRt != _gridContainer)
                boardRt.sizeDelta = targetSize;

            if (_gridContainer == null) return;
            if (_gridContainer.anchorMin != _gridContainer.anchorMax)
            {
                _gridContainer.offsetMin = Vector2.zero;
                _gridContainer.offsetMax = Vector2.zero;
                _gridContainer.anchoredPosition = Vector2.zero;
                _gridContainer.sizeDelta = Vector2.zero;
            }
            else
            {
                _gridContainer.sizeDelta = targetSize;
                _gridContainer.anchoredPosition = Vector2.zero;
            }
        }

        private void CreateCells(int width, int height)
        {
            _cells = new CellView[width, height];

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var cell = Instantiate(_cellPrefab, _gridContainer);
                    cell.name = $"Cell_{x}_{y}";
                    cell.Initialize();
                    _cells[x, y] = cell;
                }
            }
        }

        private void ClearExistingCells()
        {
            if (_cells != null)
            {
                foreach (var c in _cells)
                    if (c != null)
                        Destroy(c.gameObject);
                _cells = null;
            }

            if (_gridContainer == null) return;
            var children = new List<GameObject>();
            foreach (Transform child in _gridContainer)
                children.Add(child.gameObject);
            foreach (var child in children)
                if (child != null)
                    Destroy(child);
        }

        private bool TryGetCell(int x, int y, out CellView cell)
        {
            if (_cells is null || x < 0 || x >= _width || y < 0 || y >= _height)
            {
                cell = null;
                return false;
            }

            cell = _cells[x, y];
            return cell != null;
        }

        private bool TryGetCell(BoardCoordinate coord, out CellView cell) => TryGetCell(coord.X, coord.Y, out cell);
    }
}