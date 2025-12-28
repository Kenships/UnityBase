using System.Collections.Generic;
using _Project.Scripts.Core.SceneLoading.Interfaces;
using _Project.Scripts.Util;
using _Project.Scripts.Util.Scene;
using Sisus.Init;
using UnityEngine;
using UnityEngine.Serialization;
using ILogger = _Project.Scripts.Util.Logger.Interface.ILogger;

namespace _Project.Scripts.Core.SceneLoading
{
    public class SceneLoader : MonoBehaviour<ISceneBuilder, ILogger>
    {
        [SerializeField] private SceneReference sceneRef;
        [SerializeField] private List<SceneReference> scenesToUnload;
        [SerializeField] private bool withOverlay;
        [SerializeField] private bool setActive;
        [SerializeField] private SceneController.SceneGroup sceneGroup = SceneController.SceneGroup.None;
        
        private ISceneBuilder _sceneController;
        private ILogger _logger;
        protected override void Init(ISceneBuilder argument, ILogger logger)
        {
            _sceneController = argument;
            _logger = logger;
        }

        public void LoadSceneAdditive()
        {
            if (sceneRef.BuildIndex == 0)
            {
                _logger.LogError($"GameObject: {gameObject.name} from Scene: {gameObject.scene.name} " +
                               $"Tried to load BootStrap. Skip Scene loading");
                return;
            }

            SceneController.SceneLoadingStrategy loadingStrategy = 
                _sceneController
                .NewStrategy()
                .Load(sceneRef.BuildIndex, setActive);
            
            foreach (var scene in scenesToUnload)
            {
                if (scene.BuildIndex == 0)
                {
                    _logger.LogError($"GameObject: {gameObject.name} from Scene: {gameObject.scene.name} " +
                                   $"Tried to unload BootStrap. Skip Scene unloading");
                    continue;
                }
                loadingStrategy.Unload(scene.BuildIndex);
            }
            
            loadingStrategy
                .WithOverlay(withOverlay)
                .Execute();
        }
    }
}
