using NoQL.CEP.Blocks;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace NoQL.CEP.YoloQLBlocks
{
    public class EventWindowBlock<T> : AbstractBlock
    {
        private ConcurrentQueue<T> queue = new ConcurrentQueue<T>();
        private int windowSize;

        public EventWindowBlock(Processor p, int windowSize)
            : base(p)
        {
            this.windowSize = windowSize;
        }

        public override bool OnData(object data)
        {
            queue.Enqueue((T)data);
            while (queue.Count > windowSize)
            {
                T tempout;
                queue.TryDequeue(out tempout);
            }
            List<T> dlist = queue.ToList();
            SendToChildren(dlist);

            return false;
        }

        public override System.Type BlockInputType
        {
            get { throw new System.NotImplementedException(); }
        }

        public override System.Type BlockOutputType
        {
            get { throw new System.NotImplementedException(); }
        }
    }
}