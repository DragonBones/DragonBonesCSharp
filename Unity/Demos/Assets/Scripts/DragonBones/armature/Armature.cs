using System.Collections.Generic;

namespace DragonBones
{
    /**
     * @language zh_CN
     * 骨架，是骨骼动画系统的核心，由显示容器、骨骼、插槽、动画、事件系统构成。
     * @see dragonBones.ArmatureData
     * @see dragonBones.Bone
     * @see dragonBones.Slot
     * @see dragonBones.Animation
     * @see dragonBones.IArmatureDisplay
     * @version DragonBones 3.0
     */
    public class Armature : BaseObject, IAnimateble
    {
        private static int _onSortSlots(Slot a, Slot b)
        {
            return a._zOrder > b._zOrder ? 1 : -1;
        }
        /**
         * @language zh_CN
         * 可以用于存储临时数据。
         * @version DragonBones 3.0
         */
        public object userData;
        
        private bool _delayDispose;
        private bool _lockDispose;
        /**
         * @private
         */
        internal bool _bonesDirty;
        private bool _slotsDirty;
        /**
         * @private
         */
        internal bool _flipX;
        /**
         * @private
         */
        internal bool _flipY;
        /**
         * @private
         */
        internal int _cacheFrameIndex;
        private readonly List<Bone> _bones = new List<Bone>();
        private readonly List<Slot> _slots = new List<Slot>();
        private readonly List<ActionData> _actions = new List<ActionData>();
        private readonly List<EventObject> _events = new List<EventObject>();
        /**
         * @private
         */
        internal ArmatureData _armatureData;
        /**
         * @private
         */
        internal SkinData _skinData;
        private Animation _animation;
        private IArmatureProxy _proxy;
        private object _display;
        private IEventDispatcher<EventObject> _eventManager;
        /**
         * @private
         */
        internal Slot _parent;
        private WorldClock _clock;
        /**
         * @private
         */
        internal TextureAtlasData _replaceTextureAtlasData;
        private object _replacedTexture;

        /**
         * @private
         */
        public Armature()
        {
        }

        /**
         * @private
         */
        protected override void _onClear()
        {
            foreach (var bone in _bones)
            {
                bone.ReturnToPool();
            }

            foreach (var slot in _slots)
            {
                slot.ReturnToPool();
            }

            foreach (var evt in _events)
            {
                evt.ReturnToPool();
            }

            if (_clock != null)
            {
                _clock.Remove(this);
            }

            if (_proxy != null)
            {
                _proxy._onClear();
            }

            if (_replaceTextureAtlasData != null)
            {
                _replaceTextureAtlasData.ReturnToPool();
            }

            if (_animation != null)
            {
                _animation.ReturnToPool();
            }

            userData = null;

            _delayDispose = false;
            _lockDispose = false;
            _slotsDirty = false;
            _bonesDirty = false;
            _flipX = false;
            _flipY = false;
            _cacheFrameIndex = -1;
            _bones.Clear();
            _slots.Clear();
            _actions.Clear();
            _events.Clear();
            _armatureData = null;
            _skinData = null;
            _animation = null;
            _proxy = null;
            _display = null;
            _eventManager = null;
            _parent = null;
            _clock = null;
            _replaceTextureAtlasData = null;
            _replacedTexture = null;
        }

        /**
         * @private
         */
        private void _sortBones()
        {
            var total = _bones.Count;
            if (total <= 0)
            {
                return;
            }

            var sortHelper = _bones.ToArray();
            int index = 0;
            int count = 0;

            _bones.Clear();

            while (count < total)
            {
                var bone = sortHelper[index++];

                if (index >= total)
                {
                    index = 0;
                }

                if (_bones.Contains(bone))
                {
                    continue;
                }

                if (bone.parent != null && !_bones.Contains(bone.parent))
                {
                    continue;
                }

                if (bone.ik != null && !_bones.Contains(bone.ik))
                {
                    continue;
                }

                if (bone.ik != null && bone.ikChain > 0 && bone.ikChainIndex == bone.ikChain)
                {
                    _bones.Insert(_bones.IndexOf(bone.parent) + 1, bone); // ik, parent, bone, children
                }
                else
                {
                    _bones.Add(bone);
                }

                count++;
            }
        }
        
        private void _sortSlots()
        {
            _slots.Sort(_onSortSlots);
        }

