/**
 * The MIT License (MIT)
 *
 * Copyright (c) 2012-2017 DragonBones team and other contributors
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of
 * this software and associated documentation files (the "Software"), to deal in
 * the Software without restriction, including without limitation the rights to
 * use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
 * the Software, and to permit persons to whom the Software is furnished to do so,
 * subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
 * FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
 * COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
 * IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
 * CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
using UnityEngine;
using System.Collections.Generic;

namespace DragonBones
{
    /**
     * @language zh_CN
     * Unity 插槽。
     * @version DragonBones 3.0
     */
    public class UnityNewSlot : Slot
    {
        private static readonly int[] TRIANGLES = { 0, 1, 2, 0, 2, 3 };
        private static readonly Vector2[] _helpVector2s = { new Vector2(), new Vector2(), new Vector2(), new Vector2() };
        private static Vector3 _helpVector3 = new Vector3();

        private bool _skewed;
        private UnityArmatureComponent _proxy;
        internal GameObject _renderDisplay;

        internal MeshBuffer _meshBuffer;

        private Vector3[] _normals;
        private Vector3 _normalVal = Vector3.zero;
        internal MeshRenderer _renderer = null;
        internal MeshFilter _meshFilter = null;
        private UnityUGUIDisplay _uiDisplay = null;

        private BlendMode _currentBlendMode;

        //combineMesh
        internal bool _isIgnoreCombineMesh;
        internal bool _isCombineMesh;
        internal int _sumMeshIndex = -1;
        internal int _verticeIndex = -1;
        internal int _verticeOffset = -1;
        internal CombineMeshs _combineMesh = null;

        private bool _isActive = false;

        internal float _worldZ;

        /**
         * @private
         */
        public UnityNewSlot()
        {
        }

        /**
         * @private
         */
        protected override void _OnClear()
        {
            base._OnClear();

            if (this._meshBuffer != null)
            {
                this._meshBuffer.Dispose();
            }

            this._skewed = false;
            this._proxy = null;
            this._renderDisplay = null;

            this._meshBuffer = null;
            this._normals = null;

            this._currentBlendMode = BlendMode.Normal;

            this._isIgnoreCombineMesh = false;
            this._isCombineMesh = false;
            this._sumMeshIndex = -1;
            this._verticeIndex = -1;
            this._verticeOffset = -1;

            this._combineMesh = null;

            this._isActive = false;

            this._worldZ = 0.0f;
        }

        /**
         * @private
         */
        protected override void _InitDisplay(object value, bool isRetain)
        {

        }
        /**
         * @private
         */
        protected override void _DisposeDisplay(object value, bool isRelease)
        {
            if (!isRelease)
            {
                UnityFactoryHelper.DestroyUnityObject(value as GameObject);
            }
        }
        /**
         * @private
         */
        protected override void _OnUpdateDisplay()
        {
            _renderDisplay = (_display != null ? _display : _rawDisplay) as GameObject;

            //
            _proxy = _armature.proxy as UnityArmatureComponent;
            if (_proxy.isUGUI)
            {
                _uiDisplay = _renderDisplay.GetComponent<UnityUGUIDisplay>();
                if (_uiDisplay == null)
                {
                    _uiDisplay = _renderDisplay.AddComponent<UnityUGUIDisplay>();
                    _uiDisplay.raycastTarget = false;
                }
            }
            else
            {
                _renderer = _renderDisplay.GetComponent<MeshRenderer>();
                if (_renderer == null)
                {
                    _renderer = _renderDisplay.AddComponent<MeshRenderer>();
                }
                //
                _meshFilter = _renderDisplay.GetComponent<MeshFilter>();
                if (_meshFilter == null && _renderDisplay.GetComponent<TextMesh>() == null)
                {
                    _meshFilter = _renderDisplay.AddComponent<MeshFilter>();
                }
            }

            //init mesh
            if (this._meshBuffer == null)
            {
                this._meshBuffer = new MeshBuffer();
                this._meshBuffer.sharedMesh = MeshBuffer.GenerateMesh();
                this._meshBuffer.sharedMesh.name = this.name;
            }
        }
        /**
         * @private
         */
        protected override void _AddDisplay()
        {
            _proxy = _armature.proxy as UnityArmatureComponent;
            var container = _proxy;
            if (_renderDisplay.transform.parent != container.transform)
            {
                _renderDisplay.transform.SetParent(container.transform);

                _helpVector3.Set(0.0f, 0.0f, 0.0f);
                _SetZorder(_helpVector3);
            }
        }
        /**
         * @private
         */
        protected override void _ReplaceDisplay(object value)
        {
            var container = _proxy;
            var prevDisplay = value as GameObject;
            int index = prevDisplay.transform.GetSiblingIndex();
            prevDisplay.SetActive(false);

            _renderDisplay.hideFlags = HideFlags.None;
            _renderDisplay.transform.SetParent(container.transform);
            _renderDisplay.SetActive(true);
            _renderDisplay.transform.SetSiblingIndex(index);

            _SetZorder(prevDisplay.transform.localPosition);
        }
        /**
         * @private
         */
        protected override void _RemoveDisplay()
        {
            _renderDisplay.transform.parent = null;
        }
        /**
         * @private
         */
        protected override void _UpdateZOrder()
        {
            // _helpVector3.Set(_renderDisplay.transform.localPosition.x, _renderDisplay.transform.localPosition.y, -_zOrder * (_proxy._zSpace + 0.001f));
            _SetZorder(this._renderDisplay.transform.localPosition);
        }

        /**
         * @internal
         */
        internal void _SetZorder(Vector3 zorderPos)
        {
            this._worldZ = -this._zOrder * (this._proxy._zSpace + 0.001f);

            if (this._isCombineMesh)
            {
                var meshBuffer = this._combineMesh.meshBuffers[this._sumMeshIndex];
                for (var i = 0; i < this._meshBuffer.vertexBuffers.Length; i++)
                {
                    this._meshBuffer.vertexBuffers[i].z = this._worldZ;
                    meshBuffer.vertexBuffers[this._verticeOffset + i].z = this._worldZ;
                }
            }

            //else
            {
                zorderPos.z = this._worldZ;

                if (_renderDisplay != null)
                {
                    _renderDisplay.transform.localPosition = zorderPos;
                    _renderDisplay.transform.SetSiblingIndex(_zOrder);

                    if (_proxy.isUGUI)
                    {
                        return;
                    }

                    if (_childArmature == null)
                    {
                        _renderer.sortingLayerName = _proxy.sortingLayerName;
                        if (_proxy.sortingMode == SortingMode.SortByOrder)
                        {
                            _renderer.sortingOrder = _zOrder * UnityArmatureComponent.ORDER_SPACE;
                        }
                        else
                        {
                            _renderer.sortingOrder = _proxy.sortingOrder;
                        }
                    }
                    else
                    {
                        var childArmatureComp = childArmature.proxy as UnityArmatureComponent;
                        childArmatureComp._sortingMode = _proxy._sortingMode;
                        childArmatureComp._sortingLayerName = _proxy._sortingLayerName;
                        if (_proxy._sortingMode == SortingMode.SortByOrder)
                        {
                            childArmatureComp._sortingOrder = _zOrder * UnityArmatureComponent.ORDER_SPACE; ;
                        }
                        else
                        {
                            childArmatureComp._sortingOrder = _proxy._sortingOrder;
                        }
                    }
                }
            }
        }

        internal void CancelCombineMesh()
        {
            if (this._isCombineMesh)
            {


                this._meshBuffer.enabled = true;
                if (this._meshFilter != null)
                {
                    this._meshFilter.sharedMesh = this._meshBuffer.sharedMesh;
                    this._meshBuffer.UpdateMesh();
                }

                this._meshDirty = true;
                this._transformDirty = true;
            }

            if (this._renderDisplay != null)
            {
                this._renderDisplay.SetActive(this._isActive);
                _helpVector3.Set(this._renderDisplay.transform.localPosition.x, this._renderDisplay.transform.localPosition.y, this._worldZ);
                this._renderDisplay.transform.localPosition = _helpVector3;
                // this._SetZorder(this._renderDisplay.transform.localPosition);
            }

            //
            this._isCombineMesh = false;
            this._sumMeshIndex = -1;
            this._verticeOffset = -1;
            // this._combineMesh = null;
        }

        //
        private void _CombineMesh(bool enabled)
        {
            //引起合并的条件,Display改变，混合模式改变，Visible改变，Zorder改变
            //已经关闭合并，不再考虑
            if (this._isIgnoreCombineMesh || this._proxy.isUGUI)
            {
                return;
            }

            var combineMeshComp = this._proxy.GetComponent<CombineMeshs>();

            var isNeedCombineMesh = true;
            //已经合并过了，又触发合并，那么打断合并，用自己的网格数据还原
            if (this._isCombineMesh)
            {
                //已经合并过，除非满足一下情况，否则都不能再合并
                //1.材质没有变化，只是uv,vertex有改变（visible改变除外）
                var meshBuffer = this._combineMesh.meshBuffers[this._sumMeshIndex];
                if (meshBuffer != null && meshBuffer.name == this._meshBuffer.name)
                {
                    //材质没有变化,
                }
                //一下都会引起打断
                //如果只是UV改变，材质没有变化,那么不重新合并，但要把uv和vertex数据更新下
                // this._meshFilter.sharedMesh = this._meshBuffer.sharedMesh;
                // this._meshBuffer.UpdateMesh();
                this.CancelCombineMesh();
                UnityEngine.Debug.Log(this.name + "已被忽略");
                // this.CancelCombineMesh();

                this._isIgnoreCombineMesh = true;
                this._isCombineMesh = false;
                this._sumMeshIndex = -1;
                this._verticeOffset = -1;
            }

            //从来没有合并过，触发合并，那么尝试合并
            this._isCombineMesh = false;
            if (combineMeshComp != null)
            {
                // UnityEngine.Debug.Log(this.name + "引起合并");
                // combineMeshComp.BeginCombineMesh();
                combineMeshComp._dirty = true;
            }
        }

        /**
         * @private
         */
        internal override void _UpdateVisible()
        {
            _renderDisplay.SetActive(_parent.visible);

            if (this._isCombineMesh && !this._parent.visible)
            {
                this._CombineMesh(false);
            }
        }
        /**
         * @private
         */
        internal override void _UpdateBlendMode()
        {
            if (this._currentBlendMode == this._blendMode)
            {
                return;
            }

            if (this._childArmature == null)
            {
                if (this._uiDisplay != null)
                {
                    this._uiDisplay.material = (this._textureData as UnityTextureData).GetMaterial(this._blendMode, true);
                }
                else
                {
                    this._renderer.sharedMaterial = (this._textureData as UnityTextureData).GetMaterial(this._blendMode);
                }

                this._meshBuffer.name = this._uiDisplay != null ? this._uiDisplay.material.name : this._renderer.sharedMaterial.name;
            }
            else
            {
                foreach (var slot in _childArmature.GetSlots())
                {
                    slot._blendMode = _blendMode;
                    slot._UpdateBlendMode();
                }
            }

            this._currentBlendMode = this._blendMode;
            // UnityEngine.Debug.Log("_UpdateBlendMode");
            this._CombineMesh(true);
        }
        /**
         * @private
         */
        protected override void _UpdateColor()
        {
            if (this._childArmature == null)
            {
                var proxyTrans = _proxy._colorTransform;
                if (this._isCombineMesh)
                {
                    var meshBuffer = this._combineMesh.meshBuffers[this._sumMeshIndex];
                    for (var i = 0; i < this._meshBuffer.vertexBuffers.Length; i++)
                    {
                        var index = this._verticeOffset + i;
                        this._meshBuffer.color32Buffers[i].r = (byte)(_colorTransform.redMultiplier * proxyTrans.redMultiplier * 255);
                        this._meshBuffer.color32Buffers[i].g = (byte)(_colorTransform.greenMultiplier * proxyTrans.greenMultiplier * 255);
                        this._meshBuffer.color32Buffers[i].b = (byte)(_colorTransform.blueMultiplier * proxyTrans.blueMultiplier * 255);
                        this._meshBuffer.color32Buffers[i].a = (byte)(_colorTransform.alphaMultiplier * proxyTrans.alphaMultiplier * 255);
                        meshBuffer.color32Buffers[index] = this._meshBuffer.color32Buffers[i];
                    }

                    meshBuffer.meshDirty = true;
                    // this._combineMesh._dirty = true;
                }
                else if (this._meshBuffer.sharedMesh != null)
                {
                    for (int i = 0, l = this._meshBuffer.sharedMesh.vertexCount; i < l; ++i)
                    {
                        this._meshBuffer.color32Buffers[i].r = (byte)(_colorTransform.redMultiplier * proxyTrans.redMultiplier * 255);
                        this._meshBuffer.color32Buffers[i].g = (byte)(_colorTransform.greenMultiplier * proxyTrans.greenMultiplier * 255);
                        this._meshBuffer.color32Buffers[i].b = (byte)(_colorTransform.blueMultiplier * proxyTrans.blueMultiplier * 255);
                        this._meshBuffer.color32Buffers[i].a = (byte)(_colorTransform.alphaMultiplier * proxyTrans.alphaMultiplier * 255);
                    }
                    //
                    this._meshBuffer.sharedMesh.colors32 = this._meshBuffer.color32Buffers;
                }
            }
            else
            {
                //set all childArmature color dirty
                (this._childArmature.proxy as UnityArmatureComponent).color = _colorTransform;
            }

        }
        /**
         * @private
         */
        protected override void _UpdateFrame()
        {
            var meshData = this._display == this._meshDisplay ? this._meshData : null;
            var currentTextureData = this._textureData as UnityTextureData;

            this._isActive = this._displayIndex >= 0 && this._display != null && currentTextureData != null;
            if (this._isActive)
            {
                var currentTextureAtlas = _proxy.isUGUI ? currentTextureAtlasData.uiTexture : currentTextureAtlasData.texture;
                if (currentTextureAtlas != null)
                {
                    var textureAtlasWidth = currentTextureAtlasData.width > 0.0f ? (int)currentTextureAtlasData.width : currentTextureAtlas.mainTexture.width;
                    var textureAtlasHeight = currentTextureAtlasData.height > 0.0f ? (int)currentTextureAtlasData.height : currentTextureAtlas.mainTexture.height;

                    this._meshBuffer.Clear();

                    var textureScale = _armature.armatureData.scale * currentTextureData.parent.scale;
                    var sourceX = currentTextureData.region.x;
                    var sourceY = currentTextureData.region.y;
                    var sourceWidth = currentTextureData.region.width;
                    var sourceHeight = currentTextureData.region.height;

                    if (meshData != null)
                    {
                        var data = meshData.parent.parent.parent;
                        var intArray = data.intArray;
                        var floatArray = data.floatArray;
                        var vertexCount = intArray[meshData.offset + (int)BinaryOffset.MeshVertexCount];
                        var triangleCount = intArray[meshData.offset + (int)BinaryOffset.MeshTriangleCount];
                        int vertexOffset = intArray[meshData.offset + (int)BinaryOffset.MeshFloatOffset];
                        if (vertexOffset < 0)
                        {
                            vertexOffset += 65536; // Fixed out of bouds bug. 
                        }

                        var uvOffset = vertexOffset + vertexCount * 2;
                        if (this._meshBuffer.uvBuffers == null || this._meshBuffer.uvBuffers.Length != vertexCount)
                        {
                            this._meshBuffer.uvBuffers = new Vector2[vertexCount];
                        }

                        if (this._meshBuffer.rawVertextBuffers == null || this._meshBuffer.rawVertextBuffers.Length != vertexCount)
                        {
                            this._meshBuffer.rawVertextBuffers = new Vector3[vertexCount];
                            this._meshBuffer.vertexBuffers = new Vector3[vertexCount];
                        }

                        this._meshBuffer.triangleBuffers = new int[triangleCount * 3];

                        for (int i = 0, iV = vertexOffset, iU = uvOffset, l = vertexCount; i < l; ++i)
                        {
                            this._meshBuffer.uvBuffers[i].x = (sourceX + floatArray[iU++] * sourceWidth) / textureAtlasWidth;
                            this._meshBuffer.uvBuffers[i].y = 1.0f - (sourceY + floatArray[iU++] * sourceHeight) / textureAtlasHeight;

                            this._meshBuffer.rawVertextBuffers[i].x = floatArray[iV++] * textureScale;
                            this._meshBuffer.rawVertextBuffers[i].y = floatArray[iV++] * textureScale;

                            this._meshBuffer.vertexBuffers[i].x = this._meshBuffer.rawVertextBuffers[i].x;
                            this._meshBuffer.vertexBuffers[i].y = this._meshBuffer.rawVertextBuffers[i].y;
                        }

                        for (int i = 0; i < triangleCount * 3; ++i)
                        {
                            this._meshBuffer.triangleBuffers[i] = intArray[meshData.offset + (int)BinaryOffset.MeshVertexIndices + i];
                        }

                        this._meshBuffer.InitMesh();
                    }
                    else
                    {
                        if (this._meshBuffer.rawVertextBuffers == null || this._meshBuffer.rawVertextBuffers.Length != 4)
                        {
                            this._meshBuffer.rawVertextBuffers = new Vector3[4];
                            this._meshBuffer.vertexBuffers = new Vector3[4];
                        }

                        if (this._meshBuffer.uvBuffers == null || this._meshBuffer.uvBuffers.Length != this._meshBuffer.rawVertextBuffers.Length)
                        {
                            this._meshBuffer.uvBuffers = new Vector2[this._meshBuffer.rawVertextBuffers.Length];
                        }

                        // Normal texture.                        
                        for (int i = 0, l = 4; i < l; ++i)
                        {
                            var u = 0.0f;
                            var v = 0.0f;

                            switch (i)
                            {
                                case 0:
                                    break;

                                case 1:
                                    u = 1.0f;
                                    break;

                                case 2:
                                    u = 1.0f;
                                    v = 1.0f;
                                    break;

                                case 3:
                                    v = 1.0f;
                                    break;

                                default:
                                    break;
                            }

                            var scaleWidth = sourceWidth * textureScale;
                            var scaleHeight = sourceHeight * textureScale;
                            var pivotX = _pivotX;
                            var pivotY = _pivotY;

                            if (currentTextureData.rotated)
                            {
                                var temp = scaleWidth;
                                scaleWidth = scaleHeight;
                                scaleHeight = temp;

                                pivotX = scaleWidth - _pivotX;
                                pivotY = scaleHeight - _pivotY;

                                //uv
                                this._meshBuffer.uvBuffers[i].x = (sourceX + (1.0f - v) * sourceWidth) / textureAtlasWidth;
                                this._meshBuffer.uvBuffers[i].y = 1.0f - (sourceY + u * sourceHeight) / textureAtlasHeight;
                            }
                            else
                            {
                                //uv
                                this._meshBuffer.uvBuffers[i].x = (sourceX + u * sourceWidth) / textureAtlasWidth;
                                this._meshBuffer.uvBuffers[i].y = 1.0f - (sourceY + v * sourceHeight) / textureAtlasHeight;
                            }

                            //vertices
                            this._meshBuffer.rawVertextBuffers[i].x = (u * scaleWidth) - pivotX;
                            this._meshBuffer.rawVertextBuffers[i].y = (v) * scaleHeight - pivotY;

                            this._meshBuffer.vertexBuffers[i].x = this._meshBuffer.rawVertextBuffers[i].x;
                            this._meshBuffer.vertexBuffers[i].y = this._meshBuffer.rawVertextBuffers[i].y;
                        }

                        this._meshBuffer.triangleBuffers = TRIANGLES;

                        this._meshBuffer.InitMesh();
                    }

                    if (_proxy.isUGUI)
                    {
                        this._uiDisplay.material = currentTextureAtlas;
                        this._uiDisplay.texture = currentTextureAtlas.mainTexture;
                        this._meshBuffer.UpdateMesh();
                        this._uiDisplay.sharedMesh = this._meshBuffer.sharedMesh;
                    }
                    else
                    {
                        if (this._renderer.enabled)
                        {
                            this._meshBuffer.UpdateMesh();
                        }

                        this._meshFilter.sharedMesh = this._meshBuffer.sharedMesh;
                        this._renderer.sharedMaterial = currentTextureAtlas;
                    }

                    this._meshBuffer.name = currentTextureAtlas.name;
                    this._currentBlendMode = BlendMode.Normal;
                    this._blendModeDirty = true;
                    this._colorDirty = true;// Relpace texture will override blendMode and color.
                    this._visibleDirty = true;

                    // UnityEngine.Debug.Log("_UpdateFrame");
                    this._CombineMesh(true);
                    return;
                }
            }

            _renderDisplay.SetActive(false);
            if (_proxy.isUGUI)
            {
                _uiDisplay.material = null;
                _uiDisplay.texture = null;
                _uiDisplay.sharedMesh = null;
            }
            else
            {
                _meshFilter.sharedMesh = null;
                _renderer.sharedMaterial = null;
            }

            _helpVector3.x = 0.0f;
            _helpVector3.y = 0.0f;
            _helpVector3.z = _renderDisplay.transform.localPosition.z;

            _renderDisplay.transform.localPosition = _helpVector3;

            // UnityEngine.Debug.Log(this.name + "隐藏");

            this._CombineMesh(false);
        }

        protected override void _UpdateMesh()
        {
            if (this._meshBuffer.sharedMesh == null)
            {
                return;
            }

            var hasFFD = this._ffdVertices.Count > 0;
            var scale = _armature.armatureData.scale;
            var meshData = this._meshData;
            var weightData = meshData.weight;

            var data = meshData.parent.parent.parent;
            var intArray = data.intArray;
            var floatArray = data.floatArray;
            var vertextCount = intArray[meshData.offset + (int)BinaryOffset.MeshVertexCount];

            if (weightData != null)
            {
                int weightFloatOffset = intArray[weightData.offset + 1/*(int)BinaryOffset.MeshWeightOffset*/];
                if (weightFloatOffset < 0)
                {
                    weightFloatOffset += 65536; // Fixed out of bouds bug. 
                }

                MeshBuffer meshBuffer = null;
                if (this._isCombineMesh)
                {
                    meshBuffer = this._combineMesh.meshBuffers[this._sumMeshIndex];
                }
                var vz = -this._verticeIndex * (this._proxy._zSpace + 0.001f);
                int iB = weightData.offset + (int)BinaryOffset.WeigthBoneIndices + weightData.bones.Count, iV = weightFloatOffset, iF = 0;
                for (int i = 0; i < vertextCount; ++i)
                {
                    var boneCount = intArray[iB++];
                    float xG = 0.0f, yG = 0.0f;
                    for (var j = 0; j < boneCount; ++j)
                    {
                        var boneIndex = intArray[iB++];
                        var bone = this._meshBones[boneIndex];
                        if (bone != null)
                        {
                            var matrix = bone.globalTransformMatrix;
                            var weight = floatArray[iV++];
                            var xL = floatArray[iV++] * scale;
                            var yL = floatArray[iV++] * scale;

                            if (hasFFD)
                            {
                                xL += this._ffdVertices[iF++];
                                yL += this._ffdVertices[iF++];
                            }

                            xG += (matrix.a * xL + matrix.c * yL + matrix.tx) * weight;
                            yG += (matrix.b * xL + matrix.d * yL + matrix.ty) * weight;
                        }
                    }

                    this._meshBuffer.vertexBuffers[i].x = xG;
                    this._meshBuffer.vertexBuffers[i].y = yG;
                    // this._meshBuffer.vertexBuffers[i].z = this._worldZ;

                    if (meshBuffer != null)
                    {
                        meshBuffer.vertexBuffers[i + this._verticeOffset].x = xG;
                        meshBuffer.vertexBuffers[i + this._verticeOffset].y = yG;
                        meshBuffer.vertexBuffers[i + this._verticeOffset].z = vz;
                    }
                }

                if (meshBuffer != null)
                {
                    meshBuffer.meshDirty = true;
                }
                else
                {
                    if (_renderer && _renderer.enabled)
                    {
                        this._meshBuffer.UpdateMesh();
                    }
                }
            }
        }

        protected override void _UpdateTransform(bool isSkinnedMesh)
        {
            if (this._isCombineMesh)
            {
                if (this._ffdVertices.Count > 0)
                {
                    var scale = _armature.armatureData.scale;
                    var meshData = this._meshData;
                    var data = meshData.parent.parent.parent;
                    var vertextCount = data.intArray[meshData.offset + (int)BinaryOffset.MeshVertexCount];
                    int vertexOffset = data.intArray[meshData.offset + (int)BinaryOffset.MeshFloatOffset];
                    if (vertexOffset < 0)
                    {
                        vertexOffset += 65536; // Fixed out of bouds bug. 
                    }
                    //
                    var a = globalTransformMatrix.a;
                    var b = globalTransformMatrix.b;
                    var c = globalTransformMatrix.c;
                    var d = globalTransformMatrix.d;
                    var tx = globalTransformMatrix.tx;
                    var ty = globalTransformMatrix.ty;

                    var vx = 0.0f;
                    var vy = 0.0f;
                    var vz = -this._verticeIndex * (this._proxy._zSpace + 0.001f);
                    //this._zOrder * (this._proxy._zSpace + 0.01f);
                    var meshBuffer = this._combineMesh.meshBuffers[this._sumMeshIndex];
                    for (int i = 0, iV = 0, iF = 0, l = vertextCount; i < l; ++i)
                    {
                        vx = (data.floatArray[vertexOffset + (iV++)] * scale + this._ffdVertices[iF++]);
                        vy = (data.floatArray[vertexOffset + (iV++)] * scale + this._ffdVertices[iF++]);
                        this._meshBuffer.vertexBuffers[i].x = (vx * a + vy * c + tx);
                        this._meshBuffer.vertexBuffers[i].y = (vx * b + vy * d + ty);
                        // this._meshBuffer.vertexBuffers[i].z = this._worldZ;

                        var index = i + this._verticeOffset;
                        meshBuffer.vertexBuffers[index].x = this._meshBuffer.vertexBuffers[i].x;
                        meshBuffer.vertexBuffers[index].y = this._meshBuffer.vertexBuffers[i].y;
                        meshBuffer.vertexBuffers[index].z = vz;
                    }

                    meshBuffer.meshDirty = true;
                }
                else
                {
                    var a = globalTransformMatrix.a;
                    var b = globalTransformMatrix.b;
                    var c = globalTransformMatrix.c;
                    var d = globalTransformMatrix.d;
                    var tx = globalTransformMatrix.tx;
                    var ty = globalTransformMatrix.ty;

                    //
                    // UnityEngine.Debug.Log(this.name + "index:" + this._verticeIndex);

                    var meshBuffer = this._combineMesh.meshBuffers[this._sumMeshIndex];
                    var vx = 0.0f;
                    var vy = 0.0f;
                    // var vz = this._verticeIndex * (this._proxy._zSpace + 0.001f);
                    var vz = -this._verticeIndex * (this._proxy._zSpace + 0.001f);
                    for (int i = 0, l = this._meshBuffer.vertexBuffers.Length; i < l; i++)
                    {
                        //vertices
                        vx = this._meshBuffer.rawVertextBuffers[i].x;
                        vy = this._meshBuffer.rawVertextBuffers[i].y;

                        this._meshBuffer.vertexBuffers[i].x = vx * a + vy * c + tx;
                        this._meshBuffer.vertexBuffers[i].y = vx * b + vy * d + ty;
                        // this._meshBuffer.vertexBuffers[i].z = vz;

                        var index = i + this._verticeOffset;
                        meshBuffer.vertexBuffers[index].x = this._meshBuffer.vertexBuffers[i].x;
                        meshBuffer.vertexBuffers[index].y = this._meshBuffer.vertexBuffers[i].y;
                        meshBuffer.vertexBuffers[index].z = vz;
                    }
                    meshBuffer.meshDirty = true;
                }
            }
            else
            {
                if (isSkinnedMesh)
                {
                    var transform = _renderDisplay.transform;

                    transform.localPosition = new Vector3(0.0f, 0.0f, transform.localPosition.z);
                    transform.localEulerAngles = Vector3.zero;
                    transform.localScale = Vector3.one;
                }
                else if (this._ffdVertices.Count > 0)
                {
                    var scale = _armature.armatureData.scale;
                    var meshData = this._meshData;
                    var data = meshData.parent.parent.parent;
                    var vertextCount = data.intArray[meshData.offset + (int)BinaryOffset.MeshVertexCount];
                    int vertexOffset = data.intArray[meshData.offset + (int)BinaryOffset.MeshFloatOffset];
                    if (vertexOffset < 0)
                    {
                        vertexOffset += 65536; // Fixed out of bouds bug. 
                    }
                    //
                    var a = globalTransformMatrix.a;
                    var b = globalTransformMatrix.b;
                    var c = globalTransformMatrix.c;
                    var d = globalTransformMatrix.d;
                    var tx = globalTransformMatrix.tx;
                    var ty = globalTransformMatrix.ty;

                    var vx = 0.0f;
                    var vy = 0.0f;
                    for (int i = 0, iV = 0, iF = 0, l = vertextCount; i < l; ++i)
                    {
                        vx = (data.floatArray[vertexOffset + (iV++)] * scale + this._ffdVertices[iF++]);
                        vy = (data.floatArray[vertexOffset + (iV++)] * scale + this._ffdVertices[iF++]);

                        this._meshBuffer.vertexBuffers[i].x = (vx * a + vy * c + tx);
                        this._meshBuffer.vertexBuffers[i].y = (vx * b + vy * d + ty);
                        // this._meshBuffer.vertexBuffers[i].z = this._worldZ;
                    }

                    if (this._renderer && this._renderer.enabled)
                    {
                        this._meshBuffer.UpdateMesh();
                    }
                }
                else if (this._meshBuffer.vertexBuffers != null)
                {
                    var a = globalTransformMatrix.a;
                    var b = globalTransformMatrix.b;
                    var c = globalTransformMatrix.c;
                    var d = globalTransformMatrix.d;
                    var tx = globalTransformMatrix.tx;
                    var ty = globalTransformMatrix.ty;

                    // Normal texture.  
                    var vx = 0.0f;
                    var vy = 0.0f;
                    for (int i = 0, l = this._meshBuffer.vertexBuffers.Length; i < l; ++i)
                    {
                        //vertices
                        vx = this._meshBuffer.rawVertextBuffers[i].x;
                        vy = this._meshBuffer.rawVertextBuffers[i].y;

                        this._meshBuffer.vertexBuffers[i].x = (vx * a + vy * c + tx);
                        this._meshBuffer.vertexBuffers[i].y = (vx * b + vy * d + ty);
                        // this._meshBuffer.vertexBuffers[i].z = this._worldZ;
                    }

                    if (this._renderer && this._renderer.enabled)
                    {
                        this._meshBuffer.UpdateMesh();
                    }
                }
                else
                {
                    this.UpdateGlobalTransform(); // Update transform.

                    //localPosition
                    var flipX = _armature.flipX;
                    var flipY = _armature.flipY;
                    var transform = _renderDisplay.transform;

                    _helpVector3.x = global.x;
                    _helpVector3.y = global.y;
                    _helpVector3.z = transform.localPosition.z;

                    transform.localPosition = _helpVector3;

                    //localEulerAngles
                    if (_childArmature == null)
                    {
                        _helpVector3.x = flipY ? 180.0f : 0.0f;
                        _helpVector3.y = flipX ? 180.0f : 0.0f;
                        _helpVector3.z = global.rotation * Transform.RAD_DEG;
                    }
                    else
                    {
                        //If the childArmature is not null,
                        //X, Y axis can not flip in the container of the childArmature container,
                        //because after the flip, the Z value of the child slot is reversed,
                        //showing the order is wrong, only in the child slot to deal with X, Y axis flip 
                        _helpVector3.x = 0.0f;
                        _helpVector3.y = 0.0f;
                        _helpVector3.z = global.rotation * Transform.RAD_DEG;

                        //这里这样处理，是因为子骨架的插槽也要处理z值,那就在容器中反一下，子插槽再正过来
                        if (flipX != flipY)
                        {
                            _helpVector3.z = -_helpVector3.z;
                        }
                    }

                    if (flipX || flipY)
                    {
                        if (flipX && flipY)
                        {
                            _helpVector3.z += 180.0f;
                        }
                        else
                        {
                            if (flipX)
                            {
                                _helpVector3.z = 180.0f - _helpVector3.z;
                            }
                            else
                            {
                                _helpVector3.z = -_helpVector3.z;
                            }
                        }
                    }

                    transform.localEulerAngles = _helpVector3;

                    //Modify mesh skew. // TODO child armature skew.
                    // if ((_display == _rawDisplay || _display == _meshDisplay) && _mesh != null)
                    if ((_display == _rawDisplay || _display == _meshDisplay) && this._meshBuffer.sharedMesh != null)
                    {
                        var skew = global.skew;
                        var dSkew = skew;
                        if (flipX && flipY)
                        {
                            dSkew = -skew + Transform.PI;
                        }
                        else if (!flipX && !flipY)
                        {
                            dSkew = -skew - Transform.PI;
                        }

                        var skewed = dSkew < -0.01f || 0.01f < dSkew;
                        if (_skewed || skewed)
                        {
                            _skewed = skewed;

                            var isPositive = global.scaleX >= 0.0f;
                            var cos = Mathf.Cos(dSkew);
                            var sin = Mathf.Sin(dSkew);

                            for (int i = 0, l = this._meshBuffer.vertexBuffers.Length; i < l; ++i)
                            {
                                var x = this._meshBuffer.rawVertextBuffers[i].x;
                                var y = this._meshBuffer.rawVertextBuffers[i].y;

                                if (isPositive)
                                {
                                    this._meshBuffer.vertexBuffers[i].x = x + y * sin;
                                }
                                else
                                {
                                    this._meshBuffer.vertexBuffers[i].x = -x + y * sin;
                                }

                                this._meshBuffer.vertexBuffers[i].y = y * cos;
                            }

                            if (_renderer && _renderer.enabled)
                            {
                                this._meshBuffer.UpdateMesh();
                            }
                        }
                    }

                    //localScale
                    _helpVector3.x = global.scaleX;
                    _helpVector3.y = global.scaleY;
                    _helpVector3.z = 1.0f;

                    transform.localScale = _helpVector3;
                }
            }

            if (_childArmature != null)
            {
                UnityArmatureComponent unityArmature = (_childArmature.proxy as UnityArmatureComponent);
                _childArmature.flipX = _armature.flipX;
                _childArmature.flipY = _armature.flipY;

                unityArmature.addNormal = _proxy.addNormal;
                unityArmature.boneHierarchy = _proxy.boneHierarchy;
            }

            // UpdateNormal();
        }

        public void UpdateNormal()
        {
            var mesh = this._meshBuffer.sharedMesh;
            if (mesh != null)
            {
                if (_proxy.addNormal)
                {
                    var flipX = armature.flipX ? 1f : -1f;
                    var flipY = armature.flipY ? 1f : -1f;
                    float normalZ = -flipX * flipY;
                    if (_normals == null || _normals.Length != mesh.vertexCount)
                    {
                        _normals = new Vector3[mesh.vertexCount];
                        _normalVal.z = 0f;
                    }
                    if (normalZ != _normalVal.z)
                    {
                        _normalVal.z = normalZ;
                        for (int i = 0; i < mesh.vertexCount; ++i)
                        {
                            _normals[i] = _normalVal;
                        }
                        mesh.normals = _normals;
                    }
                }
                else
                {
                    _normals = null;
                    _normalVal.z = 0f;
                }
            }
            else
            {
                _normals = null;
                _normalVal.z = 0f;
            }
        }

        public Mesh mesh
        {
            get
            {
                if (this._meshBuffer == null)
                {
                    return null;
                }

                return this._meshBuffer.sharedMesh;
            }
        }

        public MeshRenderer meshRenderer
        {
            get { return this._renderer; }
        }

        public UnityTextureAtlasData currentTextureAtlasData
        {
            get
            {
                if (this._textureData == null || this._textureData.parent == null)
                {
                    return null;
                }

                return this._textureData.parent as UnityTextureAtlasData;
            }
        }

        public GameObject renderDisplay
        {
            get { return this._renderDisplay; }
        }

        public UnityArmatureComponent proxy
        {
            get { return this._proxy; }
        }
    }
}