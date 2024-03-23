using UnityEngine;
using System.Diagnostics;
using UnityEditor;
using System.IO;
using System.Globalization;
using System;

public class TripoSRForUnity : MonoBehaviour
{
    [Header("General Settings")]
    [SerializeField, Tooltip("Path to your Python executable")]
    private string pythonPath = "/usr/bin/python";
    
    [SerializeField, Tooltip("If true, automatically adds the generated mesh to the scene.")]
    private bool autoAddMesh = true;
    
    [SerializeField, Tooltip("If true, automatically rotates the mesh's parent GameObject to negate wrong rotations.")]
    private bool autoFixRotation = true;
    
    [SerializeField, Tooltip("If true, moves and renames the output .obj file (based on the input image's filename)")]
    private bool moveAndRename = true;
    
    [SerializeField, Tooltip("If moveAndRename = true, specifies the relative path to some folder where the output .obj file will be moved to.")]
    private string moveAndRenamePath = "Models";
    
    [SerializeField, Tooltip("If true, TripoSR's run.py debug output is printed to Unity's console.")]
    private bool showDebugLogs = true;

    [SerializeField, Tooltip("Path to input image(s).")]
    private Texture2D[] images;
    
    [Header("TripoSR Parameters")]
    [ReadOnly, SerializeField, Tooltip("Device to use. Default: 'cuda:0'")]
    private string device = "cuda:0";

    [ReadOnly, SerializeField, Tooltip("Path to the pretrained model. Default: 'stabilityai/TripoSR'")]
    private string pretrainedModelNameOrPath = "stabilityai/TripoSR";

    [SerializeField, Tooltip("Evaluation chunk size. Default: 8192")]
    private int chunkSize = 8192;

    [SerializeField, Tooltip("Marching cubes grid resolution. Default: 256")]
    private int marchingCubesResolution = 256;

    [SerializeField, Tooltip("If true, background will not be removed. Default: false")]
    private bool noRemoveBg = false;

    [SerializeField, Tooltip("Foreground to image size ratio. Default: 0.85")]
    private float foregroundRatio = 0.85f;

    [ReadOnly, SerializeField, Tooltip("Output directory. Default: 'output/'")]
    private string outputDir = "output/";

    [ReadOnly, SerializeField, Tooltip("Mesh save format. Default: 'obj'")]
    private string modelSaveFormat = "obj";

    [ReadOnly, SerializeField, Tooltip("If true, saves a rendered video. Default: false")]
    private bool render = false;

    private Process pythonProcess;
    private bool isProcessRunning = false;

    public static event Action OnPythonProcessEnded;

    public void RunTripoSR()
    {
        if (isProcessRunning)
        {
            UnityEngine.Debug.Log("A TripoSR process is already running - quitting and replacing process.");

            if (pythonProcess is { HasExited: false })
            {
                pythonProcess.Kill();
                pythonProcess.Dispose();
            }

            pythonProcess = null;
            isProcessRunning = false;
        }
        
        string[] imagePaths = new string[images.Length];
        for (int i = 0; i < images.Length; i++)
        {
            string path = AssetDatabase.GetAssetPath(images[i]);
            imagePaths[i] = Path.GetFullPath(path);
        }

        string args = $"\"{string.Join("\" \"", imagePaths)}\" --device {device} " +
                      $"--pretrained-model-name-or-path {pretrainedModelNameOrPath} " +
                      $"--chunk-size {chunkSize} --mc-resolution {marchingCubesResolution} " +
                      $"{(noRemoveBg ? "--no-remove-bg " : "")} " +
                      $"--foreground-ratio {foregroundRatio.ToString(CultureInfo.InvariantCulture)} --output-dir {Path.Combine(Application.dataPath, "TripoSR/" + outputDir)} " +
                      $"--model-save-format {((modelSaveFormat == "dae") ? "obj" : modelSaveFormat)} " +
                      $"{(render ? "--render" : "")}";

        ProcessStartInfo start = new ProcessStartInfo
        {
            FileName = pythonPath,
            Arguments = $"{Path.Combine(Application.dataPath, "TripoSR/run.py")} {args}",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        pythonProcess = new Process {StartInfo = start};
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
        
        if (moveAndRename) UnityEditor.EditorApplication.delayCall += MoveAndRenameOutputFile;
        else if (autoAddMesh) UnityEditor.EditorApplication.delayCall += () => AddMeshToScene(null);

        UnityEditor.EditorApplication.delayCall += () => OnPythonProcessEnded?.Invoke();
    }

    private void MoveAndRenameOutputFile()
    {
        string originalPath = Path.Combine(Application.dataPath, "TripoSR/" + outputDir + "0/mesh.obj");
        string modelsDirectory = "Assets/"+moveAndRenamePath;
        string newFileName = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(images[0])) + ".obj";
        string newAssetPath = Path.Combine(modelsDirectory, newFileName);
        string newPath = Path.Combine(Application.dataPath, newAssetPath.Substring("Assets/".Length));


        if (!Directory.Exists(modelsDirectory)) Directory.CreateDirectory(modelsDirectory);

        if (File.Exists(originalPath))
        {
            if (File.Exists(newPath)) UnityEngine.Debug.LogWarning($"The file '{newPath}' already exists. Please move or rename, then run TripoSR again.");
            else {
                File.Move(originalPath, newPath);
                AssetDatabase.Refresh();

                UnityEngine.Debug.Log($"Moved and renamed mesh to path: {newPath}");
            }

            if (autoAddMesh) AddMeshToScene(newAssetPath);
        }
        else UnityEngine.Debug.Log($"File @ {originalPath} does not exist - cannot move and rename.");
    }

    private void AddMeshToScene(string path = null)
    {        
        string objPath = path ?? "Assets/TripoSR/" + outputDir + "0/mesh.obj";

        AssetDatabase.Refresh();
        AssetDatabase.ImportAsset(objPath, ImportAssetOptions.ForceUpdate);

        GameObject importedObj = AssetDatabase.LoadAssetAtPath<GameObject>(objPath);

        if (importedObj != null)
        {
            GameObject instantiatedObj = Instantiate(importedObj);
            instantiatedObj.name = importedObj.name;

            UnityEngine.Debug.Log("Instantiated GameObject prefab: " + instantiatedObj.name);

            if (autoFixRotation)
            {
                instantiatedObj.transform.GetChild(0).rotation = Quaternion.Euler(new Vector3(-90f, -90f, 0f));
            }
        }
        else UnityEngine.Debug.LogError("Failed to load the mesh at path: " + objPath);
    }


    void OnDisable() { if (pythonProcess != null && !pythonProcess.HasExited) pythonProcess.Kill(); }
}
