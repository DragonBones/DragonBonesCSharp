using System.Collections.Generic;

namespace dragonBones
{
    /**
     * @language zh_CN
     * 插槽，附着在骨骼上，控制显示对象的显示状态和属性。
     * 一个骨骼上可以包含多个插槽。
     * 一个插槽中可以包含多个显示对象，同一时间只能显示其中的一个显示对象，但可以在动画播放的过程中切换显示对象实现帧动画。
     * 显示对象可以是普通的图片纹理，也可以是子骨架的显示容器，网格显示对象，还可以是自定义的其他显示对象。
     * @see dragonBones.Armature
     * @see dragonBones.Bone
     * @see dragonBones.SlotData
     * @version DragonBones 3.0
     */
    public abstract class Slot : TransformObject
    {
        /**
         * @language zh_CN
         * 子骨架是否继承父骨架的动画。 [true: 继承, false: 不继承]
         * @default true
         * @version DragonBones 4.5
         */
        public bool inheritAnimation;

        /**
         * @language zh_CN
         * 显示对象受到控制的对象，应设置为动画状态的名称或组名称，设置为 null 则表示受所有的动画状态控制。
         * @default null
         * @see dragonBones.AnimationState#displayControl
         * @see dragonBones.AnimationState#name
         * @see dragonBones.AnimationState#group
         * @version DragonBones 4.5
         */
        public string displayController;

        /**
         * @private
         */
        public bool _colorDirty;

        /**
         * @private
         */
        public bool _ffdDirty;

        /**
         * @private
         */
        public int _blendIndex;

        /**
         * @private
         */
        public int _zOrder;

        /**
         * @private
         */
        public float _pivotX;

        /**
         * @private
         */
        public float _pivotY;

        /**
         * @private
         */
        public SlotDisplayDataSet _displayDataSet;

        /**
         * @private
         */
        public MeshData _meshData;

        /**
         * @private
         */
        public Armature _childArmature;

        /**
         * @private
         */
        public object _rawDisplay;

        /**
         * @private
         */
        public object _meshDisplay;

        /**
         * @private
         */
        public List<Matrix> _cacheFrames;

        /**
         * @private
         */
        public readonly ColorTransform _colorTransform = new ColorTransform();

        /**
         * @private
         */
        public readonly List<float> _ffdVertices = new List<float>();

        /**
         * @private
         */
        public readonly List<DisplayData> _replacedDisplayDataSet = new List<DisplayData>();

        /**
         * @private
         */
        protected bool _displayDirty;

        /**
         * @private
         */
        protected bool _blendModeDirty;

        /**
         * @private
         */
        protected bool _originDirty;

        /**
         * @private
         */
        protected bool _transformDirty;

        /**
         * @private
         */
        protected int _displayIndex;

        /**
         * @private
         */
        protected BlendMode _blendMode;

        /**
         * @private
         */
        protected object _display;

        /**
         * @private
         */
        protected readonly Matrix _localMatrix = new Matrix();

        /**
         * @private
         */
        protected readonly List<object> _displayList = new List<object>();

        /**
         * @private
         */
        protected readonly List<Bone> _meshBones = new List<Bone>();

        public Slot()
        {
        }

        /**
         * @inheritDoc
         */
        protected override void _onClear()
        {
            base._onClear();

            var disposeDisplayList = new List<object>();

            foreach (var eachDisplay in _displayList)
            {
                if (
                    eachDisplay != _rawDisplay && eachDisplay != _meshDisplay &&
                    !disposeDisplayList.Contains(eachDisplay)
                )
                {
                    disposeDisplayList.Add(eachDisplay);
                }
            }

            foreach (var eachDisplay in disposeDisplayList)
            {
                if (eachDisplay is Armature)
                {
                    ((Armature)eachDisplay).dispose();
                }
                else
                {
                    _disposeDisplay(eachDisplay);
                }
            }

            if (_meshDisplay != null && _meshDisplay != _rawDisplay) // May be _meshDisplay and _rawDisplay is the same one.
            {
                _disposeDisplay(_meshDisplay);
            }

            if (_rawDisplay != null)
            {
                _disposeDisplay(_rawDisplay);
            }

            inheritAnimation = true;
            displayController = null;

            _colorDirty = false;
            _ffdDirty = false;
            _blendIndex = 0;
            _zOrder = 0;
            _pivotX = 0.0f;
            _pivotY = 0.0f;
            _displayDataSet = null;
            _meshData = null;
            _childArmature = null;
            _rawDisplay = null;
            _meshDisplay = null;
            _cacheFrames = null;
            _colorTransform.identity();
            _ffdVertices.Clear();
            _replacedDisplayDataSet.Clear();

            _displayDirty = false;
            _blendModeDirty = false;
            _originDirty = false;
            _transformDirty = false;
            _displayIndex = 0;
            _blendMode = BlendMode.Normal;
            _display = null;
            _localMatrix.identity();
            _displayList.Clear();
            _meshBones.Clear();
        }

