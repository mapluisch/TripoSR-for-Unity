using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TextureGenerator))]
public class TextureGeneratorEditor : Editor
{
    private bool isProcessRunning = false;
    private float colorLerpTimer = 0f;
    private float colorLerpDuration = 1.5f;
    private Color lightBlue = new Color(124/255f, 144/255f, 219/255f, 1f);
    private Color darkBlue = new Color(80/255f, 100/255f, 170/255f, 1f);
    private int modelPreviewSize = 256;

    void OnEnable() => TextureGenerator.OnPythonProcessEnded += OnProcessEnded;

    void OnDisable()
    {
        TextureGenerator.OnPythonProcessEnded -= OnProcessEnded;
        if (isProcessRunning) EditorApplication.update -= EditorUpdate;
    }

    private void OnProcessEnded()
    {
        isProcessRunning = false;
        EditorApplication.update -= EditorUpdate;
        colorLerpTimer = 0f;
        Repaint();
    }

    public override void OnInspectorGUI()
    {
        TextureGenerator textureInstance = (TextureGenerator)target;
        
        DrawPropertiesExcluding(serializedObject, "modelFile");

        EditorGUILayout.LabelField("Model File", EditorStyles.boldLabel);
        SerializedProperty modelFileProperty = serializedObject.FindProperty("modelFile");
        EditorGUILayout.BeginVertical();
        EditorGUILayout.PrefixLabel("Model File");

        if (modelFileProperty.objectReferenceValue != null)
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            Rect textureRect = GUILayoutUtility.GetRect(modelPreviewSize, modelPreviewSize, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
            Texture2D preview = AssetPreview.GetAssetPreview(modelFileProperty.objectReferenceValue);
            if (preview != null)
            {
                EditorGUI.DrawPreviewTexture(textureRect, preview, null, ScaleMode.ScaleToFit);
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        modelFileProperty.objectReferenceValue = EditorGUILayout.ObjectField(modelFileProperty.objectReferenceValue, typeof(Object), false);
        EditorGUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space();
        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
        EditorGUILayout.Space();

        GUIStyle bigButtonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 14,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white },
            active = { textColor = Color.white, background = ColorTex(2, 2, darkBlue) },
            margin = new RectOffset(10, 10, 4, 4),
            padding = new RectOffset(10, 10, 10, 10)
        };

        bigButtonStyle.normal.background = isProcessRunning ? ColorTex(2, 2, GetLerpedColor()) : ColorTex(2, 2, lightBlue);

        EditorGUI.BeginDisabledGroup(isProcessRunning);
        if (GUILayout.Button(isProcessRunning ? "Processing..." : "Generate Textured Object", bigButtonStyle, GUILayout.Height(50)) && !isProcessRunning)
        {
            isProcessRunning = true;
            EditorApplication.update += EditorUpdate;
            textureInstance.RunTextureGenerator();
        }
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.Space();
        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
        EditorGUILayout.Space();
    }


    private void EditorUpdate()
    {
        if (isProcessRunning) Repaint();
        else EditorApplication.update -= EditorUpdate;
    }
    
    private Texture2D ColorTex(int width, int height, Color col)
    {
        Texture2D result = new Texture2D(width, height);
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; ++i) pix[i] = col;
        result.SetPixels(pix);
        result.Apply();
        return result;
    }

    private Color GetLerpedColor()
    {
        colorLerpTimer += Time.deltaTime;
        float lerpFactor = Mathf.PingPong(colorLerpTimer / colorLerpDuration, 1);
        return Color.Lerp(lightBlue, darkBlue, lerpFactor);
    }
}
