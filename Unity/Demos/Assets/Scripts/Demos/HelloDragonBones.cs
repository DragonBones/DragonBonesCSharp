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

        armatureComponent = UnityFactory.factory.BuildArmatureComponent("DragonBoy");
        armatureComponent.animation.Play("walk");

        // Set position.
        armatureComponent.transform.localPosition = new Vector3(0.0f, 0.0f, 1.0f);

        // Set flip.
        //armatureComponent.armature.flipX = false;

        TestBones();

        TestSlots();
    }

    private void TestBones()
    {
        var bones = armatureComponent.armature.GetBones();

        foreach (var bone in bones)
        {
            Debug.Log(string.Format("boneName:{0}  -x:{1}  -y:{2}", bone.name, bone.origin.x, bone.origin.y));
        }
    }

    private void TestSlots()
    {
        var slots = armatureComponent.armature.GetSlots();

        foreach (UnitySlot slot in slots)
        {
            Debug.Log(string.Format("slotName:{0} -x:{1} -y:{2}", slot.name, slot.renderDisplay.transform.position.x, slot.renderDisplay.transform.position.y));
        }
    }
    
}