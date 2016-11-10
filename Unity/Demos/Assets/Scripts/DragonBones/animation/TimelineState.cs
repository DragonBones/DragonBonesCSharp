using System.Collections.Generic;

namespace DragonBones
{
    /**
     * @private
     */
    public class AnimationTimelineState : TimelineState<AnimationFrameData, AnimationData>
    {
        private bool _isStarted;

        public AnimationTimelineState()
        {
        }

        /**
         * @inheritDoc
         */
        override protected void _onClear()
        {
            base._onClear();

            _isStarted = false;
        }

        protected void _onCrossFrame(AnimationFrameData frame)
        {
            if (this._animationState.actionEnabled)
            {
                foreach (var action in frame.actions)
                {
                    _armature._bufferAction(action);
                }
            }

            var eventDispatcher = _armature._eventDispatcher;
            foreach (var eventData in frame.events)
            {
                var eventType = "";
                switch (eventData.type)
                {
                    case EventType.Frame:
                        eventType = EventObject.FRAME_EVENT;
                        break;

                    case EventType.Sound:
                        eventType = EventObject.SOUND_EVENT;
                        break;
                }

                if (
                    eventDispatcher.HasEventListener(eventType) || 
                    (eventData.type == EventType.Sound && _armature._eventManager.HasEventListener(eventType))
                )
                {
                    var eventObject = BaseObject.BorrowObject<EventObject>();
                    eventObject.animationState = this._animationState;
                    eventObject.frame = frame;

                    if (eventData.bone != null)
                    {
                        eventObject.bone = _armature.GetBone(eventData.bone.name);
                    }

                    if (eventData.slot != null)
                    {
                        eventObject.slot = _armature.GetSlot(eventData.slot.name);
                    }

                    eventObject.name = eventData.name;
                    // eventObject.data = eventData.data;

                    _armature._bufferEvent(eventObject, eventType);
                }
            }
        }

        override public void FadeIn(Armature armature, AnimationState animationState, AnimationData timelineData, float time)
        {
            base.FadeIn(armature, animationState, timelineData, time);

            _currentTime = time; // Pass first update. (armature.advanceTime(0))
        }

        override public void Update(float time)
        {
            var prevTime = _currentTime;
            var prevPlayTimes = _currentPlayTimes;

            if (!_isCompleted && _setCurrentTime(time))
            {
                var eventDispatcher = _armature._eventDispatcher;

                if (!_isStarted)
                {
                    _isStarted = true;

                    if (eventDispatcher.HasEventListener(EventObject.START))
                    {
                        var eventObject = BaseObject.BorrowObject<EventObject>();
                        eventObject.animationState = this._animationState;
                        _armature._bufferEvent(eventObject, EventObject.START);
                    }
                }

                if (_keyFrameCount > 0)
                {
                    var currentFrameIndex = _keyFrameCount > 1 ? (int)(_currentTime * _frameRate) : 0;
                    var currentFrame = _timeline.frames[currentFrameIndex];
                    if (_currentFrame != currentFrame)
                    {
                        if (_keyFrameCount > 1)
                        {
                            var crossedFrame = _currentFrame;
                            _currentFrame = currentFrame;

                            if (crossedFrame == null)
                            {
                                var prevFrameIndex = (int)(prevTime * _frameRate);
                                crossedFrame = _timeline.frames[prevFrameIndex];

                                if (_isReverse)
                                {
                                }
                                else
                                {
                                    if (
                                        prevTime <= crossedFrame.position ||
                                        prevPlayTimes != _currentPlayTimes
                                    )
                                    {
                                        crossedFrame = crossedFrame.prev;
                                    }
                                }
                            }

                            // TODO 1 2 3 key frame loop, first key frame after loop complete.
                            if (_isReverse)
                            {
                                while (crossedFrame != currentFrame)
                                {
                                    _onCrossFrame(crossedFrame);
                                    crossedFrame = crossedFrame.prev;
                                }
                            }
                            else
                            {
                                while (crossedFrame != currentFrame)
                                {
                                    crossedFrame = crossedFrame.next;
                                    _onCrossFrame(crossedFrame);
                                }
                            }
                        }
                        else
                        {
                            _currentFrame = currentFrame;
                            _onCrossFrame(_currentFrame);
                        }
                    }
                }

                if (prevPlayTimes != _currentPlayTimes)
                {
                    if (eventDispatcher.HasEventListener(EventObject.LOOP_COMPLETE))
                    {
                        var eventObject = BaseObject.BorrowObject<EventObject>();
                        eventObject.animationState = this._animationState;
                        _armature._bufferEvent(eventObject, EventObject.LOOP_COMPLETE);
                    }

                    if (_isCompleted && eventDispatcher.HasEventListener(EventObject.COMPLETE))
                    {
                        var eventObject = BaseObject.BorrowObject<EventObject>();
                        eventObject.animationState = this._animationState;
                        _armature._bufferEvent(eventObject, EventObject.COMPLETE);
                    }
                    _currentFrame = null;
                }
            }
        }

