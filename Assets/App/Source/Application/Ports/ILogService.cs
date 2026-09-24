namespace App.Application.Ports
{
    public interface ILogService
    {
        void Log(string message);
        void LogWarning(string message);
        void LogError(string message);
    }
}