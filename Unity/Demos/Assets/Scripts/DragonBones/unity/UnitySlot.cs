using System.Collections.Generic;
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

		public Mesh mesh{
			get { return _mesh;}
		}
		public MeshRenderer meshRenderer{
			get{ return _renderer;}
		}
		public UnityTextureAtlasData currentTextureAtlasData{
			get{ 
				if(_textureData==null || _textureData.parent==null) return null;
				return _textureData.parent as UnityTextureAtlasData;
			}
		}
		public GameObject renderDisplay{
			get{ return _renderDisplay; }
		}
		public UnityArmatureComponent proxy{
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
			_vertices2 = null;
			_normals = null;
			_colors = null;
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
			var container = _proxy.slotsRoot;
            if (_renderDisplay.transform.parent != container.transform)
            {
				_renderDisplay.transform.SetParent(container.transform);

                _helpVector3.Set(0.0f, 0.0f, -_zOrder * (_proxy.zSpace + 0.001f));
                _renderDisplay.transform.localPosition = _helpVector3;
            }
        }
        /**
         * @private
         */
        override protected void _replaceDisplay(object value)
        {
			var container = _proxy.slotsRoot;
            var prevDisplay = value as GameObject;
			int index = prevDisplay.transform.GetSiblingIndex();
            prevDisplay.hideFlags = HideFlags.HideInHierarchy;
			prevDisplay.transform.SetParent(null);
            prevDisplay.SetActive(false);

            _renderDisplay.hideFlags = HideFlags.None;
			_renderDisplay.transform.SetParent(container.transform);
            _renderDisplay.transform.localPosition = prevDisplay.transform.localPosition;
			_renderDisplay.transform.SetSiblingIndex(index);
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
            _helpVector3.Set(_renderDisplay.transform.localPosition.x, _renderDisplay.transform.localPosition.y, -_zOrder * (_proxy.zSpace + 0.001f));
			if(_renderDisplay.transform.localPosition.z!=_helpVector3.z){
				_proxy.zorderIsDirty=true;
			}
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
			if(_mesh!=null)
			{
				if(_colors==null || _colors.Length!=_mesh.vertexCount){
					_colors = new Color32[_mesh.vertexCount];
				}
				for (int i = 0, l = _mesh.vertexCount; i < l; ++i)
				{
					_colors[i].r = (byte)(_colorTransform.redMultiplier*255);
					_colors[i].g = (byte)(_colorTransform.greenMultiplier*255);
					_colors[i].b = (byte)(_colorTransform.blueMultiplier*255);
					_colors[i].a = (byte)(_colorTransform.alphaMultiplier*255);
				}
				_mesh.colors32 = _colors;
			}
        }
        /**
         * @private
         */
        override protected void _updateFrame()
        {
            var isMeshDisplay = _meshData != null && _display == _meshDisplay;
            var currentTextureData = _textureData as UnityTextureData;

			if(_proxy.isUGUI){
				_uiDisplay = _renderDisplay.GetComponent<UnityUGUIDisplay>();
				if(_uiDisplay==null){
					_uiDisplay = _renderDisplay.AddComponent<UnityUGUIDisplay>();
					_uiDisplay.raycastTarget = false;
				}
			}else{
				_renderer = _renderDisplay.GetComponent<MeshRenderer>();
				if(_renderer==null){
					_renderer = _renderDisplay.AddComponent<MeshRenderer>();
				}
				_meshFilter = _renderDisplay.GetComponent<MeshFilter>();
				if(_meshFilter==null){
					_meshFilter = _renderDisplay.AddComponent<MeshFilter>();
				}
			}

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
						if(_proxy.isUGUI){
							currentTextureAtlasData.uiTexture = _armature.replacedTexture as Material;
						}else{
                        	currentTextureAtlasData.texture = _armature.replacedTexture as Material;
						}
                        _armature._replaceTextureAtlasData = currentTextureAtlasData;
                    }

                    currentTextureData = currentTextureAtlasData.GetTexture(currentTextureData.name) as UnityTextureData;
                }

				var currentTextureAtlas = _proxy.isUGUI?currentTextureAtlasData.uiTexture :currentTextureAtlasData.texture;
                if (currentTextureAtlas != null)
                {
                    var textureAtlasWidth = currentTextureAtlasData.width > 0.0f ? currentTextureAtlasData.width : currentTextureAtlas.mainTexture.width;
                    var textureAtlasHeight = currentTextureAtlasData.height > 0.0f ? currentTextureAtlasData.height : currentTextureAtlas.mainTexture.height;

					if(_mesh==null){
	                    _mesh = new Mesh();
						_mesh.MarkDynamic();
					}else{
						_mesh.Clear();
						_mesh.uv = null;
						_mesh.vertices = null;
						_mesh.normals = null;
						_mesh.triangles = null;
						_mesh.colors32 = null;
					}

                    if (isMeshDisplay) // Mesh.
                    {
						if(_uvs==null || _uvs.Length!=_meshData.uvs.Count / 2){
                        	_uvs = new Vector2[_meshData.uvs.Count / 2];
						}
						if(_vertices==null || _vertices.Length!=_meshData.vertices.Count / 2){
	                        _vertices = new Vector3[_meshData.vertices.Count / 2];
							_vertices2 = new Vector3[_vertices.Length];
						}

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
							_vertices2[iN] = _vertices[iN];
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
							_vertices2 = new Vector3[4];
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
							_vertices2[i] = _vertices[i];
                        }

                        _mesh.vertices = _vertices; // Must set vertices before uvs.
                        _mesh.uv = _helpVector2s;
                        _mesh.triangles = TRIANGLES;
					}

					if(_proxy.isUGUI){
						_uiDisplay.material = currentTextureAtlas;
						_uiDisplay.texture = currentTextureAtlas.mainTexture;
						_mesh.RecalculateBounds();
						_uiDisplay.sharedMesh = _mesh;
					}else{
						if(_renderer.enabled){
							_mesh.RecalculateBounds();
						}
						_meshFilter.sharedMesh = _mesh;
						_renderer.sharedMaterial = currentTextureAtlas;
					}

                    _updateVisible();

                    return;
                }
            }

            _renderDisplay.SetActive(false);
			if(_proxy.isUGUI){
				_uiDisplay.material = null;
				_uiDisplay.texture = null;
				_uiDisplay.sharedMesh = null;
			}else{
				_meshFilter.sharedMesh = null;
				_renderer.sharedMaterial = null;
			}
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
					_vertices2[iH].x = xG;
					_vertices2[iH].y = -yG;
                }

				_mesh.vertices = _vertices;
				if(_renderer && _renderer.enabled) _mesh.RecalculateBounds();
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
					_vertices2[iH].x = xG;
					_vertices2[iH].y = -yG;
                }

				_mesh.vertices = _vertices;
				if(_renderer && _renderer.enabled)  _mesh.RecalculateBounds();

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
						if(_renderer && _renderer.enabled) _mesh.RecalculateBounds();
                    }
                }

                _helpVector3.x = scaleX >= 0.0f ? scaleX : -scaleX;
                _helpVector3.y = scaleY >= 0.0f ? scaleY : -scaleY;
                _helpVector3.z = 1.0f;

				transform.localScale = _helpVector3;
            }

			if(childArmature!=null){
				childArmature.flipX = _proxy.armature.flipX;
				childArmature.flipY = _proxy.armature.flipY;
				UnityArmatureComponent unityArmature = (childArmature.eventDispatcher as UnityArmatureComponent);
				unityArmature.addNormal = _proxy.addNormal;
				unityArmature.boneHierarchy = _proxy.boneHierarchy;
			}

			UpdateNormal();
        }

		public void UpdateNormal(){
			if(_mesh!=null){
				if(_proxy.addNormal){
					var flipX = armature.flipX?1f:-1f;
					var flipY = armature.flipY?1f:-1f;
					float normalZ = -flipX*flipY;
					if(_normals==null||_normals.Length!=_mesh.vertexCount)
					{
						_normals = new Vector3[_mesh.vertexCount];
						_normalVal.z = 0f;
					}
					if(normalZ!=_normalVal.z){
						_normalVal.z = normalZ;
						for(int i=0;i<_mesh.vertexCount;++i){
							_normals[i] = _normalVal;
						}
						_mesh.normals = _normals;
					}
				}else{
					_normals = null;
					_normalVal.z = 0f;
				}
			}
			else{
				_normals = null;
				_normalVal.z = 0f;
			}
		}
    }
}