using System.Collections.Generic;

namespace DragonBones
{
    /**
     * @language zh_CN
     * 骨架显示容器和事件的接口。
     * @see dragonBones.Armature#display
     * @version DragonBones 4.5
     */
    public interface IArmatureDisplay
    {
        /**
         * @private
         */
        void _onClear();

        /**
         * @language zh_CN
         * 获取使用这个显示容器的骨架。
         * @readOnly
         * @see dragonBones.Armature
         * @version DragonBones 4.5
         */
        Armature armature
        {
            get;
        }

        /**
         * @language zh_CN
         * 获取使用骨架的动画控制器。
         * @readOnly
         * @see dragonBones.Animation
         * @version DragonBones 4.5
         */
        Animation animation
        {
            get;
        }
    }
}
