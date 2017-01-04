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
        public Material texture;
        /**
         * @private
         */
        public UnityTextureAtlasData()
        {
        }
        /**
         * @private
         */
        override protected void _onClear()
        {
            base._onClear();

            if (texture != null)
            {
                //Object.Destroy(texture);
            }

            texture = null;
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
    }
}