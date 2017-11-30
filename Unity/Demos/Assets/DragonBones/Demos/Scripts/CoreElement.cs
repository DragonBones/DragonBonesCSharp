using System.Collections.Generic;
using UnityEngine;
using DragonBones;

namespace coreElement
{
    public class CoreElement : MonoBehaviour
    {
        private const string NORMAL_ANIMATION_GROUP = "normal";
        private const string AIM_ANIMATION_GROUP = "aim";
        private const string ATTACK_ANIMATION_GROUP = "attack";

        private const float G = -0.005f;
        private const float GROUND = 0.0f;
        private const float JUMP_SPEED = -0.2f;
        private const float NORMALIZE_MOVE_SPEED = 0.03f;
        private const float MAX_MOVE_SPEED_FRONT = NORMALIZE_MOVE_SPEED * 1.4f;
        private const float MAX_MOVE_SPEED_BACK = NORMALIZE_MOVE_SPEED * 1.0f;
        private static readonly string[] WEAPON_LEFT_LIST = { "weapon_1502b_l", "weapon_1005", "weapon_1005b", "weapon_1005c", "weapon_1005d", "weapon_1005e" };
        private static readonly string[] WEAPON_RIGHT_LIST = { "weapon_1502b_r", "weapon_1005", "weapon_1005b", "weapon_1005c", "weapon_1005d" };
        private static readonly string[] SKINS = { "mecha_1502b", "skin_a", "skin_b", "skin_c" };
        
        public KeyCode left = KeyCode.A;
        public KeyCode right = KeyCode.D;
        public KeyCode up = KeyCode.W;
        public KeyCode down = KeyCode.S;
        public KeyCode switchSkin = KeyCode.Space;
        public KeyCode switchLeftWeapon = KeyCode.Q;
        public KeyCode switchRightWeapon = KeyCode.E;
        
        private bool _isJumpingA = false;
        private bool _isJumpingB = false;
        private bool _isSquating = false;
        private bool _isAttackingA = false;
        private bool _isAttackingB = false;
        private int _skinIndex = 0;
        private int _weaponLeftIndex = 0;
        private int _weaponRightIndex = 0;
        private int _faceDir = 1;
        private int _moveDir = 0;
        private int _aimDir = 0;
        private float _aimRadian = 0.0f;

        private UnityArmatureComponent _armatureComponent = null;
        private Armature _weaponLeft = null;
        private Armature _weaponRight = null;
        private DragonBones.AnimationState _aimState = null;
        private DragonBones.AnimationState _walkState = null;
        private DragonBones.AnimationState _attackState = null;
        private Vector2 _speed = new Vector2();
        private Vector2 _target = new Vector2();

        //
        [SerializeField]
        private UnityDragonBonesData dragonBoneData;
        [SerializeField]
        private UnityDragonBonesData skinData;
        [SerializeField]
        private UnityDragonBonesData weaponData;

        void Start()
        {
            UnityFactory.factory.autoSearch = true;
            UnityFactory.factory.LoadData(skinData);
            UnityFactory.factory.LoadData(weaponData);
            UnityFactory.factory.LoadData(dragonBoneData);

            var armatureDisplay = new GameObject("mecha_1502b");
#if UNITY_5_6_OR_NEWER
            //armatureDisplay.AddComponent<UnityEngine.Rendering.SortingGroup>();
#endif
            _armatureComponent = UnityFactory.factory.BuildArmatureComponent("mecha_1502b", null, null, null, armatureDisplay);

#if UNITY_5_6_OR_NEWER
            //_armatureComponent.sortingMode = SortingMode.SortByOrder;
#endif
            _armatureComponent.AddEventListener(EventObject.FADE_IN_COMPLETE, _animationEventHandler);
            _armatureComponent.AddEventListener(EventObject.FADE_OUT_COMPLETE, _animationEventHandler);

            // Get weapon childArmature.
            _weaponLeft = _armatureComponent.armature.GetSlot("weapon_l").childArmature;
            _weaponRight = _armatureComponent.armature.GetSlot("weapon_r").childArmature;

            _weaponLeft.eventDispatcher.AddDBEventListener(EventObject.FRAME_EVENT, _frameEventHandler);
            _weaponRight.eventDispatcher.AddDBEventListener(EventObject.FRAME_EVENT, _frameEventHandler);

            _armatureComponent.animation.Reset();
            _armatureComponent.animation.Play("idle");
            _armatureComponent.armature.flipX = true;

            _updateAnimation();
        }

