using System;
using System.Collections.Generic;

namespace DragonBones
{
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
            this.a = value.a;
            this.b = value.b;
            this.c = value.c;
            this.d = value.d;
            this.tx = value.tx;
            this.ty = value.ty;

            return this;
        }

        /**
         * @private
         */
        public Matrix CopyFromArray(List<float> value, int offset = 0)
        {
            this.a = value[offset];
            this.b = value[offset + 1];
            this.c = value[offset + 2];
            this.d = value[offset + 3];
            this.tx = value[offset + 4];
            this.ty = value[offset + 5];

            return this;
        }

    /**
     * @language zh_CN
     * 转换为恒等矩阵。
     * @version DragonBones 3.0
     */
    public Matrix Identity()
        {
            this.a = this.d = 1.0f;
            this.b = this.c = 0.0f;
            this.tx = this.ty = 0.0f;

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
            var aA = this.a * value.a;
            var bA = 0.0f;
            var cA = 0.0f;
            var dA = this.d * value.d;
            var txA = this.tx * value.a + value.tx;
            var tyA = this.ty * value.d + value.ty;

            if (this.b != 0.0f || this.c != 0.0f)
            {
                aA += this.b * value.c;
                dA += this.c * value.b;
                bA += this.b * value.d;
                cA += this.c * value.a;
            }

            if (value.b != 0.0f || value.c != 0.0f)
            {
                bA += this.a * value.b;
                cA += this.d * value.c;
                txA += this.ty * value.c;
                tyA += this.tx * value.b;
            }

            this.a = aA;
            this.b = bA;
            this.c = cA;
            this.d = dA;
            this.tx = txA;
            this.ty = tyA;

            return this;
        }
        /**
         * @language zh_CN
         * 转换为逆矩阵。
         * @version DragonBones 3.0
         */
        public Matrix Invert()
        {
            var aA = this.a;
            var bA = this.b;
            var cA = this.c;
            var dA = this.d;
            var txA = this.tx;
            var tyA = this.ty;

            if (bA == 0.0f && cA == 0.0f)
            {
                this.b = this.c = 0.0f;
                if (aA == 0.0f || dA == 0.0f)
                {
                    this.a = this.b = this.tx = this.ty = 0.0f;
                }
                else
                {
                    aA = this.a = 1.0f / aA;
                    dA = this.d = 1.0f / dA;
                    this.tx = -aA * txA;
                    this.ty = -dA * tyA;
                }

                return this;
            }

            var determinant = aA * dA - bA * cA;
            if (determinant == 0.0f)
            {
                this.a = this.d = 1.0f;
                this.b = this.c = 0.0f;
                this.tx = this.ty = 0.0f;

                return this;
            }

            determinant = 1.0f / determinant;
            var k = this.a = dA * determinant;
            bA = this.b = -bA * determinant;
            cA = this.c = -cA * determinant;
            dA = this.d = aA * determinant;
            this.tx = -(k * txA + cA * tyA);
            this.ty = -(bA * txA + dA * tyA);

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
            result.x = this.a * x + this.c * y;
            result.y = this.b * x + this.d * y;

            if (!delta)
            {
                result.x += this.tx;
                result.y += this.ty;
            }
        }

        public void TransformRectangle(Rectangle rectangle, bool delta = false)
        {
            var a = this.a;
            var b = this.b;
            var c = this.c;
            var d = this.d;
            var tx = delta ? 0.0f : this.tx;
            var ty = delta ? 0.0f : this.ty;

            var x = rectangle.x;
            var y = rectangle.y;
            var xMax = x + rectangle.width;
            var yMax = y + rectangle.height;

            var x0 = a * x + c * y + tx;
            var y0 = b * x + d * y + ty;
            var x1 = a * xMax + c * y + tx;
            var y1 = b * xMax + d * y + ty;
            var x2 = a * xMax + c * yMax + tx;
            var y2 = b * xMax + d * yMax + ty;
            var x3 = a * x + c * yMax + tx;
            var y3 = b * x + d * yMax + ty;

            var tmp = 0.0f;

            if (x0 > x1)
            {
                tmp = x0;
                x0 = x1;
                x1 = tmp;
            }
            if (x2 > x3)
            {
                tmp = x2;
                x2 = x3;
                x3 = tmp;
            }
            
            rectangle.x = (float)Math.Floor(x0 < x2 ? x0 : x2);
            rectangle.width = (float)Math.Ceiling((x1 > x3 ? x1 : x3) - rectangle.x);

            if (y0 > y1)
            {
                tmp = y0;
                y0 = y1;
                y1 = tmp;
            }
            if (y2 > y3)
            {
                tmp = y2;
                y2 = y3;
                y3 = tmp;
            }

            rectangle.y = (float)Math.Floor(y0 < y2 ? y0 : y2);
            rectangle.height = (float)Math.Ceiling((y1 > y3 ? y1 : y3) - rectangle.y);
        }
    }
}
