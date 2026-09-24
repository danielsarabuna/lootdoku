using System;

namespace App.Application.Ports
{
    public interface IInputService
    {
        event Action OnRotateHotkeyTriggered;
        event Action OnPauseHotkeyTriggered;
    }
}