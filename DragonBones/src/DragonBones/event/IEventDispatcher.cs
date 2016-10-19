namespace DragonBones
{
    /**
     * @private
     */
    public delegate void ListenerDelegate<T>(string type, T eventObject);
    
    public interface IEventDispatcher<T>
    {
        /**
         * @private
         */
        void _onClear();
        
        bool HasEventListener(string type);
        void AddEventListener(string type, ListenerDelegate<T> listener);
        void RemoveEventListener(string type, ListenerDelegate<T> listener);
        void DispatchEvent(string type, T eventObject);
    }
}
