using System;
using System.Collections.Generic;

namespace DragonBones
{
    /**
     * @private
     */
    public class AnimationTimelineState : TimelineState<AnimationFrameData, AnimationData>
    {
        public AnimationTimelineState()
        {
        }

        protected void _onCrossFrame(AnimationFrameData frame)
        {
            if (_animationState.actionEnabled)
            {
                var actions = frame.actions;
                for (int i = 0, l = actions.Count; i < l; ++i)
                {
                    _armature._bufferAction(actions[i]);
                }
            }

            var eventDispatcher = _armature.eventDispatcher;
            foreach (var eventData in frame.events)
            {
                string eventType = null;
                switch (eventData.type)
                {
                    case EventType.Frame:
                        eventType = EventObject.FRAME_EVENT;
                        break;

                    case EventType.Sound:
                        eventType = EventObject.SOUND_EVENT;
                        break;
                }

                if (eventDispatcher.HasEventListener(eventType) || eventData.type == EventType.Sound)
                {
                    var eventObject = BaseObject.BorrowObject<EventObject>();
                    eventObject.name = eventData.name;
                    eventObject.frame = frame;
                    eventObject.data = eventData.data;
                    eventObject.animationState = _animationState;

                    if (eventData.bone != null)
                    {
                        eventObject.bone = _armature.GetBone(eventData.bone.name);
                    }

                    if (eventData.slot != null)
                    {
                        eventObject.slot = _armature.GetSlot(eventData.slot.name);
                    }

                    _armature._bufferEvent(eventObject, eventType);
                }
            }
        }

