﻿using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Index.Chain;

namespace NetTopologySuite.Noding
{
    ///<summary>
    /// Detects and records an intersection between two <see cref="ISegmentString"/>s,
    /// if one exists.  Only a single intersection is recorded.
    ///</summary>
    /// <remarks>
    /// This strategy can be configured to search for proper intersections.
    /// In this case, the presence of any intersection will still be recorded,
    /// but searching will continue until either a proper intersection has been found
    /// or no intersections are detected.
    /// </remarks>
    public class SegmentIntersectionDetector : ISegmentIntersector
    {
        private readonly LineIntersector _li;
        private bool _findProper;
        private bool _findAllTypes;

        private bool _hasIntersection;
        private bool _hasProperIntersection;
        private bool _hasNonProperIntersection;

        private ICoordinate _intPt;
        private ICoordinate[] _intSegments;

        ///<summary>
        /// Creates an intersection finder 
        ///</summary>
        /// <param name="li">The LineIntersector to use</param>
        public SegmentIntersectionDetector(LineIntersector li)
        {
            _li = li;
        }

        public bool FindProper { get { return _findProper; } set { _findProper = value; } }
        public bool FindAllIntersectionTypes { get { return _findAllTypes; } set { _findAllTypes = value; } }

        ///<summary>
        /// Tests whether an intersection was found.
        ///</summary>
        public bool HasIntersection
        {
            get { return _hasIntersection; }
        }

        ///<summary>
        /// Tests whether a proper intersection was found.
        ///</summary>
        public bool HasProperIntersection
        {
            get { return _hasProperIntersection; }
        }

        ///<summary>
        /// Tests whether a non-proper intersection was found.
        ///</summary>
        public bool HasNonProperIntersection
        {
            get { return _hasNonProperIntersection; }
        }

        ///<summary>
        /// Gets the computed location of the intersection. Due to round-off, the location may not be exact.
        ///</summary>
        public ICoordinate Intersection
        {
            get { return _intPt; }
        }

        ///<summary>Gets the endpoints of the intersecting segments.
        ///</summary>
        /// <remarks>An array of the segment endpoints (p00, p01, p10, p11)</remarks>
        public ICoordinate[] IntersectionSegments
        {
            get { return _intSegments; }
        }

        ///<summary>
        /// This method is called by clients of the <see cref="ISegmentIntersector"/> class to process
        /// intersections for two segments of the <see cref="ISegmentString"/>s being intersected.
        ///</summary>
        /// <remarks>
        /// Note that some clients (such as <see cref="MonotoneChain"/>s) may optimize away
        /// this call for segment pairs which they have determined do not intersect
        /// (e.g. by an disjoint envelope test).
        /// </remarks>
        public void ProcessIntersections(
            ISegmentString e0, int segIndex0,
            ISegmentString e1, int segIndex1
            )
        {
            // don't bother intersecting a segment with itself
            if (e0 == e1 && segIndex0 == segIndex1) return;

            var coords = e0.Coordinates;
            ICoordinate p00 = coords[segIndex0];
            ICoordinate p01 = coords[segIndex0 + 1];
            coords = e1.Coordinates;
            ICoordinate p10 = coords[segIndex1];
            ICoordinate p11 = coords[segIndex1 + 1];

            _li.ComputeIntersection(p00, p01, p10, p11);
            //  if (li.hasIntersection() && li.isProper()) Debug.println(li);

            if (_li.HasIntersection)
            {
                // System.out.println(li);

                // record intersection info
                _hasIntersection = true;

                bool isProper = _li.IsProper;
                if (isProper)
                    _hasProperIntersection = true;
                if (!isProper)
                    _hasNonProperIntersection = true;

                /*
                 * If this is the kind of intersection we are searching for
                 * OR no location has yet been recorded
                 * save the location data
                 */
                bool saveLocation = true;
                if (_findProper && !isProper) saveLocation = false;

                if (_intPt == null || saveLocation)
                {

                    // record intersection location (approximate)
                    _intPt = _li.GetIntersection(0);

                    // record intersecting segments
                    _intSegments = new ICoordinate[4];
                    _intSegments[0] = p00;
                    _intSegments[1] = p01;
                    _intSegments[2] = p10;
                    _intSegments[3] = p11;
                }
            }
        }

        public bool IsDone
        {
            get
            {
              /*
               * If finding all types, we can stop
               * when both possible types have been found.
               */
                if (_findAllTypes)
                {
                    return _hasProperIntersection && _hasNonProperIntersection;
                }

                /*
                 * If searching for a proper intersection, only stop if one is found
                 */
                if (_findProper)
                {
                    return _hasProperIntersection;
                }
                return _hasIntersection;
            }
        }
    }
}