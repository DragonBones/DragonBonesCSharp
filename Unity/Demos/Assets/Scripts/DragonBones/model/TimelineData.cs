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

        public TimelineData()
        {
        }

        /**
         * @inheritDoc
         */
        protected override void _onClear()
        {
            T prevFrame = null;
            foreach (var frame in frames)
            {
                if (prevFrame != null && frame != prevFrame)
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
        public const uint CACHE_FRAME_COUNT = 11;

        public BoneData bone;
        public float[] cachedFrames; // flag a b c d tx ty skewX skewY scaleX scaleY
        public readonly Transform originTransform = new Transform();

        public BoneTimelineData()
        {
        }

        /**
         * @inheritDoc
         */
        protected override void _onClear()
        {
            base._onClear();

            bone = null;
            cachedFrames = null;
            originTransform.Identity();
        }

        public void CacheFrames(uint cacheFrameCount)
        {
            cachedFrames = new float[cacheFrameCount * CACHE_FRAME_COUNT];
            for (int i = 0, l = cachedFrames.Length; i < l; ++i)
            {
                cachedFrames[i] = -1.0f;
            }
        }
    }

    /**
     * @private
     */
    public class SlotTimelineData : TimelineData<SlotFrameData>
    {
        public const uint CACHE_FRAME_COUNT = 11;

        public SlotData slot;
        public float[] cachedFrames; // flag a b c d tx ty skewX skewY scaleX scaleY

        public SlotTimelineData()
        {
        }

        /**
         * @inheritDoc
         */
        protected override void _onClear()
        {
            base._onClear();

            slot = null;
            cachedFrames = null;
        }

        public void CacheFrames(uint cacheFrameCount)
        {
            cachedFrames = new float[cacheFrameCount * CACHE_FRAME_COUNT];
            for (int i = 0, l = cachedFrames.Length; i < l; ++i)
            {
                cachedFrames[i] = -1.0f;
            }
        }
    }

    /**
     * @private
     */
    public class FFDTimelineData : TimelineData<ExtensionFrameData>
    {
        public int displayIndex;
        public SkinData skin;
        public SlotDisplayDataSet slot;

        public FFDTimelineData()
        {
        }

        /**
         * @inheritDoc
         */
        protected override void _onClear()
        {
            base._onClear();

            displayIndex = 0;
            skin = null;
            slot = null;
        }
    }
}
