using Obvious.Soap;
using UnityEngine;

namespace _Project.Scripts.Core.SoundPooling.ScriptableObject
{
    [CreateAssetMenu(fileName = "VolumeSO", menuName = "Scriptable Objects/Audio/VolumeSO")]
    public class VolumeSO : UnityEngine.ScriptableObject
    {
        public BoolVariable mute;
        public FloatVariable masterVolume;
        public FloatVariable musicVolume;
        public FloatVariable sfxVolume;
    }
}
