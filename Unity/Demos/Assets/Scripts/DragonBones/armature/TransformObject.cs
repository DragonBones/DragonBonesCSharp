using System;
using System.Collections.Generic;
using System.Text;

namespace DragonBones
{
    /**
     * 基础变换对象。
     * @version DragonBones 4.5
     * @language zh_CN
     */
    public abstract class TransformObject : BaseObject
    {
        /**
         * @private
         */
        protected static readonly Matrix _helpMatrix  = new Matrix();
        /**
         * @private
         */
        protected static readonly Transform _helpTransform  = new Transform();
        /**
         * @private
         */
        protected static readonly Point _helpPoint = new Point();
        /**
         * 相对于骨架坐标系的矩阵。
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public readonly Matrix globalTransformMatrix = new Matrix();
        /**
         * 相对于骨架坐标系的变换。
         * @see dragonBones.Transform
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public readonly Transform global = new Transform();
        /**
         * 相对于骨架或父骨骼坐标系的偏移变换。
         * @see dragonBones.Transform
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public readonly Transform offset = new Transform();
        /**
         * 相对于骨架或父骨骼坐标系的绑定变换。
         * @see dragonBones.Transform
         * @version DragonBones 3.0
         * @readOnly
         * @language zh_CN
         */
        public Transform origin;
        /**
         * 可以用于存储临时数据。
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public object userData;
        /**
         * @private
         */
        protected bool _globalDirty;
        /**
         * @internal
         * @private
         */
        internal Armature _armature;
        /**
         * @internal
         * @private
         */
        internal Bone _parent;
        /**
         * @private
         */
        protected override void _OnClear()
        {
            this.globalTransformMatrix.Identity();
            this.global.Identity();
            this.offset.Identity();
            this.origin = null; //
            this.userData = null;

            this._globalDirty = false;
            this._armature = null; //
            this._parent = null; //
        }

        /**
         * @internal
         * @private
         */
        internal virtual void _SetArmature(Armature value = null)
        {
            this._armature = value;
        }
        /**
         * @internal
         * @private
         */
        internal void _SetParent(Bone value = null)
        {
            this._parent = value;
        }
        /**
         * @private
         */
        public void UpdateGlobalTransform()
        {
            if (this._globalDirty)
            {
                this._globalDirty = false;
                this.global.FromMatrix(this.globalTransformMatrix);
            }
        }
        /**
         * 所属的骨架。
         * @see dragonBones.Armature
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public Armature armature
        {
            get{ return this._armature; }
        }
        /**
         * 所属的父骨骼。
         * @see dragonBones.Bone
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public Bone parent
        {
            get { return this._parent; }
        }
    }
}
