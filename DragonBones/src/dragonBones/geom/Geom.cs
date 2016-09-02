using System;

namespace dragonBones
{
    /**
     * @private
     */
    public class ColorTransform
    {
        public float alphaMultiplier = 1.0f;
        public float redMultiplier = 1.0f;
        public float greenMultiplier = 1.0f;
        public float blueMultiplier = 1.0f;
        public int alphaOffset = 0;
        public int redOffset = 0;
        public int greenOffset = 0;
        public int blueOffset = 0;

        public ColorTransform()
        {
        }

        public void copyFrom(ColorTransform value)
        {
            alphaMultiplier = value.alphaMultiplier;
            redMultiplier = value.redMultiplier;
            greenMultiplier = value.greenMultiplier;
            blueMultiplier = value.blueMultiplier;
            alphaOffset = value.alphaOffset;
            redOffset = value.redOffset;
            redOffset = value.redOffset;
            greenOffset = value.blueOffset;
        }

        public void identity()
        {
            alphaMultiplier = redMultiplier = greenMultiplier = blueMultiplier = 1.0f;
            alphaOffset = redOffset = greenOffset = blueOffset = 0;
        }
    }

    /**
     * @private
     */
    public class Point
    {
        public float x = 0.0f;
        public float y = 0.0f;

        public Point()
        {
        }

        public void copyFrom(Point value)
        {
            x = value.x;
            y = value.y;
        }

        public void clear()
        {
            x = y = 0.0f;
        }
    }

    /**
     * @private
     */
    public class Rectangle
    {
        public float x;
        public float y;
        public float width;
        public float height;

        public Rectangle()
        {
        }

        public void copyFrom(Rectangle value)
        {
            x = value.x;
            y = value.y;
            width = value.width;
            height = value.height;
        }

        public void clear()
        {
            x = y = 0.0f;
            width = height = 0.0f;
        }
    }

    /**
     * @language zh_CN
     * 2D 变换。
     * @version DragonBones 3.0
     */
    public class Transform
    {
        /**
         * @private
         */
        public static float normalizeRadian(float value)
        {
            value = (value + DragonBones.PI) % (DragonBones.PI * 2);
            value += value > 0 ? -DragonBones.PI : DragonBones.PI;

            return value;
        }

        /**
         * @language zh_CN
         * 水平位移。
         * @version DragonBones 3.0
         */
        public float x = 0.0f;

        /**
         * @language zh_CN
         * 垂直位移。
         * @version DragonBones 3.0
         */
        public float y = 0.0f;

        /**
         * @language zh_CN
         * 水平倾斜。 (以弧度为单位)
         * @version DragonBones 3.0
         */
        public float skewX = 0.0f;

        /**
         * @language zh_CN
         * 垂直倾斜。 (以弧度为单位)
         * @version DragonBones 3.0
         */
        public float skewY = 0.0f;

        /**
         * @language zh_CN
         * 水平缩放。
         * @version DragonBones 3.0
         */
        public float scaleX = 1.0f;

        /**
         * @language zh_CN
         * 垂直缩放。
         * @version DragonBones 3.0
         */
        public float scaleY = 1.0f;

        /**
         * @private
         */
        public Transform()
        {
        }

        /**
         * @private
         */
        public Transform copyFrom(Transform value)
        {
            x = value.x;
            y = value.y;
            skewX = value.skewX;
            skewY = value.skewY;
            scaleX = value.scaleX;
            scaleY = value.scaleY;

            return this;
        }

        /**
         * @private
         */
        public Transform clone()
        {
            var value = new Transform();
            value.copyFrom(this);

            return value;
        }

        /**
         * @private
         */
        public Transform identity()
        {
            x = y = skewX = skewY = 0.0f;
            scaleX = scaleY = 1.0f;

            return this;
        }

        /**
         * @private
         */
        public Transform add(Transform value)
        {
            x += value.x;
            y += value.y;
            skewX += value.skewX;
            skewY += value.skewY;
            scaleX *= value.scaleX;
            scaleY *= value.scaleY;

            return this;
        }

        /**
         * @private
         */
        public Transform minus(Transform value)
        {
            x -= value.x;
            y -= value.y;
            skewX = normalizeRadian(skewX - value.skewX);
            skewY = normalizeRadian(skewY - value.skewY);
            scaleX /= value.scaleX;
            scaleY /= value.scaleY;

            return this;
        }

