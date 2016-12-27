using System.Collections.Generic;

namespace DragonBones
{
    /**
     * @language zh_CN
     * 动画控制器，用来播放动画数据，管理动画状态。
     * @see dragonBones.AnimationData
     * @see dragonBones.AnimationState
     * @version DragonBones 3.0
     */
    public class Animation : BaseObject
    {
        /**
         * @private
         */
        protected static int _sortAnimationState(AnimationState a, AnimationState b)
        {
            return a.layer > b.layer ? -1 : 1;
        }

        /**
         * @language zh_CN
         * 动画的播放速度。 [(-N~0): 倒转播放, 0: 停止播放, (0~1): 慢速播放, 1: 正常播放, (1~N): 快速播放]
         * @default 1
         * @version DragonBones 3.0
         */
        public float timeScale;

        private bool _isPlaying;
        private bool _animationStateDirty;
        /**
         * @private
         */
        internal bool _timelineStateDirty;
        private readonly List<string> _animationNames = new List<string>();
        private readonly Dictionary<string, AnimationData> _animations = new Dictionary<string, AnimationData>();
        private readonly List<AnimationState> _animationStates = new List<AnimationState>();
        private Armature _armature;
        private AnimationState _lastAnimationState;
        private AnimationConfig _animationConfig;

        /**
         * @private
         */
        public Animation()
        {
        }

        /**
         * @inheritDoc
         */
        override protected void _onClear()
        {
            if (_animationConfig != null)
            {
                _animationConfig.ReturnToPool();
            }

            foreach (var animationState in _animationStates)
            {
                animationState.ReturnToPool();
            }

            timeScale = 1.0f;

            _isPlaying = false;
            _animationStateDirty = false;
            _timelineStateDirty = false;
            _animationNames.Clear();
            _animations.Clear();
            _animationStates.Clear();
            _armature = null;
            _lastAnimationState = null;
            _animationConfig = null;
        }

        /**
         * @private
         */
        protected void _fadeOut(AnimationConfig animationConfig)
        {
            switch (animationConfig.fadeOutMode)
            {
                case AnimationFadeOutMode.SameLayer:
                    foreach (var animationState in _animationStates)
                    {
                        if (animationState.layer == animationConfig.layer)
                        {
                            animationState.FadeOut(animationConfig.fadeOutTime, animationConfig.pauseFadeOut);
                        }
                    }
                    break;

                case AnimationFadeOutMode.SameGroup:
                    foreach (var animationState in _animationStates)
                    {
                        if (animationState.group == animationConfig.group)
                        {
                            animationState.FadeOut(animationConfig.fadeOutTime, animationConfig.pauseFadeOut);
                        }
                    }
                    break;

                case AnimationFadeOutMode.SameLayerAndGroup:
                    foreach (var animationState in _animationStates)
                    {
                        if (animationState.layer == animationConfig.layer && animationState.group == animationConfig.group)
                        {
                            animationState.FadeOut(animationConfig.fadeOutTime, animationConfig.pauseFadeOut);
                        }
                    }
                    break;

                case AnimationFadeOutMode.All:
                    foreach (var animationState in _animationStates)
                    {
                        animationState.FadeOut(animationConfig.fadeOutTime, animationConfig.pauseFadeOut);
                    }
                    break;

                case AnimationFadeOutMode.None:
                default:
                    break;
            }
        }
        /**
         * @private
         */
        internal void _init(Armature armature)
        {
            if (_armature != null)
            {
                return;
            }

            _armature = armature;
            _animationConfig = BaseObject.BorrowObject<AnimationConfig>();
        }

