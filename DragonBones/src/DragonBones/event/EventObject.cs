namespace DragonBones
{
    public class EventObject : BaseObject
    {
        /**
         * 动画开始。
         * @version DragonBones 4.5
         * @language zh_CN
         */
        public const string START = "start";
        /**
         * 动画循环播放一次完成。
         * @version DragonBones 4.5
         * @language zh_CN
         */
        public const string LOOP_COMPLETE = "loopComplete";
        /**
         * 动画播放完成。
         * @version DragonBones 4.5
         * @language zh_CN
         */
        public const string COMPLETE = "complete";
        /**
         * 动画淡入开始。
         * @version DragonBones 4.5
         * @language zh_CN
         */
        public const string FADE_IN = "fadeIn";
        /**
         * 动画淡入完成。
         * @version DragonBones 4.5
         * @language zh_CN
         */
        public const string FADE_IN_COMPLETE = "fadeInComplete";
        /**
         * 动画淡出开始。
         * @version DragonBones 4.5
         * @language zh_CN
         */
        public const string FADE_OUT = "fadeOut";
        /**
         * 动画淡出完成。
         * @version DragonBones 4.5
         * @language zh_CN
         */
        public const string FADE_OUT_COMPLETE = "fadeOutComplete";
        /**
         * 动画帧事件。
         * @version DragonBones 4.5
         * @language zh_CN
         */
        public const string FRAME_EVENT = "frameEvent";
        /**
         * 动画声音事件。
         * @version DragonBones 4.5
         * @language zh_CN
         */
        public const string SOUND_EVENT = "soundEvent";

        /**
         * @private
         */
        public float time;
        /**
         * 事件类型。
         * @version DragonBones 4.5
         * @language zh_CN
         */
        public string type;
        /**
         * 事件名称。 (帧标签的名称或声音的名称)
         * @version DragonBones 4.5
         * @language zh_CN
         */
        public string name;
        /**
         * 发出事件的骨架。
         * @version DragonBones 4.5
         * @language zh_CN
         */
        public Armature armature;
        /**
         * 发出事件的骨骼。
         * @version DragonBones 4.5
         * @language zh_CN
         */
        public Bone bone;
        /**
         * 发出事件的插槽。
         * @version DragonBones 4.5
         * @language zh_CN
         */
        public Slot slot;
        /**
         * 发出事件的动画状态。
         * @version DragonBones 4.5
         * @language zh_CN
         */
        public AnimationState animationState;
        /**
         * 自定义数据
         * @see dragonBones.CustomData
         * @version DragonBones 5.0
         * @language zh_CN
         */
        public UserData data;

        protected override void _OnClear()
        {
            this.time = 0.0f;
            this.type = string.Empty;
            this.name = string.Empty;
            this.armature = null;
            this.bone = null;
            this.slot = null;
            this.animationState = null;
            this.data = null;
        }
    }
}
