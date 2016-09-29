using System.Collections.Generic;

namespace dragonBones
{
    abstract public class BaseObject
    {
        private static uint _defaultMaxCount = 5000;
        private static Dictionary<System.Type, uint> _maxCountMap = new Dictionary<System.Type, uint>();
        private static Dictionary<System.Type, List<BaseObject>> _poolsMap = new Dictionary<System.Type, List<BaseObject>>();
        
        private static void _returnObject(BaseObject obj)
        {
            var classType = obj.GetType();
            var maxCount = _maxCountMap.ContainsKey(classType) ? _defaultMaxCount : _maxCountMap[classType];
            var pool = _poolsMap.ContainsKey(classType) ? _poolsMap[classType] : _poolsMap[classType] = new List<BaseObject>();

            if (pool.Count < maxCount)
            {
                if (!pool.Contains(obj))
                {
                    pool.Add(obj);
                }
                else
                {
                    DragonBones.warn("");
                }
            }
        }

        /**
         * @language zh_CN
         * 设置每种对象池的最大缓存数量。
         * @param classType 对象类型。
         * @param maxCount 最大缓存数量。 (设置为 0 则不缓存)
         * @version DragonBones 4.5
         */
        public static void setMaxCount(System.Type classType, uint maxCount)
        {
            if (classType != null)
            {
                _maxCountMap[classType] = maxCount;
                if (_poolsMap.ContainsKey(classType))
                {
                    var pool = _poolsMap[classType];
                    if (pool.Count > maxCount)
                    {
                        //pool.Count = maxCount;
                    }
                }
            }
            else
            {
                _defaultMaxCount = maxCount;
                foreach (var pair in _poolsMap)
                {
                    if (!_maxCountMap.ContainsKey(pair.Key))
                    {
                        continue;
                    }

                    _maxCountMap[pair.Key] = maxCount;

                    var pool = _poolsMap[pair.Key];
                    if (pool.Count > maxCount)
                    {
                        //pool.Count = maxCount;
                    }
                }
            }
        }

        /**
         * @language zh_CN
         * 清除对象池缓存的对象。
         * @param objectConstructor 对象类型。 (不设置则清除所有缓存)
         * @version DragonBones 4.5
         */
        public static void clearPool(System.Type classType)
        {
            if (classType != null)
            {
                if (_poolsMap.ContainsKey(classType))
                {
                    var pool = _poolsMap[classType];
                    if (pool.Count > 0)
                    {
                        pool.Clear();
                    }
                }
            }
            else
            {
                foreach (var pair in _poolsMap)
                {
                    var pool = _poolsMap[pair.Key];
                    if (pool.Count > 0)
                    {
                        pool.Clear();
                    }
                }
            }
        }

        /**
         * @language zh_CN
         * 从对象池中创建指定对象。
         * @param objectConstructor 对象类。
         * @version DragonBones 4.5
         */
        public static T borrowObject<T>() where T : BaseObject, new()
        {
            var type = typeof(T);
            var pool = _poolsMap.ContainsKey(type) ? _poolsMap[type] : null;
            if (pool != null && pool.Count > 0)
            {
                var index = pool.Count - 1;
                var obj = pool[index];
                pool.RemoveAt(index);
                return (T)obj;
            }
            else
            {
                var obj = new T();
                obj._onClear();
                return obj;
            }
        }

        protected BaseObject()
        {
        }

        /**
         * @private
         */
        abstract protected void _onClear();

        /**
         * @language zh_CN
         * 清除数据并返还对象池。
         * @version DragonBones 4.5
         */
        public void returnToPool()
        {
            _onClear();
            _returnObject(this);
        }
    }
}

