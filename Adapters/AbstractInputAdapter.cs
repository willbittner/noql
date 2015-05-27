namespace NoQL.CEP.Adapters
{
    /// <summary>
    ///     Represents an abstract input adapter that is typed
    /// </summary>
    /// <typeparam name="T"> The type of data that this input adapter produces</typeparam>
    public abstract class AbstractInputAdapter<T> : IAdapter
    {
        #region Delegates

        public delegate void NewDataDelegate(T data);

        #endregion Delegates

        public event NewDataDelegate NewData;

        public AbstractInputAdapter(Processor p)
        {
            CEP = p;
        }

        public void OnNewData(T data)
        {
            if (NewData != null)
            {
                NewData(data);
            }
        }

        #region IAdapter Members

        public Processor CEP { get; set; }

        #endregion IAdapter Members
    }
}