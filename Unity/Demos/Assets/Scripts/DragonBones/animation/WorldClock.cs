using System;
using System.Collections.Generic;

namespace DragonBones
{
    /**
     * @language zh_CN
     * WorldClock 提供时钟的支持，为每个加入到时钟的 IAnimatable 对象更新时间。
     * @see dragonBones.IAnimatable
     * @see dragonBones.Armature
     * @version DragonBones 3.0
     */
    public class WorldClock : IAnimateble
    {
        /**
         * @language zh_CN
         * 当前的时间。 (以秒为单位)
         * @version DragonBones 3.0
         */
        public float time = DateTime.Now.Ticks / 100.0f / DragonBones.SECOND_TO_MILLISECOND;

        /**
         * @language zh_CN
         * 时间流逝的速度，用于实现动画的变速播放。 [0: 停止播放, (0~1): 慢速播放, 1: 正常播放, (1~N): 快速播放]
         * @default 1
         * @version DragonBones 3.0
         */
        public float timeScale = 1.0f;

        private WorldClock _clock = null;
        private readonly List<IAnimateble> _animatebles = new List<IAnimateble>();

        /**
         * @language zh_CN
         * 创建一个新的 WorldClock 实例。
         * 通常并不需要单独创建 WorldClock 的实例，可以直接使用 WorldClock.clock 静态实例。
         * (创建更多独立的 WorldClock 可以更灵活的为需要更新的 IAnimateble 实例分组，实现不同组不同速度的动画播放)
         * @version DragonBones 3.0
         */
        public WorldClock()
        {
        }

        /**
         * @private
         */
        public void _onAdd(WorldClock value)
        {
            if (_clock != null)
            {
                _clock.Remove(this);
            }

            _clock = value;
        }

        /**
         * @private
         */
        public void _onRemove()
        {
            _clock = null;
        }

        /**
         * @language zh_CN
         * 为所有的 IAnimatable 实例向前播放一个指定的时间。 (通常这个方法需要在 ENTER_FRAME 事件的响应函数中被调用)
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

            passedTime *= timeScale;

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
                            this._animatebles[i - r] = animateble;
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
         * 是否包含指定的 IAnimatable 实例
         * @param value 指定的 IAnimatable 实例。
         * @returns  [true: 包含，false: 不包含]。
         * @version DragonBones 3.0
         */
        public bool Contains(IAnimateble value) {
            return _animatebles.Contains(value);
        }

        /**
         * @language zh_CN
         * 添加指定的 IAnimatable 实例。
         * @param value IAnimatable 实例。
         * @version DragonBones 3.0
         */
        public void Add(IAnimateble value)
        {
            if (value != null && !_animatebles.Contains(value))
            {
                value._onAdd(this);
                _animatebles.Add(value);
            }
        }

        /**
         * @language zh_CN
         * 移除指定的 IAnimatable 实例。
         * @param value IAnimatable 实例。
         * @version DragonBones 3.0
         */
        public void Remove(IAnimateble value)
        {
            var index = this._animatebles.IndexOf(value);
            if (index >= 0)
            {
                value._onRemove();
                _animatebles[index] = null;
            }
        }

        /**
         * @language zh_CN
         * 清除所有的 IAnimatable 实例。
         * @version DragonBones 3.0
         */
        public void Clear()
        {
            for (int i = 0, l = this._animatebles.Count; i < l; ++i)
            {
                var animateble = _animatebles[i];
                if (animateble != null)
                {
                    animateble._onRemove();
                }
                _animatebles[i] = null;
            }
        }
    }
}