        /**
         * @private
         */
        internal void _advanceTime(float passedTime)
        {
            if (!_isPlaying)
            {
                return;
            }

            if (passedTime < 0.0f)
            {
                passedTime = -passedTime;
            }

            var animationStateCount = _animationStates.Count;
            if (animationStateCount == 1)
            {
                var animationState = _animationStates[0];
                if (animationState._fadeState > 0 && animationState._subFadeState > 0)
                {
                    animationState.ReturnToPool();
                    _animationStates.Clear();
                    _animationStateDirty = true;
                    _lastAnimationState = null;
                }
                else
                {
                    var animationData = animationState.animationData;
                    var cacheFrameRate = animationData.cacheFrameRate;

                    if (_animationStateDirty && cacheFrameRate > 0.0f) // Update cachedFrameIndices.
                    {
                        _animationStateDirty = false;

                        var bones = _armature.GetBones();
                        for (int i = 0, l = bones.Count; i < l; ++i)
                        {
                            var bone = bones[i];
                            bone._cachedFrameIndices = animationData.GetBoneCachedFrameIndices(bone.name);
                        }

                        var slots = _armature.GetSlots();
                        for (int i = 0, l = slots.Count; i < l; ++i)
                        {
                            var slot = slots[i];
                            slot._cachedFrameIndices = animationData.GetSlotCachedFrameIndices(slot.name);
                        }
                    }

                    if (_timelineStateDirty)
                    {
                        animationState._updateTimelineStates();
                    }

                    animationState._advanceTime(passedTime, cacheFrameRate);
                }
            }
            else if (animationStateCount > 1)
            {
                for (int i = 0, r = 0; i < animationStateCount; ++i)
                {
                    var animationState = _animationStates[i];
                    if (animationState._fadeState > 0 && animationState._fadeProgress <= 0.0f)
                    {
                        r++;
                        animationState.ReturnToPool();
                        _animationStateDirty = true;

                        if (_lastAnimationState == animationState) // Update last animation state.
                        {
                            _lastAnimationState = null;
                        }
                    }
                    else
                    {
                        if (r > 0)
                        {
                            _animationStates[i - r] = animationState;
                        }

                        if (_timelineStateDirty)
                        {
                            animationState._updateTimelineStates();
                        }

                        animationState._advanceTime(passedTime, -1.0f);
                    }

                    if (i == animationStateCount - 1 && r > 0) // Modify animation states size.
                    {
                        DragonBones.ResizeList(_animationStates, animationStateCount - r, null);
                        
                        if (_lastAnimationState == null && _animationStates.Count > 0)
                        {
                            _lastAnimationState = _animationStates[_animationStates.Count - 1];
                        }
                    }
                }

                _armature._cacheFrameIndex = -1;
            }
            else
            {
                _armature._cacheFrameIndex = -1;
            }

            _timelineStateDirty = false;
        }

        /**
         * @language zh_CN
         * 清除所有正在播放的动画状态。
         * @version DragonBones 4.5
         */
        public void Reset()
        {
            foreach (var animationState in _animationStates)
            {
                animationState.ReturnToPool();
            }

            _isPlaying = false;
            _animationStateDirty = false;
            _timelineStateDirty = false;
            _animationConfig.Clear();
            _animationStates.Clear();
            _lastAnimationState = null;
        }

