using System;
using System.Collections.Generic;

namespace DragonBones
{
    /**
     * 骨架，是骨骼动画系统的核心，由显示容器、骨骼、插槽、动画、事件系统构成。
     * @see dragonBones.ArmatureData
     * @see dragonBones.Bone
     * @see dragonBones.Slot
     * @see dragonBones.Animation
     * @version DragonBones 3.0
     * @language zh_CN
     */
    public class Armature : BaseObject, IAnimateble
    {
        private static int _OnSortSlots(Slot a, Slot b)
        {
            return a._zOrder > b._zOrder ? 1 : -1;
        }

        /**
         * 是否继承父骨架的动画状态。
         * @default true
         * @version DragonBones 4.5
         * @language zh_CN
         */
        public bool inheritAnimation;
        /**
         * 获取骨架数据。
         * @see dragonBones.ArmatureData
         * @version DragonBones 4.5
         * @readonly
         * @language zh_CN
         */
        public ArmatureData armatureData;
        /**
         * 用于存储临时数据。
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public object userData;

        private bool _lockUpdate;
        private bool _bonesDirty;
        private bool _slotsDirty;
        private bool _zOrderDirty;
        private bool _flipX;
        private bool _flipY;

        /**
         * @internal
         * @private
         */
        internal int _cacheFrameIndex;
        private readonly List<Bone> _bones = new List<Bone>();
        private readonly List<Slot> _slots = new List<Slot>();
        private readonly List<ActionData> _actions = new List<ActionData>();
        private Animation _animation = null; // Initial value.
        private IArmatureProxy _proxy = null; // Initial value.
        private object _display;
        /**
         * @private
         */
        public TextureAtlasData _replaceTextureAtlasData = null; // Initial value.
        private object _replacedTexture;
        /**
         * @internal
         * @private
         */
        internal DragonBones _dragonBones;
        private WorldClock _clock = null; // Initial value.

        /**
         * @internal
         * @private
         */
        internal Slot _parent;
        /**
         * @private
         */
        protected override void _OnClear()
        {
            if (this._clock != null)
            { 
                // Remove clock first.
                this._clock.Remove(this);
            }

            foreach (var bone in this._bones)
            {
                bone.ReturnToPool();
            }

            foreach (var slot in this._slots)
            {
                slot.ReturnToPool();
            }

            if (this._animation != null)
            {
                this._animation.ReturnToPool();
            }

            if (this._proxy != null)
            {
                this._proxy.DBClear();
            }

            if (this._replaceTextureAtlasData != null)
            {
                this._replaceTextureAtlasData.ReturnToPool();
            }

            this.inheritAnimation = true;
            this.armatureData = null; //
            this.userData = null;

            this._lockUpdate = false;
            this._bonesDirty = false;
            this._slotsDirty = false;
            this._zOrderDirty = false;
            this._flipX = false;
            this._flipY = false;
            this._cacheFrameIndex = -1;
            this._bones.Clear();
            this._slots.Clear();
            this._actions.Clear();
            this._animation = null; //
            this._proxy = null; //
            this._display = null;
            this._replaceTextureAtlasData = null;
            this._replacedTexture = null;
            this._dragonBones = null; //
            this._clock = null;
            this._parent = null;
        }

        private void _SortBones()
        {
            var total = this._bones.Count;
            if (total <= 0)
            {
                return;
            }

            var sortHelper = this._bones.ToArray();
            var index = 0;
            var count = 0;

            this._bones.Clear();
            while (count < total)
            {
                var bone = sortHelper[index++];
                if (index >= total)
                {
                    index = 0;
                }

                if (this._bones.Contains(bone))
                {
                    continue;
                }

                if (bone.constraints.Count > 0)
                { 
                    // Wait constraint.
                    var flag = false;
                    foreach (var constraint in bone.constraints)
                    {
                        if (!this._bones.Contains(constraint.target))
                        {
                            flag = true;
                            break;
                        }
                    }

                    if (flag)
                    {
                        continue;
                    }
                }

                if (bone.parent != null && !this._bones.Contains(bone.parent))
                { 
                    // Wait parent.
                    continue;
                }

                this._bones.Add(bone);
                count++;
            }
        }

