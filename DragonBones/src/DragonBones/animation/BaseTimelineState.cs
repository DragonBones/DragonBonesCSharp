using System;
using System.Collections.Generic;

namespace DragonBones
{
    /**
     * @private
     */
    public enum TweenType
    {
        None = 0,
        Once = 1,
        Always = 2
    }

    /**
     * @private
     */
    abstract public class TimelineState<T, M> : BaseObject
        where T : FrameData<T>
        where M : TimelineData<T>
    {
        internal bool _isCompleted;
        internal uint _currentPlayTimes;
        internal float _currentTime;
        internal M _timeline;

        protected bool _isReverse;
        protected bool _hasAsynchronyTimeline;
        protected uint _frameRate;
        protected uint _keyFrameCount;
        protected uint _frameCount;
        protected float _position;
        protected float _duration;
        protected float _animationDutation;
        protected float _timeScale;
        protected float _timeOffset;
        protected T _currentFrame;
        protected Armature _armature;
        protected AnimationState _animationState;

        public TimelineState()
        {
        }

        /**
         * @inheritDoc
         */
        override protected void _onClear()
        {
            _isCompleted = false;
            _currentPlayTimes = 0;
            _currentTime = -1.0f;
            _timeline = null;

            _isReverse = false;
            _hasAsynchronyTimeline = false;
            _frameRate = 0;
            _keyFrameCount = 0;
            _frameCount = 0;
            _position = 0.0f;
            _duration = 0.0f;
            _animationDutation = 0.0f;
            _timeScale = 1.0f;
            _timeOffset = 0.0f;
            _currentFrame = null;
            _armature = null;
            _animationState = null;
        }

        virtual protected void _onUpdateFrame(bool isUpdate) { }
        virtual protected void _onArriveAtFrame(bool isUpdate) { }

        protected bool _setCurrentTime(float value)
        {
            uint currentPlayTimes = 0;

            if (_keyFrameCount == 1 && this != (object)_animationState._timeline)
            {
                _isCompleted = true;
                currentPlayTimes = 1;
            }
            else if (_hasAsynchronyTimeline)
            {
                var playTimes = _animationState.playTimes;
                var totalTime = playTimes * _duration;

                value *= _timeScale;
                if (_timeOffset != 0.0f)
                {
                    value += _timeOffset * _animationDutation;
                }

                if (playTimes > 0 && (value >= totalTime || value <= -totalTime))
                {
                    _isCompleted = true;
                    currentPlayTimes = playTimes;

                    if (value < 0.0f)
                    {
                        value = 0.0f;
                    }
                    else
                    {
                        value = _duration;
                    }
                }
                else
                {
                    _isCompleted = false;

                    if (value < 0.0f)
                    {
                        currentPlayTimes = (uint)(-value / _duration);
                        value = _duration - (-value % _duration);
                    }
                    else
                    {
                        currentPlayTimes = (uint)(value / _duration);
                        value %= _duration;
                    }

                    if (playTimes > 0 && currentPlayTimes > playTimes)
                    {
                        currentPlayTimes = playTimes;
                    }
                }

                value += _position;
            }
            else
            {
                _isCompleted = _animationState._timeline._isCompleted;
                currentPlayTimes = _animationState._timeline._currentPlayTimes;
            }

            if (_currentTime == value)
            {
                return false;
            }

            _isReverse = _currentTime > value && _currentPlayTimes == currentPlayTimes;
            _currentTime = value;
            _currentPlayTimes = currentPlayTimes;

            return true;
        }

        virtual public void FadeIn(Armature armature, AnimationState animationState, M timelineData, float time)
        {
            _armature = armature;
            _animationState = animationState;
            _timeline = timelineData;

            var isMainTimeline = this == (object)_animationState._timeline;

            _hasAsynchronyTimeline = isMainTimeline || _animationState.animationData.hasAsynchronyTimeline;
            _frameRate = _armature.armatureData.frameRate;
            _keyFrameCount = (uint)_timeline.frames.Count;
            _frameCount = _animationState.animationData.frameCount;
            _position = _animationState._position;
            _duration = _animationState._duration;
            _animationDutation = _animationState.animationData.duration;
            _timeScale = isMainTimeline ? 1.0f : (1.0f / _timeline.scale);
            _timeOffset = isMainTimeline ? 0.0f : _timeline.offset;
        }

        virtual public void FadeOut() { }

        virtual public void Update(float time)
        {
            if (!_isCompleted && _setCurrentTime(time))
            {
                var currentFrameIndex = _keyFrameCount > 1 ? (int)(_currentTime * _frameRate) : 0;
                var currentFrame = _timeline.frames[currentFrameIndex];

                if (_currentFrame != currentFrame)
                {
                    _currentFrame = currentFrame;
                    _onArriveAtFrame(true);
                }

                _onUpdateFrame(true);
            }
        }
    }

