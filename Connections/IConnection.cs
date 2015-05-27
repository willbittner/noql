using NoQL.CEP.Blocks;
using QuickGraph;
using System;

namespace NoQL.CEP.Connections
{
    public interface IConnection : IEdge<AbstractBlock>
    {
        object LastData { get; }

        Type GetDestinationType();

        void Emit(object data);

        void PrintPretty(string indent, bool last);

        bool Evaluate(object data);

        AbstractBlock Source { get; set; }

        AbstractBlock Target { get; set; }
    }
}