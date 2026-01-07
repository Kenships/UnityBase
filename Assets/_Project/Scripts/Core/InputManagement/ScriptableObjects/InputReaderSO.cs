using _Project.Scripts.Core.InputManagement.Interfaces;
using _Project.Scripts.Util.CustomAttributes;
using Sisus.Init;
using UnityEngine;

namespace _Project.Scripts.Core.InputManagement.ScriptableObjects
{
    [Service(typeof(IPlayerReader), typeof(IUIReader), typeof(IOverrideReader), typeof(IInputActionSetter), ResourcePath = "ScriptableObjects/Input/InputReaderSO")]
    [CreateAssetMenu(fileName = "InputReaderSO", menuName = "Scriptable Objects/Input/InputReaderSO")]
    public partial class InputReaderSO  : ScriptableObject, IInputActionSetter
    {
        private InputSystemActions _inputSystemActions;
        [SerializeField, ReadOnly] InputActionType activeActionType;
        
        private void OnEnable()
        {
            _inputSystemActions ??= new InputSystemActions();
            _inputSystemActions.UI.SetCallbacks(this);
            _inputSystemActions.Player.SetCallbacks(this);
            _inputSystemActions.Override.SetCallbacks(this);
            _inputSystemActions.Enable();
            
            SetAction(InputActionType.Default);
        }

        public void SetAction(InputActionType inputActionType)
        {
            activeActionType = inputActionType;
            switch (inputActionType)
            {
                case InputActionType.Player:
                    _inputSystemActions.Player.Enable();
                    _inputSystemActions.UI.Disable();
                    break;
                case InputActionType.UI:
                    _inputSystemActions.UI.Enable();
                    _inputSystemActions.Player.Disable();
                    break;
                case InputActionType.Disabled:
                    _inputSystemActions.UI.Disable();
                    _inputSystemActions.Player.Disable();
                    break;
                default:
                    _inputSystemActions.UI.Enable();
                    _inputSystemActions.Player.Enable();
                    break;
            }
        }
        
        private void OnDisable()
        {
            _inputSystemActions.Disable();
        }
    }
}
