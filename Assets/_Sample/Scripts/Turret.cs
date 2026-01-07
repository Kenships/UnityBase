using _Project.Scripts.Core.InputManagement.Interfaces;
using _Project.Scripts.Core.SoundPooling;
using _Project.Scripts.Util.Timer.Timers;
using Sisus.Init;
using UnityEngine;
using AudioType = _Project.Scripts.Core.SoundPooling.Interface.AudioType;

namespace _Sample.Scripts
{
    public class Turret : MonoBehaviour<AudioPooler, IPlayerReader, MouseTrackingService>
    {
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private Transform bulletSpawnPoint;
        [SerializeField] private float shootingSpeed;
        [SerializeField] private AudioClip bulletSound;
        private AudioPooler _audioPooler;
        private IPlayerReader _playerReader;
        private MouseTrackingService _mouseTrackingService;
        private CountdownTimer _cooldownTimer;
        private bool _isAttacking;

        protected override void Init(AudioPooler audioPooler, IPlayerReader playerReader, MouseTrackingService mouseTrackingService)
        {
            _audioPooler = audioPooler;
            _playerReader = playerReader;
            _mouseTrackingService = mouseTrackingService;
        }

        protected override void OnAwake()
        {
            _cooldownTimer = new CountdownTimer(1f / shootingSpeed);
            _playerReader.OnAttackEvent += PlayerReaderOnAttackEvent;
            _playerReader.OnPlayerEnableEvent += PlayerReaderOnPlayerEnableEvent;
        }

        private void OnDestroy()
        {
            _playerReader.OnAttackEvent -= PlayerReaderOnAttackEvent;
            _playerReader.OnPlayerEnableEvent -= PlayerReaderOnPlayerEnableEvent;
        }

        private void PlayerReaderOnPlayerEnableEvent(bool isEnabled)
        {
            if (isEnabled)
            {
                _isAttacking = false;
            }
        }

        private void PlayerReaderOnAttackEvent(bool performed)
        {
            _isAttacking = performed;
        }

        private void FixedUpdate()
        {
            if (_mouseTrackingService.TryGetMouseWallHit(out Vector3 hitPoint))
            {
                transform.LookAt(hitPoint);
            }
        }

        private void Update()
        {
            if (_isAttacking && !_cooldownTimer.IsRunning)
            {
                _cooldownTimer.Reset();
                _cooldownTimer.Start();
                Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
                _audioPooler
                    .New3DAudio(bulletSound)
                    .OnChannel(AudioType.Sfx)
                    .AtPosition(bulletSpawnPoint.position)
                    .MarkFrequent()
                    .RandomizePitch()
                    .Play();
            }
        }
    }
}
