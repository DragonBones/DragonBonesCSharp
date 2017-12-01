// using System;
// using System.Collections.Generic;

// namespace DragonBones
// {
    //public class PolygonBoundingBoxData : BoundingBoxData
    //{
    //    /**
    //     * @private
    //     */
    //    public static int PolygonIntersectsSegment( float xA, float yA, float xB, float yB,
    //                                                List<float> vertices,
    //                                                Point intersectionPointA = null,
    //                                                Point intersectionPointB = null,
    //                                                Point normalRadians = null)
    //    {
    //        if (xA == xB)
    //        {
    //            xA = xB + 0.01f;
    //        }

    //        if (yA == yB)
    //        {
    //            yA = yB + 0.01f;
    //        }

    //        var l = vertices.Count;
    //        var dXAB = xA - xB;
    //        var dYAB = yA - yB;
    //        var llAB = xA * yB - yA * xB;
    //        int intersectionCount = 0;
    //        var xC = vertices[l - 2];
    //        var yC = vertices[l - 1];
    //        var dMin = 0.0f;
    //        var dMax = 0.0f;
    //        var xMin = 0.0f;
    //        var yMin = 0.0f;
    //        var xMax = 0.0f;
    //        var yMax = 0.0f;

    //        for (int i = 0; i < l; i += 2)
    //        {
    //            var xD = vertices[i];
    //            var yD = vertices[i + 1];

    //            if (xC == xD)
    //            {
    //                xC = xD + 0.01f;
    //            }

    //            if (yC == yD)
    //            {
    //                yC = yD + 0.01f;
    //            }

    //            var dXCD = xC - xD;
    //            var dYCD = yC - yD;
    //            var llCD = xC * yD - yC * xD;
    //            var ll = dXAB * dYCD - dYAB * dXCD;
    //            var x = (llAB * dXCD - dXAB * llCD) / ll;

    //            if (((x >= xC && x <= xD) || (x >= xD && x <= xC)) && (dXAB == 0 || (x >= xA && x <= xB) || (x >= xB && x <= xA)))
    //            {
    //                var y = (llAB * dYCD - dYAB * llCD) / ll;
    //                if (((y >= yC && y <= yD) || (y >= yD && y <= yC)) && (dYAB == 0 || (y >= yA && y <= yB) || (y >= yB && y <= yA)))
    //                {
    //                    if (intersectionPointB != null)
    //                    {
    //                        var d = x - xA;
    //                        if (d < 0.0f)
    //                        {
    //                            d = -d;
    //                        }

    //                        if (intersectionCount == 0)
    //                        {
    //                            dMin = d;
    //                            dMax = d;
    //                            xMin = x;
    //                            yMin = y;
    //                            xMax = x;
    //                            yMax = y;

    //                            if (normalRadians != null)
    //                            {
    //                                normalRadians.x = (float)Math.Atan2(yD - yC, xD - xC) - (float)Math.PI * 0.5f;
    //                                normalRadians.y = normalRadians.x;
    //                            }
    //                        }
    //                        else
    //                        {
    //                            if (d < dMin)
    //                            {
    //                                dMin = d;
    //                                xMin = x;
    //                                yMin = y;

    //                                if (normalRadians != null)
    //                                {
    //                                    normalRadians.x = (float)Math.Atan2(yD - yC, xD - xC) - (float)Math.PI * 0.5f;
    //                                }
    //                            }

    //                            if (d > dMax)
    //                            {
    //                                dMax = d;
    //                                xMax = x;
    //                                yMax = y;

    //                                if (normalRadians != null)
    //                                {
    //                                    normalRadians.y = (float)Math.Atan2(yD - yC, xD - xC) - (float)Math.PI * 0.5f;
    //                                }
    //                            }
    //                        }

    //                        intersectionCount++;
    //                    }
    //                    else
    //                    {
    //                        xMin = x;
    //                        yMin = y;
    //                        xMax = x;
    //                        yMax = y;
    //                        intersectionCount++;

    //                        if (normalRadians != null)
    //                        {
    //                            normalRadians.x = (float)Math.Atan2(yD - yC, xD - xC) - (float)Math.PI * 0.5f;
    //                            normalRadians.y = normalRadians.x;
    //                        }
    //                        break;
    //                    }
    //                }
    //            }

    //            xC = xD;
    //            yC = yD;
    //        }

    //        if (intersectionCount == 1)
    //        {
    //            if (intersectionPointA != null)
    //            {
    //                intersectionPointA.x = xMin;
    //                intersectionPointA.y = yMin;
    //            }

    //            if (intersectionPointB != null)
    //            {
    //                intersectionPointB.x = xMin;
    //                intersectionPointB.y = yMin;
    //            }

    //            if (normalRadians != null)
    //            {
    //                normalRadians.y = normalRadians.x + (float)Math.PI;
    //            }
    //        }
    //        else if (intersectionCount > 1)
    //        {
    //            intersectionCount++;

    //            if (intersectionPointA != null)
    //            {
    //                intersectionPointA.x = xMin;
    //                intersectionPointA.y = yMin;
    //            }

    //            if (intersectionPointB != null)
    //            {
    //                intersectionPointB.x = xMax;
    //                intersectionPointB.y = yMax;
    //            }
    //        }

    //        return intersectionCount;
    //    }

    //    /**
    //     * @private
    //     */
    //    public float x;
    //    /**
    //     * @private
    //     */
    //    public float y;
    //    /**
    //     * 多边形顶点。
    //     * @version DragonBones 5.1
    //     * @language zh_CN
    //     */
    //    public readonly List<float> vertices = new List<float>();
    //    /**
    //     * @private
    //     */
    //    public WeightData weight = null; // Initial value.

    //    protected override void _OnClear()
    //    {
    //        base._OnClear();

    //        if (this.weight != null)
    //        {
    //            this.weight.ReturnToPool();
    //        }

    //        this.type = BoundingBoxType.Polygon;
    //        this.x = 0.0f;
    //        this.y = 0.0f;
    //        this.vertices.Clear();
    //        this.weight = null;
    //    }

    //    /**
    //     * @inherDoc
    //     */
    //    public override bool ContainsPoint(float pX, float pY)
    //    {
    //        var isInSide = false;
    //        if (pX >= this.x && pX <= this.width && pY >= this.y && pY <= this.height)
    //        {
    //            for (int i = 0, l = this.vertices.Count, iP = l - 2; i < l; i += 2)
    //            {
    //                var yA = this.vertices[iP + 1];
    //                var yB = this.vertices[i + 1];
    //                if ((yB < pY && yA >= pY) || (yA < pY && yB >= pY))
    //                {
    //                    var xA = this.vertices[iP];
    //                    var xB = this.vertices[i];
    //                    if ((pY - yB) * (xA - xB) / (yA - yB) + xB < pX)
    //                    {
    //                        isInSide = !isInSide;
    //                    }
    //                }

    //                iP = i;
    //            }
    //        }

    //        return isInSide;
    //    }

    //    /**
    //     * @inherDoc
    //     */
    //    public override int IntersectsSegment( float xA, float yA, float xB, float yB,
    //                                            Point intersectionPointA = null,
    //                                            Point intersectionPointB = null,
    //                                            Point normalRadians = null )
    //    {
    //        var intersectionCount = 0;
    //        if (RectangleBoundingBoxData.RectangleIntersectsSegment(xA, yA, xB, yB, this.x, this.y, this.width, this.height, null, null, null) != 0)
    //        {
    //            intersectionCount = PolygonBoundingBoxData.PolygonIntersectsSegment
    //                                                        (
    //                                                         xA, yA, xB, yB,
    //                                                         this.vertices,
    //                                                         intersectionPointA, intersectionPointB, normalRadians
    //                                                        );
    //        }

    //        return intersectionCount;
    //    }
    //}
// }
