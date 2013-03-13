using System.Collections.Generic;
#if SILVERLIGHT
using ArrayList = System.Collections.Generic.List<object>;
#endif

namespace NetTopologySuite.Index.Bintree
{
    /// <summary> 
    /// The base class for nodes in a <c>Bintree</c>.
    /// </summary>
    public abstract class NodeBase<T>
    {
        /// <summary> 
        /// Returns the index of the subnode that wholely contains the given interval.
        /// If none does, returns -1.
        /// </summary>
        /// <param name="interval"></param>
        /// <param name="centre"></param>
        public static int GetSubnodeIndex(Interval interval, double centre)
        {
            int subnodeIndex = -1;
            if (interval.Min >= centre) 
                subnodeIndex = 1;
            if (interval.Max <= centre) 
                subnodeIndex = 0;
            return subnodeIndex;
        }

        /// <summary>
        /// 
        /// </summary>
        private IList<T> _items = new List<T>();

        /// <summary>
        /// Subnodes are numbered as follows:
        /// 0 | 1        
        /// .
        /// </summary>
        protected Node<T>[] Subnode = new Node<T>[2];
        /*
        /// <summary>
        /// 
        /// </summary>
        protected NodeBase() { }
        */
        /// <summary>
        /// 
        /// </summary>
        public  IList<T> Items
        {
            get
            {                
                return _items;
            }
            protected set { _items = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public  void Add(T item)
        {
            _items.Add(item);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public  IList<T> AddAllItems(IList<T> items)
        {
            // items.addAll(this.items);
            foreach (T o in _items)
                items.Add(o);
            for (int i = 0; i < 2; i++)
                if (Subnode[i] != null)
                    Subnode[i].AddAllItems(items);                            
            return items;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="interval"></param>
        /// <returns></returns>
        protected abstract bool IsSearchMatch(Interval interval);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="interval"></param>
        /// <param name="resultItems"></param>
        /// <returns></returns>
        public  IList<T> AddAllItemsFromOverlapping(Interval interval, IList<T> resultItems)
        {
            if (!IsSearchMatch(interval))
                return _items;
            // resultItems.addAll(items);
            foreach (T o in _items)
                resultItems.Add(o);
            for (int i = 0; i < 2; i++)
                if (Subnode[i] != null)
                    Subnode[i].AddAllItemsFromOverlapping(interval, resultItems);                            
            return _items;
        }

        /// <summary>
        /// 
        /// </summary>
        public  int Depth
        {
            get
            {
                int maxSubDepth = 0;
                for (int i = 0; i < 2; i++)
                {
                    if (Subnode[i] != null)
                    {
                        int sqd = Subnode[i].Depth;
                        if (sqd > maxSubDepth)
                            maxSubDepth = sqd;
                    }
                }
                return maxSubDepth + 1;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public  int Count
        {
            get
            {
                int subSize = 0;
                for (int i = 0; i < 2; i++)
                    if (Subnode[i] != null)
                        subSize += Subnode[i].Count;
                return subSize + _items.Count;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public  int NodeCount
        {
            get
            {
                int subCount = 0;
                for (int i = 0; i < 2; i++)
                    if (Subnode[i] != null)
                        subCount += Subnode[i].NodeCount;
                return subCount + 1;
            }
        }
    }
}