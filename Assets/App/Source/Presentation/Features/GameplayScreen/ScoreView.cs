using DG.Tweening;
using TMPro;
using UnityEngine;

namespace App.Presentation.GameplayScreen
{
    public sealed class ScoreView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _currentScoreText;
        [SerializeField] private TMP_Text _bestScoreText;

        private int _displayedScore;
        private Tween _scoreTween;

        public void SetScore(int currentScore, int bestScore, bool animate = true)
        {
            if (_bestScoreText != null)
            {
                _bestScoreText.text = $"BEST: {bestScore}";
            }

            if (_currentScoreText == null) return;

            _scoreTween?.Kill();

            if (animate && _displayedScore != currentScore)
            {
                var from = _displayedScore;
                _scoreTween = DOTween.To(() => from, x =>
                {
                    from = x;
                    _currentScoreText.text = $"Score: {x:D6}";
                }, currentScore, 0.3F).OnComplete(() =>
                {
                    _displayedScore = currentScore;
                    _currentScoreText.text = $"Score: {currentScore:D6}";
                });
            }
            else
            {
                _displayedScore = currentScore;
                _currentScoreText.text = $"Score: {currentScore:D6}";
            }
        }

        private void OnDestroy()
        {
            _scoreTween?.Kill();
        }
    }
}