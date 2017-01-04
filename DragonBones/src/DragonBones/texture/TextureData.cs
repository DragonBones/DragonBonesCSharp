using System.Collections.Generic;

namespace DragonBones
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
         * @private
         */
        public float width;
        /**
         * @private
         */
        public float height;
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
        public readonly Dictionary<string, TextureData> textures = new Dictionary<string, TextureData>();
        /**
         * @private
         */
        public TextureAtlasData()
        {
        }
        /**
         * @private
         */
        protected override void _onClear()
        {
            foreach (var texture in textures.Values)
            {
                texture.ReturnToPool();
            }

            autoSearch = false;
            scale = 1.0f;
            width = 0.0f;
            height = 0.0f;
            name = null;
            imagePath = null;
            textures.Clear();
        }
        /**
         * @private
         */
        public abstract TextureData GenerateTextureData();
        /**
         * @private
         */
        public void AddTexture(TextureData value)
        {
            if (value != null && value.name != null && !textures.ContainsKey(value.name))
            {
                textures[value.name] = value;
                value.parent = this;
            }
            else
            {
                DragonBones.Assert(false, DragonBones.ARGUMENT_ERROR);
            }
        }
        /**
         * @private
         */
        public TextureData GetTexture(string name)
        {
            return textures.ContainsKey(name) ? textures[name] : null;
        }
        /**
         * @private
         */
        public void CopyFrom(TextureAtlasData value)
        {
            autoSearch = value.autoSearch;
            scale = value.scale;
            width = value.width;
            height = value.height;
            name = value.name;
            imagePath = value.imagePath;

            foreach (var texture in textures.Values)
            {
                texture.ReturnToPool();
            }

            textures.Clear();

            foreach (var pair in value.textures)
            {
                var texture = GenerateTextureData();
                texture.CopyFrom(pair.Value);
                textures[pair.Key] = texture;
            }
        }
    }
    /**
     * @private
     */
    public abstract class TextureData : BaseObject
    {
        public static Rectangle GenerateRectangle()
        {
            return new Rectangle();
        }

        public bool rotated;
        public string name;
        public readonly Rectangle region = new Rectangle();
        public Rectangle frame;
        public TextureAtlasData parent;

        public TextureData()
        {
        }
        
        protected override void _onClear()
        {
            rotated = false;
            name = null;
            region.Clear();
            frame = null;
            parent = null;
        }

        public void CopyFrom(TextureData value)
        {
            rotated = value.rotated;
            name = value.name;

            if (frame == null && value.frame == null)
            {
                frame = GenerateRectangle();
            }
            else if (frame != null && value.frame == null)
            {
                frame = null;
            }

            if (frame != null && value.frame != null)
            {
                frame.CopyFrom(value.frame);
            }

            parent = value.parent;
            region.CopyFrom(value.region);
        }
    }
}