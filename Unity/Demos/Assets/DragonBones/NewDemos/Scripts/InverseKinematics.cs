using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DragonBones;

public class InverseKinematics : BaseDemo
{
    private UnityArmatureComponent _armatureComp;
    private UnityArmatureComponent _floorBoardComp;

    private Bone _weaponBone;
    private Bone _leftFootBone;
    private Bone _rightFootBone;

    private DragonBones.AnimationState _aimState;

    private float _offsetRotation;
    private int _faceDir = 0;
    private float _aimRadian;

    protected override void OnStart()
    {
        // Load data
        UnityFactory.factory.LoadDragonBonesData("core_element/mecha_1502b_ske");
        UnityFactory.factory.LoadTextureAtlasData("core_element/mecha_1502b_tex");
        UnityFactory.factory.LoadDragonBonesData("floor_board/floor_board_ske");
        UnityFactory.factory.LoadTextureAtlasData("floor_board/floor_board_tex");
        // Build armature
        this._armatureComp = UnityFactory.factory.BuildArmatureComponent("mecha_1502b");
        this._floorBoardComp = UnityFactory.factory.BuildArmatureComponent("floor_board");
        // Get bone
        this._weaponBone = this._armatureComp.armature.GetBone("weapon_r");
        this._leftFootBone = this._armatureComp.armature.GetBone("foot_l");
        this._rightFootBone = this._armatureComp.armature.GetBone("foot_r");
        // Play animation
        this._armatureComp.animation.Play("idle");
        this._aimState = this._armatureComp.animation.FadeIn("aim", -1, 0, 1, "aimGroup");
        this._aimState.Stop();
        this._aimState.resetToPose = false;

        // Set localPosition
        this._armatureComp.transform.localPosition = Vector3.zero;
        this._floorBoardComp.transform.localPosition = new Vector4(0.0f, -0.25f, 0.0f);

        //
        EnableDrag(this._floorBoardComp.armature.GetSlot("circle").display as GameObject);
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
            var width = this._floorBoardComp.transform.localScale.x / 2.0f;

            this._offsetRotation = Mathf.Min(Mathf.Max(Mathf.Atan2(this._dragOffsetPosition.y, width), minRadian), maxRadian);

            // Set floor board rotation
            var floor_board = this._floorBoardComp.armature.GetSlot("floor_board").display as GameObject;
            floor_board.transform.localEulerAngles = new Vector3(0.0f, 0.0f, -this._offsetRotation * Mathf.Rad2Deg);
        }
    }

    private void UpdateFoot()
    {
        // Set foot bone offset
        this._leftFootBone.offset.y = Mathf.Sin(_offsetRotation) * this._leftFootBone.global.x;
        this._rightFootBone.offset.y = Mathf.Sin(_offsetRotation) * this._rightFootBone.global.x;
        //
        this._leftFootBone.offset.rotation = _offsetRotation * this._faceDir;
        this._rightFootBone.offset.rotation = _offsetRotation * this._faceDir;
    }

    private void UpdateAim()
    {
        var mouseWorldPosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.farClipPlane));
        var position = this._armatureComp.transform.localPosition;
        var aimOffsetY = _armatureComp.armature.GetBone("chest").global.y * this.transform.localScale.y;

        this._faceDir = mouseWorldPosition.x > 0.0f ? 1 : -1;
        this._armatureComp.armature.flipX = this._faceDir < 0;

        if (this._faceDir > 0)
        {
            this._aimRadian = Mathf.Atan2(-(mouseWorldPosition.y - position.y - aimOffsetY), mouseWorldPosition.x - position.x);
        }
        else
        {
            this._aimRadian = Mathf.PI - Mathf.Atan2(-(mouseWorldPosition.y - position.y - aimOffsetY), mouseWorldPosition.x - position.x);
            if (_aimRadian > Mathf.PI)
            {
                _aimRadian -= Mathf.PI * 2.0f;
            }
        }

        // Calculate progress
        var progress = Mathf.Abs((this._aimRadian + Mathf.PI / 2) / Mathf.PI);
        // Set currentTime
        this._aimState.currentTime = progress * (this._aimState.totalTime);
    }
}
