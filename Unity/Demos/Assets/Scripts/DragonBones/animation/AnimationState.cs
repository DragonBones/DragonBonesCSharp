using System;
using System.Collections.Generic;

namespace DragonBones
{
    /**
     * @language zh_CN
     * 动画状态，播放动画时产生，可以对单个动画的播放进行更细致的控制和调节。
     * @see dragonBones.Animation
     * @see dragonBones.AnimationData
     * @version DragonBones 3.0
     */
    public class AnimationState : BaseObject
    {
        /**
         * @language zh_CN
         * 是否对插槽的颜色，显示序列索引，深度排序，行为等拥有控制的权限。
         * @see dragonBones.Slot#displayController
         * @version DragonBones 3.0
         */
        public bool displayControl;

        /**
         * @language zh_CN
         * 是否以叠加的方式混合动画。
         * @version DragonBones 3.0
         */
        public bool additiveBlending;

        /**
         * @private
         */
        public bool actionEnabled;

        /**
         * @language zh_CN
         * 需要播放的次数。 [0: 无限循环播放, [1~N]: 循环播放 N 次]
         * @version DragonBones 3.0
         */
        public uint playTimes;

        /**
         * @language zh_CN
         * 播放速度。 [(-N~0): 倒转播放, 0: 停止播放, (0~1): 慢速播放, 1: 正常播放, (1~N): 快速播放]
         * @default 1
         * @version DragonBones 3.0
         */
        public float timeScale;

        /**
         * @language zh_CN
         * 进行动画混合时的权重。
         * @default 1
         * @version DragonBones 3.0
         */
        public float weight;

        /**
         * @language zh_CN
         * 自动淡出时需要的时间，当设置一个大于等于 0 的值，动画状态将会在播放完成后自动淡出。 (以秒为单位)
         * @default -1
         * @version DragonBones 3.0
         */
        public float autoFadeOutTime;

        /**
         * @private
         */
        public float fadeTotalTime;

        /**
         * @private
         */
        internal int _playheadState;

        /**
         * @private
         */
        internal int _fadeState;
        /**
         * @private
         */
        internal int _subFadeState;

        /**
         * @private
         */
        internal int _layer;

        /**
         * @private
         */
        internal float _position;

        /**
         * @private
         */
        internal float _duration;

        /**
         * @private
         */
        private float _fadeTime;

        /**
         * @private
         */
        private float _time;

        /**
         * @private
         */
        internal float _fadeProgress;

        /**
         * @private
         */
        internal float _weightResult;

        /**
         * @private
         */
        private string _name;

        /**
         * @private
         */
        private string _group;

        /**
         * @private
         */
        private readonly List<string> _boneMask = new List<string>();

        /**
         * @private
         */
        private readonly List<BoneTimelineState> _boneTimelines = new List<BoneTimelineState>();

        /**
         * @private
         */
        private readonly List<SlotTimelineState> _slotTimelines = new List<SlotTimelineState>();

        /**
         * @private
         */
        private readonly List<FFDTimelineState> _ffdTimelines = new List<FFDTimelineState>();

        /**
         * @private
         */
        private AnimationData _animationData;

        /**
         * @private
         */
        private Armature _armature;

        /**
         * @private
         */
        internal AnimationTimelineState _timeline;

        /**
         * @private
         */
        private ZOrderTimelineState _zOrderTimeline;

        /**
         * @private
         */
        public AnimationState()
        {
        }