        void Update()
        {
            // Input 
            var isLeft = Input.GetKey(left);
            var isRight = Input.GetKey(right);

            if (isLeft == isRight)
            {
                _move(0);
            }
            else if (isLeft)
            {
                _move(-1);
            }
            else
            {
                _move(1);
            }

            if (Input.GetKeyDown(up))
            {
                _jump();
            }

            _squat(Input.GetKey(down));
            
            if (Input.GetKeyDown(switchSkin))
            {
                _switchSkin();
            }

            if (Input.GetKeyDown(switchLeftWeapon))
            {
                _switchWeaponLeft();
            }

            if (Input.GetKeyDown(switchRightWeapon))
            {
                _switchWeaponRight();
            }

            var target = Camera.main.ScreenToWorldPoint(Input.mousePosition + new Vector3(0.0f, 0.0f, Camera.main.farClipPlane));
            _aim(target.x, target.y);

            _attack(Input.GetMouseButton(0));

            //
            _updatePosition();
            _updateAim();
            _updateAttack();
        }

        private void _animationEventHandler(string type, EventObject eventObject)
        {
            switch (type)
            {
                case EventObject.FADE_IN_COMPLETE:
                    if (eventObject.animationState.name == "jump_1")
                    {
                        _isJumpingB = true;
                        _speed.y = -JUMP_SPEED;
                        _armatureComponent.animation.FadeIn("jump_2", -1, -1, 0, NORMAL_ANIMATION_GROUP).resetToPose = false;
                    }
                    else if (eventObject.animationState.name == "jump_4")
                    {
                        _updateAnimation();
                    }
                    break;

                case EventObject.FADE_OUT_COMPLETE:
                    if (eventObject.animationState.name == "attack_01")
                    {
                        _isAttackingB = false;
                        _attackState = null;
                    }
                    break;
            }
        }

        private void _frameEventHandler(string type, EventObject eventObject)
        {
            if (eventObject.name == "fire")
            {
                var transform = (eventObject.armature.display as GameObject).transform;
                var localPoint = new Vector3(eventObject.bone.global.x, -eventObject.bone.global.y, 0.0f);
                var globalPoint = transform.worldToLocalMatrix.inverse.MultiplyPoint(localPoint);
                this._fire(globalPoint);
            }
        }

        private void _move(int dir)
        {
            if (_moveDir == dir)
            {
                return;
            }

            _moveDir = dir;
            _updateAnimation();
        }

        private void _jump()
        {
            if (_isJumpingA)
            {
                return;
            }

            _isJumpingA = true;
            _armatureComponent.animation.FadeIn("jump_1", -1.0f, -1, 0, NORMAL_ANIMATION_GROUP).resetToPose = false ;
            _walkState = null;
        }

        private void _squat(bool isSquating)
        {
            if (_isSquating == isSquating)
            {
                return;
            }

            _isSquating = isSquating;
            _updateAnimation();
        }

        private void _switchWeaponLeft()
        {
            _weaponLeftIndex++;
            if (_weaponLeftIndex >= WEAPON_LEFT_LIST.Length)
            {
                _weaponLeftIndex = 0;
            }

            _weaponLeft.eventDispatcher.RemoveDBEventListener(DragonBones.EventObject.FRAME_EVENT, _frameEventHandler);

            var weaponName = WEAPON_LEFT_LIST[_weaponLeftIndex];
            _weaponLeft = UnityFactory.factory.BuildArmature(weaponName);
            _armatureComponent.armature.GetSlot("weapon_l").childArmature = _weaponLeft;
            _weaponLeft.eventDispatcher.AddDBEventListener(DragonBones.EventObject.FRAME_EVENT, _frameEventHandler);
        }

        private void _switchWeaponRight()
        {
            _weaponRightIndex++;
            if (_weaponRightIndex >= WEAPON_RIGHT_LIST.Length)
            {
                _weaponRightIndex = 0;
            }

            _weaponRight.eventDispatcher.RemoveDBEventListener(EventObject.FRAME_EVENT, _frameEventHandler);

            var weaponName = WEAPON_RIGHT_LIST[_weaponRightIndex];
            _weaponRight = UnityFactory.factory.BuildArmature(weaponName);
            _armatureComponent.armature.GetSlot("weapon_r").childArmature = _weaponRight;
            _weaponRight.eventDispatcher.AddDBEventListener(EventObject.FRAME_EVENT, _frameEventHandler);
        }

        public void _switchSkin()
        {
            this._skinIndex++;
            this._skinIndex %= SKINS.Length;
            var skinName = SKINS[this._skinIndex];
            var skinData = UnityFactory.factory.GetArmatureData(skinName).defaultSkin;
            List<string> exclude = new List<string>();
            exclude.Add("weapon_l");
            exclude.Add("weapon_r");
            UnityFactory.factory.ReplaceSkin(this._armatureComponent.armature, skinData, false, exclude);
        }

