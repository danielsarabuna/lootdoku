using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace App.Infrastructure.Audio
{
    public sealed class AudioSourcePool : IDisposable
    {
        private readonly Transform _parent;
        private readonly List<AudioSource> _pool = new();
        private readonly int _initialSize;

        public AudioSourcePool(Transform parent, int initialSize)
        {
            _parent = parent ?? throw new ArgumentNullException(nameof(parent));
            _initialSize = Mathf.Max(1, initialSize);
            Prewarm();
        }

        public void PlayOneShot(AudioClip clip)
        {
            if (clip is null) return;
            var src = Rent();
            if (src != null)
                src.PlayOneShot(clip);
        }

        public void Dispose()
        {
            for (var i = 0; i < _pool.Count; i++)
                if (_pool[i] != null && _pool[i].gameObject != null)
                    Object.Destroy(_pool[i].gameObject);
            _pool.Clear();
        }

        private void Prewarm()
        {
            if (_parent is null) return;

            for (var i = 0; i < _initialSize; i++)
                CreateNewSource();
        }

        private AudioSource Rent()
        {
            for (var i = 0; i < _pool.Count; i++)
            {
                var src = _pool[i];
                if (src != null && !src.isPlaying)
                    return src;
            }

            return CreateNewSource();
        }

        private AudioSource CreateNewSource()
        {
            if (_parent is null) return null;

            var go = new GameObject($"AudioSource_Pooled_{_pool.Count}");
            go.transform.SetParent(_parent, false);
            var src = go.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.loop = false;
            _pool.Add(src);
            return src;
        }
    }
}