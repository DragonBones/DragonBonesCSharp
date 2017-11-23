//using System;

//namespace DragonBones
//{
//    /**
//     * Cohen–Sutherland algorithm https://en.wikipedia.org/wiki/Cohen%E2%80%93Sutherland_algorithm
//     * ----------------------
//     * | 0101 | 0100 | 0110 |
//     * ----------------------
//     * | 0001 | 0000 | 0010 |
//     * ----------------------
//     * | 1001 | 1000 | 1010 |
//     * ----------------------
//     */
//    enum OutCode
//    {
//        InSide = 0, // 0000
//        Left = 1,   // 0001
//        Right = 2,  // 0010
//        Top = 4,    // 0100
//        Bottom = 8  // 1000
//    }

//    public class RectangleBoundingBoxData : BoundingBoxData
//    {
//        /**
//         * Compute the bit code for a point (x, y) using the clip rectangle
//         */
//        private static int _ComputeOutCode(float x, float y, float xMin, float yMin, float xMax, float yMax)
//        {
//            var code = OutCode.InSide;  // initialised as being inside of [[clip window]]

//            if (x < xMin)
//            {             // to the left of clip window
//                code |= OutCode.Left;
//            }
//            else if (x > xMax)
//            {        // to the right of clip window
//                code |= OutCode.Right;
//            }

//            if (y < yMin)
//            {             // below the clip window
//                code |= OutCode.Top;
//            }
//            else if (y > yMax)
//            {        // above the clip window
//                code |= OutCode.Bottom;
//            }

//            return (int)code;
//        }
//        /**
//         * @private
//         */
//        public static int RectangleIntersectsSegment(float xA, float yA, float xB, float yB,
//                                                        float xMin, float yMin, float xMax, float yMax,
//                                                        Point intersectionPointA = null,
//                                                        Point intersectionPointB = null,
//                                                        Point normalRadians = null)
//        {
//            var inSideA = xA > xMin && xA < xMax && yA > yMin && yA < yMax;
//            var inSideB = xB > xMin && xB < xMax && yB > yMin && yB < yMax;

//            if (inSideA && inSideB)
//            {
//                return -1;
//            }

//            var intersectionCount = 0;
//            var outcode0 = RectangleBoundingBoxData._ComputeOutCode(xA, yA, xMin, yMin, xMax, yMax);
//            var outcode1 = RectangleBoundingBoxData._ComputeOutCode(xB, yB, xMin, yMin, xMax, yMax);

//            while (true)
//            {
//                if ((outcode0 | outcode1) == 0)
//                { // Bitwise OR is 0. Trivially accept and get out of loop
//                    intersectionCount = 2;
//                    break;
//                }
//                else if ((outcode0 & outcode1) != 0)
//                { // Bitwise AND is not 0. Trivially reject and get out of loop
//                    break;
//                }

//                // failed both tests, so calculate the line segment to clip
//                // from an outside point to an intersection with clip edge
//                var x = 0.0f;
//                var y = 0.0f;
//                var normalRadian = 0.0f;

//                // At least one endpoint is outside the clip rectangle; pick it.
//                var outcodeOut = outcode0 != 0 ? outcode0 : outcode1;

//                // Now find the intersection point;
//                if ((outcodeOut & (int)OutCode.Top) != 0)
//                {             // point is above the clip rectangle
//                    x = xA + (xB - xA) * (yMin - yA) / (yB - yA);
//                    y = yMin;

//                    if (normalRadians != null)
//                    {
//                        normalRadian = -(float)Math.PI * 0.5f;
//                    }
//                }
//                else if ((outcodeOut & (int)OutCode.Bottom) != 0)
//                {     // point is below the clip rectangle
//                    x = xA + (xB - xA) * (yMax - yA) / (yB - yA);
//                    y = yMax;

//                    if (normalRadians != null)
//                    {
//                        normalRadian = (float)Math.PI * 0.5f;
//                    }
//                }
//                else if ((outcodeOut & (int)OutCode.Right) != 0)
//                {      // point is to the right of clip rectangle
//                    y = yA + (yB - yA) * (xMax - xA) / (xB - xA);
//                    x = xMax;

