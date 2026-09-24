using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace App.Presentation.LoadingScreen
{
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class LoadingScreenView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _statusText;

        [SerializeField] private CanvasGroup _canvasGroup;
        private Sequence _fadeSequence;

        public void SetProgress(string status)
        {
            if (!string.IsNullOrEmpty(status)) SetStatus(status);
        }

        public void ShowInstant()
        {
            KillTransitions();
            gameObject.SetActive(true);

            _statusText.alpha = 1F;

            var cg = _canvasGroup;
            cg.alpha = 1F;
            cg.blocksRaycasts = true;
            cg.interactable = true;
        }

        public void HideTwoStep(float progressFadeDuration = 0.3F, float screenFadeDuration = 0.5F,
            Action onComplete = null)
        {
            KillTransitions();

            var cg = _canvasGroup;
            cg.interactable = false;
            cg.blocksRaycasts = false;

            if (progressFadeDuration <= 0F && screenFadeDuration <= 0F)
            {
                _statusText.alpha = 0F;
                cg.alpha = 0F;
                gameObject.SetActive(false);
                onComplete?.Invoke();
                return;
            }

            _fadeSequence = DOTween.Sequence().SetUpdate(true);

            _fadeSequence.Append(_statusText.DOFade(0F, progressFadeDuration));

            _fadeSequence.Append(cg.DOFade(0F, screenFadeDuration));
            _fadeSequence.OnComplete(() =>
            {
                gameObject.SetActive(false);
                onComplete?.Invoke();
            });
        }

        private void SetStatus(string message)
        {
            _statusText.text = message;
        }

        private void KillTransitions()
        {
            _fadeSequence?.Kill();
        }

        private void OnDestroy() => KillTransitions();
    }
}