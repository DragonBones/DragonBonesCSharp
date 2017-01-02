using System.Collections.Generic;

namespace DragonBones
{
    /**
     * @private
     */
    public abstract class TimelineData<T> : BaseObject where T : FrameData<T>
    {
        public float scale;
        /**
         * @private
         */
        public float offset;
        /**
         * @private
         */
        public readonly List<T> frames = new List<T>();
        /**
         * @private
         */
        public TimelineData()
        {
        }
        /**
         * @private
         */
        protected override void _onClear()
        {
            T prevFrame = null;
            foreach (var frame in frames)
            {
                if (prevFrame != null && frame != prevFrame) // Find key frame data.
                {
                    prevFrame.ReturnToPool();
                }

                prevFrame = frame;
            }

            scale = 1.0f;
            offset = 0.0f;
            frames.Clear();
        }
    }
    /**
     * @private
     */
    public class ZOrderTimelineData : TimelineData<ZOrderFrameData>
    {
    }
    /**
     * @private
     */
    public class BoneTimelineData : TimelineData<BoneFrameData>
    {
        public BoneData bone;
        public readonly Transform originTransform = new Transform();

        public BoneTimelineData()
        {
        }
        
        protected override void _onClear()
        {
            base._onClear();

            bone = null;
            originTransform.Identity();
        }
    }
    /**
     * @private
     */
    public class SlotTimelineData : TimelineData<SlotFrameData>
    {
        public SlotData slot;

        public SlotTimelineData()
        {
        }
        
        protected override void _onClear()
        {
            base._onClear();

            slot = null;
        }
    }
    /**
     * @private
     */
    public class FFDTimelineData : TimelineData<ExtensionFrameData>
    {
        public SkinData skin;
        public SkinSlotData slot;
        public DisplayData display;

        public FFDTimelineData()
        {
        }
        
        protected override void _onClear()
        {
            base._onClear();
            
            skin = null;
            slot = null;
            display = null;
        }
    }
}
