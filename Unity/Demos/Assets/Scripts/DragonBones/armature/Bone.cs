using System;
using System.Collections.Generic;

namespace DragonBones
{
    /**
     * 骨骼，一个骨架中可以包含多个骨骼，骨骼以树状结构组成骨架。
     * 骨骼在骨骼动画体系中是最重要的逻辑单元之一，负责动画中的平移旋转缩放的实现。
     * @see dragonBones.BoneData
     * @see dragonBones.Armature
     * @see dragonBones.Slot
     * @version DragonBones 3.0
     * @language zh_CN
     */
    public class Bone : TransformObject
    {
        /**
         * @private
         */
        internal OffsetMode offsetMode;
        /**
         * @internal
         * @private
         */
        internal readonly Transform animationPose = new Transform();
        /**
         * @internal
         * @private
         */
        internal readonly List<Constraint> constraints = new List<Constraint>();
        /**
         * @readonly
         */
        public BoneData boneData;
        /**
         * @internal
         * @private
         */
        internal bool _transformDirty;
        /**
         * @internal
         * @private
         */
        internal bool _childrenTransformDirty;
        /**
         * @internal
         * @private
         */
        internal bool _blendDirty;
        private bool _localDirty;
        private bool _visible;
        private int _cachedFrameIndex;
        /**
         * @internal
         * @private
         */
        internal int _blendLayer;
        /**
         * @internal
         * @private
         */
        internal float _blendLeftWeight;
        /**
         * @internal
         * @private
         */
        internal float _blendLayerWeight;
        private readonly List<Bone> _bones = new List<Bone>();
        private readonly List<Slot> _slots = new List<Slot>();
        /**
         * @internal
         * @private
         */
        internal List<int> _cachedFrameIndices = new List<int>();
        /**
         * @private
         */
        protected override void _OnClear()
        {
            base._OnClear();

            foreach (var constraint in this.constraints)
            {
                constraint.ReturnToPool();
            }

            this.offsetMode = OffsetMode.Additive;
            this.animationPose.Identity();
            this.constraints.Clear();
            this.boneData = null; //

            this._transformDirty = false;
            this._childrenTransformDirty = false;
            this._blendDirty = false;
            this._localDirty = true;
            this._visible = true;
            this._cachedFrameIndex = -1;
            this._blendLayer = 0;
            this._blendLeftWeight = 1.0f;
            this._blendLayerWeight = 0.0f;
            this._bones.Clear();
            this._slots.Clear();
            this._cachedFrameIndices = null;
        }
        /**
         * @private
         */
        private void _UpdateGlobalTransformMatrix(bool isCache)
        {            
            var flipX = this._armature.flipX;
            var flipY = this._armature.flipY == DragonBones.yDown;
            var global = this.global;
            var globalTransformMatrix = this.globalTransformMatrix;
            var inherit = this._parent != null;
            var dR = 0.0f;

            if (this.offsetMode == OffsetMode.Additive)
            {
                //global.CopyFrom(this.origin).Add(this.offset).Add(this.animationPose);
                global.x = this.origin.x + this.offset.x + this.animationPose.x;
                global.y = this.origin.y + this.offset.y + this.animationPose.y;
                global.skew = this.origin.skew + this.offset.skew + this.animationPose.skew;
                global.rotation = this.origin.rotation + this.offset.rotation + this.animationPose.rotation;
                global.scaleX = this.origin.scaleX * this.offset.scaleX * this.animationPose.scaleX;
                global.scaleY = this.origin.scaleY * this.offset.scaleY * this.animationPose.scaleY;
            }
            else if (this.offsetMode == OffsetMode.None)
            {
                global.CopyFrom(this.origin).Add(this.animationPose);
            }
            else
            {
                inherit = false;
                global.CopyFrom(this.offset);
            }

            if (inherit)
            {
                var parentMatrix = this._parent.globalTransformMatrix;
                if (this.boneData.inheritScale)
                {
                    if (!this.boneData.inheritRotation)
                    {
                        this._parent.UpdateGlobalTransform();

                        dR = this._parent.global.rotation; //
                        global.rotation -= dR;
                    }

                    global.ToMatrix(globalTransformMatrix);
                    globalTransformMatrix.Concat(parentMatrix);

                    if (this.boneData.inheritTranslation)
                    {
                        global.x = globalTransformMatrix.tx;
                        global.y = globalTransformMatrix.ty;
                    }
                    else
                    {
                        globalTransformMatrix.tx = global.x;
                        globalTransformMatrix.ty = global.y;
                    }

                    if (isCache)
                    {
                        global.FromMatrix(globalTransformMatrix);
                    }
                    else
                    {
                        this._globalDirty = true;
                    }
                }
                else
                {
                    if (this.boneData.inheritTranslation)
                    {
                        var x = global.x;
                        var y = global.y;
                        global.x = parentMatrix.a * x + parentMatrix.c * y + parentMatrix.tx;
                        global.y = parentMatrix.d * y + parentMatrix.b * x + parentMatrix.ty;
                    }
                    else
                    {
                        if (flipX)
                        {
                            global.x = -global.x;
                        }

                        if (flipY)
                        {
                            global.y = -global.y;
                        }
                    }

                    if (this.boneData.inheritRotation)
                    {
                        this._parent.UpdateGlobalTransform();
                        dR = this._parent.global.rotation;

                        if (this._parent.global.scaleX < 0.0f)
                        {
                            dR += (float)Math.PI;
                        }

                        if (parentMatrix.a * parentMatrix.d - parentMatrix.b * parentMatrix.c < 0.0f)
                        {
                            dR -= global.rotation * 2.0f;

                            if (flipX != flipY || this.boneData.inheritReflection)
                            {
                                global.skew += (float)Math.PI;
                            }
                        }

                        global.rotation += dR;
                    }
                    else if (flipX || flipY)
                    {
                        if (flipX && flipY)
                        {
                            dR = (float)Math.PI;
                        }
                        else
                        {
                            dR = -global.rotation * 2.0f;
                            if (flipX)
                            {
                                dR += (float)Math.PI;
                            }

                            global.skew += (float)Math.PI;
                        }

                        global.rotation += dR;
                    }

                    global.ToMatrix(globalTransformMatrix);
                }
            }
            else
            {                
                if (flipX || flipY)
                {
                    if (flipX)
                    {
                        global.x = -global.x;
                    }

                    if (flipY)
                    {
                        global.y = -global.y;
                    }

                    if (flipX && flipY)
                    {
                        dR = (float)Math.PI;
                    }
                    else
                    {
                        dR = -global.rotation * 2.0f;
                        if (flipX)
                        {
                            dR += (float)Math.PI;
                        }

                        global.skew += (float)Math.PI;
                    }

                    global.rotation += dR;
                }

                global.ToMatrix(globalTransformMatrix);
            }
        }

