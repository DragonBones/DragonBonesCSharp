using System;
using System.Collections.Generic;

namespace DragonBones
{
    /**
     * @private
     */
    internal enum BoneTransformDirty
    {
        None = 0,
        Self = 1,
        All = 2
    }
    /**
     * @language zh_CN
     * 骨骼，一个骨架中可以包含多个骨骼，骨骼以树状结构组成骨架。
     * 骨骼在骨骼动画体系中是最重要的逻辑单元之一，负责动画中的平移旋转缩放的实现。
     * @see DragonBones.BoneData
     * @see DragonBones.Armature
     * @see DragonBones.Slot
     * @version DragonBones 3.0
     */
    public class Bone : TransformObject
    {
        /**
         * @language zh_CN
         * 是否继承父骨骼的平移。
         * @version DragonBones 3.0
         */
        public bool inheritTranslation;
        /**
         * @language zh_CN
         * 是否继承父骨骼的旋转。
         * @version DragonBones 3.0
         */
        public bool inheritRotation;
        /**
         * @language zh_CN
         * 是否继承父骨骼的缩放。
         * @version DragonBones 4.5
         */
        public bool inheritScale;
        /**
         * @private
         */
        public bool ikBendPositive;
        /**
         * @language zh_CN
         * 骨骼长度。
         * @version DragonBones 4.5
         */
        public float length;
        /**
         * @private
         */
        public float ikWeight;
        /**
         * @private
         */
        internal BoneTransformDirty _transformDirty;
        private bool _visible;
        private int _cachedFrameIndex;
        private uint _ikChain;
        private int _ikChainIndex;
        /**
         * @private
         */
        internal int _updateState;
        /**
         * @private
         */
        internal int _blendLayer;
        /**
         * @private
         */
        internal float _blendLeftWeight;
        /**
         * @private
         */
        internal float _blendTotalWeight;
        /**
         * @private
         */
        internal readonly Transform _animationPose = new Transform();
        private readonly List<Bone> _bones = new List<Bone>();
        private readonly List<Slot> _slots = new List<Slot>();
        private BoneData _boneData;
        private Bone _ik;
        /**
         * @private
         */
        internal List<int> _cachedFrameIndices;
        /**
         * @private
         */
        public Bone()
        {
        }
        /**
         * @private
         */
        protected override void _onClear()
        {
            base._onClear();

            inheritTranslation = false;
            inheritRotation = false;
            inheritScale = false;
            ikBendPositive = false;
            length = 0.0f;
            ikWeight = 0.0f;

            _transformDirty = BoneTransformDirty.None;
            _visible = true;
            _cachedFrameIndex = -1;
            _ikChain = 0;
            _ikChainIndex = 0;
            _updateState = -1;
            _blendLayer = 0;
            _blendLeftWeight = 0.0f;
            _blendTotalWeight = 0.0f;
            _animationPose.Identity();
            _bones.Clear();
            _slots.Clear();
            _boneData = null;
            _ik = null;
            _cachedFrameIndices = null;
        }
        /**
         * @private
         */
        private void _updateGlobalTransformMatrix()
        {
            global.x = origin.x + offset.x + _animationPose.x;
            global.y = origin.y + offset.y + _animationPose.y;
            global.skewX = origin.skewX + offset.skewX + _animationPose.skewX;
            global.skewY = origin.skewY + offset.skewY + _animationPose.skewY;
            global.scaleX = origin.scaleX * offset.scaleX * _animationPose.scaleX;
            global.scaleY = origin.scaleY * offset.scaleY * _animationPose.scaleY;

            if (_parent != null)
            {
                var parentRotation = _parent.global.skewY; // Only inherit skew y.
                var parentMatrix = _parent.globalTransformMatrix;

                if (inheritScale)
                {
                    if (!inheritRotation)
                    {
                        global.skewX -= parentRotation;
                        global.skewY -= parentRotation;
                    }

                    global.ToMatrix(globalTransformMatrix);
                    globalTransformMatrix.Concat(parentMatrix);

                    if (!inheritTranslation)
                    {
                        globalTransformMatrix.tx = global.x;
                        globalTransformMatrix.ty = global.y;
                    }

                    global.FromMatrix(globalTransformMatrix);
                }
                else
                {
                    if (inheritTranslation)
                    {
                        var x = global.x;
                        var y = global.y;
                        global.x = parentMatrix.a * x + parentMatrix.c * y + parentMatrix.tx;
                        global.y = parentMatrix.d * y + parentMatrix.b * x + parentMatrix.ty;
                    }

                    if (inheritRotation)
                    {
                        global.skewX += parentRotation;
                        global.skewY += parentRotation;
                    }

                    global.ToMatrix(globalTransformMatrix);
                }
            }
            else
            {
                global.ToMatrix(globalTransformMatrix);
            }

            if (_ik != null && _ikChainIndex == _ikChain && ikWeight > 0.0f)
            {
                if (inheritTranslation && _ikChain > 0 && _parent != null)
                {
                    _computeIKB();
                }
                else
                {
                    _computeIKA();
                }
            }
        }
        /**
         * @private
         */
        private void _computeIKA()
        {
            var ikGlobal = _ik.global;
            var x = globalTransformMatrix.a * length;
            var y = globalTransformMatrix.b * length;

            var ikRadian =
                (float)(
                    Math.Atan2(ikGlobal.y - global.y, ikGlobal.x - global.x) +
                    offset.skewY -
                    global.skewY * 2.0f +
                    Math.Atan2(y, x)
                ) * ikWeight; // Support offset.

            global.skewX += ikRadian;
            global.skewY += ikRadian;
            global.ToMatrix(globalTransformMatrix);
        }
        /**
         * @private
         */
        private void _computeIKB()
        {
            var parentGlobal = _parent.global;
            var ikGlobal = _ik.global;

            var x = globalTransformMatrix.a * length;
            var y = globalTransformMatrix.b * length;

            var lLL = x * x + y * y;
            var lL = (float)Math.Sqrt(lLL);

            var dX = global.x - parentGlobal.x;
            var dY = global.y - parentGlobal.y;
            var lPP = dX * dX + dY * dY;
            var lP = (float)Math.Sqrt(lPP);

            dX = ikGlobal.x - parentGlobal.x;
            dY = ikGlobal.y - parentGlobal.y;
            var lTT = dX * dX + dY * dY;
            var lT = (float)Math.Sqrt(lTT);

            var ikRadianA = 0.0f;
            if (lL + lP <= lT || lT + lL <= lP || lT + lP <= lL)
            {
                ikRadianA = (float)Math.Atan2(ikGlobal.y - parentGlobal.y, ikGlobal.x - parentGlobal.x) + _parent.offset.skewY; // Support offset.
                if (lL + lP <= lT)
                {
                }
                else if (lP < lL)
                {
                    ikRadianA += DragonBones.PI;
                }
            }
            else
            {
                var h = (lPP - lLL + lTT) / (2.0f * lTT);
                var r = (float)Math.Sqrt(lPP - h * h * lTT) / lT;
                var hX = parentGlobal.x + (dX * h);
                var hY = parentGlobal.y + (dY * h);
                var rX = -dY * r;
                var rY = dX * r;

                if (ikBendPositive)
                {
                    global.x = hX - rX;
                    global.y = hY - rY;
                }
                else
                {
                    global.x = hX + rX;
                    global.y = hY + rY;
                }

                ikRadianA = (float)Math.Atan2(global.y - parentGlobal.y, global.x - parentGlobal.x) + _parent.offset.skewY; // Support offset.
            }

            ikRadianA = (ikRadianA - parentGlobal.skewY) * ikWeight;

            parentGlobal.skewX += ikRadianA;
            parentGlobal.skewY += ikRadianA;

            parentGlobal.ToMatrix(_parent.globalTransformMatrix);
            _parent._transformDirty = BoneTransformDirty.Self;

            global.x = parentGlobal.x + (float)Math.Cos(parentGlobal.skewY) * lP;
            global.y = parentGlobal.y + (float)Math.Sin(parentGlobal.skewY) * lP;

            var ikRadianB =
                (float)(
                    Math.Atan2(ikGlobal.y - global.y, ikGlobal.x - global.x) + offset.skewY -
                    global.skewY * 2 + Math.Atan2(y, x)
                ) * ikWeight; // Support offset.

            global.skewX += ikRadianB;
            global.skewY += ikRadianB;

            global.ToMatrix(globalTransformMatrix);
        }
        /**
         * @private
         */
        internal void _init(BoneData boneData)
        {
            if (_boneData != null) {
                return;
            }

            _boneData = boneData;

            inheritTranslation = _boneData.inheritTranslation;
            inheritRotation = _boneData.inheritRotation;
            inheritScale = _boneData.inheritScale;
            length = _boneData.length;
            name = _boneData.name;
            origin =_boneData.transform;
        }
        /**
         * @private
         */
        internal override void _setArmature(Armature value)
        {
            if (_armature == value)
            {
                return;
            }

            _ik = null;

            List<Slot> oldSlots = null;
            List<Bone> oldBones = null;

            if (_armature != null)
            {
                oldSlots = GetSlots();
                oldBones = GetBones();
                _armature._removeBoneFromBoneList(this);
            }

            _armature = value;

            if (_armature != null)
            {
                _armature._addBoneToBoneList(this);
            }

            if (oldSlots != null)
            {
                foreach (var slot in oldSlots)
                {
                    if (slot.parent == this)
                    {
                        slot._setArmature(_armature);
                    }
                }
            }

            if (oldBones != null)
            {
                foreach (var bone in oldBones)
                {
                    if (bone.parent == this)
                    {
                        bone._setArmature(_armature);
                    }
                }
            }
        }
        /**
         * @private
         */
        internal void _setIK(Bone value, uint chain, int chainIndex)
        {
            if (value != null)
            {
                if (chain == chainIndex)
                {
                    var chainEnd = _parent;
                    if (chain > 0 && chainEnd != null)
                    {
                        chain = 1;
                    }
                    else
                    {
                        chain = 0;
                        chainIndex = 0;
                        chainEnd = this;
                    }

                    if (chainEnd == value || chainEnd.Contains(value))
                    {
                        value = null;
                        chain = 0;
                        chainIndex = 0;
                    }
                    else
                    {
                        var ancestor = value;
                        while (ancestor.ik != null && ancestor.ikChain > 0)
                        {
                            if (chainEnd.Contains(ancestor.ik))
                            {
                                value = null;
                                chain = 0;
                                chainIndex = 0;
                                break;
                            }

                            ancestor = ancestor.parent;
                        }
                    }
                }
            }
            else
            {
                chain = 0;
                chainIndex = 0;
            }

            _ik = value;
            _ikChain = chain;
            _ikChainIndex = chainIndex;

            if (_armature != null)
            {
                _armature._bonesDirty = true;
            }
        }
        /**
         * @private
         */
        internal void _update(int cacheFrameIndex)
        {
            _updateState = -1;

            if (cacheFrameIndex >= 0 && _cachedFrameIndices != null)
            {
                var cachedFrameIndex = _cachedFrameIndices[cacheFrameIndex];
                if (cachedFrameIndex >= 0 && _cachedFrameIndex == cachedFrameIndex) // Same cache.
                {
                    _transformDirty = BoneTransformDirty.None;
                }
                else if (cachedFrameIndex >= 0.0f) // Has been Cached.
                {
                    _transformDirty = BoneTransformDirty.All;
                    _cachedFrameIndex = cachedFrameIndex;
                }
                else if (
                    _transformDirty == BoneTransformDirty.All ||
                    (_parent != null && _parent._transformDirty != BoneTransformDirty.None) ||
                    (_ik != null && ikWeight > 0.0f && _ik._transformDirty != BoneTransformDirty.None)
                ) // Dirty.
                {
                    _transformDirty = BoneTransformDirty.All;
                    _cachedFrameIndex = -1;
                }
                else if (_cachedFrameIndex >= 0) // Same cache but not cached yet.
                {
                    _transformDirty = BoneTransformDirty.None;
                    _cachedFrameIndices[cacheFrameIndex] = _cachedFrameIndex;
                }
                else // Dirty.
                {
                    _transformDirty = BoneTransformDirty.All;
                    _cachedFrameIndex = -1;
                }
            }
            else if (
                _transformDirty == BoneTransformDirty.All ||
                (_parent != null && _parent._transformDirty != BoneTransformDirty.None) ||
                (_ik != null && ikWeight > 0.0f && _ik._transformDirty != BoneTransformDirty.None)
            )
            {
                cacheFrameIndex = -1;
                _transformDirty = BoneTransformDirty.All; // For update children and ik children.
                _cachedFrameIndex = -1;
            }

            if (_transformDirty != BoneTransformDirty.None)
            {
                if (_transformDirty == BoneTransformDirty.All)
                {
                    _transformDirty = BoneTransformDirty.Self;

                    if (_cachedFrameIndex < 0)
                    {
                        _updateGlobalTransformMatrix();

                        if (cacheFrameIndex >= 0)
                        {
                            _cachedFrameIndex = _cachedFrameIndices[cacheFrameIndex] = _armature._armatureData.SetCacheFrame(globalTransformMatrix, global);
                        }
                    }
                    else
                    {
                        _armature._armatureData.GetCacheFrame(globalTransformMatrix, global, _cachedFrameIndex);
                    }

                    _updateState = 0;
                }
                else
                {
                    _transformDirty = BoneTransformDirty.None;
                }
            }
        }
        /**
         * @language zh_CN
         * 下一帧更新变换。 (当骨骼没有动画状态或动画状态播放完成时，骨骼将不在更新)
         * @version DragonBones 3.0
         */
        public void InvalidUpdate()
        {
            _transformDirty = BoneTransformDirty.All;
        }