        /**
         * @private
         */
        public Transform fromMatrix(Matrix matrix)
        {
            var backupScaleX = scaleX;
            var backupScaleY = scaleY;

            x = matrix.tx;
            y = matrix.ty;

            skewX = (float)Math.Atan(-matrix.c / matrix.d);
            skewY = (float)Math.Atan(matrix.b / matrix.a);
            if (skewX != skewX) skewX = 0;
            if (skewY != skewY) skewY = 0;

            scaleY = (float)((skewX > -DragonBones.PI_Q && skewX < DragonBones.PI_Q) ? matrix.d / Math.Cos(skewX) : -matrix.c / Math.Sin(skewX));
            scaleX = (float)((skewY > -DragonBones.PI_Q && skewY < DragonBones.PI_Q) ? matrix.a / Math.Cos(skewY) : matrix.b / Math.Sin(skewY));

            if (backupScaleX >= 0 && scaleX < 0)
            {
                scaleX = -scaleX;
                skewY = skewY - DragonBones.PI;
            }

            if (backupScaleY >= 0 && scaleY < 0)
            {
                scaleY = -scaleY;
                skewX = skewX - DragonBones.PI;
            }

            return this;
        }

        /**
         * @language zh_CN
         * 转换为矩阵。
         * @param 矩阵。
         * @version DragonBones 3.0
         */
        public Transform toMatrix(Matrix matrix)
        {
            matrix.a = scaleX * (float)Math.Cos(skewY);
            matrix.b = scaleX * (float)Math.Sin(skewY);
            matrix.c = -scaleY * (float)Math.Sin(skewX);
            matrix.d = scaleY * (float)Math.Cos(skewX);
            matrix.tx = x;
            matrix.ty = y;

            return this;
        }

        /**
         * @language zh_CN
         * 旋转。 (以弧度为单位)
         * @version DragonBones 3.0
         */
        public float rotation
        {
            get
            {
                return skewY;
            }

            set
            {
                var dValue = value - skewY;
                skewX += dValue;
                skewY += dValue;
            }
        }
    }

    /**
     * @language zh_CN
     * 2D 矩阵。
     * @version DragonBones 3.0
     */
    public class Matrix
    {
        public float a = 1.0f;
        public float b = 0.0f;
        public float c = 0.0f;
        public float d = 1.0f;
        public float tx = 0.0f;
        public float ty = 0.0f;

        public Matrix()
        {
        }

        /**
         * @language zh_CN
         * 复制矩阵。
         * @param value 需要复制的矩阵。
         * @version DragonBones 3.0
         */
        public void copyFrom(Matrix value)
        {
            a = value.a;
            b = value.b;
            c = value.c;
            d = value.d;
            tx = value.tx;
            ty = value.ty;
        }

        /**
         * @language zh_CN
         * 转换为恒等矩阵。
         * @version DragonBones 3.0
         */
        public void identity()
        {
            a = d = 1.0f;
            b = c = 0.0f;
            tx = ty = 0.0f;
        }

        /**
         * @language zh_CN
         * 将当前矩阵与另一个矩阵相乘。
         * @param value 需要相乘的矩阵。
         * @version DragonBones 3.0
         */
        public void concat(Matrix value)
        {
            var aA = a;
            var bA = b;
            var cA = c;
            var dA = d;
            var txA = tx;
            var tyA = ty;
            var aB = value.a;
            var bB = value.b;
            var cB = value.c;
            var dB = value.d;
            var txB = value.tx;
            var tyB = value.ty;

            a = aA * aB + bA * cB;
            b = aA * bB + bA * dB;
            c = cA * aB + dA * cB;
            d = cA * bB + dA * dB;
            tx = aB * txA + cB * tyA + txB;
            ty = dB * tyA + bB * txA + tyB;
        }

        /**
         * @language zh_CN
         * 转换为逆矩阵。
         * @version DragonBones 3.0
         */
        public void invert()
        {
            var aA = a;
            var bA = b;
            var cA = c;
            var dA = d;
            var txA = tx;
            var tyA = ty;
            var n = aA * dA - bA * cA;

            a = dA / n;
            b = -bA / n;
            c = -cA / n;
            d = aA / n;
            tx = (cA * tyA - dA * txA) / n;
            ty = -(aA * tyA - bA * txA) / n;
        }
        /**
         * @language zh_CN
         * 将矩阵转换应用于指定点。
         * @param x 横坐标。
         * @param y 纵坐标。
         * @param result 应用转换之后的坐标。
         * @params delta 是否忽略 tx，ty 对坐标的转换。
         * @version DragonBones 3.0
         */
        public void transformPoint(float x, float y, Point result, bool delta = false)
        {
            result.x = a * x + c * y;
            result.y = b * x + d * y;

            if (!delta)
            {
                result.x += tx;
                result.y += ty;
            }
        }
    }
}