        private void _doAction(ActionData value)
        {
            switch (value.type)
            {
                case ActionType.Play:
                    _animation.Play((string)value.data[0], (int)value.data[1]);
                    break;

                case ActionType.Stop:
                    _animation.Stop((string)value.data[0]);
                    break;

                case ActionType.GotoAndPlay:
                    _animation.GotoAndPlayByTime((string)value.data[0], (float)value.data[1], (int)value.data[2]);
                    break;

                case ActionType.GotoAndStop:
                    _animation.GotoAndStopByTime((string)value.data[0], (float)value.data[1]);
                    break;

                case ActionType.FadeIn:
                    _animation.FadeIn((string)value.data[0], (float)value.data[1], (int)value.data[2]);
                    break;

                case ActionType.FadeOut:
                    // TODO fade out
                    break;

                default:
                    break;
            }
        }

        /**
         * @private
         */
        public void _init(
            ArmatureData armatureData, SkinData skinData,
            object display, IArmatureProxy proxy, IEventDispatcher<EventObject> eventManager
        )
        {
            if (_armatureData != null)
            {
                return;
            }

            _armatureData = armatureData;
            _skinData = skinData;
            _animation = BaseObject.BorrowObject<Animation>();
            _display = display;
            _proxy = proxy;
            _eventManager = eventManager;

            _animation._init(this);
            _animation.animations = _armatureData.animations;
        }

        /**
         * @private
         */
        internal void _addBoneToBoneList(Bone value)
        {
            if (!_bones.Contains(value))
            {
                _bonesDirty = true;
                _bones.Add(value);
                _animation._timelineStateDirty = true;
            }
        }

        /**
         * @private
         */
        internal void _removeBoneFromBoneList(Bone value)
        {
            if (_bones.Contains(value))
            {
                _bones.Remove(value);
                _animation._timelineStateDirty = true;
            }
        }

        /**
         * @private
         */
        internal void _addSlotToSlotList(Slot value)
        {
            if (!_slots.Contains(value))
            {
                _slotsDirty = true;
                _slots.Add(value);
                _animation._timelineStateDirty = true;
            }
        }

        /**
         * @private
         */
        internal void _removeSlotFromSlotList(Slot value)
        {
            if (_slots.Contains(value))
            {
                _slots.Remove(value);
                _animation._timelineStateDirty = true;
            }
        }

        /**
         * @private
         */
        public void _sortZOrder(List<int> slotIndices)
        {
            var sortedSlots = _armatureData.sortedSlots;
            var isOriginal = slotIndices.Count < 1;

            for (int i = 0, l = sortedSlots.Count; i < l; ++i)
            {
                var slotIndex = isOriginal ? i : slotIndices[i];
                var slotData = sortedSlots[slotIndex];
                var slot = GetSlot(slotData.name);

                if (slot != null)
                {
                    slot._setZorder(i);
                }
            }

            _slotsDirty = true;
        }

        /**
         * @private
         */
        internal void _bufferAction(ActionData value)
        {
            _actions.Add(value);
        }

        /**
         * @private
         */
        internal void _bufferEvent(EventObject value, string type)
        {
            value.type = type;
            value.armature = this;
            _events.Add(value);
        }

        /**
         * @language zh_CN
         * 释放骨架。 (会回收到内存池)
         * @version DragonBones 3.0
         */
        public void Dispose()
        {
            _delayDispose = true;

            if (!_lockDispose && _armatureData != null)
            {
                ReturnToPool();
            }
        }

