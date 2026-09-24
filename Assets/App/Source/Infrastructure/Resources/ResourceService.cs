using System;
using System.Collections.Generic;
using System.Threading;
using App.Application.Ports;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace App.Infrastructure.Resources
{
    public sealed class ResourceService : IResourceService, IDisposable
    {
        private readonly ILogService _logger;
        private readonly List<AsyncOperationHandle> _assetHandles = new();
        private bool _isInitialized;
        private bool _isDisposed;

        public ResourceService(ILogService logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async UniTask InitializeAsync(CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (_isInitialized) return;

            var handle = Addressables.InitializeAsync(autoReleaseHandle: false);
            try
            {
                await handle.Task.AsUniTask().AttachExternalCancellation(ct);
                ct.ThrowIfCancellationRequested();
                ThrowIfDisposed();
                if (handle.Status != AsyncOperationStatus.Succeeded)
                    throw handle.OperationException ??
                          new InvalidOperationException("Addressables initialization failed.");
                _isInitialized = true;
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                _logger.LogWarning($"Resource initialization failed: {ex.Message}");
                throw;
            }
            finally
            {
                if (handle.IsValid()) Addressables.Release(handle);
            }
        }

        public async UniTask<IReadOnlyList<T>> LoadAssetsByLabelAsync<T>(string label, CancellationToken ct = default)
            where T : class
        {
            ct.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (string.IsNullOrEmpty(label)) throw new ArgumentException("Label must not be empty.", nameof(label));

            var handle = Addressables.LoadAssetsAsync<T>(label, null);
            var retained = false;
            try
            {
                var listResult = await handle.Task.AsUniTask().AttachExternalCancellation(ct);
                ct.ThrowIfCancellationRequested();
                ThrowIfDisposed();
                if (handle.Status != AsyncOperationStatus.Succeeded)
                    throw handle.OperationException ??
                          new InvalidOperationException($"Failed to load label '{label}'.");
                var result = new List<T>(listResult);
                if (result.Count > 0)
                {
                    _assetHandles.Add(handle);
                    retained = true;
                }

                return result;
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                _logger.LogWarning($"Could not load '{label}': {ex.Message}");
                throw;
            }
            finally
            {
                if (!retained && handle.IsValid()) Addressables.Release(handle);
            }
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;
            foreach (var handle in _assetHandles)
                if (handle.IsValid())
                    Addressables.Release(handle);
            _assetHandles.Clear();
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed) throw new ObjectDisposedException(nameof(ResourceService));
        }
    }
}