using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TripoSRForUnity))]
public class TripoSRForUnityEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();
        GUILayout.Box("", new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.Height(1)});
        EditorGUILayout.Space();

        TripoSRForUnity tripoInstance = (TripoSRForUnity)target;

        GUIStyle bigButtonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 14,
            fontStyle = FontStyle.Bold,
            normal = {
                textColor = Color.white,
                background = Texture2D.grayTexture
            },
            margin = new RectOffset(10, 10, 4, 4),
            padding = new RectOffset(10, 10, 10, 10)
        };

        if (GUILayout.Button("Run TripoSR", bigButtonStyle, GUILayout.Height(50)))
        {
            tripoInstance.RunTripoSR();
        }

        EditorGUILayout.Space();
        GUILayout.Box("", new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.Height(1)});
        EditorGUILayout.Space();
    }
}