        /**
         * @language zh_CN
         * 暂停播放动画。
         * @param animationName 动画状态的名称，如果未设置，则暂停所有动画状态。
         * @see dragonBones.AnimationState
         * @version DragonBones 3.0
         */
        public void Stop(string animationName = null)
        {
            if (!string.IsNullOrEmpty(animationName))
            {
                var animationState = GetState(animationName);
                if (animationState != null)
                {
                    animationState.Stop();
                }
            }
            else
            {
                _isPlaying = false;
            }
        }
        /**
         * @private
         */
        public AnimationState PlayConfig(AnimationConfig animationConfig)
        {
            if (animationConfig == null)
            {
                DragonBones.Assert(false, DragonBones.ARGUMENT_ERROR);
                return null;
            }

            var animationName = !string.IsNullOrEmpty(animationConfig.animationName) ? animationConfig.animationName : animationConfig.name;
            var animationData = _animations.ContainsKey(animationName) ? _animations[animationName] : null;
            if (animationData == null)
            {
                DragonBones.Assert(false,
                    "Non-existent animation.\n" +
                    "DragonBones name: " + _armature.armatureData.parent.name +
                    " Armature name: " + _armature.name +
                    " Animation name: " + animationName
                );

                return null;
            }

            _isPlaying = true;
            if (animationConfig.playTimes < 0)
            {
                animationConfig.playTimes = (int)animationData.playTimes;
            }

            if (float.IsNaN(animationConfig.fadeInTime) || animationConfig.fadeInTime < 0.0f)
            {
                if (_lastAnimationState != null)
                {
                    animationConfig.fadeInTime = animationData.fadeInTime;
                }
                else
                {
                    animationConfig.fadeInTime = 0.0f;
                }
            }

            if (float.IsNaN(animationConfig.fadeOutTime) || animationConfig.fadeOutTime < 0.0f)
            {
                animationConfig.fadeOutTime = animationConfig.fadeInTime;
            }

            if (animationConfig.timeScale <= -100.0f)
            {
                animationConfig.timeScale = 1.0f / animationData.scale;
            }

            _fadeOut(animationConfig);

            _lastAnimationState = BaseObject.BorrowObject<AnimationState>();
            _lastAnimationState._init(_armature, animationData, animationConfig);
            _animationStates.Add(_lastAnimationState);
            _animationStateDirty = true;
            _armature._cacheFrameIndex = -1;

            if (_animationStates.Count > 1)
            {
                _animationStates.Sort(_sortAnimationState);
            }

            // Child armature play same name animation.
            foreach (var slot in _armature.GetSlots())
            {
                if (slot.inheritAnimation)
                {
                    var childArmature = slot.childArmature;
                    if (
                        childArmature != null &&
                        childArmature.animation.HasAnimation(animationName) &&
                        childArmature.animation.GetState(animationName) == null
                    )
                    {
                        childArmature.animation.FadeIn(animationName);
                    }
                }
            }

            if (animationConfig.fadeInTime <= 0.0f)
            {
                _armature.AdvanceTime(0.0f); // Blend animation state, update armature.
            }

            return _lastAnimationState;
        }

        /**
         * @language zh_CN
         * 淡入播放指定名称的动画。
         * @param animationName 动画数据的名称。
         * @param playTimes 循环播放的次数。 [-1: 使用数据默认值, 0: 无限循环播放, [1~N]: 循环播放 N 次]
         * @param fadeInTime 淡入的时间。 [-1: 使用数据默认值, [0~N]: N 秒淡入完毕] (以秒为单位)
         * @param layer 混合的图层，图层高会优先获取混合权重。
         * @param group 混合的组，用于给动画状态编组，方便混合淡出控制。
         * @param fadeOutMode 淡出的模式。
         * @returns 返回控制这个动画数据的动画状态。
         * @see dragonBones.AnimationFadeOutMode
         * @see dragonBones.AnimationState
         * @version DragonBones 4.5
         */
        public AnimationState FadeIn(
            string animationName, float fadeInTime = -1.0f, int playTimes = -1,
            int layer = 0, string group = null, AnimationFadeOutMode fadeOutMode = AnimationFadeOutMode.SameLayerAndGroup,
            bool additiveBlending = false, bool displayControl = true,
            bool pauseFadeOut = true, bool pauseFadeIn = true
        )
        {
            _animationConfig.Clear();
            _animationConfig.fadeOutMode = fadeOutMode;
            _animationConfig.playTimes = playTimes;
            _animationConfig.layer = layer;
            _animationConfig.fadeInTime = fadeInTime;
            _animationConfig.animationName = animationName;
            _animationConfig.group = group;

            return PlayConfig(_animationConfig);
        }

        /**
         * @language zh_CN
         * 播放动画。
         * @param animationName 动画数据的名称，如果未设置，则播放默认动画，或将暂停状态切换为播放状态，或重新播放上一个正在播放的动画。 
         * @param playTimes 动画需要播放的次数。 [-1: 使用动画数据默认值, 0: 无限循环播放, [1~N]: 循环播放 N 次]
         * @returns 返回控制这个动画数据的动画状态。
         * @see dragonBones.AnimationState
         * @version DragonBones 3.0
         */
        public AnimationState Play(string animationName = null, int playTimes = -1)
        {
            _animationConfig.Clear();
            _animationConfig.playTimes = playTimes;
            _animationConfig.fadeInTime = 0.0f;
            _animationConfig.animationName = animationName;

            if (!string.IsNullOrEmpty(animationName))
            {
                PlayConfig(_animationConfig);
            }
            else if (_lastAnimationState == null)
            {
                var defaultAnimation = _armature.armatureData.defaultAnimation;
                if (defaultAnimation != null)
                {
                    _animationConfig.animationName = defaultAnimation.name;
                    PlayConfig(_animationConfig);
                }
            }
            else if (!_isPlaying || (!_lastAnimationState.isPlaying && !_lastAnimationState.isCompleted))
            {
                _isPlaying = true;
                _lastAnimationState.Play();
            }
            else
            {
                _animationConfig.animationName = _lastAnimationState.name;
                PlayConfig(_animationConfig);
            }

            return _lastAnimationState;
        }

