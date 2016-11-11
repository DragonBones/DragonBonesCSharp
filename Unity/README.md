# DragonBones Unity Library

## How to use [Demos](./Demos/)

1. Create a Unity project and import [Demos files](./Demos/) to override the project files.
2. Import the [DragonBones common source code](../DragonBones/src/) to project Assets/Scripts.
3. Import the [DragonBones Unity source code](./src/) to project Assets/Scripts.
4. Import the [3rdParty source code](../3rdParty/) to project Assets/Scripts.
5. Run project and have fun.

*Notice*
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

## How to create a new Unity project with DragonBones animation

1. Create a new project in Unity.
2. Import DragonBones [core library](../DragonBones/src/) and [unity library](./src/) into your project "[Project]/Assets/Scripts/".
3. Import the [3rdParty source code](../3rdParty/) to your project "[Project]/Assets/Scripts/".
4. Export DB animation files with DragonBones Pro to your project "[Project]/Assets/Resources/".
5. Create a new C# script like following:
```
public class HelloDragonBones :MonoBehaviour
{
    void Start()
    {
        // Load data.
       UnityFactory.factory.LoadDragonBonesData("Ubbie/Ubbie"); // DragonBones file path (without suffix)
       UnityFactory.factory.LoadTextureAtlasData("Ubbie/texture"); //Texture atlas file path (without suffix) 
        // Create armature.
        var armatureComponent =UnityFactory.factory.BuildArmatureComponent("ubbie"); // Input armature name
        // Play animation.
       armatureComponent.animation.Play("walk");
        
        // Change armatureposition.
       armatureComponent.transform.localPosition = new Vector3(0.0f, 0.0f,0.0f);
    }
}
```
