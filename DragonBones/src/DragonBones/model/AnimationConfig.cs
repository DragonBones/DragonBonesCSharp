using System.Collections.Generic;

namespace DragonBones
{
    /**
     * @language zh_CN
     * @beta
     * 动画配置，描述播放一个动画所需要的全部信息。
     * @see DragonBones.AnimationState
     * @version DragonBones 5.0
     */
    public class AnimationConfig : BaseObject
    {
        /**
         * @language zh_CN
         * 是否暂停淡出的动画。
         * @default true
         * @version DragonBones 5.0
         */
        public bool pauseFadeOut;
        /**
         * @language zh_CN
         * 淡出模式。
         * @default dragonBones.AnimationFadeOutMode.All
         * @see DragonBones.AnimationFadeOutMode
         * @version DragonBones 5.0
         */
        public AnimationFadeOutMode fadeOutMode;
        /**
         * @language zh_CN
         * 淡出时间。 [-1: 与淡入时间同步, [0~N]: 淡出时间] (以秒为单位)
         * @default -1
         * @version DragonBones 5.0
         */
        public float fadeOutTime;
        /**
         * @language zh_CN
         * 淡出缓动方式。
         * @default 0
         * @version DragonBones 5.0
         */
        public float fadeOutEasing;
        /**
         * @language zh_CN
         * 是否以增加的方式混合。
         * @default false
         * @version DragonBones 5.0
         */
        public bool additiveBlending;
        /**
         * @language zh_CN
         * 是否对插槽的显示对象有控制权。
         * @default true
         * @version DragonBones 5.0
         */
        public bool displayControl;
        /**
         * @language zh_CN
         * 是否暂停淡入的动画，直到淡入过程结束。
         * @default true
         * @version DragonBones 5.0
         */
        public bool pauseFadeIn;
        /**
         * @language zh_CN
         * 否能触发行为。
         * @default true
         * @version DragonBones 5.0
         */
        public bool actionEnabled;
        /**
         * @language zh_CN
         * 播放次数。 [-1: 使用动画数据默认值, 0: 无限循环播放, [1~N]: 循环播放 N 次]
         * @default -1
         * @version DragonBones 5.0
         */
        public int playTimes;
        /**
         * @language zh_CN
         * 混合图层，图层高会优先获取混合权重。
         * @default 0
         * @version DragonBones 5.0
         */
        public int layer;
        /**
         * @language zh_CN
         * 开始时间。 (以秒为单位)
         * @default 0
         * @version DragonBones 5.0
         */
        public float position;
        /**
         * @language zh_CN
         * 持续时间。 [-1: 使用动画数据默认值, 0: 动画停止, (0~N]: 持续时间] (以秒为单位)
         * @default -1
         * @version DragonBones 5.0
         */
        public float duration;
        /**
         * @language zh_CN
         * 播放速度。 [(-N~0): 倒转播放, 0: 停止播放, (0~1): 慢速播放, 1: 正常播放, (1~N): 快速播放]
         * @default 1
         * @version DragonBones 3.0
         */
        public float timeScale;
        /**
         * @language zh_CN
         * 淡入时间。 [-1: 使用动画数据默认值, [0~N]: 淡入时间] (以秒为单位)
         * @default -1
         * @version DragonBones 5.0
         */
        public float fadeInTime;
        /**
         * @language zh_CN
         * 自动淡出时间。 [-1: 不自动淡出, [0~N]: 淡出时间] (以秒为单位)
         * @default -1
         * @version DragonBones 5.0
         */
        public float autoFadeOutTime;
        /**
         * @language zh_CN
         * 淡入缓动方式。
         * @default 0
         * @version DragonBones 5.0
         */
        public float fadeInEasing;
        /**
         * @language zh_CN
         * 权重。
         * @default 1
         * @version DragonBones 5.0
         */
        public float weight;
        /**
         * @language zh_CN
         * 动画状态名。
         * @version DragonBones 5.0
         */
        public string name;
        /**
         * @language zh_CN
         * 动画数据名。
         * @version DragonBones 5.0
         */
        public string animationName;
        /**
         * @language zh_CN
         * 混合组，用于动画状态编组，方便控制淡出。
         * @version DragonBones 5.0
         */
        public string group;
        /**
         * @language zh_CN
         * 骨骼遮罩。
         * @version DragonBones 5.0
         */
        public readonly List<string> boneMask = new List<string>();
        /**
         * @private
         */
        public AnimationConfig()
        {
        }
        /**
         * @private
         */
        protected override void _onClear()
        {
            pauseFadeOut = true;
            fadeOutMode = AnimationFadeOutMode.All;
            fadeOutTime = -1.0f;
            fadeOutEasing = 0.0f;

            additiveBlending = false;
            displayControl = true;
            pauseFadeIn = true;
            actionEnabled = true;
            playTimes = -1;
            layer = 0;
            position = 0.0f;
            duration = -1.0f;
            timeScale = -100.0f;
            fadeInTime = -1.0f;
            autoFadeOutTime = -1.0f;
            fadeInEasing = 0.0f;
            weight = 1.0f;
            name = null;
            animationName = null;
            group = null;
            boneMask.Clear();
        }

