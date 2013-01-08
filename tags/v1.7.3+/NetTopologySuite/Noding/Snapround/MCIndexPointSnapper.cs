using System.Collections;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Index;
using GisSharpBlog.NetTopologySuite.Index.Chain;
using GisSharpBlog.NetTopologySuite.Index.Strtree;

namespace GisSharpBlog.NetTopologySuite.Noding.Snapround
{
    /// <summary>
    /// "Snaps" all <see cref="ISegmentString" />s in a <see cref="ISpatialIndex" /> containing
    /// <see cref="MonotoneChain" />s to a given <see cref="HotPixel" />.
    /// </summary>
    public class MCIndexPointSnapper
    {
        /// <summary>
        /// 
        /// </summary>
        // Public in java code... temporary modified for "safe assembly" in Sql2005
		internal static readonly int numberSnaps = 0;        

        private IList _monoChains;
        private readonly STRtree _index;

        /// <summary>
        /// Initializes a new instance of the <see cref="MCIndexPointSnapper"/> class.
        /// </summary>
        /// <param name="monoChains"></param>
        /// <param name="index"></param>
        public MCIndexPointSnapper(IList monoChains, ISpatialIndex index)
        {
            _monoChains = monoChains;
            _index = (STRtree) index;
        }

        /// <summary>
        /// 
        /// </summary>
        private class QueryVisitor : IItemVisitor
        {
            readonly IEnvelope _env;
            readonly HotPixelSnapAction _action;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="env"></param>
            /// <param name="action"></param>
            public QueryVisitor(IEnvelope env, HotPixelSnapAction action)
            {
                _env = env;
                _action = action;
            }

            /// <summary>
            /// </summary>
            /// <param name="item"></param>
            public void VisitItem(object item)
            {
                MonotoneChain testChain = (MonotoneChain) item;
                testChain.Select(_env, _action);
            }
        }

        /// <summary>
        /// Snaps (nodes) all interacting segments to this hot pixel.
        /// The hot pixel may represent a vertex of an edge,
        /// in which case this routine uses the optimization
        /// of not noding the vertex itself
        /// </summary>
        /// <param name="hotPixel">The hot pixel to snap to.</param>
        /// <param name="parentEdge">The edge containing the vertex, if applicable, or <c>null</c>.</param>
        /// <param name="vertexIndex"></param>
        /// <returns><c>true</c> if a node was added for this pixel.</returns>
        public bool Snap(HotPixel hotPixel, ISegmentString parentEdge, int vertexIndex)
        {
            IEnvelope pixelEnv = hotPixel.GetSafeEnvelope();
            HotPixelSnapAction hotPixelSnapAction = new HotPixelSnapAction(hotPixel, parentEdge, vertexIndex);
            _index.Query(pixelEnv, new QueryVisitor(pixelEnv, hotPixelSnapAction));
            return hotPixelSnapAction.IsNodeAdded;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hotPixel"></param>
        /// <returns></returns>
        public bool Snap(HotPixel hotPixel)
        {
            return Snap(hotPixel, null, -1);
        }

        /// <summary>
        /// 
        /// </summary>
        public class HotPixelSnapAction : MonotoneChainSelectAction
        {
            private readonly HotPixel _hotPixel;
            private readonly ISegmentString _parentEdge;
            private readonly int _vertexIndex;
            private bool _isNodeAdded;

            /// <summary>
            /// Initializes a new instance of the <see cref="HotPixelSnapAction"/> class.
            /// </summary>
            /// <param name="hotPixel"></param>
            /// <param name="parentEdge"></param>
            /// <param name="vertexIndex"></param>
            public HotPixelSnapAction(HotPixel hotPixel, ISegmentString parentEdge, int vertexIndex)
            {
                _hotPixel = hotPixel;
                _parentEdge = parentEdge;
                _vertexIndex = vertexIndex;
            }

            /// <summary>
            /// 
            /// </summary>
            public bool IsNodeAdded
            {
                get
                {
                    return _isNodeAdded;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="mc"></param>
            /// <param name="startIndex"></param>
            public override void Select(MonotoneChain mc, int startIndex)
            {
                INodableSegmentString ss = (INodableSegmentString) mc.Context;
                // don't snap a vertex to itself
                if (_parentEdge != null) 
                    if (ss == _parentEdge && startIndex == _vertexIndex)
                        return;
                _isNodeAdded = SimpleSnapRounder.AddSnappedNode(_hotPixel, ss, startIndex);
            }
        }
    }
}