using System.Collections.Generic;

namespace DragonBones
{
    /**
     * @language zh_CN
     * 骨架代理接口。
     * @version DragonBones 5.0
     */
    public interface IArmatureProxy : IEventDispatcher<EventObject>
    {
        /**
         * @private
         */
        void _onClear();
        /**
         * @language zh_CN
         * 释放代理和骨架。 (骨架会回收到对象池)
         * @version DragonBones 4.5
         */
        void Dispose(bool disposeProxy);
        /**
         * @language zh_CN
         * 获取骨架。
         * @readOnly
         * @see dragonBones.Armature
         * @version DragonBones 4.5
         */
        Armature armature
        {
            get;
        }
        /**
         * @language zh_CN
         * 获取动画控制器。
         * @readOnly
         * @see dragonBones.Animation
         * @version DragonBones 4.5
         */
        Animation animation
        {
            get;
        }
    }
    /**
     * @deprecated
     * @see dragonBones.core.IArmatureProxy
     */
    public interface IArmatureDisplay : IArmatureProxy
    {
    }
}
