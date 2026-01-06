using System;
using _Project.Scripts.Core.InputManagement.Interfaces;
using Sisus.Init;
using UnityEngine;

namespace _Sample.Scripts
{
    [Service(typeof(MouseTrackingService))]
    public class MouseTrackingService : MonoBehaviour<IPlayerReader>
    {
        [SerializeField] LayerMask layer;

        private Vector2 _mousePosition;
        private IPlayerReader _playerReader;
        
        protected override void Init(IPlayerReader playerReader)
        {
            _playerReader = playerReader;
        }

        protected override void OnAwake()
        {
            _playerReader.OnPlayerPointEvent += OnPlayerPoint;
        }

        private void OnDestroy()
        {
            _playerReader.OnPlayerPointEvent -= OnPlayerPoint;
        }

        private void OnPlayerPoint(Vector2 position)
        {
            _mousePosition = position;
        }

        public bool TryGetMouseWallHit(out Vector3 hitPoint)
        {
            Ray ray = Camera.main!.ScreenPointToRay(_mousePosition);
            
            bool isHit = Physics.Raycast(ray.origin, ray.direction, out RaycastHit hit, Mathf.Infinity);
            
            if (isHit)
            {
                hitPoint = hit.point;
                return true;
            }

            hitPoint = Vector3.zero;
            return false;
        }

        
    }
}
