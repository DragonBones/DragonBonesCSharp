using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DragonBones;

public class DragonBonesEvent : BaseDemo
{
	private UnityArmatureComponent _mechaArmatureComp = null;

	[SerializeField]
	private AudioSource _sound;
    protected override void OnStart()
    {
		// Load Mecha Data
        UnityFactory.factory.LoadDragonBonesData("mecha_1004d/mecha_1004d_ske");
        UnityFactory.factory.LoadTextureAtlasData("mecha_1004d/mecha_1004d_tex");

		// Build Mecha Armature
        this._mechaArmatureComp = UnityFactory.factory.BuildArmatureComponent("mecha_1004d");

        this._mechaArmatureComp.transform.localPosition = new Vector3(0.0f, -2.0f, 0.0f);

		// Add animation event listener
		this._mechaArmatureComp.AddDBEventListener(EventObject.COMPLETE, this.OnAnimationEventHandler);
		// Add sound event listener
		UnityFactory.factory.soundEventManager.AddDBEventListener(EventObject.SOUND_EVENT, this.OnSoundEventHandler);

        this._mechaArmatureComp.animation.Play("walk");
    }

    // Update is called once per frame
    protected override void OnUpdate()
    {
		if(Input.GetMouseButtonDown(0))
		{
			this._mechaArmatureComp.animation.FadeIn("skill_03", 0.2f);
		}
    }
	//
	void OnSoundEventHandler(string type, EventObject eventObject)
	{
		UnityEngine.Debug.Log(eventObject.name);
		if(eventObject.name == "footstep")
		{
			this._sound.Play();
		}
	}
	//
	void OnAnimationEventHandler(string type, EventObject eventObject)
	{
		if(eventObject.animationState.name == "skill_03")
		{
			this._mechaArmatureComp.animation.FadeIn("walk", 0.2f);
		}
	}
}
