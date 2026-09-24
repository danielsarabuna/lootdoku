using System;
using System.Collections.Generic;
using App.Domain.Score;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace App.Presentation.GameOverScreen
{
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class GameOverScreenView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _finalScoreText;
        [SerializeField] private TMP_Text _top1Text;
        [SerializeField] private TMP_Text _top2Text;
        [SerializeField] private TMP_Text _top3Text;
        [SerializeField] private Button _restartButton;

        private CanvasGroup _canvasGroup;
        private Tween _fadeTween;

        private CanvasGroup Group => _canvasGroup ??= GetComponent<CanvasGroup>();

        public void BindRestartButton(Action onRestart)
        {
            if (_restartButton == null) return;
            _restartButton.onClick.RemoveAllListeners();
            _restartButton.onClick.AddListener(() => onRestart?.Invoke());
        }

        public void DisplayResults(int finalScore, IReadOnlyList<HighScoreEntry> topScores)
        {
            if (_finalScoreText != null) _finalScoreText.text = $"FINAL SCORE\n{finalScore}";

            var t1 = topScores is { Count: > 0 } ? topScores[0].score.ToString() : "-";
            var t2 = topScores is { Count: > 1 } ? topScores[1].score.ToString() : "-";
            var t3 = topScores is { Count: > 2 } ? topScores[2].score.ToString() : "-";

            if (_top1Text != null) _top1Text.text = $"#1  {t1}";
            if (_top2Text != null) _top2Text.text = $"#2  {t2}";
            if (_top3Text != null) _top3Text.text = $"#3  {t3}";
        }

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
            else _fadeTween = cg.DOFade(1F, duration).SetUpdate(true).OnComplete(() => onComplete?.Invoke());
        }

        public void Hide(float duration = 0.2F, Action onComplete = null)
        {
            _fadeTween?.Kill();
            var cg = Group;
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
            _canvasGroup = GetComponent<CanvasGroup>();
            Group.alpha = 0F;
            Group.interactable = false;
            Group.blocksRaycasts = false;
        }

        private void OnDestroy() => _fadeTween?.Kill();
    }
}