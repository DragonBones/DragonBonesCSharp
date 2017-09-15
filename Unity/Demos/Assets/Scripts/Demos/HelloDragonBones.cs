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
    
    void Start()
    {
        var PI = DragonBones.Transform.PI;

        UnityFactory.factory.autoSearch = true;
		UnityFactory.factory.LoadData(dragonBoneData);        

        var armatureComponent = UnityFactory.factory.BuildArmatureComponent("ubbie");
        armatureComponent.animation.Play("stand");
        

        // Set position.
        armatureComponent.transform.localPosition = new Vector3(0.0f, 0.0f, 1.0f);

        //
        //Bone bone = armatureComponent.armature.GetBone("body");
        //bone.offset.skew = PI / 3.0f;

        // Set flip.
        //armatureComponent.armature.flipY = true;

        //--------------------------------------------------------

        /*var armatureComponent2 = UnityFactory.factory.BuildArmatureComponent("Dragon");
        armatureComponent2.animation.Play("walk");


        // Set position.
        armatureComponent2.transform.localPosition = new Vector3(-5.0f, 0.0f, 1.0f);

        //
        Bone bone2 = armatureComponent2.armature.GetBone("body");
        bone2.offset.skew = -PI / 3.0f;

        // Set flip.
        //armatureComponent2.armature.flipY = true;*/

        //----------------------------------------------------------------

        /*var armatureComponent3 = UnityFactory.factory.BuildArmatureComponent("Dragon");
        armatureComponent3.animation.Play("walk");


        // Set position.
        armatureComponent3.transform.localPosition = new Vector3(5.0f, 0.0f, 1.0f);

        //
        Bone bone3 = armatureComponent3.armature.GetBone("body");
        bone3.offset.skew = PI / 3.0f;

        // Set flip.
        //armatureComponent3.armature.flipY = true;*/
    }

}