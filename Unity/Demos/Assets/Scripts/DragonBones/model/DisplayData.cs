using System.Collections.Generic;

namespace DragonBones
{
    /**
     * @private
     */
    public abstract class DisplayData : BaseObject
    {
        public DisplayType type;
        public string name;
        public string path;
        public readonly Transform transform = new Transform();
        public ArmatureData parent;

        protected override void _OnClear()
        {
            this.name = "";
            this.path = "";
            this.transform.Identity();
            this.parent = null; //
        }
    }

    /**
     * @private
     */
    public class ImageDisplayData : DisplayData
    {
        public readonly Point pivot = new Point();
        public TextureData texture = null;

        protected override void _OnClear()
        {
            base._OnClear();

            this.type = DisplayType.Image;
            this.pivot.Clear();
            this.texture = null;
        }
    }

    /**
     * @private
     */
    public class ArmatureDisplayData : DisplayData
    {
        public bool inheritAnimation;
        public readonly List<ActionData> actions = new List<ActionData>();
        public ArmatureData armature = null;

        protected override void _OnClear()
        {
            base._OnClear();

            foreach (var action in this.actions) {
                action.ReturnToPool();
            }

            this.type = DisplayType.Armature;
            this.inheritAnimation = false;
            this.actions.Clear();
            this.armature = null;
        }

        /**
         * @private
         */
        internal void AddAction(ActionData value)
        {
            this.actions.Add(value);
        }
}

    /**
     * @private
     */
    public class MeshDisplayData : ImageDisplayData
    {
        public bool inheritAnimation;
        public int offset; // IntArray.
        internal WeightData weight = null; // Initial value.

        protected override void _OnClear()
        {
            base._OnClear();

            if (this.weight != null)
            {
                this.weight.ReturnToPool();
            }

            this.type = DisplayType.Mesh;
            this.inheritAnimation = false;
            this.offset = 0;
            this.weight = null;
        }
    }

    /**
     * @private
     */
    public class BoundingBoxDisplayData : DisplayData
    {
        public BoundingBoxData boundingBox = null; // Initial value.

        protected override void _OnClear()
        {
            base._OnClear();

            if (this.boundingBox != null)
            {
                this.boundingBox.ReturnToPool();
            }

            this.type = DisplayType.BoundingBox;
            this.boundingBox = null;
        }
    }

    /**
     * @private
     */
    public class WeightData : BaseObject
    {
        public int count;
        public int offset; // IntArray.
        public readonly List<BoneData> bones = new List<BoneData>();

        protected override void _OnClear()
        {
            this.count = 0;
            this.offset = 0;
            this.bones.Clear();
        }

        internal void AddBone(BoneData value)
        {
            this.bones.Add(value);
        }
    }
}