        override public void Update(float passedTime)
        {
            var prevState = _playState;
            var prevTime = _currentTime;
            var prevPlayTimes = _currentPlayTimes;

            if (_playState <= 0 && _setCurrentTime(passedTime))
            {
                var eventDispatcher = _armature.eventDispatcher;

                if (prevState < 0 && _playState != prevState)
                {
                    if (_animationState.displayControl)
                    {
                        _armature._sortZOrder(null);
                    }

                    if (eventDispatcher.HasEventListener(EventObject.START))
                    {
                        var eventObject = BaseObject.BorrowObject<EventObject>();
                        eventObject.animationState = _animationState;
                        _armature._bufferEvent(eventObject, EventObject.START);
                    }
                }

                if (prevTime < 0.0f)
                {
                    return;
                }

                bool isReverse = false;
                EventObject loopCompleteEvent  = null;
                EventObject completeEvent  = null;

                if (_currentPlayTimes != prevPlayTimes)
                {
                    if (eventDispatcher.HasEventListener(EventObject.LOOP_COMPLETE))
                    {
                        loopCompleteEvent = BaseObject.BorrowObject<EventObject>();
                        loopCompleteEvent.animationState = _animationState;
                    }

                    if (_playState > 0)
                    {
                        if (eventDispatcher.HasEventListener(EventObject.COMPLETE))
                        {
                            completeEvent = BaseObject.BorrowObject<EventObject>();
                            completeEvent.animationState = _animationState;
                        }

                        isReverse = prevTime > this._currentTime;
                    }
                    else
                    {
                        isReverse = prevTime < this._currentTime;
                    }
                }
                else
                {
                    isReverse = prevTime > this._currentTime;
                }

                if (_keyFrameCount > 1)
                {
                    var currentFrameIndex = (int)(_currentTime * _frameRate); // uint
                    var currentFrame = _timelineData.frames[currentFrameIndex];
                    if (_currentFrame != currentFrame)
                    {
                        var crossedFrame = _currentFrame;
                        _currentFrame = currentFrame;

                        if (isReverse)
                        {
                            if (crossedFrame == null)
                            {
                                var prevFrameIndex = (int)(prevTime * _frameRate);
                                crossedFrame = _timelineData.frames[prevFrameIndex];

                                if (_currentPlayTimes == prevPlayTimes) // Start.
                                {
                                    if (crossedFrame == currentFrame) // Uncrossed.
                                    {
                                        crossedFrame = null;
                                    }
                                }
                            }

                            while (crossedFrame != null)
                            {
                                if (
                                    _position <= crossedFrame.position &&
                                    crossedFrame.position <= _position + _duration
                                ) // Support interval play.
                                {
                                    _onCrossFrame(crossedFrame);
                                }

                                if (loopCompleteEvent != null && crossedFrame == _timelineData.frames[0]) // Add loop complete event after first frame.
                                {
                                    _armature._bufferEvent(loopCompleteEvent, EventObject.LOOP_COMPLETE);
                                    loopCompleteEvent = null;
                                }

                                crossedFrame = crossedFrame.prev;

                                if (crossedFrame == currentFrame)
                                {
                                    break;
                                }
                            }
                        }
                        else
                        {
                            if (crossedFrame == null)
                            {
                                var prevFrameIndex = (int)(prevTime * _frameRate);
                                crossedFrame = _timelineData.frames[prevFrameIndex];

                                if (_currentPlayTimes == prevPlayTimes) // Start.
                                {
                                    if (prevTime <= crossedFrame.position) // Crossed.
                                    {
                                        crossedFrame = crossedFrame.prev;
                                    }
                                    else if (crossedFrame == currentFrame) // Uncrossed.
                                    {
                                        crossedFrame = null;
                                    }
                                }
                            }

                            while (crossedFrame != null)
                            {
                                crossedFrame = crossedFrame.next;

                                if (loopCompleteEvent != null && crossedFrame == _timelineData.frames[0]) // Add loop complete event before first frame.
                                {
                                    _armature._bufferEvent(loopCompleteEvent, EventObject.LOOP_COMPLETE);
                                    loopCompleteEvent = null;
                                }

                                if (
                                    _position <= crossedFrame.position &&
                                    crossedFrame.position <= _position + _duration
                                ) // Support interval play.
                                {
                                    _onCrossFrame(crossedFrame);
                                }

                                if (crossedFrame == currentFrame)
                                {
                                    break;
                                }
                            }
                        }
                    }
                    else if (_keyFrameCount > 0 && _currentFrame == null)
                    {
                        _currentFrame = _timelineData.frames[0];

                        if (_currentPlayTimes == prevPlayTimes) // Start.
                        {
                            if (prevTime <= _currentFrame.position)
                            {
                                _onCrossFrame(_currentFrame);
                            }
                        }
                        else if (_position <= _currentFrame.position) // Loop complete.
                        {
                            if (!isReverse && loopCompleteEvent != null) // Add loop complete event before first frame.
                            {
                                _armature._bufferEvent(loopCompleteEvent, EventObject.LOOP_COMPLETE);
                                loopCompleteEvent = null;
                            }

                            _onCrossFrame(_currentFrame);
                        }
                    }
                }
                
                if (loopCompleteEvent != null)
                {
                    _armature._bufferEvent(loopCompleteEvent, EventObject.LOOP_COMPLETE);
                    loopCompleteEvent = null;
                }

                if (completeEvent != null)
                {
                    _armature._bufferEvent(completeEvent, EventObject.COMPLETE);
                    completeEvent = null;
                }
            }
        }

        public void setCurrentTime(float value)
        {
            _setCurrentTime(value);
            _currentFrame = null;
        }
    }
    /**
     * @private
     */
    public class ZOrderTimelineState : TimelineState<ZOrderFrameData, ZOrderTimelineData>
    {
        public ZOrderTimelineState()
        {
        }

        override protected void _onArriveAtFrame()
        {
            base._onArriveAtFrame();

            _armature._sortZOrder(_currentFrame.zOrder);
        }
    }
    /**
     * @private
     */
    public class BoneTimelineState : TweenTimelineState<BoneFrameData, BoneTimelineData>
    {
        public Bone bone;

        private bool _transformDirty;
        private TweenType _tweenTransform;
        private TweenType _tweenRotate;
        private TweenType _tweenScale;
        private readonly Transform _transform = new Transform();
        private readonly Transform _durationTransform = new Transform();
        private Transform _boneTransform;
        private Transform _originalTransform;

        public BoneTimelineState()
        {
        }
        
