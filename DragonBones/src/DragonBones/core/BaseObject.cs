using System.Collections.Generic;

namespace DragonBones
{
    /**
     * 基础对象。
     * @version DragonBones 4.5
     * @language zh_CN
     */
    abstract public class BaseObject
    {
        private static uint _hashCode = 0;
        private static uint _defaultMaxCount = 1000;
        private static readonly Dictionary<System.Type, uint> _maxCountMap = new Dictionary<System.Type, uint>();
        private static readonly Dictionary<System.Type, List<BaseObject>> _poolsMap = new Dictionary<System.Type, List<BaseObject>>();

        private static void _ReturnObject(BaseObject obj)
        {
            var classType = obj.GetType();
            var maxCount = _maxCountMap.ContainsKey(classType) ? _maxCountMap[classType] : _defaultMaxCount;
            var pool = _poolsMap.ContainsKey(classType) ? _poolsMap[classType] : _poolsMap[classType] = new List<BaseObject>();

            if (pool.Count < maxCount)
            {
                if (!pool.Contains(obj))
                {
                    pool.Add(obj);
                }
                else
                {
                    DragonBones.Assert(false, "The object is already in the pool.");
                }
            }
            else
            {

            }
        }

        /**
         * 设置每种对象池的最大缓存数量。
         * @param objectConstructor 对象类。
         * @param maxCount 最大缓存数量。 (设置为 0 则不缓存)
         * @version DragonBones 4.5
         * @language zh_CN
         */
        public static void SetMaxCount(System.Type classType, uint maxCount)
        {
            if (classType != null)
            {
                if (_poolsMap.ContainsKey(classType))
                {
                    var pool = _poolsMap[classType];
                    if (pool.Count > maxCount)
                    {
                        pool.ResizeList((int)maxCount, null);
                    }
                }

                _maxCountMap[classType] = maxCount;
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

                    var pool = _poolsMap[pair.Key];
                    if (pool.Count > maxCount)
                    {
                        pool.ResizeList((int)maxCount, null);
                    }

                    _maxCountMap[pair.Key] = maxCount;
                }
            }
        }

        /**
         * 清除对象池缓存的对象。
         * @param objectConstructor 对象类。 (不设置则清除所有缓存)
         * @version DragonBones 4.5
         * @language zh_CN
         */
        public static void ClearPool(System.Type classType)
        {
            if (classType != null)
            {
                if (_poolsMap.ContainsKey(classType))
                {
                    var pool = _poolsMap[classType];
                    if (pool != null)
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
                    if (pool != null)
                    {
                        pool.Clear();
                    }
                }
            }
        }
        /**
         * 从对象池中创建指定对象。
         * @param objectConstructor 对象类。
         * @version DragonBones 4.5
         * @language zh_CN
         */
        public static T BorrowObject<T>() where T : BaseObject, new()
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
                obj._OnClear();
                return obj;
            }
        }
        /**
         * 对象的唯一标识。
         * @version DragonBones 4.5
         * @language zh_CN
         */
        public readonly uint hashCode = _hashCode++;

        protected BaseObject()
        {
        }

        /**
         * @private
         */
        abstract protected void _OnClear();
        /**
         * @language zh_CN
         * 清除数据并返还对象池。
         * @version DragonBones 4.5
         */
        public void ReturnToPool()
        {
            _OnClear();
            _ReturnObject(this);
        }
    }
}