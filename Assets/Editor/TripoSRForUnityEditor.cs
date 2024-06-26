using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TripoSRForUnity))]
public class TripoSRForUnityEditor : Editor
{
    private bool isProcessRunning = false;
    private float colorLerpTimer = 0f;
    private float colorLerpDuration = 1.5f;
    private Color lightBlue = new Color(124/255f, 144/255f, 219/255f, 1f);
    private Color darkBlue = new Color(80/255f, 100/255f, 170/255f, 1f);
    private int imagePreviewSize = 256;

    void OnEnable() => TripoSRForUnity.OnPythonProcessEnded += OnProcessEnded;

    void OnDisable()
    {
        TripoSRForUnity.OnPythonProcessEnded -= OnProcessEnded;
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
        TripoSRForUnity tripoInstance = (TripoSRForUnity)target;
        
        DrawPropertiesExcluding(serializedObject, "images");
        
        EditorGUILayout.LabelField("Images", EditorStyles.boldLabel);
        SerializedProperty imagesProperty = serializedObject.FindProperty("images");
        int newSize = Mathf.Max(0, EditorGUILayout.IntField("Size", imagesProperty.arraySize));
        if (newSize != imagesProperty.arraySize)
        {
            imagesProperty.arraySize = newSize;
        }

        EditorGUI.indentLevel++;
        for (int i = 0; i < imagesProperty.arraySize; i++)
        {
            SerializedProperty imageProp = imagesProperty.GetArrayElementAtIndex(i);
            EditorGUILayout.BeginVertical();
            EditorGUILayout.PrefixLabel("Image " + i);
            if (imageProp.objectReferenceValue != null)
            {
                EditorGUILayout.BeginVertical();
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                Rect textureRect = GUILayoutUtility.GetRect(imagePreviewSize, imagePreviewSize, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
                EditorGUI.DrawPreviewTexture(textureRect, (Texture)imageProp.objectReferenceValue, null, ScaleMode.ScaleToFit);
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }

            imageProp.objectReferenceValue = EditorGUILayout.ObjectField(imageProp.objectReferenceValue, typeof(Texture2D), allowSceneObjects: false);

            EditorGUILayout.EndVertical();
        }
        EditorGUI.indentLevel--;

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
        if (GUILayout.Button(isProcessRunning ? "Processing..." : "Run TripoSR", bigButtonStyle, GUILayout.Height(50)) && !isProcessRunning)
        {
            isProcessRunning = true;
            EditorApplication.update += EditorUpdate;
            tripoInstance.RunTripoSR();
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
        for (int i = 0; i < pix.Length; i++) pix[i] = col;
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