        /**
         * @private
         */
        override protected void _onClear()
        {
            foreach (var boneTimelineState in _boneTimelines)
            {
                boneTimelineState.ReturnToPool();
            }

            foreach (var slotTimelineState in _slotTimelines)
            {
                slotTimelineState.ReturnToPool();
            }

            foreach (var ffdTimelineState in _ffdTimelines)
            {
                ffdTimelineState.ReturnToPool();
            }

            if (_timeline != null)
            {
                _timeline.ReturnToPool();
            }

            if (_zOrderTimeline != null)
            {
                _zOrderTimeline.ReturnToPool();
            }

            displayControl = true;
            additiveBlending = false;
            actionEnabled = false;
            playTimes = 1;
            timeScale = 1.0f;
            weight = 1.0f;
            autoFadeOutTime = -1.0f;
            fadeTotalTime = 0.0f;

            _playheadState = 0;
            _fadeState = -1;
            _subFadeState = -1;
            _layer = 0;
            _position = 0.0f;
            _duration = 0.0f;
            _fadeTime = 0.0f;
            _time = 0.0f;
            _fadeProgress = 0.0f;
            _weightResult = 0.0f;
            _name = null;
            _group = null;
            _boneMask.Clear();
            _boneTimelines.Clear();
            _slotTimelines.Clear();
            _ffdTimelines.Clear();
            _animationData = null;
            _armature = null;
            _timeline = null;
            _zOrderTimeline = null;
        }

        /**
         * @private
         */
        private void _advanceFadeTime(float passedTime)
        {
            var isFadeOut = _fadeState > 0;

            if (_subFadeState < 0) // Fade start event.
            {
                _subFadeState = 0;

                var eventType = isFadeOut? EventObject.FADE_OUT: EventObject.FADE_IN;
                if (_armature.eventDispatcher.HasEventListener(eventType))
                {
                    var eventObject = BaseObject.BorrowObject<EventObject>();
                    eventObject.animationState = this;
                    _armature._bufferEvent(eventObject, eventType);
                }
            }

            if (passedTime < 0.0f)
            {
                passedTime = -passedTime;
            }

            _fadeTime += passedTime;
            
            if (_fadeTime >= fadeTotalTime) // Fade complete.
            {
                _subFadeState = 1;
                _fadeProgress = isFadeOut ? 0.0f : 1.0f;
            }
            else if (_fadeTime > 0.0f) // Fading.
            {
                _fadeProgress = isFadeOut ? (1.0f - _fadeTime / fadeTotalTime) : (_fadeTime / fadeTotalTime);
            }
            else // Before fade.
            {
                _fadeProgress = isFadeOut ? 1.0f : 0.0f;
            }

            if (_subFadeState > 0) // Fade complete event.
            {
                if (!isFadeOut)
                {
                    _playheadState |= 1; // x1
                    _fadeState = 0;
                }

                var eventType = isFadeOut ? EventObject.FADE_OUT_COMPLETE : EventObject.FADE_IN_COMPLETE;
                if (_armature.eventDispatcher.HasEventListener(eventType))
                {
                    var eventObject = BaseObject.BorrowObject<EventObject>();
                    eventObject.animationState = this;
                    _armature._bufferEvent(eventObject, eventType);
                }
            }
        }

        /**
         * @private
         */
        internal void _init(Armature armature, AnimationData animationData, AnimationConfig animationConfig)
        {
            _armature = armature;
            _animationData = animationData;
            _name = !string.IsNullOrEmpty(animationConfig.name) ? animationConfig.name : animationConfig.animationName;

            actionEnabled = animationConfig.actionEnabled;
            additiveBlending = animationConfig.additiveBlending;
            displayControl = animationConfig.displayControl;
            playTimes = (uint)animationConfig.playTimes;
            timeScale = animationConfig.timeScale;
            fadeTotalTime = animationConfig.fadeInTime;
            autoFadeOutTime = animationConfig.autoFadeOutTime;
            weight = animationConfig.weight;
            
            if (animationConfig.pauseFadeIn)
            {
                _playheadState = 2; // 10
            }
            else
            {
                _playheadState = 3; // 11
            }

            _fadeState = -1;
            _subFadeState = -1;
            _layer = animationConfig.layer;
            _time = animationConfig.position;
            _group = animationConfig.group;

            if (animationConfig.duration < 0.0f)
            {
                _position = 0.0f;
                _duration = _animationData.duration;
            }
            else
            {
                _position = animationConfig.position;
                _duration = animationConfig.duration;
            }

            if (fadeTotalTime <= 0.0f)
            {
                _fadeProgress = 0.999999f;
            }

            if (animationConfig.boneMask.Count > 0)
            {
                DragonBones.ResizeList(_boneMask, animationConfig.boneMask.Count, null);
                for (int i = 0, l = _boneMask.Count; i < l; ++i)
                {
                    _boneMask[i] = animationConfig.boneMask[i];
                }
            }

            _timeline = BaseObject.BorrowObject<AnimationTimelineState>();
            _timeline._init(_armature, this, _animationData);

            if (_animationData.zOrderTimeline != null)
            {
                _zOrderTimeline = BaseObject.BorrowObject<ZOrderTimelineState>();
                _zOrderTimeline._init(_armature, this, _animationData.zOrderTimeline);
            }

            _updateTimelineStates();
        }