        override protected void _onClear()
        {
            base._onClear();

            bone = null;

            _transformDirty = false;
            _tweenTransform = TweenType.None;
            _tweenRotate = TweenType.None;
            _tweenScale = TweenType.None;
            _transform.Identity();
            _durationTransform.Identity();
            _boneTransform = null;
            _originalTransform = null;
        }

        override protected void _onArriveAtFrame()
        {
            base._onArriveAtFrame();

            _tweenTransform = TweenType.Once;
            _tweenRotate = TweenType.Once;
            _tweenScale = TweenType.Once;

            if (_keyFrameCount > 1 && (_tweenEasing != DragonBones.NO_TWEEN || _curve != null))
            {
                var currentTransform = _currentFrame.transform;
                var nextFrame = _currentFrame.next;
                var nextTransform = nextFrame.transform;

                // Transform.
                _durationTransform.x = nextTransform.x - currentTransform.x;
                _durationTransform.y = nextTransform.y - currentTransform.y;
                if (_durationTransform.x != 0.0f || _durationTransform.y != 0.0f)
                {
                    _tweenTransform = TweenType.Always;
                }

                // Rotate.
                var tweenRotate = _currentFrame.tweenRotate;
                if (tweenRotate != DragonBones.NO_TWEEN)
                {
                    if (tweenRotate != 0.0f)
                    {
                        if (tweenRotate > 0.0f ? nextTransform.skewY >= currentTransform.skewY : nextTransform.skewY <= currentTransform.skewY)
                        {
                            tweenRotate = tweenRotate > 0.0f ? tweenRotate - 1.0f : tweenRotate + 1.0f;
                        }

                        _durationTransform.skewX = nextTransform.skewX - currentTransform.skewX + DragonBones.PI_D * tweenRotate;
                        _durationTransform.skewY = nextTransform.skewY - currentTransform.skewY + DragonBones.PI_D * tweenRotate;
                    }
                    else
                    {
                        _durationTransform.skewX = Transform.NormalizeRadian(nextTransform.skewX - currentTransform.skewX);
                        _durationTransform.skewY = Transform.NormalizeRadian(nextTransform.skewY - currentTransform.skewY);
                    }

                    if (_durationTransform.skewX != 0.0f || _durationTransform.skewY != 0.0f)
                    {
                        _tweenRotate = TweenType.Always;
                    }
                }
                else
                {
                    _durationTransform.skewX = 0.0f;
                    _durationTransform.skewY = 0.0f;
                }

                // Scale.
                if (_currentFrame.tweenScale)
                {
                    _durationTransform.scaleX = nextTransform.scaleX - currentTransform.scaleX;
                    _durationTransform.scaleY = nextTransform.scaleY - currentTransform.scaleY;
                    if (_durationTransform.scaleX != 0.0f || _durationTransform.scaleY != 0.0f)
                    {
                        _tweenScale = TweenType.Always;
                    }
                }
                else
                {
                    _durationTransform.scaleX = 0.0f;
                    _durationTransform.scaleY = 0.0f;
                }
            }
            else
            {
                _durationTransform.x = 0.0f;
                _durationTransform.y = 0.0f;
                _durationTransform.skewX = 0.0f;
                _durationTransform.skewY = 0.0f;
                _durationTransform.scaleX = 0.0f;
                _durationTransform.scaleY = 0.0f;
            }
        }

