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
    }
}
