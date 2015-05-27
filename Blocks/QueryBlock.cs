using NoQL.CEP.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NoQL.CEP.Blocks
{
    /// <summary>
    ///     Query Block Queries an In-Ram databse and passes the child objects to its child cep-blocks
    /// </summary>
    /// <typeparam name="EventInputType">The expected INPUT event type </typeparam>
    /// <typeparam name="DatabaseQueryType">
    ///     The type of object to be from the database
    ///     (the database is partitioned by object type)
    /// </typeparam>
    /// <typeparam name="QueryReturnType">
    ///     The type of object RETURNED from the query function
    /// </typeparam>
    public class QueryBlock<EventInputType, DatabaseQueryType, QueryReturnType> : AbstractBlock
    {
        public Func<EventInputType, object> KeyFunc;
        public string KeyName = String.Empty;

        private IRamDB Database { get; set; }

        /// <summary>
        ///     Query function gets an un-enumerated ENUMERABLE of all type of DatabaseQueryType that's IN the database
        ///     and the event passed to THIS Block from the parent Block. Query the Enumerable and return a value of type
        ///     QueryReturnType
        /// </summary>
        private Func<EventInputType, IEnumerable<DatabaseQueryType>, IEnumerable<QueryReturnType>> QueryFunction { get; set; }

        private Func<EventInputType, DatabaseQueryType, QueryReturnType> QuerySingleFunction { get; set; }

        internal QueryBlock(Processor p, IRamDB database, Func<EventInputType, IEnumerable<DatabaseQueryType>, IEnumerable<QueryReturnType>> queryFunction)
            : base(p)
        {
            Database = database;
            QueryFunction = queryFunction;
        }

        internal QueryBlock(Processor p, IRamDB database, Func<EventInputType, DatabaseQueryType, QueryReturnType> queryFunction)
            : base(p)
        {
            Database = database;
            QuerySingleFunction = queryFunction;
        }

        public override bool OnData(object data)
        {
            if (!(data is EventInputType))
                throw new BlockTypeMismatchException(typeof(EventInputType), data.GetType(), this);

            Type t = data.GetType();

            object kv = null;

            if (string.IsNullOrEmpty(KeyName))
                throw new Exception("Must Set KeyName property in query Block");

            if (KeyFunc != null)
            {
                kv = KeyFunc((EventInputType)data);
            }
            else
            {
                throw new Exception("Query Block is Accessing DB with no key, we need data contracts with [Primary Key] etc");
            }

            if (QueryFunction != null)
            {
                SendToChildren(QueryFunction((EventInputType)data, Database.GetEnumerable<DatabaseQueryType>(KeyName, kv)));
            }
            else
            {
                SendToChildren(QueryFunction((EventInputType)data, Database.GetEnumerable<DatabaseQueryType>(KeyName, kv)).FirstOrDefault());
            }
            return false;
        }

        public override Type BlockInputType
        {
            get { throw new NotImplementedException(); }
        }

        public override Type BlockOutputType
        {
            get { throw new NotImplementedException(); }
        }
    }
}