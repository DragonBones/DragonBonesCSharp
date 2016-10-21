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

        public Material material;

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

            if (texture != null)
            {
                //Object.Destroy(texture);
            }

            if (material != null)
            {
#if UNITY_EDITOR
                Object.DestroyImmediate(material);
#else
                Object.Destroy(metrial);
#endif
            }

            texture = null;
            material = null;
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
        public UnityTextureData()
        {
        }

        /**
         * @inheritDoc
         */
        override protected void _onClear()
        {
            base._onClear();
        }
    }
}