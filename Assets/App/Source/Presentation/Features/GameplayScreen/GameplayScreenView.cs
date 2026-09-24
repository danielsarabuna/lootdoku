using System;
using App.Presentation.Board;
using App.Presentation.FigureTray;
using DG.Tweening;
using UnityEngine;

namespace App.Presentation.GameplayScreen
{
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class GameplayScreenView : MonoBehaviour
    {
        [SerializeField] private BoardView _boardView;
        [SerializeField] private FigureTrayView _figureTrayView;
        [SerializeField] private TopBarView _topBarView;

        private CanvasGroup _canvasGroup;
        private Tween _fadeTween;

        public BoardView BoardView => _boardView ??= GetComponentInChildren<BoardView>(true);
        public FigureTrayView FigureTrayView => _figureTrayView ??= GetComponentInChildren<FigureTrayView>(true);
        public TopBarView TopBarView => _topBarView ??= GetComponentInChildren<TopBarView>(true);

        private CanvasGroup Group => _canvasGroup ??= GetComponent<CanvasGroup>();

        public void Show(float duration = 0.3F, Action onComplete = null)
        {
            _fadeTween?.Kill();
            var cg = Group;
            cg.blocksRaycasts = true;
            cg.interactable = true;
            if (duration <= 0F)
            {
                cg.alpha = 1F;
                onComplete?.Invoke();
            }
            else
            {
                _fadeTween = cg.DOFade(1F, duration).SetUpdate(true).OnComplete(() => onComplete?.Invoke());
            }
        }

        private void Awake() => _canvasGroup = GetComponent<CanvasGroup>();

        private void OnDestroy() => _fadeTween?.Kill();
    }
}