using System;
using System.Collections.Generic;
using System.Text;

namespace DragonBones
{
    public class EventObject : BaseObject
    {
        /**
         * 动画开始。
         * @version DragonBones 4.5
         * @language zh_CN
         */
        public static readonly string START = "start";
        /**
         * 动画循环播放一次完成。
         * @version DragonBones 4.5
         * @language zh_CN
         */
        public static readonly string LOOP_COMPLETE = "loopComplete";
        /**
         * 动画播放完成。
         * @version DragonBones 4.5
         * @language zh_CN
         */
        public static readonly string COMPLETE = "complete";
        /**
         * 动画淡入开始。
         * @version DragonBones 4.5
         * @language zh_CN
         */
        public static readonly string FADE_IN = "fadeIn";
        /**
         * 动画淡入完成。
         * @version DragonBones 4.5
         * @language zh_CN
         */
        public static readonly string FADE_IN_COMPLETE = "fadeInComplete";
        /**
         * 动画淡出开始。
         * @version DragonBones 4.5
         * @language zh_CN
         */
        public static readonly string FADE_OUT = "fadeOut";
        /**
         * 动画淡出完成。
         * @version DragonBones 4.5
         * @language zh_CN
         */
        public static readonly string FADE_OUT_COMPLETE = "fadeOutComplete";
        /**
         * 动画帧事件。
         * @version DragonBones 4.5
         * @language zh_CN
         */
        public static readonly string FRAME_EVENT = "frameEvent";
        /**
         * 动画声音事件。
         * @version DragonBones 4.5
         * @language zh_CN
         */
        public static readonly string SOUND_EVENT = "soundEvent";
    }
}
