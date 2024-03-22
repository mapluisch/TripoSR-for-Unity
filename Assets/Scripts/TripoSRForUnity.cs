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
    private bool autoRotate = true;
    [SerializeField, Tooltip("If true, TripoSR's run.py debug output is printed to Unity's console.")]
    private bool showDebugLogs = true;

    [Header("TripoSR Parameters")]
    [SerializeField, Tooltip("Path to input image(s).")]
    private Texture2D[] images;

    [SerializeField, Tooltip("Device to use. Default: 'cuda:0'")]
    private string device = "cuda:0";

    [SerializeField, Tooltip("Path to the pretrained model. Default: 'stabilityai/TripoSR'")]
    private string pretrainedModelNameOrPath = "stabilityai/TripoSR";

    [SerializeField, Tooltip("Evaluation chunk size. Default: 8192")]
    private int chunkSize = 8192;

    [SerializeField, Tooltip("Marching cubes grid resolution. Default: 256")]
    private int marchingCubesResolution = 256;

    [SerializeField, Tooltip("If true, background will not be removed. Default: false")]
    private bool noRemoveBg = false;

    [SerializeField, Tooltip("Foreground to image size ratio. Default: 0.85")]
    private float foregroundRatio = 0.85f;

    [SerializeField, Tooltip("Output directory. Default: 'output/'")]
    private string outputDir = "output/";

    [SerializeField, Tooltip("Mesh save format. Default: 'obj'")]
    private string modelSaveFormat = "obj";

    [SerializeField, Tooltip("If true, saves a rendered video. Default: false")]
    private bool render = false;

    private Process pythonProcess;

    public void RunTripoSR()
    {
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
    }

    private void OnPythonProcessExited(object sender, EventArgs e)
    {
        if (autoAddMesh)
        {
            UnityEngine.Debug.Log("Auto adding mesh");
            UnityEditor.EditorApplication.delayCall += AddMeshToScene;
        }
    }

    void AddMeshToScene()
    {
        string objPath = "Assets/TripoSR/" + outputDir + "0/mesh.obj";

        AssetDatabase.Refresh(); 
        UnityEngine.Debug.Log("Asset Database refreshed.");

        AssetDatabase.ImportAsset(objPath, ImportAssetOptions.ForceUpdate);
        UnityEngine.Debug.Log("Asset re-import triggered for path: " + objPath);

        GameObject importedObj = AssetDatabase.LoadAssetAtPath<GameObject>(objPath);

        if (importedObj != null)
        {
            GameObject instantiatedObj = Instantiate(importedObj);
            instantiatedObj.name = importedObj.name;
            UnityEngine.Debug.Log("Instantiated GameObject prefab: " + instantiatedObj.name);

            if (autoRotate) instantiatedObj.transform.rotation = Quaternion.Euler(new Vector3(-90f, -90f, 0f));
        }
    }


    void OnDisable() { if (pythonProcess != null && !pythonProcess.HasExited) pythonProcess.Kill(); }
}
