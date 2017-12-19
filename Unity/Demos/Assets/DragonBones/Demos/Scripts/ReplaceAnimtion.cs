using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DragonBones;

public class ReplaceAnimtion : BaseDemo
{
    private UnityArmatureComponent _armatureCompA;
    private UnityArmatureComponent _armatureCompB;
    private UnityArmatureComponent _armatureCompC;
    private UnityArmatureComponent _armatureCompD;

    // Use this for initialization
    protected override void OnStart()
    {
        // Load Data
        UnityFactory.factory.LoadDragonBonesData("mecha_2903/mecha_2903_ske");
        UnityFactory.factory.LoadTextureAtlasData("mecha_2903/mecha_2903_tex");

        // Build armature without animations
        this._armatureCompA = UnityFactory.factory.BuildArmatureComponent("mecha_2903");
        this._armatureCompB = UnityFactory.factory.BuildArmatureComponent("mecha_2903b");
        this._armatureCompC = UnityFactory.factory.BuildArmatureComponent("mecha_2903c");
		// Build armature with animations
        this._armatureCompD = UnityFactory.factory.BuildArmatureComponent("mecha_2903d");

		// Animation can be Shared to a armature without animation.
		var sourceArmature = UnityFactory.factory.GetArmatureData("mecha_2903d"); 
        UnityFactory.factory.ReplaceAnimation(this._armatureCompA.armature, sourceArmature);
        UnityFactory.factory.ReplaceAnimation(this._armatureCompB.armature, sourceArmature);
        UnityFactory.factory.ReplaceAnimation(this._armatureCompC.armature, sourceArmature);

        //
        this._armatureCompA.transform.localPosition = new Vector3(-4.0f, -3.0f, 0.0f);
        this._armatureCompB.transform.localPosition = new Vector3(0.0f, -3.0f, 0.0f);
        this._armatureCompC.transform.localPosition = new Vector3(4.0f, -3.0f, 0.0f);
        this._armatureCompD.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
    }

    // Update is called once per frame
    protected override void OnUpdate()
    {
        if (Input.GetMouseButtonDown(0))
        {
            //
            this.ChangeAnimtion();
        }
    }

    void ChangeAnimtion()
    {
		//
        var animationName = this._armatureCompD.animation.lastAnimationName;
        if (!string.IsNullOrEmpty(animationName))
        {
            var animationNames = this._armatureCompD.animation.animationNames;
            var animationIndex = (animationNames.IndexOf(animationName) + 1) % animationNames.Count;
            this._armatureCompD.animation.Play(animationNames[animationIndex]);
        }
        else
        {
            this._armatureCompD.animation.Play();
        }

        animationName = this._armatureCompD.animation.lastAnimationName;

        this._armatureCompA.animation.Play(animationName);
        this._armatureCompB.animation.Play(animationName);
        this._armatureCompC.animation.Play(animationName);
    }
}
