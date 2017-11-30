//using System;

//namespace DragonBones
//{
//    /**
//     * 椭圆边界框。
//     * @version DragonBones 5.1
//     * @language zh_CN
//     */
//    public class EllipseBoundingBoxData : BoundingBoxData
//    {
//        public static int EllipseIntersectsSegment( float xA, float yA, float xB, float yB,
//                                                    float xC, float yC, float widthH, float heightH,
//                                                    Point intersectionPointA = null,
//                                                    Point intersectionPointB = null,
//                                                    Point normalRadians = null)
//        {
//            var d = widthH / heightH;
//            var dd = d * d;

//            yA *= d;
//            yB *= d;

//            var dX = xB - xA;
//            var dY = yB - yA;
//            var lAB = (float)Math.Sqrt(dX * dX + dY * dY);
//            var xD = dX / lAB;
//            var yD = dY / lAB;
//            var a = (xC - xA) * xD + (yC - yA) * yD;
//            var aa = a * a;
//            var ee = xA * xA + yA * yA;
//            var rr = widthH * widthH;
//            var dR = rr - ee + aa;
//            var intersectionCount = 0;

//            if (dR >= 0.0f)
//            {
//                var dT = (float)Math.Sqrt(dR);
//                var sA = a - dT;
//                var sB = a + dT;
//                var inSideA = sA < 0.0 ? -1 : (sA <= lAB ? 0 : 1);
//                var inSideB = sB < 0.0 ? -1 : (sB <= lAB ? 0 : 1);
//                var sideAB = inSideA * inSideB;

//                if (sideAB < 0)
//                {
//                    return -1;
//                }
//                else if (sideAB == 0)
//                {
//                    if (inSideA == -1)
//                    {
//                        intersectionCount = 2; // 10
//                        xB = xA + sB * xD;
//                        yB = (yA + sB * yD) / d;

//                        if (intersectionPointA != null)
//                        {
//                            intersectionPointA.x = xB;
//                            intersectionPointA.y = yB;
//                        }

//                        if (intersectionPointB != null)
//                        {
//                            intersectionPointB.x = xB;
//                            intersectionPointB.y = yB;
//                        }

//                        if (normalRadians != null)
//                        {
//                            normalRadians.x = (float)Math.Atan2(yB / rr * dd, xB / rr);
//                            normalRadians.y = normalRadians.x + (float)Math.PI;
//                        }
//                    }
//                    else if (inSideB == 1)
//                    {
//                        intersectionCount = 1; // 01
//                        xA = xA + sA * xD;
//                        yA = (yA + sA * yD) / d;

//                        if (intersectionPointA != null)
//                        {
//                            intersectionPointA.x = xA;
//                            intersectionPointA.y = yA;
//                        }

//                        if (intersectionPointB != null)
//                        {
//                            intersectionPointB.x = xA;
//                            intersectionPointB.y = yA;
//                        }

//                        if (normalRadians != null)
//                        {
//                            normalRadians.x = (float)Math.Atan2(yA / rr * dd, xA / rr);
//                            normalRadians.y = normalRadians.x + (float)Math.PI;
//                        }
//                    }
//                    else
//                    {
//                        intersectionCount = 3; // 11

//                        if (intersectionPointA != null)
//                        {
//                            intersectionPointA.x = xA + sA * xD;
//                            intersectionPointA.y = (yA + sA * yD) / d;

//                            if (normalRadians != null)
//                            {
//                                normalRadians.x = (float)Math.Atan2(intersectionPointA.y / rr * dd, intersectionPointA.x / rr);
//                            }
//                        }

//                        if (intersectionPointB != null)
//                        {
//                            intersectionPointB.x = xA + sB * xD;
//                            intersectionPointB.y = (yA + sB * yD) / d;

//                            if (normalRadians != null)
//                            {
//                                normalRadians.y = (float)Math.Atan2(intersectionPointB.y / rr * dd, intersectionPointB.x / rr);
//                            }
//                        }
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

//            this.type = BoundingBoxType.Ellipse;
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
//                    pY *= widthH / heightH;
//                    return Math.Sqrt(pX * pX + pY * pY) <= widthH;
//                }
//            }

//            return false;
//        }

//        public override int IntersectsSegment( float xA, float yA, float xB, float yB,
//                                                Point intersectionPointA,
//                                                Point intersectionPointB,
//                                                Point normalRadians)
//        {
//            var intersectionCount = EllipseBoundingBoxData.EllipseIntersectsSegment(xA, yA, xB, yB,
//                                                                                    0.0f, 0.0f, this.width * 0.5f, this.height * 0.5f,
//                                                                                    intersectionPointA, intersectionPointB, normalRadians);

//            return intersectionCount;
//        }
//    }
//}
