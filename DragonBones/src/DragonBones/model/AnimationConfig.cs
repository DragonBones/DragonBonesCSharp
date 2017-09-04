using System.Collections.Generic;

namespace DragonBones
{
    /**
     * 动画配置，描述播放一个动画所需要的全部信息。
     * @see dragonBones.AnimationState
     * @version DragonBones 5.0
     * @beta
     * @language zh_CN
     */
    public class AnimationConfig : BaseObject
    {
        /**
         * 是否暂停淡出的动画。
         * @default true
         * @version DragonBones 5.0
         * @language zh_CN
         */
        public bool pauseFadeOut;
        /**
         * 淡出模式。
         * @default dragonBones.AnimationFadeOutMode.All
         * @see dragonBones.AnimationFadeOutMode
         * @version DragonBones 5.0
         * @language zh_CN
         */
        public AnimationFadeOutMode fadeOutMode;
        /**
         * 淡出缓动方式。
         * @default TweenType.Line
         * @see dragonBones.TweenType
         * @version DragonBones 5.0
         * @language zh_CN
         */
        public TweenType fadeOutTweenType;
        /**
         * 淡出时间。 [-1: 与淡入时间同步, [0~N]: 淡出时间] (以秒为单位)
         * @default -1
         * @version DragonBones 5.0
         * @language zh_CN
         */
        public float fadeOutTime;

        /**
         * 否能触发行为。
         * @default true
         * @version DragonBones 5.0
         * @language zh_CN
         */
        public bool actionEnabled;
        /**
         * 是否以增加的方式混合。
         * @default false
         * @version DragonBones 5.0
         * @language zh_CN
         */
        public bool additiveBlending;
        /**
         * 是否对插槽的显示对象有控制权。
         * @default true
         * @version DragonBones 5.0
         * @language zh_CN
         */
        public bool displayControl;
        /**
         * 是否暂停淡入的动画，直到淡入过程结束。
         * @default true
         * @version DragonBones 5.0
         * @language zh_CN
         */
        public bool pauseFadeIn;
        /**
         * 是否将没有动画的对象重置为初始值。
         * @default true
         * @version DragonBones 5.1
         * @language zh_CN
         */
        public bool resetToPose;
        /**
         * 淡入缓动方式。
         * @default TweenType.Line
         * @see dragonBones.TweenType
         * @version DragonBones 5.0
         * @language zh_CN
         */
        public TweenType fadeInTweenType;
        /**
         * 播放次数。 [-1: 使用动画数据默认值, 0: 无限循环播放, [1~N]: 循环播放 N 次]
         * @default -1
         * @version DragonBones 5.0
         * @language zh_CN
         */
        public int playTimes;
        /**
         * 混合图层，图层高会优先获取混合权重。
         * @default 0
         * @version DragonBones 5.0
         * @language zh_CN
         */
        public int layer;
        /**
         * 开始时间。 (以秒为单位)
         * @default 0
         * @version DragonBones 5.0
         * @language zh_CN
         */
        public float position;
        /**
         * 持续时间。 [-1: 使用动画数据默认值, 0: 动画停止, (0~N]: 持续时间] (以秒为单位)
         * @default -1
         * @version DragonBones 5.0
         * @language zh_CN
         */
        public float duration;
        /**
         * 播放速度。 [(-N~0): 倒转播放, 0: 停止播放, (0~1): 慢速播放, 1: 正常播放, (1~N): 快速播放]
         * @default 1
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public float timeScale;
        /**
         * 淡入时间。 [-1: 使用动画数据默认值, [0~N]: 淡入时间] (以秒为单位)
         * @default -1
         * @version DragonBones 5.0
         * @language zh_CN
         */
        public float fadeInTime;
        /**
         * 自动淡出时间。 [-1: 不自动淡出, [0~N]: 淡出时间] (以秒为单位)
         * @default -1
         * @version DragonBones 5.0
         * @language zh_CN
         */
        public float autoFadeOutTime;
        /**
         * 混合权重。
         * @default 1
         * @version DragonBones 5.0
         * @language zh_CN
         */
        public float weight;
        /**
         * 动画状态名。
         * @version DragonBones 5.0
         * @language zh_CN
         */
        public string name;
        /**
         * 动画数据名。
         * @version DragonBones 5.0
         * @language zh_CN
         */
        public string animation;
        /**
         * 混合组，用于动画状态编组，方便控制淡出。
         * @version DragonBones 5.0
         * @language zh_CN
         */
        public string group;
        /**
         * 骨骼遮罩。
         * @version DragonBones 5.0
         * @language zh_CN
         */
        public readonly List<string> boneMask = new List<string>();

        protected void _OnClear()
        {
            this.pauseFadeOut = true;
            this.fadeOutMode = AnimationFadeOutMode.All;
            this.fadeOutTweenType = TweenType.Line;
            this.fadeOutTime = -1.0f;

            this.actionEnabled = true;
            this.additiveBlending = false;
            this.displayControl = true;
            this.pauseFadeIn = true;
            this.resetToPose = true;
            this.fadeInTweenType = TweenType.Line;
            this.playTimes = -1;
            this.layer = 0;
            this.position = 0.0f;
            this.duration = -1.0f;
            this.timeScale = -100.0f;
            this.fadeInTime = -1.0f;
            this.autoFadeOutTime = -1.0f;
            this.weight = 1.0f;
            this.name = "";
            this.animation = "";
            this.group = "";
            this.boneMask.Clear();
        }

        public void Clear()
        {
            this._onClear();
        }

        public void CopyFrom(AnimationConfig value)
        {
            this.pauseFadeOut = value.pauseFadeOut;
            this.fadeOutMode = value.fadeOutMode;
            this.autoFadeOutTime = value.autoFadeOutTime;
            this.fadeOutTweenType = value.fadeOutTweenType;

            this.actionEnabled = value.actionEnabled;
            this.additiveBlending = value.additiveBlending;
            this.displayControl = value.displayControl;
            this.pauseFadeIn = value.pauseFadeIn;
            this.resetToPose = value.resetToPose;
            this.playTimes = value.playTimes;
            this.layer = value.layer;
            this.position = value.position;
            this.duration = value.duration;
            this.timeScale = value.timeScale;
            this.fadeInTime = value.fadeInTime;
            this.fadeOutTime = value.fadeOutTime;
            this.fadeInTweenType = value.fadeInTweenType;
            this.weight = value.weight;
            this.name = value.name;
            this.animation = value.animation;
            this.group = value.group;

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
                            if (bone == currentBone)
                            {
                                continue;
                            }

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