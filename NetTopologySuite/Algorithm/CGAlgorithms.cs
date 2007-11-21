using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Algorithm
{
    public enum Orientation
    {
        /// <summary> 
        /// A value that indicates an orientation of clockwise, or a right turn.
        /// </summary>
        Clockwise = -1,

        /// <summary> 
        /// A value that indicates an orientation of clockwise, or a right turn.
        /// </summary>
        Right = Clockwise,

        /// <summary>
        /// A value that indicates an orientation of counterclockwise, or a left turn.
        /// </summary>
        CounterClockwise = 1,

        /// <summary>
        /// A value that indicates an orientation of counterclockwise, or a left turn.
        /// </summary>
        Left = CounterClockwise,

        /// <summary>
        /// A value that indicates an orientation of collinear, or no turn (straight).
        /// </summary>
        Collinear = 0,

        /// <summary>
        /// A value that indicates an orientation of collinear, or no turn (straight).
        /// </summary>
        Straight = Collinear
    }

    /// <summary>
    /// Specifies and implements various fundamental Computational Geometric algorithms.
    /// The algorithms supplied in this class are robust for Double-precision floating point.
    /// </summary>
    public static class CGAlgorithms<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<TCoordinate>, IConvertible
    {
        /// <summary> 
        /// Returns the index of the direction of the point <paramref name="q"/>
        /// relative to a vector specified by 
        /// <paramref name="p1"/><c>-</c><paramref name="p2"/>.
        /// </summary>
        /// <param name="p1">The origin point of the vector.</param>
        /// <param name="p2">The final point of the vector.</param>
        /// <param name="q">The point to compute the direction to.</param>
        /// <returns> 
        /// 1 if q is counter-clockwise (left) from p1-p2,
        /// -1 if q is clockwise (right) from p1-p2,
        /// 0 if q is collinear with p1-p2.
        /// </returns>
        public static Int32 OrientationIndex(TCoordinate p1, TCoordinate p2, TCoordinate q)
        {
            // travelling along p1->p2, turn counter clockwise to get to q return 1,
            // travelling along p1->p2, turn clockwise to get to q return -1,
            // p1, p2 and q are colinear return 0.
            Double dx1 = p2[Ordinates.X] - p1[Ordinates.X];
            Double dy1 = p2[Ordinates.Y] - p1[Ordinates.Y];
            Double dx2 = q[Ordinates.X] - p2[Ordinates.X];
            Double dy2 = q[Ordinates.Y] - p2[Ordinates.Y];

            // NOTE: Can this be moved down to NPack?
            return RobustDeterminant.SignOfDet2x2(dx1, dy1, dx2, dy2);
        }

        /// <summary> 
        /// Test whether a point lies inside a ring.
        /// The ring may be oriented in either direction.
        /// If the point lies on the ring boundary the result of this method is unspecified.
        /// This algorithm does not attempt to first check the point against the envelope
        /// of the ring.
        /// </summary>
        /// <param name="p">Point to check for ring inclusion.</param>
        /// <param name="ring">Assumed to have first point identical to last point.</param>
        /// <returns><see langword="true"/> if p is inside ring.</returns>
        public static Boolean IsPointInRing(TCoordinate p, IEnumerable<TCoordinate> ring)
        {
            Int32 i;
            Int32 crossings = 0;        // number of segment/ray crossings
            Int32 nPts = ring.Length;

            // For each segment l = (i-1, i), see if it crosses ray 
            // from test point in positive x direction.
            for (i = 1; i < nPts; i++)
            {
                Int32 i1;   // point index; i1 = i-1
                Double x1;  // translated coordinates
                Double y1;
                Double x2;
                Double y2;

                i1 = i - 1;
                TCoordinate p1 = ring[i];
                TCoordinate p2 = ring[i1];

                x1 = p1[Ordinates.X] - p[Ordinates.X];
                y1 = p1[Ordinates.Y] - p[Ordinates.Y];
                x2 = p2[Ordinates.X] - p[Ordinates.X];
                y2 = p2[Ordinates.Y] - p[Ordinates.Y];

                if (((y1 > 0) && (y2 <= 0)) || ((y2 > 0) && (y1 <= 0)))
                {
                    Double xInt; // x intersection of segment with ray

                    // segment straddles x axis, so compute intersection.
                    // NOTE: Can this be moved down to NPack?
                    xInt = RobustDeterminant.SignOfDet2x2(x1, y1, x2, y2) / (y2 - y1);

                    // crosses ray if strictly positive intersection.
                    if (0.0 < xInt)
                    {
                        crossings++;
                    }
                }
            }

            // p is inside if number of crossings is odd.
            if ((crossings % 2) == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary> 
        /// Test whether a point lies on the line segments defined by a
        /// list of coordinates.
        /// </summary>
        /// <returns> 
        /// <see langword="true"/> true if
        /// the point is a vertex of the line or lies in the interior of a line
        /// segment in the linestring.
        /// </returns>
        public static Boolean IsOnLine(TCoordinate p, IEnumerable<TCoordinate> line)
        {
            LineIntersector<TCoordinate> lineIntersector = new RobustLineIntersector<TCoordinate>();

            for (Int32 i = 1; i < line.Length; i++)
            {
                TCoordinate p0 = line[i - 1];
                TCoordinate p1 = line[i];
                lineIntersector.ComputeIntersection(p, p0, p1);

                if (lineIntersector.HasIntersection)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Computes whether a ring defined by an array of <typeparamref name="TCoordinate"/>s 
        /// is oriented counter-clockwise.
        /// </summary>
        /// <remarks>
        /// The list of points is assumed to have the first and last points equal.
        /// This will handle coordinate lists which contain repeated points.
        /// This algorithm is only guaranteed to work with valid rings.
        /// If the ring is invalid (e.g. self-crosses or touches),
        /// the computed result may not be correct.
        /// </remarks>
        public static Boolean IsCCW(IEnumerable<TCoordinate> ring)
        {
            TCoordinate first = default(TCoordinate);
            TCoordinate previous = default(TCoordinate);
            TCoordinate current = default(TCoordinate);
            //TCoordinate next = default(TCoordinate);

            TCoordinate beforeHighPoint = default(TCoordinate);
            TCoordinate highPoint = default(TCoordinate);
            TCoordinate afterHighPoint = default(TCoordinate);

            Boolean firstIsSet = false;
            Boolean needToSetNextPoint = false;

            // find highest point and the points to either side
            foreach (TCoordinate coordinate in ring)
            {
                if (!firstIsSet)
	            {
	                firstIsSet = true;
	                first = coordinate;
	            }

                previous = current;
                current = coordinate;

                if (needToSetNextPoint)
                {
                    needToSetNextPoint = false;
                    afterHighPoint = current;
                }

                if (current[Ordinates.Y] > highPoint[Ordinates.Y])
                {
                    beforeHighPoint = previous;
                    highPoint = current;
                    needToSetNextPoint = true;
                }
            }

            // The high point was the last point, so wrap the next point
            // in the ring around to the first point
            if (needToSetNextPoint)
            {
                afterHighPoint = first;
            }

            /*
             * This check catches cases where the ring contains an A-B-A configuration of points.
             * This can happen if the ring does not contain 3 distinct points
             * (including the case where the input array has fewer than 4 elements),
             * or it contains coincident line segments.
             */
            if (beforeHighPoint.Equals(highPoint) || afterHighPoint.Equals(highPoint) 
                || beforeHighPoint.Equals(afterHighPoint))
            {
                return false;
            }

            Int32 disc = (Int32)ComputeOrientation(beforeHighPoint, highPoint, afterHighPoint);

            /*
             *  If disc is exactly 0, lines are collinear.  There are two possible cases:
             *  (1) the lines lie along the x axis in opposite directions
             *  (2) the lines lie on top of one another
             *
             *  (1) is handled by checking if next is left of prev ==> CCW
             *  (2) will never happen if the ring is valid, so don't check for it
             *  (Might want to assert this)
             */
            Boolean isCCW;

            if (disc == 0)
            {
                // poly is CCW if prev x is right of next x
                isCCW = (beforeHighPoint[Ordinates.X] > afterHighPoint[Ordinates.X]);
            }
            else
            {
                // if area is positive, points are ordered CCW
                isCCW = (disc > 0);
            }

            return isCCW;
        }

        /// <summary>
        /// Computes the orientation of a point q to the directed line segment p1-p2.
        /// </summary>
        /// <remarks>
        /// The orientation of a point relative to a directed line segment indicates
        /// which way you turn to get to q after travelling from p1 to p2.
        /// </remarks>
        /// <returns> 
        /// 1 if q is counter-clockwise from p1-p2,
        /// -1 if q is clockwise from p1-p2,
        /// 0 if q is collinear with p1-p2-
        /// </returns>
        public static Orientation ComputeOrientation(TCoordinate p1, TCoordinate p2, TCoordinate q)
        {
            return (Orientation)OrientationIndex(p1, p2, q);
        }

#warning Non-robust method
        // NOTE: Can this be moved down to NPack?
        /// <summary> 
        /// Computes the distance from a point p to a line segment AB.
        /// Note: NON-ROBUST!
        /// </summary>
        /// <param name="p">The point to compute the distance for.</param>
        /// <param name="A">One point of the line.</param>
        /// <param name="B">Another point of the line (must be different to A).</param>
        /// <returns> The distance from p to line segment AB.</returns>
        public static Double DistancePointLine(TCoordinate p, TCoordinate A, TCoordinate B)
        {
            // if start == end, then use pt distance
            if (A.Equals(B))
            {
                return p.Distance(A);
            }

            // codekaizen:  Isn't this the standard v1 � v2 / norm(v2)^2 formula to project
            //              a point to the vector space spanned by the line?
            //              NPack should be able to handle it.

            // otherwise use comp.graphics.algorithms Frequently Asked Questions method
            /*(1)     	      AC dot AB
                        r =   ---------
                              ||AB||^2
             
		                r has the following meaning:
		                r=0 Point = A
		                r=1 Point = B
		                r<0 Point is on the backward extension of AB
		                r>1 Point is on the forward extension of AB
		                0<r<1 Point is interior to AB
	        */

            Double r = ((p[Ordinates.X] - A[Ordinates.X]) * (B[Ordinates.X] - A[Ordinates.X])
                            + (p[Ordinates.Y] - A[Ordinates.Y]) * (B[Ordinates.Y] - A[Ordinates.Y]))
                       /
                       ((B[Ordinates.X] - A[Ordinates.X]) * (B[Ordinates.X] - A[Ordinates.X])
                            + (B[Ordinates.Y] - A[Ordinates.Y]) * (B[Ordinates.Y] - A[Ordinates.Y]));

            if (r <= 0.0)
            {
                return p.Distance(A);
            }

            if (r >= 1.0)
            {
                return p.Distance(B);
            }


            /*(2)
		                    (Ay-Cy)(Bx-Ax)-(Ax-Cx)(By-Ay)
		                s = -----------------------------
		             	                Curve^2

		                Then the distance from C to Point = |s|*Curve.
	        */

            Double s = ((A[Ordinates.Y] - p[Ordinates.Y]) * (B[Ordinates.X] - A[Ordinates.X]) - 
                            (A[Ordinates.X] - p[Ordinates.X]) * (B[Ordinates.Y] - A[Ordinates.Y]))
                       /
                       ((B[Ordinates.X] - A[Ordinates.X]) * (B[Ordinates.X] - A[Ordinates.X]) + 
                            (B[Ordinates.Y] - A[Ordinates.Y]) * (B[Ordinates.Y] - A[Ordinates.Y]));

            return Math.Abs(s) * 
                Math.Sqrt(  
                    (B[Ordinates.X] - A[Ordinates.X]) * (B[Ordinates.X] - A[Ordinates.X]) + 
                    (B[Ordinates.Y] - A[Ordinates.Y]) * (B[Ordinates.Y] - A[Ordinates.Y]));
        }

        // NOTE: Can this be moved down to NPack?
        /// <summary> 
        /// Computes the perpendicular distance from a point p
        /// to the (infinite) line containing the points AB
        /// </summary>
        /// <param name="p">The point to compute the distance for.</param>
        /// <param name="A">One point of the line.</param>
        /// <param name="B">Another point of the line (must be different to A).</param>
        /// <returns>The perpendicular distance from p to line AB.</returns>
        public static Double DistancePointLinePerpendicular(TCoordinate p, TCoordinate A, TCoordinate B)
        {
            // use comp.graphics.algorithms Frequently Asked Questions method
            /*(2)
                            (Ay-Cy)(Bx-Ax)-(Ax-Cx)(By-Ay)
                        s = -----------------------------
                                         Curve^2

                        Then the distance from C to Point = |s|*Curve.
            */

            Double s = ((A[Ordinates.Y] - p[Ordinates.Y]) * (B[Ordinates.X] - A[Ordinates.X]) - 
                            (A[Ordinates.X] - p[Ordinates.X]) * (B[Ordinates.Y] - A[Ordinates.Y]))
                       /
                       ((B[Ordinates.X] - A[Ordinates.X]) * (B[Ordinates.X] - A[Ordinates.X]) + 
                            (B[Ordinates.Y] - A[Ordinates.Y]) * (B[Ordinates.Y] - A[Ordinates.Y]));

            return Math.Abs(s) * Math.Sqrt(((B[Ordinates.X] - A[Ordinates.X]) * (B[Ordinates.X] - A[Ordinates.X]) + (B[Ordinates.Y] - A[Ordinates.Y]) * (B[Ordinates.Y] - A[Ordinates.Y])));
        }

#warning Non-robust method
        // NOTE: Can this be moved down to NPack?
        /// <summary> 
        /// Computes the distance from a line segment AB to a line segment CD.
        /// Note: NON-ROBUST!
        /// </summary>
        /// <param name="A">A point of one line.</param>
        /// <param name="B">The second point of the line (must be different to A).</param>
        /// <param name="C">One point of the line.</param>
        /// <param name="D">Another point of the line (must be different to A).</param>
        /// <returns>The distance from line segment AB to line segment CD.</returns>
        public static Double DistanceLineLine(TCoordinate A, TCoordinate B, TCoordinate C, TCoordinate D)
        {
            // check for zero-length segments
            if (A.Equals(B))
            {
                return DistancePointLine(A, C, D);
            }

            if (C.Equals(D))
            {
                return DistancePointLine(D, A, B);
            }

            // AB and CD are line segments
            /* from comp.graphics.algo

	            Solving the above for r and s yields
				            (Ay-Cy)(Dx-Cx)-(Ax-Cx)(Dy-Cy)
	                    r = ----------------------------- (eqn 1)
				            (Bx-Ax)(Dy-Cy)-(By-Ay)(Dx-Cx)

		 	                (Ay-Cy)(Bx-Ax)-(Ax-Cx)(By-Ay)
		                s = ----------------------------- (eqn 2)
			                (Bx-Ax)(Dy-Cy)-(By-Ay)(Dx-Cx)
	            Let Point be the position vector of the intersection point, then
		            Point=A+r(B-A) or
		            Px=Ax+r(Bx-Ax)
		            Py=Ay+r(By-Ay)
	            By examining the values of r & s, you can also determine some other
                limiting conditions:
		            If 0<=r<=1 & 0<=s<=1, intersection exists
		            r<0 or r>1 or s<0 or s>1 line segments do not intersect
		            If the denominator in eqn 1 is zero, AB & CD are parallel
		            If the numerator in eqn 1 is also zero, AB & CD are collinear.

	        */
            Double r_top = (A[Ordinates.Y] - C[Ordinates.Y]) * (D[Ordinates.X] - C[Ordinates.X]) - (A[Ordinates.X] - C[Ordinates.X]) * (D[Ordinates.Y] - C[Ordinates.Y]);
            Double r_bot = (B[Ordinates.X] - A[Ordinates.X]) * (D[Ordinates.Y] - C[Ordinates.Y]) - (B[Ordinates.Y] - A[Ordinates.Y]) * (D[Ordinates.X] - C[Ordinates.X]);

            Double s_top = (A[Ordinates.Y] - C[Ordinates.Y]) * (B[Ordinates.X] - A[Ordinates.X]) - (A[Ordinates.X] - C[Ordinates.X]) * (B[Ordinates.Y] - A[Ordinates.Y]);
            Double s_bot = (B[Ordinates.X] - A[Ordinates.X]) * (D[Ordinates.Y] - C[Ordinates.Y]) - (B[Ordinates.Y] - A[Ordinates.Y]) * (D[Ordinates.X] - C[Ordinates.X]);

            if ((r_bot == 0) || (s_bot == 0))
            {
                return Math.Min(DistancePointLine(A, C, D),
                                Math.Min(DistancePointLine(B, C, D),
                                         Math.Min(DistancePointLine(C, A, B),
                                                  DistancePointLine(D, A, B))));
            }


            Double s = s_top / s_bot;
            Double r = r_top / r_bot;

            if ((r < 0) || (r > 1) || (s < 0) || (s > 1))
            {
                //no intersection
                return Math.Min(DistancePointLine(A, C, D),
                                Math.Min(DistancePointLine(B, C, D),
                                         Math.Min(DistancePointLine(C, A, B),
                                                  DistancePointLine(D, A, B))));
            }

            return 0.0; //intersection exists
        }

        /// <summary>
        /// Returns the signed area for a ring.  The area is positive ifthe ring is oriented CW.
        /// </summary>
        public static Double SignedArea(TCoordinate[] ring)
        {
            if (ring.Length < 3)
            {
                return 0.0;
            }

            Double sum = 0.0;

            for (Int32 i = 0; i < ring.Length - 1; i++)
            {
                Double bx = ring[i][Ordinates.X];
                Double by = ring[i][Ordinates.Y];
                Double cx = ring[i + 1][Ordinates.X];
                Double cy = ring[i + 1][Ordinates.Y];
                sum += (bx + cx) * (cy - by);
            }

            return -sum / 2.0;
        }

        /// <summary> 
        /// Computes the length of a linestring specified by a sequence of points.
        /// </summary>
        /// <param name="coordinates">The points specifying the linestring.</param>
        /// <returns>The length of the linestring.</returns>
        public static Double Length(ICoordinateSequence<TCoordinate> coordinates)
        {
            if (coordinates.Count < 1)
            {
                return 0.0;
            }

            Double sum = 0.0;

            for (Int32 i = 1; i < coordinates.Count; i++)
            {
                sum += coordinates[i].Distance(coordinates[i - 1]);
            }

            return sum;
        }
    }
}