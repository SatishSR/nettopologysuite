using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries.Utilities;
using GisSharpBlog.NetTopologySuite.Operation.Overlay;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Operation.Union
{
    ///<summary>
    ///Unions a collection of Geometry or a single Geometry 
    ///(which may be a collection) together.
    ///By using this special-purpose operation over a collection of geometries
    ///it is possible to take advantage of various optimizations to improve performance.
    ///Heterogeneous {@link GeometryCollection}s are fully supported.
    ///
    ///The result obeys the following contract:
    ///<list type="Bullet">
    ///<item>Unioning a set of overlapping {@link Polygons}s has the effect of
    ///merging the areas (i.e. the same effect as 
    ///iteratively unioning all individual polygons together).</item>
    ///<item>Unioning a set of {@link LineString}s has the effect of <b>fully noding</b> 
    ///and <b>dissolving</b> the input linework.
    ///In this context "fully noded" means that there will be a node or endpoint in the output 
    ///for every endpoint or line segment crossing in the input.
    ///"Dissolved" means that any duplicate (e.g. coincident) line segments or portions
    ///of line segments will be reduced to a single line segment in the output.  
    ///This is consistent with the semantics of the 
    ///{@link Geometry#union(Geometry)} operation.
    ///If <b>merged</b> linework is required, the {@link LineMerger} class can be used.</item>
    ///
    ///<item>Unioning a set of {@link Points}s has the effect of merging
    ///al identical points (producing a set with no duplicates).</item>
    ///</list>
    ///</summary>
    ///<typeparam name="TCoordinate"></typeparam>
    public class UnaryUnionOp<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
            IComparable<TCoordinate>, IConvertible,
            IComputable<Double, TCoordinate>
    {
        private readonly List<IGeometry<TCoordinate>> _lines = new List<IGeometry<TCoordinate>>();
        private readonly List<IGeometry<TCoordinate>> _points = new List<IGeometry<TCoordinate>>();
        private readonly List<IGeometry<TCoordinate>> _polygons = new List<IGeometry<TCoordinate>>();

        private IGeometryFactory<TCoordinate> _geomFact;

        ///<summary>
        /// Constructs an instance of this class
        ///</summary>
        ///<param name="geoms">an <see cref="IEnumerable{T}"/> of <see cref="IGeometry{TCoordinate}"/>s</param>
        ///<param name="geomFact">an <see cref="IGeometryFactory{TCoordinate}"/></param>
        public UnaryUnionOp(IEnumerable<IGeometry<TCoordinate>> geoms, IGeometryFactory<TCoordinate> geomFact)
        {
            _geomFact = geomFact;
            Extract(geoms);
        }

        ///<summary>
        /// Constructs an instance of this class
        ///</summary>
        ///<param name="geoms">an <see cref="IEnumerable{T}"/> of <see cref="IGeometry{TCoordinate}"/>s</param>
        public UnaryUnionOp(IEnumerable<IGeometry<TCoordinate>> geoms)
        {
            Extract(geoms);
        }


        ///<summary>
        /// Constructs an instance of this class
        ///</summary>
        ///<param name="geom">an <see cref="IGeometry{TCoordinate}"/></param>
        public UnaryUnionOp(IGeometry<TCoordinate> geom)
        {
            Extract(geom);
        }

        ///<summary>
        ///</summary>
        ///<param name="geoms"></param>
        ///<returns></returns>
        public static IGeometry<TCoordinate> Union(IEnumerable<IGeometry<TCoordinate>> geoms)
        {
            UnaryUnionOp<TCoordinate> op = new UnaryUnionOp<TCoordinate>(geoms);
            return op.Union();
        }

        ///<summary>
        ///</summary>
        ///<param name="geoms"></param>
        ///<param name="geomFact"></param>
        ///<returns></returns>
        public static IGeometry<TCoordinate> Union(IEnumerable<IGeometry<TCoordinate>> geoms,
                                                   IGeometryFactory<TCoordinate> geomFact)
        {
            UnaryUnionOp<TCoordinate> op = new UnaryUnionOp<TCoordinate>(geoms, geomFact);
            return op.Union();
        }

        ///<summary>
        ///</summary>
        ///<param name="geom"></param>
        ///<returns></returns>
        public static IGeometry<TCoordinate> Union(IGeometry<TCoordinate> geom)
        {
            UnaryUnionOp<TCoordinate> op = new UnaryUnionOp<TCoordinate>(geom);
            return op.Union();
        }

        private void Extract(IEnumerable<IGeometry<TCoordinate>> geoms)
        {
            foreach (IGeometry<TCoordinate> geom in geoms)
                Extract(geom);
        }

        private void Extract(IGeometry<TCoordinate> geom)
        {
            if (_geomFact == null)
                _geomFact = geom.Factory;

            /*
            PolygonExtracter.getPolygons(geom, polygons);
            LineStringExtracter.getLines(geom, lines);
            PointExtracter.getPoints(geom, points);
            */
            _polygons.AddRange(GeometryExtracter<TCoordinate>.Extract<IPolygon<TCoordinate>>(geom));
            _lines.AddRange(GeometryExtracter<TCoordinate>.Extract<ILineString<TCoordinate>>(geom));
            _points.AddRange(GeometryExtracter<TCoordinate>.Extract<IPoint<TCoordinate>>(geom));
        }

        ///<summary>
        /// Gets the union of the input geometries.
        /// If no input geometries were provided, a POINT EMPTY is returned.
        ///</summary>
        ///<returns>a Geometry containing the union</returns>
        /// <returns>an empty GEOMETRYCOLLECTION if no geometries were provided in the input</returns>
        public IGeometry<TCoordinate> Union()
        {
            if (_geomFact == null)
            {
                return null;
            }

            IGeometry<TCoordinate> unionPoints = null;
            if (_points.Count > 0)
            {
                IGeometry<TCoordinate> ptGeom =
                    _geomFact.BuildGeometry(_points);
                unionPoints = UnionNoOpt(ptGeom);
            }

            IGeometry<TCoordinate> unionLines = null;
            if (_lines.Count > 0)
            {
                IGeometry<TCoordinate> lineGeom = _geomFact.BuildGeometry(_lines);
                unionLines = UnionNoOpt(lineGeom);
            }

            IGeometry<TCoordinate> unionPolygons = null;
            if (_polygons.Count > 0)
            {
                unionPolygons = CascadedPolygonUnion<TCoordinate>.Union(_polygons);
            }

            /**
             * Performing two unions is somewhat inefficient,
             * but is mitigated by unioning lines and points first
             */
            IGeometry<TCoordinate> unionLA = UnionWithNull(unionLines, unionPolygons);
            IGeometry<TCoordinate> union;
            if (unionPoints == null)
                union = unionLA;
            else if (unionLA == null)
                union = unionPoints;
            else
                union = PointGeometryUnion<TCoordinate>.Union((IPuntal<TCoordinate>) unionPoints, unionLA);

            if (union == null)
                return _geomFact.CreateGeometryCollection(null);

            return union;
        }

        /*
        private static IEnumerable<IGeometry<TCoordinate>> convertPoints(IEnumerable<IPoint<TCoordinate>> geom)
        {
            foreach (IPoint<TCoordinate> point in geom)
                yield return point;
        }

        private IEnumerable<IGeometry<TCoordinate>> convertLineStrings(IEnumerable<ILineString<TCoordinate>> geom)
        {
            foreach (ILineString<TCoordinate> line in geom)
                yield return line;
        }
         */

        /// <summary>
        /// Computes the union of two geometries, either of both of which may be null.
        /// </summary>"/>
        /// <param name="g0">a <see cref="IGeometry{TCoordinate}"/></param>
        /// <param name="g1">a <see cref="IGeometry{TCoordinate}"/></param>
        /// <returns>the union of the input(s)</returns>
        /// <returns>null if both inputs are null</returns>
        private static IGeometry<TCoordinate> UnionWithNull(IGeometry<TCoordinate> g0, IGeometry<TCoordinate> g1)
        {
            if (g0 == null && g1 == null)
                return null;

            if (g1 == null)
                return g0;
            if (g0 == null)
                return g1;

            return g0.Union(g1);
        }

        /**
        ///Computes a unary union with no extra optimization,
        ///and no short-circuiting.
        ///Due to the way the overlay operations 
        ///are implemented, this is still efficient in the case of linear 
        ///and puntal geometries.
        ///
        ///@param g0 a geometry
        ///@return the union of the input geometry
         */

        private IGeometry<TCoordinate> UnionNoOpt(IGeometry<TCoordinate> g0)
        {
            IGeometry<TCoordinate> empty = _geomFact.CreatePoint();
            return OverlayOp<TCoordinate>.Overlay(g0, empty, SpatialFunctions.Union);
        }
    }
}