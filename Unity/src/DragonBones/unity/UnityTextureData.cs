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
        public UnityTextureData()
        {
        }
    }
}