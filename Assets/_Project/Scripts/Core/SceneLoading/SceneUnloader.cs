using System.Collections.Generic;
using _Project.Scripts.Core.SceneLoading.Interfaces;
using _Project.Scripts.Util.Scene;
using Sisus.Init;
using UnityEngine;

namespace _Project.Scripts.Core.SceneLoading
{
    public class SceneUnloader : MonoBehaviour<ISceneBuilder>
    {
        [SerializeField] private List<SceneReference> scenesToUnload;
        [SerializeField] private bool withOverlay;
        
        private ISceneBuilder _sceneController;
        
        protected override void Init(ISceneBuilder sceneController)
        {
            _sceneController = sceneController;
        }

        public void UnloadScene()
        {
            
            SceneController.SceneLoadingStrategy loadingStrategy = _sceneController.NewStrategy();
            
            foreach (var sceneRef in scenesToUnload)
            {
                if (sceneRef.BuildIndex == 0)
                {
                    Debug.LogError($"GameObject: {gameObject.name} from Scene: {gameObject.scene.name} " +
                                   $"Tried to unload BootStrap. Skip Scene unloading");
                    return;
                }
                loadingStrategy.Unload(sceneRef.BuildIndex);
            }
            
            loadingStrategy.WithOverlay(withOverlay).Execute();
        }
    }
}
