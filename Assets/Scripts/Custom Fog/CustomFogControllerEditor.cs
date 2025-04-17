using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CustomFogController))]
public class CustomFogControllerEditor : Editor
{
    SerializedProperty fogStartProperty;
    SerializedProperty fogEndProperty;
    SerializedProperty fogDensityProperty;
    SerializedProperty fogDistanceProperty;
    SerializedProperty methodProperty;
    SerializedProperty fogColorProperty;

    private void OnEnable()
    {
        fogStartProperty = serializedObject.FindProperty("fogStart");
        fogEndProperty = serializedObject.FindProperty("fogEnd");
        fogDensityProperty = serializedObject.FindProperty("fogDensity");
        fogDistanceProperty = serializedObject.FindProperty("fogDistance");
        methodProperty = serializedObject.FindProperty("method");
        fogColorProperty = serializedObject.FindProperty("fogColor");
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(methodProperty);
        EditorGUILayout.PropertyField(fogColorProperty);

        int method = methodProperty.enumValueIndex;

        EditorGUI.BeginDisabledGroup(method == 0);
        EditorGUILayout.PropertyField(fogDensityProperty);
        EditorGUILayout.PropertyField(fogDistanceProperty);
        EditorGUI.EndDisabledGroup();

        serializedObject.ApplyModifiedProperties();

        EditorGUI.BeginDisabledGroup(method == 1);
        EditorGUILayout.PropertyField(fogStartProperty);
        EditorGUILayout.PropertyField(fogEndProperty);
        EditorGUI.EndDisabledGroup();

        serializedObject.ApplyModifiedProperties();

    }
}
