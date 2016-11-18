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
        /**
         * @language zh_CN
         * 可以用于存储临时数据。
         * @version DragonBones 3.0
         */
        public object userData;

        /**
         * @private
         */
        internal bool _bonesDirty;

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

        /**
         * @private
         */
        internal ArmatureData _armatureData;

        /**
         * @private
         */
        internal SkinData _skinData;

        /**
         * @private
         */
        internal Animation _animation;

        /**
         * @private
         */
        internal object _display;

        /**
         * @private
         */
        internal IEventDispatcher<EventObject> _eventDispatcher;

        /**
         * @private
         */
        internal IEventDispatcher<EventObject> _eventManager;

        /**
         * @private
         */
        internal Slot _parent;

        /**
         * @private
         */
        internal WorldClock _clock;

        /**
         * @private
         */
        internal object _replacedTexture;

        /**
         * @private
         */
        protected bool _delayDispose;

        /**
         * @private
         */
        protected bool _lockDispose;

        /**
         * @private
         */
        protected bool _slotsDirty;

        /**
         * @private Store bones based on bones' hierarchy (From root to leaf)
         */
        protected readonly List<Bone> _bones = new List<Bone>();

        /**
         * @private Store slots based on slots' zOrder (From low to high)
         */
        protected readonly List<Slot> _slots = new List<Slot>();

        /**
         * @private
         */
        protected readonly List<ActionData> _actions = new List<ActionData>();

        /**
         * @private
         */
        protected readonly List<EventObject> _events = new List<EventObject>();

        /**
         * @private
         */
        public Armature()
        {
        }

        /**
         * @inheritDoc
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

            if (_animation != null)
            {
                _animation.ReturnToPool();
            }

            if (_eventDispatcher != null && _eventDispatcher != _display) // May be _eventDispatcher and _display is the same one.
            {
                _eventDispatcher._onClear();
            }

            if (_display != null)
            {
                var armatureDisplay = _display as IArmatureDisplay;
                if (armatureDisplay != null)
                {
                    armatureDisplay._onClear();
                }
            }

            if (_clock != null)
            {
                _clock.Remove(this);
            }

            userData = null;

            _bonesDirty = false;
            _flipX = false;
            _flipY = false;
            _cacheFrameIndex = -1;
            _armatureData = null;
            _skinData = null;
            _animation = null;
            _eventDispatcher = null;
            _display = null;
            _eventManager = null;
            _parent = null;
            _clock = null;
            _replacedTexture = null;

            _delayDispose = false;
            _lockDispose = false;
            _slotsDirty = false;
            _bones.Clear();
            _slots.Clear();
            _actions.Clear();
            _events.Clear();
        }

        /**
         * @private
         */
        protected void _sortBones()
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

        /**
         * @private
         */
        protected void _sortSlots()
        {
        }

        /**
         * @private
         */
        protected void _doAction(ActionData value)
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

                if (slot != null && slot._zOrder != i)
                {
                    slot._zOrder = i;
                    slot._zOrderDirty = true;
                }
            }

            this._slotsDirty = true;
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
         * @private
         */
        public void _onAdd(WorldClock value)
        {
            if (_clock != null)
            {
                _clock.Remove(this);
            }

            _clock = value;
        }

        /**
         * @private
         */
        public void _onRemove()
        {
            _clock = null;
        }

        /**
         * @language zh_CN
         * 释放骨架。 (会回收到内存池)
         * @version DragonBones 3.0
         */
        public void Dispose()
        {
            _delayDispose = true;

            if (!_lockDispose && _animation != null) //
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
                DragonBones.Warn("The armature has been disposed.");
            }

            var scaledPassedTime = passedTime * _animation.timeScale;

            // Animations.
            _animation._advanceTime(scaledPassedTime);

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

            for (int i = 0, l = _bones.Count; i < l; ++i)
            {
                var bone = _bones [i];
                bone._update(_cacheFrameIndex);
            }

            for (int i = 0, l = _slots.Count; i < l; ++i)
            {
                var slot = _slots [i];
                slot._update(_cacheFrameIndex);

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

            if (!_lockDispose)
            {
                _lockDispose = true;

                // Actions and events.
                if (_events.Count > 0) // Dispatch event before action.
                {
                    foreach (var evt in _events)
                    {
                        _eventDispatcher.DispatchEvent(evt.type, evt);

                        if (evt.type == EventObject.SOUND_EVENT)
                        {
                            _eventManager.DispatchEvent(evt.type, evt);
                        }

                        evt.ReturnToPool();
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
            if (DragonBones.IsAvailableString(boneName))
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
         * 将一个指定的插槽添加到骨架中。
         * @param value 需要添加的插槽。
         * @param parentName 需要添加到的父骨骼名称。
         * @see dragonBones.Slot
         * @version DragonBones 3.0
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
                DragonBones.Warn("");
            }
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
         * 将一个指定的骨骼添加到骨架中。
         * @param value 需要添加的骨骼。
         * @param parentName 需要添加到父骨骼的名称，如果未设置，则添加到骨架根部。
         * @see dragonBones.Bone
         * @version DragonBones 3.0
         */
        public void AddBone(Bone value, string parentName = null)
        {
            if (value != null)
            {
                value._setArmature(this);
                value._setParent(DragonBones.IsAvailableString(parentName) ? GetBone(parentName) : null);
            }
            else
            {
                DragonBones.Warn("");
            }
        }

        /**
         * @language zh_CN
         * 替换骨架的主贴图，根据渲染引擎的不同，提供不同的贴图数据。
         * @param texture 用来替换的贴图，根据渲染平台的不同，类型会有所不同，一般是 Texture 类型。
         * @version DragonBones 4.5
         */
        public void ReplaceTexture(object texture)
        {
            _replacedTexture = texture;

            foreach (var slot in _slots)
            {
                slot.InvalidUpdate();
            }
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
         * 获取显示容器，插槽的显示对象都会以此显示容器为父级，根据渲染平台的不同，类型会不同，通常是 DisplayObjectContainer 类型。
         * @version DragonBones 3.0
         */
        public object display
        {
            get { return _display; }
        }

        /**
         * @language zh_CN
         * eventDispatcher 实例。
         * @version DragonBones 3.0
         */
        public IEventDispatcher<EventObject> eventDispatcher
        {
            get { return _eventDispatcher; }
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
    }
}
