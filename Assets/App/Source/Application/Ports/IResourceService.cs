using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace App.Application.Ports
{
    public interface IResourceService
    {
        UniTask InitializeAsync(CancellationToken ct = default);

        UniTask<IReadOnlyList<T>> LoadAssetsByLabelAsync<T>(string label, CancellationToken ct = default)
            where T : class;
    }
}