        /**
         * @private
         */
        protected abstract void _initDisplay(object value);

        /**
         * @private
         */
        protected abstract void _disposeDisplay(object value);

        /**
         * @private
         */
        protected abstract void _onUpdateDisplay();

        /**
         * @private
         */
        protected abstract void _addDisplay();

        /**
         * @private
         */
        protected abstract void _replaceDisplay(object value);

        /**
         * @private
         */
        protected abstract void _removeDisplay();

        /**
         * @private Bone
         */
        internal abstract void _updateVisible();

        /**
         * @private
         */
        protected abstract void _updateBlendMode();

        /**
         * @private
         */
        protected abstract void _updateColor();

        /**
         * @private
         */
        protected abstract void _updateFilters();

        /**
         * @private
         */
        protected abstract void _updateFrame();

        /**
         * @private
         */
        protected abstract void _updateMesh();

        /**
         * @private
         */
        protected abstract void _updateTransform();

        /**
         * @private
         */
        private bool _isMeshBonesUpdate()
        {
            foreach (var bone in _meshBones)
            {
                if (bone._transformDirty != BoneTransformDirty.None)
                {
                    return true;
                }
            }

            return false;
        }

        /**
         * @private
         */
        protected void _updatePivot(DisplayData rawDisplayData, DisplayData currentDisplayData, TextureData currentTextureData)
        {
            var isReplaceDisplay = rawDisplayData != null && rawDisplayData != currentDisplayData;
            if (_meshData != null && _display == _meshDisplay)
            {
                if (_meshData != rawDisplayData.mesh && isReplaceDisplay)
                {
                    _pivotX = rawDisplayData.transform.x - currentDisplayData.transform.x;
                    _pivotY = rawDisplayData.transform.y - currentDisplayData.transform.y;
                }
                else
                {
                    _pivotX = 0.0f;
                    _pivotY = 0.0f;
                }
            }
            else
            {
                _pivotX = currentDisplayData.pivot.x;
                _pivotY = currentDisplayData.pivot.y;

                if (currentDisplayData.isRelativePivot)
                {
                    var scale = this._armature.armatureData.scale;
                    var rect = currentTextureData.frame != null ? currentTextureData.frame : currentTextureData.region;
                    var width = rect.width * scale;
                    var height = rect.height * scale;

                    if (currentTextureData.rotated)
                    {
                        width = rect.height;
                        height = rect.width;
                    }

                    _pivotX *= width;
                    _pivotY *= height;
                }

                if (currentTextureData.frame != null)
                {
                    _pivotX += currentTextureData.frame.x;
                    _pivotY += currentTextureData.frame.y;
                }

                if (isReplaceDisplay)
                {
                    _pivotX += rawDisplayData.transform.x - currentDisplayData.transform.x;
                    _pivotY += rawDisplayData.transform.y - currentDisplayData.transform.y;
                }
            }
        }

