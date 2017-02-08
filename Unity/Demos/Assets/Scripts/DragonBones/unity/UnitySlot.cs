using System.Collections.Generic;
using UnityEngine;

namespace DragonBones
{
    /**
     * @language zh_CN
     * Unity 插槽。
     * @version DragonBones 3.0
     */
    public class UnitySlot : Slot
    {
        private static readonly int[] TRIANGLES = { 0, 1, 2, 0, 2, 3 };
        private static Vector3 _helpVector3 = new Vector3();
        private static readonly Vector2[] _helpVector2s = { new Vector2(), new Vector2(), new Vector2(), new Vector2() };

        private bool _skewed;
        private UnityArmatureComponent _proxy;
        private GameObject _renderDisplay;
        private Mesh _mesh;
        private Vector2[] _uvs;
        private Vector3[] _vertices;
        /**
         * @private
         */
        public UnitySlot()
        {
        }
        /**
         * @private
         */
        override protected void _onClear()
        {
            base._onClear();

            if (_mesh != null)
            {
#if UNITY_EDITOR
                //Object.DestroyImmediate(_mesh);
#else
                Object.Destroy(_mesh);
#endif
            }

            _skewed = false;
            _proxy = null;
            _renderDisplay = null;
            _mesh = null;
            _uvs = null;
            _vertices = null;
        }
        /**
         * @private
         */
        override protected void _initDisplay(object value)
        {
        }
        /**
         * @private
         */
        override protected void _disposeDisplay(object value)
        {
            var gameObject = value as GameObject;
#if UNITY_EDITOR
            Object.DestroyImmediate(gameObject);
#else
            Object.Destroy(gameObject);
#endif
        }
        /**
         * @private
         */
        override protected void _onUpdateDisplay()
        {
            _renderDisplay = (_display != null ? _display : _rawDisplay) as GameObject;
        }
        /**
         * @private
         */
        override protected void _addDisplay()
        {
            _proxy = _armature.eventDispatcher as UnityArmatureComponent;

            var container = _armature.display as GameObject;
            if (_renderDisplay.transform.parent != container.transform)
            {
                _renderDisplay.transform.parent = container.transform;

                _helpVector3.Set(0.0f, 0.0f, -_zOrder * (_proxy.zSpace + 0.001f));
                _renderDisplay.transform.localPosition = _helpVector3;
            }
        }
        /**
         * @private
         */
        override protected void _replaceDisplay(object value)
        {
            var container = _armature.display as GameObject;
            var prevDisplay = value as GameObject;
            prevDisplay.hideFlags = HideFlags.HideInHierarchy;
            prevDisplay.transform.parent = null;
            prevDisplay.SetActive(false);

            _renderDisplay.hideFlags = HideFlags.None;
            _renderDisplay.transform.parent = container.transform;
            _renderDisplay.transform.localPosition = prevDisplay.transform.localPosition;
            _renderDisplay.SetActive(true);
        }
        /**
         * @private
         */
        override protected void _removeDisplay()
        {
            _renderDisplay.transform.parent = null;
        }
        /**
         * @private
         */
        override protected void _updateZOrder()
        {
            // var container = _armature.display as GameObject;
            _helpVector3.Set(_renderDisplay.transform.localPosition.x, _renderDisplay.transform.localPosition.y, -_zOrder * (_proxy.zSpace + 0.001f));
            _renderDisplay.transform.localPosition = _helpVector3;
        }
        /**
         * @private
         */
        override internal void _updateVisible()
        {
            _renderDisplay.SetActive(_parent.visible);
        }
        /**
         * @private
         */
        override protected void _updateBlendMode()
        {
            // TODO
            switch (_blendMode)
            {
                case BlendMode.Normal:
                    break;

                case BlendMode.Add:
                    break;

                default:
                    break;
            }
        }
        /**
         * @private
         */
        override protected void _updateColor()
        {
            // TODO
            /*var renderer = _renderDisplay.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                _helpColor.r = _colorTransform.redMultiplier;
                _helpColor.g = _colorTransform.greenMultiplier;
                _helpColor.b = _colorTransform.blueMultiplier;
                _helpColor.a = _colorTransform.alphaMultiplier;

                renderer.color = _helpColor;
            }*/

            var meshFilter = _renderDisplay.GetComponent<MeshFilter>();
            if (meshFilter != null)
            {
                var mesh = meshFilter.sharedMesh;
                if (mesh != null)
                {
                    var colors = new List<Color>(mesh.vertices.Length);
                    for (int i = 0, l = mesh.vertices.Length; i < l; ++i)
                    {
                        colors.Add(new Color(
                            _colorTransform.redMultiplier,
                            _colorTransform.greenMultiplier,
                            _colorTransform.blueMultiplier,
                            _colorTransform.alphaMultiplier
                        ));
                    }

                    mesh.SetColors(colors);
                }
            }
        }
        /**
         * @private
         */
        override protected void _updateFrame()
        {
            var isMeshDisplay = _meshData != null && _display == _meshDisplay;
            var currentTextureData = _textureData as UnityTextureData;

            var renderer = _renderDisplay.GetComponent<MeshRenderer>();
            var meshFilter = _renderDisplay.GetComponent<MeshFilter>();

            if (_display != null && _displayIndex >= 0 && currentTextureData != null)
            {
                var currentTextureAtlasData = currentTextureData.parent as UnityTextureAtlasData;

                // Update replaced texture atlas.
                if (_armature.replacedTexture != null && _displayData != null && currentTextureAtlasData == _displayData.texture.parent)
                {
                    currentTextureAtlasData = _armature._replaceTextureAtlasData as UnityTextureAtlasData;
                    if (currentTextureAtlasData == null)
                    {
                        currentTextureAtlasData = BaseObject.BorrowObject<UnityTextureAtlasData>();
                        currentTextureAtlasData.CopyFrom(_textureData.parent);
                        currentTextureAtlasData.texture = _armature.replacedTexture as Material;
                        _armature._replaceTextureAtlasData = currentTextureAtlasData;
                    }

                    currentTextureData = currentTextureAtlasData.GetTexture(currentTextureData.name) as UnityTextureData;
                }

                var currentTextureAtlas = currentTextureAtlasData.texture;
                if (currentTextureAtlas != null)
                {
                    var textureAtlasWidth = currentTextureAtlasData.width > 0.0f ? currentTextureAtlasData.width : currentTextureAtlas.mainTexture.width;
                    var textureAtlasHeight = currentTextureAtlasData.height > 0.0f ? currentTextureAtlasData.height : currentTextureAtlas.mainTexture.height;

                    if (_mesh != null)
                    {
#if UNITY_EDITOR
                        //Object.DestroyImmediate(_mesh);
#else
                        Object.Destroy(_mesh);
#endif
                    }

                    _mesh = new Mesh();

                    if (isMeshDisplay) // Mesh.
                    {
                        _uvs = new Vector2[_meshData.uvs.Count / 2];
                        _vertices = new Vector3[_meshData.vertices.Count / 2];

                        for (int i = 0, l = _meshData.uvs.Count; i < l; i += 2)
                        {
                            var iN = i / 2;
                            var u = _meshData.uvs[i];
                            var v = _meshData.uvs[i + 1];
                            _uvs[iN] = new Vector2(
                                (currentTextureData.region.x + u * currentTextureData.region.width) / textureAtlasWidth,
                                1.0f - (currentTextureData.region.y + v * currentTextureData.region.height) / textureAtlasHeight
                            );
                            _vertices[iN] = new Vector3(_meshData.vertices[i], -_meshData.vertices[i + 1], 0.0f);
                        }

                        _mesh.vertices = _vertices; // Must set vertices before uvs.
                        _mesh.uv = _uvs;
                        _mesh.triangles = _meshData.vertexIndices.ToArray();
                    }
                    else // Normal texture.
                    {
                        var pivotY = _pivotY - currentTextureData.region.height * _armature.armatureData.scale;

                        if (_vertices == null || _vertices.Length != 4)
                        {
                            _vertices = new Vector3[4];
                        }

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

                            _helpVector2s[i].x = (currentTextureData.region.x + u * currentTextureData.region.width) / textureAtlasWidth;
                            _helpVector2s[i].y = 1.0f - (currentTextureData.region.y + v * currentTextureData.region.height) / textureAtlasHeight;
                            _vertices[i].x = (u * currentTextureData.region.width) * 0.01f - _pivotX;
                            _vertices[i].y = (1.0f - v) * currentTextureData.region.height * 0.01f + pivotY;
                            _vertices[i].z = 0.0f * 0.01f;
                        }

                        _mesh.vertices = _vertices; // Must set vertices before uvs.
                        _mesh.uv = _helpVector2s;
                        _mesh.triangles = TRIANGLES;
                    }

                    meshFilter.sharedMesh = _mesh;
                    renderer.sharedMaterial = currentTextureAtlas;

                    _updateVisible();

                    return;
                }
            }

