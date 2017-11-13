using System;
using System.Collections.Generic;

namespace DragonBones
{
    /**
     * 骨架数据。
     * @version DragonBones 3.0
     * @language zh_CN
     */
    public class ArmatureData : BaseObject
    {
        /**
         * @private
         */
        public ArmatureType type;
        /**
         * 动画帧率。
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public uint frameRate;
        /**
         * @private
         */
        public uint cacheFrameRate;
        /**
         * @private
         */
        public float scale;
        /**
         * 数据名称。
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public string name;
        /**
         * @private
         */
        public readonly Rectangle aabb = new Rectangle();
        /**
         * 所有动画数据名称。
         * @see #armatures
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public readonly List<string> animationNames = new List<string>();
        /**
         * @private
         */
        public readonly List<BoneData> sortedBones = new List<BoneData>();
        /**
         * @private
         */
        public readonly List<SlotData> sortedSlots = new List<SlotData>();
        /**
         * @private
         */
        public readonly List<ActionData> defaultActions = new List<ActionData>();
        /**
         * @private
         */
        public readonly List<ActionData> actions = new List<ActionData>();
        /**
         * 所有骨骼数据。
         * @see dragonBones.BoneData
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public readonly Dictionary<string, BoneData> bones = new Dictionary<string, BoneData>();
        /**
         * 所有插槽数据。
         * @see dragonBones.SlotData
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public readonly Dictionary<string, SlotData> slots = new Dictionary<string, SlotData>();

        public readonly Dictionary<string, ConstraintData> constraints = new Dictionary<string, ConstraintData>();
        /**
         * 所有皮肤数据。
         * @see dragonBones.SkinData
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public readonly Dictionary<string, SkinData> skins = new Dictionary<string, SkinData>();
        /**
         * 所有动画数据。
         * @see dragonBones.AnimationData
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public readonly Dictionary<string, AnimationData> animations = new Dictionary<string, AnimationData>();

        /**
         * 获取默认皮肤数据。
         * @see dragonBones.SkinData
         * @version DragonBones 4.5
         * @language zh_CN
         */
        public SkinData defaultSkin = null;
        /**
         * 获取默认动画数据。
         * @see dragonBones.AnimationData
         * @version DragonBones 4.5
         * @language zh_CN
         */
        public AnimationData defaultAnimation = null;
        /**
         * @private
         */
        public CanvasData canvas = null; // Initial value.
        /**
         * @private
         */
        public UserData userData = null; // Initial value.
        /**
         * 所属的龙骨数据。
         * @see dragonBones.DragonBonesData
         * @version DragonBones 4.5
         * @language zh_CN
         */
        public DragonBonesData parent;


        protected override void _OnClear()
        {
            foreach (var action in this.defaultActions)
            {
                action.ReturnToPool();
            }

            foreach (var action in this.actions) {
                action.ReturnToPool();
            }

            foreach (var k in this.bones.Keys)
            {
                this.bones[k].ReturnToPool();
            }

            foreach (var k in this.slots.Keys)
            {
                this.slots[k].ReturnToPool();
            }

            foreach (var k in this.constraints.Keys)
            {
                this.constraints[k].ReturnToPool();
            }

            foreach (var k in this.skins.Keys)
            {
                this.skins[k].ReturnToPool();
            }

            foreach (var k in this.animations.Keys)
            {
                this.animations[k].ReturnToPool();
            }

            if (this.canvas != null)
            {
                this.canvas.ReturnToPool();
            }

            if (this.userData != null)
            {
                this.userData.ReturnToPool();
            }

            this.type = ArmatureType.Armature;
            this.frameRate = 0;
            this.cacheFrameRate = 0;
            this.scale = 1.0f;
            this.name = "";
            this.aabb.Clear();
            this.animationNames.Clear();
            this.sortedBones.Clear();
            this.sortedSlots.Clear();
            this.defaultActions.Clear();
            this.actions.Clear();
            this.bones.Clear();
            this.slots.Clear();
            this.constraints.Clear();
            this.skins.Clear();
            this.animations.Clear();
            this.defaultSkin = null;
            this.defaultAnimation = null;
            this.canvas = null;
            this.userData = null;
            this.parent = null; //
        }

