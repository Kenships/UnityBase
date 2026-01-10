using System.Collections;
using System.Collections.Generic;
using _Project.Scripts.Core.InputManagement;
using _Project.Scripts.Core.InputManagement.Interfaces;
using _Project.Scripts.Core.SceneLoading.Interfaces;
using _Project.Scripts.UI.Interfaces;
using Sisus.Init;
using UnityEngine;
using UnityEngine.SceneManagement;
using ILogger = _Project.Scripts.Util.Logger.Interface.ILogger;

namespace _Project.Scripts.Core.SceneLoading
{
    [Service(typeof(ISceneBuilder), typeof(ISceneFocusRetrieval), LoadScene = 0)]
    public class SceneController : MonoBehaviour<ITransition, IInputActionSetter, ILogger>, ISceneBuilder,
                                   ISceneFocusRetrieval
    {
        private struct SceneGroupData
        {
            public ActionMap ActionMap { get; set; }
            public List<int> SceneBuildIndices { get; set; }
        }

        private ITransition _loadingOverlay;
        private ILogger _logger;
        private IInputActionSetter _inputReader;

        protected override void Init(ITransition argument, IInputActionSetter inputReader, ILogger logger)
        {
            _loadingOverlay = argument;
            _inputReader = inputReader;
            _logger = logger;
        }

        private readonly Dictionary<int, SceneGroup> _loadedScenes = new();
        private readonly HashSet<int> _disabledScenes = new();
        private readonly Stack<SceneGroupData> _sceneGroupStack = new();
        private readonly Dictionary<SceneGroup, List<int>> _sceneGroupToSceneList = new();
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

                _loadedScenes.Add(buildIndex, SceneGroup.None);
                _sceneGroupStack.Push(new SceneGroupData
                                      {
                                          ActionMap = ActionMap.Player,
                                          SceneBuildIndices = new List<int> { buildIndex }
                                      });
            }
        }

        #region Input State

        private void SetInputState()
        {
            ActionMap actionMap = _sceneGroupStack.Count == 0 ? ActionMap.Default : _sceneGroupStack.Peek().ActionMap;

            _inputReader.SetAction(actionMap);
        }

        #endregion

        #region Scene Focus

        public bool IsFocused(int sceneBuildIndex)
        {
            return GetFocusedScenes().Contains(sceneBuildIndex);
        }

        public List<int> GetFocusedScenes()
        {
            return _sceneGroupStack.Count > 0 ? _sceneGroupStack.Peek().SceneBuildIndices : new List<int>();
        }
        
        private void UpdateSceneGroupStack(int sceneBuildIndex, SceneGroup sceneGroup, ActionMap actionMap)
        {
            // Update SceneGroupStack
            if (sceneGroup != SceneGroup.None)
            {
                if (!_sceneGroupToSceneList.TryGetValue(sceneGroup, out List<int> value))
                {
                    List<int> sceneList = new List<int> { sceneBuildIndex };
                    SceneGroupData sceneGroupData = new SceneGroupData
                                                    {
                                                        SceneBuildIndices = sceneList, ActionMap = actionMap
                                                    };
                    _sceneGroupToSceneList.Add(sceneGroup, sceneList);
                    _sceneGroupStack.Push(sceneGroupData);
                }
                else
                {
                    value.Add(sceneBuildIndex);
                }
            }
            else
            {
                _sceneGroupStack.Push(new SceneGroupData
                                      {
                                          SceneBuildIndices = new List<int> { sceneBuildIndex },
                                          ActionMap = actionMap
                                      });
            }
        }

        private void UpdateSceneGroupStackOnRemove(int buildIndex)
        {
            RefreshSceneGroupStack();

            SceneGroup sceneGroup = _loadedScenes[buildIndex];
            if (sceneGroup != SceneGroup.None)
            {
                _sceneGroupToSceneList[sceneGroup].Remove(buildIndex);
                if (_sceneGroupToSceneList[sceneGroup].Count == 0)
                {
                    _sceneGroupToSceneList.Remove(sceneGroup);
                }
            }
            else
            {
                if (_sceneGroupStack.Peek().SceneBuildIndices[0] == buildIndex)
                {
                    _sceneGroupStack.Pop();
                }

                foreach (var sceneGroupData in _sceneGroupStack)
                {
                    List<int> sceneList = sceneGroupData.SceneBuildIndices;
                    if (sceneList.Contains(buildIndex))
                    {
                        sceneList.Remove(buildIndex);
                    }
                }
            }

            RefreshSceneGroupStack();
        }

        private void RefreshSceneGroupStack()
        {
            while (_sceneGroupStack.Count != 0 && _sceneGroupStack.Peek().SceneBuildIndices.Count == 0)
            {
                _sceneGroupStack.Pop();
            }
        }

        #endregion

        #region Loading Strategy Execution

        public SceneLoadingStrategy NewStrategy()
        {
            return new SceneLoadingStrategy(this);
        }

        private Coroutine ExecuteLoadingStrategy(SceneLoadingStrategy sceneLoadingStrategy)
        {
            if (_isBusy)
            {
                _logger.LogWarning("SceneLoading is busy. Cannot load new strategy.");
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

            foreach (var sceneBuildIndex in sceneLoadingStrategy.ScenesToUnload)
            {
                yield return UnloadSceneRoutine(sceneBuildIndex);
            }

            foreach (var sceneBuildIndex in sceneLoadingStrategy.ScenesToLoad)
            {
                // Item 1: SceneGroup, Item 2: InputActionType
                yield return AdditiveLoadRoutine(sceneBuildIndex, sceneLoadingStrategy.SceneGroup,
                    sceneBuildIndex == sceneLoadingStrategy.ActiveSceneBuildIndex);
                UpdateSceneGroupStack(sceneBuildIndex, sceneLoadingStrategy.SceneGroup, sceneLoadingStrategy.ActionMap);
            }
            
            foreach (var sceneBuildIndex in sceneLoadingStrategy.ScenesToDisable)
            {
                yield return DisableSceneRoutine(sceneBuildIndex);
            }

            if (sceneLoadingStrategy.Overlay)
            {
                _loadingOverlay.Hide();
                yield return new WaitForSeconds(_loadingOverlay.TransitionDuration);
            }

            SetInputState();

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

        private IEnumerator AdditiveLoadRoutine(int sceneBuildIndex, SceneGroup sceneGroup,
            bool setActive = false)
        {
            if (_loadedScenes.ContainsKey(sceneBuildIndex))
            {
                _logger.LogWarning($"Scene {sceneBuildIndex} is already loaded. Skipping.");
                yield break;
            }

            if (_disabledScenes.Contains(sceneBuildIndex))
            {
                foreach (var root in SceneManager.GetSceneByBuildIndex(sceneBuildIndex).GetRootGameObjects())
                {
                    root.SetActive(false);
                }
            }
            else
            {
                AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneBuildIndex, LoadSceneMode.Additive);

                if (loadOp == null)
                    yield break;

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
            }

            if (setActive)
            {
                Scene newScene = SceneManager.GetSceneByBuildIndex(sceneBuildIndex);

                if (newScene.IsValid() && newScene.isLoaded)
                {
                    SceneManager.SetActiveScene(newScene);
                }
            }

            _loadedScenes.Add(sceneBuildIndex, sceneGroup);
        }

        

        private IEnumerator DisableSceneRoutine(int sceneBuildIndex)
        {
            if (!_loadedScenes.ContainsKey(sceneBuildIndex))
            {
                _logger.LogWarning($"Scene {sceneBuildIndex} is not loaded. Skipping.");
                yield break;
            }

            foreach (var root in SceneManager.GetSceneByBuildIndex(sceneBuildIndex).GetRootGameObjects())
            {
                root.SetActive(false);
            }

            _disabledScenes.Add(sceneBuildIndex);

            //Update SceneGroupStack
            UpdateSceneGroupStackOnRemove(sceneBuildIndex);

            _loadedScenes.Remove(sceneBuildIndex);
        }

        private IEnumerator UnloadSceneRoutine(int buildIndex)
        {
            if (!_loadedScenes.ContainsKey(buildIndex))
            {
                _logger.LogWarning($"Scene {buildIndex} is not loaded. Skipping.");
                yield break;
            }

            AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(buildIndex);

            if (unloadOp == null)
            {
                _logger.LogWarning($"Scene {buildIndex} failed to load.");
                yield break;
            }

            while (!unloadOp.isDone)
            {
                yield return null;
            }

            // Update SceneGroupStack
            UpdateSceneGroupStackOnRemove(buildIndex);

            _loadedScenes.Remove(buildIndex);
        }

        #endregion

        #region Scene Loading Strategy

        public enum SceneGroup
        {
            Level,
            None
        }

        public class SceneLoadingStrategy
        {
            public List<int> ScenesToLoad { get; } = new();
            public List<int> ScenesToUnload { get; } = new();
            public List<int> ScenesToDisable { get; } = new();
            public int ActiveSceneBuildIndex { get; private set; }
            public bool ClearUnusedAssets { get; private set; } = false;
            public bool Overlay { get; private set; } = false;
            public SceneGroup SceneGroup { get; private set; }
            public ActionMap ActionMap { get; private set; }

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

            public SceneLoadingStrategy BufferLoad(int sceneBuildIndex)
            {
                ScenesToLoad.Add(sceneBuildIndex);
                ScenesToDisable.Add(sceneBuildIndex);
                return this;
            }

            public SceneLoadingStrategy SetSceneGroup(SceneGroup sceneGroup)
            {
                SceneGroup = sceneGroup;
                return this;
            }
            
            public SceneLoadingStrategy SetActionMap(ActionMap actionMap)
            {
                ActionMap = actionMap;
                return this;
            }

            public SceneLoadingStrategy Disable(int sceneBuildIndex)
            {
                ScenesToDisable.Add(sceneBuildIndex);
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
