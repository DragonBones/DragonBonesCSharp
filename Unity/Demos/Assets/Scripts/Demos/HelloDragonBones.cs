using UnityEngine;
using DragonBones;
using System;
using System.Collections;

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

    [SerializeField]
    private UnityDragonBonesData skinData;
    [SerializeField]
    private UnityDragonBonesData weaponData;
    
    void Start()
    {
        //TestHello();
        //TestDragon();
        //TestLoadDemo();
        TestZorder();
        //TestCoreElement();
        //TestRotation();
    }

    private void TestRotation()
    {
        UnityFactory.factory.autoSearch = true;
        UnityFactory.factory.LoadDragonBonesData("temp/zhu_def_ske");
        UnityFactory.factory.LoadTextureAtlasData("temp/zhu_def_tex");

        var armatureComponent = UnityFactory.factory.BuildArmatureComponent("zhu_def", "zhu_def");
        armatureComponent.animation.Play("idle");
        //armatureComponent.armature.flipX = true;

        // Set position.
        armatureComponent.transform.localPosition = new Vector3(0.0f, -2.0f, 1.0f);
    }

    private void TestLoadDemo()
    {
        UnityFactory.factory.LoadDragonBonesData("NewDragon/NewDragon_ske");
        UnityFactory.factory.LoadTextureAtlasData("NewDragon/NewDragon_tex");

        var armatureComponent = UnityFactory.factory.BuildArmatureComponent("armatureName");
        armatureComponent.animation.Play("walk");
        //armatureComponent.armature.flipX = true;

        // Set position.
        armatureComponent.transform.localPosition = new Vector3(0.0f, -2.0f, 1.0f);      
    }

    private void TestDragon()
    {
        UnityFactory.factory.autoSearch = true;
        UnityFactory.factory.LoadDragonBonesData("tt/Dragon_ske");
        UnityFactory.factory.LoadTextureAtlasData("tt/Dragon_tex").autoSearch = true;

        var armatureComponent = UnityFactory.factory.BuildArmatureComponent("Dragon");
        armatureComponent.animation.Play("walk");
        //armatureComponent.armature.flipX = true;

        // Set position.
        armatureComponent.transform.localPosition = new Vector3(0.0f, -2.0f, 1.0f);
    }

    private void TestZorder()
    {
        UnityFactory.factory.autoSearch = true;
        //UnityFactory.factory.LoadDragonBonesData("temp/c/003/zhangfei_skeleton");
        //UnityFactory.factory.LoadTextureAtlasData("temp/c/003/zhangfei_texture").autoSearch = true;

        //var armatureComponent = UnityFactory.factory.BuildArmatureComponent("zhangfei");
        //var state = armatureComponent.animation.Play("cheer");

        UnityFactory.factory.LoadDragonBonesData("temp/c/008/boss");
        UnityFactory.factory.LoadTextureAtlasData("temp/c/008/boss_texture").autoSearch = true;

        var armatureComponent = UnityFactory.factory.BuildArmatureComponent("die");
        var state = armatureComponent.animation.Play("run01");
        //state.playTimes = 1;
        //armatureComponent.armature.flipX = true;

        // Set position.
        armatureComponent.transform.localPosition = new Vector3(0.0f, -2.0f, 1.0f);
    }

    private void TestCoreElement()
    {
        UnityFactory.factory.autoSearch = true;
        UnityFactory.factory.LoadData(dragonBoneData);
        UnityFactory.factory.LoadData(skinData);
        UnityFactory.factory.LoadData(weaponData);

        var armatureComponent = UnityFactory.factory.BuildArmatureComponent("mecha_1502b");
        armatureComponent.animation.Play("idle");

        armatureComponent.armature.flipX = true;

        //StartCoroutine(DestroySele(armatureComponent));
    }

    private IEnumerator DestroySele(UnityArmatureComponent com)
    {
        yield return new WaitForSeconds(2.0f);

        com.armature.Dispose();
        //Destroy(this.gameObject);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //TestHello();

            Debug.Log("TestHello");
        }
    }

    private void TestHello()
    {
        UnityFactory.factory.autoSearch = true;
        UnityFactory.factory.LoadData(dragonBoneData);

        var armatureComponent = UnityFactory.factory.BuildArmatureComponent("DragonBoy");
        armatureComponent.animation.Play("stand");

        // Set position.
        armatureComponent.transform.localPosition = new Vector3(0.0f, -2.0f, 1.0f);

        //StartCoroutine(DestroySele(armatureComponent));

        //
        //Bone bone = armatureComponent.armature.GetBone("body");
        //bone.offset.skew = PI / 3.0f;

        // Set flip.
        //armatureComponent.armature.flipX = true;
        //armatureComponent.armature.flipY = true;

        //--------------------------------------------------------

        /*var armatureComponent2 = UnityFactory.factory.BuildArmatureComponent("DragonBoy");
        armatureComponent2.animation.Play("walk");


        // Set position.
        armatureComponent2.transform.localPosition = new Vector3(-5.0f, 0.0f, 1.0f);

        //
        Bone bone2 = armatureComponent2.armature.GetBone("body");
        //bone2.offset.skew = -PI / 3.0f;

        armatureComponent2.transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
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