using UnityEngine;
using DragonBones;

/**
 * How to use
 * 1. Load and parse data.
 *    factory.LoadDragonBonesData("DragonBonesDataPath");
 *    factory.LoadTextureAtlasData("TextureAtlasDataPath");
 *    
 * 2. Build armature.
 *    armatureComponent = factory.BuildArmatureComponent("armatureName");
 * 
 * 3. Play animation.
 *    armatureComponent.animation.Play("animationName");
 */
public class HelloDragonBones :MonoBehaviour
{
    void Start()
    {
        UnityFactory.factory.LoadDragonBonesData("DragonBoy/DragonBoy");
        UnityFactory.factory.LoadTextureAtlasData("DragonBoy/DragonBoy_texture_1");

        var armatureComponent = UnityFactory.factory.BuildArmatureComponent("DragonBoy");
        armatureComponent.animation.Play("walk");

        // Set position.
        armatureComponent.transform.localPosition = new Vector3(5.0f, 0.0f, 1.0f);

        // Set flip.
        armatureComponent.armature.flipX = true;
    }
}