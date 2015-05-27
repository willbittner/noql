using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Timers;

namespace NoQL.CEP.Adapters
{
    public static class ObjectToStringKeyValueBuilder
    {
        public static Dictionary<string, string> Build<T>(T objIn)
        {
            var output = new Dictionary<string, string>();
            PropertyInfo[] properties = typeof(T).GetProperties();
            foreach (PropertyInfo property in properties)
            {
                object value = property.GetValue(objIn);
                object name = property.Name;
                if (value == null) output.Add(name.ToString(), "");
                else output.Add(name.ToString(), value.ToString());
            }
            return output;
        }
    }

    public class CSVFileOutputAdapter<ItemType> : IOutputAdapter<ItemType>
    {
        private bool hasWrittenCols;
        private ConcurrentBag<ItemType> items = new ConcurrentBag<ItemType>();

        private Timer timer1 = new Timer();
        private StreamWriter writer;

        public CSVFileOutputAdapter(string fileName)
        {
            writer = new StreamWriter(@"..\localstore\" + fileName + DateTime.Now.Millisecond + ".csv");

            timer1.Interval = 1000;
            timer1.Elapsed += WriteFunction;
            timer1.Start();
        }

        public void WriteFunction(object obj, ElapsedEventArgs args)
        {
            lock (items)
            {
                ItemType itemTaken;
                while (items.TryTake(out itemTaken))
                {
                    Dictionary<string, string> objkeyvalue =
                        ObjectToStringKeyValueBuilder.Build(itemTaken);
                    var builder = new StringBuilder();
                    var titleBuilder = new StringBuilder();
                    foreach (var pair in objkeyvalue)
                    {
                        builder.Append(pair.Value + ",");
                        if (!hasWrittenCols) titleBuilder.Append(pair.Key + ",");
                    }
                    builder.Insert(builder.Length - 1, "");
                    if (!hasWrittenCols) titleBuilder.Insert(builder.Length - 1, "");
                    if (!hasWrittenCols)
                    {
                        writer.WriteLine(titleBuilder.ToString());
                        hasWrittenCols = true;
                    }
                    string lineString = builder.ToString();
                    writer.WriteLine(builder.ToString());
                }
                writer.Flush();
            }
        }

        #region IOutputAdapter<ItemType> Members

        public void OnOutput(ItemType output)
        {
            items.Add(output);
        }

        #endregion IOutputAdapter<ItemType> Members
    }
}