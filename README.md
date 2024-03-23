<h1 align="center">TripoSR for Unity</h1>
<div align="center">
  <img src="https://github.com/mapluisch/TripoSR-for-Unity/assets/31780571/bef716b7-5a3a-43a7-85b2-35312371d132">
  <p>Seamlessly generate high-quality 3D meshes from 2D images within Unity using <a href="https://github.com/VAST-AI-Research/TripoSR">TripoSR by StabilityAI and TripoAI</a>.</p>
</div>

## Overview
This project integrates TripoSR, a "a state-of-the-art open-source model for fast feedforward 3D reconstruction from a single image" by StabilityAI and TripoAI, directly into the Unity Editor. 

Thus, this project enables the transformation of 2D images into textured 3D meshes within Unity (both in Editor and in Playmode), useful for various applications such as game asset creation or rapid prototyping. 

The generated 3D meshes are imported using a modified vertex color importer (based on [Andrew Raphael Lukasik's importer](https://gist.github.com/andrew-raphael-lukasik/3559728d022a4c96f491924f8285e1bf)) and auto-assigned to a base Material with custom shader to utilize and display the vertex colors correctly with normal lighting.

Tested with Unity 2022.3.22f1 running on Windows 11 and Ubuntu 22.04.

## Demo
This demo clip shows the creation of a 3D mesh based on a 2D Texture in realtime:

https://github.com/mapluisch/TripoSR-for-Unity/assets/31780571/ab54b1f7-1bd3-4a11-9b79-68757d6dc47e

## Setup
1. Ensure you have Python installed on your system.
2. Run `pip install --upgrade setuptools`, `pip install torch` (in case you don't have PyTorch installed) and `pip install -r requirements.txt` from within this project's `Assets/TripoSR` folder.
3. In Unity, create a GameObject and attach the `TripoSRForUnity` script (or simply open up my `SampleScene`).
4. Configure the path to your python executable: For Windows, run `where python` within Command Prompt. For Unix, run `which python` within Terminal.
5. Configure the other public variables in the Inspector as needed.
   
## Usage
Once you have set up your scene with the `TripoSRForUnity` component and configured the parameters, you can run the process by clicking the `Run TripoSR` button in the inspector.

When you run TripoSR for the first time, the model weights will be downloaded and cached - this only occurs once.

### General Settings
- `autoAddMesh`: When enabled, the generated mesh is automatically added to the Unity scene as GameObject.
- `autoFixRotation`: If enabled, will automatically correct the wrong object rotation after adding the mesh to the scene.
- `moveAndRename`: Moves and renames the output `.obj` file based on the input image's filename if enabled.
- `moveAndRenamePath`: The directory to which the `.obj` file will be moved. Must start with `Assets/`.
- `showDebugLogs`: Enables the display of debug outputs from the `run.py` script in the Unity console.

### TripoSR Parameters
All TripoSR parameters are exposed by my script. Feel free to change them as you see fit.

## Contributions
Contributions are welcome! Feel free to open issues for bugs/features or submit pull requests.

## License
This project is licensed under the MIT License. See `LICENSE` for more information.

## Acknowledgments
Special thanks to Stability AI and contributors of the TripoSR project. 

Please cite their work adequately in case you use it in your own projects:

```BibTeX
@article{TripoSR2024,
  title={TripoSR: Fast 3D Object Reconstruction from a Single Image},
  author={Tochilkin, Dmitry and Pankratz, David and Liu, Zexiang and Huang, Zixuan and and Letts, Adam and Li, Yangguang and Liang, Ding and Laforte, Christian and Jampani, Varun and Cao, Yan-Pei},
  journal={arXiv preprint arXiv:2403.02151},
  year={2024}
}
```

