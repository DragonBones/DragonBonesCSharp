using System.Collections.Generic;

namespace DragonBones
{
    /**
     * 自定义数据。
     * @version DragonBones 5.0
     * @language zh_CN
     */
    public class UserData : BaseObject
    {
        /**
         * 自定义整数。
         * @version DragonBones 5.0
         * @language zh_CN
         */
        public readonly List<int> ints = new List<int>();
        /**
         * 自定义浮点数。
         * @version DragonBones 5.0
         * @language zh_CN
         */
        public readonly List<float> floats = new List<float>();
        /**
         * 自定义字符串。
         * @version DragonBones 5.0
         * @language zh_CN
         */
        public readonly List<string> strings = new List<string>();

        /**
         * @private
         */
        protected override void _onClear()
        {
            this.ints.Clear();
            this.floats.Clear();
            this.strings.Clear();
        }

        /**
         * 获取自定义整数。
         * @version DragonBones 5.0
         * @language zh_CN
         */
        public int getInt(int index = 0)
        {
            return index >= 0 && index < this.ints.Count ? this.ints[index] : 0;
        }
        /**
         * 获取自定义浮点数。
         * @version DragonBones 5.0
         * @language zh_CN
         */
        public float getFloat(int index = 0)
        {
            return index >= 0 && index < this.floats.Count ? this.floats[index] : 0.0f;
        }
        /**
         * 获取自定义字符串。
         * @version DragonBones 5.0
         * @language zh_CN
         */
        public string getString(int index = 0)
        {
            return index >= 0 && index < this.strings.Count ? this.strings[index] : string.Empty;
        }
    }

    public class ActionData : BaseObject
    {
        public ActionType type;
        public string name; // Frame event name | Sound event name | Animation name
        public BoneData bone;
        public SlotData slot;
        public UserData data;

        protected override void _onClear()
        {
            if (this.data != null)
            {
                this.data.ReturnToPool();
            }

            this.type = ActionType.Play;
            this.name = "";
            this.bone = null;
            this.slot = null;
            this.data = null;
        }
    }
}
