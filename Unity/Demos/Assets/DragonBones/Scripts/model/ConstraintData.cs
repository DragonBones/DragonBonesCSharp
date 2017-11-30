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
        public BoneData root;
        public BoneData bone = null;

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
        public bool scaleEnabled;
        public bool bendPositive;
        public float weight;

        protected override void _OnClear()
        {
            base._OnClear();

            this.scaleEnabled = false;
            this.bendPositive = false;
            this.weight = 1.0f;
        }
    }
}
