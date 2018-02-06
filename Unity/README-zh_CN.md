# DragonBones Unity Library
[README in English](./README.md)
## [示例](./Demos/)
* [Hello DragonBones](./Demos/Assets/DragonBones/Demos/Scripts/HelloDragonBones.cs)
* 运行示例只需要使用 Unity 将 [示例文件夹](./Demos/) 当作项目打开。

## 使用方法
1. 创建一个 Unity 项目或使用上述示例项目。
2. 分别复制 [DragonBones 公共库源码](../DragonBones/src/)、[DragonBones Unity 库源码](./src/)、[第三方库源码](../3rdParty/) 中的所有文件夹和文件到项目的 Assets/Scripts 文件夹下。
3. 运行项目。

## 注意事项
* 如果是升级覆盖源码文件，可能会有文件夹或文件名的变更，需要注意下面几种情况：
    * 检查是否有多余的旧源码文件夹和文件残留而导致重定义，建议升级之前先删除所有的旧源码文件夹和文件。
    * 检查其他可能由于文件夹或文件名变更而导致的编译错误。
* 确保项目结构如下:
```
Your project
    |-- Assets
        |-- DragonBones
            |-- Demos (如果不需要，可以删除。)
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
```-- ...
```