            _renderDisplay.SetActive(false);
            meshFilter.sharedMesh = null;
            renderer.sharedMaterial = null;
            _helpVector3.x = 0.0f;
            _helpVector3.y = 0.0f;
            _helpVector3.z = _renderDisplay.transform.localPosition.z;

            _renderDisplay.transform.localPosition = _helpVector3;
        }
        /**
         * @private
         */
        override protected void _updateMesh()
        {
            if (_mesh == null)
            {
                return;
            }

            var hasFFD = _ffdVertices.Count > 0;

            if (_meshData.skinned)
            {
                for (int i = 0, iF = 0, l = _meshData.vertices.Count; i < l; i += 2)
                {
                    int iH = i / 2;

                    var boneIndices = _meshData.boneIndices[iH];
                    var boneVertices = _meshData.boneVertices[iH];
                    var weights = _meshData.weights[iH];

                    var xG = 0.0f;
                    var yG = 0.0f;

                    for (int iB = 0, lB = boneIndices.Length; iB < lB; ++iB)
                    {
                        var bone = _meshBones[boneIndices[iB]];
                        var matrix = bone.globalTransformMatrix;
                        var weight = weights[iB];

                        var xL = 0.0f;
                        var yL = 0.0f;

                        if (hasFFD)
                        {
                            xL = boneVertices[iB * 2] + _ffdVertices[iF];
                            yL = boneVertices[iB * 2 + 1] + _ffdVertices[iF + 1];
                        }
                        else
                        {
                            xL = boneVertices[iB * 2];
                            yL = boneVertices[iB * 2 + 1];
                        }

                        xG += (matrix.a * xL + matrix.c * yL + matrix.tx) * weight;
                        yG += (matrix.b * xL + matrix.d * yL + matrix.ty) * weight;

                        iF += 2;
                    }

                    _vertices[iH].x = xG;
                    _vertices[iH].y = -yG;
                }

                _mesh.vertices = _vertices;
            }
            else if (hasFFD)
            {
                var vertices = _meshData.vertices;
                for (int i = 0, l = _meshData.vertices.Count; i < l; i += 2)
                {
                    int iH = i / 2;
                    var xG = vertices[i] + _ffdVertices[i];
                    var yG = vertices[i + 1] + _ffdVertices[i + 1];
                    _vertices[iH].x = xG;
                    _vertices[iH].y = -yG;
                }

                _mesh.vertices = _vertices;

                // Modify flip.
                _transformDirty = true;
            }
        }
        /**
         * @private
         */
        override protected void _updateTransform(bool isSkinnedMesh)
        {
            if (isSkinnedMesh) // Identity transform.
            {
                if (_armature.flipX)
                {
                    _helpVector3.y = 180.0f;
                }
                else
                {
                    _helpVector3.y = 0.0f;
                }

                if (_armature.flipY)
                {
                    _helpVector3.x = 180.0f;
                }
                else
                {
                    _helpVector3.x = 0.0f;
                }

                _helpVector3.z = 0.0f;

                _renderDisplay.transform.localPosition = new Vector3(0.0f, 0.0f, _renderDisplay.transform.localPosition.z);
                _renderDisplay.transform.localEulerAngles = _helpVector3;
                _renderDisplay.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            }
            else
            {
                var flipX = _armature.flipX;
                var flipY = _armature.flipY;
                var scaleX = flipX ? -global.scaleX : global.scaleX;
                var scaleY = flipY ? -global.scaleY : global.scaleY;
                var transform = _renderDisplay.transform;

                _helpVector3.x = globalTransformMatrix.tx;
                _helpVector3.y = -globalTransformMatrix.ty;
                _helpVector3.z = transform.localPosition.z;

                if (flipX)
                {
                    _helpVector3.x = -_helpVector3.x;
                }

                if (flipY)
                {
                    _helpVector3.y = -_helpVector3.y;
                }

                transform.localPosition = _helpVector3;

                if (scaleY >= 0.0f || _childArmature != null)
                {
                    _helpVector3.x = 0.0f;
                }
                else
                {
                    _helpVector3.x = 180.0f;
                }

                if (scaleX >= 0.0f || _childArmature != null)
                {
                    _helpVector3.y = 0.0f;
                }
                else
                {
                    _helpVector3.y = 180.0f;
                }

                _helpVector3.z = -global.skewY * DragonBones.RADIAN_TO_ANGLE;

                if (flipX != flipY && _childArmature != null)
                {
                    _helpVector3.z = -_helpVector3.z;
                }

                transform.localEulerAngles = _helpVector3;

                // Modify mesh skew. // TODO child armature skew.
                if ((_display == _rawDisplay || _display == _meshDisplay) && _mesh != null)
                {
                    var dSkew = global.skewX - global.skewY;
                    var skewed = dSkew < -0.01f || 0.01f < dSkew;
                    if (_skewed || skewed)
                    {
                        _skewed = skewed;

                        var isPositive = global.scaleX >= 0.0f;
                        var cos = Mathf.Cos(dSkew);
                        var sin = Mathf.Sin(dSkew);

                        var vertices = _mesh.vertices;
                        for (int i = 0, l = _vertices.Length; i < l; ++i)
                        {
                            var x = _vertices[i].x;
                            var y = _vertices[i].y;

                            if (isPositive)
                            {
                                vertices[i].x = x + y * sin;
                            }
                            else
                            {
                                vertices[i].x = -x + y * sin;
                            }

                            vertices[i].y = y * cos;
                        }

                        _mesh.vertices = vertices;
                    }
                }

                _helpVector3.x = scaleX >= 0.0f ? scaleX : -scaleX;
                _helpVector3.y = scaleY >= 0.0f ? scaleY : -scaleY;
                _helpVector3.z = 1.0f;

                transform.localScale = _helpVector3;
            }
        }
    }
}