using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace App.Presentation.FigureTray
{
    public sealed class FigureSlotView : MonoBehaviour
    {
        [SerializeField] private RectTransform _figureContainer;
        [SerializeField] private Button _rotateButton;
        private int _slotIndex;
        private Action<int> _onRotateRequested;

        public RectTransform FigureContainer => _figureContainer;
        public DraggableFigureView CurrentFigureView { get; private set; }
        public bool HasFigure => CurrentFigureView != null;

        public void Initialize(int slotIndex, Action<int> onRotateRequested)
        {
            _slotIndex = slotIndex;
            _onRotateRequested = onRotateRequested;
            SetRotateButtonVisible(false);
        }

        public void SetFigureView(DraggableFigureView figureView)
        {
            CurrentFigureView = figureView;
            SetRotateButtonVisible(figureView != null);
        }

        public void ClearFigureView()
        {
            if (CurrentFigureView != null)
            {
                Destroy(CurrentFigureView.gameObject);
                CurrentFigureView = null;
            }

            if (_figureContainer != null)
            {
                var children = new List<GameObject>();
                foreach (Transform child in _figureContainer)
                    children.Add(child.gameObject);
                foreach (var child in children)
                    if (child != null)
                        Destroy(child);
            }

            SetRotateButtonVisible(false);
        }

        private void Awake()
        {
            _rotateButton.onClick.AddListener(() => _onRotateRequested?.Invoke(_slotIndex));
        }

        private void SetRotateButtonVisible(bool visible)
        {
            _rotateButton.gameObject.SetActive(visible);
        }
    }
}