        /**
         * @private
         */
        internal void _updateTimelineStates()
        {
            var boneTimelineStates = new Dictionary<string, BoneTimelineState>();
            var slotTimelineStates = new Dictionary<string, SlotTimelineState>();
            var ffdTimelineStates = new Dictionary<string, FFDTimelineState>();

            foreach (var boneTimelineState in _boneTimelines) // Creat bone timelines map.
            {
                boneTimelineStates[boneTimelineState.bone.name] = boneTimelineState;
            }

            foreach (var bone in _armature.GetBones())
            {
                var boneTimelineName = bone.name;
                if (ContainsBoneMask(boneTimelineName))
                {
                    var boneTimelineData = _animationData.GetBoneTimeline(boneTimelineName);
                    if (boneTimelineData != null)
                    {
                        if (boneTimelineStates.ContainsKey(boneTimelineName)) // Remove bone timeline from map.
                        {
                            boneTimelineStates.Remove(boneTimelineName);
                        }
                        else // Create new bone timeline.
                        {
                            var boneTimelineState = BaseObject.BorrowObject<BoneTimelineState>();
                            boneTimelineState.bone = bone;
                            boneTimelineState._init(_armature, this, boneTimelineData);
                            _boneTimelines.Add(boneTimelineState);
                        }
                    }
                }
            }

            foreach (var boneTimelineState in boneTimelineStates.Values) // Remove bone timelines.
            {
                boneTimelineState.bone.InvalidUpdate(); //
                _boneTimelines.Remove(boneTimelineState);
                boneTimelineState.ReturnToPool();
            }

            foreach (var slotTimelineState in _slotTimelines) // Create slot timelines map.
            {
                slotTimelineStates[slotTimelineState.slot.name] = slotTimelineState;
            }

            foreach (var ffdTimelineState in _ffdTimelines) // Create ffd timelines map.
            {
                var display = ffdTimelineState._timelineData.display;
                var meshName = display.inheritAnimation ? display.mesh.name : display.name;
                ffdTimelineStates[meshName] = ffdTimelineState;
            }

            foreach (var slot in _armature.GetSlots())
            {
                var slotTimelineName = slot.name;
                var parentTimelineName = slot.parent.name;
                var resetFFDVertices = false;

                if (ContainsBoneMask(parentTimelineName))
                {
                    var slotTimelineData = _animationData.GetSlotTimeline(slotTimelineName);
                    if (slotTimelineData != null)
                    {
                        if (slotTimelineStates.ContainsKey(slotTimelineName)) // Remove slot timeline from map.
                        {
                            slotTimelineStates.Remove(slotTimelineName);
                        }
                        else // Create new slot timeline.
                        {
                            var slotTimelineState = BaseObject.BorrowObject<SlotTimelineState>();
                            slotTimelineState.slot = slot;
                            slotTimelineState._init(_armature, this, slotTimelineData);
                            _slotTimelines.Add(slotTimelineState);
                        }
                    }
                    
                    var ffdTimelineDatas = this._animationData.GetFFDTimeline(_armature._skinData.name, slotTimelineName);
                    if (ffdTimelineDatas != null)
                    {
                        foreach (var pair in ffdTimelineDatas)
                        {
                            if (ffdTimelineStates.ContainsKey(pair.Key)) // Remove ffd timeline from map.
                            {
                                ffdTimelineStates.Remove(pair.Key);
                            }
                            else // Create new ffd timeline.
                            {
                                var ffdTimelineState = BaseObject.BorrowObject<FFDTimelineState>();
                                ffdTimelineState.slot = slot;
                                ffdTimelineState._init(_armature, this, pair.Value);
                                _ffdTimelines.Add(ffdTimelineState);
                            }
                        }
                    }
                    else
                    {
                        resetFFDVertices = true;
                    }
                }
                else
                {
                    resetFFDVertices = true;
                }

                if (resetFFDVertices)
                {
                    for (int i = 0, l = slot._ffdVertices.Count; i < l; ++i)
                    {
                        slot._ffdVertices[i] = 0.0f;
                    }

                    slot._meshDirty = true;
                }
            }

            foreach (var slotTimelineState in slotTimelineStates.Values) // Remove slot timelines.
            {
                _slotTimelines.Remove(slotTimelineState);
                slotTimelineState.ReturnToPool();
            }

            foreach (var ffdTimelineState in ffdTimelineStates.Values)// Remove ffd timelines.
            {
                _ffdTimelines.Remove(ffdTimelineState);
                ffdTimelineState.ReturnToPool();
            }
        }

