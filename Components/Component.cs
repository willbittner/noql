using NoQL.CEP.Adapters;
using NoQL.CEP.Blocks;
using NoQL.CEP.YoloFactory;
using System;

namespace NoQL.CEP.Components
{
    public interface IComponent
    {
        Processor CEP { get; set; }

        string ComponentName { get; set; }

        AbstractBlock Attach(ICEPExpression expression);

        bool Detach(AbstractBlock blockRef);

        string GetComponentName();
    }

    public interface IComponent<DataType> : IComponent
    {
        AbstractInputAdapter<DataType> Attach(AbstractInputAdapter<DataType> adapter);

        AbstractBlock Attach(ICEPExpression expression);

        CEPExpression<DataType> Express(Action<object> outputFunction);

        CEPExpression<DataType> Express();

        void OnNewData(DataType data);
    }
}