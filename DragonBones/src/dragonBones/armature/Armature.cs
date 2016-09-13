using System;
using System.Collections.Generic;

namespace dragonBones
{
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
        public bool _bonesDirty;

        /**
         * @private
         */
        public int _cacheFrameIndex;

        /**
         * @private
         */
        public ArmatureData _armatureData;

        /**
         * @private
         */
        public SkinData _skinData;

        /**
         * @private
         */
        public Animation _animation;

        /**
         * @private
         */
        public IArmatureDisplay _display;

        /**
         * @private
         */
        public Slot _parent;

        /**
         * @private
         */
        public object _replacedTexture;

        /**
         * @private
         */
        private bool _delayDispose;

        /**
         * @private
         */
        private bool _lockDispose;

        /**
         * @private
         */
        private bool _slotsDirty;

        /**
         * @private Store bones based on bones' hierarchy (From root to leaf)
         */
        private List<Bone> _bones = new List<Bone>();

        /**
         * @private Store slots based on slots' zOrder (From low to high)
         */
        private List<Slot> _slots = new List<Slot>();

        /**
         * @private
         */
        private List<ActionData> _actions = new List<ActionData>();

        /**
         * @private
         */
        private List<EventObject> _events = new List<EventObject>();

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
                bone.returnToPool();
            }

            foreach (var slot in _slots)
            {
                slot.returnToPool();
            }

            foreach (var evt in _events)
            {
                evt.returnToPool();
            }

            userData = null;

            _bonesDirty = false;
            _cacheFrameIndex = -1;
            _armatureData = null;
            _skinData = null;

            if (_animation != null)
            {
                _animation.returnToPool();
                _animation = null;
            }

            if (_display != null)
            {
                _display._onClear();
                _display = null;
            }

            _parent = null;
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

                if (_bones.IndexOf(bone) >= 0)
                {
                    continue;
                }

                if (bone.parent != null && _bones.IndexOf(bone.parent) < 0)
                {
                    continue;
                }

                if (bone.ik != null && _bones.IndexOf(bone.ik) < 0)
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
        private void _sortSlots()
        {
        }

        /**
         * @private
         */
        private void _doAction(ActionData value)
        {
            switch (value.type)
            {
                case ActionType.Play:
                    _animation.play(value.data[0], value.data[1]);
                    break;

                case ActionType.Stop:
                    _animation.stop(value.data[0]);
                    break;

                case ActionType.GotoAndPlay:
                    _animation.gotoAndPlayByTime(value.data[0], value.data[1], value.data[2]);
                    break;

                case ActionType.GotoAndStop:
                    _animation.gotoAndStopByTime(value.data[0], value.data[1]);
                    break;

                case ActionType.FadeIn:
                    _animation.fadeIn(value.data[0], value.data[1], value.data[2]);
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
        public void _addBoneToBoneList(Bone value)
        {
            if (_bones.IndexOf(value) < 0)
            {
                _bonesDirty = true;
                _bones.Add(value);
                _animation._timelineStateDirty = true;
            }
        }

        /**
         * @private
         */
        public void _removeBoneFromBoneList(Bone value)
        {
            var index = _bones.IndexOf(value);
            if (index >= 0)
            {
                _bones.RemoveAt(index);
                _animation._timelineStateDirty = true;
            }
        }

        /**
         * @private
         */
        public void _addSlotToSlotList(Slot value)
        {
            if (_slots.IndexOf(value) < 0)
            {
                _slotsDirty = true;
                _slots.Add(value);
                _animation._timelineStateDirty = true;
            }
        }

        /**
         * @private
         */
        public void _removeSlotFromSlotList(Slot value)
        {
            var index = _slots.IndexOf(value);
            if (index >= 0)
            {
                _slots.RemoveAt(index);
                _animation._timelineStateDirty = true;
            }
        }

        /**
         * @private
         */
        public void _bufferAction(ActionData value)
        {
            _actions.Add(value);
        }

        /**
         * @private
         */
        public void _bufferEvent(EventObject value, string type)
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
        public void dispose()
        {
            _delayDispose = true;

            if (!_lockDispose && _animation != null) //
            {
                returnToPool();
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
        public void advanceTime(float passedTime)
        {
            if (_animation == null)
            {
                DragonBones.warn("The armature has been disposed.");
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

            foreach (var bone in _bones)
            {
                bone._update(_cacheFrameIndex);
            }

            foreach (var slot in _slots)
            {
                slot._update(_cacheFrameIndex);

                var childArmature = slot._childArmature;
                if (childArmature != null)
                {
                    if (slot.inheritAnimation) // Animation's time scale will impact to childArmature.
                    {
                        childArmature.advanceTime(scaledPassedTime);
                    }
                    else
                    {
                        childArmature.advanceTime(passedTime);
                    }
                }
            }

            //
            if (DragonBones.debugDraw)
            {
                _display._debugDraw();
            }

            if (!_lockDispose)
            {
                _lockDispose = true;

                // Actions and events.
                if (_events.Count > 0) // Dispatch event before action.
                {
                    foreach (var evt in _events)
                    {
                        if (EventObject._soundEventManager != null && evt.type == EventObject.SOUND_EVENT)
                        {
                            EventObject._soundEventManager._dispatchEvent(evt);
                        }
                        else
                        {
                            _display._dispatchEvent(evt);
                        }

                        evt.returnToPool();
                    }

                    _events.Clear();
                }

                if (_actions.Count > 0)
                {
                    foreach (var action in _actions)
                    {
                        if (action.slot != null)
                        {
                            var slot = getSlot(action.slot.name);
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
                            this._doAction(action);
                        }
                    }

                    _actions.Clear();
                }

                _lockDispose = false;
            }

            if (_delayDispose)
            {
                returnToPool();
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
        public void invalidUpdate(string boneName, bool updateSlotDisplay = false)
        {
            if (boneName.Length > 0)
            {
                var bone = getBone(boneName);
                if (bone != null)
                {
                    bone.invalidUpdate();

                    if (updateSlotDisplay)
                    {
                        foreach (var slot in _slots)
                        {
                            if (slot.parent == bone)
                            {
                                slot.invalidUpdate();
                            }
                        }
                    }
                }
            }
            else
            {
                foreach (var bone in _bones)
                {
                    bone.invalidUpdate();
                }

                if (updateSlotDisplay)
                {
                    foreach (var slot in _slots)
                    {
                        slot.invalidUpdate();
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
        public Slot getSlot(string name)
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
        public Slot getSlotByDisplay(object display)
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
        public void addSlot(Slot value, string parentName)
        {
            var bone = getBone(parentName);
            if (bone != null)
            {
                value._setArmature(this);
                value._setParent(bone);
            }
            else
            {
                DragonBones.warn("");
            }
        }

        /**
         * @language zh_CN
         * 将一个指定的插槽从骨架中移除。
         * @param value 需要移除的插槽
         * @see dragonBones.Slot
         * @version DragonBones 3.0
         */
        public void removeSlot(Slot value)
        {
            if (value != null && value.armature == this)
            {
                value._setParent(null);
                value._setArmature(null);
            }
            else
            {
                DragonBones.warn("");
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
        public Bone getBone(string name)
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
        public Bone getBoneByDisplay(object display)
        {
            var slot = getSlotByDisplay(display);

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
        public void addBone(Bone value, string parentName = null)
        {
            if (value != null)
            {
                value._setArmature(this);
                value._setParent(DragonBones.isAvailableString(parentName) ? this.getBone(parentName) : null);
            }
            else
            {
                DragonBones.warn("");
            }
        }

        /**
         * @language zh_CN
         * 将一个指定的骨骼从骨架中移除。
         * @param value 需要移除的骨骼。
         * @see dragonBones.Bone
         * @version DragonBones 3.0
         */
        public void removeBone(Bone value)
        {
            if (value != null && value.armature == this)
            {
                value._setParent(null);
                value._setArmature(null);
            }
            else
            {
                DragonBones.warn("");
            }
        }

        /**
         * @language zh_CN
         * 替换骨架的主贴图，根据渲染引擎的不同，提供不同的贴图数据。
         * @param texture 用来替换的贴图，根据渲染平台的不同，类型会有所不同，一般是 Texture 类型。
         * @version DragonBones 4.5
         */
        public void replaceTexture(object texture)
        {
            _replacedTexture = texture;

            foreach (var slot in _slots)
            {
                slot.invalidUpdate();
            }
        }

        /**
         * @language zh_CN
         * 获取所有骨骼。
         * @see dragonBones.Bone
         * @version DragonBones 3.0
         */
        public List<Bone> getBones()
        {
            return _bones;
        }

        /**
         * @language zh_CN
         * 获取所有插槽。
         * @see dragonBones.Slot
         * @version DragonBones 3.0
         */
        public List<Slot> getSlots()
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
            get { return _armatureData != null ? this._armatureData.name : null; }
        }

        /**
         * @language zh_CN
         * 获取骨架数据。
         * @see dragonBones.ArmatureData
         * @version DragonBones 4.5
         */
        public ArmatureData armatureData
        {
            get { return this._armatureData; }
        }

        /**
         * @language zh_CN
         * 获得动画控制器。
         * @see dragonBones.Animation
         * @version DragonBones 3.0
         */
        public Animation animation
        {
            get { return this._animation; }
        }

        /**
         * @language zh_CN
         * 获取显示容器，插槽的显示对象都会以此显示容器为父级，根据渲染平台的不同，类型会不同，通常是 DisplayObjectContainer 类型。
         * @version DragonBones 3.0
         */
        public IArmatureDisplay display
        {
            get { return this._display; }
        }

        /**
         * @language zh_CN
         * 获取父插槽。 (当此骨架是某个骨架的子骨架时，可以通过此属性向上查找从属关系)
         * @see dragonBones.Slot
         * @version DragonBones 4.5
         */
        public Slot parent
        {
            get { return this._parent; }
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
            get { return this._armatureData.cacheFrameRate; }

            set
            {
                if (this._armatureData.cacheFrameRate != value)
                {
                    this._armatureData.cacheFrames(value);

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
    }
}
