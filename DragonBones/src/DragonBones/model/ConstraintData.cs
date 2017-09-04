namespace DragonBones
{
    /**
     * @private
     */
    public abstract class ConstraintData : BaseObject
    {
        public int order;
        public BoneData target;
        public BoneData bone;
        public BoneData root = null;

        protected override void _onClear()
        {
            this.order = 0;
            this.target = null; 
            this.bone = null; 
            this.root = null;
        }
    }
    /**
     * @private
     */
    public class IKConstraintData : ConstraintData
    {
        public bool bendPositive;
        public bool scaleEnabled;
        public float weight;

        protected override void _onClear()
        {
            base._onClear();

            this.bendPositive = false;
            this.scaleEnabled = false;
            this.weight = 1.0f;
        }
}
}
