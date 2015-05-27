using System.Collections.Generic;

namespace NoQL.CEP.NewExpressions
{
    public interface INewComponentManager
    {
        // will be deprecated asap
        INewComponent Get(string name);

        INewComponent Get(INewComponent comp);

        List<INewComponent> GetAll();

        void Register(string name, int protectionCode);
    }
}