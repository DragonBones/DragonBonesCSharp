using System.Collections.Generic;

namespace DragonBones
{
    /**
     * @language zh_CN
     * 基础变换对象。
     * @version DragonBones 4.5
     */
    public abstract class TransformObject : BaseObject
    {
        /**
         * @language zh_CN
         * 可以用于存储临时数据。
         * @version DragonBones 3.0
         */
        public object userData;

        /**
         * @language zh_CN
         * 对象的名称。
         * @version DragonBones 3.0
         */
        public string name;

        /**
         * @language zh_CN
         * 相对于骨架坐标系的变换。
         * @see dragonBones.Transform
         * @version DragonBones 3.0
         */
        public readonly Transform global = new Transform();

        /**
         * @language zh_CN
         * 相对于骨架或父骨骼坐标系的绑定变换。
         * @see dragonBones.Transform
         * @version DragonBones 3.0
         */
        public readonly Transform origin = new Transform();

        /**
         * @language zh_CN
         * 相对于骨架或父骨骼坐标系的偏移变换。
         * @see dragonBones.Transform
         * @version DragonBones 3.0
         */
        public readonly Transform offset = new Transform();

        /**
         * @language zh_CN
         * 相对于骨架坐标系的矩阵。
         * @version DragonBones 3.0
         */
        public readonly Matrix globalTransformMatrix = new Matrix();

        /**
         * @private
         */
        internal Armature _armature;

        /**
         * @private
         */
        internal Bone _parent;

        /**
         * @private
         */
        public TransformObject()
        {
        }

        /**
         * @inheritDoc
         */
        protected override void _onClear()
        {
            userData = null;
            name = null;
            global.Identity();
            origin.Identity();
            offset.Identity();
            globalTransformMatrix.Identity();

            _armature = null;
            _parent = null;
        }

        /**
         * @private
         */
        internal virtual void _setArmature(Armature value)
        {
            _armature = value;
        }

        /**
         * @private
         */
        internal void _setParent(Bone value)
        {
            _parent = value;
        }

        /**
         * @language zh_CN
         * 所属的骨架。
         * @see dragonBones.Armature
         * @version DragonBones 3.0
         */
        public Armature armature
        {
            get { return _armature; }
        }

        /**
         * @language zh_CN
         * 所属的父骨骼。
         * @see dragonBones.Bone
         * @version DragonBones 3.0
         */
        public Bone parent
        {
            get { return _parent; }
        }
    }
}