        public void Clear()
        {
            _onClear();
        }

        public void CopyFrom(AnimationConfig value)
        {
            pauseFadeOut = value.pauseFadeOut;
            fadeOutMode = value.fadeOutMode;
            autoFadeOutTime = value.autoFadeOutTime;
            fadeOutEasing = value.fadeOutEasing;

            additiveBlending = value.additiveBlending;
            displayControl = value.displayControl;
            pauseFadeIn = value.pauseFadeIn;
            actionEnabled = value.actionEnabled;
            playTimes = value.playTimes;
            layer = value.layer;
            position = value.position;
            duration = value.duration;
            timeScale = value.timeScale;
            fadeInTime = value.fadeInTime;
            fadeOutTime = value.fadeOutTime;
            fadeInEasing = value.fadeInEasing;
            weight = value.weight;
            name = value.name;
            animationName = value.animationName;
            group = value.group;

            DragonBones.ResizeList(boneMask, value.boneMask.Count, null);
            for (int i = 0, l = boneMask.Count; i < l; ++i)
            {
                boneMask[i] = value.boneMask[i];
            }
        }

        public bool ContainsBoneMask(string name)
        {
            return boneMask.Count == 0 || boneMask.Contains(name);
        }

        public void AddBoneMask(Armature armature, string name, bool recursive = false)
        {
            var currentBone = armature.GetBone(name);
            if (currentBone == null)
            {
                return;
            }

            if (!boneMask.Contains(name)) // Add mixing
            {
                boneMask.Add(name);
            }

            if (recursive) // Add recursive mixing.
            {
                var bones = armature.GetBones();
                for (int i = 0, l = bones.Count; i < l; ++i)
                {
                    var bone = bones[i];
                    if (!boneMask.Contains(bone.name) && currentBone.Contains(bone))
                    {
                        boneMask.Add(bone.name);
                    }
                }
            }
        }

        public void RemoveBoneMask(Armature armature, string name, bool recursive = true)
        {
            if (boneMask.Contains(name)) // Remove mixing.
            {
                boneMask.Remove(name);
            }

            if (recursive)
            {
                var currentBone = armature.GetBone(name);
                if (currentBone != null)
                {
                    var bones = armature.GetBones();
                    if (boneMask.Count > 0) // Remove recursive mixing.
                    {
                        for (int i = 0, l = bones.Count; i < l; ++i)
                        {
                            var bone = bones[i];
                            if (boneMask.Contains(bone.name) && currentBone.Contains(bone))
                            {
                                boneMask.Remove(bone.name);
                            }
                        }
                    }
                    else // Add unrecursive mixing.
                    {
                        for (int i = 0, l = bones.Count; i < l; ++i)
                        {
                            var bone = bones[i];
                            if (!currentBone.Contains(bone))
                            {
                                boneMask.Add(bone.name);
                            }
                        }
                    }
                }
            }
        }
    }
}