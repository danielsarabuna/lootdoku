using System;
using System.Collections.Generic;
using App.Domain.Board;
using App.Domain.Figures;
using UnityEngine;

namespace App.Infrastructure.Config.ScriptableObjects
{
    [CreateAssetMenu(fileName = "NewFigure", menuName = "Lootdoku/Config/Figure Data")]
    public sealed class FigureDataSO : ScriptableObject
    {
        public const int Dimension = 5;
        private const int CellCount = Dimension * Dimension;

        [SerializeField] private string _figureName = "Figure";
        [SerializeField] private int _colorIndex = 0;
        [SerializeField] private bool[] _cells = new bool[CellCount];

        public bool GetCell(int x, int y)
        {
            if (x < 0 || x >= Dimension || y < 0 || y >= Dimension) return false;
            EnsureCellsArray();
            return _cells[y * Dimension + x];
        }

        public void SetCell(int x, int y, bool value)
        {
            if (x < 0 || x >= Dimension || y < 0 || y >= Dimension) return;
            EnsureCellsArray();
            _cells[y * Dimension + x] = value;
        }

        public void ClearAll()
        {
            EnsureCellsArray();
            Array.Clear(_cells, 0, _cells.Length);
        }

        public void InvertAll()
        {
            EnsureCellsArray();
            for (var i = 0; i < _cells.Length; i++)
                _cells[i] = !_cells[i];
        }

        public Figure ToDomainFigure()
        {
            EnsureCellsArray();
            var activeCoords = new List<BoardCoordinate>();

            for (var y = 0; y < Dimension; y++)
            {
                for (var x = 0; x < Dimension; x++)
                    if (_cells[y * Dimension + x])
                        activeCoords.Add(new BoardCoordinate(x, y));
            }

            var shape = new FigureShape(activeCoords);
            return new Figure(name, _figureName, shape, RotationAngle.Deg0, _colorIndex);
        }

        private void EnsureCellsArray()
        {
            if (_cells is not { Length: CellCount })
            {
                var old = _cells;
                _cells = new bool[CellCount];
                if (old != null) Array.Copy(old, _cells, Math.Min(old.Length, CellCount));
            }
        }
    }
}