        /**
         * @private
         */
        protected void _updateDisplay()
        {
            var prevDisplay = _display != null ? _display : _rawDisplay;
            var prevChildArmature = _childArmature;

            if (_displayIndex >= 0 && _displayIndex < _displayList.Count)
            {
                _display = _displayList[_displayIndex];
                if (_display is Armature)
                {
                    _childArmature = (Armature)_display;
                    _display = _childArmature._display;
                }
                else
                {
                    _childArmature = null;
                }
            }
            else
            {
                _display = null;
                _childArmature = null;
            }

            var currentDisplay = _display != null ? _display : _rawDisplay;

            if (currentDisplay != prevDisplay)
            {
                _onUpdateDisplay();

                if (prevDisplay != null)
                {
                    _replaceDisplay(prevDisplay);
                }
                else
                {
                    _addDisplay();
                }

                _blendModeDirty = true;
                _colorDirty = true;
            }

            // Update origin.
            if (_displayDataSet != null && _displayIndex >= 0 && _displayIndex < _displayDataSet.displays.Count)
            {
                origin.copyFrom(_displayDataSet.displays[_displayIndex].transform);
                _originDirty = true;
            }

            // Update meshData.
            _updateMeshData(false);

            // Update frame.
            if (currentDisplay == _rawDisplay || currentDisplay == _meshDisplay)
            {
                _updateFrame();
            }

            // Update child armature.
            if (_childArmature != prevChildArmature)
            {
                if (prevChildArmature != null)
                {
                    prevChildArmature._parent = null; // Update child armature parent.
                    if (inheritAnimation)
                    {
                        prevChildArmature.animation.reset();
                    }
                }

                if (_childArmature != null)
                {
                    _childArmature._parent = this; // Update child armature parent.
                    if (inheritAnimation)
                    {
                        if (_childArmature.cacheFrameRate == 0) // Set child armature frameRate.
                        {
                            var cacheFrameRate = _armature.cacheFrameRate;
                            if (cacheFrameRate != 0)
                            {
                                _childArmature.cacheFrameRate = cacheFrameRate;
                            }
                        }

                        // Child armature action.                        
                        var slotData = _armature.armatureData.getSlot(name);
                        var actions = slotData.actions.Count > 0 ? slotData.actions : _childArmature.armatureData.actions;
                        if (actions.Count > 0)
                        {
                            foreach (var action in actions)
                            {
                                _childArmature._bufferAction(action);
                            }
                        }
                        else
                        {
                            _childArmature.animation.play();
                        }
                    }
                }
            }
        }

        /**
         * @private
         */
        protected void _updateLocalTransformMatrix()
        {
            this.global.copyFrom(this.origin).add(this.offset).toMatrix(_localMatrix);
        }

        /**
         * @private
         */
        protected void _updateGlobalTransformMatrix()
        {
            this.globalTransformMatrix.copyFrom(_localMatrix);
            this.globalTransformMatrix.concat(this._parent.globalTransformMatrix);
            this.global.fromMatrix(this.globalTransformMatrix);
        }

        /**
         * @inheritDoc
         */
        public override void _setArmature(Armature value)
        {
            if (this._armature == value)
            {
                return;
            }

            if (this._armature != null)
            {
                this._armature._removeSlotFromSlotList(this);
            }

            this._armature = value;

            _onUpdateDisplay();

            if (this._armature != null)
            {
                this._armature._addSlotToSlotList(this);
                _addDisplay();
            }
            else
            {
                _removeDisplay();
            }
        }

        /**
         * @private
         */
        public void _updateMeshData(bool isTimelineUpdate)
        {
            var prevMeshData = _meshData;
            var rawMeshData = (MeshData)null;
            if (_display != null && _display == _meshDisplay && _displayIndex >= 0)
            {
                rawMeshData = (_displayDataSet != null && _displayIndex < _displayDataSet.displays.Count) ? _displayDataSet.displays[_displayIndex].mesh : null;
                var replaceDisplayData = (_displayIndex < _replacedDisplayDataSet.Count) ? _replacedDisplayDataSet[_displayIndex] : null;
                var replaceMeshData = replaceDisplayData != null ? replaceDisplayData.mesh : null;
                _meshData = replaceMeshData != null ? replaceMeshData : rawMeshData;
            }
            else
            {
                _meshData = null;
            }

            if (_meshData != prevMeshData)
            {
                if (_meshData != null && _meshData == rawMeshData)
                {
                    if (_meshData.skinned)
                    {
                        _meshBones.Capacity = _meshData.bones.Count;

                        for (int i = 0, l = _meshBones.Count; i < l; ++i)
                        {
                            _meshBones[i] = this._armature.getBone(_meshData.bones[i].name);
                        }

                        var ffdVerticesCount = 0;
                        for (int i = 0, l = _meshData.boneIndices.Count; i < l; ++i)
                        {
                            ffdVerticesCount += _meshData.boneIndices[i].Count;
                        }

                        _ffdVertices.Capacity = ffdVerticesCount * 2;
                    }
                    else
                    {
                        _meshBones.Clear();
                        _ffdVertices.Capacity = _meshData.vertices.Count;
                    }

                    for (int i = 0, l = _ffdVertices.Capacity; i < l; ++i)
                    {
                        _ffdVertices[i] = 0;
                    }

                    _ffdDirty = true;
                }
                else
                {
                    _meshBones.Clear();
                    _ffdVertices.Clear();
                }

                if (isTimelineUpdate)
                {
                    this._armature.animation._updateFFDTimelineStates();
                }
            }
        }

