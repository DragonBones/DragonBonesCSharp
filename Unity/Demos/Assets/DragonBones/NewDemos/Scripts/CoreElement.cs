using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DragonBones;

public class CoreElement : BaseDemo
{
    public const float G = -0.005f;
    public const float GROUND = 0.0f;

    public const KeyCode left = KeyCode.A;
    public const KeyCode right = KeyCode.D;
    public const KeyCode up = KeyCode.W;
    public const KeyCode down = KeyCode.S;
    public const KeyCode switchSkin = KeyCode.Space;
    public const KeyCode switchLeftWeapon = KeyCode.Q;
    public const KeyCode switchRightWeapon = KeyCode.E;

    private Mecha _player;

    protected override void OnStart()
    {
        // Load data
        UnityFactory.factory.LoadDragonBonesData("mecha_1502b/mecha_1502b_ske");
        UnityFactory.factory.LoadTextureAtlasData("mecha_1502b/mecha_1502b_tex");
        UnityFactory.factory.LoadDragonBonesData("skin_1502b/skin_1502b_ske");
        UnityFactory.factory.LoadTextureAtlasData("skin_1502b/skin_1502b_tex");
        UnityFactory.factory.LoadDragonBonesData("weapon_1000/weapon_1000_ske");
        UnityFactory.factory.LoadTextureAtlasData("weapon_1000/weapon_1000_tex");

        //
        this._player = new Mecha();
    }

    protected override void OnUpdate()
    {
        // Input 
        var isLeft = Input.GetKey(left);
        var isRight = Input.GetKey(right);

        if (isLeft == isRight)
        {
            this._player.Move(0);
        }
        else if (isLeft)
        {
            this._player.Move(-1);
        }
        else
        {
            this._player.Move(1);
        }

        if (Input.GetKeyDown(up))
        {
            this._player.Jump();
        }

        this._player.Squat(Input.GetKey(down));

        if (Input.GetKeyDown(switchSkin))
        {
            this._player.SwitchSkin();
        }

        if (Input.GetKeyDown(switchLeftWeapon))
        {
            this._player.SwitchWeaponL();
        }

        if (Input.GetKeyDown(switchRightWeapon))
        {
            this._player.SwitchWeaponR();
        }

        var target = Camera.main.ScreenToWorldPoint(Input.mousePosition + new Vector3(0.0f, 0.0f, Camera.main.farClipPlane));
        this._player.Aim(target.x, target.y);

        this._player.Attack(Input.GetMouseButton(0));
        this._player.Update();
    }
}

public class Mecha
{
    private const float JUMP_SPEED = -0.2f;
    private const float NORMALIZE_MOVE_SPEED = 0.03f;
    private const float MAX_MOVE_SPEED_FRONT = NORMALIZE_MOVE_SPEED * 1.4f;
    private const float MAX_MOVE_SPEED_BACK = NORMALIZE_MOVE_SPEED * 1.0f;
    private const string NORMAL_ANIMATION_GROUP = "normal";
    private const string AIM_ANIMATION_GROUP = "aim";
    private const string ATTACK_ANIMATION_GROUP = "attack";

    private static readonly string[] WEAPON_L_LIST = { "weapon_1502b_l", "weapon_1005", "weapon_1005b", "weapon_1005c", "weapon_1005d", "weapon_1005e" };
    private static readonly string[] WEAPON_R_LIST = { "weapon_1502b_r", "weapon_1005", "weapon_1005b", "weapon_1005c", "weapon_1005d" };
    private static readonly string[] SKINS = { "mecha_1502b", "skin_a", "skin_b", "skin_c" };

    private bool _isJumpingA = false;
    private bool _isJumpingB = false;
    private bool _isSquating = false;
    private bool _isAttackingA = false;
    private bool _isAttackingB = false;
    private int _weaponRIndex = 0;
    private int _weaponLIndex = 0;
    private int _skinIndex = 0;
    private int _faceDir = 1;
    private int _aimDir = 0;
    private int _moveDir = 0;
    private float _aimRadian = 0.0f;
    private float _speedX = 0.0f;
    private float _speedY = 0.0f;
    private DragonBones.Armature _armature;
    private UnityArmatureComponent _armatureComponent;
    private DragonBones.Armature _weaponL;
    private DragonBones.Armature _weaponR;
    private DragonBones.AnimationState _aimState = null;
    private DragonBones.AnimationState _walkState = null;
    private DragonBones.AnimationState _attackState = null;
    private Vector2 _target = Vector2.zero;
    // private Vector2 _helpPoint = Vector2.zero;

