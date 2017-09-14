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
	public UnityDragonBonesData dragonBoneData;

    private UnityArmatureComponent armatureComponent;
    void Start()
    {
        UnityFactory.factory.autoSearch = true;
		UnityFactory.factory.LoadData(dragonBoneData);

        //UnityFactory.factory.load

        armatureComponent = UnityFactory.factory.BuildArmatureComponent("DragonBoy");
        armatureComponent.animation.Play("walk");

        // Set position.
        armatureComponent.transform.localPosition = new Vector3(0.0f, 0.0f, 1.0f);

        // Set flip.
        //armatureComponent.armature.flipY = true;
    }
    
}