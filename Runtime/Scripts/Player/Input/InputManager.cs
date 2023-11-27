using System;
using UnityEngine.InputSystem;

namespace AQM.Tools
{
    public enum PlayerDevices
    {
        Keyboard,
        Controller
    }
    
    public class InputManager : Singleton<InputManager>
    {
        private InputDevice _lastDevice;
        private PlayerInput _playerInput;
        private PlayerDevices CurrentDevice { get; set; }

        public InputActions playerInputActions;

        private void Awake()
        {
            playerInputActions = new InputActions();

            _playerInput = FindObjectOfType<PlayerInput>();
            _playerInput.defaultActionMap = playerInputActions.Player.Get().name;
            _playerInput.actions = playerInputActions.asset;
        }

        private void Start()
        {
            ToogleActionMap(playerInputActions.Player);
            TriggerEvent(_playerInput.currentControlScheme);
        }

        public static event Action<InputActionMap> ActionMapChange;

        public void ToogleActionMap(InputActionMap actionMap)
        {
            if (actionMap.enabled) return;

            playerInputActions.Disable();
            _playerInput.currentActionMap = actionMap;
            ActionMapChange?.Invoke(actionMap);
            actionMap.Enable();
        }
        
        private void TriggerEvent(string newDevice)
        {
            if (newDevice.Equals("Gamepad"))
            {
                CurrentDevice = PlayerDevices.Controller;
            }
            else if (newDevice.Contains("Keyboard & Mouse"))
            {
                CurrentDevice = PlayerDevices.Keyboard;
            }
        }
    }
}