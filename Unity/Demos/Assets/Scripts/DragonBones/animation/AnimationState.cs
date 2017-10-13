using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DragonBones
{
    /**
     * @internal
     * @private
     */
    internal class BonePose : BaseObject
    {

        public readonly Transform current = new Transform();
        public readonly Transform delta = new Transform();
        public readonly Transform result = new Transform();

        protected override void _OnClear()
        {
            this.current.Identity();
            this.delta.Identity();
            this.result.Identity();
        }
    }
    /**
     * 动画状态，播放动画时产生，可以对每个播放的动画进行更细致的控制和调节。
     * @see dragonBones.Animation
     * @see dragonBones.AnimationData
     * @version DragonBones 3.0
     * @language zh_CN
     */
    public class AnimationState : BaseObject
    {
        /**
         * 是否将骨架的骨骼和插槽重置为绑定姿势（如果骨骼和插槽在这个动画状态中没有动画）。
         * @version DragonBones 5.1
         * @language zh_CN
         */
        public bool resetToPose;
        /**
         * 是否以增加的方式混合。
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public bool additiveBlending;
        /**
         * 是否对插槽的显示对象有控制权。
         * @see dragonBones.Slot#displayController
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public bool displayControl;
        /**
         * 是否能触发行为。
         * @version DragonBones 5.0
         * @language zh_CN
         */
        public bool actionEnabled;
        /**
         * 混合图层。
         * @version DragonBones 3.0
         * @readonly
         * @language zh_CN
         */
        public int layer;
        /**
         * 播放次数。 [0: 无限循环播放, [1~N]: 循环播放 N 次]
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public int playTimes;
        /**
         * 播放速度。 [(-N~0): 倒转播放, 0: 停止播放, (0~1): 慢速播放, 1: 正常播放, (1~N): 快速播放]
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public float timeScale;
        /**
         * 混合权重。
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public float weight;
        /**
         * 自动淡出时间。 [-1: 不自动淡出, [0~N]: 淡出时间] (以秒为单位)
         * 当设置一个大于等于 0 的值，动画状态将会在播放完成后自动淡出。
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public float autoFadeOutTime;
        /**
         * @private
         */
        internal float fadeTotalTime;
        /**
         * 动画名称。
         * @version DragonBones 3.0
         * @readonly
         * @language zh_CN
         */
        public string name;
        /**
         * 混合组。
         * @version DragonBones 3.0
         * @readonly
         * @language zh_CN
         */
        public string group;
        /**
         * 动画数据。
         * @see dragonBones.AnimationData
         * @version DragonBones 3.0
         * @readonly
         * @language zh_CN
         */
        public AnimationData animationData;

        private bool _timelineDirty;
        /**
         * @internal
         * @private
         * xx: Play Enabled, Fade Play Enabled
         */
        internal int _playheadState;
        /**
         * @internal
         * @private
         * -1: Fade in, 0: Fade complete, 1: Fade out;
         */
        internal int _fadeState;
        /**
         * @internal
         * @private
         * -1: Fade start, 0: Fading, 1: Fade complete;
         */
        internal int _subFadeState;
        /**
         * @internal
         * @private
         */
        internal float _position;
        /**
         * @internal
         * @private
         */
        internal float _duration;
        private float _fadeTime;
        private float _time;
        /**
         * @internal
         * @private
         */
        internal float _fadeProgress;
        private float _weightResult;
        private readonly List<string> _boneMask = new List<string>();
        private readonly List<BoneTimelineState> _boneTimelines = new List<BoneTimelineState>();
        private readonly List<SlotTimelineState> _slotTimelines = new List<SlotTimelineState>();
        private readonly Dictionary<string, BonePose> _bonePoses = new Dictionary<string, BonePose>();
        private Armature _armature;
        /**
         * @internal
         * @private
         */
        internal ActionTimelineState _actionTimeline = null; // Initial value.
        private ZOrderTimelineState _zOrderTimeline = null; // Initial value.
        /**
         * @private
         */
        protected override void _OnClear()
        {
            foreach (var timeline in this._boneTimelines)
            {
                timeline.ReturnToPool();
            }

            foreach (var timeline in this._slotTimelines)
            {
                timeline.ReturnToPool();
            }

            foreach (var bonePose in this._bonePoses.Values)
            {
                bonePose.ReturnToPool();
            }

            if (this._actionTimeline != null)
            {
                this._actionTimeline.ReturnToPool();
            }

            if (this._zOrderTimeline != null)
            {
                this._zOrderTimeline.ReturnToPool();
            }

            this.resetToPose = false;
            this.additiveBlending = false;
            this.displayControl = false;
            this.actionEnabled = false;
            this.layer = 0;
            this.playTimes = 1;
            this.timeScale = 1.0f;
            this.weight = 1.0f;
            this.autoFadeOutTime = 0.0f;
            this.fadeTotalTime = 0.0f;
            this.name = string.Empty;
            this.group = string.Empty;
            this.animationData = null; //

            this._timelineDirty = true;
            this._playheadState = 0;
            this._fadeState = -1;
            this._subFadeState = -1;
            this._position = 0.0f;
            this._duration = 0.0f;
            this._fadeTime = 0.0f;
            this._time = 0.0f;
            this._fadeProgress = 0.0f;
            this._weightResult = 0.0f;
            this._boneMask.Clear();
            this._boneTimelines.Clear();
            this._slotTimelines.Clear();
            this._bonePoses.Clear();
            this._armature = null; //
            this._actionTimeline = null; //
            this._zOrderTimeline = null;
        }

        private bool _IsDisabled(Slot slot)
        {
            if (this.displayControl)
            {
                var displayController = slot.displayController;
                if (displayController == null ||
                    displayController == this.name ||
                    displayController == this.group)
                {
                    return false;
                }
            }

            return true;
        }

        private void _AdvanceFadeTime(float passedTime)
        {
            var isFadeOut = this._fadeState > 0;

            if (this._subFadeState < 0)
            {
                // Fade start event.
                this._subFadeState = 0;

                var eventType = isFadeOut ? EventObject.FADE_OUT : EventObject.FADE_IN;
                if (this._armature.proxy.HasEventListener(eventType))
                {
                    var eventObject = BaseObject.BorrowObject<EventObject>();
                    eventObject.type = eventType;
                    eventObject.armature = this._armature;
                    eventObject.animationState = this;
                    this._armature._dragonBones.BufferEvent(eventObject);
                }
            }

            if (passedTime < 0.0f)
            {
                passedTime = -passedTime;
            }

            this._fadeTime += passedTime;

            if (this._fadeTime >= this.fadeTotalTime)
            { 
                // Fade complete.
                this._subFadeState = 1;
                this._fadeProgress = isFadeOut ? 0.0f : 1.0f;
            }
            else if (this._fadeTime > 0.0f)
            { 
                // Fading.
                this._fadeProgress = isFadeOut ? (1.0f - this._fadeTime / this.fadeTotalTime) : (this._fadeTime / this.fadeTotalTime);
            }
            else
            { 
                // Before fade.
                this._fadeProgress = isFadeOut ? 1.0f : 0.0f;
            }

            if (this._subFadeState > 0)
            { 
                // Fade complete event.
                if (!isFadeOut)
                {
                    this._playheadState |= 1; // x1
                    this._fadeState = 0;
                }

                var eventType = isFadeOut ? EventObject.FADE_OUT_COMPLETE : EventObject.FADE_IN_COMPLETE;
                if (this._armature.proxy.HasEventListener(eventType))
                {
                    var eventObject = BaseObject.BorrowObject<EventObject>();
                    eventObject.type = eventType;
                    eventObject.armature = this._armature;
                    eventObject.animationState = this;
                    this._armature._dragonBones.BufferEvent(eventObject);
                }
            }
        }

        private void _BlendBoneTimline(BoneTimelineState timeline)
        {
            var bone = timeline.bone;
            var bonePose = timeline.bonePose.result;
            var animationPose = bone.animationPose;
            var boneWeight = this._weightResult > 0.0f ? this._weightResult : -this._weightResult;

            if (!bone._blendDirty)
            {
                bone._blendDirty = true;
                bone._blendLayer = this.layer;
                bone._blendLayerWeight = boneWeight;
                bone._blendLeftWeight = 1.0f;

                animationPose.x = bonePose.x * boneWeight;
                animationPose.y = bonePose.y * boneWeight;
                animationPose.rotation = bonePose.rotation * boneWeight;
                animationPose.skew = bonePose.skew * boneWeight;
                animationPose.scaleX = (bonePose.scaleX - 1.0f) * boneWeight + 1.0f;
                animationPose.scaleY = (bonePose.scaleY - 1.0f) * boneWeight + 1.0f;
            }
            else
            {
                boneWeight *= bone._blendLeftWeight;
                bone._blendLayerWeight += boneWeight;

                animationPose.x += bonePose.x * boneWeight;
                animationPose.y += bonePose.y * boneWeight;
                animationPose.rotation += bonePose.rotation * boneWeight;
                animationPose.skew += bonePose.skew * boneWeight;
                animationPose.scaleX += (bonePose.scaleX - 1.0f) * boneWeight;
                animationPose.scaleY += (bonePose.scaleY - 1.0f) * boneWeight;
            }

            if (this._fadeState != 0 || this._subFadeState != 0)
            {
                bone._transformDirty = true;
            }
        }

        /**
         * @private
         * @internal
         */
        internal void Init(Armature armature, AnimationData animationData, AnimationConfig animationConfig)
        {
            if (this._armature != null)
            {
                return;
            }

            this._armature = armature;

            this.animationData = animationData;
            this.resetToPose = animationConfig.resetToPose;
            this.additiveBlending = animationConfig.additiveBlending;
            this.displayControl = animationConfig.displayControl;
            this.actionEnabled = animationConfig.actionEnabled;
            this.layer = animationConfig.layer;
            this.playTimes = animationConfig.playTimes;
            this.timeScale = animationConfig.timeScale;
            this.fadeTotalTime = animationConfig.fadeInTime;
            this.autoFadeOutTime = animationConfig.autoFadeOutTime;
            this.weight = animationConfig.weight;
            this.name = animationConfig.name.Length > 0 ? animationConfig.name : animationConfig.animation;
            this.group = animationConfig.group;

            if (animationConfig.pauseFadeIn)
            {
                this._playheadState = 2; // 10
            }
            else
            {
                this._playheadState = 3; // 11
            }

            if (animationConfig.duration < 0.0f)
            {
                this._position = 0.0f;
                this._duration = this.animationData.duration;
                if (animationConfig.position != 0.0f)
                {
                    if (this.timeScale >= 0.0f)
                    {
                        this._time = animationConfig.position;
                    }
                    else
                    {
                        this._time = animationConfig.position - this._duration;
                    }
                }
                else
                {
                    this._time = 0.0f;
                }
            }
            else
            {
                this._position = animationConfig.position;
                this._duration = animationConfig.duration;
                this._time = 0.0f;
            }

            if (this.timeScale < 0.0f && this._time == 0.0f)
            {
                this._time = -0.000001f; // Turn to end.
            }

            if (this.fadeTotalTime <= 0.0f)
            {
                this._fadeProgress = 0.999999f; // Make different.
            }

            if (animationConfig.boneMask.Count > 0)
            {
                this._boneMask.ResizeList(animationConfig.boneMask.Count);
                for (int i = 0, l = this._boneMask.Count; i < l; ++i)
                {
                    this._boneMask[i] = animationConfig.boneMask[i];
                }
            }

            this._actionTimeline = BaseObject.BorrowObject<ActionTimelineState>();
            this._actionTimeline.Init(this._armature, this, this.animationData.actionTimeline);
            this._actionTimeline.currentTime = this._time;
            if (this._actionTimeline.currentTime < 0.0f)
            {
                this._actionTimeline.currentTime = this._duration - this._actionTimeline.currentTime;
            }

            if (this.animationData.zOrderTimeline != null)
            {
                this._zOrderTimeline = BaseObject.BorrowObject<ZOrderTimelineState>();
                this._zOrderTimeline.Init(this._armature, this, this.animationData.zOrderTimeline);
            }
        }
        /**
         * @private
         * @internal
         */
        internal void UpdateTimelines()
        {
            Dictionary<string, List<BoneTimelineState>> boneTimelines = new Dictionary<string, List<BoneTimelineState>>();
            foreach (var timeline in this._boneTimelines)
            {
                // Create bone timelines map.
                var timelineName = timeline.bone.name;
                if (!(boneTimelines.ContainsKey(timelineName)))
                {
                    boneTimelines[timelineName] = new List<BoneTimelineState>();
                }

                boneTimelines[timelineName].Add(timeline);
            }

            foreach (Bone bone in this._armature.GetBones())
            {
                var timelineName = bone.name;
                if (!this.ContainsBoneMask(timelineName))
                {
                    continue;
                }

                var timelineDatas = this.animationData.GetBoneTimeline(timelineName);
                if (boneTimelines.ContainsKey(timelineName))
                {
                    // Remove bone timeline from map.
                    boneTimelines.Remove(timelineName);
                }
                else
                {
                    // Create new bone timeline.
                    var bonePose = this._bonePoses.ContainsKey(timelineName) ? this._bonePoses[timelineName] : (this._bonePoses[timelineName] = BaseObject.BorrowObject<BonePose>());
                    if (timelineDatas != null)
                    {
                        foreach (var timelineData in timelineDatas)
                        {
                            switch (timelineData.type)
                            {
                                case TimelineType.BoneAll:
                                    {
                                        var timeline = BaseObject.BorrowObject<BoneAllTimelineState>();
                                        timeline.bone = bone;
                                        timeline.bonePose = bonePose;
                                        timeline.Init(this._armature, this, timelineData);
                                        this._boneTimelines.Add(timeline);
                                        break;
                                    }
                                case TimelineType.BoneTranslate:
                                    {
                                        var timeline = BaseObject.BorrowObject<BoneTranslateTimelineState>();
                                        timeline.bone = bone;
                                        timeline.bonePose = bonePose;
                                        timeline.Init(this._armature, this, timelineData);
                                        this._boneTimelines.Add(timeline);
                                        break;
                                    }
                                case TimelineType.BoneRotate:
                                    {
                                        var timeline = BaseObject.BorrowObject<BoneRotateTimelineState>();
                                        timeline.bone = bone;
                                        timeline.bonePose = bonePose;
                                        timeline.Init(this._armature, this, timelineData);
                                        this._boneTimelines.Add(timeline);
                                        break;
                                    }
                                case TimelineType.BoneScale:
                                    {
                                        var timeline = BaseObject.BorrowObject<BoneScaleTimelineState>();
                                        timeline.bone = bone;
                                        timeline.bonePose = bonePose;
                                        timeline.Init(this._armature, this, timelineData);
                                        this._boneTimelines.Add(timeline);
                                        break;
                                    }

                                default:
                                    break;
                            }
                        }
                    }
                    else if (this.resetToPose)
                    {
                        // Pose timeline.
                        var timeline = BaseObject.BorrowObject<BoneAllTimelineState>();
                        timeline.bone = bone;
                        timeline.bonePose = bonePose;
                        timeline.Init(this._armature, this, null);
                        this._boneTimelines.Add(timeline);
                    }
                }
            }

            foreach (var k in boneTimelines.Keys)
            {
                // Remove bone timelines.
                var timeLines = boneTimelines[k];
                foreach (var tiemLine in timeLines)
                {
                    this._boneTimelines.Remove(tiemLine);
                    tiemLine.ReturnToPool();
                }
            }

            Dictionary<string, List<SlotTimelineState>> slotTimelines = new Dictionary<string, List<SlotTimelineState>>();
            List<int> ffdFlags = new List<int>();

            foreach (var timeline in this._slotTimelines)
            {
                // Create slot timelines map.
                var timelineName = timeline.slot.name;
                if (!(slotTimelines.ContainsKey(timelineName)))
                {
                    slotTimelines[timelineName] = new List<SlotTimelineState>();
                }

                slotTimelines[timelineName].Add(timeline);
            }

            foreach (Slot slot in this._armature.GetSlots())
            {
                var boneName = slot.parent.name;
                if (!this.ContainsBoneMask(boneName))
                {
                    continue;
                }

                var timelineName = slot.name;
                var timelineDatas = this.animationData.GetSlotTimeline(timelineName);
                if (slotTimelines.ContainsKey(timelineName))
                {
                    // Remove slot timeline from map.
                    slotTimelines.Remove(timelineName);
                }
                else
                {
                    // Create new slot timeline.
                    var displayIndexFlag = false;
                    var colorFlag = false;
                    ffdFlags.Clear();

                    if (timelineDatas != null)
                    {
                        foreach (var timelineData in timelineDatas)
                        {
                            switch (timelineData.type)
                            {
                                case TimelineType.SlotDisplay:
                                    {
                                        var timeline = BaseObject.BorrowObject<SlotDislayIndexTimelineState>();
                                        timeline.slot = slot;
                                        timeline.Init(this._armature, this, timelineData);
                                        this._slotTimelines.Add(timeline);
                                        displayIndexFlag = true;
                                        break;
                                    }
                                case TimelineType.SlotColor:
                                    {
                                        var timeline = BaseObject.BorrowObject<SlotColorTimelineState>();
                                        timeline.slot = slot;
                                        timeline.Init(this._armature, this, timelineData);
                                        this._slotTimelines.Add(timeline);
                                        colorFlag = true;
                                        break;
                                    }
                                case TimelineType.SlotFFD:
                                    {
                                        var timeline = BaseObject.BorrowObject<SlotFFDTimelineState>();
                                        timeline.slot = slot;
                                        timeline.Init(this._armature, this, timelineData);
                                        this._slotTimelines.Add(timeline);
                                        ffdFlags.Add((int)timeline.meshOffset);
                                        break;
                                    }

                                default:
                                    break;
                            }
                        }
                    }

                    if (this.resetToPose)
                    {
                        // Pose timeline.
                        if (!displayIndexFlag)
                        {
                            var timeline = BaseObject.BorrowObject<SlotDislayIndexTimelineState>();
                            timeline.slot = slot;
                            timeline.Init(this._armature, this, null);
                            this._slotTimelines.Add(timeline);
                        }

                        if (!colorFlag)
                        {
                            var timeline = BaseObject.BorrowObject<SlotColorTimelineState>();
                            timeline.slot = slot;
                            timeline.Init(this._armature, this, null);
                            this._slotTimelines.Add(timeline);
                        }

                        if (slot.rawDisplayDatas != null)
                        {
                            foreach (var displayData in slot.rawDisplayDatas)
                            {
                                if (displayData != null && displayData.type == DisplayType.Mesh && ffdFlags.IndexOf((displayData as MeshDisplayData).offset) < 0)
                                {
                                    var timeline = BaseObject.BorrowObject<SlotFFDTimelineState>();
                                    timeline.slot = slot;
                                    timeline.Init(this._armature, this, null);
                                    this._slotTimelines.Add(timeline);
                                }
                            }
                        }
                    }
                }
            }

            foreach (var k in slotTimelines.Keys)
            {
                // Remove slot timelines.
                var timeLines = slotTimelines[k];
                foreach (var timeline in timeLines)
                {
                    this._slotTimelines.Remove(timeline);
                    timeline.ReturnToPool();
                }
            }
        }
        /**
         * @private
         * @internal
         */
        internal void AdvanceTime(float passedTime, float cacheFrameRate)
        {
            // Update fade time.
            if (this._fadeState != 0 || this._subFadeState != 0)
            {
                this._AdvanceFadeTime(passedTime);
            }

            // Update time.
            if (this._playheadState == 3)
            { 
                // 11
                if (this.timeScale != 1.0f)
                {
                    passedTime *= this.timeScale;
                }

                this._time += passedTime;
            }

            if (this._timelineDirty)
            {
                this._timelineDirty = false;
                this.UpdateTimelines();
            }

            if (this.weight == 0.0f)
            {
                return;
            }

            var isCacheEnabled = this._fadeState == 0 && cacheFrameRate > 0.0f;
            var isUpdateTimeline = true;
            var isUpdateBoneTimeline = true;
            var time = this._time;
            this._weightResult = this.weight * this._fadeProgress;

            // Update main timeline.
            this._actionTimeline.Update(time);

            if (isCacheEnabled)
            { 
                // Cache time internval.
                var internval = cacheFrameRate * 2.0f;
                this._actionTimeline.currentTime = (float)Math.Floor(this._actionTimeline.currentTime * internval) / internval;
            }

            if (this._zOrderTimeline != null)
            { 
                // Update zOrder timeline.
                this._zOrderTimeline.Update(time);
            }

            if (isCacheEnabled)
            { 
                // Update cache.
                var cacheFrameIndex = (int)Math.Floor(this._actionTimeline.currentTime * cacheFrameRate); // uint
                if (this._armature._cacheFrameIndex == cacheFrameIndex)
                { 
                    // Same cache.
                    isUpdateTimeline = false;
                    isUpdateBoneTimeline = false;
                }
                else
                {
                    this._armature._cacheFrameIndex = cacheFrameIndex;
                    if (this.animationData.cachedFrames[cacheFrameIndex])
                    { 
                        // Cached.
                        isUpdateBoneTimeline = false;
                    }
                    else
                    { 
                        // Cache.
                        this.animationData.cachedFrames[cacheFrameIndex] = true;
                    }
                }
            }

            if (isUpdateTimeline)
            {
                if (isUpdateBoneTimeline)
                {
                    Bone bone = null;
                    BoneTimelineState prevTimeline = null; //
                    for (int i = 0, l = this._boneTimelines.Count; i < l; ++i)
                    {
                        var timeline = this._boneTimelines[i];
                        if (bone != timeline.bone)
                        {
                            if (bone != null)
                            {
                                this._BlendBoneTimline(prevTimeline);
                                if (bone._blendDirty)
                                {
                                    if (bone._blendLeftWeight > 0.0f)
                                    {
                                        if (bone._blendLayer != this.layer)
                                        {
                                            if (bone._blendLayerWeight >= bone._blendLeftWeight)
                                            {
                                                bone._blendLeftWeight = 0.0f;
                                                bone = null;
                                            }
                                            else
                                            {
                                                bone._blendLayer = this.layer;
                                                bone._blendLeftWeight -= bone._blendLayerWeight;
                                                bone._blendLayerWeight = 0.0f;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        bone = null;
                                    }
                                }
                            }

                            bone = timeline.bone;
                        }

                        if (bone != null)
                        {
                            timeline.Update(time);
                            if (i == l - 1)
                            {
                                this._BlendBoneTimline(timeline);
                            }
                            else
                            {
                                prevTimeline = timeline;
                            }
                        }
                    }
                }

                for (int i = 0, l = this._slotTimelines.Count; i < l; ++i)
                {
                    var timeline = this._slotTimelines[i];
                    if (this._IsDisabled(timeline.slot))
                    {
                        continue;
                    }

                    timeline.Update(time);
                }
            }

            if (this._fadeState == 0)
            {
                if (this._subFadeState > 0)
                {
                    this._subFadeState = 0;
                }

                if (this._actionTimeline.playState > 0)
                {
                    if (this.autoFadeOutTime >= 0.0f)
                    { 
                        // Auto fade out.
                        this.FadeOut(this.autoFadeOutTime);
                    }
                }
            }
        }

        /**
         * 继续播放。
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public void Play()
        {
            this._playheadState = 3; // 11
        }
        /**
         * 暂停播放。
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public void Stop()
        {
            this._playheadState &= 1; // 0x
        }
        /**
         * 淡出动画。
         * @param fadeOutTime 淡出时间。 (以秒为单位)
         * @param pausePlayhead 淡出时是否暂停动画。
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public void FadeOut(float fadeOutTime, bool pausePlayhead = true)
        {
            if (fadeOutTime < 0.0f)
            {
                fadeOutTime = 0.0f;
            }

            if (pausePlayhead)
            {
                this._playheadState &= 2; // x0
            }

            if (this._fadeState > 0)
            {
                if (fadeOutTime > this.fadeTotalTime - this._fadeTime)
                { 
                    // If the animation is already in fade out, the new fade out will be ignored.
                    return;
                }
            }
            else
            {
                this._fadeState = 1;
                this._subFadeState = -1;

                if (fadeOutTime <= 0.0f || this._fadeProgress <= 0.0f)
                {
                    this._fadeProgress = 0.000001f; // Modify fade progress to different value.
                }

                foreach (var timeline in this._boneTimelines)
                {
                    timeline.FadeOut();
                }

                foreach (var timeline in this._slotTimelines)
                {
                    timeline.FadeOut();
                }
            }

            this.displayControl = false; //
            this.fadeTotalTime = this._fadeProgress > 0.000001f ? fadeOutTime / this._fadeProgress : 0.0f;
            this._fadeTime = this.fadeTotalTime * (1.0f - this._fadeProgress);
        }

        /**
         * 是否包含骨骼遮罩。
         * @param name 指定的骨骼名称。
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public bool ContainsBoneMask(string name)
        {
            return this._boneMask.Count == 0 || this._boneMask.IndexOf(name) >= 0;
        }
        /**
         * 添加骨骼遮罩。
         * @param name 指定的骨骼名称。
         * @param recursive 是否为该骨骼的子骨骼添加遮罩。
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public void AddBoneMask(string name, bool recursive = true)
        {
            var currentBone = this._armature.GetBone(name);
            if (currentBone == null) {
                return;
            }

            if (this._boneMask.IndexOf(name) < 0)
            { 
                // Add mixing
                this._boneMask.Add(name);
            }

            if (recursive)
            { 
                // Add recursive mixing.
                foreach (var bone in this._armature.GetBones())
                {
                    if (this._boneMask.IndexOf(bone.name) < 0 && currentBone.Contains(bone))
                    {
                    this._boneMask.Add(bone.name);
                    }
                }
            }

            this._timelineDirty = true;
        }
        /**
         * 删除骨骼遮罩。
         * @param name 指定的骨骼名称。
         * @param recursive 是否删除该骨骼的子骨骼遮罩。
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public void RemoveBoneMask(string name, bool recursive = true)
        {
            if (this._boneMask.Contains(name))
            {
                this._boneMask.Remove(name);
            }

            if (recursive)
            {
                var currentBone = this._armature.GetBone(name);
                if (currentBone != null)
                {
                    var bones = this._armature.GetBones();
                    if (this._boneMask.Count > 0)
                    {
                        // Remove recursive mixing.
                        foreach (var bone in bones)
                        {
                            if (this._boneMask.Contains(bone.name) && currentBone.Contains(bone))
                            {
                                this._boneMask.Remove(bone.name);
                            }
                        }
                    }
                    else
                    {
                        // Add unrecursive mixing.
                        foreach (var bone in bones)
                        {
                            if (bone == currentBone)
                            {
                                continue;
                            }

                            if (!currentBone.Contains(bone))
                            {
                                this._boneMask.Add(bone.name);
                            }
                        }
                    }
                }
            }

            this._timelineDirty = true;
        }
        /**
         * 删除所有骨骼遮罩。
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public void RemoveAllBoneMask()
        {
            this._boneMask.Clear();
            this._timelineDirty = true;
        }
        /**
         * 是否正在淡入。
         * @version DragonBones 5.1
         * @language zh_CN
         */
        public bool isFadeIn
        {
            get { return this._fadeState < 0; }
        }
        /**
         * 是否正在淡出。
         * @version DragonBones 5.1
         * @language zh_CN
         */
        public bool isFadeOut
        {
            get { return this._fadeState > 0; }
        }
        /**
         * 是否淡入完毕。
         * @version DragonBones 5.1
         * @language zh_CN
         */
        public bool isFadeComplete
        {
            get { return this._fadeState == 0; }
        }
        /**
         * 是否正在播放。
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public bool isPlaying
        {
            get { return (this._playheadState & 2) != 0 && this._actionTimeline.playState <= 0; }
        }
        /**
         * 是否播放完毕。
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public bool isCompleted
        {
            get { return this._actionTimeline.playState > 0; }
        }
        /**
         * 当前播放次数。
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public int currentPlayTimes
        {
            get { return this._actionTimeline.currentPlayTimes; }
        }

        /**
         * 总时间。 (以秒为单位)
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public float totalTime
        {
            get { return this._duration; }
        }
        /**
         * 当前播放的时间。 (以秒为单位)
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public float currentTime
        {
            get { return this._actionTimeline.currentTime; }
            set
            {
                var currentPlayTimes = this._actionTimeline.currentPlayTimes - (this._actionTimeline.playState > 0 ? 1 : 0);
                if (value < 0.0f || this._duration < value)
                {
                    value = (value % this._duration) + currentPlayTimes * this._duration;
                    if (value < 0.0f)
                    {
                        value += this._duration;
                    }
                }

                if (this.playTimes > 0 && currentPlayTimes == this.playTimes - 1 && value == this._duration)
                {
                    value = this._duration - 0.000001f;
                }

                if (this._time == value)
                {
                    return;
                }

                this._time = value;
                this._actionTimeline.SetCurrentTime(this._time);

                if (this._zOrderTimeline != null)
                {
                    this._zOrderTimeline.playState = -1;
                }

                foreach (var timeline in this._boneTimelines)
                {
                    timeline.playState = -1;
                }

                foreach (var timeline in this._slotTimelines)
                {
                    timeline.playState = -1;
                }
            }
        }
}
}
