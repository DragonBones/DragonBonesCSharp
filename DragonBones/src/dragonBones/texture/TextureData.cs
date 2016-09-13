using System.Collections.Generic;

namespace dragonBones
{
    public abstract class TextureAtlasData : BaseObject
    {
        /**
         * @language zh_CN
         * 是否开启共享搜索。
         * @default false
         * @version DragonBones 4.5
         */
        public bool autoSearch;

        /**
         * @language zh_CN
         * 贴图集缩放系数。
         * @version DragonBones 3.0
         */
        public float scale;

        /**
         * @language zh_CN
         * 贴图集名称。
         * @version DragonBones 3.0
         */
        public string name;

        /**
         * @language zh_CN
         * 贴图集图片路径。
         * @version DragonBones 3.0
         */
        public string imagePath;

        /**
         * @private
         */
        public Dictionary<string, TextureData> textures = new Dictionary<string, TextureData>();

        /**
         * @private
         */
        public TextureAtlasData()
        {
        }

        /**
         * @inheritDoc
         */
        protected override void _onClear()
        {
            foreach (var pair in textures)
            {
                pair.Value.returnToPool();
            }

            autoSearch = false;
            scale = 1.0f;
            name = null;
            imagePath = null;
            textures.Clear();
        }

        /**
         * @private
         */
        public abstract TextureData generateTextureData();

        /**
         * @private
         */
        public void addTexture(TextureData value)
        {
            if (value != null && value.name != null && !textures.ContainsKey(value.name))
            {
                this.textures[value.name] = value;
                value.parent = this;
            }
            else
            {
                DragonBones.warn("");
            }
        }

        /**
         * @private
         */
        public TextureData getTexture(string name)
        {
            return textures[name];
        }
    }

    /**
     * @private
     */
    public abstract class TextureData : BaseObject
    {
        public static Rectangle generateRectangle()
        {
            return new Rectangle();
        }

        public bool rotated;
        public string name;
        public Rectangle frame;
        public TextureAtlasData parent;
        public Rectangle region = new Rectangle();

        public TextureData()
        {
        }

        /**
         * @inheritDoc
         */
        protected override void _onClear()
        {
            rotated = false;
            name = null;
            frame = null;
            parent = null;
            region.clear();
        }
    }
}