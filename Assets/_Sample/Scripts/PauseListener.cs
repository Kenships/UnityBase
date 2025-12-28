using _Project.Scripts.Core.InputManagement.Interfaces;
using _Project.Scripts.Core.SceneLoading.Interfaces;
using Sisus.Init;
using UnityEngine.Events;

namespace _Sample.Scripts
{
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
}
