using System.Collections.Generic;

namespace DragonBones
{
    /**
     * @language zh_CN
     * 动画控制器，用来播放动画数据，管理动画状态。
     * @see DragonBones.AnimationData
     * @see DragonBones.AnimationState
     * @version DragonBones 3.0
     */
    public class Animation : BaseObject
    {
        private static int _sortAnimationState(AnimationState a, AnimationState b)
        {
            return a.layer > b.layer ? -1 : 1;
        }
        /**
         * @language zh_CN
         * 播放速度。 [0: 停止播放, (0~1): 慢速播放, 1: 正常播放, (1~N): 快速播放]
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
        /**
         * @private
         */
        internal int _cacheFrameIndex;
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
         * @private
         */
        override protected void _onClear()
        {
            foreach (var animationState in _animationStates)
            {
                animationState.ReturnToPool();
            }

            if (_animationConfig != null)
            {
                _animationConfig.ReturnToPool();
            }

            timeScale = 1.0f;

            _isPlaying = false;
            _animationStateDirty = false;
            _timelineStateDirty = false;
            _cacheFrameIndex = -1;
            _animationNames.Clear();
            _animations.Clear();
            _animationStates.Clear();
            _armature = null;
            _lastAnimationState = null;
            _animationConfig = null;
        }

        private void _fadeOut(AnimationConfig animationConfig)
        {
            int i = 0, l = _animationStates.Count;
            AnimationState animationState = null;
            switch (animationConfig.fadeOutMode)
            {
                case AnimationFadeOutMode.SameLayer:
                    for (; i < l; ++i)
                    {
                        animationState = _animationStates[i];
                        if (animationState.layer == animationConfig.layer)
                        {
                            animationState.FadeOut(animationConfig.fadeOutTime, animationConfig.pauseFadeOut);
                        }
                    }
                    break;

                case AnimationFadeOutMode.SameGroup:
                    for (; i < l; ++i)
                    {
                        animationState = _animationStates[i];
                        if (animationState.group == animationConfig.group)
                        {
                            animationState.FadeOut(animationConfig.fadeOutTime, animationConfig.pauseFadeOut);
                        }
                    }
                    break;

                case AnimationFadeOutMode.SameLayerAndGroup:
                    for (; i < l; ++i)
                    {
                        animationState = _animationStates[i];
                        if (animationState.layer == animationConfig.layer && 
                            animationState.group == animationConfig.group
                        )
                        {
                            animationState.FadeOut(animationConfig.fadeOutTime, animationConfig.pauseFadeOut);
                        }
                    }
                    break;

                case AnimationFadeOutMode.All:
                    for (; i < l; ++i)
                    {
                        animationState = _animationStates[i];
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

            if (_armature.inheritAnimation && _armature._parent != null) // Inherit parent animation timeScale.
            {
                passedTime *= _armature._parent._armature.animation.timeScale;
            }

            if (timeScale != 1.0f)
            {
                passedTime *= timeScale;
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

                        animationState._advanceTime(passedTime, 0.0f);
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

                _cacheFrameIndex = -1;
            }
            else
            {
                _cacheFrameIndex = -1;
            }

            _timelineStateDirty = false;
        }
        /**
         * @language zh_CN
         * 清除所有动画状态。
         * @see DragonBones.AnimationState
         * @version DragonBones 4.5
         */
        public void Reset()
        {
            for (int i = 0, l = _animationStates.Count; i < l; ++i)
            {
                _animationStates[i].ReturnToPool();
            }

            _isPlaying = false;
            _animationStateDirty = false;
            _timelineStateDirty = false;
            _cacheFrameIndex = -1;
            _animationConfig.Clear();
            _animationStates.Clear();
            _lastAnimationState = null;
        }
        /**
         * @language zh_CN
         * 暂停播放动画。
         * @param animationName 动画状态的名称，如果未设置，则暂停所有动画状态。
         * @see DragonBones.AnimationState
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
         * @language zh_CN
         * @beta
         * 通过动画配置来播放动画。
         * @param animationConfig 动画配置。
         * @returns 对应的动画状态。
         * @see DragonBones.AnimationConfig
         * @see DragonBones.AnimationState
         * @version DragonBones 5.0
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

            if (animationConfig.fadeInTime < 0.0f || float.IsNaN(animationConfig.fadeInTime))
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

            if (animationConfig.fadeOutTime < 0.0f || float.IsNaN(animationConfig.fadeOutTime))
            {
                animationConfig.fadeOutTime = animationConfig.fadeInTime;
            }

            if (animationConfig.timeScale <= -100.0f || float.IsNaN(animationConfig.timeScale)) //
            {
                animationConfig.timeScale = 1.0f / animationData.scale;
            }

            if (animationData.duration > 0.0f)
            {
                if (float.IsNaN(animationConfig.position))
                {
                    animationConfig.position = 0.0f;
                }
                else if (animationConfig.position < 0.0f)
                {
                    animationConfig.position %= animationData.duration;
                    animationConfig.position = animationData.duration - animationConfig.position;
                }
                else if (animationConfig.position == animationData.duration)
                {
                    animationConfig.position -= 0.000001f;
                }
                else if (animationConfig.position > animationData.duration)
                {
                    animationConfig.position %= animationData.duration;
                }

                if (animationConfig.duration > 0.0f && animationConfig.position + animationConfig.duration > animationData.duration)
                {
                    animationConfig.duration = animationData.duration - animationConfig.position;
                }

                if (animationConfig.duration == 0.0f)
                {
                    animationConfig.playTimes = 1;
                }
                else if (animationConfig.playTimes < 0)
                {
                    animationConfig.playTimes = (int)animationData.playTimes;
                }
            }
            else
            {
                animationConfig.playTimes = 1;
                animationConfig.position = 0.0f;
                animationConfig.duration = 0.0f;
            }

            _fadeOut(animationConfig);

            _lastAnimationState = BaseObject.BorrowObject<AnimationState>();
            _lastAnimationState._init(_armature, animationData, animationConfig);
            _animationStates.Add(_lastAnimationState);
            _animationStateDirty = true;
            _cacheFrameIndex = -1;

            if (_animationStates.Count > 1)
            {
                _animationStates.Sort(_sortAnimationState);
            }

            // Child armature play same name animation.
            var slots = _armature.GetSlots();
            for (int i = 0, l = slots.Count; i < l; ++i)
            {
                var childArmature = slots[i].childArmature;
                if (
                    childArmature != null && childArmature.inheritAnimation &&
                    childArmature.animation.HasAnimation(animationName) &&
                    childArmature.animation.GetState(animationName) == null
                )
                {
                    childArmature.animation.FadeIn(animationName); //
                }
            }

            if (animationConfig.fadeInTime <= 0.0f) // Blend animation state, update armature.
            {
                _armature.AdvanceTime(0.0f);
            }

            return _lastAnimationState;
        }
        /**
         * @language zh_CN
         * 淡入播放动画。
         * @param animationName 动画数据名称。
         * @param playTimes 播放次数。 [-1: 使用动画数据默认值, 0: 无限循环播放, [1~N]: 循环播放 N 次]
         * @param fadeInTime 淡入时间。 [-1: 使用动画数据默认值, [0~N]: 淡入时间] (以秒为单位)
         * @param layer 混合图层，图层高会优先获取混合权重。
         * @param group 混合组，用于动画状态编组，方便控制淡出。
         * @param fadeOutMode 淡出模式。
         * @returns 对应的动画状态。
         * @see DragonBones.AnimationFadeOutMode
         * @see DragonBones.AnimationState
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
         * @param animationName 动画数据名称，如果未设置，则播放默认动画，或将暂停状态切换为播放状态，或重新播放上一个正在播放的动画。 
         * @param playTimes 播放次数。 [-1: 使用动画数据默认值, 0: 无限循环播放, [1~N]: 循环播放 N 次]
         * @returns 对应的动画状态。
         * @see DragonBones.AnimationState
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
         * 从指定时间开始播放动画。
         * @param animationName 动画数据的名称。
         * @param time 开始时间。 (以秒为单位)
         * @param playTimes 播放次数。 [-1: 使用动画数据默认值, 0: 无限循环播放, [1~N]: 循环播放 N 次]
         * @returns 对应的动画状态。
         * @see DragonBones.AnimationState
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
         * 从指定帧开始播放动画。
         * @param animationName 动画数据的名称。
         * @param frame 帧。
         * @param playTimes 播放次数。 [-1: 使用动画数据默认值, 0: 无限循环播放, [1~N]: 循环播放 N 次]
         * @returns 对应的动画状态。
         * @see DragonBones.AnimationState
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
         * 从指定进度开始播放动画。
         * @param animationName 动画数据的名称。
         * @param progress 进度。 [0~1]
         * @param playTimes 播放次数。 [-1: 使用动画数据默认值, 0: 无限循环播放, [1~N]: 循环播放 N 次]
         * @returns 对应的动画状态。
         * @see DragonBones.AnimationState
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
         * 将动画停止到指定的时间。
         * @param animationName 动画数据的名称。
         * @param time 时间。 (以秒为单位)
         * @returns 对应的动画状态。
         * @see DragonBones.AnimationState
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
         * 将动画停止到指定的帧。
         * @param animationName 动画数据的名称。
         * @param frame 帧。
         * @returns 对应的动画状态。
         * @see DragonBones.AnimationState
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
         * 将动画停止到指定的进度。
         * @param animationName 动画数据的名称。
         * @param progress 进度。 [0 ~ 1]
         * @returns 对应的动画状态。
         * @see DragonBones.AnimationState
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
         * 获取动画状态。
         * @param animationName 动画状态的名称。
         * @see DragonBones.AnimationState
         * @version DragonBones 3.0
         */
        public AnimationState GetState(string animationName)
        {
            for (int i = 0, l = _animationStates.Count; i < l; ++i)
            {
                var animationState = _animationStates[i];
                if (animationState.name == animationName)
                {
                    return animationState;
                }
            }

            return null;
        }
        /**
         * @language zh_CN
         * 是否包含动画数据。
         * @param animationName 动画数据的名称。
         * @see DragonBones.AnimationData
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
         * @see DragonBones.AnimationState
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

                    for (int i = 0, l = _animationStates.Count; i < l; ++i)
                    {
                        if (!_animationStates[i].isCompleted)
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
         * @see DragonBones.AnimationState
         * @version DragonBones 3.0
         */
        public AnimationState lastAnimationState
        {
            get { return _lastAnimationState; }
        }
        /**
         * @language zh_CN
         * 一个可以快速使用的动画配置实例。
         * @see DragonBones.AnimationConfig
         * @version DragonBones 5.0
         */
        public AnimationConfig animationConfig
        {
            get
            {
                _animationConfig.Clear();
                return _animationConfig;
            }
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
         * @see DragonBones.AnimationData
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
                        _animations[pair.Key] = pair.Value;
                        _animationNames.Add(pair.Key);
                    }
                }
            }
        }
    }
}