    public Mecha()
    {
        this._armatureComponent = UnityFactory.factory.BuildArmatureComponent("mecha_1502b");
        this._armature = this._armatureComponent.armature;

        this._armatureComponent.transform.localPosition = new Vector3(0.0f, CoreElement.GROUND, 0.0f);

        this._armatureComponent.AddDBEventListener(DragonBones.EventObject.FADE_IN_COMPLETE, this._OnAnimationEventHandler);
        this._armatureComponent.AddDBEventListener(DragonBones.EventObject.FADE_OUT_COMPLETE, this._OnAnimationEventHandler);
        this._armatureComponent.AddDBEventListener(DragonBones.EventObject.COMPLETE, this._OnAnimationEventHandler);

        //
        this._weaponL = this._armature.GetSlot("weapon_l").childArmature;
        this._weaponR = this._armature.GetSlot("weapon_r").childArmature;
        this._weaponL.eventDispatcher.AddDBEventListener(DragonBones.EventObject.FRAME_EVENT, this._OnFrameEventHandler);
        this._weaponR.eventDispatcher.AddDBEventListener(DragonBones.EventObject.FRAME_EVENT, this._OnFrameEventHandler);

        this._UpdateAnimation();
    }

    public void Move(int dir)
    {
        if (this._moveDir == dir)
        {
            return;
        }

        this._moveDir = dir;
        this._UpdateAnimation();
    }

    public void Jump()
    {
        if (this._isJumpingA)
        {
            return;
        }
        //
        this._isJumpingA = true;
        this._armature.animation.FadeIn("jump_1", -1.0f, -1, 0, NORMAL_ANIMATION_GROUP).resetToPose = false;
        this._walkState = null;
    }

    public void Squat(bool isSquating)
    {
        if (this._isSquating == isSquating)
        {
            return;
        }

        this._isSquating = isSquating;
        this._UpdateAnimation();
    }

    public void Attack(bool isAttacking)
    {
        if (this._isAttackingA == isAttacking)
        {
            return;
        }

        this._isAttackingA = isAttacking;
    }

    public void SwitchWeaponL()
    {
        this._weaponL.eventDispatcher.RemoveDBEventListener(DragonBones.EventObject.FRAME_EVENT, this._OnFrameEventHandler);

        this._weaponLIndex++;
        this._weaponLIndex %= WEAPON_L_LIST.Length;
        var weaponName = WEAPON_L_LIST[this._weaponLIndex];
        this._weaponL = UnityFactory.factory.BuildArmature(weaponName);
        this._armature.GetSlot("weapon_l").childArmature = this._weaponL;
        this._weaponL.eventDispatcher.AddDBEventListener(DragonBones.EventObject.FRAME_EVENT, this._OnFrameEventHandler);
    }

    public void SwitchWeaponR()
    {
        this._weaponR.eventDispatcher.RemoveDBEventListener(DragonBones.EventObject.FRAME_EVENT, this._OnFrameEventHandler);

        this._weaponRIndex++;
        this._weaponRIndex %= WEAPON_R_LIST.Length;
        var weaponName = WEAPON_R_LIST[this._weaponRIndex];
        this._weaponR = UnityFactory.factory.BuildArmature(weaponName);
        this._armature.GetSlot("weapon_r").childArmature = this._weaponR;
        this._weaponR.eventDispatcher.AddDBEventListener(DragonBones.EventObject.FRAME_EVENT, this._OnFrameEventHandler);
    }

    public void SwitchSkin()
    {
        this._skinIndex++;
        this._skinIndex %= SKINS.Length;
        var skinName = SKINS[this._skinIndex];
        var skinData = UnityFactory.factory.GetArmatureData(skinName).defaultSkin;
        List<string> exclude = new List<string>();
        exclude.Add("weapon_l");
        exclude.Add("weapon_r");
        UnityFactory.factory.ReplaceSkin(this._armature, skinData, false, exclude);
    }

    public void Aim(float x, float y)
    {
        this._target.x = x;
        this._target.y = y;
    }

    public void Update()
    {
        this._UpdatePosition();
        this._UpdateAim();
        this._UpdateAttack();
    }

    private void _UpdatePosition()
    {
        if (this._speedX == 0.0f && !_isJumpingB)
        {
            return;
        }

        var position = this._armatureComponent.transform.localPosition;

        if (this._speedX != 0.0f)
        {
            position.x += this._speedX * this._armatureComponent.animation.timeScale;

            if (position.x < -4.0f)
            {
                position.x = -4.0f;
            }
            else if (position.x > 4.0f)
            {
                position.x = 4.0f;
            }
        }

        if (this._isJumpingB)
        {
            if (this._speedY > -0.05f && this._speedY + CoreElement.G <= -0.05f)
            {
                this._armatureComponent.animation.FadeIn("jump_3", -1.0f, -1, 0, NORMAL_ANIMATION_GROUP).resetToPose = false;
            }

            this._speedY += CoreElement.G;
            position.y += this._speedY * this._armatureComponent.animation.timeScale;

            if (position.y < CoreElement.GROUND)
            {
                position.y = CoreElement.GROUND;
                this._isJumpingA = false;
                this._isJumpingB = false;
                this._speedX = 0.0f;
                this._speedY = 0.0f;
                this._armatureComponent.animation.FadeIn("jump_4", -1.0f, -1, 0, NORMAL_ANIMATION_GROUP).resetToPose = false;
            }
        }

        this._armatureComponent.transform.localPosition = position;
    }

