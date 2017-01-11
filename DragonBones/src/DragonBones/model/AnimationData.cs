using System;
using System.Collections.Generic;

namespace DragonBones
{
    /**
     * @language zh_CN
     * 动画数据。
     * @version DragonBones 3.0
     */
    public class AnimationData : TimelineData<AnimationFrameData>
    {
        /**
         * @language zh_CN
         * 持续的帧数。
         * @version DragonBones 3.0
         */
        public uint frameCount;
        /**
         * @language zh_CN
         * 播放次数。 [0: 无限循环播放, [1~N]: 循环播放 N 次]
         * @version DragonBones 3.0
         */
        public uint playTimes;
        /**
         * @language zh_CN
         * 持续时间。 (以秒为单位)
         * @version DragonBones 3.0
         */
        public float duration;
        /**
         * @language zh_CN
         * 淡入时间。 (以秒为单位)
         * @version DragonBones 3.0
         */
        public float fadeInTime;
        /**
         * @private
         */
        public float cacheFrameRate;
        /**
         * @language zh_CN
         * 数据名称。
         * @version DragonBones 3.0
         */
        public string name;
        /**
         * @private
         */
        public readonly Dictionary<string, BoneTimelineData> boneTimelines = new Dictionary<string, BoneTimelineData>();
        /**
         * @private
         */
        public readonly Dictionary<string, SlotTimelineData> slotTimelines = new Dictionary<string, SlotTimelineData>();
        /**
         * @private
         */
        public readonly Dictionary<string, Dictionary<string, Dictionary<string, FFDTimelineData>>> ffdTimelines = new Dictionary<string, Dictionary<string, Dictionary<string, FFDTimelineData>>>(); // skin slot mesh
        /**
         * @private
         */
        public readonly List<bool> cachedFrames = new List<bool>();
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
        public ZOrderTimelineData zOrderTimeline;
        /**
         * @private
         */
        public AnimationData()
        {
        }
        /**
		 * @private
		 */
        protected override void _onClear()
        {
            base._onClear();

            foreach (var pair in boneTimelines)
            {
                pair.Value.ReturnToPool();
            }

            foreach (var pair in slotTimelines)
            {
                pair.Value.ReturnToPool();
            }

            foreach (var pair in ffdTimelines)
            {
                foreach (var pairA in pair.Value)
                {
                    foreach (var pairB in pairA.Value)
                    {
                        pairB.Value.ReturnToPool();
                    }
                }
            }
            
            frameCount = 0;
            playTimes = 0;
            duration = 0.0f;
            fadeInTime = 0.0f;
            cacheFrameRate = 0.0f;
            name = null;
            boneTimelines.Clear();
            slotTimelines.Clear();
            ffdTimelines.Clear();
            cachedFrames.Clear();
            boneCachedFrameIndices.Clear();
            slotCachedFrameIndices.Clear();
            zOrderTimeline = null;
        }
        /**
         * @private
         */
        public void CacheFrames(float frameRate)
        {
            if (cacheFrameRate > 0.0f)
            {
                return;
            }

            cacheFrameRate = Math.Max((float)Math.Ceiling(frameRate * scale), 1.0f);
            var cacheFrameCount = (int)Math.Ceiling(cacheFrameRate * duration) + 1;
            
            DragonBones.ResizeList(cachedFrames, 0, false);
            DragonBones.ResizeList(cachedFrames, cacheFrameCount, false);

            foreach (var k in boneTimelines.Keys)
            {
                var indices = new List<int>(cacheFrameCount);
                for (int i = 0, l = indices.Capacity; i < l; ++i)
                {
                    indices.Add(-1);
                }

                boneCachedFrameIndices[k] = indices;
            }

            foreach (var k in slotTimelines.Keys)
            {
                var indices = new List<int>(cacheFrameCount);
                for (int i = 0, l = indices.Capacity; i < l; ++i)
                {
                    indices.Add(-1);
                }

                slotCachedFrameIndices[k] = indices;
            }
        }
        /**
         * @private
         */
        public void AddBoneTimeline(BoneTimelineData value)
        {
            if (value != null && value.bone != null && !boneTimelines.ContainsKey(value.bone.name))
            {
                boneTimelines[value.bone.name] = value;
            }
            else
            {
                DragonBones.Assert(false, DragonBones.ARGUMENT_ERROR);
            }
        }
        /**
         * @private
         */
        public void AddSlotTimeline(SlotTimelineData value)
        {
            if (value != null && value.slot != null && !slotTimelines.ContainsKey(value.slot.name))
            {
                slotTimelines[value.slot.name] = value;
            }
            else
            {
                DragonBones.Assert(false, DragonBones.ARGUMENT_ERROR);
            }
        }
        /**
         * @private
         */
        public void AddFFDTimeline(FFDTimelineData value)
        {
            if (value != null && value.skin != null && value.slot != null)
            {
                var skin = ffdTimelines.ContainsKey(value.skin.name) ?
                    ffdTimelines[value.skin.name] :
                    (ffdTimelines[value.skin.name] = new Dictionary<string, Dictionary<string, FFDTimelineData>>());

                var slot = skin.ContainsKey(value.slot.slot.name) ?
                    skin[value.slot.slot.name] :
                    (skin[value.slot.slot.name] = new Dictionary<string, FFDTimelineData>());

                if (!slot.ContainsKey(value.display.name))
                {
                    slot[value.display.name] = value;
                }
                else
                {
                    DragonBones.Assert(false, DragonBones.ARGUMENT_ERROR);
                }
            }
            else
            {
                DragonBones.Assert(false, DragonBones.ARGUMENT_ERROR);
            }
        }
        /**
         * @private
         */
        public BoneTimelineData GetBoneTimeline(string name)
        {
            return boneTimelines.ContainsKey(name) ? boneTimelines[name] : null;
        }
        /**
         * @private
         */
        public SlotTimelineData GetSlotTimeline(string name)
        {
            return slotTimelines.ContainsKey(name) ? slotTimelines[name] : null;
        }
        /**
         * @private
         */
        public Dictionary<string, FFDTimelineData> GetFFDTimeline(string skinName, string slotName)
        {
            if (ffdTimelines.ContainsKey(skinName))
            {
                var skin = ffdTimelines[skinName];
                if (skin.ContainsKey(slotName))
                {
                    return skin.ContainsKey(slotName) ? skin[slotName] : null;
                }
            }

            return null;
        }
        /**
         * @private
         */
        public List<int> GetBoneCachedFrameIndices(string name)
        {
            return boneCachedFrameIndices.ContainsKey(name) ? boneCachedFrameIndices[name] : null;
        }
        /**
         * @private
         */
        public List<int> GetSlotCachedFrameIndices(string name)
        {
            return slotCachedFrameIndices.ContainsKey(name) ? slotCachedFrameIndices[name] : null;
        }
    }
}
