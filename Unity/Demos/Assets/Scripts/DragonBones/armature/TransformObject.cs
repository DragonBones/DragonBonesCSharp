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
         * 对象的名称。
         * @readOnly
         * @version DragonBones 3.0
         */
        public string name;
        /**
         * @language zh_CN
         * 相对于骨架坐标系的矩阵。
         * @readOnly
         * @version DragonBones 3.0
         */
        public readonly Matrix globalTransformMatrix = new Matrix();
        /**
         * @language zh_CN
         * 相对于骨架坐标系的变换。
         * @readOnly
         * @see DragonBones.Transform
         * @version DragonBones 3.0
         */
        public readonly Transform global = new Transform();
        /**
         * @language zh_CN
         * 相对于骨架或父骨骼坐标系的偏移变换。
         * @see DragonBones.Transform
         * @version DragonBones 3.0
         */
        public readonly Transform offset = new Transform();
        /**
         * @language zh_CN
         * 相对于骨架或父骨骼坐标系的绑定变换。
         * @readOnly
         * @see DragonBones.Transform
         * @version DragonBones 3.0
         */
        public Transform origin;
        /**
         * @language zh_CN
         * 可以用于存储临时数据。
         * @version DragonBones 3.0
         */
        public object userData;
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
         * @private
         */
        protected override void _onClear()
        {
            name = null;
            global.Identity();
            offset.Identity();
            globalTransformMatrix.Identity();
            origin = null;
            userData = null;

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
         * @see DragonBones.Armature
         * @version DragonBones 3.0
         */
        public Armature armature
        {
            get { return _armature; }
        }
        /**
         * @language zh_CN
         * 所属的父骨骼。
         * @see DragonBones.Bone
         * @version DragonBones 3.0
         */
        public Bone parent
        {
            get { return _parent; }
        }
    }
}