    private void _UpdateAim()
    {
        var position = this._armatureComponent.transform.localPosition;
        this._faceDir = this._target.x > position.x ? 1 : -1;

        if (this._faceDir < 0.0f ? !this._armatureComponent.armature.flipX : this._armatureComponent.armature.flipX)
        {
            this._armatureComponent.armature.flipX = !this._armatureComponent.armature.flipX;

            if (this._moveDir != 0)
            {
                this._UpdateAnimation();
            }
        }

        var aimOffsetY = this._armatureComponent.armature.GetBone("chest").global.y * this._armatureComponent.transform.localScale.y;

        if (this._faceDir > 0)
        {
            this._aimRadian = Mathf.Atan2(-(this._target.y - position.y - aimOffsetY), this._target.x - position.x);
        }
        else
        {
            this._aimRadian = Mathf.PI - Mathf.Atan2(-(this._target.y - position.y - aimOffsetY), this._target.x - position.x);
            if (this._aimRadian > Mathf.PI)
            {
                this._aimRadian -= Mathf.PI * 2.0f;
            }
        }

        int aimDir = 0;
        if (this._aimRadian > 0.0f)
        {
            aimDir = -1;
        }
        else
        {
            aimDir = 1;
        }

        if (this._aimState == null || this._aimDir != aimDir)
        {
            this._aimDir = aimDir;

            // Animation mixing.
            if (this._aimDir >= 0)
            {
                this._aimState = this._armatureComponent.animation.FadeIn("aim_up", -1.0f, 1, 0, AIM_ANIMATION_GROUP, AnimationFadeOutMode.SameGroup);
            }
            else
            {
                this._aimState = this._armatureComponent.animation.FadeIn("aim_down", -1.0f, 1, 0, AIM_ANIMATION_GROUP, AnimationFadeOutMode.SameGroup);
            }

            this._aimState.resetToPose = false;
        }

        this._aimState.weight = Mathf.Abs(this._aimRadian / Mathf.PI * 2.0f);

        this._armatureComponent.armature.InvalidUpdate();
    }

    private void _UpdateAttack()
    {
        if (!this._isAttackingA || this._isAttackingB)
        {
            return;
        }

        this._isAttackingB = true;
        this._attackState = this._armature.animation.FadeIn("attack_01", -1.0f, -1, 0, ATTACK_ANIMATION_GROUP);
        this._attackState.resetToPose = false;
        this._attackState.autoFadeOutTime = this._attackState.fadeTotalTime;
    }

    private void _UpdateAnimation()
    {
        if (this._isJumpingA)
        {
            return;
        }

        if (this._isSquating)
        {
            this._speedX = 0;
            this._armature.animation.FadeIn("squat", -1.0f, -1, 0, NORMAL_ANIMATION_GROUP).resetToPose = false;

            this._walkState = null;
            return;
        }

        if (this._moveDir == 0)
        {
            this._speedX = 0;
            this._armature.animation.FadeIn("idle", -1.0f, -1, 0, NORMAL_ANIMATION_GROUP).resetToPose = false;
            this._walkState = null;
        }
        else
        {
            if (this._walkState == null)
            {
                this._walkState = this._armature.animation.FadeIn("walk", -1.0f, -1, 0, NORMAL_ANIMATION_GROUP);
                this._walkState.resetToPose = false;
            }

            if (this._moveDir * this._faceDir > 0)
            {
                this._walkState.timeScale = MAX_MOVE_SPEED_FRONT / NORMALIZE_MOVE_SPEED;
            }
            else
            {
                this._walkState.timeScale = -MAX_MOVE_SPEED_BACK / NORMALIZE_MOVE_SPEED;
            }

            if (this._moveDir * this._faceDir > 0)
            {
                this._speedX = MAX_MOVE_SPEED_FRONT * this._faceDir;
            }
            else
            {
                this._speedX = -MAX_MOVE_SPEED_BACK * this._faceDir;
            }
        }
    }

