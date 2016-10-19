using System;
using System.Collections.Generic;

namespace DragonBones
{
    /**
     * @language zh_CN
     * 事件数据。
     * @version DragonBones 4.5
     */
    public class EventObject : BaseObject
    {
        /**
         * @language zh_CN
         * 动画开始。
         * @version DragonBones 4.5
         */
        public const string START = "start";

        /**
         * @language zh_CN
         * 动画循环播放一次完成。
         * @version DragonBones 4.5
         */
        public const string LOOP_COMPLETE = "loopComplete";

        /**
         * @language zh_CN
         * 动画播放完成。
         * @version DragonBones 4.5
         */
        public const string COMPLETE = "complete";

        /**
         * @language zh_CN
         * 动画淡入开始。
         * @version DragonBones 4.5
         */
        public const string FADE_IN = "fadeIn";

        /**
         * @language zh_CN
         * 动画淡入完成。
         * @version DragonBones 4.5
         */
        public const string FADE_IN_COMPLETE = "fadeInComplete";

        /**
         * @language zh_CN
         * 动画淡出开始。
         * @version DragonBones 4.5
         */
        public const string FADE_OUT = "fadeOut";

        /**
         * @language zh_CN
         * 动画淡出完成。
         * @version DragonBones 4.5
         */
        public const string FADE_OUT_COMPLETE = "fadeOutComplete";

        /**
         * @language zh_CN
         * 动画帧事件。
         * @version DragonBones 4.5
         */
        public const string FRAME_EVENT = "frameEvent";

        /**
         * @language zh_CN
         * 动画声音事件。
         * @version DragonBones 4.5
         */
        public const string SOUND_EVENT = "soundEvent";

        /**
         * @language zh_CN
         * 事件类型。
         * @version DragonBones 4.5
         */
        public string type;

        /**
         * @language zh_CN
         * 事件名称。 (帧标签的名称或声音的名称)
         * @version DragonBones 4.5
         */
        public string name;

        /**
         * @language zh_CN
         * 扩展的数据。
         * @version DragonBones 4.5
         */
        // public object data; // TODO

        /**
         * @language zh_CN
         * 发出事件的骨架。
         * @version DragonBones 4.5
         */
        public Armature armature;

        /**
         * @language zh_CN
         * 发出事件的骨骼。
         * @version DragonBones 4.5
         */
        public Bone bone;

        /**
         * @language zh_CN
         * 发出事件的插槽。
         * @version DragonBones 4.5
         */
        public Slot slot;

        /**
         * @language zh_CN
         * 发出事件的动画状态。
         * @version DragonBones 4.5
         */
        public AnimationState animationState;

        /**
         * @private
         */
        public AnimationFrameData frame;

        /**
         * @language zh_CN
         * 用户数据。
         * @version DragonBones 4.5
         */
        public object userData;

        /**
         * @private
         */
        public EventObject()
        {
        }

        /**
         * @inheritDoc
         */
        protected override void _onClear()
        {
            type = null;
            name = null;
            // data = null;
            armature = null;
            bone = null;
            slot = null;
            animationState = null;
            frame = null;
            userData = null;
        }
    }
}