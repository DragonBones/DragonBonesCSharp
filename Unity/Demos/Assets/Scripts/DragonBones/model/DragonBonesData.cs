using System;
using System.Collections.Generic;
using System.Text;

namespace DragonBones
{
    /**
     * 龙骨数据。
     * 一个龙骨数据包含多个骨架数据。
     * @see dragonBones.ArmatureData
     * @version DragonBones 3.0
     * @language zh_CN
     */
    public class DragonBonesData : BaseObject
    {
        /**
         * 是否开启共享搜索。
         * @default false
         * @version DragonBones 4.5
         * @language zh_CN
         */
        public bool autoSearch;
        /**
         * 动画帧频。
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public uint frameRate;
        /**
         * 数据版本。
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public string version;
        /**
         * 数据名称。(该名称与龙骨项目名保持一致)
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public string name;
        /**
         * @private
         */
        public ArmatureData stage;
        /**
         * @private
         */
        public readonly List<uint> frameIndices = new List<uint>();
        /**
         * @private
         */
        public readonly List<float> cachedFrames = new List<float>();
        /**
         * 所有骨架数据名称。
         * @see #armatures
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public readonly List<string> armatureNames = new List<string>();
        /**
         * 所有骨架数据。
         * @see dragonBones.ArmatureData
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public readonly Dictionary<string, ArmatureData> armatures = new Dictionary<string, ArmatureData>();
        /**
        * @private
        */
        internal byte[] binary;
        /**
         * @private
         */
        internal short[] intArray;
        /**
         * @private
         */
        internal float[] floatArray;
        /**
         * @private
         */
        internal short[] frameIntArray;
        /**
         * @private
         */
        internal float[] frameFloatArray;
        /**
         * @private
         */
        internal short[] frameArray;
        /**
         * @private
         */
        internal ushort[] timelineArray;
        /**
         * @private
         */
        internal UserData userData = null; // Initial value.

        protected override void _OnClear()
        {
            foreach (var k in this.armatures.Keys)
            {
                this.armatures[k].ReturnToPool();
            }

            if (this.userData != null)
            {
                this.userData.ReturnToPool();
            }

            this.autoSearch = false;
            this.frameRate = 0;
            this.version = "";
            this.name = "";
            this.stage = null;
            this.frameIndices.Clear();
            this.cachedFrames.Clear();
            this.armatureNames.Clear();
            this.armatures.Clear();
            this.binary = null;
            this.intArray = null; //
            this.floatArray = null; //
            this.frameIntArray = null; //
            this.frameFloatArray = null; //
            this.frameArray = null; //
            this.timelineArray = null; //
            this.userData = null;
        }

        /**
         * @private
         */
        public void AddArmature(ArmatureData value)
        {
            if (this.armatures.ContainsKey(value.name))
            {
                Helper.Assert(false, "Replace armature: " + value.name);
                this.armatures[value.name].ReturnToPool();
            }

            value.parent = this;
            this.armatures[value.name] = value;
            this.armatureNames.Add(value.name);
        }

        /**
         * 获取骨架数据。
         * @param name 骨架数据名称。
         * @see dragonBones.ArmatureData
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public ArmatureData GetArmature(string name)
        {
            return this.armatures.ContainsKey(name) ? this.armatures[name] : null;
        }
    }
}
