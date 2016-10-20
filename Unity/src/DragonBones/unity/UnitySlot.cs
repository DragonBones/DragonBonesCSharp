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
        public static string defaultShaderName = "Sprites/Default";

        private static Vector3 _helpVector3 = new Vector3();
        private static Color _helpColor = new Color();

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
                Object.DestroyImmediate(_mesh);
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

            // TODO sortingGroup(Unity 5.5)
            _helpVector3.Set(0.0f, 0.0f, -this._zOrder * (armatureComponent.zSpace + 0.00001f));
            _renderDisplay.transform.localPosition = _helpVector3;
        }

        /**
         * @private
         */
        override protected void _replaceDisplay(object value)
        {
            var container = this._armature._display as GameObject;
            var prevDisplay = value as GameObject;

            _renderDisplay.transform.parent = container.transform;

            var armatureComponent = container.GetComponent<UnityArmatureComponent>();
            if (armatureComponent != null && Application.isPlaying)
            {
                prevDisplay.transform.parent = UnityFactory._hiddenObject.transform;
            }
            else
            {
                _renderDisplay.SetActive(true);
                prevDisplay.SetActive(false);
            }

            _renderDisplay.transform.localPosition = prevDisplay.transform.localPosition; // TODO sortingGroup(Unity 5.5)
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
            var renderer = _renderDisplay.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                // TODO
                /*if (
                    this._colorTransform.redMultiplier != 1.0f ||
                    this._colorTransform.greenMultiplier != 1.0f ||
                    this._colorTransform.blueMultiplier != 1.0f ||
                    this._colorTransform.redOffset != 0 ||
                    this._colorTransform.greenOffset != 0 ||
                    this._colorTransform.blueOffset != 0 ||
                    this._colorTransform.alphaOffset != 0
                )
                {
                }*/

                _helpColor.r = this._colorTransform.redMultiplier;
                _helpColor.g = this._colorTransform.greenMultiplier;
                _helpColor.b = this._colorTransform.blueMultiplier;
                _helpColor.a = this._colorTransform.alphaMultiplier;

                renderer.color = _helpColor;
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
            if (this._display != null && this._displayIndex >= 0)
            {
                var rawDisplayData = this._displayIndex < this._displayDataSet.displays.Count ? this._displayDataSet.displays[this._displayIndex] : null;
                var replacedDisplayData = this._displayIndex < this._replacedDisplayDataSet.Count ? this._replacedDisplayDataSet[this._displayIndex] : null;
                var currentDisplayData = replacedDisplayData != null ? replacedDisplayData : rawDisplayData;
                var currentTextureData = currentDisplayData.texture as UnityTextureData;
                if (currentTextureData != null)
                {
                    var textureAtlasTexture = (currentTextureData.parent as UnityTextureAtlasData).texture;
                    if (currentTextureData.texture == null && textureAtlasTexture != null) // Create and cache texture.
                    {
                        var rect = new Rect(
                            currentTextureData.region.x, 
                            textureAtlasTexture.height - currentTextureData.region.y - currentTextureData.region.height, 
                            currentTextureData.region.width, 
                            currentTextureData.region.height
                        );

                        currentTextureData.texture = Sprite.Create(textureAtlasTexture, rect, new Vector2());
                    }

                    if (currentTextureData.texture != null)
                    {
                        var currentTextureAtlasTexture = this._armature._replacedTexture != null ? (this._armature._replacedTexture as Texture2D) : textureAtlasTexture;
                        if (currentTextureData.texture.texture != currentTextureAtlasTexture)
                        {
                            var prevTexture = currentTextureData.texture;
                            currentTextureData.texture = Sprite.Create(currentTextureAtlasTexture, prevTexture.textureRect, prevTexture.pivot, prevTexture.pixelsPerUnit);
#if UNITY_EDITOR
                            Object.DestroyImmediate(prevTexture);
#else
                            Object.Destroy(prevTexture);
#endif
                        }
                    }

                    this._updatePivot(rawDisplayData, currentDisplayData, currentTextureData);
                    
                    if (this._meshData != null && this._display == this._meshDisplay) // Mesh.
                    {
                        if (_mesh == null)
                        {
                            _mesh = new Mesh();
                        }

                        _uvs = new Vector2[this._meshData.uvs.Count / 2];
                        _vertices = new Vector3[this._meshData.vertices.Count / 2];
                        //var triangles = new int[this._meshData.vertexIndices.Count];

                        for (int i = 0, l = this._meshData.uvs.Count; i < l; i += 2)
                        {
                            var iN = i / 2;
                            var u = this._meshData.uvs[i];
                            var v = this._meshData.uvs[i + 1];
                            _uvs[iN] = new Vector2(
                                (currentTextureData.region.x + u * currentTextureData.region.width) / textureAtlasTexture.width,
                                1.0f - (currentTextureData.region.y + v * currentTextureData.region.height) / textureAtlasTexture.height
                            );
                            _vertices[iN] = new Vector3(this._meshData.vertices[i], -this._meshData.vertices[i + 1], 0.0f);
                        }

                        //for (int i = 0, l = this._meshData.vertexIndices.Count; i < l; ++i)
                        //{
                        //    triangles[i] = this._meshData.vertexIndices[i];
                        //}

                        _mesh.vertices = _vertices; // Must set vertices before uvs.
                        _mesh.uv = _uvs;
                        _mesh.triangles = this._meshData.vertexIndices.ToArray();
                        
                        if (currentTextureData.material == null)
                        {
                            var shader = Shader.Find(defaultShaderName);
                            currentTextureData.material = new Material(shader);
                            currentTextureData.material.mainTexture = currentTextureData.texture.texture;
                        }

                        var meshRenderer = _renderDisplay.GetComponent<MeshRenderer>();
                        if (meshRenderer == null)
                        {
                            Object.DestroyImmediate(_renderDisplay.GetComponent<Renderer>()); // RemoveComponent
                            meshRenderer = _renderDisplay.AddComponent<MeshRenderer>();
                        }

                        meshRenderer.enabled = true;
                        meshRenderer.material = currentTextureData.material;

                        var meshFilter = _renderDisplay.GetComponent<MeshFilter>();
                        if (meshFilter == null)
                        {
                            meshFilter = _renderDisplay.AddComponent<MeshFilter>();
                        }

                        meshFilter.mesh = _mesh;

                        // Identity transform.
                        if (this._meshData.skinned)
                        {
                            _renderDisplay.transform.localPosition = new Vector3(0.0f, 0.0f, _renderDisplay.transform.localPosition.z);
                            _renderDisplay.transform.localEulerAngles = new Vector3();
                            _renderDisplay.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                        }
                    }
                    else // Normal texture.
                    {
                        this._pivotY -= currentTextureData.region.height * this._armature.armatureData.scale;

                        var spriteRendererA = _renderDisplay.GetComponent<SpriteRenderer>();
                        if (spriteRendererA == null)
                        {
                            Object.DestroyImmediate(_renderDisplay.GetComponent<Renderer>()); // RemoveComponent
                            Object.DestroyImmediate(_renderDisplay.GetComponent<MeshFilter>()); // RemoveComponent
                            spriteRendererA = _renderDisplay.AddComponent<SpriteRenderer>();
                        }

                        spriteRendererA.enabled = true;
                        spriteRendererA.sprite = currentTextureData.texture;
                    }

                    //this._updateVisible();

                    return;
                }
            }

            this._pivotX = 0.0f;
            this._pivotY = 0.0f;
            
            //_renderDisplay.SetActive(false);

            var spriteRendererB = _renderDisplay.GetComponent<SpriteRenderer>();
            if (spriteRendererB == null)
			{
				Object.DestroyImmediate(_renderDisplay.GetComponent<Renderer>()); // RemoveComponent
                Object.DestroyImmediate(_renderDisplay.GetComponent<MeshFilter>()); // RemoveComponent
                spriteRendererB = _renderDisplay.AddComponent<SpriteRenderer>();
            }

            spriteRendererB.enabled = false;
            spriteRendererB.sprite = null;

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
            var transform = _renderDisplay.transform;
            
            _helpVector3.x = this.globalTransformMatrix.tx - (this.globalTransformMatrix.a * this._pivotX + this.globalTransformMatrix.c * this._pivotY);
            _helpVector3.y = -(this.globalTransformMatrix.ty - (this.globalTransformMatrix.b * this._pivotX + this.globalTransformMatrix.d * this._pivotY));
            _helpVector3.z = transform.localPosition.z; // TODO sortingGroup(Unity 5.5)

            transform.localPosition = _helpVector3;

            _helpVector3.x = 0.0f;
            _helpVector3.y = 0.0f;
            _helpVector3.z = -this.global.skewY * DragonBones.RADIAN_TO_ANGLE;

            transform.localEulerAngles = _helpVector3;

            _helpVector3.x = this.global.scaleX;
            _helpVector3.y = this.global.scaleY;
            _helpVector3.z = 1.0f;

            transform.localScale = _helpVector3;
        }
    }
}