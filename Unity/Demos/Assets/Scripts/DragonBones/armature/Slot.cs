using System;
using System.Collections.Generic;

namespace DragonBones
{
    /**
     * @language zh_CN
     * 插槽，附着在骨骼上，控制显示对象的显示状态和属性。
     * 一个骨骼上可以包含多个插槽。
     * 一个插槽中可以包含多个显示对象，同一时间只能显示其中的一个显示对象，但可以在动画播放的过程中切换显示对象实现帧动画。
     * 显示对象可以是普通的图片纹理，也可以是子骨架的显示容器，网格显示对象，还可以是自定义的其他显示对象。
     * @see DragonBones.Armature
     * @see DragonBones.Bone
     * @see DragonBones.SlotData
     * @version DragonBones 3.0
     */
    public abstract class Slot : TransformObject
    {
        /**
         * @private
         */
        protected static readonly Point _helpPoint = new Point();
        /**
         * @private
         */
        protected static readonly Matrix _helpMatrix = new Matrix();
        /**
         * @language zh_CN
         * 显示对象受到控制的动画状态或混合组名称，设置为 null 则表示受所有的动画状态控制。
         * @default null
         * @see DragonBones.AnimationState#displayControl
         * @see DragonBones.AnimationState#name
         * @see DragonBones.AnimationState#group
         * @version DragonBones 4.5
         */
        public string displayController;
        /**
         * @private
         */
        protected bool _displayDirty;
        /**
         * @private
         */
        protected bool _zOrderDirty;
        /**
         * @private
         */
        protected bool _blendModeDirty;
        /**
         * @private
         */
        internal bool _colorDirty;
        /**
         * @private
         */
        protected bool _originalDirty;
        /**
         * @private
         */
        protected bool _transformDirty;
        /**
         * @private
         */
        internal bool _meshDirty;
        /**
         * @private
         */
        protected int _updateState;
        /**
         * @private
         */
        protected BlendMode _blendMode;
        /**
         * @private
         */
        protected int _displayIndex;
        /**
         * @private
         */
        protected int _cachedFrameIndex;
        /**
         * @private
         */
        internal int _zOrder;
        /**
         * @private
         */
        protected float _pivotX;
        /**
         * @private
         */
        protected float _pivotY;
        /**
         * @private
         */
        protected readonly Matrix _localMatrix = new Matrix();
        /**
         * @private
         */
        internal readonly ColorTransform _colorTransform = new ColorTransform();
        /**
         * @private
         */
        internal readonly List<float> _ffdVertices = new List<float>();
        /**
         * @private
         */
        protected readonly List<object> _displayList = new List<object>();
        /**
         * @private
         */
        internal readonly List<TextureData> _textureDatas = new List<TextureData>();
        /**
         * @private
         */
        internal readonly List<DisplayData> _replacedDisplayDatas = new List<DisplayData>();
        /**
         * @private
         */
        protected readonly List<Bone> _meshBones = new List<Bone>();
        /**
         * @private
         */
        protected SkinSlotData _skinSlotData;
        /**
         * @private
         */
        protected DisplayData _displayData;
        /**
         * @private
         */
        protected DisplayData _replacedDisplayData;
        /**
         * @private
         */
        protected TextureData _textureData;
        /**
         * @private
         */
        internal MeshData _meshData;
        /**
         * @private
         */
        protected BoundingBoxData _boundingBoxData;
        /**
         * @private
         */
        protected object _rawDisplay;
        /**
         * @private
         */
        protected object _meshDisplay;
        /**
         * @private
         */
        protected object _display;
        /**
         * @private
         */
        protected Armature _childArmature;
        /**
         * @private
         */
        internal List<int> _cachedFrameIndices;
        /**
         * @private
         */
        public Slot()
        {
        }
        /**
         * @private
         */
        protected override void _onClear()
        {
            base._onClear();

            var disposeDisplayList = new List<object>();
            for (int i = 0, l = _displayList.Count; i < l; ++i)
            {
                var eachDisplay = _displayList[i];
                if (
                    eachDisplay != _rawDisplay && eachDisplay != _meshDisplay &&
                    !disposeDisplayList.Contains(eachDisplay)
                )
                {
                    disposeDisplayList.Add(eachDisplay);
                }
            }

            for (int i = 0, l = disposeDisplayList.Count; i < l; ++i)
            {
                var eachDisplay = disposeDisplayList[i];
                if (eachDisplay is Armature)
                {
                    (eachDisplay as Armature).Dispose();
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
            
            displayController = null;

            _displayDirty = false;
            _zOrderDirty = false;
            _blendModeDirty = false;
            _colorDirty = false;
            _originalDirty = false;
            _transformDirty = false;
            _meshDirty = false;
            _updateState = -1;
            _blendMode = BlendMode.Normal;
            _displayIndex = -1;
            _cachedFrameIndex = -1;
            _zOrder = -1;
            _pivotX = 0.0f;
            _pivotY = 0.0f;
            _localMatrix.Identity();
            _colorTransform.Identity();
            _ffdVertices.Clear();
            _displayList.Clear();
            _textureDatas.Clear();
            _replacedDisplayDatas.Clear();
            _meshBones.Clear();
            _skinSlotData = null;
            _displayData = null;
            _replacedDisplayData = null;
            _textureData = null;
            _meshData = null;
            _boundingBoxData = null;
            _rawDisplay = null;
            _meshDisplay = null;
            _display = null;
            _childArmature = null;
            _cachedFrameIndices = null;
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
         * @private
         */
        protected abstract void _updateZOrder();
        /**
         * @private
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
        protected abstract void _updateFrame();
        /**
         * @private
         */
        protected abstract void _updateMesh();
        /**
         * @private
         */
        protected abstract void _updateTransform(bool isSkinnedMesh);
        /**
         * @private
         */
        protected bool _isMeshBonesUpdate()
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
        protected void _updateDisplayData()
        {
            var prevDisplayData = _displayData;
            var prevReplaceDisplayData = _replacedDisplayData;
            var prevTextureData = _textureData;
            var prevMeshData = _meshData;
            var currentDisplay = _displayIndex >= 0 && _displayIndex < _displayList.Count ? _displayList[_displayIndex] : null;

            if (_displayIndex >= 0 && _displayIndex < _skinSlotData.displays.Count)
            {
                _displayData = _skinSlotData.displays[_displayIndex];
            }
            else
            {
                _displayData = null;
            }

            if (_displayIndex >= 0 && _displayIndex < _replacedDisplayDatas.Count)
            {
                _replacedDisplayData = _replacedDisplayDatas[_displayIndex];
            }
            else
            {
                _replacedDisplayData = null;
            }

            if (_displayData != prevDisplayData || _replacedDisplayData != prevReplaceDisplayData || this._display != currentDisplay)
            {
                var currentDisplayData = _replacedDisplayData != null ? _replacedDisplayData : _displayData;
                if (currentDisplayData != null && (currentDisplay == _rawDisplay || currentDisplay == _meshDisplay))
                {
                    if (_replacedDisplayData != null)
                    {
                        _textureData = _replacedDisplayData.texture;
                    }
                    else if (_displayIndex < _textureDatas.Count && _textureDatas[_displayIndex] != null)
                    {
                        _textureData = _textureDatas[_displayIndex];
                    }
                    else
                    {
                        _textureData = _displayData.texture;
                    }

                    if (currentDisplay == _meshDisplay)
                    {
                        if (_replacedDisplayData != null && _replacedDisplayData.mesh != null)
                        {
                            _meshData = _replacedDisplayData.mesh;
                        }
                        else
                        {
                            _meshData = _displayData.mesh;
                        }
                    }
                    else
                    {
                        _meshData = null;
                    }

                    // Update pivot offset.
                    if (_meshData != null)
                    {
                        _pivotX = 0.0f;
                        _pivotY = 0.0f;
                    }
                    else if (_textureData != null)
                    {
                        var scale = _armature.armatureData.scale;
                        _pivotX = currentDisplayData.pivot.x;
                        _pivotY = currentDisplayData.pivot.y;

                        if (currentDisplayData.isRelativePivot)
                        {
                            var rect = _textureData.frame != null ? _textureData.frame : _textureData.region;
                            var width = rect.width * scale;
                            var height = rect.height * scale;

                            if (_textureData.rotated)
                            {
                                width = rect.height;
                                height = rect.width;
                            }

                            _pivotX *= width;
                            _pivotY *= height;
                        }

                        if (_textureData.frame != null)
                        {
                            _pivotX += _textureData.frame.x * scale;
                            _pivotY += _textureData.frame.y * scale;
                        }
                    }
                    else
                    {
                        _pivotX = 0.0f;
                        _pivotY = 0.0f;
                    }

                    if (
                        _displayData != null && currentDisplayData != _displayData &&
                        (_meshData == null || _meshData != _displayData.mesh)
                    )
                    {
                        _displayData.transform.ToMatrix(_helpMatrix);
                        _helpMatrix.Invert();
                        _helpMatrix.TransformPoint(0.0f, 0.0f, _helpPoint);
                        _pivotX -= _helpPoint.x;
                        _pivotY -= _helpPoint.y;

                        currentDisplayData.transform.ToMatrix(_helpMatrix);
                        _helpMatrix.Invert();
                        _helpMatrix.TransformPoint(0.0f, 0.0f, _helpPoint);
                        _pivotX += _helpPoint.x;
                        _pivotY += _helpPoint.y;
                    }

                    if (_meshData != prevMeshData) // Update mesh bones and ffd vertices.
                    {
                        if (_meshData != null && _displayData != null && _meshData == _displayData.mesh)
                        {
                            if (_meshData.skinned)
                            {
                                DragonBones.ResizeList(_meshBones, _meshData.bones.Count, null);

                                for (int i = 0, l = _meshBones.Count; i < l; ++i)
                                {
                                    _meshBones[i] = _armature.GetBone(_meshData.bones[i].name);
                                }

                                int ffdVerticesCount = 0;
                                for (int i = 0, l = _meshData.boneIndices.Count; i < l; ++i)
                                {
                                    ffdVerticesCount += _meshData.boneIndices[i].Length;
                                }

                                DragonBones.ResizeList(_ffdVertices, ffdVerticesCount * 2, 0.0f);
                            }
                            else
                            {
                                _meshBones.Clear();
                                DragonBones.ResizeList(_ffdVertices, _meshData.vertices.Count, 0.0f);
                            }

                            for (int i = 0, l = _ffdVertices.Count; i < l; ++i)
                            {
                                _ffdVertices[i] = 0.0f;
                            }

                            _meshDirty = true;
                        }
                        else
                        {
                            _meshBones.Clear();
                            _ffdVertices.Clear();
                        }
                    }
                    else if (_textureData != prevTextureData)
                    {
                        _meshDirty = true;
                    }
                }
                else
                {
                    _textureData = null;
                    _meshData = null;
                    _pivotX = 0.0f;
                    _pivotY = 0.0f;
                    _meshBones.Clear();
                    _ffdVertices.Clear();
                }

                _displayDirty = true;
                _originalDirty = true;

                if (_displayData != null)
                {
                    origin = _displayData.transform;
                }
                else if (_replacedDisplayData != null)
                {
                    origin = _replacedDisplayData.transform;
                }
            }

            // Update bounding box data.
            if (_replacedDisplayData != null)
            {
                _boundingBoxData = _replacedDisplayData.boundingBox;
            }
            else if (_displayData != null)
            {
                _boundingBoxData = _displayData.boundingBox;
            }
            else
            {
                _boundingBoxData = null;
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
                    _display = _childArmature.display;
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
                _replaceDisplay(prevDisplay);

                _blendModeDirty = true;
                _colorDirty = true;
            }

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
                    prevChildArmature.clock = null;
                    prevChildArmature._parent = null; // Update child armature parent.
                    if (prevChildArmature.inheritAnimation)
                    {
                        prevChildArmature.animation.Reset();
                    }
                }

                if (_childArmature != null)
                {
                    _childArmature.clock = _armature.clock;
                    _childArmature._parent = this; // Update child armature parent.

                    // Update child armature flip.
                    _childArmature.flipX = _armature.flipX;
                    _childArmature.flipY = _armature.flipY;

                    if (_childArmature.inheritAnimation)
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
                        var actions = _skinSlotData.slot.actions.Count > 0 ? _skinSlotData.slot.actions : _childArmature.armatureData.actions;
                        if (actions.Count > 0)
                        {
                            foreach (var action in actions)
                            {
                                _childArmature._bufferAction(action);
                            }
                        }
                        else
                        {
                            _childArmature.animation.Play();
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
            if (origin != null)
            {
                global.CopyFrom(origin).Add(offset).ToMatrix(_localMatrix);
            }
            else
            {
                global.CopyFrom(offset).ToMatrix(_localMatrix);
            }

        }
        /**
         * @private
         */
        protected void _updateGlobalTransformMatrix()
        {
            globalTransformMatrix.CopyFrom(_localMatrix);
            globalTransformMatrix.Concat(_parent.globalTransformMatrix);
            global.FromMatrix(globalTransformMatrix);
        }
        /**
         * @private
         */
        internal void _init(SkinSlotData skinSlotData, object rawDisplay, object meshDisplay)
        {
            if (_skinSlotData != null)
            {
                return;
            }

            _skinSlotData = skinSlotData;

            var slotData = _skinSlotData.slot;

            name = slotData.name;

            _zOrder = slotData.zOrder;
            _blendMode = slotData.blendMode;
            _colorTransform.CopyFrom(slotData.color);
            _rawDisplay = rawDisplay;
            _meshDisplay = meshDisplay;
            DragonBones.ResizeList(_textureDatas, _skinSlotData.displays.Count, null);

            _blendModeDirty = true;
            _colorDirty = true;
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

            if (_armature != null)
            {
                _armature._removeSlotFromSlotList(this);
            }

            _armature = value;

            _onUpdateDisplay();

            if (_armature != null)
            {
                _armature._addSlotToSlotList(this);
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
        internal void _update(int cacheFrameIndex)
        {
            _updateState = -1;

            if (_displayDirty)
            {
                _displayDirty = false;
                _updateDisplay();
            }

            if (_zOrderDirty)
            {
                _zOrderDirty = false;
                _updateZOrder();
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

            if (_originalDirty)
            {
                _originalDirty = false;
                _transformDirty = true;
                _updateLocalTransformMatrix();
            }

            if (cacheFrameIndex >= 0 && _cachedFrameIndices != null)
            {
                var cachedFrameIndex = _cachedFrameIndices[cacheFrameIndex];
                if (_cachedFrameIndex >= 0 && _cachedFrameIndex == cachedFrameIndex) // Same cache.
                {
                    _transformDirty = false;
                }
                else if (cachedFrameIndex >= 0) // Has been Cached.
                {
                    _transformDirty = true;
                    _cachedFrameIndex = cachedFrameIndex;
                }
                else if (_transformDirty || _parent._transformDirty != BoneTransformDirty.None) // Dirty.
                {
                    _transformDirty = true;
                    _cachedFrameIndex = -1;
                }
                else if (_cachedFrameIndex >= 0) // Same cache but not cached yet.
                {
                    _transformDirty = false;
                    _cachedFrameIndices[cacheFrameIndex] = _cachedFrameIndex;
                }
                else // Dirty.
                {
                    _transformDirty = true;
                    _cachedFrameIndex = -1;
                }
            }
            else if (_transformDirty || _parent._transformDirty != BoneTransformDirty.None) // Dirty.
            {
                cacheFrameIndex = -1;
                _transformDirty = true;
                _cachedFrameIndex = -1;
            }

            if (_meshData != null && _displayData != null && _meshData == _displayData.mesh)
            {
                if (_meshDirty || (_meshData.skinned && _isMeshBonesUpdate()))
                {
                    _meshDirty = false;

                    _updateMesh();
                }

                if (_meshData.skinned)
                {
                    if (_transformDirty)
                    {
                        _transformDirty = false;
                        _updateTransform(true);
                    }

                    return;
                }
            }

            if (_transformDirty)
            {
                _transformDirty = false;

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

                _updateTransform(false);

                _updateState = 0;
            }
        }
        /**
         * @private
         */
        internal void _updateTransformAndMatrix()
        {
            if (_updateState < 0)
            {
                _updateState = 0;
                _updateLocalTransformMatrix();
                _updateGlobalTransformMatrix();
            }
        }
        /**
         * @private
         */
        internal bool _setDisplayList(List<object> value)
        {
            if (value != null && value.Count > 0)
            {
                if (_displayList.Count != value.Count)
                {
                    DragonBones.ResizeList(_displayList, value.Count, null);
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

            _updateDisplayData();

            return _displayDirty;
        }
        /**
         * @private
         */
        internal bool _setDisplayIndex(int value)
        {
            if (_displayIndex == value)
            {
                return false;
            }

            _displayIndex = value;
            _displayDirty = true;

            _updateDisplayData();

            return true;
        }
        /**
         * @private
         */
        internal bool _setZorder(int value)
        {
            if (_zOrder == value)
            {
                //return false;
            }

            _zOrder = value;
            _zOrderDirty = true;

            return true;
        }
        /**
         * @private
         */
        internal bool _setColor(ColorTransform value)
        {
            _colorTransform.CopyFrom(value);
            _colorDirty = true;

            return true;
        }
        /**
         * @language zh_CN
         * 判断指定的点是否在插槽的自定义包围盒内。
         * @param x 点的水平坐标。（骨架内坐标系）
         * @param y 点的垂直坐标。（骨架内坐标系）
         * @version DragonBones 5.0
         */
        public bool ContainsPoint(float x, float y)
        {
            if (_boundingBoxData == null)
            {
                return false;
            }

            _updateTransformAndMatrix();

            _helpMatrix.CopyFrom(globalTransformMatrix);
            _helpMatrix.Invert();
            _helpMatrix.TransformPoint(x, y, _helpPoint);

            return _boundingBoxData.ContainsPoint(_helpPoint.x, _helpPoint.y);
        }
        /**
         * @language zh_CN
         * 判断指定的线段与插槽的自定义包围盒是否相交。
         * @param xA 线段起点的水平坐标。（骨架内坐标系）
         * @param yA 线段起点的垂直坐标。（骨架内坐标系）
         * @param xB 线段终点的水平坐标。（骨架内坐标系）
         * @param yB 线段终点的垂直坐标。（骨架内坐标系）
         * @param intersectionPointA 线段从起点到终点与包围盒相交的第一个交点。（骨架内坐标系）
         * @param intersectionPointB 线段从终点到起点与包围盒相交的第一个交点。（骨架内坐标系）
         * @param normalRadians 碰撞点处包围盒切线的法线弧度。 [x: 第一个碰撞点处切线的法线弧度, y: 第二个碰撞点处切线的法线弧度]
         * @returns 相交的情况。 [-1: 不相交且线段在包围盒内, 0: 不相交, 1: 相交且有一个交点且终点在包围盒内, 2: 相交且有一个交点且起点在包围盒内, 3: 相交且有两个交点, N: 相交且有 N 个交点]
         * @version DragonBones 5.0
         */
        public int IntersectsSegment(
            float xA, float yA, float xB, float yB,
            Point intersectionPointA = null,
            Point intersectionPointB = null,
            Point normalRadians = null
        )
        {
            if (_boundingBoxData == null)
            {
                return 0;
            }

            _updateTransformAndMatrix();

            _helpMatrix.CopyFrom(globalTransformMatrix);
            _helpMatrix.Invert();
            _helpMatrix.TransformPoint(xA, yA, _helpPoint);
            xA = _helpPoint.x;
            yA = _helpPoint.y;
            _helpMatrix.TransformPoint(xB, yB, _helpPoint);
            xB = _helpPoint.x;
            yB = _helpPoint.y;

            var intersectionCount = _boundingBoxData.IntersectsSegment(xA, yA, xB, yB, intersectionPointA, intersectionPointB, normalRadians);
            if (intersectionCount > 0)
            {
                if (intersectionCount == 1 || intersectionCount == 2)
                {
                    if (intersectionPointA != null)
                    {
                        globalTransformMatrix.TransformPoint(intersectionPointA.x, intersectionPointA.y, intersectionPointA);
                        if (intersectionPointB != null)
                        {
                            intersectionPointB.x = intersectionPointA.x;
                            intersectionPointB.y = intersectionPointA.y;
                        }
                    }
                    else if (intersectionPointB != null)
                    {
                        globalTransformMatrix.TransformPoint(intersectionPointB.x, intersectionPointB.y, intersectionPointB);
                    }
                }
                else
                {
                    if (intersectionPointA != null)
                    {
                        globalTransformMatrix.TransformPoint(intersectionPointA.x, intersectionPointA.y, intersectionPointA);
                    }

                    if (intersectionPointB != null)
                    {
                        globalTransformMatrix.TransformPoint(intersectionPointB.x, intersectionPointB.y, intersectionPointB);
                    }
                }

                if (normalRadians != null)
                {
                    globalTransformMatrix.TransformPoint((float)Math.Cos(normalRadians.x), (float)Math.Sin(normalRadians.x), _helpPoint, true);
                    normalRadians.x = (float)Math.Atan2(_helpPoint.y, _helpPoint.x);

                    globalTransformMatrix.TransformPoint((float)Math.Cos(normalRadians.y), (float)Math.Sin(normalRadians.y), _helpPoint, true);
                    normalRadians.y = (float)Math.Atan2(_helpPoint.y, _helpPoint.x);
                }
            }

            return intersectionCount;
        }
        /**
         * @language zh_CN
         * 在下一帧更新显示对象的状态。
         * @version DragonBones 4.5
         */
        public void InvalidUpdate()
        {
            _displayDirty = true;
            _transformDirty = true;
        }
        /**
         * @private
         */
        public SkinSlotData skinSlotData
        {
            get
            {
                return _skinSlotData;
            }
        }
        /**
         * @private
         */
        public BoundingBoxData boundingBoxData
        {
            get
            {
                return _boundingBoxData;
            }
        }
        /**
         * @private
         */
        public object rawDisplay
        {
            get { return _rawDisplay; }
        }
        /**
         * @private
         */
        public object meshDisplay
        {
            get { return _meshDisplay; }
        }
        /**
         * @language zh_CN
         * 此时显示的显示对象在显示列表中的索引。
         * @version DragonBones 4.5
         */
        public int displayIndex
        {
            get { return _displayIndex; }

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
                for (int i = 0, l = backupDisplayList.Length; i < l; ++i)
                {
                    var eachDisplay = backupDisplayList[i];
                    if (
                        eachDisplay != null && eachDisplay != _rawDisplay && eachDisplay != _meshDisplay &&
                        !_displayList.Contains(eachDisplay) &&
                        !disposeDisplayList.Contains(eachDisplay)
                    )
                    {
                        disposeDisplayList.Add(eachDisplay);
                    }
                }

                for (int i = 0, l = disposeDisplayList.Count; i < l; ++i)
                {
                    var eachDisplay = disposeDisplayList[i];
                    if (eachDisplay is Armature)
                    {
                        (eachDisplay as Armature).Dispose();
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
                        DragonBones.ResizeList(replaceDisplayList, _displayIndex + 1, null);
                    }

                    replaceDisplayList[_displayIndex] = value;
                    displayList = replaceDisplayList;
                }
            }
        }
        /**
         * @language zh_CN
         * 此时显示的子骨架。
         * @see DragonBones.Armature
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

                display = value;
            }
        }
    }
}