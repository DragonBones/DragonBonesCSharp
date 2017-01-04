using System.Collections.Generic;
using UnityEngine;

namespace DragonBones
{
    /**
     * @inheritDoc
     */
    public class UnityEventDispatcher<T> : MonoBehaviour, IEventDispatcher<T>
    {
        private readonly Dictionary<string, ListenerDelegate<T>> _listeners = new Dictionary<string, ListenerDelegate<T>>();
        /**
         * @private
         */
        public UnityEventDispatcher()
        {
        }
        /**
         * @inheritDoc
         */
        public void DispatchEvent(string type, T eventObject)
        {
            if (!_listeners.ContainsKey(type))
            {
                return;
            }
            else
            {
                _listeners[type](type, eventObject);
            }
        }
        /**
         * @inheritDoc
         */
        public bool HasEventListener(string type)
        {
            return _listeners.ContainsKey(type);
        }
        /**
         * @inheritDoc
         */
        public void AddEventListener(string type, ListenerDelegate<T> listener)
        {
            if (_listeners.ContainsKey(type))
            {
                var delegates = _listeners[type].GetInvocationList();
                for (int i = 0, l = delegates.Length; i < l; ++i)
                {
                    if (listener == delegates[i] as ListenerDelegate<T>)
                    {
                        return;
                    }
                }

                _listeners[type] += listener;
            }
            else
            {
                _listeners.Add(type, listener);
            }
        }
        /**
         * @inheritDoc
         */
        public void RemoveEventListener(string type, ListenerDelegate<T> listener)
        {
            if (!_listeners.ContainsKey(type))
            {
                return;
            }

            var delegates = _listeners[type].GetInvocationList();
            for (int i = 0, l = delegates.Length; i < l; ++i)
            {
                if (listener == delegates[i] as ListenerDelegate<T>)
                {
                    _listeners[type] -= listener;
                    break;
                }
            }

            if (_listeners[type] == null)
            {
                _listeners.Remove(type);
            }
        }
    }
}
