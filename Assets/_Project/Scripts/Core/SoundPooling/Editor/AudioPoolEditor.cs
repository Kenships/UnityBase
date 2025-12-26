using System;
using UnityEditor;

namespace _Project.Scripts.Core.SoundPooling.Editor
{
    [CustomEditor(typeof(AudioPooler))]
    public class AudioPoolEditor : UnityEditor.Editor
    {
        private SerializedProperty _createBuffer;
        private SerializedProperty _bufferSize;
        private SerializedProperty _maxAudioSource;
        private SerializedProperty _audioMixerGroups;
        private SerializedProperty _numberOfActiveSources;
        private SerializedProperty _numberOfInactiveSources;
        private SerializedProperty _activeSourcesByAudioType;
        private SerializedProperty _activeSourcesBySceneIndex;
        private SerializedProperty _audioOverridePolicy;

        private void OnEnable()
        {
            _createBuffer = serializedObject.FindProperty("createBuffer");
            _bufferSize = serializedObject.FindProperty("bufferSize");
            _maxAudioSource = serializedObject.FindProperty("maxAudioSources");
            _audioMixerGroups = serializedObject.FindProperty("audioMixerGroups");
            _numberOfActiveSources = serializedObject.FindProperty("numberOfActiveSources");
            _numberOfInactiveSources = serializedObject.FindProperty("numberOfInactiveSources");
            _activeSourcesByAudioType = serializedObject.FindProperty("activeSourcesByAudioType");
            _activeSourcesBySceneIndex = serializedObject.FindProperty("activeSourcesBySceneIndex");
            _audioOverridePolicy =  serializedObject.FindProperty("audioOverridePolicy");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_createBuffer);

            if (_createBuffer.boolValue)
            {
                EditorGUILayout.Space();
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(_bufferSize);
                if (EditorGUI.EndChangeCheck())
                {
                    SerializedProperty serializedDictionary = _maxAudioSource.FindPropertyRelative("_serializedList");
                    int bufferMax = 0;
                    
                    for (int i = 0; i < serializedDictionary.arraySize; i++)
                    {
                        SerializedProperty serializedKeyValuePair = serializedDictionary.GetArrayElementAtIndex(i);
                        SerializedProperty serializedValue = serializedKeyValuePair.FindPropertyRelative("Value");
                        bufferMax += serializedValue.intValue;
                    }

                    _bufferSize.intValue = Math.Clamp(_bufferSize.intValue, 0, bufferMax);
                }
            }

            EditorGUILayout.PropertyField(_maxAudioSource);
            EditorGUILayout.PropertyField(_audioMixerGroups);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_audioOverridePolicy);

            EditorGUILayout.Space();
            
            EditorGUILayout.PropertyField(_numberOfActiveSources);
            EditorGUILayout.PropertyField(_numberOfInactiveSources);
            
            EditorGUILayout.PropertyField(_activeSourcesByAudioType);
            EditorGUILayout.PropertyField(_activeSourcesBySceneIndex);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
