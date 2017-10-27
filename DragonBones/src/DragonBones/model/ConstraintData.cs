namespace DragonBones
{
    /**
     * @private
     */
    public abstract class ConstraintData : BaseObject
    {
        public int order;
        public string name;
        public BoneData target;
        public BoneData bone;
        public BoneData root = null;

        protected override void _OnClear()
        {
            this.order = 0;
            this.name = string.Empty;
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

        protected override void _OnClear()
        {
            base._OnClear();

            this.bendPositive = false;
            this.scaleEnabled = false;
            this.weight = 1.0f;
        }
}
}
