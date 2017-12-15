using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DragonBones;

public class InverseKinematics : BaseDemo
{

    public UnityEngine.Transform floorBoard;
    public UnityEngine.Transform touchPoint;

    private UnityArmatureComponent _armatureComp;

    private Bone _weaponBone;
    private Bone _leftFootBone;
    private Bone _rightFootBone;

    private float _offsetRotation;

	private DragonBones.AnimationState _aimState;
	private int _faceDir = 0;
    private int _aimDir = 0;
    private float _aimRadian;

    protected override void OnStart()
    {
        //
        UnityFactory.factory.LoadDragonBonesData("core_element/mecha_1502b_ske");
        UnityFactory.factory.LoadTextureAtlasData("core_element/mecha_1502b_tex");
        //
        this._armatureComp = UnityFactory.factory.BuildArmatureComponent("mecha_1502b");
        //
        this._weaponBone = this._armatureComp.armature.GetBone("weapon_r");
        this._leftFootBone = this._armatureComp.armature.GetBone("foot_l");
        this._rightFootBone = this._armatureComp.armature.GetBone("foot_r");
        //
        this._armatureComp.animation.Play("walk");
        //
        EnableDrag(this.touchPoint.gameObject);
    }

    protected override void OnUpdate()
    {
        this.UpdateFoot();
        this.UpdateAim();
    }

    protected override void OnTouch(TouchType type)
    {
        if (type == TouchType.TOUCH_MOVE)
        {
            var minRadian = -30.0f * Mathf.Deg2Rad;
            var maxRadian = 20.0f * Mathf.Deg2Rad;
            var width = this.floorBoard.transform.localScale.x / 2.0f;
            this._offsetRotation = Mathf.Min(Mathf.Max(Mathf.Atan2(this._dragOffsetPosition.y, width), minRadian), maxRadian);

            // Set floor board rotation
            this.floorBoard.localEulerAngles = new Vector3(0.0f, 0.0f, -this._offsetRotation * Mathf.Rad2Deg);
        }
    }

    private void UpdateFoot()
    {
        // Set foot bone offset
        this._leftFootBone.offset.y = Mathf.Sin(_offsetRotation) * this._leftFootBone.global.x;
        this._rightFootBone.offset.y = Mathf.Sin(_offsetRotation) * this._rightFootBone.global.x;

        this._leftFootBone.offset.rotation = _offsetRotation;
        this._rightFootBone.offset.rotation = _offsetRotation;
    }

    private void UpdateAim()
    {
		var position = this._armatureComp.transform.localPosition;
        var aimOffsetY = this._weaponBone.global.y * this._armatureComp.transform.localScale.y;
		var mouseWorldPosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.farClipPlane));
        // this._aimRadian = Mathf.Atan2(Input.mousePosition.y - aimOffsetY, Input.mousePosition.x);

		this._aimRadian = Mathf.Atan2(-(mouseWorldPosition.y - position.y - aimOffsetY), mouseWorldPosition.x - position.x);

        var aimDir = 0;
        if (this._aimRadian > 0.0f)
        {
            aimDir = -1;
        }
        else
        {
            aimDir = 1;
        }

        if (this._aimDir != aimDir)
        {
            this._aimDir = aimDir;

            // Animation mixing.
            if (this._aimDir >= 0)
            {
                this._aimState = this._armatureComp.animation.FadeIn(
                    "aim_up", 0.01f, 1,
                    0, "aimGroup"
                );
            }
            else
            {
                this._aimState = this._armatureComp.animation.FadeIn(
                    "aim_down", 0.01f, 1,
                    0, "aimGroup"
                );
            }
        }

		//
		this._aimState.resetToPose = false;
        this._aimState.weight = Mathf.Abs(this._aimRadian / Mathf.PI * 2);
        this._armatureComp.armature.InvalidUpdate();
    }
}
