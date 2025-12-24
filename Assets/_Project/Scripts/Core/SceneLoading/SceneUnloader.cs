using _Project.Scripts.Core.SceneLoading.Interfaces;
using _Project.Scripts.Util.Scene;
using Sisus.Init;
using UnityEngine;

namespace _Project.Scripts.Core.SceneLoading
{
    public class SceneUnloader : MonoBehaviour<ISceneBuilder>
    {
        [SerializeField] private bool withOverlay;
        [SerializeField] private SceneReference sceneRef;
        
        private ISceneBuilder _sceneController;
        
        protected override void Init(ISceneBuilder sceneController)
        {
            _sceneController = sceneController;
        }

        public void UnloadScene()
        {
            if (sceneRef.BuildIndex == 0)
            {
                Debug.LogError($"GameObject: {gameObject.name} from Scene: {gameObject.scene.name} " +
                               $"Tried to unload BootStrap. Skip Scene unloading");
                return;
            }
            
            _sceneController
                .NewStrategy()
                .Unload(sceneRef.BuildIndex)
                .WithOverlay(withOverlay)
                .Execute();
        }
    }
}
