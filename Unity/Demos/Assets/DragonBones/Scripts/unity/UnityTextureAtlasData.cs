using System.Collections.Generic;
using UnityEngine;

namespace DragonBones
{
    /**
     * @language zh_CN
     * Unity 贴图集数据。
     * @version DragonBones 3.0
     */
    public class UnityTextureAtlasData : TextureAtlasData
    {
        /**
         * @private
         */
        internal bool _disposeEnabled;
        /**
         * @language zh_CN
         * Unity 贴图。
         * @version DragonBones 3.0
         */
        public Material texture;
		public Material uiTexture;
        /**
         * @private
         */
        public UnityTextureAtlasData()
        {
        }
        /**
         * @private
         */
        override protected void _OnClear()
        {
            base._OnClear();

            if (_disposeEnabled && texture != null)
            {
#if UNITY_EDITOR
                //Object.DestroyImmediate(texture);
#else
                Object.Destroy(texture);
#endif
            }

			if (_disposeEnabled && uiTexture != null)
			{
#if UNITY_EDITOR
				//Object.DestroyImmediate(uiTexture);
#else
				Object.Destroy(uiTexture);
#endif
			}

            _disposeEnabled = false;
            texture = null;
			uiTexture = null;
        }
        /**
         * @private
         */
        override public TextureData CreateTexture()
        {
            return BaseObject.BorrowObject<UnityTextureData>();
        }
    }

    /// <private/>
    internal class UnityTextureData : TextureData
    {
        /// <summary>
        /// 叠加模式材质球的缓存池
        /// </summary>
        internal Dictionary<string, Material> _cacheBlendModeMats = new Dictionary<string, Material>();

        public UnityTextureData()
        {
        }

        protected override void _OnClear()
        {
            base._OnClear();

            foreach (var key in this._cacheBlendModeMats.Keys)
            {
                var mat = this._cacheBlendModeMats[key];
                if (mat != null)
                {
#if UNITY_EDITOR
                //Object.DestroyImmediate(texture);
#else
                Object.Destroy(mat);
#endif
                }

                //this._cacheBlendModeMats[key] = null;
            }

            //
            this._cacheBlendModeMats.Clear();
        }

        private Material _GetMaterial(BlendMode blendMode)
        {
            //normal model, return the parent shareMaterial
            if (blendMode == BlendMode.Normal)
            {
                return (this.parent as UnityTextureAtlasData).texture;
            }

            var blendModeStr = blendMode.ToString();

            if (this._cacheBlendModeMats.ContainsKey(blendModeStr))
            {
                return this._cacheBlendModeMats[blendModeStr];
            }

            //framebuffer won't work in the editor mode
#if UNITY_EDITOR
            var newMaterial = new Material(Resources.Load<Shader>("BlendModes/Grab"));
#else
            var newMaterial = new Material(Resources.Load<Shader>("BlendModes/Framebuffer"));
#endif
            newMaterial.hideFlags = HideFlags.HideAndDontSave;
            newMaterial.mainTexture = (this.parent as UnityTextureAtlasData).texture.mainTexture;

            this._cacheBlendModeMats.Add(blendModeStr, newMaterial);

            return newMaterial;
        }

        private Material _GetUIMaterial(BlendMode blendMode)
        {
            //normal model, return the parent shareMaterial
            if (blendMode == BlendMode.Normal)
            {
                return (this.parent as UnityTextureAtlasData).uiTexture;
            }

            var blendModeStr = "UI_" + blendMode.ToString();

            if (this._cacheBlendModeMats.ContainsKey(blendModeStr))
            {
                return this._cacheBlendModeMats[blendModeStr];
            }

            //framebuffer won't work in the editor mode
#if UNITY_EDITOR
            var newMaterial = new Material(Resources.Load<Shader>("BlendModes/UIGrab"));
#else
            var newMaterial = new Material(Resources.Load<Shader>("BlendModes/UIFramebuffer"));
#endif
            newMaterial.hideFlags = HideFlags.HideAndDontSave;
            newMaterial.mainTexture = (this.parent as UnityTextureAtlasData).texture.mainTexture;

            this._cacheBlendModeMats.Add(blendModeStr, newMaterial);

            return newMaterial;
        }

        internal Material GetMaterial(BlendMode blendMode, bool isUGUI = false)
        {
            if (isUGUI)
            {
                return _GetUIMaterial(blendMode);
            }
            else
            {
                return _GetMaterial(blendMode);
            }
        }

        public override void CopyFrom(TextureData value)
        {
            base.CopyFrom(value);

            //
            (value as UnityTextureData)._cacheBlendModeMats = this._cacheBlendModeMats;
        }
    }
}