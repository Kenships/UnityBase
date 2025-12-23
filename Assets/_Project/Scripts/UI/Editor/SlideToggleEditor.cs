using _Project.Scripts.UI.UIElements;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UI;
using UnityEngine;

namespace _Project.Scripts.UI.Editor
{
    [CustomEditor(typeof(SlideToggle), true)]
    [CanEditMultipleObjects]
    public class SlideToggleEditor : SelectableEditor
    {
        SerializedProperty m_isOnProperty;
        SerializedProperty m_toggleBallProperty;
        SerializedProperty m_backgroundProperty;
        SerializedProperty m_onColorProperty;
        SerializedProperty m_offColorProperty;
        SerializedProperty m_onValueChangedProperty;
        SerializedProperty m_animationTimeProperty;
        
        
        
        
        protected override void OnEnable()
        {
            base.OnEnable();
            m_isOnProperty = serializedObject.FindProperty("isOn");
            m_toggleBallProperty = serializedObject.FindProperty("toggleBall");
            m_backgroundProperty = serializedObject.FindProperty("background");
            m_onColorProperty = serializedObject.FindProperty("onColor");
            m_offColorProperty = serializedObject.FindProperty("offColor");
            m_animationTimeProperty = serializedObject.FindProperty("animationTime");
            m_onValueChangedProperty = serializedObject.FindProperty("onValueChanged");
            
            
        }
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.Space();

            serializedObject.Update();
            SlideToggle toggle = serializedObject.targetObject as SlideToggle;
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_isOnProperty);
            if (EditorGUI.EndChangeCheck())
            {
                if (!Application.isPlaying)
                    EditorSceneManager.MarkSceneDirty(toggle.gameObject.scene);
                toggle.IsOn = m_isOnProperty.boolValue;
            }
            EditorGUILayout.PropertyField(m_toggleBallProperty);
            EditorGUILayout.PropertyField(m_backgroundProperty);
            
            EditorGUILayout.PropertyField(m_onColorProperty);
            EditorGUILayout.PropertyField(m_offColorProperty);
            EditorGUILayout.PropertyField(m_animationTimeProperty);
            
            EditorGUILayout.Space();

            // Draw the event notification options
            EditorGUILayout.PropertyField(m_onValueChangedProperty);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
