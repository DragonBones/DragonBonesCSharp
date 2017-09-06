using System.Collections.Generic;

namespace DragonBones
{
    /**
     * 插槽，附着在骨骼上，控制显示对象的显示状态和属性。
     * 一个骨骼上可以包含多个插槽。
     * 一个插槽中可以包含多个显示对象，同一时间只能显示其中的一个显示对象，但可以在动画播放的过程中切换显示对象实现帧动画。
     * 显示对象可以是普通的图片纹理，也可以是子骨架的显示容器，网格显示对象，还可以是自定义的其他显示对象。
     * @see dragonBones.Armature
     * @see dragonBones.Bone
     * @see dragonBones.SlotData
     * @version DragonBones 3.0
     * @language zh_CN
     */
    public abstract class Slot : TransformObject
    {
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
         * @readonly
         */
        public SlotData slotData;
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
        protected bool _visibleDirty;
        /**
         * @private
         */
        protected bool _blendModeDirty;
        /**
         * @internal
         * @private
         */
        internal bool _colorDirty;
        /**
         * @internal
         * @private
         */
        internal bool _meshDirty;
        /**
         * @private
         */
        protected bool _transformDirty;
        /**
         * @private
         */
        protected bool _visible;
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
        protected int _animationDisplayIndex;
        /**
         * @internal
         * @private
         */
        internal int _zOrder;
        /**
         * @private
         */
        protected int _cachedFrameIndex;
        /**
         * @internal
         * @private
         */
        internal float _pivotX;
        /**
         * @internal
         * @private
         */
        internal float _pivotY;
        /**
         * @private
         */
        protected readonly Matrix _localMatrix = new Matrix();
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
        public readonly List<DisplayData> _displayDatas = new List<DisplayData>();
        /**
         * @private
         */
        protected readonly List<Armature> _displayList = new List<Armature>();
        /**
         * @private
         */
        protected readonly List<Bone> _meshBones = new List<Bone>();
        /**
         * @internal
         * @private
         */
        internal List<DisplayData> _rawDisplayDatas = new List<DisplayData>();
        /**
         * @private
         */
        protected DisplayData _displayData;
        /**
         * @private
         */
        protected TextureData _textureData;
        /**
         * @internal
         * @private
         */
        internal MeshDisplayData _meshData;
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
         * @internal
         * @private
         */
        internal List<int> _cachedFrameIndices = new List<int>();
        /**
         * @private
         */
        public Slot()
        {
        }
        /**
         * @private
         */
        protected override void _OnClear()
        {
            var disposeDisplayList = new List<object>();
            for (int i = 0, l = _displayList.Count; i < l; ++i)
            {
                var eachDisplay = _displayList[i];
                if ( eachDisplay != _rawDisplay && eachDisplay != _meshDisplay && !disposeDisplayList.Contains(eachDisplay))
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

            if (this._meshDisplay != null && this._meshDisplay != this._rawDisplay)
            { 
                // May be _meshDisplay and _rawDisplay is the same one.
                this._disposeDisplay(this._meshDisplay);
            }

            if (this._rawDisplay != null)
            {
                this._disposeDisplay(this._rawDisplay);
            }

            this.displayController = null;
            this.slotData = null; //

            this._displayDirty = false;
            this._zOrderDirty = false;
            this._blendModeDirty = false;
            this._colorDirty = false;
            this._meshDirty = false;
            this._transformDirty = false;
            this._visible = true;
            this._blendMode = BlendMode.Normal;
            this._displayIndex = -1;
            this._animationDisplayIndex = -1;
            this._zOrder = 0;
            this._cachedFrameIndex = -1;
            this._pivotX = 0.0f;
            this._pivotY = 0.0f;
            this._localMatrix.Identity();
            this._colorTransform.Identity();
            this._ffdVertices.Clear();
            this._displayList.Clear();
            this._displayDatas.Clear();
            this._meshBones.Clear();
            this._rawDisplayDatas = null; //
            this._displayData = null;
            this._textureData = null;
            this._meshData = null;
            this._boundingBoxData = null;
            this._rawDisplay = null;
            this._meshDisplay = null;
            this._display = null;
            this._childArmature = null;
            this._cachedFrameIndices = null;
        }

        /**
         * @private
         */
        protected abstract void _InitDisplay(object value);
        /**
         * @private
         */
        protected abstract void _DisposeDisplay(object value);
        /**
         * @private
         */
        protected abstract void _OnUpdateDisplay();
        /**
         * @private
         */
        protected abstract void _AddDisplay();
        /**
         * @private
         */
        protected abstract void _ReplaceDisplay(object value);
        /**
         * @private
         */
        protected abstract void _RemoveDisplay();
        /**
         * @private
         */
        protected abstract void _UpdateZOrder();
        /**
         * @private
         */
        public abstract void _UpdateVisible();
        /**
         * @private
         */
        protected abstract void _UpdateBlendMode();
        /**
         * @private
         */
        protected abstract void _UpdateColor();
        /**
         * @private
         */
        protected abstract void _UpdateFrame();
        /**
         * @private
         */
        protected abstract void _UpdateMesh();
        /**
         * @private
         */
        protected abstract void _UpdateTransform(bool isSkinnedMesh);

        /**
         * @private
         */
        protected void _UpdateDisplayData()
        {
            var prevDisplayData = this._displayData;
            var prevTextureData = this._textureData;
            var prevMeshData = this._meshData;
            var rawDisplayData = this._displayIndex >= 0 && this._displayIndex < this._rawDisplayDatas.Count ? this._rawDisplayDatas[this._displayIndex] : null;

            if (this._displayIndex >= 0 && this._displayIndex < this._displayDatas.Count)
            {
                this._displayData = this._displayDatas[this._displayIndex];
            }
            else
            {
                this._displayData = null;
            }

            // Update texture and mesh data.
            if (this._displayData != null)
            {
                if (this._displayData.type == DisplayType.Image || this._displayData.type == DisplayType.Mesh)
                {
                    this._textureData = (this._displayData as ImageDisplayData).texture;
                    if (this._displayData.type == DisplayType.Mesh)
                    {
                        this._meshData = this._displayData as MeshDisplayData;
                    }
                    else if (rawDisplayData != null && rawDisplayData.type == DisplayType.Mesh)
                    {
                        this._meshData = rawDisplayData as MeshDisplayData;
                    }
                    else
                    {
                        this._meshData = null;
                    }
                }
                else
                {
                    this._textureData = null;
                    this._meshData = null;
                }
            }
            else
            {
                this._textureData = null;
                this._meshData = null;
            }

            // Update bounding box data.
            if (this._displayData != null && this._displayData.type == DisplayType.BoundingBox)
            {
                this._boundingBoxData = (this._displayData as BoundingBoxDisplayData).boundingBox;
            }
            else if (rawDisplayData != null && rawDisplayData.type == DisplayType.BoundingBox)
            {
                this._boundingBoxData = (rawDisplayData as BoundingBoxDisplayData).boundingBox;
            }
            else
            {
                this._boundingBoxData = null;
            }

            if (this._displayData != prevDisplayData || this._textureData != prevTextureData || this._meshData != prevMeshData)
            {
                // Update pivot offset.
                if (this._meshData != null)
                {
                    this._pivotX = 0.0f;
                    this._pivotY = 0.0f;
                }
                else if (this._textureData != null)
                {
                    var imageDisplayData = this._displayData as ImageDisplayData;
                    var scale = this._armature.armatureData.scale;
                    var frame = this._textureData.frame;

                    this._pivotX = imageDisplayData.pivot.x;
                    this._pivotY = imageDisplayData.pivot.y;

                    var rect = frame != null ? frame : this._textureData.region;
                    var width = rect.width * scale;
                    var height = rect.height * scale;

                    if (this._textureData.rotated && frame == null)
                    {
                        width = rect.height;
                        height = rect.width;
                    }

                    this._pivotX *= width;
                    this._pivotY *= height;

                    if (frame != null)
                    {
                        this._pivotX += frame.x * scale;
                        this._pivotY += frame.y * scale;
                    }
                }
                else
                {
                    this._pivotX = 0.0f;
                    this._pivotY = 0.0f;
                }

                // Update mesh bones and ffd vertices.
                if (this._meshData != prevMeshData)
                {
                    if (this._meshData != null)// && this._meshData === this._displayData
                    {
                        if (this._meshData.weight != null)
                        {
                            this._ffdVertices.ResizeList(this._meshData.weight.count * 2);
                            this._meshBones.ResizeList(this._meshData.weight.bones.Count);

                            for (int i = 0, l = this._meshBones.Count; i < l; ++i)
                            {
                                this._meshBones[i] = this._armature.GetBone(this._meshData.weight.bones[i].name);
                            }
                        }
                        else
                        {
                            var vertexCount = this._meshData.parent.parent.intArray[this._meshData.offset + (int)BinaryOffset.MeshVertexCount];
                            this._ffdVertices.ResizeList(vertexCount * 2);
                            this._meshBones.Clear();
                        }

                        for (int i = 0, l = this._ffdVertices.Count; i < l; ++i)
                        {
                            this._ffdVertices[i] = 0.0f;
                        }

                        this._meshDirty = true;
                    }
                    else
                    {
                        this._ffdVertices.Clear();
                        this._meshBones.Clear();
                    }
                }
                else if (this._meshData != null && this._textureData != prevTextureData)
                {
                    // Update mesh after update frame.
                    this._meshDirty = true;
                }

                if (this._displayData != null && rawDisplayData != null && this._displayData != rawDisplayData && this._meshData == null)
                {
                    rawDisplayData.transform.ToMatrix(Slot._helpMatrix);
                    Slot._helpMatrix.Invert();
                    Slot._helpMatrix.TransformPoint(0.0f, 0.0f, Slot._helpPoint);
                    this._pivotX -= Slot._helpPoint.x;
                    this._pivotY -= Slot._helpPoint.y;

                    this._displayData.transform.ToMatrix(Slot._helpMatrix);
                    Slot._helpMatrix.Invert();
                    Slot._helpMatrix.TransformPoint(0.0f, 0.0f, Slot._helpPoint);
                    this._pivotX += Slot._helpPoint.x;
                    this._pivotY += Slot._helpPoint.y;
                }

                // Update original transform.
                if (rawDisplayData != null)
                {
                    this.origin = rawDisplayData.transform;
                }
                else if (this._displayData != null)
                {
                    this.origin = this._displayData.transform;
                }

                this._displayDirty = true;
                this._transformDirty = true;
            }
        }

        /**
         * @private
         */
        protected void _UpdateDisplay()
        {
            var prevDisplay = this._display != null ? this._display : this._rawDisplay;
            var prevChildArmature = this._childArmature;

            // Update display and child armature.
            if (this._displayIndex >= 0 && this._displayIndex < this._displayList.Count)
            {
                this._display = this._displayList[this._displayIndex];
                if (this._display != null && this._display is Armature)
                {
                    this._childArmature = this._display as Armature;
                    this._display = this._childArmature.display;
                }
                else
                {
                    this._childArmature = null;
                }
            }
            else
            {
                this._display = null;
                this._childArmature = null;
            }

            // Update display.
            var currentDisplay = this._display != null ? this._display : this._rawDisplay;
            if (currentDisplay != prevDisplay)
            {
                this._OnUpdateDisplay();
                this._ReplaceDisplay(prevDisplay);

                this._visibleDirty = true;
                this._blendModeDirty = true;
                this._colorDirty = true;
            }

            // Update frame.
            if (currentDisplay == this._rawDisplay || currentDisplay == this._meshDisplay)
            {
                this._UpdateFrame();
            }

            // Update child armature.
            if (this._childArmature != prevChildArmature)
            {
                if (prevChildArmature != null)
                {
                    // Update child armature parent.
                    prevChildArmature._parent = null; 
                    prevChildArmature.clock = null;
                    if (prevChildArmature.inheritAnimation)
                    {
                        prevChildArmature.animation.reset();
                    }
                }

                if (this._childArmature != null)
                {
                    // Update child armature parent.
                    this._childArmature._parent = this; 
                    this._childArmature.clock = this._armature.clock;
                    if (this._childArmature.inheritAnimation)
                    {
                        // Set child armature cache frameRate.
                        if (this._childArmature.cacheFrameRate == 0)
                        {
                            const cacheFrameRate = this._armature.cacheFrameRate;
                            if (cacheFrameRate != 0)
                            {
                                this._childArmature.cacheFrameRate = cacheFrameRate;
                            }
                        }

                        // Child armature action.
                        List<ActionData> actions = null;
                        if (this._displayData != null && this._displayData.type == DisplayType.Armature)
                        {
                            actions = (this._displayData as ArmatureDisplayData).actions;
                        }
                        else
                        {
                            var rawDisplayData = this._displayIndex >= 0 && this._displayIndex < this._rawDisplayDatas.Count ? this._rawDisplayDatas[this._displayIndex] : null;
                            if (rawDisplayData != null && rawDisplayData.type == DisplayType.Armature)
                            {
                                actions = (rawDisplayData as ArmatureDisplayData).actions;
                            }
                        }

                        if (actions != null && actions.Count > 0)
                        {
                            foreach (var action in actions)
                            {
                                this._childArmature._BufferAction(action, false); // Make sure default action at the beginning.
                            }
                        }
                        else
                        {
                            this._childArmature.animation.play();
                        }
                    }
                }
            }
        }

        /**
         * @private
         */
        protected void _UpdateGlobalTransformMatrix(bool isCache)
        {
            this.globalTransformMatrix.CopyFrom(this._localMatrix);
            this.globalTransformMatrix.Concat(this._parent.globalTransformMatrix);
            if (isCache)
            {
                this.global.fromMatrix(this.globalTransformMatrix);
            }
            else
            {
                this._globalDirty = true;
            }
        }

        /**
         * @private
         */
        protected bool _IsMeshBonesUpdate()
        {
            foreach (var bone in this._meshBones)
            {
                if (bone != null && bone._childrenTransformDirty)
                {
                    return true;
                }
            }

            return false;
        }
        /**
         * @internal
         * @private
         */
        public void  _SetArmature(Armature value = null)
        {
            if (this._armature == value)
            {
                return;
             }

            if (this._armature != null)
            {
                this._armature._removeSlotFromSlotList(this);
            }

            this._armature = value; //

            this._OnUpdateDisplay();

            if (this._armature != null)
            {
                this._armature._AddSlotToSlotList(this);
                this._AddDisplay();
            }
            else
            {
                this._RemoveDisplay();
            }
        }

        /**
         * @internal
         * @private
         */
        public bool _SetDisplayIndex(int value, bool isAnimation = false)
        {
            if (isAnimation)
            {
                if (this._animationDisplayIndex == value)
                {
                    return false;
                }

                this._animationDisplayIndex = value;
            }

            if (this._displayIndex == value)
            {
                return false;
            }

            this._displayIndex = value;
            this._displayDirty = true;

            this._UpdateDisplayData();

            return this._displayDirty;
        }

        /**
         * @internal
         * @private
         */
        public bool _SetZorder(int value)
        {
            if (this._zOrder == value)
            {
                //return false;
            }

            this._zOrder = value;
            this._zOrderDirty = true;

            return this._zOrderDirty;
        }

        /**
         * @internal
         * @private
         */
        public bool _SetColor(ColorTransform value)
        {
            this._colorTransform.CopyFrom(value);
            this._colorDirty = true;

            return this._colorDirty;
        }
}
}
