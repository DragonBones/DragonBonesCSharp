using System;
using System.Collections.Generic;

namespace DragonBones
{
    /**
     * 动画数据。
     * @version DragonBones 3.0
     * @language zh_CN
     */
    public class AnimationData : BaseObject
    {
        /**
         * @private
         */
        public uint frameIntOffset; // FrameIntArray.
        /**
         * @private
         */
        public uint frameFloatOffset; // FrameFloatArray.
        /**
         * @private
         */
        public uint frameOffset; // FrameArray.
        /**
         * 持续的帧数。 ([1~N])
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public uint frameCount;
        /**
         * 播放次数。 [0: 无限循环播放, [1~N]: 循环播放 N 次]
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public uint playTimes;
        /**
         * 持续时间。 (以秒为单位)
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public float duration;
        /**
         * @private
         */
        public float scale;
        /**
         * 淡入时间。 (以秒为单位)
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public float fadeInTime;
        /**
         * @private
         */
        public float cacheFrameRate;
        /**
         * 数据名称。
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public string name;
        /**
         * @private
         */
        public readonly List<bool> cachedFrames = new List<bool>();
        /**
         * @private
         */
        public readonly Dictionary<string, List<TimelineData>> boneTimelines = new Dictionary<string, List<TimelineData>>();
        /**
         * @private
         */
        public readonly Dictionary<string, List<TimelineData>> slotTimelines = new Dictionary<string, List<TimelineData>>();
        /**
         * @private
         */
        public readonly Dictionary<string, List<int>> boneCachedFrameIndices = new Dictionary<string, List<int>>();
        /**
         * @private
         */
        public readonly Dictionary<string, List<int>> slotCachedFrameIndices = new Dictionary<string, List<int>>();
        /**
         * @private
         */
        public TimelineData actionTimeline = null; // Initial value.
        /**
         * @private
         */
        public TimelineData zOrderTimeline = null; // Initial value.
        /**
         * @private
         */
        public ArmatureData parent;

        public AnimationData()
        {

        }

        protected override void _OnClear()
        {
            foreach (var pair in boneTimelines)
            {
                for (int i = 0; i < pair.Value.Count; ++i)
                {
                    pair.Value[i].ReturnToPool();
                }
            }

            foreach (var pair in slotTimelines)
            {
                for (int i = 0; i < pair.Value.Count; ++i)
                {
                    pair.Value[i].ReturnToPool();
                }
            }

            if (this.actionTimeline != null)
            {
                this.actionTimeline.ReturnToPool();
            }

            if (this.zOrderTimeline != null)
            {
                this.zOrderTimeline.ReturnToPool();
            }

            this.frameIntOffset = 0;
            this.frameFloatOffset = 0;
            this.frameOffset = 0;
            this.frameCount = 0;
            this.playTimes = 0;
            this.duration = 0.0f;
            this.scale = 1.0f;
            this.fadeInTime = 0.0f;
            this.cacheFrameRate = 0.0f;
            this.name = "";
            this.boneTimelines.Clear();
            this.slotTimelines.Clear();
            this.boneCachedFrameIndices.Clear();
            this.slotCachedFrameIndices.Clear();
            this.cachedFrames.Clear();

            this.actionTimeline = null;
            this.zOrderTimeline = null;
            this.parent = null;
        }

        public void CacheFrames(float frameRate)
        {
            if (this.cacheFrameRate > 0.0f)
            {
                // TODO clear cache.
                return;
            }

            this.cacheFrameRate = Math.Max((float)Math.Ceiling(frameRate * scale), 1.0f);
            var cacheFrameCount = (int)Math.Ceiling(this.cacheFrameRate * duration) + 1; // Cache one more frame.

            cachedFrames.ResizeList(0, false);
            cachedFrames.ResizeList(cacheFrameCount, false);

            foreach (var bone in this.parent.sortedBones)
            {
                var indices = new List<int>(cacheFrameCount);
                for (int i = 0, l = indices.Count; i < l; ++i)
                {
                    indices[i] = -1;
                }

                this.boneCachedFrameIndices[bone.name] = indices;
            }

            foreach (var slot in this.parent.sortedSlots)
            {
                var indices = new List<int>(cacheFrameCount);
                for (int i = 0, l = indices.Count; i < l; ++i)
                {
                    indices[i] = -1;
                }

                this.slotCachedFrameIndices[slot.name] = indices;
            }
        }

        /**
         * @private
         */
        public void AddBoneTimeline(BoneData bone, TimelineData tiemline)
        {
            if (bone == null || tiemline == null)
            {
                return;
            }

            if (!this.boneTimelines.ContainsKey(bone.name))
            {
                this.boneTimelines[bone.name] = new List<TimelineData>();
            }

            var timelines = this.boneTimelines[bone.name];
            if (!timelines.Contains(tiemline))
            {
                timelines.Add(tiemline);
            }
        }
        /**
         * @private
         */
        public void AddSlotTimeline(SlotData slot, TimelineData timeline)
        {
            if (slot == null || timeline == null)
            {
                return;
            }

            if (!this.slotTimelines.ContainsKey(slot.name))
            {
                this.slotTimelines[slot.name] = new List<TimelineData>();
            }

            var timelines = this.slotTimelines[slot.name];
            if (!timelines.Contains(timeline))
            {
                timelines.Add(timeline);
            }
        }

        /**
         * @private
         */
        public List<TimelineData> GetBoneTimeline(string name)
        {
            return this.boneTimelines.ContainsKey(name) ? this.boneTimelines[name] : null;
        }
        /**
         * @private
         */
        public List<TimelineData> GetSlotTimeline(string name)
        {
            return slotTimelines.ContainsKey(name) ? slotTimelines[name] : null;
        }
        /**
         * @private
         */
        public List<int> GetBoneCachedFrameIndices(string name)
        {
            return this.boneCachedFrameIndices.ContainsKey(name) ? this.boneCachedFrameIndices[name] : null;
        }

        /**
         * @private
         */
        public List<int> GetSlotCachedFrameIndices(string name)
        {
            return this.slotCachedFrameIndices.ContainsKey(name) ? this.slotCachedFrameIndices[name] : null;
        }
    }

    /**
     * @private
     */
    public class TimelineData : BaseObject
    {
        public TimelineType type;
        public uint offset; // TimelineArray.
        public int frameIndicesOffset; // FrameIndices.

        protected override void _OnClear()
        {
            this.type = TimelineType.BoneAll;
            this.offset = 0;
            this.frameIndicesOffset = -1;
        }
    }
}
