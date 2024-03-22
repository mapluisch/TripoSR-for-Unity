using UnityEngine;
using System.Diagnostics;
using UnityEditor;
using System.IO;
using System.Globalization;

public class TripoSRForUnity : MonoBehaviour
{
    [Header("General Settings")]
    [SerializeField, Tooltip("If true, TripoSR's run.py debug output is printed to Unity's console.")]
    private bool showDebugLogs = true;
    [SerializeField, Tooltip("Path to your Python executable")]
    private string pythonPath = "/usr/bin/python";

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
                      $"--model-save-format {modelSaveFormat} " +
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

    void OnDisable() { if (pythonProcess != null && !pythonProcess.HasExited) pythonProcess.Kill(); }
}
