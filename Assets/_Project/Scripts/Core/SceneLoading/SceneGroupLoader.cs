using System.Collections.Generic;
using _Project.Scripts.Core.SceneLoading.Interfaces;
using _Project.Scripts.Util.Scene;
using Sisus.Init;
using UnityEngine;
using ILogger = _Project.Scripts.Util.Logger.Interface.ILogger;

namespace _Project.Scripts.Core.SceneLoading
{
    /// <summary>
    /// Loads multiple scenes on awake.
    /// Limitations: Cannot set active scene on load.
    /// Potential Fix: change List<SceneReference> to SerializableDictionary<SceneReference, bool>
    /// </summary>
    public class SceneGroupLoader : MonoBehaviour<ISceneBuilder, ILogger>
    {
        [SerializeField] private List<SceneReference> sceneRefs;
        [SerializeField] private bool withOverlay;
        [SerializeField] private SceneController.SceneGroup sceneGroup = SceneController.SceneGroup.None;
        [SerializeField] private bool replaceCurrentScene;
        [SerializeField] private bool loadOnAwake;
        
        private ISceneBuilder _sceneController;
        private ILogger _logger;
        protected override void Init(ISceneBuilder argument, ILogger logger)
        {
            _sceneController = argument;
            _logger = logger;
        }

        protected override void OnAwake()
        {
            if(!loadOnAwake) return;
            LoadScenes();
        }

        public void LoadScenes()
        {
            SceneController.SceneLoadingStrategy loadingStrategy = _sceneController.NewStrategy();

            foreach (var scene in sceneRefs)
            {
                if (scene.BuildIndex == 0)
                {
                    _logger.LogError($"GameObject: {gameObject.name} from Scene: {gameObject.scene.name} " +
                                   $"Tried to load BootStrap. Skip Scene loading");
                    continue;
                }
                
                loadingStrategy.Load(scene.BuildIndex);
            }

            if (replaceCurrentScene)
            {
                loadingStrategy.Unload(gameObject.scene.buildIndex);
            }

            loadingStrategy
                .WithOverlay(withOverlay)
                .Execute();
        }
    }
}