        /**
         * @internal
         * @private
         */
        internal override void _SetArmature(Armature value = null)
        {
            if (this._armature == value)
            {
                return;
            }

            List<Slot> oldSlots = null;
            List<Bone> oldBones = null;

            if (this._armature != null)
            {
                oldSlots = this.GetSlots();
                oldBones = this.GetBones();
                this._armature._RemoveBoneFromBoneList(this);
            }

            this._armature = value; //

            if (this._armature != null)
            {
                this._armature._AddBoneToBoneList(this);
            }

            if (oldSlots != null)
            {
                foreach (var slot in oldSlots)
                {
                    if (slot.parent == this)
                    {
                        slot._SetArmature(this._armature);
                    }
                }
            }

            if (oldBones != null)
            {
                foreach (var bone in oldBones)
                {
                    if (bone.parent == this)
                    {
                        bone._SetArmature(this._armature);
                    }
                }
            }
        }
        /**
         * @internal
         * @private
         */
        internal void Init(BoneData boneData)
        {
            if (this.boneData != null)
            {
                return;
            }

            this.boneData = boneData;
            this.name = this.boneData.name;
            this.origin = this.boneData.transform;
        }
        /**
         * @internal
         * @private
         */
        internal void Update(int cacheFrameIndex)
        {
            this._blendDirty = false;

            if (cacheFrameIndex >= 0 && this._cachedFrameIndices != null)
            {
                var cachedFrameIndex = this._cachedFrameIndices[cacheFrameIndex];

                if (cachedFrameIndex >= 0 && this._cachedFrameIndex == cachedFrameIndex)
                {
                    // Same cache.
                    this._transformDirty = false;
                }
                else if (cachedFrameIndex >= 0)
                {
                    // Has been Cached.
                    this._transformDirty = true;
                    this._cachedFrameIndex = cachedFrameIndex;
                }
                else
                {
                    if (this.constraints.Count > 0)
                    {
                        // Update constraints.
                        foreach (var constraint in this.constraints)
                        {
                            constraint.Update();
                        }
                    }

                    if (this._transformDirty || (this._parent != null && this._parent._childrenTransformDirty))
                    {
                        // Dirty.
                        this._transformDirty = true;
                        this._cachedFrameIndex = -1;
                    }
                    else if (this._cachedFrameIndex >= 0)
                    {
                        // Same cache, but not set index yet.
                        this._transformDirty = false;
                        this._cachedFrameIndices[cacheFrameIndex] = this._cachedFrameIndex;
                    }
                    else
                    {
                        // Dirty.
                        this._transformDirty = true;
                        this._cachedFrameIndex = -1;
                    }
                }
            }
            else
            {
                if (this.constraints.Count > 0)
                { 
                    // Update constraints.
                    foreach (var constraint in this.constraints)
                    {
                        constraint.Update();
                    }
                }

                if (this._transformDirty || (this._parent != null && this._parent._childrenTransformDirty))
                { 
                    // Dirty.
                    cacheFrameIndex = -1;
                    this._transformDirty = true;
                    this._cachedFrameIndex = -1;
                }
            }

            if (this._transformDirty)
            {
                this._transformDirty = false;
                this._childrenTransformDirty = true;

                if (this._cachedFrameIndex < 0)
                {
                    var isCache = cacheFrameIndex >= 0;
                    if (this._localDirty)
                    {
                        this._UpdateGlobalTransformMatrix(isCache);
                    }

                    if (isCache && this._cachedFrameIndices != null)
                    {
                        this._cachedFrameIndex = this._cachedFrameIndices[cacheFrameIndex] = this._armature.armatureData.SetCacheFrame(this.globalTransformMatrix, this.global);
                    }
                }
                else
                {
                    this._armature.armatureData.GetCacheFrame(this.globalTransformMatrix, this.global, this._cachedFrameIndex);
                }
            }
            else if (this._childrenTransformDirty)
            {
                this._childrenTransformDirty = false;
            }

            this._localDirty = true;
        }
        /**
         * @internal
         * @private
         */
        internal void UpdateByConstraint()
        {
            if (this._localDirty)
            {
                this._localDirty = false;
                if (this._transformDirty || (this._parent != null && this._parent._childrenTransformDirty))
                {
                    this._UpdateGlobalTransformMatrix(true);
                }

                this._transformDirty = true;
            }
        }
        /**
         * @internal
         * @private
         */
        internal void AddConstraint(Constraint constraint)
        {
            if (!this.constraints.Contains(constraint))
            {
                this.constraints.Add(constraint);
            }
        }
        /**
         * 下一帧更新变换。 (当骨骼没有动画状态或动画状态播放完成时，骨骼将不在更新)
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public void InvalidUpdate()
        {
            this._transformDirty = true;
        }
        /**
         * 是否包含骨骼或插槽。
         * @returns
         * @see dragonBones.TransformObject
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public bool Contains(TransformObject child)
        {
            if (child == this)
            {
                return false;
            }

            TransformObject ancestor = child;
            while (ancestor != this && ancestor != null)
            {
                ancestor = ancestor.parent;
            }

            return ancestor == this;
        }
        /**
         * 所有的子骨骼。
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public List<Bone> GetBones()
        {
            this._bones.Clear();

            foreach (var bone in this._armature.GetBones())
            {
                if (bone.parent == this)
                {
                    this._bones.Add(bone);
                }
            }

            return this._bones;
        }
        /**
         * 所有的插槽。
         * @see dragonBones.Slot
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public List<Slot> GetSlots()
        {
            this._slots.Clear();

            foreach (var slot in this._armature.GetSlots())
            {
                if (slot.parent == this)
                {
                    this._slots.Add(slot);
                }
            }

            return this._slots;
        }
        /**
         * 控制此骨骼所有插槽的可见。
         * @default true
         * @see dragonBones.Slot
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public bool visible
        {
            get { return this._visible; }
            set
            {
                if (this._visible == value)
                {
                    return;
                }

                this._visible = value;

                foreach (var slot in this._armature.GetSlots())
                {
                    if (slot._parent == this)
                    {
                        slot._UpdateVisible();
                    }
                }
            }
        }
    }
}
