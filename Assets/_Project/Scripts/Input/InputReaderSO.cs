using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Action002.Input
{
    [CreateAssetMenu(fileName = "InputReader", menuName = "Action002/Input/Input Reader")]
    public class InputReaderSO : ScriptableObject, InputSystem_Actions.IPlayerActions
    {
        public event Action<Vector2> OnMoveEvent;
        public event Action OnSwitchPolarityEvent;

        private InputSystem_Actions inputActions;

        private void OnEnable()
        {
            if (inputActions == null)
            {
                inputActions = new InputSystem_Actions();
                inputActions.Player.SetCallbacks(this);
            }
        }

        private void OnDisable()
        {
            inputActions?.Player.Disable();
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            OnMoveEvent?.Invoke(context.ReadValue<Vector2>());
        }

        public void OnSwitchPolarity(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
                OnSwitchPolarityEvent?.Invoke();
        }

        public void EnablePlayerInput()
        {
            if (inputActions == null)
            {
                inputActions = new InputSystem_Actions();
                inputActions.Player.SetCallbacks(this);
            }
            inputActions.Player.Enable();
        }

        public void DisablePlayerInput()
        {
            if (inputActions != null)
            {
                inputActions.Player.Disable();
            }
        }
    }
}
