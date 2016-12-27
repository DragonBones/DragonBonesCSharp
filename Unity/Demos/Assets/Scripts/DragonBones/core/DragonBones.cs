using System.Collections.Generic;
using System.Diagnostics;

namespace DragonBones
{
    /**
     * @private
     */
    public enum ArmatureType
    {
        None = -1,
        Armature = 0,
        MovieClip = 1,
        Stage = 2
    }

    /**
     * @private
     */
    public enum DisplayType
    {
        None = -1,
        Image = 0,
        Armature = 1,
        Mesh = 2,
        BoundingBox = 3
    }

    /**
     * @private
     */
    public enum BoundingBoxType
    {
        None = -1,
        Rectangle = 0,
        Ellipse = 1,
        Polygon = 2
    }

    /**
     * @private
     */
    public enum EventType
    {
        None = -1,
        Frame = 10,
        Sound = 11
    }

    /**
     * @private
     */
    public enum ActionType
    {
        None = -1,
        Play = 0,
        Stop = 1,
        GotoAndPlay = 2,
        GotoAndStop = 3,
        FadeIn = 4,
        FadeOut = 5
    }

    /**
     * @private
     */
    public enum BlendMode
    {
        None = -1,
        Normal = 0,
        Add = 1,
        Alpha = 2,
        Darken = 3,
        Difference = 4,
        Erase = 5,
        HardLight = 6,
        Invert = 7,
        Layer = 8,
        Lighten = 9,
        Multiply = 10,
        Overlay = 11,
        Screen = 12,
        Subtract = 13
    }

    public class DragonBones
    {
        /**
         * @private
         */
        public const float PI = 3.14159265358979323846f;

        /**
         * @private
         */
        public const float PI_D = PI * 2.0f;

        /**
         * @private
         */
        public const float PI_H = PI / 2.0f;

        /**
         * @private
         */
        public const float PI_Q = PI / 4.0f;

        /**
         * @private
         */
        public const float ANGLE_TO_RADIAN = PI / 180.0f;

        /**
         * @private
         */
        public const float RADIAN_TO_ANGLE = 180.0f / PI;

        /**
         * @private
         */
        public const float SECOND_TO_MILLISECOND = 1000.0f;

        /**
         * @private
         */
        public const float NO_TWEEN = 100.0f;

        public const string VSESION = "5.0.0";
        /**
         * @private
         */
        public const string ARGUMENT_ERROR = "Argument error.";

        /**
         * @private
         */
        public static void Assert(bool condition, string message)
        {
            Debug.Assert(condition, message);
        }

        /**
         * @private
         */
        public static void ResizeList<T>(List<T> list, int count, T value)
        {
            if (list.Count == count)
            {
                return;
            }

            if (list.Count > count)
            {
                list.RemoveRange(count, list.Count - count);
            }
            else
            {
                list.Capacity = count;
                for (int i = list.Count, l = count; i < l; ++i)
                {
                    list.Add(value);
                }
            }
        }
    }
}