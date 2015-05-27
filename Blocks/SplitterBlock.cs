using System.Collections;

namespace NoQL.CEP.Blocks
{
    public class SplitterBlock : AbstractBlock
    {
        public SplitterBlock(Processor p)
            : base(p)
        {
        }

        public override bool OnData(object data)
        {
            if (data is IEnumerable)
            {
                var newData = (data as IEnumerable);

                lock (newData)
                {
                    foreach (object d in newData)
                    {
                        SendToChildren(d);
                    }
                }
                return false;
            }
            else
            {
                SendToChildren(data);
            }
            return true;
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