namespace DragonBones
{
    /**
     * @private
     */
    public delegate void ListenerDelegate<T>(string type, T eventObject);
    /**
     * @language zh_CN
     * 事件接口。
     * @version DragonBones 4.5
     */
    public interface IEventDispatcher<T>
    {
        /**
         * @language zh_CN
         * 是否包含指定类型的事件。
         * @param type 事件类型。
         * @version DragonBones 4.5
         */
        bool HasEventListener(string type);
        /**
         * @language zh_CN
         * 添加事件。
         * @param type 事件类型。
         * @param listener 事件回调。
         * @version DragonBones 4.5
         */
        void AddEventListener(string type, ListenerDelegate<T> listener);
        /**
         * @language zh_CN
         * 移除事件。
         * @param type 事件类型。
         * @param listener 事件回调。
         * @version DragonBones 4.5
         */
        void RemoveEventListener(string type, ListenerDelegate<T> listener);
        /**
         * @private
         */
        void DispatchEvent(string type, T eventObject);
    }
}