    private void _Fire(Vector3 firePoint)
    {
        firePoint.x += Random.Range(-0.01f, 0.01f);
        firePoint.y += Random.Range(-0.01f, 0.01f);
        firePoint.z = -0.2f;

        var bulletArmatureComonponnet = UnityFactory.factory.BuildArmatureComponent("bullet_01");
        var bulletComonponnet = bulletArmatureComonponnet.gameObject.AddComponent<Bullet>();
        var radian = _faceDir < 0 ? Mathf.PI - this._aimRadian : this._aimRadian;
        bulletArmatureComonponnet.animation.timeScale = _armatureComponent.animation.timeScale;
        bulletComonponnet.transform.position = firePoint;
        bulletComonponnet.Init("fire_effect_01", radian + Random.Range(-0.01f, 0.01f), 0.4f);
    }

    private void _OnAnimationEventHandler(string type, EventObject evt)
    {
        switch (evt.type)
        {
            case DragonBones.EventObject.FADE_IN_COMPLETE:
                {
                    if (evt.animationState.name == "jump_1")
                    {
                        this._isJumpingB = true;
                        this._speedY = -JUMP_SPEED;

                        if (this._moveDir != 0)
                        {
                            if (this._moveDir * this._faceDir > 0)
                            {
                                this._speedX = MAX_MOVE_SPEED_FRONT * this._faceDir;
                            }
                            else
                            {
                                this._speedX = -MAX_MOVE_SPEED_BACK * this._faceDir;
                            }
                        }

                        this._armature.animation.FadeIn("jump_2", -1.0f, -1, 0, NORMAL_ANIMATION_GROUP).resetToPose = false;
                    }
                }
                break;

            case DragonBones.EventObject.FADE_OUT_COMPLETE:
                {
                    if (evt.animationState.name == "attack_01")
                    {
                        this._isAttackingB = false;
                        this._attackState = null;
                    }
                }
                break;

            case DragonBones.EventObject.COMPLETE:
                {
                    if (evt.animationState.name == "jump_4")
                    {
                        this._isJumpingA = false;
                        this._isJumpingB = false;
                        this._UpdateAnimation();
                    }
                }
                break;
        }
    }

    private void _OnFrameEventHandler(string type, EventObject eventObject)
    {
        if (eventObject.name == "fire")
        {
            var transform = (eventObject.armature.display as GameObject).transform;
            var localPoint = new Vector3(eventObject.bone.global.x, -eventObject.bone.global.y, 0.0f);
            var globalPoint = transform.worldToLocalMatrix.inverse.MultiplyPoint(localPoint);

            this._Fire(globalPoint);
        }
    }
}

[RequireComponent(typeof(UnityArmatureComponent))]
public class Bullet : MonoBehaviour
{
    private UnityArmatureComponent _armatureComponent = null;
    private UnityArmatureComponent _effectComponent = null;
    private Vector3 _speed = new Vector3();

    void Awake()
    {
        this._armatureComponent = this.gameObject.GetComponent<UnityArmatureComponent>();
    }

    public void Init(string effectArmatureName, float radian, float speed)
    {
        this._speed.x = Mathf.Cos(radian) * speed * this._armatureComponent.animation.timeScale;
        this._speed.y = -Mathf.Sin(radian) * speed * this._armatureComponent.animation.timeScale;

        var rotation = this.transform.localEulerAngles;
        rotation.z = -radian * DragonBones.Transform.RAD_DEG;
        this.transform.localEulerAngles = rotation;
        this._armatureComponent.armature.animation.Play("idle");

        if (effectArmatureName != null)
        {
            this._effectComponent = UnityFactory.factory.BuildArmatureComponent(effectArmatureName);

            var effectRotation = this._effectComponent.transform.localEulerAngles;
            var effectScale = this._effectComponent.transform.localScale;
            effectRotation.z = -radian * DragonBones.Transform.RAD_DEG;
            if (Random.Range(0.0f, 1.0f) < 0.5)
            {
                effectRotation.x = 180.0f;
                effectRotation.z = -effectRotation.z;
            }

            effectScale.x = Random.Range(1.0f, 2.0f);
            effectScale.y = Random.Range(1.0f, 1.5f);

            this._effectComponent.animation.timeScale = this._armatureComponent.animation.timeScale;
            this._effectComponent.transform.localPosition = this.transform.localPosition;
            this._effectComponent.transform.localEulerAngles = effectRotation;
            this._effectComponent.transform.localScale = effectScale;
            this._effectComponent.animation.Play("idle");
        }
    }

    void Update()
    {
        if (this._armatureComponent.armature == null)
        {
            return;
        }

        this.transform.localPosition += this._speed;

        if (this.transform.localPosition.x < -7.0f || this.transform.localPosition.x > 7.0f ||
            this.transform.localPosition.y < -7.0f || this.transform.localPosition.y > 7.0f)
        {
            this._armatureComponent.armature.Dispose();

            if (this._effectComponent != null)
            {
                this._effectComponent.armature.Dispose();
            }
        }
    }
}
