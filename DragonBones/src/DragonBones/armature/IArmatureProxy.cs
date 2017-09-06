namespace DragonBones
{
    /**
     * 骨架代理接口。
     * @version DragonBones 5.0
     * @language zh_CN
     */
    public interface IArmatureProxy : IEventDispatcher<EventObject>
    {
        /**
         * @private
         */
        void DBInit(Armature armature);
        /**
         * @private
         */
        void DBClear();
        /**
         * @private
         */
        void DBUpdate();
        /**
         * 释放代理和骨架。 (骨架会回收到对象池)
         * @version DragonBones 4.5
         * @language zh_CN
         */
        void Dispose(bool disposeProxy);
        /**
         * 获取骨架。
         * @see dragonBones.Armature
         * @version DragonBones 4.5
         * @language zh_CN
         */
         Armature armature { get; }
        /**
         * 获取动画控制器。
         * @see dragonBones.Animation
         * @version DragonBones 4.5
         * @language zh_CN
         */
         AnimationConfig animation { get; }
    }

    /**
     * @deprecated
     * @see DragonBones.IArmatureProxy
     */
    public interface IArmatureDisplay : IArmatureProxy
    {
    }
}
