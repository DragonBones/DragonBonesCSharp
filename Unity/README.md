# DragonBones Unity Library
[中文 README](./README-zh_CN.md)
## [Demos](./Demos/)
* [Hello DragonBones](./Demos/Assets/DragonBones/Demos/Scripts/HelloDragonBones.cs)

## How to use
1. Create a Unity project.
2. Copy [DragonBones common source code](../DragonBones/src/), [Dragonbones Unity source code](./src/), [3rdParty source code](../3rdParty/) all folders and files to the project's Assets/Scripts folder.
3. Run project and have fun.

## Notice
* If you are upgrading the overwrite source file, there may be changes to the folder or file name, you need to pay attention to the following several situations:
    * Check for additional legacy source folders and file residues that cause redefinition and recommend that you delete all old source folders and files before upgrading.
    * Check other compilation errors that may result from folders or files name change.
* Maker sure project structure like this:
```
Your project
    |-- Assets
        |-- DragonBones
            |-- Demos (You can remove it if you don't need.)
            |-- Scripts        
                |-- 3rdParty
                |-- animation
                |-- armature
                |-- ...
                |-- unity
                |-- ...
            |-- Editor
            |-- Resources
                |-- Shaders files
                |-- ...
            |-- ...
        |-- Resources
            |-- DragonBonesData files
            |-- ...
        |-- Scripts
        |-- ...
    |-- ...
```