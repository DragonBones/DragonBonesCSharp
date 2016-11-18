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
        //private static Color _helpColor = new Color();
        private static readonly Vector2[] _helpVector2s = { new Vector2(), new Vector2(), new Vector2(), new Vector2() };
        private static readonly Vector3[] _helpVector3s = { new Vector3(), new Vector3(), new Vector3(), new Vector3() };

        private GameObject _renderDisplay;
        private Mesh _mesh;
        private Vector2[] _uvs;
        private Vector3[] _vertices;

        /**
         * @language zh_CN
         * 创建一个空的插槽。
         * @version DragonBones 3.0
         */
        public UnitySlot()
        {
        }

        /**
         * @inheritDoc
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
            if (this._rawDisplay == null)
            {
                this._rawDisplay = new GameObject();
            }

            _renderDisplay = (this._display != null ? this._display : this._rawDisplay) as GameObject;
        }

        /**
         * @private
         */
        override protected void _addDisplay()
        {
            var container = this._armature._display as GameObject;
            var armatureComponent = container.GetComponent<UnityArmatureComponent>();
            _renderDisplay.transform.parent = container.transform;
            
            _helpVector3.Set(0.0f, 0.0f, -this._zOrder * (armatureComponent.zSpace + 0.001f));
            _renderDisplay.transform.localPosition = _helpVector3;
        }

        /**
         * @private
         */
        override protected void _replaceDisplay(object value)
        {
            var container = this._armature._display as GameObject;
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
            var container = this._armature._display as GameObject;
            var armatureComponent = container.GetComponent<UnityArmatureComponent>();
            _helpVector3.Set(_renderDisplay.transform.localPosition.x, _renderDisplay.transform.localPosition.y, -this._zOrder * (armatureComponent.zSpace + 0.001f));
            _renderDisplay.transform.localPosition = _helpVector3;
        }

        /**
         * @private
         */
        override internal void _updateVisible()
        {
            _renderDisplay.SetActive(this._parent.visible);
        }

        /**
         * @private
         */
        override protected void _updateBlendMode()
        {
            // TODO
            switch (this._blendMode)
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
                _helpColor.r = this._colorTransform.redMultiplier;
                _helpColor.g = this._colorTransform.greenMultiplier;
                _helpColor.b = this._colorTransform.blueMultiplier;
                _helpColor.a = this._colorTransform.alphaMultiplier;

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
                            this._colorTransform.redMultiplier, 
                            this._colorTransform.greenMultiplier, 
                            this._colorTransform.blueMultiplier, 
                            this._colorTransform.alphaMultiplier
                        ));
                    }

                    mesh.SetColors(colors);
                }
            }
        }

        /**
         * @private
         */
        override protected void _updateFilters() { }

        /**
         * @private
         */
        override protected void _updateFrame()
        {
            var renderer = _renderDisplay.GetComponent<MeshRenderer>();
            var meshFilter = _renderDisplay.GetComponent<MeshFilter>();

            if (this._display != null && this._displayIndex >= 0)
            {
                var rawDisplayData = this._displayIndex < this._displayDataSet.displays.Count ? this._displayDataSet.displays[this._displayIndex] : null;
                var replacedDisplayData = this._displayIndex < this._replacedDisplayDataSet.Count ? this._replacedDisplayDataSet[this._displayIndex] : null;
                var currentDisplayData = replacedDisplayData != null ? replacedDisplayData : rawDisplayData;
                var currentTextureData = currentDisplayData.texture as UnityTextureData;
                if (currentTextureData != null)
                {
                    var textureAtlasData = currentTextureData.parent as UnityTextureAtlasData;
                    var textureAtlasTexture = textureAtlasData.texture;
                    if (textureAtlasTexture != null)
                    {
                        var textureAtlasWidth = textureAtlasTexture.mainTexture.width;
                        var textureAtlasHeight = textureAtlasTexture.mainTexture.height;

                        if (_mesh == null)
                        {
                            _mesh = new Mesh();
                        }

                        this._updatePivot(rawDisplayData, currentDisplayData, currentTextureData);

                        if (this._meshData != null && this._display == this._meshDisplay) // Mesh.
                        {
                            _uvs = new Vector2[this._meshData.uvs.Count / 2];
                            _vertices = new Vector3[this._meshData.vertices.Count / 2];

                            for (int i = 0, l = this._meshData.uvs.Count; i < l; i += 2)
                            {
                                var iN = i / 2;
                                var u = this._meshData.uvs[i];
                                var v = this._meshData.uvs[i + 1];
                                _uvs[iN] = new Vector2(
                                    (currentTextureData.region.x + u * currentTextureData.region.width) / textureAtlasWidth,
                                    1.0f - (currentTextureData.region.y + v * currentTextureData.region.height) / textureAtlasHeight
                                );
                                _vertices[iN] = new Vector3(this._meshData.vertices[i], -this._meshData.vertices[i + 1], 0.0f);
                            }

                            _mesh.vertices = _vertices; // Must set vertices before uvs.
                            _mesh.uv = _uvs;
                            _mesh.triangles = this._meshData.vertexIndices.ToArray();

                            // Identity transform.
                            if (this._meshData.skinned)
                            {
                                _renderDisplay.transform.localPosition = new Vector3(0.0f, 0.0f, _renderDisplay.transform.localPosition.z);

                                if (this._armature._flipX)
                                {
                                    _helpVector3.y = 180.0f;
                                }
                                else
                                {
                                    _helpVector3.y = 0.0f;
                                }

                                if (this._armature._flipY)
                                {
                                    _helpVector3.x = 180.0f;
                                }
                                else
                                {
                                    _helpVector3.x = 0.0f;
                                }

                                _helpVector3.z = 0.0f;
                                _renderDisplay.transform.localEulerAngles = _helpVector3;

                                _renderDisplay.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                            }
                        }
                        else // Normal texture.
                        {
                            this._pivotY -= currentTextureData.region.height * this._armature.armatureData.scale;

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
                                _helpVector3s[i].x = u * currentTextureData.region.width * 0.01f;
                                _helpVector3s[i].y = (1.0f - v) * currentTextureData.region.height * 0.01f;
                                _helpVector3s[i].z = 0.0f * 0.01f;
                            }

                            _mesh.vertices = _helpVector3s; // Must set vertices before uvs.
                            _mesh.uv = _helpVector2s;
                            _mesh.triangles = TRIANGLES;
                        }

                        meshFilter.sharedMesh = _mesh;

                        if (this._armature._replacedTexture != null)
                        {
                            renderer.sharedMaterial = this._armature._replacedTexture as Material;
                        }
                        else
                        {
                            renderer.sharedMaterial = textureAtlasData.texture;
                        }

                        this._updateVisible();

                        return;
                    }
                }
            }

            this._pivotX = 0.0f;
            this._pivotY = 0.0f;
            
            _renderDisplay.SetActive(false);

            _helpVector3.x = this.origin.x;
            _helpVector3.y = this.origin.y;
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

            var hasFFD = this._ffdVertices.Count > 0;

            if (this._meshData.skinned)
            {
                for (int i = 0, iF = 0, l = this._meshData.vertices.Count; i < l; i += 2)
                {
                    int iH = i / 2;

                    var boneIndices = this._meshData.boneIndices[iH];
                    var boneVertices = this._meshData.boneVertices[iH];
                    var weights = this._meshData.weights[iH];

                    var xG = 0.0f;
                    var yG = 0.0f;

                    for (int iB = 0, lB = boneIndices.Length; iB < lB; ++iB)
                    {
                        var bone = this._meshBones[boneIndices[iB]];
                        var matrix = bone.globalTransformMatrix;
                        var weight = weights[iB];
                        
                        var xL = 0.0f;
                        var yL = 0.0f;

                        if (hasFFD)
                        {
                            xL = boneVertices[iB * 2] + this._ffdVertices[iF];
                            yL = boneVertices[iB * 2 + 1] + this._ffdVertices[iF + 1];
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
                var vertices = this._meshData.vertices;
                for (int i = 0, l = this._meshData.vertices.Count; i < l; i += 2)
                {
                    int iH = i / 2;
                    var xG = vertices[i] + this._ffdVertices[i];
                    var yG = vertices[i + 1] + this._ffdVertices[i + 1];
                    _vertices[iH].x = xG;
                    _vertices[iH].y = -yG;
                }

                _mesh.vertices = _vertices;
            }
        }

        /**
         * @private
         */
        override protected void _updateTransform()
        {
            var flipX = this._armature._flipX;
            var flipY = this._armature._flipY;
            var scaleX = flipX ? -this.global.scaleX : this.global.scaleX;
            var scaleY = flipY ? -this.global.scaleY : this.global.scaleY;
            var transform = _renderDisplay.transform;
            
            _helpVector3.x = this.globalTransformMatrix.tx - (this.globalTransformMatrix.a * this._pivotX + this.globalTransformMatrix.c * this._pivotY);
            _helpVector3.y = -(this.globalTransformMatrix.ty - (this.globalTransformMatrix.b * this._pivotX + this.globalTransformMatrix.d * this._pivotY));
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
            
            if (scaleY >= 0.0f || this._childArmature != null)
            {
                _helpVector3.x = 0.0f;
            }
            else
            {
                _helpVector3.x = 180.0f;
            }

            if (scaleX >= 0.0f || this._childArmature != null)
            {
                _helpVector3.y = 0.0f;
            }
            else
            {
                _helpVector3.y = 180.0f;
            }

            _helpVector3.z = -this.global.skewY * DragonBones.RADIAN_TO_ANGLE;

            if (flipX != flipY && this._childArmature != null)
            {
                _helpVector3.z = -_helpVector3.z;
            }

            transform.localEulerAngles = _helpVector3;

            _helpVector3.x = scaleX >= 0.0f ? scaleX : -scaleX;
            _helpVector3.y = scaleY >= 0.0f ? scaleY : -scaleY;
            _helpVector3.z = 1.0f;

            transform.localScale = _helpVector3;
        }
    }
}