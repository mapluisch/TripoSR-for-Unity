<h1 align="center">TripoSR for Unity</h1>
<div align="center">
  <img src="https://github.com/mapluisch/TripoSR-for-Unity/assets/31780571/4df6c785-8163-43e5-9c32-4b3ac7809ca8" style="width:90%">
  <p><em>Seamlessly generate high-quality 3D meshes from 2D images and retexture them using text-based texture generation directly in Unity</em>.</p>
</div>

## Overview
This project integrates [TripoSR](https://github.com/VAST-AI-Research/TripoSR), a "a state-of-the-art open-source model for fast feedforward 3D reconstruction from a single image" by StabilityAI and TripoAI, and [triposr-texture-gen](https://github.com/ejones/triposr-texture-gen) directly into Unity Editor. 

### Features
- Transformation of 2D images into textured 3D meshes, available both in Editor and in Playmode.
- Text-based texture generation, applied directly to the generated 3D meshes.

The generated 3D meshes are imported using a modified vertex color importer (based on [Andrew Raphael Lukasik's importer](https://gist.github.com/andrew-raphael-lukasik/3559728d022a4c96f491924f8285e1bf)) and auto-assigned to a base Material with custom shader to utilize and display the vertex colors correctly (without surface normals, though).

Tested with Unity 2022.3.22f1 running on Windows 11 and Ubuntu 22.04.

## Demo
The first demo clip shows the creation of a 3D mesh based on a 2D Texture within the Editor.

The generated meshes are colored (vertex colors), react to light, and (optionally) automatically use `MeshCollider` and `Rigidbody` for physics interaction.

https://github.com/mapluisch/TripoSR-for-Unity/assets/31780571/f27f62e0-00e3-4c14-8458-97302a82e76d

This clip shows the text-based retexturing of a TripoSR-generated 3D mesh:

https://github.com/mapluisch/TripoSR-for-Unity/assets/31780571/3e2fbb54-6828-49f0-80da-310bcedfc4c9

This demo clip shows 3D mesh generation in Playmode: 

https://github.com/mapluisch/TripoSR-for-Unity/assets/31780571/d6b85653-a672-495f-b268-f4996075a4c1


## Setup - TripoSR

### Integrating the .unitypackage into your project
0. Download the latest release `.unitypackage` and import it into your project (`Assets > Import Package`).
1. `cd` into the `Assets` folder of your Unity project (using Command Prompt, Terminal, ...) and clone the latest repo of TripoSR: `git clone https://github.com/VAST-AI-Research/TripoSR.git`.
2. After cloning TripoSR, `cd` into the TripoSR folder that you just created by cloning the repo.
3. Run `pip install --upgrade setuptools`, `pip install torch` (in case you don't have PyTorch installed) and `pip install -r requirements.txt`.
3. Add the `TripoSR` Prefab (found in `Assets > Prefabs`) to your scene.
4. Configure the path to your python executable in the `TripoSR` GameObject within your scene: For Windows, run `where python` within Command Prompt. For Unix, run `which python` within Terminal.
5. Configure the other public variables in the Inspector as needed.

### Using this repo's Unity project
0. Clone this repo.
1. Ensure you have Python installed on your system.
2. Run `pip install --upgrade setuptools`, `pip install torch` (in case you don't have PyTorch installed) and `pip install -r requirements.txt` from within this project's `Assets/TripoSR` folder.
3. In Unity, add the `TripoSR` Prefab to the scene (or simply open up my `SampleScene`).
4. Configure the path to your python executable in the `TripoSR` GameObject within your scene: For Windows, run `where python` within Command Prompt. For Unix, run `which python` within Terminal.
5. Configure the other public variables in the Inspector as needed.

## Setup - TextureGenerator
I've adapted the `text2texture.py` script from [triposr-texture-gen](https://github.com/ejones/triposr-texture-gen), so it doesn't open up the 3D viewer and works in-situ. When you clone the repo on your own, this won't be included.

### When using the .unitypackage in your project
0. Follow the general setup for TripoSR from above
1. `cd` into the `Assets` folder and clone the latest [triposr-texture-gen](https://github.com/ejones/triposr-texture-gen).
2. Run `pip install -r -requirements.txt` from within the `Assets/triposr-texture-gen` directory.
3. Run `pip install huggingface-hub --upgrade` to fix some compatibility problems.
4. Add the `TextureGenerator` Prefab to your scene.
5. Configure the path to your python executable.
6. Configure the public vars to your liking.

### When using this repo's Unity project
0. Follow the general setup from above
1. Run `pip install -r -requirements.txt` from within the `Assets/triposr-texture-gen` directory.
2. Run `pip install huggingface-hub --upgrade` to fix some compatibility problems.
3. Add the `TextureGenerator` Prefab to your scene (or use my `SampleScene`).
4. Configure the path to your python executable.
5. Configure the public vars to your liking.

   
## Usage - TripoSR
Once you have set up your scene with the `TripoSRForUnity` component and configured the parameters, you can run the process by clicking the `Run TripoSR` button in the inspector.

When you run TripoSR for the first time, the model weights will be downloaded and cached - this only occurs once; subsequent runs use the cached model.

### General Settings
- `autoAddMesh`: When enabled, the generated mesh is automatically added to the Unity scene as GameObject.
- `autoAddPhysicsComponents`: Whether or not to automatically add physics components (i.e., convex `MeshCollider` and `Rigidbody`) to the generated mesh.
- `autoFixRotation`: If enabled, will automatically correct the wrong object rotation after adding the mesh to the scene.
- `moveAndRename`: Moves and renames the output `.obj` file based on the input image's filename if enabled.
- `moveAndRenamePath`: The directory to which the `.obj` file will be moved. Must start with `Assets/`.
- `showDebugLogs`: Enables the display of debug outputs from the `run.py` script in the Unity console.

### TripoSR Parameters
All TripoSR parameters are exposed by my script. Feel free to change them as you see fit.

I've made some of them `ReadOnly` within the Inspector, since you shouldn't really change those vars (e.g. model name, device to use). You can still change them within the script of course.

## Usage - TextureGenerator
Likewise, you can run the texture generator by clicking on `Generate Textured Object` within the `TextureGenerator`-component in the inspector.

When you run the texture generator for the first time, model weights will also be downloaded and cached, which also only occurs the first time.

Set the model file in the inspector to your `.obj` from `Assets > Models`, describe the design you would like as model description, and start the generator.

## Known Issues
TripoSR `.obj`s only consist of `v`s and `f`s, surface normals are not calculated. When Unity calculates the normals upon import, they are not smoothed correctly (even when using high smoothing angles).

For now, I've disabled normal calculation - feel free to create a PR if you know how to correctly handle this issue.

## License
This project is licensed under the MIT License. See `LICENSE` for more information.

## Acknowledgments
Special thanks to StabilityAI, TripoAI, and contributors of the TripoSR project. 

Please cite their work adequately in case you use it in your own publications:

```BibTeX
@article{TripoSR2024,
  title={TripoSR: Fast 3D Object Reconstruction from a Single Image},
  author={Tochilkin, Dmitry and Pankratz, David and Liu, Zexiang and Huang, Zixuan and and Letts, Adam and Li, Yangguang and Liang, Ding and Laforte, Christian and Jampani, Varun and Cao, Yan-Pei},
  journal={arXiv preprint arXiv:2403.02151},
  year={2024}
}
```

Also special thanks to @ejones for [triposr-texture-gen](https://github.com/ejones/triposr-texture-gen).

