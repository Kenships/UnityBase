using System;
using _Project.Scripts.Core.InputManagement.Interfaces;
using Sisus.Init;
using UnityEngine;

namespace _Project.Scripts.Core.InputManagement.ScriptableObjects
{
    [Service(typeof(IPlayerReader), typeof(IUIReader), typeof(IOverrideReader), ResourcePath = "ScriptableObjects/Input/InputReaderSO")]
    [CreateAssetMenu(fileName = "InputReaderSO", menuName = "Scriptable Objects/Input/InputReaderSO")]
    public partial class InputReaderSO  : ScriptableObject
    {
        private InputSystemActions _inputSystemActions;
        
        private void OnEnable()
        {
            _inputSystemActions ??= new InputSystemActions();
            _inputSystemActions.UI.SetCallbacks(this);
            _inputSystemActions.Player.SetCallbacks(this);
            _inputSystemActions.Override.SetCallbacks(this);
            _inputSystemActions.Enable();
        }
        
        private void OnDisable()
        {
            _inputSystemActions.Disable();
        }
    }
}
