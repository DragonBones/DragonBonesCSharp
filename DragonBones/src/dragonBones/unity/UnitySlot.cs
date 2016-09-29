using UnityEngine;

namespace dragonBones
{
    /**
     * @language zh_CN
     * Unity 插槽。
     * @version DragonBones 3.0
     */
    public class UnitySlot : Slot
    {
        private static Shader _defaultShader = Shader.Find("Transparent/Depth Ordered Unlit");

        private GameObject _renderDisplay;

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

            _renderDisplay = null;
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
            Object.Destroy((GameObject)value);
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

            _renderDisplay = (GameObject)(this._display != null ? this._display : this._rawDisplay);
        }

        /**
         * @private
         */
        override protected void _addDisplay()
        {
            var container = (UnityArmatureDisplay)this._armature._display;
            _renderDisplay.transform.parent = container.gameObject.transform;
        }

        /**
         * @private
         */
        override protected void _replaceDisplay(object value)
        {
            var container = (UnityArmatureDisplay)this._armature._display;
            var prevDisplay = (GameObject)value;
            _renderDisplay.transform.parent = container.gameObject.transform;
            prevDisplay.transform.parent = null;
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
            switch (this._blendMode)
            {
                case BlendMode.Normal:
                    break;

                case BlendMode.Add:
                    break;

                case BlendMode.Erase:
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
            if (
                this._colorTransform.redMultiplier != 1 ||
                this._colorTransform.greenMultiplier != 1 ||
                this._colorTransform.blueMultiplier != 1 ||
                this._colorTransform.redOffset != 0 ||
                this._colorTransform.greenOffset != 0 ||
                this._colorTransform.blueOffset != 0 ||
                this._colorTransform.alphaOffset != 0
            )
            {
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
            var frameDisplay = (SpriteRenderer)_renderDisplay.GetComponent<SpriteRenderer>();
            
            if (this._display != null && this._displayIndex >= 0)
            {
                var rawDisplayData = this._displayIndex < this._displayDataSet.displays.Count ? this._displayDataSet.displays[this._displayIndex] : null;
                var replacedDisplayData = this._displayIndex < this._replacedDisplayDataSet.Count ? this._replacedDisplayDataSet[this._displayIndex] : null;
                var currentDisplayData = replacedDisplayData != null ? replacedDisplayData : rawDisplayData;
                var currentTextureData = (UnityTextureData)currentDisplayData.texture;
                if (currentTextureData != null)
                {
                    var textureAtlasTexture = ((UnityTextureAtlasData)currentTextureData.parent).texture;
                    if (currentTextureData.texture == null && textureAtlasTexture != null) // Create and cache material.
                    {
                        var rect = new Rect(currentTextureData.region.x, currentTextureData.region.y, currentTextureData.region.width, currentTextureData.region.height);
                        var pivot = new Vector2();
                        //currentTextureData.material = new Material(_defaultShader);
                        //currentTextureData.material.mainTexture = textureAtlasTexture;
                        currentTextureData.texture = Sprite.Create(textureAtlasTexture, rect, pivot);
                    }

                    if (currentTextureData.texture)
                    {
                        //this._armature._replacedTexture
                    }

                    this._updatePivot(rawDisplayData, currentDisplayData, currentTextureData);
                    
                    if (this._meshData != null && this._display == this._meshDisplay) // Mesh.
                    {
                        var meshDisplay = (GameObject)this._meshDisplay;

                        /*meshNode.uvs.length = 0;
                        meshNode.vertices.length = 0;
                        meshNode.indices.length = 0;

                        for (let i = 0, l = this._meshData.vertices.length; i < l; ++i)
                        {
                            meshNode.uvs[i] = this._meshData.uvs[i];
                            meshNode.vertices[i] = this._meshData.vertices[i];
                        }

                        for (let i = 0, l = this._meshData.vertexIndices.length; i < l; ++i)
                        {
                            meshNode.indices[i] = this._meshData.vertexIndices[i];
                        }

                        meshDisplay.$setBitmapData(texture);

                        meshDisplay.$updateVertices();
                        meshDisplay.$invalidateTransform();

                        // Identity transform.
                        if (this._meshData.skinned)
                        {
                            const transformationMatrix = meshDisplay.matrix;
                            transformationMatrix.identity();
                            meshDisplay.matrix = transformationMatrix;
                        }*/
                    }
                    else // Normal texture.
                    {
                        frameDisplay.sprite = currentTextureData.texture;
                    }

                    this._updateVisible();

                    return;
                }
            }

            this._pivotX = 0;
            this._pivotY = 0;

            /*
            frameDisplay.visible = false;
            frameDisplay.sprite = null;
            frameDisplay.
                x = this.origin.x;
            frameDisplay.y = this.origin.y;
            */

            var position = frameDisplay.transform.position;
            position.x = this.origin.x;
            position.y = this.origin.y;
        }

        /**
         * @private
         */
        override protected void _updateMesh()
        {
            /*const meshDisplay = < egret.Mesh > this._meshDisplay;
            const meshNode = < egret.sys.MeshNode > meshDisplay.$renderNode;
            const hasFFD = this._ffdVertices.length > 0;

            if (this._meshData.skinned)
            {
                for (let i = 0, iF = 0, l = this._meshData.vertices.length; i < l; i += 2)
                {
                    let iH = i / 2;

                    const boneIndices = this._meshData.boneIndices[iH];
                    const boneVertices = this._meshData.boneVertices[iH];
                    const weights = this._meshData.weights[iH];

                    let xG = 0, yG = 0;

                    for (let iB = 0, lB = boneIndices.length; iB < lB; ++iB)
                    {
                        const bone = this._meshBones[boneIndices[iB]];
                        const matrix = bone.globalTransformMatrix;
                        const weight = weights[iB];

                        let xL = 0, yL = 0;
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

                    meshNode.vertices[i] = xG;
                    meshNode.vertices[i + 1] = yG;
                }

                meshDisplay.$updateVertices();
                meshDisplay.$invalidateTransform();
            }
            else if (hasFFD)
            {
                const vertices = this._meshData.vertices;
                for (let i = 0, l = this._meshData.vertices.length; i < l; i += 2)
                {
                    const xG = vertices[i] + this._ffdVertices[i];
                    const yG = vertices[i + 1] + this._ffdVertices[i + 1];
                    meshNode.vertices[i] = xG;
                    meshNode.vertices[i + 1] = yG;
                }

                meshDisplay.$updateVertices();
                meshDisplay.$invalidateTransform();
            }*/
        }

        /**
         * @private
         */
        override protected void _updateTransform()
        {
            var position = _renderDisplay.transform.position;
            position.x = this.globalTransformMatrix.tx - (this.globalTransformMatrix.a * this._pivotX + this.globalTransformMatrix.c * this._pivotY);
            position.y = this.globalTransformMatrix.ty - (this.globalTransformMatrix.b * this._pivotX + this.globalTransformMatrix.d * this._pivotY);
        }
    }
}
