using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Simplify;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Simplify
{
    [TestFixture]
    public class TopologyPreservingSimplifierTest
    {
        /// <summary>
        /// TestMultiPolygonWithSmallComponents
        /// </summary>
        /// <remarks>
        /// Test is from http://postgis.refractions.net/pipermail/postgis-users/2008-April/019327.html
        /// </remarks>
        [Test]
        public void TestMultiPolygonWithSmallComponents()
        {
            String geomStr = "MULTIPOLYGON(((13.73095 51.024734,13.7309323 51.0247668,13.7306959 51.0247959,13.7292724 51.0249742,13.7280216 51.0251252,13.7266598 51.0252998,13.7259617 51.0254072,13.7258854 51.0254201,13.7253253 51.0255144,13.725276 51.025492,13.724538 51.025631,13.7230288 51.0259021,13.7223529 51.0260273,13.7223299 51.0260863,13.7222292 51.026391,13.7220002 51.0273366,13.7217875 51.0282094,13.721746 51.028243,13.7217693 51.0282803,13.7215512 51.0291967,13.721513 51.029222,13.7215203 51.0292567,13.7212713 51.0295967,13.7222258 51.0299532,13.722234 51.03,13.7222931 51.0299823,13.7232514 51.0303187,13.7242514 51.0306715,13.724263 51.030714,13.7243024 51.0306951,13.7249934 51.0309315,13.7265097 51.0314552,13.7266116 51.0313952,13.7267988 51.0313334,13.7269952 51.0313243,13.72703 51.0314107,13.7271637 51.0313254,13.7272524 51.0313839,13.72739 51.031449,13.7276768 51.0313074,13.7283793 51.0309944,13.7296654 51.0304157,13.7297572 51.0303637,13.729845 51.0303139,13.7299557 51.0301763,13.7300964 51.0300176,13.730252 51.0298919,13.7304615 51.0297932,13.730668 51.0297363,13.730743 51.029783,13.7307859 51.0298398,13.7307094 51.0301388,13.730624 51.030263,13.7306955 51.0303267,13.7301182 51.0325594,13.7300528 51.0325663,13.7301114 51.0327342,13.7301645 51.0329094,13.7300035 51.0327693,13.7299669 51.0327351,13.7299445 51.0327211,13.7298934 51.032814,13.7298539 51.0328585,13.7297737 51.0328321,13.7288526 51.0325639,13.7288201 51.0324367,13.7284426 51.0324383,13.7276461 51.032179,13.7274569 51.0321976,13.7272787 51.0322421,13.7271265 51.0322903,13.7267034 51.0322495,13.7265364 51.0322161,13.7259018 51.0324269,13.7258649 51.03242,13.725733 51.0326646,13.7251933 51.0328876,13.7247918 51.0331374,13.7244439 51.0331106,13.7242967 51.0334273,13.7239131 51.0337529,13.7237035 51.0338511,13.7235429 51.033967,13.7233375 51.0339148,13.7232064 51.0339347,13.7231786 51.0339863,13.7228848 51.0340776,13.7224481 51.0341888,13.7220471 51.0342483,13.7217493 51.0343198,13.721552 51.0343861,13.7214718 51.0344095,13.7215108 51.034534,13.7205032 51.0349932,13.7197657 51.0352983,13.7195764 51.0352291,13.7195934 51.0352797,13.7182451 51.0359157,13.7181108 51.0359003,13.7181657 51.0359571,13.717622 51.0361956,13.7159749 51.0369683,13.7159057 51.0369284,13.7158604 51.0370288,13.7157161 51.0370124,13.7157523 51.0370733,13.7153708 51.0372801,13.7150274 51.0374899,13.7144074 51.0379192,13.7138287 51.0383899,13.7137514 51.0383857,13.7137492 51.0384566,13.7134249 51.0387269,13.7130179 51.0390385,13.7125791 51.0393343,13.7120736 51.039611,13.7115839 51.0398558,13.7112945 51.0399894,13.7114637 51.0402313,13.7123153 51.041449,13.7126333 51.0417033,13.713371 51.0421453,13.7138861 51.0424061,13.7142518 51.0425683,13.7164587 51.0435668,13.7167995 51.0437957,13.7170883 51.0439897,13.7190694 51.0451663,13.7196131 51.0458277,13.7197562 51.0461521,13.7198262 51.0464192,13.7198377 51.0467389,13.7205681 51.0455573,13.7210009 51.0450379,13.7214987 51.0445401,13.7220306 51.0442859,13.7227215 51.0439558,13.7237962 51.0434514,13.723979 51.0435278,13.7241448 51.0435041,13.7241052 51.0436042,13.7247987 51.0438896,13.7250186 51.0439093,13.7250579 51.0440386,13.7257225 51.0443545,13.7259312 51.0443456,13.725955 51.0443813,13.7260235 51.0443873,13.7260682 51.0445303,13.7282191 51.0455848,13.7290532 51.045927,13.7292643 51.0458591,13.7292228 51.0459969,13.729706 51.0461854,13.7303185 51.046393,13.7309107 51.0465601,13.731546 51.0466841,13.7321939 51.0467752,13.7332896 51.0468999,13.7333733 51.0469094,13.7334778 51.0468127,13.7335706 51.0469078,13.733651 51.0470684,13.7338458 51.0471508,13.7346109 51.0472333,13.7346367 51.0471474,13.7346922 51.0470697,13.7346666 51.0470056,13.7346564 51.0468714,13.7345552 51.0467095,13.7336001 51.0465496,13.733427 51.046454,13.7335317 51.0464255,13.7347225 51.0465948,13.7348421 51.0466562,13.7349123 51.0466203,13.736811 51.0468537,13.7382043 51.0469796,13.7383487 51.0469803,13.7394909 51.0469005,13.7400899 51.0467949,13.7405051 51.0464739,13.7408331 51.0462204,13.7412027 51.0463256,13.741053 51.0466451,13.7407291 51.0469007,13.7405095 51.0469726,13.7400888 51.0470337,13.7393051 51.0471049,13.7393014 51.0472015,13.7393088 51.0473019,13.7395556 51.0473056,13.7404944 51.0472245,13.740932 51.0470192,13.7414421 51.0465652,13.7414893 51.0465576,13.7416494 51.0464916,13.7416003 51.0466074,13.7416246 51.04663,13.741668 51.0466443,13.7417272 51.0467159,13.7417503 51.0466716,13.7423587 51.0468732,13.7426958 51.0470246,13.7429143 51.0471813,13.74318 51.04726,13.7430363 51.0472995,13.7433021 51.047588,13.7434678 51.0475916,13.7433805 51.0477019,13.7436362 51.0479981,13.7446308 51.0491622,13.7447961 51.0491827,13.744722 51.0492509,13.7448536 51.0494078,13.745056 51.0494766,13.7450313 51.0496901,13.7453573 51.0500052,13.7465317 51.0512807,13.7466999 51.0513722,13.746638 51.0514149,13.7468683 51.0516781,13.7470071 51.051777,13.7469985 51.0518746,13.7470732 51.0519866,13.7471316 51.0520528,13.7472989 51.0523089,13.7472368 51.0523858,13.7473063 51.0524932,13.7473468 51.0527412,13.7473392 51.0531614,13.7472987 51.0533157,13.7473919 51.0534224,13.7472684 51.0534549,13.7472134 51.0536926,13.7472913 51.0537784,13.7473216 51.053725,13.7474649 51.0537575,13.7474492 51.053833,13.7475625 51.0537839,13.7497379 51.0544435,13.7515333 51.0551019,13.7527693 51.0555438,13.7549766 51.0564993,13.7550622 51.0565364,13.755105 51.0566612,13.7552745 51.0566237,13.7558661 51.0560648,13.7559318 51.0560101,13.755908 51.055897,13.7559252 51.0558292,13.7559566 51.0557055,13.7564494 51.0551377,13.7564124 51.0550457,13.7573213 51.0539813,13.7575007 51.0539933,13.757856 51.0540047,13.7580394 51.054028,13.7580896 51.053984,13.7580949 51.0539463,13.7579963 51.0538534,13.7581294 51.0537147,13.7582346 51.0535957,13.758354 51.053433,13.758363 51.053392,13.7583656 51.0533457,13.758359 51.0532095,13.7583338 51.0530937,13.7582902 51.0529647,13.7580365 51.0522637,13.7577683 51.051463,13.7573182 51.0501993,13.7571595 51.0497164,13.7567579 51.0490095,13.7563383 51.0482979,13.7557757 51.0473383,13.7557095 51.0472522,13.7555771 51.0471199,13.7554448 51.0470471,13.7548596 51.0462612,13.7547097 51.046054,13.7549127 51.0460086,13.7548633 51.0459174,13.7548127 51.0458413,13.7547176 51.0457237,13.7538293 51.0449222,13.7530218 51.0441346,13.7526711 51.0437838,13.752446 51.0435522,13.7522297 51.0433547,13.751704 51.042833,13.7513058 51.0424448,13.7505766 51.0417281,13.7499967 51.0411283,13.7497695 51.0408943,13.7493849 51.0405205,13.7486222 51.0397896,13.7478209 51.0390261,13.7477474 51.0389532,13.7477041 51.0389189,13.7476277 51.0388729,13.7475781 51.0388513,13.7472699 51.038726,13.747131 51.0386506,13.7469329 51.0385052,13.7468562 51.0384284,13.7466683 51.0383483,13.7467998 51.038236,13.7473841 51.0380129,13.747838 51.0378277,13.7481801 51.0376558,13.7489728 51.0370285,13.7491313 51.0368016,13.7492665 51.0363477,13.7493166 51.0359389,13.7492966 51.0358087,13.7493888 51.0356942,13.7492867 51.0357016,13.7492855 51.0354359,13.7492829 51.034867,13.7492723 51.0348311,13.7492455 51.0347398,13.7493034 51.0346612,13.7491987 51.0346142,13.748866 51.034723,13.748791 51.034201,13.748335 51.034159,13.748294 51.034034,13.748205 51.033764,13.7488691 51.0333037,13.748962 51.033245,13.7486777 51.0332252,13.7483008 51.032683,13.7484397 51.0324582,13.7469913 51.0327817,13.7466998 51.0326205,13.7459997 51.0314852,13.7460996 51.0313569,13.745967 51.0314864,13.7449355 51.0317377,13.7447301 51.0316513,13.7446705 51.0318463,13.7420262 51.0323659,13.7419131 51.0322884,13.7418636 51.0322552,13.7416501 51.0321425,13.7415567 51.0317708,13.7414972 51.0314666,13.741484 51.0311492,13.741923 51.031003,13.7418649 51.030884,13.74209 51.0304134,13.7422077 51.0300143,13.7421975 51.0299222,13.742286 51.029835,13.7421463 51.0297533,13.7420951 51.0296254,13.7415933 51.0288452,13.7414906 51.0286855,13.7414437 51.0286127,13.7413482 51.0284642,13.7410545 51.0280777,13.7407158 51.0277229,13.7401513 51.0273842,13.7392803 51.0270293,13.7382744 51.0267844,13.737321 51.0267454,13.7365929 51.0267541,13.736556 51.026812,13.7364715 51.026754,13.7357088 51.0268017,13.7353967 51.02678,13.73534 51.02685,13.7352667 51.0267757,13.734907 51.0267324,13.734824 51.02679,13.7347684 51.0267064,13.7342093 51.0266674,13.73409 51.026725,13.7340359 51.0266283,13.7335072 51.0265633,13.733407 51.02663,13.7333208 51.0265373,13.7317087 51.0263813,13.7317173 51.0263119,13.73167 51.026241,13.7317563 51.0261602,13.7318473 51.0258395,13.7318647 51.0254971,13.73183 51.0253281,13.7317736 51.0252414,13.731663 51.025181,13.7316826 51.0251114,13.7310803 51.0247604,13.73095 51.024734)),((13.7368533 51.0470386,13.7368426 51.0471226,13.7368067 51.0472669,13.7368255 51.0473828,13.7369099 51.0474154,13.7376695 51.0474677,13.7382756 51.0474245,13.738513 51.0474297,13.7386105 51.0474065,13.738705 51.0473737,13.7385856 51.0473757,13.7385618 51.0473751,13.7385263 51.0473743,13.7384706 51.0473744,13.7383071 51.0473734,13.7383822 51.0473564,13.7390821 51.047287,13.7390933 51.047209,13.7390933 51.0471421,13.7368533 51.0470386)),((13.7367293 51.0470057,13.7346615 51.0466892,13.7347551 51.0468411,13.7347754 51.0470359,13.7347106 51.0471899,13.7356421 51.0472919,13.7366963 51.0474074,13.736705 51.047249,13.7367293 51.0470057)))";
            new GeometryOperationValidator(
                TPSimplifierResult.GetResult(
                    geomStr,
                    0.0057))
                .Test();
        }

        /// <summary>
        /// TestPolygonWithSpike
        /// </summary>
        /// <remarks>
        /// Test is from http://lists.jump-project.org/pipermail/jts-devel/2008-February/002350.html
        /// </remarks>/
        [Ignore("This test appears to fail because when the shape is simplified, the spike results in a cross-over of the polygon bounday which is invalid.  The test in JTS needs to be investigated as to whether it is working, because the code in NTS and JTS looks to be the same.")]
        public void TestPolygonWithSpike()
        {
            String geomStr = "POLYGON ((3312459.605 6646878.353, 3312460.524 6646875.969, 3312459.427 6646878.421, 3312460.014 6646886.391, 3312465.889 6646887.398, 3312470.827 6646884.839, 3312475.4 6646878.027, 3312477.289 6646871.694, 3312472.748 6646869.547, 3312468.253 6646874.01, 3312463.52 6646875.779, 3312459.605 6646878.353))";
            new GeometryOperationValidator(
                TPSimplifierResult.GetResult(
                    geomStr,
                    2.0))
                .Test();
        }

        [Test]
        public void TestPolygonNoReduction()
        {
            new GeometryOperationValidator(
                TPSimplifierResult.GetResult(
                    "POLYGON ((20 220, 40 220, 60 220, 80 220, 100 220, 120 220, 140 220, 140 180, 100 180, 60 180,     20 180, 20 220))",
                    10.0))
                .Test();
        }

        [Test]
        public void TestPolygonNoReductionWithConflicts()
        {
            new GeometryOperationValidator(
                TPSimplifierResult.GetResult(
                    "POLYGON ((40 240, 160 241, 280 240, 280 160, 160 240, 40 140, 40 240))",
                    10.0))
                .Test();
        }

        [Test]
        public void TestPolygonWithTouchingHole()
        {
            new GeometryOperationValidator(
                TPSimplifierResult.GetResult(
                    "POLYGON ((80 200, 240 200, 240 60, 80 60, 80 200), (120 120, 220 120, 180 199, 160 200, 140 199, 120 120))",
                    10.0))
                .SetExpectedResult("POLYGON ((80 200, 240 200, 240 60, 80 60, 80 200), (120 120, 220 120, 180 199, 160 200, 140 199, 120 120))")
                .Test();
        }

        [Test]
        public void TestFlattishPolygon()
        {
            new GeometryOperationValidator(
                TPSimplifierResult.GetResult(
                    "POLYGON ((0 0, 50 0, 53 0, 55 0, 100 0, 70 1,  60 1, 50 1, 40 1, 0 0))",
                    10.0))
                .Test();
        }

        [Ignore("This test fails because the flat hole simplifies to a linestring, which causes the result to be a geometry collection of a linearring for the polygon boundary, and a linestring representing the hole, and this doesn't match the expectation of a polygon with a hole.  Need to investigate what the result in JTS is because the code looks to be the same between JTS and NTS")]
        public void TestPolygonWithFlattishHole()
        {
            String geomStr = "POLYGON ((0 0, 0 200, 200 200, 200 0, 0 0), (140 40, 90 95, 40 160, 95 100, 140 40))";
            new GeometryOperationValidator(
                TPSimplifierResult.GetResult(
                    geomStr,
                    20.0))
                .SetExpectedResult(geomStr)
                .Test();
        }

        [Test]
        public void TestTinySquare()
        {
            new GeometryOperationValidator(
                TPSimplifierResult.GetResult(
              "POLYGON ((0 5, 5 5, 5 0, 0 0, 0 1, 0 5))",
                10.0))
                .Test();
        }

        [Test]
        public void TestTinyLineString()
        {
            new GeometryOperationValidator(
                TPSimplifierResult.GetResult(
                    "LINESTRING (0 5, 1 5, 2 5, 5 5)",
                    10.0))
                .Test();
        }

        [Test]
        public void TestMultiPoint()
        {
            new GeometryOperationValidator(
                TPSimplifierResult.GetResult(
                    "MULTIPOINT(80 200, 240 200, 240 60, 80 60, 80 200, 140 199, 120 120)",
                    10.0))
                .Test();
        }

        [Test]
        public void TestMultiLineString()
        {
            new GeometryOperationValidator(
                TPSimplifierResult.GetResult(
                    "MULTILINESTRING( (0 0, 50 0, 70 0, 80 0, 100 0), (0 0, 50 1, 60 1, 100 0) )",
                    10.0))
                .Test();
        }
        
        [Test]
        public void TestGeometryCollection()
        {
            new GeometryOperationValidator(
                TPSimplifierResult.GetResult(
                    "GEOMETRYCOLLECTION ("
                    + "MULTIPOINT (80 200, 240 200, 240 60, 80 60, 80 200, 140 199, 120 120),"
                    + "POLYGON ((80 200, 240 200, 240 60, 80 60, 80 200)),"
                    + "LINESTRING (80 200, 240 200, 240 60, 80 60, 80 200, 140 199, 120 120)"
                    + ")"
                    ,10.0))
                .Test();
        }
    }

    class TPSimplifierResult
    {
        private static WKTReader rdr = new WKTReader();

        public static IGeometry[] GetResult(String wkt, double tolerance)
        {
            IGeometry[] ioGeom = new Geometry[2];
            ioGeom[0] = rdr.Read(wkt);
            ioGeom[1] = TopologyPreservingSimplifier.Simplify(ioGeom[0], tolerance);
            Console.WriteLine(ioGeom[1]);
            return ioGeom;
        }
    }
}
