using System;
using System.Collections.Generic;

namespace DragonBones
{
    /**
     * @private 
     */
    public class CanvasData : BaseObject
    {
        public bool hasBackground;
        public int color;
        public float x;
        public float y;
        public float width;
        public float height;

        /**
         * @private
         */
        protected override void _OnClear()
        {
            this.hasBackground = false;
            this.color = 0x000000;
            this.x = 0.0f;
            this.y = 0.0f;
            this.width = 0.0f;
            this.height = 0.0f;
        }
    }

    /**
     * 骨架数据。
     * @version DragonBones 3.0
     * @language zh_CN
     */
    public class ArmatureData : BaseObject
    {

        protected override void _OnClear()
        {
            throw new NotImplementedException();
        }
    }

    /**
     * 骨骼数据。
     * @version DragonBones 3.0
     * @language zh_CN
     */
    public class BoneData : BaseObject
    {
        /**
         * @private
         */
        public bool inheritTranslation;
        /**
         * @private
         */
        public bool inheritRotation;
        /**
         * @private
         */
        public bool inheritScale;
        /**
         * @private
         */
        public bool inheritReflection;
        /**
         * @private
         */
        public float length;
        /**
         * 数据名称。
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public string name;
        /**
         * @private
         */
        public readonly Transform transform = new Transform();
        /**
         * @private
         */
        public readonly List<ConstraintData> constraints = new List<ConstraintData>();
        /**
         * @private
         */
        public UserData userData = null; // Initial value.
        /**
         * 所属的父骨骼数据。
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public BoneData parent = null;

        /**
         * @private
         */
        protected override void _OnClear()
        {
            foreach (var constraint in this.constraints)
            {
                constraint.ReturnToPool();
            }

            if (this.userData != null)
            {
                this.userData.ReturnToPool();
            }

            this.inheritTranslation = false;
            this.inheritRotation = false;
            this.inheritScale = false;
            this.inheritReflection = false;
            this.length = 0.0f;
            this.name = "";
            this.transform.Identity();
            this.constraints.Clear();
            this.userData = null;
            this.parent = null;
        }
    }

    /**
     * 插槽数据。
     * @see dragonBones.Slot
     * @version DragonBones 3.0
     * @language zh_CN
     */
    public class SlotData : BaseObject
    {
        /**
         * @private
         */
        public static readonly ColorTransform DEFAULT_COLOR = new ColorTransform();

        /**
         * @private
         */
        public static ColorTransform CreateColor()
        {
            return new ColorTransform();
        }

        /**
         * @private
         */
        public BlendMode blendMode;
        /**
         * @private
         */
        public int displayIndex;
        /**
         * @private
         */
        public int zOrder;
        /**
         * 数据名称。
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public string name;
        /**
         * @private
         */
        public ColorTransform color = null; // Initial value.
        /**
         * @private
         */
        public UserData userData = null; // Initial value.
        /**
         * 所属的父骨骼数据。
         * @see dragonBones.BoneData
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public BoneData parent;
        /**
         * @private
         */
        protected override void _OnClear()
        {
            if (this.userData != null)
            {
                this.userData.ReturnToPool();
            }

            this.blendMode = BlendMode.Normal;
            this.displayIndex = 0;
            this.zOrder = 0;
            this.name = "";
            this.color = null; //
            this.userData = null;
            this.parent = null; //
        }
    }
    /**
     * 皮肤数据。（通常一个骨架数据至少包含一个皮肤数据）
     * @version DragonBones 3.0
     * @language zh_CN
     */
    public class SkinData : BaseObject
    {
        /**
         * 数据名称。
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public string name;
        /**
         * @private
         */
        public readonly Dictionary<string, List<DisplayData>> displays = new Dictionary<string, List<DisplayData>>();


    }
}