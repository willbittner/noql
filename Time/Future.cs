using System;
using System.Threading;

namespace NoQL.CEP.Time
{
    /// <summary>
    /// Class for providing operations in the future
    /// </summary>
    /// CITE: http://stackoverflow.com/questions/3756038/c-sharp-execute-action-after-x-seconds
    public static class Future
    {
        #region Members

        /// <summary>
        /// Specifies the method that will be fired to execute the delayed anonymous method.
        /// </summary>
        private readonly static TimerCallback timer = new TimerCallback(Future.ExecuteDelayedAction);

        #endregion Members

        #region Methods

        /// <summary>
        /// Method that executes an anonymous method after a delay period.
        /// </summary>
        /// <param name="action">The anonymous method that needs to be executed.</param>
        /// <param name="delay">The period of delay to wait before executing.</param>
        /// <param name="interval">The period (in milliseconds) to delay before executing the anonymous method again (Timeout.Infinite to disable).</param>
        public static void Do(Action action, TimeSpan delay, int interval = Timeout.Infinite)
        {
            // create a new thread timer to execute the method after the delay
            new Timer(timer, action, Convert.ToInt32(delay.TotalMilliseconds), interval);
        }

        /// <summary>
        /// Method that executes an anonymous method after a delay period.
        /// </summary>
        /// <param name="action">The anonymous method that needs to be executed.</param>
        /// <param name="delay">The period of delay (in milliseconds) to wait before executing.</param>
        /// <param name="interval">The period (in milliseconds) to delay before executing the anonymous method again (Timeout.Infinite to disable).</param>
        public static void Do(Action action, int delay, int interval = Timeout.Infinite)
        {
            Do(action, TimeSpan.FromMilliseconds(delay), interval);
        }

        /// <summary>
        /// Method that executes an anonymous method after a delay period.
        /// </summary>
        /// <param name="action">The anonymous method that needs to be executed.</param>
        /// <param name="dueTime">The due time when this method needs to be executed.</param>
        /// <param name="interval">The period (in milliseconds) to delay before executing the anonymous method again (Timeout.Infinite to disable).</param>
        public static void Do(Action action, DateTime dueTime, int interval = Timeout.Infinite)
        {
            if (dueTime < DateTime.Now)
            {
                throw new ArgumentOutOfRangeException("dueTime", "The specified due time has already elapsed.");
            }

            Do(action, dueTime - DateTime.Now, interval);
        }

        /// <summary>
        /// Method that executes a delayed action after a specific interval.
        /// </summary>
        /// <param name="o">The Action delegate that is to be executed.</param>
        /// <remarks>This method is invoked on its own thread.</remarks>
        private static void ExecuteDelayedAction(object o)
        {
            // invoke the anonymous method
            (o as Action).Invoke();

            return;
        }

        #endregion Methods
    }
}