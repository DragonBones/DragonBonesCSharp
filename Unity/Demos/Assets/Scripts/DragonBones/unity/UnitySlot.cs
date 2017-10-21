using UnityEngine;

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
        private static Vector3 _helpVector3 = new Vector3();
        private static readonly Vector2[] _helpVector2s = { new Vector2(), new Vector2(), new Vector2(), new Vector2() };

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

		public Mesh mesh
        {
			get { return _mesh;}
		}

		public MeshRenderer meshRenderer
        {
			get{ return _renderer;}
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
			get{ return _renderDisplay; }
		}

		public UnityArmatureComponent proxy
        {
			get{ return _proxy;}
		}

        /**
         * @private
         */
        public UnitySlot()
        {
        }

        /**
         * @private
         */
        override protected void _OnClear()
        {
            base._OnClear();

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
			_vertices2 = null;
            _normals = null;
            _colors = null;
        }
        /**
         * @private
         */
        override protected void _InitDisplay(object value)
        {
        }
        /**
         * @private
         */
        override protected void _DisposeDisplay(object value)
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
        override protected void _OnUpdateDisplay()
        {
            _renderDisplay = (_display != null ? _display : _rawDisplay) as GameObject;
        }
        /**
         * @private
         */
        override protected void _AddDisplay()
        {
            _proxy = _armature.proxy as UnityArmatureComponent;
			var container = _proxy.slotsRoot;
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
        override protected void _ReplaceDisplay(object value)
        {
            var container = _proxy.slotsRoot;
            var prevDisplay = value as GameObject;
			int index = prevDisplay.transform.GetSiblingIndex();
            prevDisplay.hideFlags = HideFlags.HideInHierarchy;
            prevDisplay.transform.SetParent(null, false);
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
        override protected void _RemoveDisplay()
        {
            _renderDisplay.transform.parent = null;
        }
        /**
         * @private
         */
        override protected void _UpdateZOrder()
        {
            _helpVector3.Set(_renderDisplay.transform.localPosition.x, _renderDisplay.transform.localPosition.y, -_zOrder * (_proxy.zSpace + 0.001f));
			if(_renderDisplay.transform.localPosition.z != _helpVector3.z)
            {
				_proxy.zorderIsDirty = true;
			}
            
            _SetZorder(_helpVector3);
        }

        /**
         * @private
         */
        private void _SetZorder(Vector3 zorderPos)
        {
#if UNITY_5_6_OR_NEWER
            var sortingGroup = _proxy.GetComponent<UnityEngine.Rendering.SortingGroup>();
            if (sortingGroup == null)
            {
                sortingGroup = _proxy.GetComponent<UnityEngine.Rendering.SortingGroup>();
            }

            //childArmature zorder
            if (_childArmature != null)
            {
                var childProxy = _childArmature.proxy as UnityArmatureComponent;
                var childSortingGroup = childProxy.GetComponent<UnityEngine.Rendering.SortingGroup>();
                if (childSortingGroup != null)
                {
                    childSortingGroup.sortingOrder = _zOrder;
                }
            }
#endif
            if (_renderDisplay != null)
            {
                _renderDisplay.transform.localPosition = zorderPos;
                if (!_proxy.isUGUI)
                {
                    if (_renderer == null)
                    {
                        _renderer = _renderDisplay.AddComponent<MeshRenderer>();
                    }

                    _renderer.sortingOrder = _zOrder;
                }
            }
        }

        /**
         * @private
         */
        override internal void _UpdateVisible()
        {
            _renderDisplay.SetActive(_parent.visible);
        }
        /**
         * @private
         */
        override protected void _UpdateBlendMode()
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
        override protected void _UpdateColor()
        {
            if (_mesh != null)
            {
                if (_colors == null || _colors.Length != _mesh.vertexCount)
                {
                    _colors = new Color32[_mesh.vertexCount];
                }

                for (int i = 0, l = _mesh.vertexCount; i < l; ++i)
                {
                    _colors[i].r = (byte)(_colorTransform.redMultiplier * 255);
                    _colors[i].g = (byte)(_colorTransform.greenMultiplier * 255);
                    _colors[i].b = (byte)(_colorTransform.blueMultiplier * 255);
                    _colors[i].a = (byte)(_colorTransform.alphaMultiplier * 255);
                }
                _mesh.colors32 = _colors;
            }
        }
        /**
         * @private
         */
        protected override void _UpdateFrame()
        {
            var meshData = this._display == this._meshDisplay ? this._meshData : null;
            var currentTextureData = this._textureData as UnityTextureData;

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

                _meshFilter = _renderDisplay.GetComponent<MeshFilter>();
                if (_meshFilter == null)
                {
                    _meshFilter = _renderDisplay.AddComponent<MeshFilter>();
                }
            }

            if (this._displayIndex >= 0 && this._display != null && currentTextureData != null)
            {
                if (this._armature.replacedTexture != null && this._rawDisplayDatas.Contains(this._displayData))
                {
                    var currentTextureAtlasData = currentTextureData.parent as UnityTextureAtlasData;
                    if (this._armature._replaceTextureAtlasData == null)
                    {
                        currentTextureAtlasData = BaseObject.BorrowObject<UnityTextureAtlasData>();
                        currentTextureAtlasData.CopyFrom(currentTextureData.parent);

                        if (_proxy.isUGUI)
                        {
                            currentTextureAtlasData.uiTexture = _armature.replacedTexture as Material;
                        }
                        else
                        {
                            currentTextureAtlasData.texture = _armature.replacedTexture as Material;
                        }

                        this._armature._replaceTextureAtlasData = currentTextureAtlasData;
                    }
                    else
                    {
                        currentTextureAtlasData = this._armature._replaceTextureAtlasData as UnityTextureAtlasData;
                    }

                    currentTextureData = currentTextureAtlasData.GetTexture(currentTextureData.name) as UnityTextureData;
                }
                
                var currentTextureAtlas = _proxy.isUGUI ? currentTextureAtlasData.uiTexture : currentTextureAtlasData.texture;
                if (currentTextureAtlas != null)
                {
                    var textureAtlasWidth = currentTextureAtlasData.width > 0.0f ? (int)currentTextureAtlasData.width : currentTextureAtlas.mainTexture.width;
                    var textureAtlasHeight = currentTextureAtlasData.height > 0.0f ? (int)currentTextureAtlasData.height : currentTextureAtlas.mainTexture.height;
                    
                    if (_mesh == null)
                    {
                        _mesh = new Mesh();
                        _mesh.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
                        _mesh.MarkDynamic();
                    }

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
                                xL += this._ffdVertices[iF++] * scale;
                                yL += this._ffdVertices[iF++] * scale;
                            }

                            xG += (matrix.a * xL + matrix.c * yL + matrix.tx) * weight; /** (1.0f / scale)) * weight;*/
                            yG += (matrix.b * xL + matrix.d * yL + matrix.ty) * weight; /** (1.0f / scale)) * weight;*/
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

                Vector3[] vertices = new Vector3[vertextCount];
                for (int i = 0, iV = 0, iF = 0, l = vertextCount; i < l; ++i)
                {
                    _vertices[i].x = (floatArray[vertexOffset + (iV++)] * scale + this._ffdVertices[iF++])/* * scale*/;
                    _vertices[i].y = - (floatArray[vertexOffset + (iV++)] * scale + this._ffdVertices[iF++])/* * scale*/;
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
                // Identity transform.
                _helpVector3.x = 0.0f;
                _helpVector3.y = 0.0f;
                _helpVector3.z = 0.0f;

                var transform = _renderDisplay.transform;

                transform.localPosition = new Vector3(0.0f, 0.0f, transform.localPosition.z);
                transform.localEulerAngles = _helpVector3;
                transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            }
            else
            {
                this.UpdateGlobalTransform(); // Update transform.
                //QQ
                //if (this.name.Contains("C01_left_leg08"))
                //{
                //    UnityEngine.Debug.Log("---------------------" + "solt name:" + this.name + "---------------------");
                //    UnityEngine.Debug.Log("slot rotation:" + global.rotation + " solt skew:" + global.skew);
                //}

                var flipX = _armature.flipX;
                var flipY = _armature.flipY;
                var transform = _renderDisplay.transform;

                _helpVector3.x = global.x;
                _helpVector3.y = global.y;
                _helpVector3.z = transform.localPosition.z;

                transform.localPosition = _helpVector3;

                _helpVector3.x = 0.0f;
                _helpVector3.y = 0.0f;
                _helpVector3.z = global.rotation * Transform.RAD_DEG;

                if (flipX != flipY)
                {
                    _helpVector3.z = -_helpVector3.z;
                    if (flipX)
                    {
                        _helpVector3.y = 180.0f;
                        _helpVector3.z -= 180.0f;
                    }

                    if (flipY)
                    {
                        _helpVector3.x = 180.0f;
                    }
                }
                
                transform.localEulerAngles = _helpVector3;

                //Modify mesh skew. // TODO child armature skew.
                if ((_display == _rawDisplay || _display == _meshDisplay) && _mesh != null)
                {
                    //var dSkew = global.skew;

                    //dSkew = Mathf.RoundToInt(dSkew * 100) % Mathf.RoundToInt(Transform.PI * 100);
                    //dSkew = dSkew / 100.0f;

                    //var skewed = dSkew < -0.01f || 0.01f < dSkew;
                    //if (skewed)
                    //{
                    //    skewed = Transform.PI - Mathf.Abs(dSkew) > 0.01f;
                    //}

                    //_skewed = false;
                    //skewed = false;
                    //var skewed = (dSkew > -0.01f && 0.01f > dSkew) && global.rotation != 0.0f;

                    var dSkew = global.skew - global.rotation;
                    var skewed = dSkew < -0.01f || 0.01f < dSkew;
                    var isSkewed = _skewed || skewed;
                    //TODO
                    isSkewed = false;
                    if (isSkewed)
                    {
                        _skewed = skewed;
                        //dSkew = global.skew - global.rotation;

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
                
                _helpVector3.x = global.scaleX;
                _helpVector3.y = global.scaleY;
                _helpVector3.z = 1.0f;

                transform.localScale = _helpVector3;
            }

            if (childArmature != null)
            {
                //childArmature.flipX = _proxy.armature.flipX;
                //childArmature.flipY = _proxy.armature.flipY;
                UnityArmatureComponent unityArmature = (childArmature.proxy as UnityArmatureComponent);
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
    }
}