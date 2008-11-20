using System;
using GeoAPI.Geometries;
using GeoAPI.IO.WellKnownText;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.LinearReferencing;
using NetTopologySuite.Coordinates;

namespace GisSharpBlog.NetTopologySuite.Samples.LinearReferencing
{
    /// <summary>
    /// Examples of Linear Referencing
    /// </summary>
    public class LinearReferencingExample
    {
        private static readonly IGeometryFactory<BufferedCoordinate2D> _factory
            = GeometryFactory<BufferedCoordinate2D>.CreateFixedPrecision(
                new BufferedCoordinate2DSequenceFactory());

        private static readonly WktReader<BufferedCoordinate2D> _reader
            = new WktReader<BufferedCoordinate2D>(_factory, null);

        public void Run()
        {
            RunExtractedLine("LINESTRING (0 0, 10 10, 20 20)", 1, 10);
            RunExtractedLine("MULTILINESTRING ((0 0, 10 10), (20 20, 25 25, 30 40))", 1, 20);
        }

        public void RunExtractedLine(String wkt, Double start, Double end)
        {
            Console.WriteLine("=========================");
            IGeometry<BufferedCoordinate2D> g1 = _reader.Read(wkt);
            Console.WriteLine("Input Geometry: " + g1);
            Console.WriteLine("Indices to extract: " + start + " " + end);

            LengthIndexedLine<BufferedCoordinate2D> indexedLine
                = new LengthIndexedLine<BufferedCoordinate2D>(g1);

            IGeometry<BufferedCoordinate2D> subLine = indexedLine.ExtractLine(start, end);
            Console.WriteLine("Extracted Line: " + subLine);

            Double[] index = indexedLine.IndicesOf(subLine);
            Console.WriteLine("Indices of extracted line: " + index[0] + " " + index[1]);

            BufferedCoordinate2D midpt = indexedLine.ExtractPoint((index[0] + index[1])/2);
            Console.WriteLine("Midpoint of extracted line: " + midpt);
        }
    }
}