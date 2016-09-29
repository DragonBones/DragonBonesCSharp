using UnityEngine;

namespace dragonBones
{
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
                //this.texture.dispose();
                texture = null;
            }
        }

        /**
         * @private
         */
        override public TextureData generateTextureData()
        {
            return BaseObject.borrowObject<UnityTextureData>();
        }
    }

    /**
     * @private
     */
    public class UnityTextureData : TextureData
    {
        public Sprite texture;

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
                Object.Destroy(texture);
                texture = null;
            }
        }
    }
}