        /**
         * @private
         */
        public void _update(int cacheFrameIndex)
        {
            _blendIndex = 0;

            if (_displayDirty)
            {
                _displayDirty = false;
                _updateDisplay();
            }

            if (_display == null)
            {
                return;
            }

            if (_blendModeDirty)
            {
                _blendModeDirty = false;
                _updateBlendMode();
            }

            if (_colorDirty)
            {
                _colorDirty = false;
                _updateColor();
            }

            if (_meshData != null)
            {
                if (_ffdDirty || (_meshData.skinned && _isMeshBonesUpdate()))
                {
                    _ffdDirty = false;

                    _updateMesh();
                }

                if (_meshData.skinned)
                {
                    return;
                }
            }

            if (_originDirty)
            {
                _originDirty = false;
                _transformDirty = true;
                _updateLocalTransformMatrix();
            }

            if (cacheFrameIndex >= 0)
            {
                var cacheFrame = _cacheFrames[cacheFrameIndex];
                if (this.globalTransformMatrix == cacheFrame) // Same cache.
                {
                    _transformDirty = false;
                }
                else if (cacheFrame != null) // Has been Cached.
                {
                    _transformDirty = true;
                    this.globalTransformMatrix = cacheFrame;
                }
                else if (_transformDirty || this._parent._transformDirty != BoneTransformDirty.None) // Dirty.
                {
                    _transformDirty = true;
                    this.globalTransformMatrix = this._globalTransformMatrix;
                }
                else if (this.globalTransformMatrix != this._globalTransformMatrix) // Same cache but not cached yet.
                {
                    _transformDirty = false;
                    _cacheFrames[cacheFrameIndex] = this.globalTransformMatrix;
                }
                else // Dirty.
                {
                    _transformDirty = true;
                    this.globalTransformMatrix = this._globalTransformMatrix;
                }
            }
            else if (_transformDirty || this._parent._transformDirty != BoneTransformDirty.None) // Dirty.
            {
                _transformDirty = true;
                this.globalTransformMatrix = this._globalTransformMatrix;
            }

            if (_transformDirty)
            {
                _transformDirty = false;

                if (this.globalTransformMatrix == this._globalTransformMatrix)
                {
                    _updateGlobalTransformMatrix();

                    if (cacheFrameIndex >= 0 && _cacheFrames[cacheFrameIndex] == null)
                    {
                        this.globalTransformMatrix = SlotTimelineData.cacheFrame(_cacheFrames, cacheFrameIndex, this._globalTransformMatrix);
                    }
                }

                _updateTransform();
            }
        }

        /**
         * @private
         */
        public bool _setDisplayList(List<object> value)
        {
            if (value != null && value.Count > 0)
            {
                if (_displayList.Count != value.Count)
                {
                    DragonBones.resizeList(_displayList, value.Count, null);
                }

                for (int i = 0, l = value.Count; i < l; ++i) // Retain input render displays.
                {
                    var eachDisplay = value[i];
                    if (
                        eachDisplay != null && eachDisplay != _rawDisplay && eachDisplay != _meshDisplay &&
                        !(eachDisplay is Armature) && !_displayList.Contains(eachDisplay)
                    )
                    {
                        _initDisplay(eachDisplay);
                    }

                    _displayList[i] = eachDisplay;
                }
            }
            else if (_displayList.Count > 0)
            {
                _displayList.Clear();
            }

            if (_displayIndex >= 0 && _displayIndex < _displayList.Count)
            {
                _displayDirty = _display != _displayList[_displayIndex];
            }
            else
            {
                _displayDirty = _display != null;
            }

            return _displayDirty;
        }

