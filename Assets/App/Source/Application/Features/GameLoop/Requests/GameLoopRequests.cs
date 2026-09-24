using System;

namespace App.Application.GameLoop.Requests
{
    public readonly struct InitializeGameRequest
    {
        public IProgress<string> Progress { get; }

        public InitializeGameRequest(IProgress<string> progress)
        {
            Progress = progress;
        }
    }

    public readonly struct InitializeGameResult
    {
        public bool Success { get; }
        public static InitializeGameResult Succeeded => new(true);
        public static InitializeGameResult Failed => new(false);

        private InitializeGameResult(bool success)
        {
            Success = success;
        }
    }

    public readonly struct StartNewGameRequest
    {
    }

    public readonly struct StartNewGameResult
    {
    }

    public readonly struct TogglePauseRequest
    {
    }

    public readonly struct TogglePauseResult
    {
    }

    public readonly struct RestoreGameRequest
    {
    }

    public readonly struct RestoreGameResult
    {
        public bool Restored { get; }
        public static RestoreGameResult Succeeded => new(true);
        public static RestoreGameResult Failed => new(false);

        private RestoreGameResult(bool restored)
        {
            Restored = restored;
        }
    }
}