        /**
         * @private
         */
        public void SortBones()
        {
            var total = this.sortedBones.Count;
            if (total <= 0)
            {
                return;
            }

            var sortHelper = this.sortedBones.ToArray();
            var index = 0;
            var count = 0;
            this.sortedBones.Clear();
            while (count < total)
            {
                var bone = sortHelper[index++];
                if (index >= total)
                {
                    index = 0;
                }

                if (this.sortedBones.Contains(bone))
                {
                    continue;
                }

                //if (bone.constraints.Count > 0)
                //{
                //    var flag = false;
                //    foreach (var constraint in bone.constraints)
                //    {
                //        if (!this.sortedBones.Contains(constraint.target))
                //        {
                //            flag = true;
                //        }
                //    }

                //    if (flag)
                //    {
                //        continue;
                //    }
                //}

                var flag = false;
                foreach (var constraint in this.constraints.Values)
                {
                    // Wait constraint.
                    if (constraint.bone == bone && !this.sortedBones.Contains(constraint.target))
                    {
                        flag = true;
                        break;
                    }
                }

                if (flag)
                {
                    continue;
                }


                if (bone.parent != null && !this.sortedBones.Contains(bone.parent))
                {
                    // Wait parent.
                    continue;
                }

                this.sortedBones.Add(bone);
                count++;
            }
        }

        /**
         * @private
         */
        public void CacheFrames(uint frameRate)
        {
            if (this.cacheFrameRate > 0)
            { 
                // TODO clear cache.
                return;
            }

            this.cacheFrameRate = frameRate;
            foreach (var k in this.animations.Keys)
            {
                this.animations[k].CacheFrames(this.cacheFrameRate);
            }
        }

        /**
         * @private
         */
        public int SetCacheFrame(Matrix globalTransformMatrix, Transform transform)
        {
            var dataArray = this.parent.cachedFrames;
            var arrayOffset = dataArray.Count;

            dataArray.ResizeList(arrayOffset + 10, 0.0f);

            dataArray[arrayOffset] = globalTransformMatrix.a;
            dataArray[arrayOffset + 1] = globalTransformMatrix.b;
            dataArray[arrayOffset + 2] = globalTransformMatrix.c;
            dataArray[arrayOffset + 3] = globalTransformMatrix.d;
            dataArray[arrayOffset + 4] = globalTransformMatrix.tx;
            dataArray[arrayOffset + 5] = globalTransformMatrix.ty;
            dataArray[arrayOffset + 6] = transform.rotation;
            dataArray[arrayOffset + 7] = transform.skew;
            dataArray[arrayOffset + 8] = transform.scaleX;
            dataArray[arrayOffset + 9] = transform.scaleY;

            return arrayOffset;
        }

        /**
        * @private
        */
        public void GetCacheFrame(Matrix globalTransformMatrix, Transform transform, int arrayOffset)
        {
            var dataArray = this.parent.cachedFrames;
            globalTransformMatrix.a = dataArray[arrayOffset];
            globalTransformMatrix.b = dataArray[arrayOffset + 1];
            globalTransformMatrix.c = dataArray[arrayOffset + 2];
            globalTransformMatrix.d = dataArray[arrayOffset + 3];
            globalTransformMatrix.tx = dataArray[arrayOffset + 4];
            globalTransformMatrix.ty = dataArray[arrayOffset + 5];
            transform.rotation = dataArray[arrayOffset + 6];
            transform.skew = dataArray[arrayOffset + 7];
            transform.scaleX = dataArray[arrayOffset + 8];
            transform.scaleY = dataArray[arrayOffset + 9];
            transform.x = globalTransformMatrix.tx;
            transform.y = globalTransformMatrix.ty;
        }

        /**
        * @private
        */
        public void AddBone(BoneData value)
        {
            if (value != null && !string.IsNullOrEmpty(value.name))
            {
                if (this.bones.ContainsKey(value.name))
                {
                    Helper.Assert(false, "Same bone: " + value.name);
                    this.bones[value.name].ReturnToPool();
                }

                this.bones[value.name] = value;
                this.sortedBones.Add(value);
            }
        }
        /**
         * @private
         */
        public void AddSlot(SlotData value)
        {
            if (value != null && !string.IsNullOrEmpty(value.name))
            {
                if (this.slots.ContainsKey(value.name))
                {
                    Helper.Assert(false, "Same slot: " + value.name);
                    this.slots[value.name].ReturnToPool();
                }

                this.slots[value.name] = value;
                this.sortedSlots.Add(value);
            }
        }
        public void AddConstraint(ConstraintData value)
        {
            if (value != null && !string.IsNullOrEmpty(value.name))
            {
                if (this.constraints.ContainsKey(value.name))
                {
                    Helper.Assert(false, "Same constraint: " + value.name);
                    this.slots[value.name].ReturnToPool();
                }

                this.constraints[value.name] = value;
            }
        }
        /**
        * @private
        */
        public void AddSkin(SkinData value)
        {
            if (value != null && !string.IsNullOrEmpty(value.name))
            {
                if (this.skins.ContainsKey(value.name))
                {
                    Helper.Assert(false, "Same slot: " + value.name);
                    this.skins[value.name].ReturnToPool();
                }

                value.parent = this;
                this.skins[value.name] = value;
                if (this.defaultSkin == null)
                {
                    this.defaultSkin = value;
                }
            }
        }
        /**
         * @private
         */
        public void AddAnimation(AnimationData value)
        {
            if (value != null && !string.IsNullOrEmpty(value.name))
            {
                if (this.animations.ContainsKey(value.name))
                {
                    Helper.Assert(false, "Same animation: " + value.name);
                    this.animations[value.name].ReturnToPool();
                }

                value.parent = this;
                this.animations[value.name] = value;
                this.animationNames.Add(value.name);
                if (this.defaultAnimation == null)
                {
                    this.defaultAnimation = value;
                }
            }
        }
        /**
         * @private
         */
        internal void AddAction(ActionData value, bool isDefault)
        {
            if (isDefault)
            {
                this.defaultActions.Add(value);
            }
            else
            {
                this.actions.Add(value);
            }
        }

