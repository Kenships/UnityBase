using System.Collections.Generic;
using _Project.Scripts.Core.SceneLoading.Interfaces;
using _Project.Scripts.Util;
using _Project.Scripts.Util.Scene;
using Sisus.Init;
using UnityEngine;

namespace _Project.Scripts.Core.SceneLoading
{
    /// <summary>
    /// Loads multiple scenes on awake.
    /// Limitations: Cannot set active scene on load.
    /// Potential Fix: change List<SceneReference> to SerializableDictionary<SceneReference, bool>
    /// </summary>
    public class InitialSceneLoader : MonoBehaviour<ISceneBuilder>
    {
        [SerializeField] private List<SceneReference> sceneRefs;
        [SerializeField] private bool withOverlay;
        
        private ISceneBuilder _sceneController;
        protected override void Init(ISceneBuilder argument)
        {
            _sceneController = argument;
        }

        protected override void OnAwake()
        {
            SceneController.SceneLoadingStrategy loadingStrategy = _sceneController.NewStrategy();

            foreach (var scene in sceneRefs)
            {
                if (scene.BuildIndex == 0)
                {
                    Debug.LogError($"GameObject: {gameObject.name} from Scene: {gameObject.scene.name} " +
                                   $"Tried to load BootStrap. Skip Scene loading");
                    continue;
                }
                
                loadingStrategy.Load(scene.BuildIndex);
            }

            loadingStrategy
                .WithOverlay(withOverlay)
                .Execute();
        }
    }
}