        private void _SortSlots()
        {
            this._slots.Sort(Armature._OnSortSlots);
        }

        /**
         * @internal
         * @private
         */
        internal void _SortZOrder(short[] slotIndices, int offset)
        {
            var slotDatas = this.armatureData.sortedSlots;
            var isOriginal = slotIndices == null;

            if (this._zOrderDirty || !isOriginal)
            {
                for (int i = 0, l = slotDatas.Count; i < l; ++i)
                {
                    var slotIndex = isOriginal ? i : slotIndices[offset + i];
                    if (slotIndex < 0 || slotIndex >= l)
                    {
                        continue;
                    }

                    var slotData = slotDatas[slotIndex];
                    var slot = this.GetSlot(slotData.name);
                    if (slot != null)
                    {
                        slot._SetZorder(i);
                    }
                }

                this._slotsDirty = true;
                this._zOrderDirty = !isOriginal;
            }
        }
        /**
         * @internal
         * @private
         */
        internal void _AddBoneToBoneList(Bone value)
        {
            if (!this._bones.Contains(value))
            {
                this._bonesDirty = true;
                this._bones.Add(value);
                this._animation._timelineDirty = true;
            }
        }
        /**
         * @internal
         * @private
         */
        internal void _RemoveBoneFromBoneList(Bone value)
        {
            if (this._bones.Contains(value))
            {
                this._bones.Remove(value);
                this._animation._timelineDirty = true;
            }
        }
        /**
         * @internal
         * @private
         */
        internal void _AddSlotToSlotList(Slot value)
        {
            if (!this._slots.Contains(value))
            {
                this._slotsDirty = true;
                this._slots.Add(value);
                this._animation._timelineDirty = true;
            }
        }
        /**
         * @internal
         * @private
         */
        internal void _RemoveSlotFromSlotList(Slot value)
        {
            if (this._slots.Contains(value))
            {
                this._slots.Remove(value);
                this._animation._timelineDirty = true;
            }
        }
        /**
         * @internal
         * @private
         */
        internal void _BufferAction(ActionData action, bool append)
        {
            if (!this._actions.Contains(action))
            {
                if (append)
                {
                    this._actions.Add(action);
                }
                else
                {
                    this._actions.Insert(0, action);
                }
            }
        }
        /**
         * 释放骨架。 (回收到对象池)
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public void Dispose()
        {
            if (this.armatureData != null)
            {
                this._lockUpdate = true;
                this._dragonBones.BufferObject(this);
            }
        }
        /**
         * @private
         */
        public void Init(ArmatureData armatureData, IArmatureProxy proxy, object display, DragonBones dragonBones)
        {
            if (this.armatureData != null)
            {
                return;
            }

            this.armatureData = armatureData;
            this._animation = BaseObject.BorrowObject<Animation>();
            this._proxy = proxy;
            this._display = display;
            this._dragonBones = dragonBones;

            this._proxy.DBInit(this);
            this._animation.Init(this);
            this._animation.animations = this.armatureData.animations;
        }
        /**
         * 更新骨架和动画。
         * @param passedTime 两帧之间的时间间隔。 (以秒为单位)
         * @see dragonBones.IAnimateble
         * @see dragonBones.WorldClock
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public void AdvanceTime(float passedTime)
        {
            if (this._lockUpdate)
            {
                return;
            }

            if (this.armatureData == null)
            {
                Helper.Assert(false, "The armature has been disposed.");
                return;
            }
            else if (this.armatureData.parent == null)
            {
                Helper.Assert(false, "The armature data has been disposed.\nPlease make sure dispose armature before call factory.clear().");
                return;
            }

            var prevCacheFrameIndex = this._cacheFrameIndex;

            // Update nimation.
            this._animation.AdvanceTime(passedTime);

            // Sort bones and slots.
            if (this._bonesDirty)
            {
                this._bonesDirty = false;
                this._SortBones();
            }

            if (this._slotsDirty)
            {
                this._slotsDirty = false;
                this._SortSlots();
            }

            // Update bones and slots.
            if (this._cacheFrameIndex < 0 || this._cacheFrameIndex != prevCacheFrameIndex)
            {
                int i = 0, l = 0;
                for (i = 0, l = this._bones.Count; i < l; ++i)
                {
                    this._bones[i].Update(this._cacheFrameIndex);
                }

                for (i = 0, l = this._slots.Count; i < l; ++i)
                {
                    this._slots[i].Update(this._cacheFrameIndex);
                }
            }

            if (this._actions.Count > 0)
            {
                this._lockUpdate = true;
                foreach (var action in this._actions)
                {
                    if (action.type == ActionType.Play)
                    {
                        this._animation.FadeIn(action.name);
                    }
                }

                this._actions.Clear();
                this._lockUpdate = false;
            }

            this._proxy.DBUpdate();
        }
        /**
         * 更新骨骼和插槽。 (当骨骼没有动画状态或动画状态播放完成时，骨骼将不在更新)
         * @param boneName 指定的骨骼名称，如果未设置，将更新所有骨骼。
         * @param updateSlotDisplay 是否更新插槽的显示对象。
         * @see dragonBones.Bone
         * @see dragonBones.Slot
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public void InvalidUpdate(string boneName = null, bool updateSlotDisplay = false)
        {

            if (!string.IsNullOrEmpty(boneName))
            {
                Bone bone = this.GetBone(boneName);
                if (bone != null)
                {
                    bone.InvalidUpdate();

                    if (updateSlotDisplay)
                    {
                        foreach (var slot in this._slots)
                        {
                            if (slot.parent == bone)
                            {
                                slot.InvalidUpdate();
                            }
                        }
                    }
                }
            }
            else
            {
                foreach (var bone in this._bones)
                {
                    bone.InvalidUpdate();
                }

                if (updateSlotDisplay)
                {
                    foreach (var slot in this._slots)
                    {
                        slot.InvalidUpdate();
                    }
                }
            }
        }
        /**
         * 判断点是否在所有插槽的自定义包围盒内。
         * @param x 点的水平坐标。（骨架内坐标系）
         * @param y 点的垂直坐标。（骨架内坐标系）
         * @version DragonBones 5.0
         * @language zh_CN
         */
        public Slot ContainsPoint(float x, float y)
        {
            foreach (var slot in this._slots)
            {
                if (slot.ContainsPoint(x, y))
                {
                    return slot;
                }
            }

            return null;
        }
        /**
         * 判断线段是否与骨架的所有插槽的自定义包围盒相交。
         * @param xA 线段起点的水平坐标。（骨架内坐标系）
         * @param yA 线段起点的垂直坐标。（骨架内坐标系）
         * @param xB 线段终点的水平坐标。（骨架内坐标系）
         * @param yB 线段终点的垂直坐标。（骨架内坐标系）
         * @param intersectionPointA 线段从起点到终点与包围盒相交的第一个交点。（骨架内坐标系）
         * @param intersectionPointB 线段从终点到起点与包围盒相交的第一个交点。（骨架内坐标系）
         * @param normalRadians 碰撞点处包围盒切线的法线弧度。 [x: 第一个碰撞点处切线的法线弧度, y: 第二个碰撞点处切线的法线弧度]
         * @returns 线段从起点到终点相交的第一个自定义包围盒的插槽。
         * @version DragonBones 5.0
         * @language zh_CN
         */
         public Slot IntersectsSegment(float xA, float yA, float xB, float yB,
                                        Point intersectionPointA = null,
                                        Point intersectionPointB = null,
                                        Point normalRadians = null)
         {
            var isV = xA == xB;
            var dMin = 0.0f;
            var dMax = 0.0f;
            var intXA = 0.0f;
            var intYA = 0.0f;
            var intXB = 0.0f;
            var intYB = 0.0f;
            var intAN = 0.0f;
            var intBN = 0.0f;
            Slot intSlotA = null;
            Slot intSlotB = null;

            foreach (var slot in this._slots)
            {
                var intersectionCount = slot.IntersectsSegment(xA, yA, xB, yB, intersectionPointA, intersectionPointB, normalRadians);
                if (intersectionCount > 0)
                {
                    if (intersectionPointA != null || intersectionPointB != null)
                    {
                        if (intersectionPointA != null)
                        {
                            var d = isV ? intersectionPointA.y - yA : intersectionPointA.x - xA;
                            if (d < 0.0f)
                            {
                                d = -d;
                            }

                            if (intSlotA == null || d < dMin)
                            {
                                dMin = d;
                                intXA = intersectionPointA.x;
                                intYA = intersectionPointA.y;
                                intSlotA = slot;

                                if (normalRadians != null)
                                {
                                    intAN = normalRadians.x;
                                }
                            }
                        }

                        if (intersectionPointB != null)
                        {
                            var d = intersectionPointB.x - xA;
                            if (d < 0.0f)
                            {
                                d = -d;
                            }

                            if (intSlotB == null || d > dMax)
                            {
                                dMax = d;
                                intXB = intersectionPointB.x;
                                intYB = intersectionPointB.y;
                                intSlotB = slot;

                                if (normalRadians != null)
                                {
                                    intBN = normalRadians.y;
                                }
                            }
                        }
                    }
                    else
                    {
                        intSlotA = slot;
                        break;
                    }
                }
            }

            if (intSlotA != null && intersectionPointA != null)
            {
                intersectionPointA.x = intXA;
                intersectionPointA.y = intYA;

                if (normalRadians != null)
                {
                    normalRadians.x = intAN;
                }
            }

            if (intSlotB != null && intersectionPointB != null)
            {
                intersectionPointB.x = intXB;
                intersectionPointB.y = intYB;

                if (normalRadians != null)
                {
                    normalRadians.y = intBN;
                }
            }

            return intSlotA;
        }
        /**
         * 获取指定名称的骨骼。
         * @param name 骨骼的名称。
         * @returns 骨骼。
         * @see dragonBones.Bone
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public Bone GetBone(string name)
        {
            foreach (var bone in this._bones)
            {
                if (bone.name == name)
                {
                    return bone;
                }
            }

            return null;
        }
        /**
         * 通过显示对象获取骨骼。
         * @param display 显示对象。
         * @returns 包含这个显示对象的骨骼。
         * @see dragonBones.Bone
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public Bone GetBoneByDisplay(object display)
        {
            var slot = this.GetSlotByDisplay(display);

            return slot != null ? slot.parent : null;
        }
        /**
         * 获取插槽。
         * @param name 插槽的名称。
         * @returns 插槽。
         * @see dragonBones.Slot
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public Slot GetSlot(string name)
        {
            foreach (var slot in this._slots)
            {
                if (slot.name == name)
                {
                    return slot;
                }
            }

            return null;
        }
        /**
         * 通过显示对象获取插槽。
         * @param display 显示对象。
         * @returns 包含这个显示对象的插槽。
         * @see dragonBones.Slot
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public Slot GetSlotByDisplay(object display)
        {
            if (display != null)
            {
                foreach (var slot in this._slots)
                {
                    if (slot.display == display)
                    {
                        return slot;
                    }
                }
            }

            return null;
        }
        /**
         * @deprecated
         */
        public void AddBone(Bone value, string parentName = null)
        {
            Helper.Assert(value != null, "add bone is null");

            value._SetArmature(this);
            value._SetParent(parentName != null ? this.GetBone(parentName) : null);
        }
        /**
         * @deprecated
         */
        public void RemoveBone(Bone value)
        {
            Helper.Assert(value != null && value.armature == this, "bone is null");

            value._SetParent(null);
            value._SetArmature(null);
        }
        /**
         * @deprecated
         */
        public void AddSlot(Slot value, string parentName)
        {
            var bone = this.GetBone(parentName);

            Helper.Assert(value != null && bone != null, "slot value is null");

            value._SetArmature(this);
            value._SetParent(bone);
        }
        /**
         * @deprecated
         */
        public void RemoveSlot(Slot value)
        {
            Helper.Assert(value != null && value.armature == this, "remove slot is null");

            value._SetParent(null);
            value._SetArmature(null);
        }
        /**
         * 获取所有骨骼。
         * @see dragonBones.Bone
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public List<Bone> GetBones()
        {
            return this._bones;
        }
        /**
         * 获取所有插槽。
         * @see dragonBones.Slot
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public List<Slot> GetSlots()
        {
            return this._slots;
        }

        public bool flipX
        {
            get { return this._flipX; }
            set
            {
                if (this._flipX == value)
                {
                    return;
                }

                this._flipX = value;
                this.InvalidUpdate();
            }
        }
        public bool flipY
        {
            get { return this._flipY; }
            set
            {
                if (this._flipY == value)
                {
                    return;
                }

                this._flipY = value;
                this.InvalidUpdate();
            }
        }
        /**
         * 动画缓存帧率，当设置的值大于 0 的时，将会开启动画缓存。
         * 通过将动画数据缓存在内存中来提高运行性能，会有一定的内存开销。
         * 帧率不宜设置的过高，通常跟动画的帧率相当且低于程序运行的帧率。
         * 开启动画缓存后，某些功能将会失效，比如 Bone 和 Slot 的 offset 属性等。
         * @see dragonBones.DragonBonesData#frameRate
         * @see dragonBones.ArmatureData#frameRate
         * @version DragonBones 4.5
         * @language zh_CN
         */
        public uint cacheFrameRate
        {
            get { return this.armatureData.cacheFrameRate; }
            set
            {
                if (this.armatureData.cacheFrameRate != value)
                {
                    this.armatureData.CacheFrames(value);

                    // Set child armature frameRate.
                    foreach (var slot in this._slots)
                    {
                        var childArmature = slot.childArmature;
                        if (childArmature != null)
                        {
                            childArmature.cacheFrameRate = value;
                        }
                    }
                }
            }
        }
        /**
         * 骨架名称。
         * @see dragonBones.ArmatureData#name
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public string name
        {
            get { return this.armatureData.name; }
        }
        /**
         * 获得动画控制器。
         * @see dragonBones.Animation
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public Animation animation
        {
            get { return this._animation; }
        }
        /**
         * @pivate
         */
        public IArmatureProxy proxy
        {
            get { return this._proxy; }
        }
        /**
         * 获取显示容器，插槽的显示对象都会以此显示容器为父级，根据渲染平台的不同，类型会不同，通常是 DisplayObjectContainer 类型。
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public object display
        {
            get { return this._display; }
        }
        /**
         * @language zh_CN
         * 替换骨架的主贴图，根据渲染引擎的不同，提供不同的贴图数据。
         * @version DragonBones 4.5
         */
        public object replacedTexture
        {
            get { return this._replacedTexture; }
            set
            {
                if (this._replacedTexture == value)
                {
                    return;
                }

                if (this._replaceTextureAtlasData != null)
                {
                    this._replaceTextureAtlasData.ReturnToPool();
                    this._replaceTextureAtlasData = null;
                }

                this._replacedTexture = value;

                foreach (var slot in this._slots)
                {
                    slot.InvalidUpdate();
                    slot.Update(-1);
                }
            }
        }
        /**
         * @inheritDoc
         */
        public WorldClock clock
        {
            get { return this._clock; }
            set
            {
                if (this._clock == value)
                {
                    return;
                }

                if (this._clock != null)
                {
                    this._clock.Remove(this);
                }

                this._clock = value;

                if (this._clock != null)
                {
                    this._clock.Add(this);
                }

                // Update childArmature clock.
                foreach (var slot in this._slots)
                {
                    var childArmature = slot.childArmature;
                    if (childArmature != null)
                    {
                        childArmature.clock = this._clock;
                    }
                }
            }
        }
        /**
         * 获取父插槽。 (当此骨架是某个骨架的子骨架时，可以通过此属性向上查找从属关系)
         * @see dragonBones.Slot
         * @version DragonBones 4.5
         * @language zh_CN
         */
        public Slot parent
        {
            get { return this._parent; }
        }
    }
}