//                    if (normalRadians != null)
//                    {
//                        normalRadian = 0;
//                    }
//                }
//                else if ((outcodeOut & (int)OutCode.Left) != 0)
//                {       // point is to the left of clip rectangle
//                    y = yA + (yB - yA) * (xMin - xA) / (xB - xA);
//                    x = xMin;

//                    if (normalRadians != null)
//                    {
//                        normalRadian = (float)Math.PI;
//                    }
//                }

//                // Now we move outside point to intersection point to clip
//                // and get ready for next pass.
//                if (outcodeOut == outcode0)
//                {
//                    xA = x;
//                    yA = y;
//                    outcode0 = RectangleBoundingBoxData._ComputeOutCode(xA, yA, xMin, yMin, xMax, yMax);

//                    if (normalRadians != null)
//                    {
//                        normalRadians.x = normalRadian;
//                    }
//                }
//                else
//                {
//                    xB = x;
//                    yB = y;
//                    outcode1 = RectangleBoundingBoxData._ComputeOutCode(xB, yB, xMin, yMin, xMax, yMax);

//                    if (normalRadians != null)
//                    {
//                        normalRadians.y = normalRadian;
//                    }
//                }
//            }

//            if (intersectionCount > 0)
//            {
//                if (inSideA)
//                {
//                    intersectionCount = 2; // 10

//                    if (intersectionPointA != null)
//                    {
//                        intersectionPointA.x = xB;
//                        intersectionPointA.y = yB;
//                    }

//                    if (intersectionPointB != null)
//                    {
//                        intersectionPointB.x = xB;
//                        intersectionPointB.y = xB;
//                    }

//                    if (normalRadians != null)
//                    {
//                        normalRadians.x = normalRadians.y + (float)Math.PI;
//                    }
//                }
//                else if (inSideB)
//                {
//                    intersectionCount = 1; // 01

//                    if (intersectionPointA != null)
//                    {
//                        intersectionPointA.x = xA;
//                        intersectionPointA.y = yA;
//                    }

//                    if (intersectionPointB != null)
//                    {
//                        intersectionPointB.x = xA;
//                        intersectionPointB.y = yA;
//                    }

//                    if (normalRadians != null)
//                    {
//                        normalRadians.y = normalRadians.x + (float)Math.PI;
//                    }
//                }
//                else
//                {
//                    intersectionCount = 3; // 11
//                    if (intersectionPointA != null)
//                    {
//                        intersectionPointA.x = xA;
//                        intersectionPointA.y = yA;
//                    }

//                    if (intersectionPointB != null)
//                    {
//                        intersectionPointB.x = xB;
//                        intersectionPointB.y = yB;
//                    }
//                }
//            }

//            return intersectionCount;
//        }
//        /**
//         * @private
//         */
//        protected override void _OnClear()
//        {
//            base._OnClear();

//            this.type = BoundingBoxType.Rectangle;
//        }

//        /**
//         * @inherDoc
//         */
//        public override bool ContainsPoint(float pX, float pY)
//        {
//            var widthH = this.width * 0.5f;
//            if (pX >= -widthH && pX <= widthH)
//            {
//                var heightH = this.height * 0.5f;
//                if (pY >= -heightH && pY <= heightH)
//                {
//                    return true;
//                }
//            }

//            return false;
//        }

//        public override int IntersectsSegment(float xA, float yA, float xB, float yB,
//                                             Point intersectionPointA = null,
//                                             Point intersectionPointB = null,
//                                             Point normalRadians = null)
//        {
//            var widthH = this.width * 0.5f;
//            var heightH = this.height * 0.5f;
//            var intersectionCount = RectangleBoundingBoxData.RectangleIntersectsSegment
//            (
//                xA, yA, xB, yB,
//                -widthH, -heightH, widthH, heightH,
//                intersectionPointA, intersectionPointB, normalRadians
//            );

//            return intersectionCount;
//        }
//    }
//}
