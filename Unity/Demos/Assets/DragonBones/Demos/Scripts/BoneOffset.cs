using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DragonBones;

public class BoneOffset : BaseDemo
{

    protected override void OnStart()
    {
        // Load data
        UnityFactory.factory.LoadDragonBonesData("bullet_01/bullet_01_ske");
        UnityFactory.factory.LoadTextureAtlasData("bullet_01/bullet_01_tex");

        //
        for (var i = 0; i < 100; ++i)
        {
            var armatureComp = UnityFactory.factory.BuildArmatureComponent("bullet_01");
            armatureComp.AddDBEventListener(DragonBones.EventObject.COMPLETE, this._OnAnimationHandler);

            //
            this._MoveTo(armatureComp);
        }
    }

    private void _OnAnimationHandler(string type, EventObject eventObject)
    {
        this._MoveTo(eventObject.armature.proxy as UnityArmatureComponent);
    }

    private void _MoveTo(UnityArmatureComponent armatureComp)
    {
        var fromX = armatureComp.transform.localPosition.x;
        var fromY = armatureComp.transform.localPosition.y;
        var toX = Random.Range(0.0f, 1.0f) * Screen.width - Screen.width * 0.5f;
        var toY = Random.Range(0.0f, 1.0f) * Screen.height - Screen.height * 0.5f;
        var dX = toX - fromX;
        var dY = toY - fromY;
        var rootSlot = armatureComp.armature.GetBone("root");
        var bulletSlot = armatureComp.armature.GetBone("bullet");
        // Modify root and bullet bone offset.
        rootSlot.offset.scaleX = Mathf.Sqrt(dX * dX + dY * dY) / 100; // Bullet translate distance is 100 px.
        rootSlot.offset.rotation = Mathf.Atan2(dY, dX);
        rootSlot.offset.skew = Random.Range(0.0f, 1.0f) * Mathf.PI - Mathf.PI * 0.5f; // Random skew.
        bulletSlot.offset.scaleX = 0.5f + Random.Range(0.0f, 1.0f) * 0.5f; // Random scale.
        bulletSlot.offset.scaleY = 0.5f + Random.Range(0.0f, 1.0f) * 0.5f;
        // Update root and effect bone.
        rootSlot.InvalidUpdate();
        bulletSlot.InvalidUpdate();
        //
        armatureComp.animation.timeScale = 0.5f + Random.Range(0.0f, 1.0f) * 1.0f; // Random animation speed.
        armatureComp.animation.Play("idle", 1);
    }
}
