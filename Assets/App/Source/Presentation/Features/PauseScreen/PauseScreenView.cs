using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace App.Presentation.PauseScreen
{
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class PauseScreenView : MonoBehaviour
    {
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _restartButton;

        [SerializeField] private CanvasGroup _canvasGroup;
        private Tween _fadeTween;


        public void BindButtons(Action onResume, Action onRestart)
        {
            _resumeButton.onClick.RemoveAllListeners();
            _resumeButton.onClick.AddListener(() => onResume?.Invoke());

            _restartButton.onClick.RemoveAllListeners();
            _restartButton.onClick.AddListener(() => onRestart?.Invoke());
        }

        public void Show(float duration = 0.25F, Action onComplete = null)
        {
            _fadeTween?.Kill();
            var cg = _canvasGroup;
            cg.blocksRaycasts = true;
            cg.interactable = true;
            if (duration <= 0F)
            {
                cg.alpha = 1F;
                onComplete?.Invoke();
            }
            else _fadeTween = cg.DOFade(1F, duration).SetUpdate(true).OnComplete(() => onComplete?.Invoke());
        }

        public void Hide(float duration = 0.25F, Action onComplete = null)
        {
            _fadeTween?.Kill();
            var cg = _canvasGroup;
            cg.interactable = false;
            cg.blocksRaycasts = false;
            if (duration <= 0F)
            {
                cg.alpha = 0F;
                onComplete?.Invoke();
            }
            else _fadeTween = cg.DOFade(0F, duration).SetUpdate(true).OnComplete(() => onComplete?.Invoke());
        }

        private void Awake()
        {
            _canvasGroup.alpha = 0F;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        private void OnDestroy() => _fadeTween?.Kill();
    }
}