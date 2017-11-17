using System.Collections.Generic;

namespace DragonBones
{
    /**
     * 贴图集数据。
     * @version DragonBones 3.0
     * @language zh_CN
     */
    public abstract class TextureAtlasData : BaseObject
    {
        /**
         * 是否开启共享搜索。
         * @default false
         * @version DragonBones 4.5
         * @language zh_CN
         */
        public bool autoSearch;
        /**
         * @private
         */
        public uint width;
        /**
         * @private
         */
        public uint height;
        /**
         * 贴图集缩放系数。
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public float scale;
        /**
         * 贴图集名称。
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public string name;
        /**
         * 贴图集图片路径。
         * @version DragonBones 3.0
         * @language zh_CN
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
        protected override void _OnClear()
        {
            foreach (var value in this.textures.Values)
            {
                value.ReturnToPool();
            }

            this.autoSearch = false;
            this.width = 0;
            this.height = 0;
            this.scale = 1.0f;
            this.textures.Clear();
            this.name = "";
            this.imagePath = "";
        }
        /**
         * @private
         */
        public void CopyFrom(TextureAtlasData value)
        {
            this.autoSearch = value.autoSearch;
            this.scale = value.scale;
            this.width = value.width;
            this.height = value.height;
            this.name = value.name;
            this.imagePath = value.imagePath;

            foreach (var texture in this.textures.Values)
            {
                texture.ReturnToPool();
            }

            this.textures.Clear();

            foreach (var pair in value.textures)
            {
                var texture = CreateTexture();
                texture.CopyFrom(pair.Value);
                textures[pair.Key] = texture;
            }
        }
        /**
         * @private
         */
        public abstract TextureData CreateTexture();
        /**
         * @private
         */
        public void AddTexture(TextureData value)
        {
            if (value != null)
            {
                if (this.textures.ContainsKey(value.name))
                {
                    Helper.Assert(false, "Same texture: " + value.name);
                    this.textures[value.name].ReturnToPool();
                }

                value.parent = this;
                this.textures[value.name] = value;
            }
        }
        /**
         * @private
         */
        public TextureData GetTexture(string name)
        {
            return textures.ContainsKey(name) ? textures[name] : null;
        }
        
    }
    /**
     * @private
     */
    public abstract class TextureData : BaseObject
    {
        public static Rectangle CreateRectangle()
        {
            return new Rectangle();
        }

        public bool rotated;
        public string name;
        public readonly Rectangle region = new Rectangle();
        public TextureAtlasData parent;
        public Rectangle frame = null; // Initial value.

        protected override void _OnClear()
        {
            this.rotated = false;
            this.name = "";
            this.region.Clear();
            this.parent = null; //
            this.frame = null;
        }

        public void CopyFrom(TextureData value)
        {
            this.rotated = value.rotated;
            this.name = value.name;
            this.region.CopyFrom(value.region);
            this.parent = value.parent;

            if (this.frame == null && value.frame != null)
            {
                this.frame = TextureData.CreateRectangle();
            }
            else if (this.frame != null && value.frame == null)
            {
                this.frame = null;
            }

            if (this.frame != null && value.frame != null)
            {
                this.frame.CopyFrom(value.frame);
            }
        }
    }
}