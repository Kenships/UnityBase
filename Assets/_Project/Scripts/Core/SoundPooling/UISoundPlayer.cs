using _Project.Scripts.Core.SoundPooling;
using Sisus.Init;
using UnityEngine;
using AudioType = _Project.Scripts.Core.SoundPooling.Interface.AudioType;

public class UISoundPlayer : MonoBehaviour<AudioPooler>
{
    private AudioPooler _audioPooler;
    protected override void Init(AudioPooler playerReader)
    {
        _audioPooler = playerReader;
    }
    
    public void PlaySound(AudioClip clip)
    {
        _audioPooler.New2DAudio(clip)
            .OnChannel(AudioType.UI)
            .RandomizePitch()
            .Play();
    }
}
