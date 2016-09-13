using System;
using System.Collections.Generic;

namespace dragonBones
{
    /**
     * @private
     */
    public interface IEventDispatcher
    {
        void _onClear();
        bool _hasEvent(string type);
        void _dispatchEvent(EventObject eventObject);
    }

    public class EventObject : BaseObject
    {
    }
}