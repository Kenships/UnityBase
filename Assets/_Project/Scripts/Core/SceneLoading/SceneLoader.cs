using _Project.Scripts.Core.SceneLoading.Interfaces;
using _Project.Scripts.Util;
using _Project.Scripts.Util.Scene;
using Sisus.Init;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Project.Scripts.Core.SceneLoading
{
    public class SceneLoader : MonoBehaviour<ISceneBuilder>
    {
        [SerializeField] private SceneReference sceneRef;
        [SerializeField] private bool withOverlay;
        [SerializeField] private bool setActive;
        
        private ISceneBuilder _sceneController;
        protected override void Init(ISceneBuilder argument)
        {
            _sceneController = argument;
        }

        public void TransitionToScene()
        {
            if (sceneRef.BuildIndex == 0)
            {
                Debug.LogError($"GameObject: {gameObject.name} from Scene: {gameObject.scene.name} " +
                               $"Tried to load BootStrap. Skip Scene loading");
                return;
            }
            
            _sceneController
                .NewStrategy()
                .Load(sceneRef.BuildIndex, setActive)
                .Unload(gameObject.scene.buildIndex)
                .WithOverlay()
                .Execute();
        }

        public void LoadSceneAdditive()
        {
            if (sceneRef.BuildIndex == 0)
            {
                Debug.LogError($"GameObject: {gameObject.name} from Scene: {gameObject.scene.name} " +
                               $"Tried to load BootStrap. Skip Scene loading");
                return;
            }
            
            _sceneController
                .NewStrategy()
                .Load(sceneRef.BuildIndex, setActive)
                .WithOverlay(withOverlay)
                .Execute();
        }
    }
}
