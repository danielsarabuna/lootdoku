using System;
using UnityEngine;
using UnityEngine.UI;

namespace App.Presentation.GameplayScreen
{
    public sealed class TopBarView : MonoBehaviour
    {
        [SerializeField] private ScoreView _scoreView;
        [SerializeField] private Button _pauseButton;

        public ScoreView ScoreView => _scoreView;

        public void Bind(Action onPauseClicked)
        {
            if (_pauseButton == null) return;
            _pauseButton.onClick.RemoveAllListeners();
            _pauseButton.onClick.AddListener(() => onPauseClicked?.Invoke());
        }
    }
}