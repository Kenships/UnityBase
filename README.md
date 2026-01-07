# Unity Base Project

# Features ⭐
- [Audio Pooler](#audio-pooler-)
- [Input Reader](#input-reader-)
- [Scene Controller](#scene-controller-)
- [Other Features](#other-features)

## Audio Pooler 🔊
Description:
A service used to manage and play audio from any class.

Usage: 

2D Audio
```c#
audioPooler
  .New2DAudio(clip)
  .OnChannel(AudioType.Sfx)
  .Play();
```
3D Audio

```c#
audioPooler
  .New2DAudio(clip)
  .OnChannel(AudioType.Sfx)
  .AtPosition(transform.position)
  .Play();
```

Features: 

An IAudioPlayer Object is returned after ```.Play();```.
This object can be used to pause, play, and stop the sound. It also has an additional volume fade feature.
```c#
public void FadeVolume(float volume, float duration = 0f, bool stopOnSilent = true){ ... }
```
There is also an OnAudioFinished callback for when the sound is stopped or finished.

All Config methods:
```
// All Config
.AddToScene(int sceneBuildIndex) // Associates audio to scene
.OnChannel(AudioType audioType) // to add to mixer group. Default value is sfx
.SetVolume(float volume)
.SetPitch(float pitch)
.RandomizePitch(float min = 0.05f, float max = 0.05f)
.LoopAudio()
.AddPriority(int priority) // Priority is used to determine which sound gets replaced when overflow occurs. Default value is 1
.MarkFrequent() // Priority set to 0

// 3D Additional Configs
.AtPosition(Vector3 position)
.WithMinDistance(float minDistance)
.WithMaxDistance(float maxDistance)
.BypassReverbZones()
```
Additional Features:

From ```AudioPooler : IAudioPoolSceneController``` you can call ```FadeAllVolumeFromScene(int sceneBuildIndex, float volume, float duration)``` to fade all audioSources tied to a scene

Limitations and future improvements: 
- There are currently AudioSource configurations that I did not implement; however, I believe I covered the most important ones, and adding additional ones is quite trivial
- An ```AudioPlayer : IAudioPlayer``` is the default object returned by ```.Play()```, and it is a pointer to an allocated AudioSource. If the AudioSource is stopped, the pointer is set to null, and the client of the AudioPlayer will no longer be controlling anything.

## Input Reader 🎮
Description: 
A service to notify clients of user input.

Usage:
Inject using Init(args) or add the created scriptable object as a reference, then subscribe accordingly to the events required.

example:
```c#
public class PauseListener : MonoBehaviour<IOverrideReader, ISceneFocusRetrieval>
{
    public UnityEvent onPauseEvent;
    private IOverrideReader _inputReader;
    private ISceneFocusRetrieval _sceneFocusRetrieval;
    protected override void Init(IOverrideReader overrideReader, ISceneFocusRetrieval sceneFocusRetrieval)
    {
        _inputReader = overrideReader;
        _sceneFocusRetrieval = sceneFocusRetrieval;
    }

    private void Start()
    {
        _inputReader.OnEscapeEvent += OnPause;
    }
    
    private void OnDestroy()
    {
        _inputReader.OnEscapeEvent -= OnPause;
    }

    private void OnPause()
    {
        if(_sceneFocusRetrieval.IsFocused(gameObject.scene.buildIndex))
        {
          onPauseEvent?.Invoke();
        }
    }
}
```

Input Reader is separated into Interfaces that use UnityEvents as hooks to detect input

Current Interfaces/Examples:
- ```IPlayerReader```
- ```IUIReader```
- ```IOverrideReader```

All interfaces are inherited by ```InputReaderSO``` (SO stands for Scriptable Object). 

Limitations and future improvement:
Currently, there is no separation of the readers; they are separated into partial classes. I hope to separate each action map into its own scriptable object and have InputReader be the aggregate service.

## Scene Controller 🎬
Description:
A service that provides easy access to scene loading and management of Active input maps tied to scene focus.

Scene Loading Example:
```c#
sceneController
  .NewStrategy()
  .SetSceneGroup(SceneGroup.Level)
  .SetActionMap(ActionMap.Player)
  .Load(gameScene.BuildIndex)
  .WithOverlay()
  .Execute();
```
Features:
- all in one ```SceneLoader``` Monobehavior class for any general loading uses
- supplementary lightweight ```SceneUnloader``` used to unload menus, etc.
- scene group; for a large environment, it might be helpful to chunk out sections into scenes, which can be organized via scene groups.
- Scene group focus stack
  - With ```ISceneFocusRetriever``` you can find the current focused scene group
  - Scene groups marked with None by default create their own scene stack
- Input stack
  - Similar to group focus stack, it tracks the default input map associated with a scene group
  - The input map is currently set when loading the scene.
 
Limitations and future improvements:
- There is currently no option to change the default map after the scene is loaded
- Only one scene loading strategy can execute at a time. A potential upgrade is to add an execution queue.
- There is no option to add extra logic to the scene execution. Could be added via a Config to hold a coroutine.

Heavy Inspiration from [The Code Otter](https://www.youtube.com/@thecodeotter)

## Other Features
- Logger
  - Loggers are controlled by the logger parent, but can also be controlled per scene
- Timer
  - Set to Unity's internal game loop
  - works by just hitting start!
  - Super easy to add your own custom timers
  - Credit: [git-ammend](https://www.youtube.com/@git-amend)
- Toggle Slider UI Element
- Custom attributes
  - ```[ReadOnly]```
  - ```[ShowIf(nameOf(variable))]``` used for showing fields based on a bool in the editor.
- ```SceneReference``` is a serializable class that stores a build index, which is shown as the actual scene name in the editor.
  - If not all scenes are options in the scene list, right-click the field and click "Refresh Scene List"
  - **Note**: if scenes are removed/added from the middle of the scene list, references will become offset
  