        override protected void _onUpdateFrame()
        {
            base._onUpdateFrame();

            var tweenProgress = 0.0f;
            var currentTransform = _currentFrame.transform;

            if (_tweenTransform != TweenType.None)
            {
                if (_tweenTransform == TweenType.Once)
                {
                    _tweenTransform = TweenType.None;
                    tweenProgress = 0.0f;
                }
                else
                {
                    tweenProgress = _tweenProgress;
                }

                if (_animationState.additiveBlending) // Additive blending.
                {
                    _transform.x = currentTransform.x + _durationTransform.x * tweenProgress;
                    _transform.y = currentTransform.y + _durationTransform.y * tweenProgress;
                }
                else // Normal blending.
                {
                    _transform.x = _originalTransform.x + currentTransform.x + _durationTransform.x * tweenProgress;
                    _transform.y = _originalTransform.y + currentTransform.y + _durationTransform.y * tweenProgress;
                }

                _transformDirty = true;
            }

            if (_tweenRotate != TweenType.None)
            {
                if (_tweenRotate == TweenType.Once)
                {
                    _tweenRotate = TweenType.None;
                    tweenProgress = 0.0f;
                }
                else
                {
                    tweenProgress = _tweenProgress;
                }

                if (_animationState.additiveBlending) // Additive blending.
                {
                    _transform.skewX = currentTransform.skewX + _durationTransform.skewX * tweenProgress;
                    _transform.skewY = currentTransform.skewY + _durationTransform.skewY * tweenProgress;
                }
                else // Normal blending.
                {
                    _transform.skewX = _originalTransform.skewX + currentTransform.skewX + _durationTransform.skewX * tweenProgress;
                    _transform.skewY = _originalTransform.skewY + currentTransform.skewY + _durationTransform.skewY * tweenProgress;
                }

                _transformDirty = true;
            }

            if (_tweenScale != TweenType.None)
            {
                if (_tweenScale == TweenType.Once)
                {
                    _tweenScale = TweenType.None;
                    tweenProgress = 0.0f;
                }
                else
                {
                    tweenProgress = _tweenProgress;
                }

                if (_animationState.additiveBlending) // Additive blending.
                {
                    _transform.scaleX = currentTransform.scaleX + _durationTransform.scaleX * tweenProgress;
                    _transform.scaleY = currentTransform.scaleY + _durationTransform.scaleY * tweenProgress;
                }
                else // Normal blending.
                {
                    _transform.scaleX = _originalTransform.scaleX * (currentTransform.scaleX + _durationTransform.scaleX * tweenProgress);
                    _transform.scaleY = _originalTransform.scaleY * (currentTransform.scaleY + _durationTransform.scaleY * tweenProgress);
                }

                _transformDirty = true;
            }
        }

        override public void _init(Armature armature, AnimationState animationState, BoneTimelineData timelineData)
        {
            base._init(armature, animationState, timelineData);

            _originalTransform = _timelineData.originTransform;
            _boneTransform = bone._animationPose;
        }

        override public void FadeOut()
        {
            _transform.skewX = Transform.NormalizeRadian(_transform.skewX);
            _transform.skewY = Transform.NormalizeRadian(_transform.skewY);
        }

        override public void Update(float passedTime)
        {
            // Blend animation state.
            var animationLayer = _animationState._layer;
            var weight = _animationState._weightResult;

            if (bone._updateState <= 0)
            {
                base.Update(passedTime);

                bone._blendLayer = animationLayer;
                bone._blendLeftWeight = 1.0f;
                bone._blendTotalWeight = weight;

                _boneTransform.x = _transform.x * weight;
                _boneTransform.y = _transform.y * weight;
                _boneTransform.skewX = _transform.skewX * weight;
                _boneTransform.skewY = _transform.skewY * weight;
                _boneTransform.scaleX = (_transform.scaleX - 1) * weight + 1.0f;
                _boneTransform.scaleY = (_transform.scaleY - 1) * weight + 1.0f;

                bone._updateState = 1;
            }
            else if (bone._blendLeftWeight > 0.0f)
            {
                if (bone._blendLayer != animationLayer)
                {
                    if (bone._blendTotalWeight >= bone._blendLeftWeight)
                    {
                        bone._blendLeftWeight = 0.0f;
                    }
                    else
                    {
                        bone._blendLayer = animationLayer;
                        bone._blendLeftWeight -= bone._blendTotalWeight;
                        bone._blendTotalWeight = 0.0f;
                    }
                }

                weight *= bone._blendLeftWeight;
                if (weight > 0.0f)
                {
                    base.Update(passedTime);

                    bone._blendTotalWeight += weight;

                    _boneTransform.x += _transform.x * weight;
                    _boneTransform.y += _transform.y * weight;
                    _boneTransform.skewX += _transform.skewX * weight;
                    _boneTransform.skewY += _transform.skewY * weight;
                    _boneTransform.scaleX += (_transform.scaleX - 1.0f) * weight;
                    _boneTransform.scaleY += (_transform.scaleY - 1.0f) * weight;

                    bone._updateState++;
                }
            }

            if (bone._updateState > 0)
            {
                if (_transformDirty || _animationState._fadeState != 0 || _animationState._subFadeState != 0)
                {
                    _transformDirty = false;

                    bone.InvalidUpdate();
                }
            }
        }
    }
    /**
     * @private
     */
    public class SlotTimelineState : TweenTimelineState<SlotFrameData, SlotTimelineData>
    {
        public Slot slot;

