using System;
using System.Collections.Generic;

namespace dragonBones
{
    /**
     * @language zh_CN
     * 动画数据。
     * @version DragonBones 3.0
     */
    public class AnimationData : TimelineData<AnimationFrameData>
    {
        /**
         * @private
         */
        public bool hasAsynchronyTimeline;

        /**
         * @language zh_CN
         * 持续的帧数。
         * @version DragonBones 3.0
         */
        public uint frameCount;

        /**
         * @language zh_CN
         * 循环播放的次数。 [0: 无限循环播放, [1~N]: 循环播放 N 次]
         * @version DragonBones 3.0
         */
        public uint playTimes;

        /**
         * @language zh_CN
         * 开始的时间。 (以秒为单位)
         * @version DragonBones 3.0
         */
        public float position;

        /**
         * @language zh_CN
         * 持续的时间。 (以秒为单位)
         * @version DragonBones 3.0
         */
        public float duration;

        /**
         * @language zh_CN
         * 淡入混合的时间。 (以秒为单位)
         * @version DragonBones 3.0
         */
        public float fadeInTime;

        /**
         * @private
         */
        public float cacheTimeToFrameScale;

        /**
         * @language zh_CN
         * 数据名称。
         * @version DragonBones 3.0
         */
        public string name;

        /**
         * @private
         */
        public AnimationData animation;

        /**
         * @private
         */
        public Dictionary<string, BoneTimelineData> boneTimelines = new Dictionary<string, BoneTimelineData>();

        /**
         * @private
         */
        public Dictionary<string, SlotTimelineData> slotTimelines = new Dictionary<string, SlotTimelineData>();

        /**
         * @private
         */
        public Dictionary<string, Dictionary<string, Dictionary<string, FFDTimelineData>>> ffdTimelines = new Dictionary<string, Dictionary<string, Dictionary<string, FFDTimelineData>>>(); // skin slot displayIndex

        /**
         * @private
         */
        public List<bool> cachedFrames = new List<bool>();

        /**
         * @private
         */
        public AnimationData()
        {
        }

        /**
         * @inheritDoc
         */
        protected override void _onClear()
        {
            base._onClear();

            foreach (var pair in boneTimelines)
            {
                pair.Value.returnToPool();
            }

            foreach (var pair in slotTimelines)
            {
                pair.Value.returnToPool();
            }

            foreach (var pair in ffdTimelines)
            {
                foreach (var pairA in pair.Value)
                {
                    foreach (var pairB in pairA.Value)
                    {
                        pairB.Value.returnToPool();
                    }
                }
            }

            hasAsynchronyTimeline = false;
            frameCount = 0;
            playTimes = 0;
            position = 0.0f;
            duration = 0.0f;
            fadeInTime = 0.0f;
            cacheTimeToFrameScale = 0.0f;
            name = null;
            animation = null;
            boneTimelines.Clear();
            slotTimelines.Clear();
            ffdTimelines.Clear();
            cachedFrames.Clear();
        }

        /**
         * @private
         */
        public void cacheFrames(float value)
        {
            if (animation != null)
            {
                return;
            }

            var cacheFrameCount = (uint)Math.Max(Math.Floor((frameCount + 1) * scale * value), 1);

            cacheTimeToFrameScale = cacheFrameCount / (duration + 0.000001f); //
            cachedFrames.Clear();
            // cachedFrames.Capacity = cacheFrameCount; // TODO

            foreach (var pari in boneTimelines)
            {
                pari.Value.cacheFrames(cacheFrameCount);
            }

            foreach (var pari in slotTimelines)
            {
                pari.Value.cacheFrames(cacheFrameCount);
            }
        }

        /**
         * @private
         */
        public void addBoneTimeline(BoneTimelineData value)
        {
            if (value != null && value.bone != null && !boneTimelines.ContainsKey(value.bone.name))
            {
                boneTimelines[value.bone.name] = value;
            }
            else
            {
                DragonBones.assert("");
            }
        }

        /**
         * @private
         */
        public void addSlotTimeline(SlotTimelineData value)
        {
            if (value != null && value.slot != null && !slotTimelines.ContainsKey(value.slot.name))
            {
                slotTimelines[value.slot.name] = value;
            }
            else
            {
                DragonBones.assert("");
            }
        }

        /**
         * @private
         */
        public void addFFDTimeline(FFDTimelineData value)
        {
            if (value != null && value.skin != null && value.slot != null)
            {
                var skin = ffdTimelines.ContainsKey(value.skin.name) ?
                    ffdTimelines[value.skin.name] :
                    (ffdTimelines[value.skin.name] = new Dictionary<string, Dictionary<string, FFDTimelineData>>());

                var slot = skin.ContainsKey(value.slot.slot.name) ?
                    skin[value.slot.slot.name] :
                    (skin[value.slot.slot.name] = new Dictionary<string, FFDTimelineData>());

                if (!slot.ContainsKey(value.displayIndex.ToString()))
                {
                    slot[value.displayIndex.ToString()] = value;
                }
                else
                {
                    DragonBones.assert("");
                }
            }
            else
            {
                DragonBones.assert("");
            }
        }

        /**
         * @private
         */
        public BoneTimelineData getBoneTimeline(string name)
        {
            return boneTimelines[name];
        }

        /**
         * @private
         */
        public SlotTimelineData getSlotTimeline(string name)
        {
            return slotTimelines[name];
        }

        /**
         * @private
         */
        public FFDTimelineData getFFDTimeline(string skinName, string slotName, int displayIndex)
        {
            if (ffdTimelines.ContainsKey(skinName))
            {
                var skin = ffdTimelines[skinName];
                if (skin.ContainsKey(slotName))
                {
                    var slot = skin[slotName];
                    return slot[displayIndex.ToString()];
                }
            }

            return null;
        }
    }
}
