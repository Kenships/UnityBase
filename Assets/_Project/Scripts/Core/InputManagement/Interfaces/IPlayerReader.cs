using UnityEngine;
using UnityEngine.Events;

namespace _Project.Scripts.Core.InputManagement.Interfaces
{
    public interface IPlayerReader
    {
        public event UnityAction<Vector2> OnMoveEvent;
        public event UnityAction<Vector2> OnLookEvent;
        public event UnityAction OnAttackEvent;
        public event UnityAction OnInteractEvent;
        public event UnityAction OnCrouchEvent;
        public event UnityAction OnJumpEvent;
        public event UnityAction OnPreviousEvent;
        public event UnityAction OnNextEvent;
        public event UnityAction<bool> OnSprintEvent;
    }
}