        private bool _colorDirty;
        private TweenType _tweenColor;
        private readonly ColorTransform _color = new ColorTransform();
        private readonly ColorTransform _durationColor = new ColorTransform();
        private ColorTransform _slotColor;

        public SlotTimelineState()
        {
        }
        
        override protected void _onClear()
        {
            base._onClear();

            slot = null;

            _colorDirty = false;
            _tweenColor = TweenType.None;
            _color.Identity();
            _durationColor.Identity();
            _slotColor = null;
        }

        override protected void _onArriveAtFrame()
        {
            base._onArriveAtFrame();

            if (_animationState._isDisabled(slot))
            {
                _tweenEasing = DragonBones.NO_TWEEN;
                _curve = null;
                _tweenColor = TweenType.None;
                return;
            }

            var displayIndex = _currentFrame.displayIndex;
            if (_playState >= 0 && slot.displayIndex != displayIndex)
            {
                slot._setDisplayIndex(displayIndex);
            }

            if (displayIndex >= 0)
            {
                _tweenColor = TweenType.None;

                var currentColor = _currentFrame.color;

                if (_tweenEasing != DragonBones.NO_TWEEN || _curve != null)
                {
                    var nextFrame = _currentFrame.next;
                    var nextColor = nextFrame.color;
                    if (currentColor != nextColor)
                    {
                        _durationColor.alphaMultiplier = nextColor.alphaMultiplier - currentColor.alphaMultiplier;
                        _durationColor.redMultiplier = nextColor.redMultiplier - currentColor.redMultiplier;
                        _durationColor.greenMultiplier = nextColor.greenMultiplier - currentColor.greenMultiplier;
                        _durationColor.blueMultiplier = nextColor.blueMultiplier - currentColor.blueMultiplier;
                        _durationColor.alphaOffset = nextColor.alphaOffset - currentColor.alphaOffset;
                        _durationColor.redOffset = nextColor.redOffset - currentColor.redOffset;
                        _durationColor.greenOffset = nextColor.greenOffset - currentColor.greenOffset;
                        _durationColor.blueOffset = nextColor.blueOffset - currentColor.blueOffset;

                        if (
                            _durationColor.alphaMultiplier != 0.0f ||
                            _durationColor.redMultiplier != 0.0f ||
                            _durationColor.greenMultiplier != 0.0f ||
                            _durationColor.blueMultiplier != 0.0f ||
                            _durationColor.alphaOffset != 0 ||
                            _durationColor.redOffset != 0 ||
                            _durationColor.greenOffset != 0 ||
                            _durationColor.blueOffset != 0
                        )
                        {
                            _tweenColor = TweenType.Always;
                        }
                    }
                }

                if (_tweenColor == TweenType.None)
                {
                    if (
                        _slotColor.alphaMultiplier != currentColor.alphaMultiplier ||
                        _slotColor.redMultiplier != currentColor.redMultiplier ||
                        _slotColor.greenMultiplier != currentColor.greenMultiplier ||
                        _slotColor.blueMultiplier != currentColor.blueMultiplier ||
                        _slotColor.alphaOffset != currentColor.alphaOffset ||
                        _slotColor.redOffset != currentColor.redOffset ||
                        _slotColor.greenOffset != currentColor.greenOffset ||
                        _slotColor.blueOffset != currentColor.blueOffset
                    )
                    {
                        _tweenColor = TweenType.Once;
                    }
                }
            }
            else
            {
                _tweenEasing = DragonBones.NO_TWEEN;
                _curve = null;
                _tweenColor = TweenType.None;
            }
        }

