# DragonBones Unity Library

<p align="center">
<h2 align="center"><a href="./README.md">English</a>          中文版</h2>
</p>


## 使用方法

1. 复制[DragonBones common source code](../DragonBones/src/)中的DragonBones目录到项目中的Assets/Scripts目录下。
2. 复制[DragonBones Unity source code](./src/)中的DragonBones目录替换覆盖到项目中的Assets/Scripts目录下。
3. 复制[3rdParty source code](../3rdParty/)中的rapidjson目录到项目的Assets/Scripts目录下。
4. 运行就行了。

*注意事项*
* 确保项目的结构如下:
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

## 如何创建一个可以使用龙骨动画的Untiy新项目

1. 在Unity中新建一个项目。
2. 复制[DragonBones core library](../DragonBones/src/)中的DragonBones目录和[unity library](./src/)中的DragonBones目录到项目的/Assets/Scripts/目录下。
3. 复制[3rdParty source code](../3rdParty/)中的rapidjson目录到项目的Assets/Scripts目录下。
4. 在DragonBones Pro制作动画，并导出DragonBones json格式（当前只支持到5.0的数据格式），复制导出的三个文件（两个json和一个png）到项目的/Assets/Resources/目录下。
5. 新建一个 C# 脚本，例如：
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
