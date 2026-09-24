using System.Threading;
using Cysharp.Threading.Tasks;

namespace App.Application.Ports
{
    public enum AudioSfxType
    {
        Take,
        Rotate,
        Place,
        DestroyCells
    }

    public interface IAudioService
    {
        void PlayMusic();
        void SetPauseDucking(bool isPaused);
        void PlaySfx(AudioSfxType sfxType);
        UniTask InitializeAsync(CancellationToken ct = default);
    }
}