using System.Collections;
using System.Collections.Generic;
using _Project.Scripts.UI.Interfaces;
using _Project.Scripts.Util;
using NUnit.Framework;
using Sisus.Init;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Project.Scripts.Core.SceneLoading
{
    [Service(typeof(SceneController), LoadScene = 0)]
    public class SceneController : MonoBehaviour<ITransition>
    {
        private ITransition _loadingOverlay;
        
        protected override void Init(ITransition argument)
        {
            _loadingOverlay = argument;
        }
        
        private readonly HashSet<int> _loadedScenes = new();
        private bool _isBusy;

        protected override void OnAwake()
        {
            for (int index = 0; index < SceneManager.sceneCount; index++)
            {
                int buildIndex = SceneManager.GetSceneAt(index).buildIndex;

                // 0 is bootstrap
                if (buildIndex == 0)
                {
                    continue;
                }

                _loadedScenes.Add(buildIndex);
            }
        }

        public SceneLoadingStrategy NewStrategy()
        {
            return new SceneLoadingStrategy(this);
        }
        
        private Coroutine ExecuteLoadingStrategy(SceneLoadingStrategy sceneLoadingStrategy)
        {
            if (_isBusy)
            {
                Debug.LogWarning("SceneLoading is busy. Cannot load new strategy.");
                return null;
            }

            _isBusy = true;
            return StartCoroutine(ChangeSceneRoutine(sceneLoadingStrategy));
        }

        private IEnumerator ChangeSceneRoutine(SceneLoadingStrategy sceneLoadingStrategy)
        {
            if (sceneLoadingStrategy.Overlay)
            {
                _loadingOverlay.Show();
                yield return new WaitForSeconds(_loadingOverlay.TransitionDuration);
            }

            if (sceneLoadingStrategy.ClearUnusedAssets)
            {
                yield return CleanUpUnusedAssetsRoutine();
            }
            
            foreach(var sceneBuildIndex in sceneLoadingStrategy.ScenesToUnload)
            {
                yield return UnloadSceneRoutine(sceneBuildIndex);
            }

            foreach (var sceneBuildIndex in sceneLoadingStrategy.ScenesToLoad)
            {
                Assert.IsNotNull(sceneBuildIndex, "SceneName was Null");
                if (_loadedScenes.Contains(sceneBuildIndex))
                {
                    Debug.LogWarning($"Scene {sceneBuildIndex} is already loaded. Skipping.");
                    continue;
                }
                yield return AdditiveLoadRoutine(sceneBuildIndex, sceneBuildIndex == sceneLoadingStrategy.ActiveSceneBuildIndex);
            }
            
            if (sceneLoadingStrategy.Overlay)
            {
                _loadingOverlay.Hide();
                yield return new WaitForSeconds(_loadingOverlay.TransitionDuration);
            }
            _isBusy = false;
        }

        private IEnumerator CleanUpUnusedAssetsRoutine()
        {
            AsyncOperation cleanUpOp = Resources.UnloadUnusedAssets();
            while (!cleanUpOp.isDone)
            {
                yield return null;
            }
            
        }

        private IEnumerator AdditiveLoadRoutine(int sceneBuildIndex, bool setActive = false)
        {
            AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneBuildIndex, LoadSceneMode.Additive);
            
            if(loadOp == null) yield break;
            
            loadOp.allowSceneActivation = false;
            
            while (loadOp.progress < 0.9f)
            {
                yield return null;
            }
            
            loadOp.allowSceneActivation = true;
            
            while (!loadOp.isDone)
            {
                yield return null;
            }

            if (setActive)
            {
                Scene newScene = SceneManager.GetSceneByBuildIndex(sceneBuildIndex);

                if (newScene.IsValid() && newScene.isLoaded)
                {
                    SceneManager.SetActiveScene(newScene);
                }
            }
            
            _loadedScenes.Add(sceneBuildIndex);
        }

        private IEnumerator UnloadSceneRoutine(int buildIndex)
        {
            if (!_loadedScenes.Contains(buildIndex))
            {
                Debug.LogWarning($"Scene {buildIndex} is not loaded. Skipping.");
                yield break;
            }
            AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(buildIndex);

            if (unloadOp == null)
            {
                Debug.LogWarning($"Scene {buildIndex} failed to load.");
                yield break;
            }

            while (!unloadOp.isDone)
            {
                yield return null;
            }

            _loadedScenes.Remove(buildIndex);
        }
        
        #region Scene Loading Strategy
        public class SceneLoadingStrategy
        {
            public HashSet<int> ScenesToLoad { get; } = new();
            public List<int> ScenesToUnload { get; } = new();
            public int ActiveSceneBuildIndex { get; private set; }
            public bool ClearUnusedAssets { get; private set; } = false;
            public bool Overlay { get; private set; } = false;
            
            private readonly SceneController _controller;
            
            public SceneLoadingStrategy(SceneController controller)
            {
                _controller = controller;
            }

            public SceneLoadingStrategy Load(int sceneBuildIndex, bool setActive = false)
            {
                ScenesToLoad.Add(sceneBuildIndex);
                ActiveSceneBuildIndex = setActive ? sceneBuildIndex : ActiveSceneBuildIndex;
                return this;
            }
            
            public SceneLoadingStrategy Unload(int sceneBuildIndex)
            {
                ScenesToUnload.Add(sceneBuildIndex);
                return this;
            }
            
            public SceneLoadingStrategy WithOverlay(bool withOverlay = true)
            {
                Overlay = withOverlay;
                return this;
            }
            
            public SceneLoadingStrategy WithClearUnusedAssets()
            {
                ClearUnusedAssets = true;
                return this;
            }
            

            public Coroutine Execute()
            {
                return _controller.ExecuteLoadingStrategy(this);
            }
        }
#endregion
    }
}


