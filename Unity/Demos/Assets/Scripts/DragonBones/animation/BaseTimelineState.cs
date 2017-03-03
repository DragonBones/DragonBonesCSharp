using System;
using System.Collections.Generic;

namespace DragonBones
{
    /**
     * @private
     */
    internal enum TweenType
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
        internal int _playState; // -1 start 0 play 1 complete
        internal uint _currentPlayTimes;
        internal float _currentTime;
        internal M _timelineData;

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
        protected AnimationTimelineState _mainTimeline;

        public TimelineState()
        {
        }
        
        override protected void _onClear()
        {
            _playState = -1;
            _currentPlayTimes = 0;
            _currentTime = -1.0f;
            _timelineData = null;

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
            _mainTimeline = null;
        }

        virtual protected void _onUpdateFrame() { }
        virtual protected void _onArriveAtFrame() { }

        protected bool _setCurrentTime(float passedTime)
        {
            var prevState = _playState;
            uint currentPlayTimes = 0;
            var currentTime = 0.0f;

            if (_mainTimeline != null && _keyFrameCount == 1)
            {
                _playState = _animationState._timeline._playState >= 0 ? 1 : -1;
                currentPlayTimes = 1;
                currentTime = _mainTimeline._currentTime;
            }
            else if (_mainTimeline == null || _timeScale != 1.0f || _timeOffset != 0.0f) // Scale and offset.
            {
                var playTimes = _animationState.playTimes;
                var totalTime = playTimes * _duration;

                passedTime *= _timeScale;
                if (_timeOffset != 0.0f)
                {
                    passedTime += _timeOffset * _animationDutation;
                }

                if (playTimes > 0 && (passedTime >= totalTime || passedTime <= -totalTime))
                {
                    if (_playState <= 0 && _animationState._playheadState == 3)
                    {
                        _playState = 1;
                    }

                    currentPlayTimes = playTimes;

                    if (passedTime < 0.0f)
                    {
                        currentTime = 0.0f;
                    }
                    else
                    {
                        currentTime = _duration;
                    }
                }
                else
                {
                    if (_playState != 0 && _animationState._playheadState == 3)
                    {
                        _playState = 0;
                    }

                    if (passedTime < 0.0f)
                    {
                        passedTime = -passedTime;
                        currentPlayTimes = (uint)(passedTime / _duration);
                        currentTime = _duration - (passedTime % _duration);
                    }
                    else
                    {
                        currentPlayTimes = (uint)(passedTime / _duration);
                        currentTime = passedTime % _duration;
                    }

                    currentTime += _position;
                }
            }
            else
            {
                _playState = _animationState._timeline._playState;
                currentPlayTimes = _animationState._timeline._currentPlayTimes;
                currentTime = _mainTimeline._currentTime;
            }

            if (_currentPlayTimes == currentPlayTimes && _currentTime == currentTime)
            {
                return false;
            }
            
            // Clear frame flag when timeline start or loopComplete.
            if (
                (prevState < 0 && _playState != prevState) || 
                (_playState <= 0 && _currentPlayTimes != currentPlayTimes)
            )
            {
                _currentFrame = null;
            }
            
            _currentPlayTimes = currentPlayTimes;
            _currentTime = currentTime;

            return true;
        }

        virtual public void _init(Armature armature, AnimationState animationState, M timelineData)
        {
            _armature = armature;
            _animationState = animationState;
            _timelineData = timelineData;
            _mainTimeline = _animationState._timeline;

            if (this == (object)_mainTimeline)
            {
                _mainTimeline = null;
            }

            _frameRate = _armature.armatureData.frameRate;
            _keyFrameCount = (uint)_timelineData.frames.Count;
            _frameCount = _animationState.animationData.frameCount;
            _position = _animationState._position;
            _duration = _animationState._duration;
            _animationDutation = _animationState.animationData.duration;
            _timeScale = _mainTimeline == null ? 1.0f : (1.0f / _timelineData.scale);
            _timeOffset = _mainTimeline == null ? 0.0f : _timelineData.offset;
        }

        virtual public void FadeOut() { }

        virtual public void Update(float passedTime)
        {
            if (_playState <= 0 && _setCurrentTime(passedTime))
            {
                var currentFrameIndex = _keyFrameCount > 1 ? (int)(_currentTime * _frameRate) : 0; // uint
                var currentFrame = _timelineData.frames[currentFrameIndex];

                if (_currentFrame != currentFrame)
                {
                    _currentFrame = currentFrame;
                    _onArriveAtFrame();
                }

                _onUpdateFrame();
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
                value = ((float)Math.Acos(1.0f - progress * 2.0f) / DragonBones.PI);
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
            var valueIndex = (int)(progress * segmentCount); // uint
            var fromValue = valueIndex == 0 ? 0.0f : samples[valueIndex - 1];
            var toValue = (valueIndex == segmentCount - 1) ? 1.0f : samples[valueIndex];

            return fromValue + (toValue - fromValue) * (progress * segmentCount - valueIndex);
        }

        protected float _tweenProgress;
        protected float _tweenEasing;
        protected float[] _curve;

        public TweenTimelineState()
        {
        }

        override protected void _onClear()
        {
            base._onClear();

            _tweenProgress = 0.0f;
            _tweenEasing = DragonBones.NO_TWEEN;
            _curve = null;
        }

        override protected void _onArriveAtFrame()
        {
            if (
                _keyFrameCount > 1 &&
                (
                    _currentFrame.next != _timelineData.frames[0] ||
                    _animationState.playTimes == 0 ||
                    _animationState.currentPlayTimes < _animationState.playTimes - 1
                )
            )
            {
                _tweenEasing = _currentFrame.tweenEasing;
                _curve = _currentFrame.curve;
            }
            else
            {
                _tweenEasing = DragonBones.NO_TWEEN;
                _curve = null;
            }
        }

        override protected void _onUpdateFrame()
        {
            if (_tweenEasing != DragonBones.NO_TWEEN)
            {
                _tweenProgress = (_currentTime - _currentFrame.position) / _currentFrame.duration;
                if (_tweenEasing != 0.0f)
                {
                    _tweenProgress = _getEasingValue(_tweenProgress, _tweenEasing);
                }
            }
            else if (_curve != null)
            {
                _tweenProgress = (_currentTime - _currentFrame.position) / _currentFrame.duration;
                _tweenProgress = _getCurveEasingValue(_tweenProgress, _curve);
            }
            else
            {
                _tweenProgress = 0.0f;
            }
        }
    }
}