        private void _aim(float x, float y)
        {
            if (_aimDir == 0)
            {
                _aimDir = 10;
            }

            _target.x = x;
            _target.y = y;
        }

        private void _attack(bool isAttacking)
        {
            if (_isAttackingA == isAttacking)
            {
                return;
            }

            _isAttackingA = isAttacking;
        }

        private void _fire(Vector3 firePoint)
        {
            firePoint.x += Random.Range(-0.01f, 0.01f);
            firePoint.y += Random.Range(-0.01f, 0.01f);
            firePoint.z = -0.2f;

            var bulletArmatureComonponnet = UnityFactory.factory.BuildArmatureComponent("bullet_01");
            var bulletComonponnet = bulletArmatureComonponnet.gameObject.AddComponent<Bullet>();
            var radian = _faceDir < 0 ? Mathf.PI - _aimRadian : _aimRadian;
            bulletArmatureComonponnet.animation.timeScale = _armatureComponent.animation.timeScale;
            bulletComonponnet.transform.position = firePoint;
            bulletComonponnet.Init("fire_effect_01", radian + Random.Range(-0.01f, 0.01f), 0.4f);
        }

        private void _updateAnimation()
        {
            if (_isJumpingA || _isJumpingB)
            {
                return;
            }

            if (_isSquating)
            {
                _speed.x = 0.0f;
                _armatureComponent.animation.FadeIn("squat", -1.0f, -1, 0, NORMAL_ANIMATION_GROUP).resetToPose = false;
                _walkState = null;
                return;
            }

            if (_moveDir == 0.0f)
            {
                _speed.x = 0.0f;
                _armatureComponent.animation.FadeIn("idle", -1.0f, -1, 0, NORMAL_ANIMATION_GROUP).resetToPose = false;
                _walkState = null;
            }
            else
            {
                if (_walkState == null)
                {
                    _walkState = _armatureComponent.animation.FadeIn("walk", -1.0f, -1, 0, NORMAL_ANIMATION_GROUP);
                    this._walkState.resetToPose = false;
                }
                
                if (this._moveDir * this._faceDir > 0.0f)
                {
                    this._walkState.timeScale = MAX_MOVE_SPEED_FRONT / NORMALIZE_MOVE_SPEED;
                }
                else
                {
                    this._walkState.timeScale = -MAX_MOVE_SPEED_BACK / NORMALIZE_MOVE_SPEED;
                }

                if (this._moveDir * this._faceDir > 0.0f)
                {
                    _speed.x = MAX_MOVE_SPEED_FRONT * this._faceDir;
                }
                else
                {
                    _speed.x = -MAX_MOVE_SPEED_BACK * this._faceDir;
                }
            }
        }

        private void _updatePosition()
        {
            if (_speed.x == 0.0f && !_isJumpingB)
            {
                return;
            }

            var position = this._armatureComponent.transform.localPosition;
            //var position = this.transform.localPosition;
            
            if (_speed.x != 0.0f)
            {
                position.x += _speed.x * _armatureComponent.animation.timeScale;

                if (position.x < -4.0f)
                {
                    position.x = -4.0f;
                }
                else if (position.x > 4.0f)
                {
                    position.x = 4.0f;
                }
            }

            if (_isJumpingB)
            {
                if (_speed.y > -0.05f && _speed.y + G <= -0.05f)
                {
                    _armatureComponent.animation.FadeIn("jump_3", -1.0f, -1, 0, NORMAL_ANIMATION_GROUP).resetToPose = false;
                }

                _speed.y += G * _armatureComponent.animation.timeScale;
                position.y += _speed.y * _armatureComponent.animation.timeScale;

                if (position.y < GROUND)
                {
                    position.y = GROUND;
                    _isJumpingA = false;
                    _isJumpingB = false;
                    _speed.x = 0.0f;
                    _speed.y = 0.0f;
                    _armatureComponent.animation.FadeIn("jump_4", -1.0f, -1, 0, NORMAL_ANIMATION_GROUP).resetToPose = false;

                    if (_isSquating || _moveDir != 0.0f)
                    {
                        _updateAnimation();
                    }
                }
            }

            this._armatureComponent.transform.localPosition = position;
            //this.transform.localPosition = position;
        }

