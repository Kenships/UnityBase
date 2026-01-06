using System;
using _Project.Scripts.Core.SoundPooling.ScriptableObject;
using Sisus.Init;
using UnityEngine;

namespace _Project.Scripts.Core.SoundPooling
{
    public class VolumeInitializer : MonoBehaviour<VolumeSO>

    {
        private VolumeSO _volume;
        protected override void Init(VolumeSO playerReader)
        {
            _volume = playerReader;
        }

        private void Start()
        {
            _volume.InitializeMixer();
        }
    }
}
