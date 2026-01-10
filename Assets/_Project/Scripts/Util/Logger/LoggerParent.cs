using System.Collections.Generic;
using _Project.Scripts.Util.Logger.Interface;
using Sisus.Init;
using UnityEngine;
using ILogger = _Project.Scripts.Util.Logger.Interface.ILogger;

namespace _Project.Scripts.Util.Logger
{
    [Service(typeof(ILoggerFactory))]
    public class LoggerParent : MonoBehaviour, ILoggerFactory
    {
        [SerializeField] private bool isEnabled = true;

        private void OnValidate()
        {
            #if UNITY_EDITOR
            SetAllActive(isEnabled);    
            #endif
        }

        private readonly List<ILogger> _loggers = new();
        public ILogger CreateLogger()
        {
            ILogger logger = new Logger();
            _loggers.Add(logger);
            return logger;
        }
        
        private void SetAllActive(bool active)
        {
            foreach (var logger in _loggers)
            {
                logger.SetActive(active);
            }
        }
    }
}
