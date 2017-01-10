using System;

namespace DragonBones
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

        public void CopyFrom(ColorTransform value)
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

        public void Identity()
        {
            alphaMultiplier = redMultiplier = greenMultiplier = blueMultiplier = 1.0f;
            alphaOffset = redOffset = greenOffset = blueOffset = 0;
        }
    }
    
    public class Point
    {
        public float x = 0.0f;
        public float y = 0.0f;

        public Point()
        {
        }

        public void CopyFrom(Point value)
        {
            x = value.x;
            y = value.y;
        }

        public void Clear()
        {
            x = y = 0.0f;
        }
    }
    
    public class Rectangle
    {
        public float x;
        public float y;
        public float width;
        public float height;

        public Rectangle()
        {
        }

        public void CopyFrom(Rectangle value)
        {
            x = value.x;
            y = value.y;
            width = value.width;
            height = value.height;
        }

        public void Clear()
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
        public static float NormalizeRadian(float value)
        {
            value = (value + DragonBones.PI) % (DragonBones.PI * 2);
            value += value > 0.0f ? -DragonBones.PI : DragonBones.PI;

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
        override public string ToString()
        {
            return "[object DragonBones.Transform] x:" + x + " y:" + y + " skewX:" + skewX + " skewY:" + skewY + " scaleX:" + scaleX + " scaleY:" + scaleY;
        }
        /**
         * @private
         */
        public Transform CopyFrom(Transform value)
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
        public Transform Identity()
        {
            x = y = skewX = skewY = 0.0f;
            scaleX = scaleY = 1.0f;

            return this;
        }
        /**
         * @private
         */
        public Transform Add(Transform value)
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
        public Transform Minus(Transform value)
        {
            x -= value.x;
            y -= value.y;
            skewX = NormalizeRadian(skewX - value.skewX);
            skewY = NormalizeRadian(skewY - value.skewY);
            scaleX /= value.scaleX;
            scaleY /= value.scaleY;

            return this;
        }
        /**
         * @private
         */
        public Transform FromMatrix(Matrix matrix)
        {
            var backupScaleX = scaleX;
            var backupScaleY = scaleY;

            x = matrix.tx;
            y = matrix.ty;

            skewX = (float)Math.Atan(-matrix.c / matrix.d);
            skewY = (float)Math.Atan(matrix.b / matrix.a);

            if (float.IsNaN(skewX))
            {
                skewX = 0.0f;
            }

            if (float.IsNaN(skewY))
            {
                skewY = 0.0f;
            }

            scaleY = (float)((skewX > -DragonBones.PI_Q && skewX < DragonBones.PI_Q) ? matrix.d / Math.Cos(skewX) : -matrix.c / Math.Sin(skewX));
            scaleX = (float)((skewY > -DragonBones.PI_Q && skewY < DragonBones.PI_Q) ? matrix.a / Math.Cos(skewY) : matrix.b / Math.Sin(skewY));

            if (backupScaleX >= 0.0f && scaleX < 0.0f)
            {
                scaleX = -scaleX;
                skewY = skewY - DragonBones.PI;
            }

            if (backupScaleY >= 0.0f && scaleY < 0.0f)
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
        public Transform ToMatrix(Matrix matrix)
        {
            if (skewX != 0.0f || skewY != 0.0f)
            {
                matrix.a = (float)Math.Cos(skewY);
                matrix.b = (float)Math.Sin(skewY);

                if (skewX == skewY)
                {
                    matrix.c = -matrix.b;
                    matrix.d = matrix.a;
                }
                else
                {
                    matrix.c = -(float)Math.Sin(skewX);
                    matrix.d = (float)Math.Cos(skewX);
                }

                if (scaleX != 1.0f || scaleY != 1.0f)
                {
                    matrix.a *= scaleX;
                    matrix.b *= scaleX;
                    matrix.c *= scaleY;
                    matrix.d *= scaleY;
                }
            }
            else
            {
                matrix.a = scaleX;
                matrix.b = 0.0f;
                matrix.c = 0.0f;
                matrix.d = scaleY;
            }

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
            get { return skewY; }

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
         * @private
         */
        override public string ToString()
        {
            return "[object DragonBones.Matrix] a:" + a + " b:" + b + " c:" + c + " d:" + d + " tx:" + tx + " ty:" + ty;
        }
        /**
         * @language zh_CN
         * 复制矩阵。
         * @param value 需要复制的矩阵。
         * @version DragonBones 3.0
         */
        public Matrix CopyFrom(Matrix value)
        {
            a = value.a;
            b = value.b;
            c = value.c;
            d = value.d;
            tx = value.tx;
            ty = value.ty;

            return this;
        }
        /**
         * @language zh_CN
         * 转换为恒等矩阵。
         * @version DragonBones 3.0
         */
        public Matrix Identity()
        {
            a = d = 1.0f;
            b = c = 0.0f;
            tx = ty = 0.0f;

            return this;
        }
        /**
         * @language zh_CN
         * 将当前矩阵与另一个矩阵相乘。
         * @param value 需要相乘的矩阵。
         * @version DragonBones 3.0
         */
        public Matrix Concat(Matrix value)
        {
            var aA = a * value.a;
            var bA = 0.0f;
            var cA = 0.0f;
            var dA = d * value.d;
            var txA = tx * value.a + value.tx;
            var tyA = ty * value.d + value.ty;

            if (b != 0.0f || c != 0.0f)
            {
                aA += b * value.c;
                dA += c * value.b;
                bA += b * value.d;
                cA += c * value.a;
            }

            if (value.b != 0.0f || value.c != 0.0f)
            {
                bA += a * value.b;
                cA += d * value.c;
                txA += ty * value.c;
                tyA += tx * value.b;
            }

            a = aA;
            b = bA;
            c = cA;
            d = dA;
            tx = txA;
            ty = tyA;

            return this;
        }
        /**
         * @language zh_CN
         * 转换为逆矩阵。
         * @version DragonBones 3.0
         */
        public Matrix Invert()
        {
            var aA = a;
            var bA = b;
            var cA = c;
            var dA = d;
            var txA = tx;
            var tyA = ty;

            if (bA == 0.0f && cA == 0.0f)
            {
                b = c = 0.0f;
                if (aA == 0.0f || dA == 0.0f)
                {
                    a = b = tx = ty = 0.0f;
                }
                else
                {
                    aA = a = 1.0f / aA;
                    dA = d = 1.0f / dA;
                    tx = -aA * txA;
                    ty = -dA * tyA;
                }

                return this;
            }

            var determinant = aA * dA - bA * cA;
            if (determinant == 0.0f)
            {
                a = d = 1.0f;
                b = c = 0.0f;
                tx = ty = 0.0f;

                return this;
            }

            determinant = 1.0f / determinant;
            var k = a = dA * determinant;
            bA = b = -bA * determinant;
            cA = c = -cA * determinant;
            dA = d = aA * determinant;
            tx = -(k * txA + cA * tyA);
            ty = -(bA * txA + dA * tyA);

            return this;
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
        public void TransformPoint(float x, float y, Point result, bool delta = false)
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