        /**
         * @private
         */
        internal void _advanceTime(float passedTime, float cacheFrameRate)
        {
            // Update fade time.
            if (_fadeState != 0 || _subFadeState != 0)
            {
                _advanceFadeTime(passedTime);
            }

            // Update time.
            passedTime *= timeScale;
            if (passedTime != 0.0f && _playheadState == 3) // 11
            {
                _time += passedTime;
            }

            // weight.
            _weightResult = weight * _fadeProgress;

            if (_weightResult != 0.0f)
            {
                var isUpdatesTimeline = true;
                var isUpdatesBoneTimeline = true;
                var time = _time;

                // Cache time internval.
                var isCacheEnabled = this._fadeState == 0 && cacheFrameRate > 0.0f;
                if (isCacheEnabled)
                {
                    time = cacheFrameRate * 2.0f;
                    time = (float)Math.Floor(_time * time) / time;
                }

                // Update main timeline.                
                _timeline.Update(time, -1.0f);

                var normalizedTime = _timeline._currentTime;

                // Update zOrder timeline.
                if (_zOrderTimeline != null)
                {
                    _zOrderTimeline.Update(time, normalizedTime);
                }

                // Update cache.
                if (isCacheEnabled)
                {
                    var cacheFrameIndex = (int)Math.Floor(_timeline._currentTime * cacheFrameRate); // uint
                    if (_armature._cacheFrameIndex == cacheFrameIndex) // Same cache.
                    {
                        isUpdatesTimeline = false;
                        isUpdatesBoneTimeline = false;
                    }
                    else
                    {
                        _armature._cacheFrameIndex = cacheFrameIndex;

                        if (_animationData.cachedFrames[cacheFrameIndex]) // Cached.
                        {
                            isUpdatesBoneTimeline = false;
                        }
                        else // Cache.
                        {
                            _animationData.cachedFrames[cacheFrameIndex] = true;
                        }
                    }
                }

                // Update timelines.
                if (isUpdatesTimeline)
                {
                    if (isUpdatesBoneTimeline)
                    {
                        for (int i = 0, l = _boneTimelines.Count; i < l; ++i)
                        {
                            _boneTimelines[i].Update(time, normalizedTime);
                        }
                    }

                    for (int i = 0, l = _slotTimelines.Count; i < l; ++i)
                    {
                        _slotTimelines[i].Update(time, normalizedTime);
                    }

                    for (int i = 0, l = _ffdTimelines.Count; i < l; ++i)
                    {
                        _ffdTimelines[i].Update(time, normalizedTime);
                    }
                }
            }

            if (_fadeState == 0)
            {
                if (_subFadeState > 0)
                {
                    _subFadeState = 0;
                }

                // Auto fade out.
                if (autoFadeOutTime >= 0.0f)
                {
                    if (_timeline._playState > 0)
                    {
                        FadeOut(autoFadeOutTime);
                    }
                }
            }
        }

