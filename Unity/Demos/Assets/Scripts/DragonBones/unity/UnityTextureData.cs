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
         * @language zh_CN
         * Unity 贴图。
         * @version DragonBones 3.0
         */
        public Texture2D texture;

        /**
         * @private
         */
        public UnityTextureAtlasData()
        {
        }

        /**
         * @inheritDoc
         */
        override protected void _onClear()
        {
            base._onClear();

            if (texture)
            {
                //Object.Destroy(texture);
                texture = null;
            }
        }

        /**
         * @private
         */
        override public TextureData GenerateTextureData()
        {
            return BaseObject.BorrowObject<UnityTextureData>();
        }
    }

    /**
     * @private
     */
    public class UnityTextureData : TextureData
    {
        public Sprite texture;
        public Material material;

        public UnityTextureData()
        {
        }

        /**
         * @inheritDoc
         */
        override protected void _onClear()
        {
            base._onClear();

            if (texture != null)
            {
#if UNITY_EDITOR
                Object.DestroyImmediate(texture);
#else
                Object.Destroy(texture);
#endif
                texture = null;
            }

            if (material)
            {
#if UNITY_EDITOR
                Object.DestroyImmediate(material);
#else
                Object.Destroy(metrial);
#endif
                material = null;
            }
        }
    }
}