using System;
using System.Collections.Generic;

namespace DragonBones
{
    public class AnimationState : BaseObject
    {
        /**
         * @private
         */
        public static bool stateActionEnabled = true;

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
        internal int _fadeState;

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
        internal float _weightResult;

        /**
         * @private
         */
        internal float _fadeProgress;

        /**
         * @private
         */
        internal string _group;

        /**
         * @private
         */
        internal AnimationTimelineState _timeline;

        /**
         * @private
         */
        private bool _isPlaying;

        /**
         * @private
         */
        private bool _isPausePlayhead;

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
        private string _name;

        /**
         * @private
         */
        private Armature _armature;

        /**
         * @private
         */
        private AnimationData _animationData;

        /**
         * @private
         */
        private ZOrderTimelineState _zOrderTimeline;

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
        public AnimationState()
        {
        }

        /**
         * @inheritDoc
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

            _fadeState = 0;
            _layer = 0;
            _position = 0.0f;
            _duration = 0.0f;
            _weightResult = 0.0f;
            _fadeProgress = 0.0f;
            _group = null;
            _timeline = null;

            _isPlaying = true;
            _isPausePlayhead = false;
            _fadeTime = 0.0f;
            _time = 0.0f;
            _name = null;
            _armature = null;
            _animationData = null;
            _zOrderTimeline = null;
            _boneMask.Clear();
            _boneTimelines.Clear();
            _slotTimelines.Clear();
            _ffdTimelines.Clear();
        }

        /**
         * @private
         */
        private void _advanceFadeTime(float passedTime)
        {
            if (passedTime < 0.0f)
            {
                passedTime = -passedTime;
            }

            _fadeTime += passedTime;

            var fadeProgress = 0.0f;
            if (_fadeTime >= fadeTotalTime) // Fade complete.
            {
                fadeProgress = _fadeState > 0 ? 0.0f : 1.0f;
            }
            else if (_fadeTime > 0.0f) // Fading.
            {
                fadeProgress = _fadeState > 0 ? (1 - _fadeTime / fadeTotalTime) : (_fadeTime / fadeTotalTime);
            }
            else // Before fade.
            {
                fadeProgress = _fadeState > 0 ? 1.0f : 0.0f;
            }

            if (_fadeProgress != fadeProgress)
            {
                _fadeProgress = fadeProgress;

                var eventDispatcher = _armature._eventDispatcher;

                if (_fadeTime <= passedTime)
                {
                    if (_fadeState > 0)
                    {
                        if (eventDispatcher.HasEventListener(EventObject.FADE_OUT))
                        {
                            var evt = BaseObject.BorrowObject<EventObject>();
                            evt.animationState = this;
                            _armature._bufferEvent(evt, EventObject.FADE_OUT);
                        }
                    }
                    else
                    {
                        if (eventDispatcher.HasEventListener(EventObject.FADE_IN))
                        {
                            var evt = BaseObject.BorrowObject<EventObject>();
                            evt.animationState = this;
                            _armature._bufferEvent(evt, EventObject.FADE_IN);
                        }
                    }
                }

                if (_fadeTime >= fadeTotalTime)
                {
                    if (_fadeState > 0)
                    {
                        if (eventDispatcher.HasEventListener(EventObject.FADE_OUT_COMPLETE))
                        {
                            var evt = BaseObject.BorrowObject<EventObject>();
                            evt.animationState = this;
                            _armature._bufferEvent(evt, EventObject.FADE_OUT_COMPLETE);
                        }
                    }
                    else
                    {
                        _isPausePlayhead = false;
                        _fadeState = 0;

                        if (eventDispatcher.HasEventListener(EventObject.FADE_IN_COMPLETE))
                        {
                            var evt = BaseObject.BorrowObject<EventObject>();
                            evt.animationState = this;
                            _armature._bufferEvent(evt, EventObject.FADE_IN_COMPLETE);
                        }
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
                    !DragonBones.IsAvailableString(slot.displayController) ||
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
         * @private
         */
        internal void _fadeIn(
            Armature armature, AnimationData clip, string animationName,
            uint playTimes, float position, float duration, float time, float timeScale, float fadeInTime,
            bool pausePlayhead
        )
        {
            _armature = armature;
            _animationData = clip;
            _name = animationName;

            actionEnabled = stateActionEnabled;
            this.playTimes = playTimes;
            this.timeScale = timeScale;
            fadeTotalTime = fadeInTime;

            _fadeState = -1;
            _position = position;
            _duration = duration;
            _time = time;
            _isPausePlayhead = pausePlayhead;
            if (fadeTotalTime <= 0.0f)
            {
                _fadeProgress = 0.999999f;
            }

            _timeline = BaseObject.BorrowObject<AnimationTimelineState>();
            _timeline.FadeIn(_armature, this, _animationData, _time);

            if (_animationData.zOrderTimeline != null)
            {
                _zOrderTimeline = BaseObject.BorrowObject<ZOrderTimelineState>();
                _zOrderTimeline.FadeIn(_armature, this, _animationData.zOrderTimeline, _time);
            }

            _updateTimelineStates();
        }

        /**
         * @private
         */
        internal void _updateTimelineStates()
        {
            var time = _time;
            if (!_animationData.hasAsynchronyTimeline)
            {
                time = _timeline._currentTime;
            }

            var boneTimelineStates = new Dictionary<string, BoneTimelineState>();
            var slotTimelineStates = new Dictionary<string, SlotTimelineState>();

            foreach (var boneTimelineState in _boneTimelines) // Creat bone timelines map.
            {
                boneTimelineStates.Add(boneTimelineState.bone.name, boneTimelineState);
            }

            foreach (var bone in _armature.GetBones())
            {
                var boneTimelineName = bone.name;
                var boneTimelineData = _animationData.GetBoneTimeline(boneTimelineName);

                if (boneTimelineData != null && ContainsBoneMask(boneTimelineName))
                {
                    var boneTimelineState = boneTimelineStates.ContainsKey(boneTimelineName) ? boneTimelineStates[boneTimelineName] : null;
                    if (boneTimelineState != null) // Remove bone timeline from map.
                    {
                        boneTimelineStates.Remove(boneTimelineName);
                    }
                    else // Create new bone timeline.
                    {
                        boneTimelineState = BaseObject.BorrowObject<BoneTimelineState>();
                        boneTimelineState.bone = bone;
                        boneTimelineState.FadeIn(_armature, this, boneTimelineData, time);
                        _boneTimelines.Add(boneTimelineState);
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

            foreach (var slot in _armature.GetSlots())
            {
                var slotTimelineName = slot.name;
                var parentTimelineName = slot.parent.name;
                var slotTimelineData = _animationData.GetSlotTimeline(slotTimelineName);

                if (slotTimelineData != null && ContainsBoneMask(parentTimelineName) && _fadeState <= 0)
                {
                    var slotTimelineState = slotTimelineStates.ContainsKey(slotTimelineName) ? slotTimelineStates[slotTimelineName] : null;
                    if (slotTimelineState != null) // Remove slot timeline from map.
                    {
                        slotTimelineStates.Remove(slotTimelineName);
                    }
                    else // Create new slot timeline.
                    {
                        slotTimelineState = BaseObject.BorrowObject<SlotTimelineState>();
                        slotTimelineState.slot = slot;
                        slotTimelineState.FadeIn(_armature, this, slotTimelineData, time);
                        _slotTimelines.Add(slotTimelineState);
                    }
                }
            }

            foreach (var slotTimelineState in slotTimelineStates.Values) // Remove slot timelines.
            {
                _slotTimelines.Remove(slotTimelineState);
                slotTimelineState.ReturnToPool();
            }

            _updateFFDTimelineStates();
        }

        /**
         * @private
         */
        internal void _updateFFDTimelineStates()
        {
            var time = _time;
            if (!_animationData.hasAsynchronyTimeline)
            {
                time = _timeline._currentTime;
            }

            var ffdTimelineStates = new Dictionary<string, FFDTimelineState>();

            foreach (var ffdTimelineState in _ffdTimelines) // Create ffd timelines map.
            {
                ffdTimelineStates[ffdTimelineState.slot.name] = ffdTimelineState;
            }

            foreach (var slot in _armature.GetSlots())
            {
                var slotTimelineName = slot.name;
                var parentTimelineName = slot.parent.name;

                if (slot._meshData != null)
                {
                    var displayIndex = slot.displayIndex;
                    var rawMeshData = displayIndex < slot._displayDataSet.displays.Count ? slot._displayDataSet.displays[displayIndex].mesh : null;

                    if (slot._meshData == rawMeshData)
                    {
                        var ffdTimelineData = _animationData.GetFFDTimeline(_armature._skinData.name, slotTimelineName, displayIndex);
                        if (ffdTimelineData != null && ContainsBoneMask(parentTimelineName)) // && !_isFadeOut
                        {
                            var ffdTimelineState = ffdTimelineStates.ContainsKey(slotTimelineName) ? ffdTimelineStates[slotTimelineName] : null;
                            if (ffdTimelineState != null && ffdTimelineState._timeline == ffdTimelineData) // Remove ffd timeline from map.
                            {
                                ffdTimelineStates.Remove(slotTimelineName);
                            }
                            else // Create new ffd timeline.
                            {
                                ffdTimelineState = BaseObject.BorrowObject<FFDTimelineState>();
                                ffdTimelineState.slot = slot;
                                ffdTimelineState.FadeIn(_armature, this, ffdTimelineData, time);
                                _ffdTimelines.Add(ffdTimelineState);
                            }
                        }
                        else
                        {
                            for (int i = 0, l = slot._ffdVertices.Count; i < l; ++i) // Clear slot ffd.
                            {
                                slot._ffdVertices[i] = 0.0f;
                            }

                            slot._ffdDirty = true;
                        }
                    }
                }
            }

            foreach (var ffdTimelineState in ffdTimelineStates.Values)// Remove ffd timelines.
            {
                //ffdTimelineState.slot._ffdDirty = true;
                _ffdTimelines.Remove(ffdTimelineState);
                ffdTimelineState.ReturnToPool();
            }
        }

        /**
         * @private
         */
        internal void _advanceTime(float passedTime, float weightLeft, int index)
        {
            // Update fade time. (Still need to be update even if the passedTime is zero)
            if (_fadeState != 0)
            {
                _advanceFadeTime(passedTime);
            }

            // Update time.
            passedTime *= timeScale;
            if (passedTime != 0.0f && _isPlaying && !_isPausePlayhead)
            {
                _time += passedTime;
            }

            // Blend weight.
            _weightResult = weight * _fadeProgress * weightLeft;

            if (_weightResult > 0.0f)
            {
                var isCacheEnabled = _fadeProgress >= 1.0f && index == 0 && _armature.cacheFrameRate > 0;
                var cacheTimeToFrameScale = _animationData.cacheTimeToFrameScale;
                var isUpdatesTimeline = true;
                var isUpdatesBoneTimeline = true;
                var time = cacheTimeToFrameScale * 2.0f;
                time = isCacheEnabled ? ((float)Math.Floor(_time * time) / time) : _time; // Cache time internval.

                // Update main timeline.                
                _timeline.Update(time);
                if (!_animationData.hasAsynchronyTimeline)
                {
                    time = _timeline._currentTime;
                }

                if (_zOrderTimeline != null)
                {
                    _zOrderTimeline.Update(time);
                }

                if (isCacheEnabled)
                {
                    var cacheFrameIndex = (int)Math.Floor(_timeline._currentTime * cacheTimeToFrameScale); // int
                    if (_armature._cacheFrameIndex == cacheFrameIndex) // Same cache.
                    {
                        isUpdatesTimeline = false;
                        isUpdatesBoneTimeline = false;
                    }
                    else
                    {
                        _armature._cacheFrameIndex = cacheFrameIndex;

                        if (_armature._animation._animationStateDirty) // Update _cacheFrames.
                        {
                            _armature._animation._animationStateDirty = false;

                            for (int i = 0, l = _boneTimelines.Count; i < l; ++i)
                            {
                                var boneTimelineState = _boneTimelines[i];
                                boneTimelineState.bone._cacheFrames = (boneTimelineState._timeline).cachedFrames;
                            }

                            for (int i = 0, l = _slotTimelines.Count; i < l; ++i)
                            {
                                var slotTimelineState = _slotTimelines[i];
                                slotTimelineState.slot._cacheFrames = (slotTimelineState._timeline).cachedFrames;
                            }
                        }

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
                else
                {
                    _armature._cacheFrameIndex = -1;
                }

                if (isUpdatesTimeline)
                {
                    if (isUpdatesBoneTimeline)
                    {
                        for (int i = 0, l = _boneTimelines.Count; i < l; ++i)
                        {
                            var boneTimelineState = _boneTimelines[i];
                            boneTimelineState.Update(time);
                        }
                    }

                    for (int i = 0, l = _slotTimelines.Count; i < l; ++i)
                    {
                        var slotTimelineState = _slotTimelines[i];
                        slotTimelineState.Update(time);
                    }

                    for (int i = 0, l = _ffdTimelines.Count; i < l; ++i)
                    {
                        var ffdTimelineState = _ffdTimelines[i];
                        ffdTimelineState.Update(time);
                    }
                }
            }

            if (autoFadeOutTime >= 0.0f && _fadeProgress >= 1.0f && _timeline._isCompleted)
            {
                FadeOut(autoFadeOutTime);
            }
        }

        /**
         * @language zh_CN
         * 继续播放。
         * @version DragonBones 3.0
         */
        public void Play()
        {
            _isPlaying = true;
        }

        /**
         * @language zh_CN
         * 暂停播放。
         * @version DragonBones 3.0
         */
        public void Stop()
        {
            _isPlaying = false;
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

            _isPausePlayhead = pausePlayhead;

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

            displayControl = false;
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
            return _boneMask.Count < 1 || _boneMask.Contains(name);
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

            if (
                !_boneMask.Contains(name) &&
                _animationData.GetBoneTimeline(name) != null
                ) // Add mixing
            {
                _boneMask.Add(name);
            }

            if (recursive)
            {
                foreach (var bone in _armature.GetBones())
                {
                    var boneName = bone.name;
                    if (
                        !_boneMask.Contains(boneName) &&
                        _animationData.GetBoneTimeline(boneName) != null &&
                        currentBone.Contains(bone)
                        ) // Add recursive mixing.
                    {
                        _boneMask.Add(boneName);
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
                    foreach (var bone in _armature.GetBones())
                    {
                        var boneName = bone.name;
                        if (
                            _boneMask.Contains(boneName) &&
                            currentBone.Contains(bone)
                            ) // Remove recursive mixing.
                        {
                            _boneMask.Remove(boneName);
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
            get { return _timeline._isCompleted; }
        }

        /**
         * @language zh_CN
         * 是否正在播放。
         * @version DragonBones 3.0
         */
        public bool isPlaying
        {
            get { return _isPlaying && !_timeline._isCompleted; }
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

                var currentPlayTimes = _timeline._currentPlayTimes - (_timeline._isCompleted ? 1 : 0);
                value = (value % _duration) + currentPlayTimes * _duration;
                if (_time == value)
                {
                    return;
                }

                _time = value;
                _timeline.setCurrentTime(_time);

                if (_zOrderTimeline != null)
                {
                    _zOrderTimeline._isCompleted = false;
                }

                foreach (var boneTimelineState in _boneTimelines)
                {
                    boneTimelineState._isCompleted = false;
                }

                foreach (var slotTimelineState in _slotTimelines)
                {
                    slotTimelineState._isCompleted = false;
                }

                foreach (var ffdTimelineState in _ffdTimelines)
                {
                    ffdTimelineState._isCompleted = false;
                }
            }
        }
    }
}