        /**
         * @language zh_CN
         * 更新骨架和动画。 (可以使用时钟实例或显示容器来更新)
         * @param passedTime 两帧之前的时间间隔。 (以秒为单位)
         * @see dragonBones.IAnimateble
         * @see dragonBones.WorldClock
         * @see dragonBones.IArmatureDisplay
         * @version DragonBones 3.0
         */
        public void AdvanceTime(float passedTime)
        {
            if (_animation == null)
            {
                DragonBones.Assert(false, "The armature has been disposed.");
            }

            var scaledPassedTime = passedTime * _animation.timeScale;

            // Bones and slots.
            if (_bonesDirty)
            {
                _bonesDirty = false;
                _sortBones();
            }

            if (_slotsDirty)
            {
                _slotsDirty = false;
                _sortSlots();
            }

            var prevCacheFrameIndex = _cacheFrameIndex;

            // Update animations.
            _animation._advanceTime(scaledPassedTime);

            // Update bones.
            if (_cacheFrameIndex < 0 || _cacheFrameIndex != prevCacheFrameIndex)
            {
                for (int i = 0, l = _bones.Count; i < l; ++i)
                {
                    var bone = _bones[i];
                    bone._update(_cacheFrameIndex);
                }
            }

            // Update slots.
            for (int i = 0, l = _slots.Count; i < l; ++i)
            {
                var slot = _slots [i];
                slot._update(_cacheFrameIndex);

                // Update childArmature.
                var childArmature = slot._childArmature;
                if (childArmature != null)
                {
                    if (slot.inheritAnimation) // Animation's time scale will impact to childArmature.
                    {
                        childArmature.AdvanceTime(scaledPassedTime);
                    }
                    else
                    {
                        childArmature.AdvanceTime(passedTime);
                    }
                }
            }

            //


            if (!_lockDispose)
            {
                _lockDispose = true;

                // Actions and events.
                if (_events.Count > 0) // Dispatch event before action.
                {
                    foreach (var eventObject in _events)
                    {
                        _proxy.DispatchEvent(eventObject.type, eventObject);

                        if (eventObject.type == EventObject.SOUND_EVENT)
                        {
                            _eventManager.DispatchEvent(eventObject.type, eventObject);
                        }

                        eventObject.ReturnToPool();
                    }

                    _events.Clear();
                }

                if (_actions.Count > 0)
                {
                    foreach (var action in _actions)
                    {
                        if (action.slot != null)
                        {
                            var slot = GetSlot(action.slot.name);
                            if (slot != null)
                            {
                                var childArmature = slot._childArmature;
                                if (childArmature != null)
                                {
                                    childArmature._doAction(action);
                                }
                            }
                        }
                        else if (action.bone != null)
                        {
                            foreach (var slot in _slots)
                            {
                                var childArmature = slot._childArmature;
                                if (childArmature != null)
                                {
                                    childArmature._doAction(action);
                                }
                            }
                        }
                        else
                        {
                            _doAction(action);
                        }
                    }

                    _actions.Clear();
                }

                _lockDispose = false;
            }

            if (_delayDispose)
            {
                ReturnToPool();
            }
        }