        /**
         * 获取骨骼数据。
         * @param name 数据名称。
         * @version DragonBones 3.0
         * @see dragonBones.BoneData
         * @language zh_CN
         */
        public BoneData GetBone(string name)
        {
            return (!string.IsNullOrEmpty(name) && bones.ContainsKey(name)) ? bones[name] : null;
        }
        /**
         * 获取插槽数据。
         * @param name 数据名称。
         * @version DragonBones 3.0
         * @see dragonBones.SlotData
         * @language zh_CN
         */
        public SlotData GetSlot(string name)
        {
            return (!string.IsNullOrEmpty(name) && slots.ContainsKey(name)) ? slots[name] : null;
        }
        /**
         * 获取皮肤数据。
         * @param name 数据名称。
         * @version DragonBones 3.0
         * @see dragonBones.SkinData
         * @language zh_CN
         */
        public SkinData GetSkin(string name)
        {
            return !string.IsNullOrEmpty(name) ? (skins.ContainsKey(name) ? skins[name] : null) : defaultSkin;
        }
        /**
         * 获取动画数据。
         * @param name 数据名称。
         * @version DragonBones 3.0
         * @see dragonBones.AnimationData
         * @language zh_CN
         */
        public AnimationData GetAnimation(string name)
        {
            return !string.IsNullOrEmpty(name) ? (animations.ContainsKey(name) ? animations[name] : null) : defaultAnimation;
        }
    }

    /**
     * 骨骼数据。
     * @version DragonBones 3.0
     * @language zh_CN
     */
    public class BoneData : BaseObject
    {
        /**
         * @private
         */
        public bool inheritTranslation;
        /**
         * @private
         */
        public bool inheritRotation;
        /**
         * @private
         */
        public bool inheritScale;
        /**
         * @private
         */
        public bool inheritReflection;
        /**
         * @private
         */
        public float length;
        /**
         * 数据名称。
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public string name;
        /**
         * @private
         */
        public readonly Transform transform = new Transform();
        /**
         * @private
         */
        public UserData userData = null; // Initial value.
        /**
         * 所属的父骨骼数据。
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public BoneData parent = null;

        /**
         * @private
         */
        protected override void _OnClear()
        {
            if (this.userData != null)
            {
                this.userData.ReturnToPool();
            }

            this.inheritTranslation = false;
            this.inheritRotation = false;
            this.inheritScale = false;
            this.inheritReflection = false;
            this.length = 0.0f;
            this.name = "";
            this.transform.Identity();
            this.userData = null;
            this.parent = null;
        }
    }

    /**
     * 插槽数据。
     * @see dragonBones.Slot
     * @version DragonBones 3.0
     * @language zh_CN
     */
    public class SlotData : BaseObject
    {
        /**
         * @private
         */
        public static readonly ColorTransform DEFAULT_COLOR = new ColorTransform();

        /**
         * @private
         */
        public static ColorTransform CreateColor()
        {
            return new ColorTransform();
        }

        /**
         * @private
         */
        public BlendMode blendMode;
        /**
         * @private
         */
        public int displayIndex;
        /**
         * @private
         */
        public int zOrder;
        /**
         * 数据名称。
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public string name;
        /**
         * @private
         */
        public ColorTransform color = null; // Initial value.
        /**
         * @private
         */
        public UserData userData = null; // Initial value.
        /**
         * 所属的父骨骼数据。
         * @see dragonBones.BoneData
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public BoneData parent;
        /**
         * @private
         */
        protected override void _OnClear()
        {
            if (this.userData != null)
            {
                this.userData.ReturnToPool();
            }

            this.blendMode = BlendMode.Normal;
            this.displayIndex = 0;
            this.zOrder = 0;
            this.name = "";
            this.color = null; //
            this.userData = null;
            this.parent = null; //
        }
    }
}