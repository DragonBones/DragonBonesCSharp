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
    [DisallowMultipleComponent]
    public class UnitySlot : Slot
    {
        private static readonly int[] TRIANGLES = { 0, 1, 2, 0, 2, 3 };
        private static readonly Vector2[] _helpVector2s = { new Vector2(), new Vector2(), new Vector2(), new Vector2() };
        private static Vector3 _helpVector3 = new Vector3();

        private bool _skewed;
        private UnityArmatureComponent _proxy;
        private GameObject _renderDisplay;
        private Mesh _mesh;
        private Vector2[] _uvs;
        private Vector3[] _vertices;
        private Vector3[] _vertices2;
        private Vector3[] _normals;
        private Color32[] _colors;
        private Vector3 _normalVal = Vector3.zero;
        private MeshRenderer _renderer = null;
        private MeshFilter _meshFilter = null;
        private UnityUGUIDisplay _uiDisplay = null;

        private BlendMode _currentBlendMode;

        /**
         * @private
         */
        public UnitySlot()
        {
        }

        /**
         * @private
         */
        protected override void _OnClear()
        {
            base._OnClear();

            if (_mesh != null)
            {
                UnityFactoryHelper.DestroyUnityObject(_mesh);
            }

            _skewed = false;
            _proxy = null;
            _renderDisplay = null;
            _mesh = null;
            _uvs = null;
            _vertices = null;
            _vertices2 = null;
            _normals = null;
            _colors = null;

            _currentBlendMode = BlendMode.Normal;
        }

        /**
         * @private
         */
        protected override void _InitDisplay(object value)
        {

        }
        /**
         * @private
         */
        protected override void _DisposeDisplay(object value)
        {
            UnityFactoryHelper.DestroyUnityObject(value as GameObject);
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
                if (_meshFilter == null)
                {
                    _meshFilter = _renderDisplay.AddComponent<MeshFilter>();
                }
            }

            //init mesh
            if (_mesh == null)
            {
                _mesh = new Mesh();
                _mesh.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
                _mesh.MarkDynamic();
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

                _helpVector3.Set(0.0f, 0.0f, -_zOrder * (_proxy.zSpace + 0.001f));
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
            _helpVector3.Set(_renderDisplay.transform.localPosition.x, _renderDisplay.transform.localPosition.y, -_zOrder * (_proxy._zSpace + 0.001f));

            _SetZorder(_helpVector3);
        }

        /**
         * @internal
         */
        internal void _SetZorder(Vector3 zorderPos)
        {
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
                    if (_renderer == null)
                    {
                        _renderer = _renderDisplay.GetComponent<MeshRenderer>();
                        if (_renderer == null)
                        {
                            _renderer = _renderDisplay.AddComponent<MeshRenderer>();
                        }
                    }

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

        /**
         * @private
         */
        internal override void _UpdateVisible()
        {
            _renderDisplay.SetActive(_parent.visible);
        }
        /**
         * @private
         */
        internal override void _UpdateBlendMode()
        {
            if (_currentBlendMode == _blendMode)
            {
                return;
            }

            if (_childArmature == null)
            {
                if (_uiDisplay != null)
                {
                    _uiDisplay.material = (this._textureData as UnityTextureData).GetMaterial(_blendMode, true);
                }
                else
                {
                    _renderer.sharedMaterial = (this._textureData as UnityTextureData).GetMaterial(_blendMode);
                }
            }
            else
            {
                foreach (var slot in _childArmature.GetSlots())
                {
                    slot._blendMode = _blendMode;
                    slot._UpdateBlendMode();
                }
            }

            _currentBlendMode = _blendMode;
        }
        /**
         * @private
         */
        protected override void _UpdateColor()
        {
            if (this._childArmature == null)
            {
                if (_mesh != null)
                {
                    if (_colors == null || _colors.Length != _mesh.vertexCount)
                    {
                        _colors = new Color32[_mesh.vertexCount];
                    }

                    var proxyTrans = _proxy._colorTransform;
                    for (int i = 0, l = _mesh.vertexCount; i < l; ++i)
                    {
                        _colors[i].r = (byte)(_colorTransform.redMultiplier * proxyTrans.redMultiplier * 255);
                        _colors[i].g = (byte)(_colorTransform.greenMultiplier * proxyTrans.greenMultiplier * 255);
                        _colors[i].b = (byte)(_colorTransform.blueMultiplier * proxyTrans.blueMultiplier * 255);
                        _colors[i].a = (byte)(_colorTransform.alphaMultiplier * proxyTrans.alphaMultiplier * 255);
                    }
                    //
                    _mesh.colors32 = _colors;
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

            if (this._displayIndex >= 0 && this._display != null && currentTextureData != null)
            {
                var currentTextureAtlas = _proxy.isUGUI ? currentTextureAtlasData.uiTexture : currentTextureAtlasData.texture;
                if (currentTextureAtlas != null)
                {
                    var textureAtlasWidth = currentTextureAtlasData.width > 0.0f ? (int)currentTextureAtlasData.width : currentTextureAtlas.mainTexture.width;
                    var textureAtlasHeight = currentTextureAtlasData.height > 0.0f ? (int)currentTextureAtlasData.height : currentTextureAtlas.mainTexture.height;

                    var meshDisplay = this._mesh;
                    meshDisplay.Clear();
                    meshDisplay.uv = null;
                    meshDisplay.vertices = null;
                    meshDisplay.normals = null;
                    meshDisplay.triangles = null;
                    meshDisplay.colors32 = null;

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

                        if (this._uvs == null || this._uvs.Length != vertexCount)
                        {
                            this._uvs = new Vector2[vertexCount];
                        }

                        if (this._vertices == null || this._vertices.Length != vertexCount)
                        {
                            this._vertices = new Vector3[vertexCount];
                            this._vertices2 = new Vector3[vertexCount];
                        }

                        int[] triangles = new int[triangleCount * 3];

                        for (int i = 0, iV = vertexOffset, iU = uvOffset, l = vertexCount; i < l; ++i)
                        {
                            this._vertices[i].x = floatArray[iV++] * textureScale;
                            this._vertices[i].y = floatArray[iV++] * textureScale;

                            this._uvs[i].x = (sourceX + floatArray[iU++] * sourceWidth) / textureAtlasWidth;
                            this._uvs[i].y = 1.0f - (sourceY + floatArray[iU++] * sourceHeight) / textureAtlasHeight;

                            this._vertices2[i] = this._vertices[i];
                        }

                        for (int i = 0; i < triangleCount * 3; ++i)
                        {
                            triangles[i] = intArray[meshData.offset + (int)BinaryOffset.MeshVertexIndices + i];
                        }

                        //
                        meshDisplay.vertices = this._vertices;
                        meshDisplay.uv = this._uvs;// Must set vertices before uvs.
                        meshDisplay.triangles = triangles;
                    }
                    else
                    {
                        if (_vertices == null || _vertices.Length != 4)
                        {
                            _vertices = new Vector3[4];
                            _vertices2 = new Vector3[4];
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
                            var pivotY = scaleHeight - _pivotY;

                            if (currentTextureData.rotated)
                            {
                                var temp = scaleWidth;
                                scaleWidth = scaleHeight;
                                scaleHeight = temp;

                                pivotX = scaleWidth - _pivotX;
                                pivotY = scaleHeight - _pivotY;

                                //uv
                                _helpVector2s[i].x = (sourceX + (1.0f - v) * sourceWidth) / textureAtlasWidth;
                                _helpVector2s[i].y = 1.0f - (sourceY + u * sourceHeight) / textureAtlasHeight;
                            }
                            else
                            {
                                //uv
                                _helpVector2s[i].x = (sourceX + u * sourceWidth) / textureAtlasWidth;
                                _helpVector2s[i].y = 1.0f - (sourceY + v * sourceHeight) / textureAtlasHeight;
                            }

                            //vertices
                            _vertices[i].x = (u * scaleWidth) - pivotX;
                            _vertices[i].y = (1.0f - v) * scaleHeight - pivotY;

                            _vertices[i].z = 0.0f;
                            _vertices2[i] = _vertices[i];
                        }

                        _mesh.vertices = _vertices; // Must set vertices before uvs.
                        _mesh.uv = _helpVector2s;
                        _mesh.triangles = TRIANGLES;
                    }

                    if (_proxy.isUGUI)
                    {
                        _uiDisplay.material = currentTextureAtlas;
                        _uiDisplay.texture = currentTextureAtlas.mainTexture;
                        _mesh.RecalculateBounds();
                        _uiDisplay.sharedMesh = _mesh;
                    }
                    else
                    {
                        if (_renderer.enabled)
                        {
                            _mesh.RecalculateBounds();
                        }

                        _meshFilter.sharedMesh = _mesh;
                        _renderer.sharedMaterial = currentTextureAtlas;
                    }

                    this._currentBlendMode = BlendMode.Normal;
                    this._blendModeDirty = true;
                    this._colorDirty = true;// Relpace texture will override blendMode and color.
                    this._visibleDirty = true;
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
        }

        protected override void _UpdateMesh()
        {
            if (_mesh == null)
            {
                return;
            }

            //
            var hasFFD = this._ffdVertices.Count > 0;
            var scale = _armature.armatureData.scale;
            var meshData = this._meshData;
            var weightData = meshData.weight;
            var meshDisplay = this._mesh;

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

                    _vertices[i].x = xG;
                    _vertices[i].y = yG;
                    _vertices2[i].x = _vertices[i].x;
                    _vertices2[i].y = _vertices[i].y;
                }

                meshDisplay.vertices = _vertices;

                if (_renderer && _renderer.enabled)
                {
                    meshDisplay.RecalculateBounds();
                }
            }
            else if (hasFFD)
            {
                int vertexOffset = intArray[meshData.offset + (int)BinaryOffset.MeshFloatOffset];
                if (vertexOffset < 0)
                {
                    vertexOffset += 65536; // Fixed out of bouds bug. 
                }

                for (int i = 0, iV = 0, iF = 0, l = vertextCount; i < l; ++i)
                {
                    _vertices[i].x = (floatArray[vertexOffset + (iV++)] * scale + this._ffdVertices[iF++]);
                    _vertices[i].y = -(floatArray[vertexOffset + (iV++)] * scale + this._ffdVertices[iF++]);
                    _vertices2[i].x = _vertices[i].x;
                    _vertices2[i].y = _vertices[i].y;
                }

                meshDisplay.vertices = _vertices;

                if (_renderer && _renderer.enabled)
                {
                    meshDisplay.RecalculateBounds();
                }
            }
        }

        protected override void _UpdateTransform(bool isSkinnedMesh)
        {
            if (isSkinnedMesh)
            {
                var transform = _renderDisplay.transform;

                transform.localPosition = new Vector3(0.0f, 0.0f, transform.localPosition.z);
                transform.localEulerAngles = Vector3.zero;
                transform.localScale = Vector3.one;
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
                if ((_display == _rawDisplay || _display == _meshDisplay) && _mesh != null)
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

                        for (int i = 0, l = _vertices.Length; i < l; ++i)
                        {
                            var x = _vertices[i].x;
                            var y = _vertices[i].y;

                            if (isPositive)
                            {
                                _vertices2[i].x = x + y * sin;
                            }
                            else
                            {
                                _vertices2[i].x = -x + y * sin;
                            }

                            _vertices2[i].y = y * cos;
                        }

                        _mesh.vertices = _vertices2;
                        if (_renderer && _renderer.enabled)
                        {
                            _mesh.RecalculateBounds();
                        }
                    }
                }

                //localScale
                _helpVector3.x = global.scaleX;
                _helpVector3.y = global.scaleY;
                _helpVector3.z = 1.0f;

                transform.localScale = _helpVector3;
            }

            if (_childArmature != null)
            {
                UnityArmatureComponent unityArmature = (_childArmature.proxy as UnityArmatureComponent);
                _childArmature.flipX = _armature.flipX;
                _childArmature.flipY = _armature.flipY;

                unityArmature.addNormal = _proxy.addNormal;
                unityArmature.boneHierarchy = _proxy.boneHierarchy;
            }

            UpdateNormal();
        }

        public void UpdateNormal()
        {
            if (_mesh != null)
            {
                if (_proxy.addNormal)
                {
                    var flipX = armature.flipX ? 1f : -1f;
                    var flipY = armature.flipY ? 1f : -1f;
                    float normalZ = -flipX * flipY;
                    if (_normals == null || _normals.Length != _mesh.vertexCount)
                    {
                        _normals = new Vector3[_mesh.vertexCount];
                        _normalVal.z = 0f;
                    }
                    if (normalZ != _normalVal.z)
                    {
                        _normalVal.z = normalZ;
                        for (int i = 0; i < _mesh.vertexCount; ++i)
                        {
                            _normals[i] = _normalVal;
                        }
                        _mesh.normals = _normals;
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
            get { return _mesh; }
        }

        public MeshRenderer meshRenderer
        {
            get { return _renderer; }
        }

        public UnityTextureAtlasData currentTextureAtlasData
        {
            get
            {
                if (_textureData == null || _textureData.parent == null)
                {
                    return null;
                }

                return _textureData.parent as UnityTextureAtlasData;
            }
        }
        
        public GameObject renderDisplay
        {
            get { return _renderDisplay; }
        }

        public UnityArmatureComponent proxy
        {
            get { return _proxy; }
        }
    }
}