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

        /**
         * @private
         */
        internal bool _animationStateDirty;

        /**
         * @private
         */
        internal bool _timelineStateDirty;

        /**
         * @private
         */
        internal Armature _armature;

        /**
         * @private
         */
        protected bool _isPlaying;

        /**
         * @private
         */
        protected float _time;

        /**
         * @private
         */
        protected AnimationState _lastAnimationState;

        /**
         * @private
         */
        protected readonly Dictionary<string, AnimationData> _animations = new Dictionary<string, AnimationData>();

        /**
         * @private
         */
        protected readonly List<AnimationState> _animationStates = new List<AnimationState>();

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
            foreach (var animationState in _animationStates)
            {
                animationState.ReturnToPool();
            }

            timeScale = 1.0f;

            _animationStateDirty = false;
            _timelineStateDirty = false;
            _armature = null;

            _isPlaying = false;
            _time = 0.0f;
            _lastAnimationState = null;
            _animationStates.Clear();
        }

        /**
         * @private
         */
        protected void _fadeOut(float fadeOutTime, float layer, string group, AnimationFadeOutMode fadeOutMode, bool pauseFadeOut)
        {
            switch (fadeOutMode)
            {
                case AnimationFadeOutMode.SameLayer:
                    foreach (var animationState in _animationStates)
                    {
                        if (animationState.layer == layer)
                        {
                            animationState.FadeOut(fadeOutTime, pauseFadeOut);
                        }
                    }
                    break;

                case AnimationFadeOutMode.SameGroup:
                    foreach (var animationState in _animationStates)
                    {
                        if (animationState.group == group)
                        {
                            animationState.FadeOut(fadeOutTime, pauseFadeOut);
                        }
                    }
                    break;

                case AnimationFadeOutMode.All:
                    foreach (var animationState in _animationStates)
                    {
                        if (fadeOutTime == 0.0f)
                        {
                            animationState.ReturnToPool();
                        }
                        else
                        {
                            animationState.FadeOut(fadeOutTime, pauseFadeOut);
                        }
                    }

                    if (fadeOutTime == 0.0f)
                    {
                        _animationStates.Clear();
                    }
                    break;

                case AnimationFadeOutMode.SameLayerAndGroup:
                    foreach (var animationState in _animationStates)
                    {
                        if (animationState.layer == layer && animationState.group == group)
                        {
                            animationState.FadeOut(fadeOutTime, pauseFadeOut);
                        }
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
        internal void _updateFFDTimelineStates()
        {
            foreach (var animationState in _animationStates)
            {
                animationState._updateFFDTimelineStates();
            }
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
                if (animationState._fadeState > 0 && animationState._fadeProgress <= 0.0f)
                {
                    animationState.ReturnToPool();
                    _animationStates.Clear();
                    _animationStateDirty = true;
                    _lastAnimationState = null;
                }
                else
                {
                    if (_timelineStateDirty)
                    {
                        animationState._updateTimelineStates();
                    }

                    animationState._advanceTime(passedTime, 1.0f, 0);
                }
            }
            else if (animationStateCount > 1)
            {
                var prevLayer = _animationStates[0]._layer;
                var weightLeft = 1.0f;
                var layerTotalWeight = 0.0f;
                int animationIndex = 1; // If has multiply animation state, first index is 1.

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
                            if (i - r >= 0)
                            {
                                _lastAnimationState = _animationStates[i - r];
                            }
                            else
                            {
                                _lastAnimationState = null;
                            }
                        }
                    }
                    else
                    {
                        if (r > 0)
                        {
                            _animationStates[i - r] = animationState;
                        }

                        if (prevLayer != animationState._layer)
                        { // Update weight left.
                            prevLayer = animationState._layer;

                            if (layerTotalWeight >= weightLeft)
                            {
                                weightLeft = 0.0f;
                            }
                            else
                            {
                                weightLeft -= layerTotalWeight;
                            }

                            layerTotalWeight = 0.0f;
                        }

                        if (_timelineStateDirty)
                        {
                            animationState._updateTimelineStates();
                        }

                        animationState._advanceTime(passedTime, weightLeft, animationIndex);

                        if (animationState._weightResult > 0.0f)
                        { // Update layer total weight.
                            layerTotalWeight += animationState._weightResult;
                            animationIndex++;
                        }
                    }

                    if (i == animationStateCount - 1 && r > 0) // Modify animation states size.
                    {
                        DragonBones.ResizeList(_animationStates, animationStateCount - r, null);
                    }
                }
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
            _lastAnimationState = null;
            _animationStates.Clear();
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
            if (DragonBones.IsAvailableString(animationName))
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
         * 播放动画。
         * @param animationName 动画数据的名称，如果未设置，则播放默认动画，或将暂停状态切换为播放状态，或重新播放上一个正在播放的动画。 
         * @param playTimes 动画需要播放的次数。 [-1: 使用动画数据默认值, 0: 无限循环播放, [1~N]: 循环播放 N 次]
         * @returns 返回控制这个动画数据的动画状态。
         * @see dragonBones.AnimationState
         * @version DragonBones 3.0
         */
        public AnimationState Play(string animationName = null, int playTimes = -1)
        {
            var animationState = (AnimationState)null;
            if (DragonBones.IsAvailableString(animationName))
            {
                animationState = FadeIn(animationName, 0.0f, playTimes, 0, null, AnimationFadeOutMode.All);
            }
            else if (_lastAnimationState == null)
            {
                var defaultAnimation = _armature.armatureData.defaultAnimation;
                if (defaultAnimation != null)
                {
                    animationState = FadeIn(defaultAnimation.name, 0.0f, playTimes, 0, null, AnimationFadeOutMode.All);
                }
            }
            else if (!_isPlaying || (!_lastAnimationState.isPlaying && !_lastAnimationState.isCompleted))
            {
                _isPlaying = true;
                _lastAnimationState.Play();
            }
            else
            {
                animationState = FadeIn(_lastAnimationState.name, 0.0f, playTimes, 0, null, AnimationFadeOutMode.All);
            }

            return animationState;
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
         * @param additiveBlending 以叠加的形式混合。
         * @param displayControl 是否对显示对象属性可控。
         * @param pauseFadeOut 暂停需要淡出的动画。
         * @param pauseFadeIn 暂停需要淡入的动画，直到淡入结束才开始播放。
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
            if (!_animations.ContainsKey(animationName))
            {
                _time = 0;
                DragonBones.Warn(
                    "Non-existent animation. " +
                    " DragonBones: " + _armature.armatureData.parent.name +
                    " Armature: " + _armature.name +
                    " Animation: " + animationName
                );
                return null;
            }

            var animationData = _animations[animationName];

            if (float.IsNaN(_time))
            {
                _time = 0.0f;
            }

            _isPlaying = true;

            if (fadeInTime < 0.0f || float.IsNaN(fadeInTime))
            {
                if (_lastAnimationState != null)
                {
                    fadeInTime = animationData.fadeInTime;
                }
                else
                {
                    fadeInTime = 0.0f;
                }
            }

            if (playTimes < 0)
            {
                playTimes = (int)animationData.playTimes;
            }

            _fadeOut(fadeInTime, layer, group, fadeOutMode, pauseFadeOut);

            _lastAnimationState = BaseObject.BorrowObject<AnimationState>();
            _lastAnimationState._layer = layer;
            _lastAnimationState._group = group;
            _lastAnimationState.additiveBlending = additiveBlending;
            _lastAnimationState.displayControl = displayControl;
            _lastAnimationState._fadeIn(
                _armature, animationData.animation != null ? animationData.animation : animationData, animationName,
                (uint)playTimes, animationData.position, animationData.duration, _time, 1 / animationData.scale, fadeInTime,
                pauseFadeIn
            );
            _animationStates.Add(_lastAnimationState);
            _animationStateDirty = true;
            _time = 0.0f;
            _armature._cacheFrameIndex = -1;

            if (_animationStates.Count > 1)
            {
                _animationStates.Sort(_sortAnimationState);
            }

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

            if (fadeInTime <= 0.0f)
            {
                _armature.AdvanceTime(0.0f); // Blend animation state, update armature. (pass actions and events) 
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
            _time = time;

            return FadeIn(animationName, 0.0f, playTimes, 0, null, AnimationFadeOutMode.All);
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
            var animationData = _animations[animationName];
            if (animationData != null)
            {
                _time = animationData.duration * frame / animationData.frameCount;
            }

            return FadeIn(animationName, 0.0f, playTimes, 0, null, AnimationFadeOutMode.All);
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
            var animationData = _animations[animationName];
            if (animationData != null)
            {
                _time = animationData.duration * (progress > 0.0f ? progress : 0.0f);
            }

            return FadeIn(animationName, 0.0f, playTimes, 0, null, AnimationFadeOutMode.All);
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
                        if (animationState.isCompleted)
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
         * 所有动画数据名称。
         * @see #animations
         * @version DragonBones 4.5
         */
        public List<string> animationNames
        {
            get { return _armature._armatureData.animationNames; }
        }

        /**
         * @language zh_CN
         * 所有的动画数据。
         * @see dragonBones.AnimationData
         * @version DragonBones 4.5
         */
        public Dictionary<string, AnimationData> animations
        {
            get { return this._animations; }
            set
            {
                if (_animations == value)
                {
                    return;
                }

                _animations.Clear();

                if (value != null)
                {
                    foreach (var pair in value)
                    {
                        this._animations[pair.Key] = value[pair.Key];
                    }
                }
            }
        }
    }
}