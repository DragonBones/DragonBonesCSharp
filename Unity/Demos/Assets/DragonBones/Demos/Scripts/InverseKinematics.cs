using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DragonBones;

public class InverseKinematics : BaseDemo
{
    private UnityArmatureComponent _armatureComp;
    private UnityArmatureComponent _floorBoardComp;

    private Bone _chestBone;
    private Bone _leftFootBone;
    private Bone _rightFootBone;
    private Bone _circleBone;
    private Bone _floorBoardBone;

    private DragonBones.AnimationState _aimState;

    private float _offsetRotation;
    private int _faceDir = 0;
    private float _aimRadian;

    protected override void OnStart()
    {
        // Load data
        UnityFactory.factory.LoadDragonBonesData("mecha_1406/mecha_1406_ske");
        UnityFactory.factory.LoadTextureAtlasData("mecha_1406/mecha_1406_tex");
        UnityFactory.factory.LoadDragonBonesData("floor_board/floor_board_ske");
        UnityFactory.factory.LoadTextureAtlasData("floor_board/floor_board_tex");
        // Build armature
        this._armatureComp = UnityFactory.factory.BuildArmatureComponent("mecha_1406");
        this._floorBoardComp = UnityFactory.factory.BuildArmatureComponent("floor_board");
        // Get bone
        this._chestBone = this._armatureComp.armature.GetBone("chest");
        this._leftFootBone = this._armatureComp.armature.GetBone("foot_l");
        this._rightFootBone = this._armatureComp.armature.GetBone("foot_r");
        this._circleBone = this._floorBoardComp.armature.GetBone("circle");
        this._floorBoardBone = this._floorBoardComp.armature.GetBone("floor_board");
        // Play animation
        this._armatureComp.animation.Play("idle");
        this._aimState = this._armatureComp.animation.FadeIn("aim", 0.1f, 1, 0, "aimGroup");
        this._aimState.resetToPose = false;
        this._aimState.Stop();
        //
        this._floorBoardComp.animation.Play("idle");
        this._floorBoardComp.armature.GetSlot("player").display = this._armatureComp.gameObject;
        // Set localPosition
        this._armatureComp.transform.localPosition = Vector3.zero;
        this._floorBoardComp.transform.localPosition = new Vector4(0.0f, -0.25f, 0.0f);

        //
        this._floorBoardComp.CloseCombineMeshs();

        //
        EnableDrag(this._floorBoardComp.armature.GetSlot("circle").display as GameObject);
    }

    protected override void OnUpdate()
    {
        this._UpdateAim();
        this._UpdateFoot();
    }

    private void _UpdateAim()
    {
        var mouseWorldPosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.farClipPlane));
        var position = this._armatureComp.transform.localPosition;
        var aimOffsetY = this._chestBone.global.y * this.transform.localScale.y;

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

    private void _UpdateFoot()
    {
        var minRadian = -25.0f * Mathf.Deg2Rad;
        var maxRadian = 25.0f * Mathf.Deg2Rad;
        var circleRadian = Mathf.Atan2(-this._circleBone.global.y, this._circleBone.global.x);

        if (this._circleBone.global.x < 0.0)
        {
            circleRadian = DragonBones.Transform.NormalizeRadian(circleRadian + Mathf.PI);
        }

        this._offsetRotation = Mathf.Min(Mathf.Max(circleRadian, minRadian), maxRadian);
        this._floorBoardBone.offset.rotation = this._offsetRotation;
        this._floorBoardBone.InvalidUpdate();

        // Set foot bone offset
        var tan = Mathf.Tan(this._offsetRotation);
        var sinR = 1.0f / Mathf.Sin(Mathf.PI * 0.5f - this._offsetRotation) - 1.0f;

        this._leftFootBone.offset.y = tan * this._leftFootBone.global.x + this._leftFootBone.origin.y * sinR;
        this._leftFootBone.offset.rotation = this._offsetRotation * this._faceDir;
        this._leftFootBone.InvalidUpdate();
        //
        this._rightFootBone.offset.y = tan * this._rightFootBone.global.x + this._rightFootBone.origin.y * sinR;
        this._rightFootBone.offset.rotation = this._offsetRotation * this._faceDir;
        this._rightFootBone.InvalidUpdate();
    }

}
