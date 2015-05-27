using System;
using System.Runtime.CompilerServices;

namespace NoQL.CEP.Connections
{
    /// <summary>
    ///     Type Specific Filter (See generic filter). Ensure type-safeness
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Filter<T>
    {
        public Func<T, bool> FitnessCriterion { get; set; }

        public Filter()
        {
        }

        public Filter(Func<T, bool> fitnessCriterion)
        {
            FitnessCriterion = fitnessCriterion;
        }

        public virtual bool CheckType(object data)
        {
            return data is T;
            //return type is T;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual bool IsFit(T data)
        {
            return FitnessCriterion == null || FitnessCriterion(data);
        }
    }

    public class StrictFilter<T> : Filter<T>
    {
        public bool CheckType(Type type)
        {
            return type.Name == typeof(T).Name;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool IsFit(T data)
        {
            return FitnessCriterion == null || FitnessCriterion(data);
        }
    }
}