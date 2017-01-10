using System.Collections.Generic;

namespace DragonBones
{
    /**
     * @language zh_CN
     * 自定义数据。
     * @version DragonBones 5.0
     */
    public class CustomData : BaseObject
    {
        /**
         * @language zh_CN
         * 自定义整数。
         * @version DragonBones 5.0
         */
        public readonly List<int> ints = new List<int>();
        /**
         * @language zh_CN
         * 自定义浮点数。
         * @version DragonBones 5.0
         */
        public readonly List<float> floats = new List<float>();
        /**
         * @language zh_CN
         * 自定义字符串。
         * @version DragonBones 5.0
         */
        public readonly List<string> strings = new List<string>();
        /**
         * @private
         */
        public CustomData()
        {
        }
        /**
         * @private
         */
        protected override void _onClear()
        {
            ints.Clear();
            floats.Clear();
            strings.Clear();
        }
        /**
         * @language zh_CN
         * 获取自定义整数。
         * @version DragonBones 5.0
         */
        public int getInt(int index = 0)
        {
            return index >= 0 && index < ints.Count ? ints[index] : 0;
        }
        /**
         * @language zh_CN
         * 获取自定义浮点数。
         * @version DragonBones 5.0
         */
        public float getFloat(int index = 0)
        {
            return index >= 0 && index < floats.Count ? floats[index] : 0.0f;
        }
        /**
         * @language zh_CN
         * 获取自定义字符串。
         * @version DragonBones 5.0
         */
        public string getString(int index = 0)
        {
            return index >= 0 && index < strings.Count ? strings[index] : null;
        }
    }
    /**
     * @private
     */
    public class EventData : BaseObject
    {

        public EventType type;
        public string name;
        public BoneData bone;
        public SlotData slot;
        public CustomData data;

        public EventData()
        {
        }

        protected override void _onClear()
        {
            if (data != null)
            {
                data.ReturnToPool();
            }

            type = EventType.None;
            name = null;
            bone = null;
            slot = null;
            data = null;
        }
    }
    /**
     * @private
     */
    public class ActionData : BaseObject
    {
        public ActionType type;
        public BoneData bone;
        public SlotData slot;
        public AnimationConfig animationConfig;

        public ActionData()
        {
        }

        protected override void _onClear()
        {
            if (animationConfig != null)
            {
                animationConfig.ReturnToPool();
            }

            type = ActionType.Play;
            bone = null;
            slot = null;
            animationConfig = null;
        }
    }
    /**
     * @language zh_CN
     * 龙骨数据。
     * 一个龙骨数据包含多个骨架数据。
     * @see DragonBones.ArmatureData
     * @version DragonBones 3.0
     */
    public class DragonBonesData : BaseObject
    {
        /**
         * @language zh_CN
         * 是否开启共享搜索。
         * @default false
         * @see DragonBones.ArmatureData
         * @version DragonBones 4.5
         */
        public bool autoSearch;
        /**
         * @language zh_CN
         * 动画帧频。
         * @version DragonBones 3.0
         */
        public uint frameRate;
        /**
         * @language zh_CN
         * 数据名称。
         * @version DragonBones 3.0
         */
        public string name;
        /**
         * @language zh_CN
         * 所有骨架数据。
         * @see DragonBones.ArmatureData
         * @version DragonBones 3.0
         */
        public readonly Dictionary<string, ArmatureData> armatures = new Dictionary<string, ArmatureData>();
        /**
         * @private
         */
        public readonly List<float> cachedFrames = new List<float>();
        /**
         * @private
         */
        public CustomData userData;

        private readonly List<string> _armatureNames = new List<string>();
        /**
         * @private
         */
        public DragonBonesData()
        {
        }
        /**
         * @private
         */
        protected override void _onClear()
        {
            foreach (var pair in armatures)
            {
                pair.Value.ReturnToPool();
            }

            if (userData != null)
            {
                userData.ReturnToPool();
            }

            autoSearch = false;
            frameRate = 0;
            name = null;
            armatures.Clear();
            cachedFrames.Clear();
            userData = null;

            _armatureNames.Clear();
        }
        /**
         * @language zh_CN
         * 获取骨架。
         * @param name 骨架数据骨架名称。
         * @see DragonBones.ArmatureData
         * @version DragonBones 3.0
         */
        public ArmatureData GetArmature(string name)
        {
            return armatures.ContainsKey(name) ? armatures[name] : null;
        }
        /**
         * @private
         */
        public void AddArmature(ArmatureData value)
        {
            if (value != null && value.name != null && !armatures.ContainsKey(value.name))
            {
                armatures[value.name] = value;
                _armatureNames.Add(value.name);
                value.parent = this;
            }
            else
            {
                DragonBones.Assert(false, DragonBones.ARGUMENT_ERROR);
            }
        }
        /**
         * @language zh_CN
         * 所有骨架数据名称。
         * @see #armatures
         * @version DragonBones 3.0
         */
        public List<string> armatureNames
        {
            get { return _armatureNames; }
        }
    }
}
