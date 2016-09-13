using System.Collections.Generic;

namespace dragonBones
{
    public interface IAnimateble
    {
        /**
        * @language zh_CN
        * 更新一个指定的时间。
        * @param passedTime 前进的时间。 (以秒为单位)
        * @version DragonBones 3.0
        */
        void advanceTime(float passedTime);
    }

    public class Animation : BaseObject
    {
    }
}
