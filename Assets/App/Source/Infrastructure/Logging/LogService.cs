using App.Application.Ports;
using UnityEngine;

namespace App.Infrastructure.Logging
{
    public sealed class LogService : ILogService
    {
        public void Log(string message) => Debug.Log(message);

        public void LogWarning(string message) => Debug.LogWarning(message);

        public void LogError(string message) => Debug.LogError(message);
    }
}