using System;
using System.Collections.Generic;
using System.Threading;
using App.Application.Ports;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using Object = UnityEngine.Object;

namespace App.Infrastructure.Audio
{
    public sealed class AudioService : IAudioService, IDisposable
    {
        private GameObject _audioRoot;
        private AudioSource _bgmSource;
        private AudioSourcePool _sfxPool;

        private AudioClip _bgmClip;
        private readonly List<AudioClip> _musicClips = new();
        private AudioClip _sfxTake;
        private AudioClip _sfxRotate;
        private AudioClip _sfxPlace;
        private AudioClip _sfxDestroyCells;

        private readonly float _normalBgmVolume = 0.7F;
        private readonly float _duckedBgmVolume = 0.2F;
        private readonly float _duckDuration = 0.3F;

        private Tweener _fadeTween;
        private readonly IResourceService _resourceService;
        private readonly ILogService _logger;
        private bool _isDisposed;

        public AudioService(GameObject parent, IResourceService resourceService, ILogService logger)
        {
            if (parent is null)
                throw new ArgumentNullException(nameof(parent), "AudioService requires a parent GameObject.");
            _resourceService = resourceService;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            SetupGameObjectHierarchy(parent.transform);
        }

        public async UniTask InitializeAsync(CancellationToken ct = default) => await LoadAudioResourcesAsync(ct);

        public void PlayMusic()
        {
            if (_bgmSource is null) return;

            if (_fadeTween != null && _fadeTween.IsActive())
            {
                _fadeTween.Kill();
                _fadeTween = null;
            }

            DOTween.Kill(_bgmSource);

            _bgmSource.volume = _normalBgmVolume;

            if (_bgmClip is null) return;

            if (_bgmSource.clip != _bgmClip)
                _bgmSource.clip = _bgmClip;

            if (!_bgmSource.isPlaying)
                _bgmSource.Play();
        }

        public void PlaySfx(AudioSfxType type)
        {
            var clip = GetSfxClip(type);
            if (clip is not null)
                _sfxPool?.PlayOneShot(clip);
        }

        public void SetPauseDucking(bool isPaused)
        {
            if (_bgmSource is null) return;

            var targetVolume = isPaused ? _duckedBgmVolume : _normalBgmVolume;

            if (_fadeTween != null && _fadeTween.IsActive())
            {
                _fadeTween.Kill();
                _fadeTween = null;
            }

            DOTween.Kill(_bgmSource);

            _fadeTween = _bgmSource.DOFade(targetVolume, _duckDuration)
                .SetTarget(_bgmSource)
                .SetUpdate(true);
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            if (_fadeTween != null && _fadeTween.IsActive())
            {
                _fadeTween.Kill();
                _fadeTween = null;
            }

            if (_bgmSource != null)
                DOTween.Kill(_bgmSource);

            _sfxPool?.Dispose();
            _sfxPool = null;

            if (_audioRoot == null) return;
            Object.Destroy(_audioRoot);
            _audioRoot = null;
        }

        private void SetupGameObjectHierarchy(Transform parent)
        {
            _audioRoot = new GameObject("AudioService");
            _audioRoot.transform.SetParent(parent, false);

            var bgmGo = new GameObject("BGM_Source");
            bgmGo.transform.SetParent(_audioRoot.transform, false);
            _bgmSource = bgmGo.AddComponent<AudioSource>();
            _bgmSource.playOnAwake = false;
            _bgmSource.loop = true;

            _sfxPool = new AudioSourcePool(_audioRoot.transform, initialSize: 4);
        }

        private async UniTask LoadAudioResourcesAsync(CancellationToken ct = default)
        {
            if (_resourceService is null) return;

            try
            {
                var musicClips = await _resourceService.LoadAssetsByLabelAsync<AudioClip>("Music", ct);
                if (musicClips != null)
                {
                    _musicClips.Clear();
                    _musicClips.AddRange(musicClips);
                    if (_musicClips.Count > 0)
                        _bgmClip = _musicClips[0];
                }

                var sfxClips = await _resourceService.LoadAssetsByLabelAsync<AudioClip>("SFX", ct);
                if (sfxClips != null)
                {
                    foreach (var clip in sfxClips)
                    {
                        if (clip is null) continue;
                        var name = clip.name.ToLowerInvariant();
                        if (name.Contains("take")) _sfxTake = clip;
                        else if (name.Contains("rotate")) _sfxRotate = clip;
                        else if (name.Contains("place")) _sfxPlace = clip;
                        else if (name.Contains("destroy") || name.Contains("clear")) _sfxDestroyCells = clip;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Could not load audio: {ex.Message}");
            }
        }

        private AudioClip GetSfxClip(AudioSfxType type) => type switch
        {
            AudioSfxType.Take => _sfxTake,
            AudioSfxType.Rotate => _sfxRotate,
            AudioSfxType.Place => _sfxPlace,
            AudioSfxType.DestroyCells => _sfxDestroyCells,
            _ => null
        };
    }
}