        /**
         * @language zh_CN
         * 更新骨骼和插槽的变换。 (当骨骼没有动画状态或动画状态播放完成时，骨骼将不在更新)
         * @param boneName 指定的骨骼名称，如果未设置，将更新所有骨骼。
         * @param updateSlotDisplay 是否更新插槽的显示对象。
         * @see dragonBones.Bone
         * @see dragonBones.Slot
         * @version DragonBones 3.0
         */
        public void InvalidUpdate(string boneName = null, bool updateSlotDisplay = false)
        {
            if (!string.IsNullOrEmpty(boneName))
            {
                var bone = GetBone(boneName);
                if (bone != null)
                {
                    bone.InvalidUpdate();

                    if (updateSlotDisplay)
                    {
                        foreach (var slot in _slots)
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
                foreach (var bone in _bones)
                {
                    bone.InvalidUpdate();
                }

                if (updateSlotDisplay)
                {
                    foreach (var slot in _slots)
                    {
                        slot.InvalidUpdate();
                    }
                }
            }
        }

        /**
         * @language zh_CN
         * 判断指定的点是否在所有插槽的自定义包围盒内。
         * @param x 点的水平坐标。（骨架内坐标系）
         * @param y 点的垂直坐标。（骨架内坐标系）
         * @param color 指定的包围盒颜色。 [0: 与所有包围盒进行判断, N: 仅当包围盒的颜色为 N 时才进行判断]
         * @version DragonBones 5.0
         */
        public Slot ContainsPoint(float x, float y, uint color = 0x000000)
        {
            for (int i = 0, l = _slots.Count; i < l; ++i)
            {
                var slot = _slots[i];
                if (slot.ContainsPoint(x, y, color))
                {
                    return slot;
                }
            }

            return null;
        }

        /**
         * @language zh_CN
         * 判断指定的线段与骨架的所有插槽的自定义包围盒是否相交。
         * @param xA 线段起点的水平坐标。（骨架内坐标系）
         * @param yA 线段起点的垂直坐标。（骨架内坐标系）
         * @param xB 线段终点的水平坐标。（骨架内坐标系）
         * @param yB 线段终点的垂直坐标。（骨架内坐标系）
         * @param color 指定的包围盒颜色。 [0: 与所有包围盒进行判断, N: 仅当包围盒的颜色为 N 时才进行判断]
         * @param intersectionPointA 线段从起点到终点与包围盒相交的第一个交点。（骨架内坐标系）
         * @param intersectionPointB 线段从终点到起点与包围盒相交的第一个交点。（骨架内坐标系）
         * @param normalRadians 碰撞点处包围盒切线的法线弧度。 [x: 第一个碰撞点处切线的法线弧度, y: 第二个碰撞点处切线的法线弧度]
         * @returns 线段从起点到终点相交的第一个自定义包围盒的插槽。
         * @version DragonBones 5.0
         */
        public Slot intersectsSegment(
            float xA, float yA, float xB, float yB,
            uint color = 0x000000,
            Point intersectionPointA = null,
            Point intersectionPointB = null,
            Point normalRadians = null
        )
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

            for (int i = 0, l = _slots.Count; i < l; ++i)
            {
                var slot = _slots[i];
                var intersectionCount = slot.IntersectsSegment(xA, yA, xB, yB, color, intersectionPointA, intersectionPointB, normalRadians);
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
         * @language zh_CN
         * 获取指定名称的骨骼。
         * @param name 骨骼的名称。
         * @returns 骨骼。
         * @see dragonBones.Bone
         * @version DragonBones 3.0
         */
        public Bone GetBone(string name)
        {
            foreach (var bone in _bones)
            {
                if (bone.name == name)
                {
                    return bone;
                }
            }

            return null;
        }

        /**
         * @language zh_CN
         * 通过显示对象获取骨骼。
         * @param display 显示对象。
         * @returns 包含这个显示对象的骨骼。
         * @see dragonBones.Bone
         * @version DragonBones 3.0
         */
        public Bone GetBoneByDisplay(object display)
        {
            var slot = GetSlotByDisplay(display);

            return slot != null ? slot.parent : null;
        }

        /**
         * @language zh_CN
         * 获取指定名称的插槽。
         * @param name 插槽的名称。
         * @returns 插槽。
         * @see dragonBones.Slot
         * @version DragonBones 3.0
         */
        public Slot GetSlot(string name)
        {
            foreach (var slot in _slots)
            {
                if (slot.name == name)
                {
                    return slot;
                }
            }

            return null;
        }

        /**
         * @language zh_CN
         * 通过显示对象获取插槽。
         * @param display 显示对象。
         * @returns 包含这个显示对象的插槽。
         * @see dragonBones.Slot
         * @version DragonBones 3.0
         */
        public Slot GetSlotByDisplay(object display)
        {
            if (display != null)
            {
                foreach (var slot in _slots)
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
         * @language zh_CN
         * 替换骨架的主贴图，根据渲染引擎的不同，提供不同的贴图数据。
         * @param texture 用来替换的贴图，根据渲染平台的不同，类型会有所不同，一般是 Texture 类型。
         * @version DragonBones 4.5
         */
        public void ReplaceTexture(object texture)
        {
            replacedTexture = texture;
        }

        /**
         * @language zh_CN
         * 获取所有骨骼。
         * @see dragonBones.Bone
         * @version DragonBones 3.0
         */
        public List<Bone> GetBones()
        {
            return _bones;
        }

        /**
         * @language zh_CN
         * 获取所有插槽。
         * @see dragonBones.Slot
         * @version DragonBones 3.0
         */
        public List<Slot> GetSlots()
        {
            return _slots;
        }

        /**
         * @language zh_CN
         * 骨架名称。
         * @see dragonBones.ArmatureData#name
         * @version DragonBones 3.0
         */
        public string name
        {
            get { return _armatureData != null ? _armatureData.name : null; }
        }

        /**
         * @language zh_CN
         * 获取骨架数据。
         * @see dragonBones.ArmatureData
         * @version DragonBones 4.5
         */
        public ArmatureData armatureData
        {
            get { return _armatureData; }
        }

        /**
         * @language zh_CN
         * 获得动画控制器。
         * @see dragonBones.Animation
         * @version DragonBones 3.0
         */
        public Animation animation
        {
            get { return _animation; }
        }

        /**
         * @language zh_CN
         * eventDispatcher 实例。
         * @version DragonBones 3.0
         */
        public IEventDispatcher<EventObject> eventDispatcher
        {
            get { return _proxy; }
        }

        /**
         * @language zh_CN
         * 获取显示容器，插槽的显示对象都会以此显示容器为父级，根据渲染平台的不同，类型会不同，通常是 DisplayObjectContainer 类型。
         * @version DragonBones 3.0
         */
        public object display
        {
            get { return _display; }
        }

        /**
         * @language zh_CN
         * 获取父插槽。 (当此骨架是某个骨架的子骨架时，可以通过此属性向上查找从属关系)
         * @see dragonBones.Slot
         * @version DragonBones 4.5
         */
        public Slot parent
        {
            get { return _parent; }
        }

        /**
         * @language zh_CN
         * 动画缓存的帧率，当设置一个大于 0 的帧率时，将会开启动画缓存。
         * 通过将动画数据缓存在内存中来提高运行性能，会有一定的内存开销。
         * 帧率不宜设置的过高，通常跟动画的帧率相当且低于程序运行的帧率。
         * 开启动画缓存后，某些功能将会失效，比如 Bone 和 Slot 的 offset 属性等。
         * @see dragonBones.DragonBonesData#frameRate
         * @see dragonBones.ArmatureData#frameRate
         * @version DragonBones 4.5
         */
        public uint cacheFrameRate
        {
            get { return _armatureData.cacheFrameRate; }

            set
            {
                if (_armatureData.cacheFrameRate != value)
                {
                    _armatureData.CacheFrames(value);

                    // Set child armature frameRate.
                    foreach (var slot in _slots)
                    {
                        var childArmature = slot.childArmature;
                        if (childArmature != null && childArmature.cacheFrameRate == 0)
                        {
                            childArmature.cacheFrameRate = value;
                        }
                    }
                }
            }
        }

        /**
         * @private
         */
        public WorldClock clock
        {
            get { return _clock; }
            set
            {
                if (_clock == value)
                {
                    return;
                }

                var prevClock = _clock;
                _clock = value;

                if (prevClock != null)
                {
                    prevClock.Remove(this);
                }

                if (_clock != null)
                {
                    _clock.Add(this);
                }
            }
        }
        /**
         * @language zh_CN
         * 替换骨架的主贴图，根据渲染引擎的不同，提供不同的贴图数据。
         * @version DragonBones 4.5
         */
        public object replacedTexture
        {
            get { return _replacedTexture; }
            set
            {
                if (_replaceTextureAtlasData != null)
                {
                    _replaceTextureAtlasData.ReturnToPool();
                    _replaceTextureAtlasData = null;
                }

                _replacedTexture = value;

                for (int i = 0, l = _slots.Count; i < l; ++i)
                {
                    _slots[i].InvalidUpdate();
                    _slots[i]._update(-1);
                }
            }
        }

        public bool flipX
        {
            get { return _flipX; }

            set
            {
                if (_flipX == value)
                {
                    return;
                }

                _flipX = value;
                
                // Flipping child armature.
                foreach (var slot in _slots)
                {
                    var childArmature = slot.childArmature;
                    if (childArmature != null)
                    {
                        childArmature.flipX = _flipX;
                    }
                }

                InvalidUpdate(null, true);
            }
        }

        public bool flipY
        {
            get { return _flipY; }

            set
            {
                if (_flipY == value)
                {
                    return;
                }

                _flipY = value;

                // Flipping child armature.
                foreach (var slot in _slots)
                {
                    var childArmature = slot.childArmature;
                    if (childArmature != null)
                    {
                        childArmature.flipY = _flipY;
                    }
                }

                InvalidUpdate(null, true);
            }
        }

        /**
         * @deprecated
         */
        public void AddSlot(Slot value, string parentName)
        {
            var bone = GetBone(parentName);
            if (bone != null)
            {
                value._setArmature(this);
                value._setParent(bone);
            }
            else
            {
                DragonBones.Assert(false, DragonBones.ARGUMENT_ERROR);
            }
        }

        /**
         * @deprecated
         */
        public void AddBone(Bone value, string parentName = null)
        {
            if (value != null)
            {
                value._setArmature(this);
                value._setParent(!string.IsNullOrEmpty(parentName) ? GetBone(parentName) : null);
            }
            else
            {
                DragonBones.Assert(false, DragonBones.ARGUMENT_ERROR);
            }
        }
    }
}
