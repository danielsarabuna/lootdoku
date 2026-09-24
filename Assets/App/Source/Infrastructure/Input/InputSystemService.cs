using System;
using App.Application.Ports;
using UnityEngine.InputSystem;
using VContainer.Unity;

namespace App.Infrastructure.Input
{
    public sealed class InputSystemService : IInputService, IInitializable, IDisposable
    {
        public event Action OnRotateHotkeyTriggered;
        public event Action OnPauseHotkeyTriggered;

        private readonly InputAction _rotateAction;
        private readonly InputAction _pauseAction;
        private bool _isEnabled;
        private bool _isDisposed;

        public InputSystemService()
        {
            _rotateAction = new InputAction("RotateHotkey", InputActionType.Button, "<Keyboard>/r");
            _rotateAction.performed += OnRotatePerformed;

            _pauseAction = new InputAction("PauseHotkey", InputActionType.Button, "<Keyboard>/escape");
            _pauseAction.performed += OnPausePerformed;

            Enable();
        }

        public void Initialize() => Enable();

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            Disable();

            if (_rotateAction != null)
            {
                _rotateAction.performed -= OnRotatePerformed;
                _rotateAction.Dispose();
            }

            if (_pauseAction != null)
            {
                _pauseAction.performed -= OnPausePerformed;
                _pauseAction.Dispose();
            }
        }

        private void Enable()
        {
            if (_isEnabled || _isDisposed) return;
            _rotateAction?.Enable();
            _pauseAction?.Enable();
            _isEnabled = true;
        }

        private void Disable()
        {
            if (!_isEnabled || _isDisposed) return;
            _rotateAction?.Disable();
            _pauseAction?.Disable();
            _isEnabled = false;
        }

        private void OnRotatePerformed(InputAction.CallbackContext context) => OnRotateHotkeyTriggered?.Invoke();

        private void OnPausePerformed(InputAction.CallbackContext context) => OnPauseHotkeyTriggered?.Invoke();
    }
}