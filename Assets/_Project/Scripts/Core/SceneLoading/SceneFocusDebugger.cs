using System;
using System.Collections.Generic;
using _Project.Scripts.Core.SceneLoading.Interfaces;
using Sisus.Init;
using UnityEngine;

namespace _Project.Scripts.Core.SceneLoading
{
    public class SceneFocusDebugger : MonoBehaviour<ISceneFocusRetrieval>
    {
        [SerializeField] private List<int> focusedScenes;
        ISceneFocusRetrieval _sceneFocusRetrieval;
        protected override void Init(ISceneFocusRetrieval argument)
        {
            _sceneFocusRetrieval = argument;
        }

        private void Update()
        {
            focusedScenes = _sceneFocusRetrieval.GetFocusedScenes();
        }
    }
}
