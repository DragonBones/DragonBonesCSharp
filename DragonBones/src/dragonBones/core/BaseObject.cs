using System.Collections.Generic;

namespace dragonBones
{
    public abstract class BaseObject
    {
        private static uint _defaultMaxCount = 5000;
        private static Dictionary<System.Type, uint> _maxCountMap = new Dictionary<System.Type, uint>();
        private static Dictionary<System.Type, List<BaseObject>> _poolsMap = new Dictionary<System.Type, List<BaseObject>>();
        
        private static void _returnObject(BaseObject obj)
        {
            var classType = obj.GetType();
            var maxCount = BaseObject._maxCountMap.ContainsKey(classType) ? BaseObject._defaultMaxCount : BaseObject._maxCountMap[classType];
            var pool = BaseObject._poolsMap.ContainsKey(classType) ? BaseObject._poolsMap[classType] : BaseObject._poolsMap[classType] = new List<BaseObject>();

            if (pool.Count < maxCount)
            {
                if (!pool.Contains(obj))
                {
                    pool.Add(obj);
                }
                else
                {
                    DragonBones.assert("");
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
                BaseObject._maxCountMap[classType] = maxCount;
                if (BaseObject._poolsMap.ContainsKey(classType))
                {
                    var pool = BaseObject._poolsMap[classType];
                    if (pool.Count > maxCount)
                    {
                        //pool.Count = maxCount;
                    }
                }
            }
            else
            {
                BaseObject._defaultMaxCount = maxCount;
                foreach (var pair in BaseObject._poolsMap)
                {
                    if (!BaseObject._maxCountMap.ContainsKey(pair.Key))
                    {
                        continue;
                    }

                    BaseObject._maxCountMap[pair.Key] = maxCount;

                    var pool = BaseObject._poolsMap[pair.Key];
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
                if (BaseObject._poolsMap.ContainsKey(classType))
                {
                    var pool = BaseObject._poolsMap[classType];
                    if (pool.Count > 0)
                    {
                        pool.Clear();
                    }
                }
            }
            else
            {
                foreach (var pair in BaseObject._poolsMap)
                {
                    var pool = BaseObject._poolsMap[pair.Key];
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
            //new T();
            return null;
        }

        protected BaseObject()
        {
        }

        /**
         * @private
         */
        protected abstract void _onClear();

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

