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
    /**
     * @private
     */
    internal class UnityTextureData : TextureData
    {
        internal Dictionary<BlendMode, Material> _blendModeMats = new Dictionary<BlendMode, Material>();

        public UnityTextureData()
        {
        }

        protected override void _OnClear()
        {
            base._OnClear();

            //
            this._blendModeMats.Clear();
        }

        internal Material GetMaterial(BlendMode blendMode)
        {
            //已经有了，就返回一个
            if (this._blendModeMats.ContainsKey(blendMode))
            {
                return this._blendModeMats[blendMode];
            }

            //理论上，找普通材质不应该走到这里，而是到父亲里找
            if (blendMode == BlendMode.Normal)
            {
                return (this.parent as UnityTextureAtlasData).texture;
            }

            //剩下的就是支持blendMode的材质
            var newMaterial = new Material();

            return null;
        }

        public override void CopyFrom(TextureData value)
        {
            base.CopyFrom(value);

            //
            (value as UnityTextureData)._blendModeMats = this._blendModeMats;
        }
    }
}