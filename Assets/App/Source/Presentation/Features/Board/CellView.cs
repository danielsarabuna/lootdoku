using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace App.Presentation.Board
{
    public sealed class CellView : MonoBehaviour
    {
        [SerializeField] private Image _image;
        [SerializeField] private Sprite _emptySprite;
        [SerializeField] private Sprite _occupiedSprite;
        private Tween _animationTween;
        public bool IsOccupied { get; private set; }

        private Color EmptyNormalColor => new(0.92F, 0.85F, 0.75F, 0.16F);
        private Color OccupiedColor => Color.white;
        private Color ValidPreviewColor => new(1.0F, 1.0F, 1.0F, 0.65F);
        private Color InvalidPreviewColor => new(0.95F, 0.35F, 0.35F, 0.5F);

        public void Initialize()
        {
            SetEmpty();
        }

        public void SetEmpty()
        {
            IsOccupied = false;
            _animationTween?.Kill();
            transform.localScale = Vector3.one;
            _image.sprite = _emptySprite;
            _image.color = GetEmptyColor();
        }

        public void SetOccupied(bool animate = true)
        {
            IsOccupied = true;
            _animationTween?.Kill();

            var color = GetPaletteColor();
            if (_occupiedSprite != null && _image != null) _image.sprite = _occupiedSprite;
            if (_image != null) _image.color = color;

            transform.localScale = Vector3.one;
            if (animate)
                _animationTween = transform.DOPunchScale(Vector3.one * 0.25F, 0.2F, 8);
        }

        public void ShowPreview(bool isValid)
        {
            if (IsOccupied) return;
            if (_image == null) return;
            _image.sprite = isValid switch
            {
                true when _occupiedSprite != null => _occupiedSprite,
                false when _emptySprite != null => _emptySprite,
                _ => _image.sprite
            };

            _image.color = isValid ? ValidPreviewColor : InvalidPreviewColor;
        }

        public void ResetPreview()
        {
            if (IsOccupied) return;
            _image.sprite = _emptySprite;
            _image.color = GetEmptyColor();
        }

        public void PlayClearAnimation(Action onComplete = null)
        {
            _animationTween?.Kill();
            _animationTween = transform.DOScale(Vector3.zero, 0.25F)
                .SetEase(Ease.InBack)
                .OnComplete(() =>
                {
                    SetEmpty();
                    onComplete?.Invoke();
                });
        }

        private void Awake()
        {
            _image.raycastTarget = false;
            _emptySprite = _image.sprite;
        }

        private Color GetEmptyColor() => EmptyNormalColor;

        private Color GetPaletteColor() => OccupiedColor;

        private void OnDestroy() => _animationTween?.Kill();
    }
}