using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DragonBones;

public class AnimationBase : BaseDemo
{
    private UnityArmatureComponent _armatureComp;

    protected override void OnStart()
    {
        // Load Data
        UnityFactory.factory.LoadDragonBonesData("progress_bar/progress_bar_ske");
        UnityFactory.factory.LoadTextureAtlasData("progress_bar/progress_bar_tex");

        // Build Armature
        this._armatureComp = UnityFactory.factory.BuildArmatureComponent("progress_bar");
		// Add Listeners
        this._armatureComp.AddDBEventListener(EventObject.START, this.OnAnimationEventHandler);
        this._armatureComp.AddDBEventListener(EventObject.LOOP_COMPLETE, this.OnAnimationEventHandler);
        this._armatureComp.AddDBEventListener(EventObject.COMPLETE, this.OnAnimationEventHandler);
        this._armatureComp.AddDBEventListener(EventObject.FADE_IN, this.OnAnimationEventHandler);
        this._armatureComp.AddDBEventListener(EventObject.FADE_IN_COMPLETE, this.OnAnimationEventHandler);
        this._armatureComp.AddDBEventListener(EventObject.FADE_OUT, this.OnAnimationEventHandler);
        this._armatureComp.AddDBEventListener(EventObject.FADE_OUT_COMPLETE, this.OnAnimationEventHandler);
        this._armatureComp.AddDBEventListener(EventObject.FRAME_EVENT, this.OnAnimationEventHandler);

		this._armatureComp.animation.Play("idle");
    }

	protected override void OnTouch(TouchType type)
	{
		var mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		var localPosition = this._armatureComp.transform.localPosition;
		var progress = (mouseWorldPosition.x - localPosition.x + 3.0f) / 6.0f;
		progress = Mathf.Min(Mathf.Max(progress, 0.0f), 1.0f);
		switch(type)
		{
			case TouchType.TOUCH_BEGIN:
			{
				this._armatureComp.animation.GotoAndStopByProgress("idle", progress);
			}
			break;
			case TouchType.TOUCH_END:
			{
				this._armatureComp.animation.Play();
			}
			break;
			case TouchType.TOUCH_MOVE:
			{
				var animationState = this._armatureComp.animation.GetState("idle");
				if(animationState != null)
				{
					animationState.currentTime = animationState.totalTime * progress;
				}
			}
			break;
		}
	}

    private void OnAnimationEventHandler(string type, EventObject eventObject)
    {
		UnityEngine.Debug.Log(string.Format("animationName:{0},eventType:{1},eventName:{2}", eventObject.animationState.name, type, eventObject.name));
    }
}
