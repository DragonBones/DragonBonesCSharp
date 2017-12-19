using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DragonBones;

public class HelloDragonBones : BaseDemo
{
    //
    public UnityDragonBonesData dragonBoneData;
    protected override void OnStart()
    {
        // 1.Load and parse data
        if (true)
        {
            // Scheme 1: Load UnityDragonBonesData
            UnityFactory.factory.LoadData(this.dragonBoneData);
        }
        else
        {
            // Scheme 2: Load JsonData in Resources
            // UnityFactory.factory.LoadDragonBonesData("mecha_1002_101d/mecha_1002_101d_ske");
            // UnityFactory.factory.LoadTextureAtlasData("mecha_1002_101d/mecha_1002_101d_tex");
        }

        // 2.Build armature
        var armatureComponent = UnityFactory.factory.BuildArmatureComponent("mecha_1002_101d");

        // 3.Play animation
        armatureComponent.animation.Play("idle");

        // Set name
        armatureComponent.name = "dynamic_mecha_1002_101d";

        // Set position.
        armatureComponent.transform.localPosition = new Vector3(3.0f, -1.5f, 1.0f);
    }
}
