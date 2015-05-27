using NoQL.CEP.Blocks;
using NoQL.CEP.Datastructures;
using NoQL.CEP.Logging;
using System;
using System.Collections.Generic;

namespace NoQL.CEP.NewExpressions
{
    public interface INewCEPExpression<InputType> : INewComponent, INewCEPExpression
    {
        AbstractBlock InputBlock { get; set; }

        AbstractBlock OutputBlock { get; set; }

        INewCEPExpression<InputType> Seal();

        [Obsolete("Use non reference based Overriden version")]
        INewCEPExpression<InputType> Branch<InputType>(out INewCEPExpression<InputType> expr);

        INewCEPExpression<InputType> Branch<InputType>(INewCEPExpression<InputType> expr);

        INewCEPExpression<InputType> Branch();

        INewCEPExpression<InputType> Count();

        INewCEPExpression<InputType> Delete(string DatabaseName);

        INewCEPExpression<InputType> Express();

        INewCEPExpression<IDictionary<IndexType, DataType>> GroupBy<DataType, IndexType>(Func<DataType, IndexType> groupFunc);

        INewCEPExpression<InputType> Name<T>(string name);

        INewCEPExpression<InputType> Name(INewComponent name);

        INewCEPExpression<InputType> Name<T>(INewComponent name);

        INewCEPExpression<InputType> Name(string name);

        INewCEPExpression<InputType> NotIn(INewCEPExpression<InputType> expr);

        INewCEPExpression<InputType> OnError(Action<object, Exception> ErrorAction, bool bubbleToProcessor = false);

        INewCEPExpression<InputType> Log(LogSeverity severity, string message);

        INewCEPExpression<InputType> Perform(Action<InputType> action);

        INewCEPExpression<Pair<DateTime, InputType>> Time();

        INewCEPExpression<KeyValuePair<InputType, IEnumerable<OutputType>>> Query<OutputType>(string databaseName, string indexName, Func<InputType, object> indexFunc);

        INewCEPExpression<InputType> Save<InputType>(string DatabaseName);

        INewCEPExpression<InputType> Update<InputType>(string DatabaseName, UpdatePolicy policy);

        INewCEPExpression<OutputType> Select<OutputType>(Func<InputType, OutputType> selectFunc, string name = "");

        INewCEPExpression<SingletonType> Split<SingletonType>();

        INewCEPExpression<OutputType> Sum<OutputType>(Func<InputType, OutputType, OutputType> sumFunc) where OutputType : new();

        INewCEPExpression<decimal> SumDecimal();

        INewCEPExpression<int> SumInt();

        INewCEPExpression<long> SumLong();

        INewCEPExpression<InputType> Where(Func<InputType, bool> whereFunc);

        INewCEPExpression<InputType> Where(Func<InputType, bool> whereFunc, INewCEPExpression<InputType> elseBranch);

        [Obsolete("Use non reference based Overriden version")]
        INewCEPExpression<InputType> Where(Func<InputType, bool> whereFunc, out INewCEPExpression<InputType> elseBranch);

        INewCEPExpression<IEnumerable<InputType>> Window(TimeSpan span);

        INewCEPExpression<IEnumerable<InputType>> Window(int numEvents);

        INewCEPExpression<Pair<InputType, OtherType>> Merge<OtherType>(INewCEPExpression<OtherType> otherExpression, Func<Pair<InputType, OtherType>, bool> mergeOn = null, bool waitForMerge = true);

        INewCEPExpression<object> Timer(double intervalMs);

        INewCEPExpression<InputType> Delay(double intervalMs);

        INewCEPExpression<Pair<IEnumerable<InputType>, TriggerType>> IndexTrigger<TriggerType>(INewCEPExpression<TriggerType> TriggerExpr, Func<InputType, TriggerType, bool> MatchFunction, bool deleteOnMatch = true);

        INewCEPExpression<Pair<IEnumerable<SingletonType>, SingletonType>> Explode<SingletonType>();

        INewCEPExpression<IEnumerable<SingletonType>> Implode<SingletonType>();
    }

    public interface INewCEPExpression
    {
    }
}