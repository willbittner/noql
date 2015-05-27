using NoQL.CEP.Blocks;
using System;

namespace NoQL.CEP.NewExpressions
{
    public interface INewComponent
    {
        AbstractBlock InputBlock { get; set; }

        AbstractBlock OutputBlock { get; set; }

        void Attach(INewComponent component);

        void Send(object data);

        void OnReceive<T>(Action<T> recvFunc);

        string ComponentName { get; set; }

        int ID { get; set; }

        int CepID { get; set; }

        void Detach(INewComponent component);
    }
}