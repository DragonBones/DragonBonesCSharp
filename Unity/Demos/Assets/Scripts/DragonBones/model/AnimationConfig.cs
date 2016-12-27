using System;
using System.Collections.Generic;

namespace DragonBones
{
    /**
     * @language zh_CN
     * 动画混合时，使用的淡出方式。
     * @see dragonBones.Animation#fadeIn()
     * @version DragonBones 4.5
     */
    public enum AnimationFadeOutMode
    {
        /**
         * @language zh_CN
         * 不淡出动画。
         * @version DragonBones 4.5
         */
        None = 0,

        /**
        * @language zh_CN
         * 淡出同层的动画。
         * @version DragonBones 4.5
         */
        SameLayer = 1,

        /**
         * @language zh_CN
         * 淡出同组的动画。
         * @version DragonBones 4.5
         */
        SameGroup = 2,

        /**
         * @language zh_CN
         * 淡出同层并且同组的动画。
         * @version DragonBones 4.5
         */
        SameLayerAndGroup = 3,

        /**
         * @language zh_CN
         * 淡出所有动画。
         * @version DragonBones 4.5
         */
        All = 4
    }

    /**
     * @private
     */
    public class AnimationConfig : BaseObject
    {
        public bool pauseFadeOut;
        public AnimationFadeOutMode fadeOutMode;
        public float fadeOutTime;
        public float fadeOutEasing;

        public bool additiveBlending;
        public bool displayControl;
        public bool pauseFadeIn;
        public bool actionEnabled;
        public int playTimes;
        public int layer;
        public float position;
        public float duration;
        public float timeScale;
        public float fadeInTime;
        public float autoFadeOutTime;
        public float fadeInEasing;
        public float weight;
        public string name;
        public string animationName;
        public string group;
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
            this._onClear();
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

            if (recursive)
            {
                foreach (var bone in armature.GetBones())
                {
                    if (!boneMask.Contains(bone.name) && currentBone.Contains(bone)) // Add recursive mixing.
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
                    if (boneMask.Count > 0)
                    {
                        foreach (var bone in bones)
                        {
                            if (boneMask.Contains(bone.name) && currentBone.Contains(bone)) // Remove recursive mixing.
                            {
                                boneMask.Remove(bone.name);
                            }
                        }
                    }
                    else
                    {
                        foreach (var bone in bones)
                        {
                            if (!currentBone.Contains(bone)) // Add unrecursive mixing.
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