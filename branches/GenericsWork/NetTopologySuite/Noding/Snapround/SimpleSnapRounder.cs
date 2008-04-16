using System;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Noding.Snapround
{
    /// <summary>
    /// Uses Snap Rounding to compute a rounded,
    /// fully noded arrangement from a set of <see cref="NodedSegmentString{TCoordinate}" />s.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Implements the Snap Rounding technique described in Hobby, 
    /// Guibas and Marimont, and Goodrich et al.
    /// Snap Rounding assumes that all vertices lie on a uniform grid
    /// (hence the precision model of the input must be fixed precision,
    /// and all the input vertices must be rounded to that precision).
    /// </para>
    /// <para>
    /// This implementation uses simple iteration over the line segments.
    /// This implementation appears to be fully robust using an integer precision model.
    /// It will function with non-integer precision models, but the
    /// results are not 100% guaranteed to be correctly noded.
    /// </para>
    /// </remarks>
    public class SimpleSnapRounder<TCoordinate> : INoder<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        /// <summary>
        /// Adds a new node (equal to the snap pt) to the segment
        /// if the segment passes through the hot pixel.
        /// </summary>
        public static Boolean AddSnappedNode(HotPixel<TCoordinate> hotPix, NodedSegmentString<TCoordinate> segStr, Int32 segIndex)
        {
            LineSegment<TCoordinate> segment = segStr[segIndex];

            if (hotPix.Intersects(segment))
            {
                segStr.AddIntersection(hotPix.Coordinate, segIndex);
                return true;
            }

            return false;
        }

        private readonly ICoordinateFactory<TCoordinate> _factory;
        private readonly LineIntersector<TCoordinate> _li = null;
        private readonly Double _scaleFactor;
        //private IEnumerable<SegmentString<TCoordinate>> _nodedSegStrings = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleSnapRounder{TCoordinate}"/> class.
        /// </summary>
        /// <param name="pm">The <see cref="IPrecisionModel{TCoordinate}" /> to use.</param>
        public SimpleSnapRounder(IPrecisionModel<TCoordinate> pm, ICoordinateFactory<TCoordinate> factory)
        {
            _li = CGAlgorithms<TCoordinate>.CreateRobustLineIntersector();
            _li.PrecisionModel = pm;
            _scaleFactor = pm.Scale;
            _factory = factory;
        }

        /// <summary>
        /// Computes nodes introduced as a result of
        /// snapping segments to vertices of other segments.
        /// </summary>
        public IEnumerable<NodedSegmentString<TCoordinate>> ComputeVertexSnaps(IEnumerable<NodedSegmentString<TCoordinate>> edges)
        {
            foreach (NodedSegmentString<TCoordinate> edge0 in edges)
            {
                foreach (NodedSegmentString<TCoordinate> edge1 in edges)
                {
                    computeVertexSnaps(edge0, edge1);
                }
            }

            return edges;
        }

        /// <summary>
        /// Computes the noding for a collection of <see cref="NodedSegmentString{TCoordinate}" />s.
        /// Some Noders may add all these nodes to the input <see cref="NodedSegmentString{TCoordinate}" />s;
        /// others may only add some or none at all.
        /// </summary>
        public IEnumerable<NodedSegmentString<TCoordinate>> Node(IEnumerable<NodedSegmentString<TCoordinate>> inputSegmentStrings)
        {
            inputSegmentStrings = snapRound(inputSegmentStrings, _li);
            return NodedSegmentString<TCoordinate>.GetNodedSubstrings(inputSegmentStrings);
        }

        private void checkCorrectness(IEnumerable<NodedSegmentString<TCoordinate>> inputSegmentStrings)
        {
            IEnumerable<NodedSegmentString<TCoordinate>> resultSegStrings 
                = NodedSegmentString<TCoordinate>.GetNodedSubstrings(inputSegmentStrings);
            NodingValidator<TCoordinate> nv = new NodingValidator<TCoordinate>(resultSegStrings);

            try
            {
                nv.CheckValid();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
            }
        }

        private IEnumerable<NodedSegmentString<TCoordinate>> snapRound(IEnumerable<NodedSegmentString<TCoordinate>> segStrings, LineIntersector<TCoordinate> li)
        {
            IEnumerable<TCoordinate> intersections = findInteriorIntersections(segStrings, li);
            computeSnaps(segStrings, intersections);
            ComputeVertexSnaps(segStrings);
            return segStrings;
        }

        /// <summary>
        /// Computes all interior intersections in the collection of <see cref="NodedSegmentString{TCoordinate}" />s,
        /// and returns their <typeparamref name="TCoordinate"/>s.
        /// Does NOT node the segStrings.
        /// </summary>
        /// <returns>A list of <typeparamref name="TCoordinate"/>s for the intersections.</returns>
        private static IEnumerable<TCoordinate> findInteriorIntersections(IEnumerable<NodedSegmentString<TCoordinate>> segStrings, LineIntersector<TCoordinate> li)
        {
            IntersectionFinderAdder<TCoordinate> intFinderAdder = new IntersectionFinderAdder<TCoordinate>(li);
            SinglePassNoder<TCoordinate> noder = new MonotoneChainIndexNoder<TCoordinate>(intFinderAdder);
            noder.Node(segStrings);
            return intFinderAdder.InteriorIntersections;
        }

        /// <summary>
        /// Computes nodes introduced as a result of snapping segments to snap points (hot pixels).
        /// </summary>
        private void computeSnaps(IEnumerable<NodedSegmentString<TCoordinate>> segStrings, IEnumerable<TCoordinate> snapPts)
        {
            foreach (NodedSegmentString<TCoordinate> ss in segStrings)
            {
                computeSnaps(ss, snapPts);
            }
        }

        private void computeSnaps(NodedSegmentString<TCoordinate> ss, IEnumerable<TCoordinate> snapPts)
        {
            foreach (TCoordinate snapPt in snapPts)
            {
                HotPixel<TCoordinate> hotPixel = new HotPixel<TCoordinate>(snapPt, _scaleFactor, _li, _factory);
                
                for (Int32 i = 0; i < ss.Count - 1; i++)
                {
                    AddSnappedNode(hotPixel, ss, i);
                }
            }
        }

        /// <summary>
        /// Performs a brute-force comparison of every segment in each <see cref="NodedSegmentString{TCoordinate}" />.
        /// This has O(n^2) performance.
        /// </summary>
        private void computeVertexSnaps(NodedSegmentString<TCoordinate> e0, NodedSegmentString<TCoordinate> e1)
        {
            IEnumerable<TCoordinate> pts0 = e0.Coordinates;
            IEnumerable<TCoordinate> pts1 = e1.Coordinates;

            Int32 i0 = 0, i1 = 0;

            foreach (TCoordinate coordinate0 in pts0)
            {
                HotPixel<TCoordinate> hotPixel = new HotPixel<TCoordinate>(coordinate0, _scaleFactor, _li, _factory);

                IEnumerator<TCoordinate> pts1Enumerator = pts1.GetEnumerator();

                while (pts1Enumerator.MoveNext())
                {
                    // don't snap a vertex to itself
                    if (e0 == e1)
                    {
                        if (i0 == i1)
                        {
                            continue;
                        }
                    }

                    Boolean isNodeAdded = AddSnappedNode(hotPixel, e1, i1);

                    // if a node is created for a vertex, that vertex must be noded too
                    if (isNodeAdded)
                    {
                        e0.AddIntersection(coordinate0, i0);
                    }

                    i1 += 1;
                }

                i0 += 1;
            }
        }
    }
}