        override protected void _onUpdateFrame()
        {
            base._onUpdateFrame();

            var tweenProgress = 0.0f;

            if (_tweenColor != TweenType.None)
            {
                if (_tweenColor == TweenType.Once)
                {
                    _tweenColor = TweenType.None;
                    tweenProgress = 0.0f;
                }
                else
                {
                    tweenProgress = _tweenProgress;
                }

                var currentColor = _currentFrame.color;
                _color.alphaMultiplier = currentColor.alphaMultiplier + _durationColor.alphaMultiplier * tweenProgress;
                _color.redMultiplier = currentColor.redMultiplier + _durationColor.redMultiplier * tweenProgress;
                _color.greenMultiplier = currentColor.greenMultiplier + _durationColor.greenMultiplier * tweenProgress;
                _color.blueMultiplier = currentColor.blueMultiplier + _durationColor.blueMultiplier * tweenProgress;
                _color.alphaOffset = currentColor.alphaOffset + (int)(_durationColor.alphaOffset * tweenProgress);
                _color.redOffset = currentColor.redOffset + (int)(_durationColor.redOffset * tweenProgress);
                _color.greenOffset = currentColor.greenOffset + (int)(_durationColor.greenOffset * tweenProgress);
                _color.blueOffset = currentColor.blueOffset + (int)(_durationColor.blueOffset * tweenProgress);

                _colorDirty = true;
            }
        }

        override public void _init(Armature armature, AnimationState animationState, SlotTimelineData timelineData)
        {
            base._init(armature, animationState, timelineData);

            _slotColor = slot._colorTransform;
        }

        override public void FadeOut()
        {
            _tweenColor = TweenType.None;
        }

        override public void Update(float passedTime)
        {
            base.Update(passedTime);

            // Fade animation.
            if (_tweenColor != TweenType.None || _colorDirty)
            {
                if (_animationState._fadeState != 0 || _animationState._subFadeState != 0)
                {
                    var fadeProgress = (float)Math.Pow(_animationState._fadeProgress, 4.0f);

                    _slotColor.alphaMultiplier += (_color.alphaMultiplier - _slotColor.alphaMultiplier) * fadeProgress;
                    _slotColor.redMultiplier += (_color.redMultiplier - _slotColor.redMultiplier) * fadeProgress;
                    _slotColor.greenMultiplier += (_color.greenMultiplier - _slotColor.greenMultiplier) * fadeProgress;
                    _slotColor.blueMultiplier += (_color.blueMultiplier - _slotColor.blueMultiplier) * fadeProgress;
                    _slotColor.alphaOffset += (int)((_color.alphaOffset - _slotColor.alphaOffset) * fadeProgress);
                    _slotColor.redOffset += (int)((_color.redOffset - _slotColor.redOffset) * fadeProgress);
                    _slotColor.greenOffset += (int)((_color.greenOffset - _slotColor.greenOffset) * fadeProgress);
                    _slotColor.blueOffset += (int)((_color.blueOffset - _slotColor.blueOffset) * fadeProgress);

                    slot._colorDirty = true;
                }
                else if (_colorDirty)
                {
                    _colorDirty = false;

                    _slotColor.alphaMultiplier = _color.alphaMultiplier;
                    _slotColor.redMultiplier = _color.redMultiplier;
                    _slotColor.greenMultiplier = _color.greenMultiplier;
                    _slotColor.blueMultiplier = _color.blueMultiplier;
                    _slotColor.alphaOffset = _color.alphaOffset;
                    _slotColor.redOffset = _color.redOffset;
                    _slotColor.greenOffset = _color.greenOffset;
                    _slotColor.blueOffset = _color.blueOffset;

                    slot._colorDirty = true;
                }
            }
        }
    }
    /**
     * @private
     */
    public class FFDTimelineState : TweenTimelineState<ExtensionFrameData, FFDTimelineData>
    {
        public Slot slot;

        private bool _ffdDirty;
        private TweenType _tweenFFD;
        private readonly List<float> _ffdVertices = new List<float>();
        private readonly List<float> _durationFFDVertices = new List<float>();
        private List<float> _slotFFDVertices;

