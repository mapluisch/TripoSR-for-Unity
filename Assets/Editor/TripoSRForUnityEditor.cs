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
        
        Color pastelLightBlue = new Color(124/255f, 144/255f, 219/255f, 1f);
        Color pressedBlue = new Color(124/255f, 144/255f, 219/255f, 0.8f);
            
        GUIStyle bigButtonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 14,
            fontStyle = FontStyle.Bold,
            normal = {
                textColor = Color.white,
                background = ColorTex(2, 2, pastelLightBlue)
            },
            active = {
                textColor = Color.white,
                background = ColorTex(2, 2, pressedBlue)
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
    
    private Texture2D ColorTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; ++i) pix[i] = col;
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }
}