    /**
     * @private
     */
    abstract public class TweenTimelineState<T, M> : TimelineState<T, M>
        where T : TweenFrameData<T>
        where M : TimelineData<T>
    {
        internal static float _getEasingValue(float progress, float easing)
        {
            if (progress <= 0.0f)
            {
                return 0.0f;
            }
            else if (progress >= 1.0f)
            {
                return 1.0f;
            }

            var value = 1.0f;
            if (easing > 2.0f)
            {
                return progress;
            }
            else if (easing > 1.0f) // Ease in out.
            {
                value = 0.5f * (1.0f - (float)Math.Cos(progress * Math.PI));
                easing -= 1.0f;
            }
            else if (easing > 0.0f) // Ease out.
            {
                value = 1.0f - (float)Math.Pow(1.0f - progress, 2.0f);
            }
            else if (easing >= -1.0f) // Ease in.
            {
                easing *= -1.0f;
                value = (float)Math.Pow(progress, 2.0f);
            }
            else if (easing >= -2.0f) // Ease out in.
            {
                easing *= -1.0f;
                value = (float)(Math.Acos(1.0f - progress * 2.0f) / Math.PI);
                easing -= 1.0f;
            }
            else
            {
                return progress;
            }

            return (value - progress) * easing + progress;
        }

        internal static float _getCurveEasingValue(float progress, float[] samples)
        {
            if (progress <= 0.0f)
            {
                return 0.0f;
            }
            else if (progress >= 1.0f)
            {
                return 1.0f;
            }

            var segmentCount = samples.Length + 1; // + 2 - 1
            var valueIndex = (uint)(progress * segmentCount); // floor
            var fromValue = valueIndex == 0 ? 0 : samples[valueIndex - 1];
            var toValue = (valueIndex == segmentCount - 1) ? 1 : samples[valueIndex];

            return fromValue + (toValue - fromValue) * (progress - valueIndex / segmentCount);
        }

        protected float _tweenProgress;
        protected float _tweenEasing;
        protected float[] _curve;

        public TweenTimelineState()
        {
        }

        /**
         * @inheritDoc
         */
        override protected void _onClear()
        {
            base._onClear();

            _tweenProgress = 0.0f;
            _tweenEasing = DragonBones.NO_TWEEN;
            _curve = null;
        }

        override protected void _onArriveAtFrame(bool isUpdate)
        {
            _tweenEasing = _currentFrame.tweenEasing;
            _curve = _currentFrame.curve;

            if (
                _keyFrameCount <= 1 ||
                (
                    _currentFrame.next == _timeline.frames[0] &&
                    (_tweenEasing != DragonBones.NO_TWEEN || _curve != null) &&
                    _animationState.playTimes > 0 &&
                    _animationState.currentPlayTimes == _animationState.playTimes - 1
                )
            )
            {
                _tweenEasing = DragonBones.NO_TWEEN;
                _curve = null;
            }
        }

        override protected void _onUpdateFrame(bool isUpdate)
        {
            if (_tweenEasing != DragonBones.NO_TWEEN)
            {
                _tweenProgress = (_currentTime - _currentFrame.position + _position) / _currentFrame.duration;
                if (_tweenEasing != 0.0f)
                {
                    _tweenProgress = _getEasingValue(_tweenProgress, _tweenEasing);
                }
            }
            else if (_curve != null)
            {
                _tweenProgress = (_currentTime - _currentFrame.position + _position) / _currentFrame.duration;
                _tweenProgress = _getCurveEasingValue(_tweenProgress, _curve);
            }
            else
            {
                _tweenProgress = 0.0f;
            }
        }

        protected TweenType _updateExtensionKeyFrame(ExtensionFrameData current, ExtensionFrameData next, ExtensionFrameData result)
        {
            var tweenType = TweenType.None;
            if (current.type == next.type)
            {
                for (int i = 0, l = current.tweens.Count; i < l; ++i)
                {
                    var tweenDuration = next.tweens[i] - current.tweens[i];
                    result.tweens[i] = tweenDuration;

                    if (tweenDuration > 0.0f)
                    {
                        tweenType = TweenType.Always;
                    }
                }
            }

            if (tweenType == TweenType.None)
            {
                if (result.type != current.type)
                {
                    tweenType = TweenType.Once;
                    result.type = current.type;
                }

                if (result.tweens.Count != current.tweens.Count)
                {
                    tweenType = TweenType.Once;
                    DragonBones.ResizeList(result.tweens, current.tweens.Count, 0.0f);
                }

                if (result.keys.Count != current.keys.Count)
                {
                    tweenType = TweenType.Once;
                    DragonBones.ResizeList(result.keys, current.keys.Count, 0);
                }

                for (int i = 0, l = current.keys.Count; i < l; ++i)
                {
                    var key = current.keys[i];
                    if (result.keys[i] != key)
                    {
                        tweenType = TweenType.Once;
                        result.keys[i] = key;
                    }
                }
            }

            return tweenType;
        }
    }
}