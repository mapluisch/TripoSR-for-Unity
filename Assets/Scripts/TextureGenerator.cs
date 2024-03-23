using UnityEngine;
using System.Diagnostics;
using UnityEditor;
using System.IO;
using System;

public class TextureGenerator : MonoBehaviour
{
    [Header("General Settings")]
    [SerializeField, Tooltip("Path to your Python executable")]
    private string pythonPath = "/usr/bin/python";

    [SerializeField, Tooltip("Path to the 3D model file.")]
    private UnityEngine.Object modelFile;

    [SerializeField, Tooltip("Short description of the desired model appearance.")]
    private string modelDescription;
    [SerializeField, Tooltip("If true, automatically adds the generated mesh to the scene.")]
    private bool autoAddMesh = true;

    [SerializeField, Tooltip("If true, automatically adds MeshCollider & RigidBody.")]
    private bool autoAddPhysicsComponents = true;

    [SerializeField, Tooltip("If true, TextureGenerator's run.py debug output is printed to Unity's console.")]
    private bool showDebugLogs = true;

    [Header("TextureGenerator Parameters")]
    [ReadOnly, SerializeField, Tooltip("SD 1.5-based model for texture image generation. Default: 'Lykon/dreamshaper-8'")]
    private string imageModel = "Lykon/dreamshaper-8";

    [SerializeField, Tooltip("Number of inference steps for texture image generation. Default: 12")]
    private int steps = 12;

    [ReadOnly, SerializeField, Tooltip("Output directory to save the results. Default: 'output/'")]
    private string outputDir = "output/";

    private Process pythonProcess;
    private bool isProcessRunning = false;

    public static event Action OnPythonProcessEnded;

    public void RunTextureGenerator()
    {
        if (isProcessRunning)
        {
            UnityEngine.Debug.Log("A TextureGenerator process is already running - quitting and replacing process.");

            if (pythonProcess is { HasExited: false })
            {
                pythonProcess.Kill();
                pythonProcess.Dispose();
            }

            pythonProcess = null;
            isProcessRunning = false;
        }

        string modelPath = AssetDatabase.GetAssetPath(modelFile);
        string args = $"\"{modelPath}\" \"{modelDescription}\" " +
                      $"--image-model {imageModel} --steps {steps} " +
                      $"--output-dir {Path.Combine(Application.dataPath, "triposr-texture-gen/" + outputDir)}";

        ProcessStartInfo start = new ProcessStartInfo
        {
            FileName = pythonPath,
            Arguments = $"{Path.Combine(Application.dataPath, "triposr-texture-gen/text2texture.py")} {args}",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        pythonProcess = new Process { StartInfo = start };
        pythonProcess.StartInfo = start;
        pythonProcess.EnableRaisingEvents = true;
        pythonProcess.Exited += OnPythonProcessExited;

        pythonProcess.OutputDataReceived += (sender, e) =>
        {
            if (showDebugLogs && !string.IsNullOrEmpty(e.Data))
            {
                UnityEngine.Debug.Log(e.Data);
            }
        };

        pythonProcess.ErrorDataReceived += (sender, e) =>
        {
            if (showDebugLogs && !string.IsNullOrEmpty(e.Data))
            {
                UnityEngine.Debug.Log(e.Data);
            }
        };

        pythonProcess.Start();
        pythonProcess.BeginOutputReadLine();
        pythonProcess.BeginErrorReadLine();
        isProcessRunning = true;
    }

    private void OnPythonProcessExited(object sender, EventArgs e)
    {
        isProcessRunning = false;
        pythonProcess = null;

        if (autoAddMesh) UnityEditor.EditorApplication.delayCall += () => AddMeshToScene();

        UnityEditor.EditorApplication.delayCall += () => OnPythonProcessEnded?.Invoke();
    }

    private void AddMeshToScene()
    {        
        string objPath = AssetDatabase.GetAssetPath(modelFile).Replace(".obj", "-tex.obj");
        string newAssetPath = AssetDatabase.GetAssetPath(modelFile).Replace(".obj", "-tex.obj");
        print(newAssetPath);

        AssetDatabase.Refresh();
        AssetDatabase.ImportAsset(objPath, ImportAssetOptions.ForceUpdate);

        GameObject importedObj = AssetDatabase.LoadAssetAtPath<GameObject>(objPath);

        if (importedObj != null)
        {
            GameObject instantiatedObj = Instantiate(importedObj);
            instantiatedObj.name = importedObj.name;

            UnityEngine.Debug.Log("Instantiated GameObject prefab: " + instantiatedObj.name);

            if (autoAddPhysicsComponents) 
            {
                GameObject meshObj = instantiatedObj.transform.GetChild(0).gameObject;
                MeshCollider mc = meshObj.AddComponent<MeshCollider>();
                mc.convex = true;
                meshObj.AddComponent<Rigidbody>();
            }
        }
        else UnityEngine.Debug.LogError("Failed to load the mesh at path: " + objPath);
    }


    void OnDisable() { if (pythonProcess != null && !pythonProcess.HasExited) pythonProcess.Kill(); }
}
