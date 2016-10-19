# DragonBones Unity Library

## [Demos](./Demos/)

## How to use
1. Create a Unity project and import [Demos files](./Demos/) to override the project files.
2. Import the [DragonBones common source code](../DragonBones/src/) to project Assets/Scripts.
3. Import the [DragonBones Unity source code](./src/) to project Assets/Scripts.
4. Import the [3rdParty source code](../3rdParty/) to project Assets/Scripts.
5. Run project and have fun.

## Notice
* Maker sure project structure like this:
```
project
    |-- Assets
        |-- Scripts
            |-- DragonBones
                |-- animation
                |-- armature
                |-- ...
                |-- unity
                |-- ...
            |-- 3rdParty Scripts
            |-- Demos Scripts
            |-- ...
        |-- Resources
            |-- DragonBonesData files
            |-- ...
        |-- ...
    |-- ...
```