        /**
         * @language zh_CN
         * 是否包含骨骼或插槽。
         * @returns
         * @see DragonBones.TransformObject
         * @version DragonBones 3.0
         */
        public bool Contains(TransformObject child)
        {
            if (child != null)
            {
                if (child == this)
                {
                    return false;
                }

                var ancestor = child;
                while (ancestor != this && ancestor != null)
                {
                    ancestor = ancestor.parent;
                }

                return ancestor == this;
            }

            return false;
        }
        /**
         * @language zh_CN
         * 所有的子骨骼。
         * @version DragonBones 3.0
         */
        public List<Bone> GetBones()
        {
            _bones.Clear();

            var bones = _armature.GetBones();
            foreach (var bone in bones)
            {
                if (bone.parent == this)
                {
                    _bones.Add(bone);
                }
            }

            return _bones;
        }
        /**
         * @language zh_CN
         * 所有的插槽。
         * @see DragonBones.Slot
         * @version DragonBones 3.0
         */
        public List<Slot> GetSlots()
        {
            _slots.Clear();

            var slots = _armature.GetSlots();
            foreach (var slot in slots)
            {
                if (slot.parent == this)
                {
                    _slots.Add(slot);
                }
            }

            return _slots;
        }
        /**
         * @deprecated
         */
        public BoneData boneData
        {
            get { return _boneData; }
        }
        /**
         * @language zh_CN
         * 控制此骨骼所有插槽的可见。
         * @default true
         * @see DragonBones.Slot
         * @version DragonBones 3.0
         */
        public bool visible
        {
            get { return _visible; }

            set
            {
                if (_visible == value)
                {
                    return;
                }

                _visible = value;
                var slots = _armature.GetSlots();
                foreach (var slot in slots)
                {
                    if (slot._parent == this)
                    {
                        slot._updateVisible();
                    }
                }
            }
        }

        /**
         * @deprecated
         */
        public uint ikChain
        {
            get { return _ikChain; }
        }
        /**
         * @deprecated
         */
        public int ikChainIndex
        {
            get { return _ikChainIndex; }
        }
        /**
         * @deprecated
         */
        public Bone ik
        {
            get { return _ik; }
        }
    }
}