        private void _updateAim()
        {
            if (_aimDir == 0)
            {
                return;
            }

            var position = this._armatureComponent.transform.localPosition;
            //var position = this.transform.localPosition;
            _faceDir = _target.x > position.x ? 1 : -1;

            if (_faceDir < 0.0f ? !_armatureComponent.armature.flipX : _armatureComponent.armature.flipX)
            {
                _armatureComponent.armature.flipX = !_armatureComponent.armature.flipX;

                if (_moveDir != 0)
                {
                    _updateAnimation();
                }
            }

            var aimOffsetY = _armatureComponent.armature.GetBone("chest").global.y * this.transform.localScale.y;

            if (_faceDir > 0)
            {
                _aimRadian = Mathf.Atan2(-(_target.y - position.y - aimOffsetY), _target.x - position.x);
            }
            else
            {
                _aimRadian = Mathf.PI - Mathf.Atan2(-(_target.y - position.y - aimOffsetY), _target.x - position.x);
                if (_aimRadian > Mathf.PI)
                {
                    _aimRadian -= Mathf.PI * 2.0f;
                }
            }
            
            int aimDir = 0;
            if (_aimRadian > 0.0f)
            {
                aimDir = -1;
            }
            else
            {
                aimDir = 1;
            }

            if (_aimDir != aimDir)
            {
                _aimDir = aimDir;

                // Animation mixing.
                if (_aimDir >= 0)
                {
                    _aimState = _armatureComponent.animation.FadeIn(
                        "aim_up", -1.0f, 1,
                        0, AIM_ANIMATION_GROUP, AnimationFadeOutMode.SameGroup
                    );
                }
                else
                {
                    _aimState = _armatureComponent.animation.FadeIn(
                        "aim_down", -1.0f, 1,
                        0, AIM_ANIMATION_GROUP, AnimationFadeOutMode.SameGroup
                    );
                }

                _aimState.resetToPose = false;
                // Add bone mask.
                //_aimState.addBoneMask("pelvis");
            }

            _aimState.weight = Mathf.Abs(_aimRadian / Mathf.PI * 2.0f);

            //_armature.invalidUpdate("pelvis"); // Only update bone mask.
            _armatureComponent.armature.InvalidUpdate();
        }

        private void _updateAttack()
        {
            if (!_isAttackingA || _isAttackingB)
            {
                return;
            }

            _isAttackingB = true;

            // Animation mixing.
            _attackState = _armatureComponent.animation.FadeIn(
                "attack_01", -1.0f, -1,
                0, ATTACK_ANIMATION_GROUP, AnimationFadeOutMode.SameGroup
            );

            _attackState.resetToPose = false;
            _attackState.autoFadeOutTime = _attackState.fadeTotalTime;
            _attackState.AddBoneMask("pelvis");
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
            _armatureComponent = this.gameObject.GetComponent<UnityArmatureComponent>();
        }

        void Update()
        {
            if (_armatureComponent.armature == null)
            {
                return;
            }

            this.transform.localPosition += _speed;

            if (this.transform.localPosition.x < -7.0f || this.transform.localPosition.x > 7.0f ||
                this.transform.localPosition.y < -7.0f || this.transform.localPosition.y > 7.0f )
            {
                _armatureComponent.armature.Dispose();

                if (_effectComponent != null)
                {
                    _effectComponent.armature.Dispose();
                }
            }
        }

        public void Init(string effectArmatureName, float radian, float speed)
        {
            _speed.x = Mathf.Cos(radian) * speed * _armatureComponent.animation.timeScale;
            _speed.y = -Mathf.Sin(radian) * speed * _armatureComponent.animation.timeScale;

            var rotation = this.transform.localEulerAngles;
            rotation.z = -radian * DragonBones.Transform.RAD_DEG;
            this.transform.localEulerAngles = rotation;
            _armatureComponent.armature.animation.Play("idle");

            if (effectArmatureName != null)
            {
                _effectComponent = UnityFactory.factory.BuildArmatureComponent(effectArmatureName);

                var effectRotation = _effectComponent.transform.localEulerAngles;
                var effectScale = _effectComponent.transform.localScale;
                effectRotation.z = -radian * DragonBones.Transform.RAD_DEG;
                if (Random.Range(0.0f, 1.0f) < 0.5)
                {
                    effectRotation.x = 180.0f;
                    effectRotation.z = -effectRotation.z;
                }

                effectScale.x = Random.Range(1.0f, 2.0f);
                effectScale.y = Random.Range(1.0f, 1.5f);

                _effectComponent.animation.timeScale = _armatureComponent.animation.timeScale;
                _effectComponent.transform.localPosition = this.transform.localPosition;
                _effectComponent.transform.localEulerAngles = effectRotation;
                _effectComponent.transform.localScale = effectScale;
                _effectComponent.animation.Play("idle");
            }
        }
    }
}