        /**
         * @private
         */
        public bool _setDisplayIndex(int value)
        {
            if (_displayIndex == value)
            {
                return false;
            }

            _displayIndex = value;
            _displayDirty = true;

            return _displayDirty;
        }

        /**
         * @private
         */
        public bool _setBlendMode(BlendMode value)
        {
            if (_blendMode == value)
            {
                return false;
            }

            _blendMode = value;
            _blendModeDirty = true;

            return true;
        }

        /**
         * @private
         */
        public bool _setColor(ColorTransform value)
        {
            _colorTransform.copyFrom(value);
            _colorDirty = true;

            return true;
        }

        /**
         * @language zh_CN
         * 在下一帧更新显示对象的状态。
         * @version DragonBones 4.5
         */
        public void invalidUpdate()
        {
            _displayDirty = true;
        }

        /**
         * @private
         */
        public object rawDisplay
        {
            get { return this._rawDisplay; }
        }

        /**
         * @private
         */
        public object meshDisplay
        {
            get { return this._meshDisplay; }
        }

        /**
         * @language zh_CN
         * 此时显示的显示对象在显示列表中的索引。
         * @version DragonBones 4.5
         */
        public int displayIndex
        {
            get { return this._displayIndex; }

            set
            {
                if (_setDisplayIndex(value))
                {
                    _update(-1);
                }
            }
        }

        /**
         * @language zh_CN
         * 包含显示对象或子骨架的显示列表。
         * @version DragonBones 3.0
         */
        public List<object> displayList
        {
            get { return new List<object>(_displayList.ToArray()); }

            set
            {
                var backupDisplayList = _displayList.ToArray(); // Copy.
                var disposeDisplayList = new List<object>();

                if (_setDisplayList(value))
                {
                    _update(-1);
                }

                // Release replaced render displays.
                foreach (var eachDisplay in backupDisplayList)
                {
                    if (
                        eachDisplay != null && eachDisplay != _rawDisplay && eachDisplay != _meshDisplay &&
                        !_displayList.Contains(eachDisplay) &&
                        !disposeDisplayList.Contains(eachDisplay)
                    )
                    {
                        disposeDisplayList.Add(eachDisplay);
                    }
                }

                foreach (var eachDisplay in disposeDisplayList)
                {
                    if (eachDisplay is Armature)
                    {
                        ((Armature)eachDisplay).dispose();
                    }
                    else
                    {
                        _disposeDisplay(eachDisplay);
                    }
                }
            }
        }

        /**
         * @language zh_CN
         * 此时显示的显示对象。
         * @version DragonBones 3.0
         */
        public object display
        {
            get { return _display; }
            set
            {
                if (_display == value)
                {
                    return;
                }

                var displayListLength = _displayList.Count;
                if (_displayIndex < 0 && displayListLength == 0) // Emprty.
                {
                    _displayIndex = 0;
                }

                if (_displayIndex < 0)
                {
                    return;
                }
                else
                {
                    var replaceDisplayList = displayList; // Copy.
                    if (displayListLength <= _displayIndex)
                    {
                        DragonBones.resizeList(replaceDisplayList, _displayIndex + 1, null);
                    }

                    replaceDisplayList[_displayIndex] = value;
                    displayList = replaceDisplayList;
                }
            }
        }

        /**
         * @language zh_CN
         * 此时显示的子骨架。
         * @see dragonBones.Armature
         * @version DragonBones 3.0
         */
        public Armature childArmature
        {
            get { return _childArmature; }

            set
            {
                if (_childArmature == value)
                {
                    return;
                }

                if (value != null)
                {
                    ((IArmatureDisplay)value.display).advanceTimeBySelf(false); // Stop child armature self advanceTime.
                }

                this.display = value;
            }
        }
    }
}