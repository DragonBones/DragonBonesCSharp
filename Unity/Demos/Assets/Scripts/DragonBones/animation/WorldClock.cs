using System;
using System.Collections.Generic;

namespace DragonBones
{
    /**
     * @language zh_CN
     * WorldClock 提供时钟支持，为每个加入到时钟的 IAnimatable 对象更新时间。
     * @see DragonBones.IAnimateble
     * @see DragonBones.Armature
     * @version DragonBones 3.0
     */
    public class WorldClock : IAnimateble
    {
        /**
         * @language zh_CN
         * 当前时间。 (以秒为单位)
         * @version DragonBones 3.0
         */
        public float time = DateTime.Now.Ticks / 100.0f / DragonBones.SECOND_TO_MILLISECOND;
        /**
         * @language zh_CN
         * 时间流逝速度，用于控制动画变速播放。 [0: 停止播放, (0~1): 慢速播放, 1: 正常播放, (1~N): 快速播放]
         * @default 1
         * @version DragonBones 3.0
         */
        public float timeScale = 1.0f;

        private readonly List<IAnimateble> _animatebles = new List<IAnimateble>();
        private WorldClock _clock = null;
        /**
         * @language zh_CN
         * 创建一个新的 WorldClock 实例。
         * 通常并不需要单独创建 WorldClock 实例，可以直接使用 WorldClock.clock 静态实例。
         * (创建更多独立的 WorldClock 实例可以更灵活的为需要更新的 IAnimateble 实例分组，用于控制不同组不同的播放速度)
         * @version DragonBones 3.0
         */
        public WorldClock()
        {
        }
        /**
         * @language zh_CN
         * 为所有的 IAnimatable 实例更新时间。
         * @param passedTime 前进的时间。 (以秒为单位，当设置为 -1 时将自动计算当前帧与上一帧的时间差)
         * @version DragonBones 3.0
         */
        public void AdvanceTime(float passedTime)
        {
            if (float.IsNaN(passedTime))
            {
                passedTime = 0.0f;
            }

            if (passedTime < 0.0f)
            {
                passedTime = DateTime.Now.Ticks / 100.0f / DragonBones.SECOND_TO_MILLISECOND - time;
            }
            
            if (timeScale != 1.0f)
            {
                passedTime *= timeScale;
            }

            if (passedTime < 0.0f)
            {
                time -= passedTime;
            }
            else
            {
                time += passedTime;
            }

            if (passedTime > 0.0f)
            {
                int i = 0, r = 0, l = _animatebles.Count;
                for (; i < l; ++i)
                {
                    var animateble = _animatebles[i];
                    if (animateble != null)
                    {
                        if (r > 0)
                        {
                            _animatebles[i - r] = animateble;
                            _animatebles[i] = null;
                        }

                        animateble.AdvanceTime(passedTime);
                    }
                    else
                    {
                        r++;
                    }
                }

                if (r > 0)
                {
                    l = _animatebles.Count;
                    for (; i < l; ++i)
                    {
                        var animateble = _animatebles[i];
                        if (animateble != null)
                        {
                                _animatebles[i - r] = animateble;
                        }
                        else
                        {
                            r++;
                        }
                    }

                    DragonBones.ResizeList(_animatebles, l - r, null);
                }
            }
        }
        /** 
         * 是否包含 IAnimatable 实例
         * @param value IAnimatable 实例。
         * @version DragonBones 3.0
         */
        public bool Contains(IAnimateble value) {
            return _animatebles.Contains(value);
        }
        /**
         * @language zh_CN
         * 添加 IAnimatable 实例。
         * @param value IAnimatable 实例。
         * @version DragonBones 3.0
         */
        public void Add(IAnimateble value)
        {
            if (value != null && !_animatebles.Contains(value))
            {
                _animatebles.Add(value);
                value.clock = this;
            }
        }
        /**
         * @language zh_CN
         * 清除所有的 IAnimatable 实例。
         * @version DragonBones 3.0
         */
        public void Remove(IAnimateble value)
        {
            var index =     _animatebles.IndexOf(value);
            if (index >= 0)
            {
                _animatebles[index] = null;
                value.clock = null;
            }
        }
        /**
         * @language zh_CN
         * 清除所有的 IAnimatable 实例。
         * @version DragonBones 3.0
         */
        public void Clear()
        {
            for (int i = 0, l =     _animatebles.Count; i < l; ++i)
            {
                var animateble = _animatebles[i];
                _animatebles[i] = null;
                if (animateble != null)
                {
                    animateble.clock = null;
                }
            }
        }
        /**
         * @inheritDoc
         */
        public WorldClock clock
        {
            get { return _clock; }
            set
            {
                if (_clock == value)
                {
                    return;
                }

                var prevClock = _clock;
                _clock = value;

                if (prevClock != null)
                {
                    prevClock.Remove(this);
                }

                if (_clock != null)
                {
                    _clock.Add(this);
                }
            }
        }
    }
}