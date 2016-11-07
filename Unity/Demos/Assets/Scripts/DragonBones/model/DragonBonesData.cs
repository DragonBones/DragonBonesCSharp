using System.Collections.Generic;

namespace DragonBones
{
    public class DragonBonesData : BaseObject
    {
        /**
         * @language zh_CN
         * 是否开启共享搜索。
         * @default false
         * @see dragonBones.ArmatureData
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
         * 所有的骨架数据。
         * @see dragonBones.ArmatureData
         * @version DragonBones 3.0
         */
        public readonly Dictionary<string, ArmatureData> armatures = new Dictionary<string, ArmatureData>();

        private readonly List<string> _armatureNames = new List<string>();

        /**
         * @private
         */
        public DragonBonesData()
        {
        }

        /**
         * @inheritDoc
         */
        protected override void _onClear()
        {
            foreach (var pair in armatures)
            {
                pair.Value.ReturnToPool();
            }

            autoSearch = false;
            frameRate = 0;
            name = null;
            armatures.Clear();

            _armatureNames.Clear();
        }

        /**
         * @language zh_CN
         * 获取指定名称的骨架。
         * @param name 骨架数据骨架名称。
         * @see dragonBones.ArmatureData
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
                DragonBones.Warn("");
            }
        }

        /**
         * @language zh_CN
         * 所有的骨架数据名称。
         * @see #armatures
         * @version DragonBones 3.0
         */
        public List<string> armatureNames
        {
            get { return _armatureNames; }
        }
    }
}
