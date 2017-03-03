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
        UnityFactory.factory.LoadDragonBonesData("Test/superman_1_ske");
        UnityFactory.factory.LoadTextureAtlasData("Test/superman_1_tex");

        var armatureComponent = UnityFactory.factory.BuildArmatureComponent("Armature");
        armatureComponent.animation.Play("walk");

        // Set position.
        armatureComponent.transform.localPosition = new Vector3(5.0f, 0.0f, 1.0f);

        // Set flip.
        armatureComponent.armature.flipX = true;
    }
}