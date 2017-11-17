namespace DragonBones
{
    /**
     * @private
     */
    public delegate void ListenerDelegate<T>(string type, T eventObject);
    /**
     * 事件接口。
     * @version DragonBones 4.5
     * @language zh_CN
     */
    public interface IEventDispatcher<T>
    {
        bool HasDBEventListener(string type);
        void DispatchDBEvent(string type, T eventObject);
        void AddDBEventListener(string type, ListenerDelegate<T> listener);
        void RemoveDBEventListener(string type, ListenerDelegate<T> listener);
        /**
         * @private
         */
        //void DispatchEvent(string type, T eventObject);
        /**
         * 是否包含指定类型的事件。
         * @param type 事件类型。
         * @version DragonBones 4.5
         * @language zh_CN
         */
        //bool HasEventListener(string type);
        /**
         * 添加事件。
         * @param type 事件类型。
         * @param listener 事件回调。
         * @version DragonBones 4.5
         * @language zh_CN
         */
        //void AddEventListener(string type, ListenerDelegate<T> listener);
        /**
         * 移除事件。
         * @param type 事件类型。
         * @param listener 事件回调。
         * @version DragonBones 4.5
         * @language zh_CN
         */
        //void RemoveEventListener(string type, ListenerDelegate<T> listener);
    }
}