        /**
         * @private
         */
        internal bool _isDisabled(Slot slot)
        {
            if (
                displayControl &&
                (
                    string.IsNullOrEmpty(slot.displayController) ||
                    slot.displayController == _name ||
                    slot.displayController == _group
                )
            )
            {
                return false;
            }

            return true;
        }

        /**
         * @language zh_CN
         * 继续播放。
         * @version DragonBones 3.0
         */
        public void Play()
        {
            _playheadState = 3; // 11
        }

        /**
         * @language zh_CN
         * 暂停播放。
         * @version DragonBones 3.0
         */
        public void Stop()
        {
            _playheadState &= 1; // 0x
        }

        /**
         * @language zh_CN
         * 淡出动画。
         * @param fadeOutTime 淡出时间。 (以秒为单位)
         * @param pausePlayhead 淡出时是否暂停动画。 [true: 暂停, false: 不暂停]
         * @version DragonBones 3.0
         */
        public void FadeOut(float fadeOutTime, bool pausePlayhead = true)
        {
            if (fadeOutTime < 0.0f || float.IsNaN(fadeOutTime))
            {
                fadeOutTime = 0.0f;
            }

            if (pausePlayhead)
            {
                _playheadState &= 2; // x0
            }

            if (_fadeState > 0)
            {
                if (fadeOutTime > fadeOutTime - _fadeTime)
                {
                    // If the animation is already in fade out, the new fade out will be ignored.
                    return;
                }
            }
            else
            {
                _fadeState = 1;
                _subFadeState = -1;

                if (fadeOutTime <= 0.0f || _fadeProgress <= 0.0f)
                {
                    _fadeProgress = 0.000001f; // Modify _fadeProgress to different value.
                }

                foreach (var boneTimelineState in _boneTimelines)
                {
                    boneTimelineState.FadeOut();
                }

                foreach (var slotTimelineState in _slotTimelines)
                {
                    slotTimelineState.FadeOut();
                }
            }

            displayControl = false; //
            fadeTotalTime = _fadeProgress > 0.000001f ? fadeOutTime / _fadeProgress : 0.0f;
            _fadeTime = fadeTotalTime * (1.0f - _fadeProgress);
        }

        /**
         * @language zh_CN
         * 是否包含指定的骨骼遮罩。
         * @param name 指定的骨骼名称。
         * @version DragonBones 3.0
         */
        public bool ContainsBoneMask(string name)
        {
            return _boneMask.Count == 0 || _boneMask.Contains(name);
        }

        /**
         * @language zh_CN
         * 添加指定的骨骼遮罩。
         * @param boneName 指定的骨骼名称。
         * @param recursive 是否为该骨骼的子骨骼添加遮罩。
         * @version DragonBones 3.0
         */
        public void AddBoneMask(string name, bool recursive = true)
        {
            var currentBone = _armature.GetBone(name);
            if (currentBone == null)
            {
                return;
            }

            if (!_boneMask.Contains(name)) // Add mixing
            {
                _boneMask.Add(name);
            }

            if (recursive)
            {
                foreach (var bone in _armature.GetBones())
                {
                    if (!_boneMask.Contains(bone.name) && currentBone.Contains(bone)) // Add recursive mixing.
                    {
                        _boneMask.Add(bone.name);
                    }
                }
            }

            _updateTimelineStates();
        }

