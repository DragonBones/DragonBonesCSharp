using System.Collections.Generic;

namespace dragonBones
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
                    prevFrame.returnToPool();
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
    public class BoneTimelineData : TimelineData<BoneFrameData>
    {
        public static Matrix cacheFrame(List<Matrix> cacheFrames, int cacheFrameIndex, Matrix globalTransformMatrix)
        {
            var cacheMatrix = cacheFrames[cacheFrameIndex] = new Matrix();
            cacheMatrix.copyFrom(globalTransformMatrix);

            return cacheMatrix;
        }

        public BoneData bone;
        public readonly Transform originTransform = new Transform();
        public readonly List<Matrix> cachedFrames = new List<Matrix>();

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
            originTransform.identity();
            cachedFrames.Clear();
        }

        public void cacheFrames(uint cacheFrameCount)
        {
            cachedFrames.Clear();
            // cachedFrames.length = cacheFrameCount; // TODO
        }
    }

    /**
     * @private
     */
    public class SlotTimelineData : TimelineData<SlotFrameData>
    {
        public static Matrix cacheFrame(List<Matrix> cacheFrames, int cacheFrameIndex, Matrix globalTransformMatrix)
        {
            var cacheMatrix = cacheFrames[cacheFrameIndex] = new Matrix();
            cacheMatrix.copyFrom(globalTransformMatrix);

            return cacheMatrix;
        }

        public SlotData slot;
        public readonly List<Matrix> cachedFrames = new List<Matrix>();

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
            cachedFrames.Clear();
        }

        public void cacheFrames(uint cacheFrameCount)
        {
            cachedFrames.Clear();
            DragonBones.resizeList(cachedFrames, (int)cacheFrameCount, null);
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
