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
        private SerializedProperty _inactiveSources;
        private SerializedProperty _activeSources;
        private SerializedProperty _audioOverridePolicy;

        private void OnEnable()
        {
            _createBuffer = serializedObject.FindProperty("createBuffer");
            _bufferSize = serializedObject.FindProperty("bufferSize");
            _maxAudioSource = serializedObject.FindProperty("maxAudioSources");
            _audioMixerGroups = serializedObject.FindProperty("audioMixerGroups");
            _inactiveSources = serializedObject.FindProperty("inactiveSources");
            _activeSources = serializedObject.FindProperty("activeSources");
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
            EditorGUILayout.PropertyField(_inactiveSources);
            EditorGUILayout.PropertyField(_activeSources);
            EditorGUILayout.PropertyField(_audioOverridePolicy);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
