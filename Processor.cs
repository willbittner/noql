using NoQL.CEP.Blocks;
using NoQL.CEP.Blocks.Factories;
using NoQL.CEP.Datastructures;
using NoQL.CEP.JobManagers;
using NoQL.CEP.Logging;
using NoQL.CEP.NewExpressions;
using NoQL.CEP.Profiling;
using NoQL.CEP.Time;
using NoQL.CEP.YoloQLBlocks;

/***
Copyright 2015 William K. Bittner

This file is part of NoQL.

NoQL is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

NoQL is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with NoQL. If not, see http://www.gnu.org/licenses/.
***/

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace NoQL.CEP
{
    /// <summary>
    ///     NoQL Complex Event Processor
    /// </summary>
    public class Processor
    {
        #region FilterStrategyEnum enum

        public enum FilterStrategyEnum
        {
            EARLY,
            LATE
        }

        #endregion FilterStrategyEnum enum

        /// <summary>
        ///     This holds all of the Instances of RamDB
        /// </summary>
        internal static ConcurrentDictionary<string, IRamDB> RamDatabases = new ConcurrentDictionary<string, IRamDB>();

        //internal FilterStrategyEnum FilterStrategy = FilterStrategyEnum.EARLY;
        public int ID = 0;

        public ScriptManager ScriptsManager;

        public Func<AbstractBlock, Exception, bool> ErrorHandler { get; set; }

        /// <summary>
        ///     The Historical data, contains all objects collected
        /// </summary>
        public IRamDB HistoricalData { get; set; }

        public IJobManager JobManager { get; set; }

        public ITimeProvider TimeProvider { get; set; }

        public ILogProvider LoggingProvider { get; set; }

        public static IProfilingProvider ProfilingProvider { get; set; }

        public static ObjectPool ObjectPool { get; set; }

        public ConcurrentDictionary<string, IRamDB> RamDatabase
        {
            get { return RamDatabases; }
        }

        /// <summary>
        ///     List of open threads
        /// </summary>
        private Thread[] Threads { get; set; }

        static Processor()
        {
            ProfilingProvider = new DefaultProfilingProvider();
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="threadCount">Number of threads that this CEP will use</param>
        public Processor(int threadCount, int id)
        {
            TimeProvider = new DefaultTimeProvider();
           // var update = new TimedUpdated();
            JobManager = new SimpleJobManager();
            LoggingProvider = new ConsoleLoggingProvider();
            ID = id;
            Threads = new Thread[threadCount];
            HistoricalData = new TestRamDB();
            ObjectPool = new ObjectPool();

            Express2.BlockFactory = CreateBlockFactory();
            //ScriptsManager = new ScriptManager(@"..\..\..\Dashx.CoreCEP.Scripts\Scripts");

            for (int i = 0; i < threadCount; i++)
            {
                Threads[i] = new Thread(ServiceLoop);
                Threads[i].Name = "CEP Worker " + i;
                Threads[i].Start();
            }

            Console.WriteLine(" CEPID: {0} - Num Threads: {1}", ID, threadCount);
            //System.Timers.Timer timer1 = new System.Timers.Timer();
            //timer1.Interval = 3000;
            //timer1.Elapsed += new ElapsedEventHandler(PrintQueueSize);
            //timer1.Start();
            ErrorHandler = (block, exception) =>
                           {
                               if (exception is ThreadAbortException)
                                   return false;
                               throw new Exception("*****Error in block: " + block.DebugName + " Exception: " + exception.Message); //+ "\n\r--\n\r" + exception.StackTrace);
                               return true;
                           };
            //  System.Threading.Timer timer1 = new System.Threading.Timer(PrintQueueSize,null,TimeSpan.FromMilliseconds(0),TimeSpan.FromMilliseconds(1000));
        }

        public BlockFactory CreateBlockFactory()
        {
            return new BlockFactory(this);
        }

        public IRamDB CreateRamDB<T>(string dbName, int preAllocSize = 0)
        {
            var retDb = new TestRamDB();
            if (!RamDatabase.TryAdd(dbName, retDb))
            {
                //throw new Exception("Database already exists: " + dbName);
                return RamDatabase[dbName];
            }
            retDb.Init<T>();
            return retDb;
        }

        public IRamDB GetRamDb(string dbName)
        {
            IRamDB dbOut;
            if (!RamDatabase.TryGetValue(dbName, out dbOut))
            {
                throw new Exception("Tried to access a RamDB that doesn't exist: " + dbName);
            }
            return dbOut;
        }

        /// <summary>
        ///     Consumer loop to operate jobs (several threads can run this loop)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining, MethodCodeType = MethodCodeType.IL)]
        public void ServiceLoop()
        {
            // ((SwollenSachs)JobManager).RegisterSackLicker();
            //GCSettings.LatencyMode = GCLatencyMode.Batch;
            while (true)
            {
                try
                {
                    Job job;
                    while ((job = JobManager.Next()) != null)
                    {
                        job.Accept(job.Data);
                    }
                }
                catch (ThreadAbortException e)
                {
                    //Console.WriteLine("Service loop abort exception: " + e.Message + "\n\r" + e.StackTrace);
                    return;
                }
                catch (Exception e)
                {
                    Debug.Print("In Processor, exception in service loop: " + e);
                    //throw new Exception("In Processor, problem in the service loop");
                }
            }
        }

        /// <summary>
        ///     Immediately Shutsdown the CEP processing. This action can not be undone.
        /// </summary>
        public void Shutdown()
        {
            foreach (Thread thread in Threads.Where(thread => thread != null))
            {
                thread.Abort();
            }
        }

        /// <summary>
        ///     Shutsdown the CEP after all jobs are completed. This action can not be undone.
        /// </summary>
        public void SpinWaitShutdown()
        {
        // I should be shot and killed for using a goto
        ghettoJump:
            while (JobManager.Size > 0)
                Thread.Yield();

            Thread.Sleep(3000);
            if (JobManager.Size > 0) goto ghettoJump;

            Shutdown();
        }

        public void WaitForNone()
        {
        // I should be shot and killed for using a goto

            ///Damn right you should, but I'm gonna go ahead and copy it.
        ghettoJump:
            while (JobManager.Size > 0)
                Thread.Yield();

            Thread.Sleep(3000);
            if (JobManager.Size > 0) goto ghettoJump;
        }

        public void WaitUntilExit()
        {
            foreach (Thread thread in Threads.Where(thread => thread != null))
            {
                thread.Join();
            }
        }
    }
}