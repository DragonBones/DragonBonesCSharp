namespace DragonBones
{
    public abstract class BoundingBoxData : BaseObject
    {
        /**
         * 边界框类型。
         * @version DragonBones 5.0
         * @language zh_CN
         */
        public BoundingBoxType type;
        /**
         * 边界框颜色。
         * @version DragonBones 5.0
         * @language zh_CN
         */
        public uint color;
        /**
         * 边界框宽。（本地坐标系）
         * @version DragonBones 5.0
         * @language zh_CN
         */
        public float width;
        /**
         * 边界框高。（本地坐标系）
         * @version DragonBones 5.0
         * @language zh_CN
         */
        public float height;

        protected override void _OnClear()
        {
            this.color = 0x000000;
            this.width = 0.0f;
            this.height = 0.0f;
        }
        /**
         * 是否包含点。
         * @version DragonBones 5.0
         * @language zh_CN
         */
        public abstract bool ContainsPoint(float pX, float pY);

        /**
         * 是否与线段相交。
         * @version DragonBones 5.0
         * @language zh_CN
         */
        public abstract int IntersectsSegment(
            float xA, float yA, float xB, float yB,
            Point intersectionPointA = null,
            Point intersectionPointB = null,
            Point normalRadians = null
        );
    }
}