        public FFDTimelineState()
        {
        }
        
        override protected void _onClear()
        {
            base._onClear();

            slot = null;

            _ffdDirty = false;
            _tweenFFD = TweenType.None;
            _ffdVertices.Clear();
            _durationFFDVertices.Clear();
            _slotFFDVertices = null;
        }

        override protected void _onArriveAtFrame()
        {
            if (slot.displayIndex >= 0 && _animationState._isDisabled(slot))
            {
                _tweenEasing = DragonBones.NO_TWEEN;
                _curve = null;
                _tweenFFD = TweenType.None;
                return;
            }

            base._onArriveAtFrame();

            _tweenFFD = TweenType.None;

            if (_tweenEasing != DragonBones.NO_TWEEN || _curve != null)
            {
                var currentFFDVertices = _currentFrame.tweens;
                var nextFFDVertices = _currentFrame.next.tweens;
                for (int i = 0, l = currentFFDVertices.Count; i < l; ++i)
                {
                    var duration = nextFFDVertices[i] - currentFFDVertices[i];
                    _durationFFDVertices[i] = duration;
                    if (duration != 0.0f)
                    {
                        _tweenFFD = TweenType.Always;
                    }
                }
            }

            if (_tweenFFD == TweenType.None)
            {
                _tweenFFD = TweenType.Once;

                for (int i = 0, l = _durationFFDVertices.Count; i < l; ++i)
                {
                    _durationFFDVertices[i] = 0.0f;
                }
            }
        }

        override protected void _onUpdateFrame()
        {
            base._onUpdateFrame();

            var tweenProgress = 0.0f;

            if (_tweenFFD != TweenType.None)
            {
                if (_tweenFFD == TweenType.Once)
                {
                    _tweenFFD = TweenType.None;
                    tweenProgress = 0.0f;
                }
                else
                {
                    tweenProgress = _tweenProgress;
                }

                var currentFFDVertices = _currentFrame.tweens;
                for (int i = 0, l = currentFFDVertices.Count; i < l; ++i)
                {
                    _ffdVertices[i] = currentFFDVertices[i] + _durationFFDVertices[i] * tweenProgress;
                }

                _ffdDirty = true;
            }
        }

        override public void _init(Armature armature, AnimationState animationState, FFDTimelineData timelineData)
        {
            base._init(armature, animationState, timelineData);

            _slotFFDVertices = slot._ffdVertices;
            DragonBones.ResizeList(_ffdVertices, _timelineData.frames[0].tweens.Count, 0.0f);
            DragonBones.ResizeList(_durationFFDVertices, _ffdVertices.Count, 0.0f);

            for (int i = 0, l = _ffdVertices.Count; i < l; ++i)
            {
                _ffdVertices[i] = 0.0f;
            }

            for (int i = 0, l = _durationFFDVertices.Count; i < l; ++i)
            {
                _durationFFDVertices[i] = 0.0f;
            }
        }

        override public void FadeOut()
        {
            _tweenFFD = TweenType.None;
        }

        override public void Update(float passedTime)
        {
            if (slot.parent._blendLayer < _animationState._layer)
            {
                return;
            }

            base.Update(passedTime);
            
            if (slot._meshData != _timelineData.display.mesh)
            {
                return;
            }

            // Fade animation.
            if (_tweenFFD != TweenType.None || _ffdDirty)
            {
                if (_animationState._fadeState != 0 || _animationState._subFadeState != 0)
                {
                    var fadeProgress = (float)Math.Pow(_animationState._fadeProgress, 4.0f);

                    for (int i = 0, l = _ffdVertices.Count; i < l; ++i)
                    {
                        _slotFFDVertices[i] += (_ffdVertices[i] - _slotFFDVertices[i]) * fadeProgress;
                    }

                    slot._meshDirty = true;
                }
                else if (_ffdDirty)
                {
                    _ffdDirty = false;

                    for (int i = 0, l = _ffdVertices.Count; i < l; ++i)
                    {
                        _slotFFDVertices[i] = _ffdVertices[i];
                    }

                    slot._meshDirty = true;
                }
            }
        }
    }
}