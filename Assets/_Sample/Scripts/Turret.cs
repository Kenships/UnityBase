using System;
using _Project.Scripts.Core.SceneLoading.Interfaces;
using _Project.Scripts.Core.SoundPooling;
using _Project.Scripts.Util.Timer.Timers;
using Sisus.Init;
using UnityEngine;
using UnityEngine.InputSystem;
using AudioType = _Project.Scripts.Core.SoundPooling.Interface.AudioType;

namespace _Sample.Scripts
{
    public class Turret : MonoBehaviour<AudioPooler, MouseTrackingService, ISceneFocusRetrieval>
    {
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private Transform bulletSpawnPoint;
        [SerializeField] private float shootingSpeed;
        [SerializeField] private AudioClip bulletSound;
        private AudioPooler _audioPooler;
        private MouseTrackingService _mouseTrackingService;
        private ISceneFocusRetrieval _sceneFocusRetrieval;
        private CountdownTimer _cooldownTimer;

        protected override void Init(AudioPooler audioPooler, MouseTrackingService mouseTrackingService,
            ISceneFocusRetrieval sceneFocusRetrieval)
        {
            _audioPooler = audioPooler;
            _mouseTrackingService = mouseTrackingService;
            _sceneFocusRetrieval = sceneFocusRetrieval;
        }

        protected override void OnAwake()
        {
            _cooldownTimer = new CountdownTimer(1f / shootingSpeed);
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
            if (Mouse.current.leftButton.isPressed && !_cooldownTimer.IsRunning &&
                _sceneFocusRetrieval.IsFocused(gameObject.scene.buildIndex))
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