        /**
         * @language zh_CN
         * 删除指定的骨骼遮罩。
         * @param boneName 指定的骨骼名称。
         * @param recursive 是否删除该骨骼的子骨骼遮罩。
         * @version DragonBones 3.0
         */
        public void RemoveBoneMask(string name, bool recursive = true)
        {
            if (_boneMask.Contains(name)) // Remove mixing.
            {
                _boneMask.Remove(name);
            }

            if (recursive)
            {
                var currentBone = _armature.GetBone(name);
                if (currentBone != null)
                {
                    var bones = _armature.GetBones();
                    if (_boneMask.Count > 0)
                    {
                        foreach (var bone in bones)
                        {
                            if (_boneMask.Contains(bone.name) && currentBone.Contains(bone)) // Remove recursive mixing.
                            {
                                _boneMask.Remove(bone.name);
                            }
                        }
                    }
                    else
                    {
                        foreach (var bone in bones)
                        {
                            if (!currentBone.Contains(bone)) // Add unrecursive mixing.
                            {
                                _boneMask.Add(bone.name);
                            }
                        }
                    }
                }
            }

            _updateTimelineStates();
        }

        /**
         * @language zh_CN
         * 删除所有骨骼遮罩。
         * @version DragonBones 3.0
         */
        public void RemoveAllBoneMask()
        {
            _boneMask.Clear();
            _updateTimelineStates();
        }

        /**
         * @language zh_CN
         * 动画图层。
         * @see dragonBones.Animation#fadeIn()
         * @version DragonBones 3.0
         */
        public int layer
        {
            get { return _layer; }
        }

        /**
         * @language zh_CN
         * 动画组。
         * @see dragonBones.Animation#fadeIn()
         * @version DragonBones 3.0
         */
        public string group
        {
            get { return _group; }
        }

        /**
         * @language zh_CN
         * 动画名称。
         * @see dragonBones.AnimationData#name
         * @version DragonBones 3.0
         */
        public string name
        {
            get { return _name; }
        }

        /**
         * @language zh_CN
         * 动画数据。
         * @see dragonBones.AnimationData
         * @version DragonBones 3.0
         */
        public AnimationData animationData
        {
            get { return _animationData; }
        }

        /**
         * @language zh_CN
         * 是否播放完毕。
         * @version DragonBones 3.0
         */
        public bool isCompleted
        {
            get { return _timeline._playState > 0; }
        }

        /**
         * @language zh_CN
         * 是否正在播放。
         * @version DragonBones 3.0
         */
        public bool isPlaying
        {
            get { return (_playheadState & 2) != 0 && _timeline._playState <= 0; } // 1x
        }

        /**
         * @language zh_CN
         * 当前动画的播放次数。
         * @version DragonBones 3.0
         */
        public uint currentPlayTimes
        {
            get { return _timeline._currentPlayTimes; }
        }

        /**
         * @language zh_CN
         * 当前动画的总时间。 (以秒为单位)
         * @version DragonBones 3.0
         */
        public float totalTime
        {
            get { return _duration; }
        }

        /**
         * @language zh_CN
         * 当前动画的播放时间。 (以秒为单位)
         * @version DragonBones 3.0
         */
        public float currentTime
        {
            get { return _timeline._currentTime; }
            set
            {
                if (value < 0.0f || float.IsNaN(value))
                {
                    value = 0.0f;
                }

                var currentPlayTimes = _timeline._currentPlayTimes - (_timeline._playState > 0 ? 1 : 0);
                value = (value % _duration) + currentPlayTimes * _duration;
                if (_time == value)
                {
                    return;
                }

                _time = value;
                _timeline.setCurrentTime(_time);

                if (_zOrderTimeline != null)
                {
                    _zOrderTimeline._playState = -1;
                }

                foreach (var boneTimelineState in _boneTimelines)
                {
                    boneTimelineState._playState = -1;
                }

                foreach (var slotTimelineState in _slotTimelines)
                {
                    slotTimelineState._playState = -1;
                }

                foreach (var ffdTimelineState in _ffdTimelines)
                {
                    ffdTimelineState._playState = -1;
                }
            }
        }
    }
}