        public void setCurrentTime(float value)
        {
            this._setCurrentTime(value);
            this._currentFrame = null;
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

        override protected void _onArriveAtFrame(bool isUpdate)
        {
            base._onArriveAtFrame(isUpdate);
            this._armature._sortZOrder(this._currentFrame.zOrder);
        }
}

    /**
     * @private
     */
    public class BoneTimelineState : TweenTimelineState<BoneFrameData, BoneTimelineData>
    {
        public Bone bone;

        private TweenType _tweenTransform;
        private TweenType _tweenRotate;
        private TweenType _tweenScale;
        private Transform _boneTransform;
        private Transform _originTransform;
        private readonly Transform _transform = new Transform();
        private readonly Transform _currentTransform = new Transform();
        private readonly Transform _durationTransform = new Transform();

        public BoneTimelineState()
        {
        }

        /**
         * @inheritDoc
         */
        override protected void _onClear()
        {
            base._onClear();

            bone = null;

            _tweenTransform = TweenType.None;
            _tweenRotate = TweenType.None;
            _tweenScale = TweenType.None;
            _boneTransform = null;
            _originTransform = null;
            _transform.Identity();
            _currentTransform.Identity();
            _durationTransform.Identity();
        }

        override protected void _onArriveAtFrame(bool isUpdate)
        {
            base._onArriveAtFrame(isUpdate);

            _currentTransform.CopyFrom(this._currentFrame.transform);

            _tweenTransform = TweenType.Once;
            _tweenRotate = TweenType.Once;
            _tweenScale = TweenType.Once;

            if (_keyFrameCount > 1 && (this._tweenEasing != DragonBones.NO_TWEEN || this._curve != null))
            {
                var nextFrame = this._currentFrame.next;
                var nextTransform = nextFrame.transform;

                // Transform.
                _durationTransform.x = nextTransform.x - _currentTransform.x;
                _durationTransform.y = nextTransform.y - _currentTransform.y;
                if (_durationTransform.x != 0.0f || _durationTransform.y != 0.0f)
                {
                    _tweenTransform = TweenType.Always;
                }

                // Rotate.
                var tweenRotate = _currentFrame.tweenRotate;
                if (!float.IsNaN(tweenRotate))
                {
                    if (tweenRotate != 0.0f)
                    {
                        if (tweenRotate > 0.0f ? nextTransform.skewY >= _currentTransform.skewY : nextTransform.skewY <= _currentTransform.skewY)
                        {
                            var rotate = tweenRotate > 0 ? tweenRotate - 1.0f : tweenRotate + 1.0f;
                            _durationTransform.skewX = nextTransform.skewX - _currentTransform.skewX + DragonBones.PI_D * rotate;
                            _durationTransform.skewY = nextTransform.skewY - _currentTransform.skewY + DragonBones.PI_D * rotate;
                        }
                        else
                        {
                            _durationTransform.skewX = nextTransform.skewX - _currentTransform.skewX + DragonBones.PI_D * tweenRotate;
                            _durationTransform.skewY = nextTransform.skewY - _currentTransform.skewY + DragonBones.PI_D * tweenRotate;
                        }
                    }
                    else
                    {
                        _durationTransform.skewX = Transform.NormalizeRadian(nextTransform.skewX - _currentTransform.skewX);
                        _durationTransform.skewY = Transform.NormalizeRadian(nextTransform.skewY - _currentTransform.skewY);
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
                    _durationTransform.scaleX = nextTransform.scaleX - _currentTransform.scaleX;
                    _durationTransform.scaleY = nextTransform.scaleY - _currentTransform.scaleY;
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

        override protected void _onUpdateFrame(bool isUpdate)
        {
            if (_tweenTransform != TweenType.None || _tweenRotate != TweenType.None || _tweenScale != TweenType.None)
            {
                base._onUpdateFrame(isUpdate);

                var tweenProgress = 0.0f;

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

                    if (this._animationState.additiveBlending) // Additive blending.
                    {
                        _transform.x = _currentTransform.x + _durationTransform.x * tweenProgress;
                        _transform.y = _currentTransform.y + _durationTransform.y * tweenProgress;
                    }
                    else // Normal blending.
                    {
                        _transform.x = _originTransform.x + _currentTransform.x + _durationTransform.x * tweenProgress;
                        _transform.y = _originTransform.y + _currentTransform.y + _durationTransform.y * tweenProgress;
                    }
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

                    if (this._animationState.additiveBlending) // Additive blending.
                    {
                        _transform.skewX = _currentTransform.skewX + _durationTransform.skewX * tweenProgress;
                        _transform.skewY = _currentTransform.skewY + _durationTransform.skewY * tweenProgress;
                    }
                    else // Normal blending.
                    {
                        _transform.skewX = _originTransform.skewX + _currentTransform.skewX + _durationTransform.skewX * tweenProgress;
                        _transform.skewY = _originTransform.skewY + _currentTransform.skewY + _durationTransform.skewY * tweenProgress;
                    }
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

                    if (this._animationState.additiveBlending) // Additive blending.
                    {
                        _transform.scaleX = _currentTransform.scaleX + _durationTransform.scaleX * tweenProgress;
                        _transform.scaleY = _currentTransform.scaleY + _durationTransform.scaleY * tweenProgress;
                    }
                    else // Normal blending.
                    {
                        _transform.scaleX = _originTransform.scaleX * (_currentTransform.scaleX + _durationTransform.scaleX * tweenProgress);
                        _transform.scaleY = _originTransform.scaleY * (_currentTransform.scaleY + _durationTransform.scaleY * tweenProgress);
                    }
                }

                bone.InvalidUpdate();
            }
        }

        override public void FadeIn(Armature armature, AnimationState animationState, BoneTimelineData timelineData, float time)
        {
            base.FadeIn(armature, animationState, timelineData, time);

            _originTransform = _timeline.originTransform;
            _boneTransform = bone._animationPose;
        }

        override public void FadeOut()
        {
            _transform.skewX = Transform.NormalizeRadian(_transform.skewX);
            _transform.skewY = Transform.NormalizeRadian(_transform.skewY);
        }

        override public void Update(float time)
        {
            base.Update(time);

            // Blend animation state.
            var weight = this._animationState._weightResult;

            if (weight > 0.0f)
            {
                if (bone._blendIndex == 0)
                {
                    _boneTransform.x = _transform.x * weight;
                    _boneTransform.y = _transform.y * weight;
                    _boneTransform.skewX = _transform.skewX * weight;
                    _boneTransform.skewY = _transform.skewY * weight;
                    _boneTransform.scaleX = (_transform.scaleX - 1) * weight + 1.0f;
                    _boneTransform.scaleY = (_transform.scaleY - 1) * weight + 1.0f;
                }
                else
                {
                    _boneTransform.x += _transform.x * weight;
                    _boneTransform.y += _transform.y * weight;
                    _boneTransform.skewX += _transform.skewX * weight;
                    _boneTransform.skewY += _transform.skewY * weight;
                    _boneTransform.scaleX += (_transform.scaleX - 1.0f) * weight;
                    _boneTransform.scaleY += (_transform.scaleY - 1.0f) * weight;
                }

                bone._blendIndex++;
                
                if (this._animationState._fadeState != 0)
                {
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
        private ColorTransform _slotColor;
        private readonly ColorTransform _color = new ColorTransform();
        private readonly ColorTransform _durationColor = new ColorTransform();

        public SlotTimelineState()
        {
        }

        /**
         * @inheritDoc
         */
        override protected void _onClear()
        {
            base._onClear();

            slot = null;

            _colorDirty = false;
            _tweenColor = TweenType.None;
            _slotColor = null;
            _color.Identity();
            _durationColor.Identity();
        }

        override protected void _onArriveAtFrame(bool isUpdate)
        {
            base._onArriveAtFrame(isUpdate);

            if (this._animationState._isDisabled(slot))
            {
                _tweenEasing = DragonBones.NO_TWEEN;
                _curve = null;
                _tweenColor = TweenType.None;
                return;
            }

            if (slot._displayDataSet != null)
            {
                var displayIndex = _currentFrame.displayIndex;
                if (slot.displayIndex >= 0 && displayIndex >= 0)
                {
                    if (slot._displayDataSet.displays.Count > 1)
                    {
                        slot._setDisplayIndex(displayIndex);
                    }
                }
                else
                {
                    slot._setDisplayIndex(displayIndex);
                }

                slot._updateMeshData(true);
            }

            if (_currentFrame.displayIndex >= 0)
            {
                _tweenColor = TweenType.None;

                var currentColor = _currentFrame.color;

                if (this._keyFrameCount > 1 && (this._tweenEasing != DragonBones.NO_TWEEN || this._curve != null))
                {
                    var nextFrame = this._currentFrame.next;
                    var nextColor = nextFrame.color;
                    if (currentColor != nextColor && nextFrame.displayIndex >= 0)
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

        override protected void _onUpdateFrame(bool isUpdate)
        {
            base._onUpdateFrame(isUpdate);

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

        override public void FadeIn(Armature armature, AnimationState animationState, SlotTimelineData timelineData, float time)
        {
            base.FadeIn(armature, animationState, timelineData, time);

            _slotColor = slot._colorTransform;
        }

        override public void FadeOut()
        {
            _tweenColor = TweenType.None;
        }

        override public void Update(float time)
        {
            base.Update(time);

            // Fade animation.
            if (_tweenColor != TweenType.None || _colorDirty)
            {
                var weight = this._animationState._weightResult;
                if (weight > 0.0f)
                {
                    if (this._animationState._fadeState != 0)
                    {
                        var fadeProgress = this._animationState._fadeProgress;

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
    }

    /**
     * @private
     */
    public class FFDTimelineState : TweenTimelineState<ExtensionFrameData, FFDTimelineData>
    {
        public Slot slot;

        private TweenType _tweenFFD;
        private List<float> _slotFFDVertices;
        private ExtensionFrameData _durationFFDFrame;
        private readonly List<float> _ffdVertices = new List<float>();

        public FFDTimelineState()
        {
        }

        /**
         * @inheritDoc
         */
        override protected void _onClear()
        {
            base._onClear();

            slot = null;

            _tweenFFD = TweenType.None;
            _slotFFDVertices = null;

            if (_durationFFDFrame != null)
            {
                _durationFFDFrame.ReturnToPool();
                _durationFFDFrame = null;
            }

            _ffdVertices.Clear();
        }

        override protected void _onArriveAtFrame(bool isUpdate)
        {
            base._onArriveAtFrame(isUpdate);

            _tweenFFD = TweenType.None;

            if (this._tweenEasing != DragonBones.NO_TWEEN || this._curve != null)
            {
                _tweenFFD = _updateExtensionKeyFrame(_currentFrame, _currentFrame.next, _durationFFDFrame);
            }

            if (_tweenFFD == TweenType.None)
            {
                var currentFFDVertices = _currentFrame.tweens;
                for (int i = 0, l = currentFFDVertices.Count; i < l; ++i)
                {
                    if (_slotFFDVertices[i] != currentFFDVertices[i])
                    {
                        _tweenFFD = TweenType.Once;
                        break;
                    }
                }
            }
        }

        override protected void _onUpdateFrame(bool isUpdate)
        {
            base._onUpdateFrame(isUpdate);

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
                var nextFFDVertices = _durationFFDFrame.tweens;
                for (int i = 0, l = currentFFDVertices.Count; i < l; ++i)
                {
                    _ffdVertices[i] = currentFFDVertices[i] + nextFFDVertices[i] * tweenProgress;
                }

                slot._ffdDirty = true;
            }
        }

        override public void FadeIn(Armature armature, AnimationState animationState, FFDTimelineData timelineData, float time)
        {
            base.FadeIn(armature, animationState, timelineData, time);

            _slotFFDVertices = slot._ffdVertices;
            _durationFFDFrame = BaseObject.BorrowObject<ExtensionFrameData>();
            DragonBones.ResizeList(_durationFFDFrame.tweens, _slotFFDVertices.Count, 0.0f);
            DragonBones.ResizeList(_ffdVertices, _slotFFDVertices.Count, 0.0f);

            for (int i = 0, l = _durationFFDFrame.tweens.Count; i < l; ++i)
            {
                _durationFFDFrame.tweens[i] = 0.0f;
            }

            for (int i = 0, l = _ffdVertices.Count; i < l; ++i)
            {
                _ffdVertices[i] = 0.0f;
            }
        }

        override public void Update(float time)
        {
            base.Update(time);

            // Blend animation.
            var weight = this._animationState._weightResult;
            if (weight > 0.0f)
            {
                if (slot._blendIndex == 0)
                {
                    for (int i = 0, l = _ffdVertices.Count; i < l; ++i)
                    {
                        _slotFFDVertices[i] = _ffdVertices[i] * weight;
                    }
                }
                else
                {
                    for (int i = 0, l = _ffdVertices.Count; i < l; ++i)
                    {
                        _slotFFDVertices[i] += _ffdVertices[i] * weight;
                    }
                }

                slot._blendIndex++;
               
                if (this._animationState._fadeState != 0)
                {
                    slot._ffdDirty = true;
                }
            }
        }
    }
}