        /**
         * @language zh_CN
         * 指定名称的动画从指定时间开始播放。
         * @param animationName 动画数据的名称。
         * @param time 时间。 (以秒为单位)
         * @param playTimes 动画循环播放的次数。 [-1: 使用动画数据默认值, 0: 无限循环播放, [1~N]: 循环播放 N 次]
         * @returns 返回控制这个动画数据的动画状态。
         * @see dragonBones.AnimationState
         * @version DragonBones 4.5
         */
        public AnimationState GotoAndPlayByTime(string animationName, float time = 0.0f, int playTimes = -1)
        {
            _animationConfig.Clear();
            _animationConfig.playTimes = playTimes;
            _animationConfig.position = time;
            _animationConfig.fadeInTime = 0.0f;
            _animationConfig.animationName = animationName;

            return PlayConfig(_animationConfig);
        }

        /**
         * @language zh_CN
         * 指定名称的动画从指定帧开始播放。
         * @param animationName 动画数据的名称。
         * @param frame 帧。
         * @param playTimes 动画循环播放的次数。[-1: 使用动画数据默认值, 0: 无限循环播放, [1~N]: 循环播放 N 次]
         * @returns 返回控制这个动画数据的动画状态。
         * @see dragonBones.AnimationState
         * @version DragonBones 4.5
         */
        public AnimationState GotoAndPlayByFrame(string animationName, uint frame = 0, int playTimes = -1)
        {
            _animationConfig.Clear();
            _animationConfig.playTimes = playTimes;
            _animationConfig.fadeInTime = 0.0f;
            _animationConfig.animationName = animationName;

            if (_animations.ContainsKey(animationName))
            {
                var animationData = _animations[animationName];
                _animationConfig.position = animationData.duration * frame / animationData.frameCount;
            }

            return PlayConfig(_animationConfig);
        }

        /**
         * @language zh_CN
         * 指定名称的动画从指定进度开始播放。
         * @param animationName 动画数据的名称。
         * @param progress 进度。 [0~1]
         * @param playTimes 动画循环播放的次数。[-1: 使用动画数据默认值, 0: 无限循环播放, [1~N]: 循环播放 N 次]
         * @returns 返回控制这个动画数据的动画状态。
         * @see dragonBones.AnimationState
         * @version DragonBones 4.5
         */
        public AnimationState GotoAndPlayByProgress(string animationName, float progress = 0.0f, int playTimes = -1)
        {
            _animationConfig.Clear();
            _animationConfig.playTimes = playTimes;
            _animationConfig.fadeInTime = 0.0f;
            _animationConfig.animationName = animationName;

            if (_animations.ContainsKey(animationName))
            {
                var animationData = _animations[animationName];
                _animationConfig.position = animationData.duration * (progress > 0.0f ? progress : 0.0f);
            }

            return PlayConfig(_animationConfig);
        }

        /**
         * @language zh_CN
         * 播放指定名称的动画到指定的时间并停止。
         * @param animationName 动画数据的名称。
         * @param time 时间。 (以秒为单位)
         * @returns 返回控制这个动画数据的动画状态。
         * @see dragonBones.AnimationState
         * @version DragonBones 4.5
         */
        public AnimationState GotoAndStopByTime(string animationName, float time = 0.0f)
        {
            var animationState = GotoAndPlayByTime(animationName, time, 1);
            if (animationState != null)
            {
                animationState.Stop();
            }

            return animationState;
        }

        /**
         * @language zh_CN
         * 播放指定名称的动画到指定的帧并停止。
         * @param animationName 动画数据的名称。
         * @param frame 帧。
         * @returns 返回控制这个动画数据的动画状态。
         * @see dragonBones.AnimationState
         * @version DragonBones 4.5
         */
        public AnimationState GotoAndStopByFrame(string animationName, uint frame = 0)
        {
            var animationState = GotoAndPlayByFrame(animationName, frame, 1);
            if (animationState != null)
            {
                animationState.Stop();
            }

            return animationState;
        }

        /**
         * @language zh_CN
         * 播放指定名称的动画到指定的进度并停止。
         * @param animationName 动画数据的名称。
         * @param progress 进度。 [0~1]
         * @returns 返回控制这个动画数据的动画状态。
         * @see dragonBones.AnimationState
         * @version DragonBones 4.5
         */
        public AnimationState GotoAndStopByProgress(string animationName, float progress = 0.0f)
        {
            var animationState = GotoAndPlayByProgress(animationName, progress, 1);
            if (animationState != null)
            {
                animationState.Stop();
            }

            return animationState;
        }

        /**
         * @language zh_CN
         * 获取指定名称的动画状态。
         * @param animationName 动画状态的名称。
         * @see dragonBones.AnimationState
         * @version DragonBones 3.0
         */
        public AnimationState GetState(string animationName)
        {
            foreach (var animationState in _animationStates)
            {
                if (animationState.name == animationName)
                {
                    return animationState;
                }
            }

            return null;
        }

        /**
         * @language zh_CN
         * 是否包含指定名称的动画数据。
         * @param animationName 动画数据的名称。
         * @see dragonBones.AnimationData
         * @version DragonBones 3.0
         */
        public bool HasAnimation(string animationName)
        {
            return _animations.ContainsKey(animationName);
        }

        /**
         * @language zh_CN
         * 动画是否处于播放状态。
         * @version DragonBones 3.0
         */
        public bool isPlaying
        {
            get
            {
                if (_animationStates.Count > 1)
                {
                    return _isPlaying && !isCompleted;
                }
                else if (_lastAnimationState != null)
                {
                    return _isPlaying && _lastAnimationState.isPlaying;
                }

                return _isPlaying;
            }
        }

        /**
         * @language zh_CN
         * 所有动画状态是否均已播放完毕。
         * @see dragonBones.AnimationState
         * @version DragonBones 3.0
         */
        public bool isCompleted
        {
            get
            {
                if (_lastAnimationState != null)
                {
                    if (!_lastAnimationState.isCompleted)
                    {
                        return false;
                    }

                    foreach (var animationState in _animationStates)
                    {
                        if (!animationState.isCompleted)
                        {
                            return false;
                        }
                    }

                    return true;
                }

                return false;
            }
        }

        /**
         * @language zh_CN
         * 上一个正在播放的动画状态的名称。
         * @see #lastAnimationState
         * @version DragonBones 3.0
         */
        public string lastAnimationName
        {
            get { return _lastAnimationState != null ? _lastAnimationState.name : null; }
        }

        /**
         * @language zh_CN
         * 上一个正在播放的动画状态。
         * @see dragonBones.AnimationState
         * @version DragonBones 3.0
         */
        public AnimationState lastAnimationState
        {
            get { return _lastAnimationState; }
        }
        /**
         * @private
         */
        public AnimationConfig animationConfig
        {
            get { return _animationConfig; }
        }

        /**
         * @language zh_CN
         * 所有动画数据名称。
         * @see #animations
         * @version DragonBones 4.5
         */
        public List<string> animationNames
        {
            get { return _animationNames; }
        }

        /**
         * @language zh_CN
         * 所有的动画数据。
         * @see dragonBones.AnimationData
         * @version DragonBones 4.5
         */
        public Dictionary<string, AnimationData> animations
        {
            get { return _animations; }
            set
            {
                if (_animations == value)
                {
                    return;
                }

                _animationNames.Clear();
                _animations.Clear();

                if (value != null)
                {
                    foreach (var pair in value)
                    {
                        _animationNames.Add(pair.Key);
                        _animations[pair.Key] = pair.Value;
                    }
                }
            }
        }
    }
}