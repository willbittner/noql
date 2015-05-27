using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace NoQL.CEP.YoloQLBlocks
{
    public class TestRamDB : IRamDB
    {
        private DataSet data;
        private ServiceStack.Net30.Collections.Concurrent.ConcurrentDictionary<Type, DataTable> tables = new ServiceStack.Net30.Collections.Concurrent.ConcurrentDictionary<Type, DataTable>();
        private Dictionary<Type, List<DataColumn>> keys = new Dictionary<Type, List<DataColumn>>();
        private Dictionary<Type, List<string>> idxNames = new Dictionary<Type, List<string>>();
        private Dictionary<Type, Dictionary<string, Func<object, object>>> keyselector = new Dictionary<Type, Dictionary<string, Func<object, object>>>();
        private object updatelock = new object();
        private static string DATAFIELD = "pirate__data";

        public TestRamDB()
        {
            data = new DataSet();
        }

        public void Add<T>(T obj)
        {
            //try
            //{
            //    var cols = keys[typeof(T)];
            //    var table = tables[typeof(T)];
            //    var row = table.NewRow();
            //    foreach (var name in idxNames[typeof(T)])
            //    {
            //        var func = keyselector[typeof(T)][name];

            //        row[name] = func(obj);

            //    }
            //    row[DATAFIELD] = obj;
            //    table.Rows.Add(row);
            //}

            //catch (Exception e)
            //{
            Update(obj, UpdatePolicy.UPDATE_OR_INSERT);
            //}
        }

        public string CreateIndex<T>(Func<T, object> keyFunction)
        {
            throw new NotImplementedException();
        }

        public void CreateIndex<T>(string indexName, Func<T, object> keyFunction)
        {
            lock (updatelock)
            {
                var col = new DataColumn(indexName);
                col.Unique = false;

                if (keys[typeof(T)] == null) keys[typeof(T)] = new List<DataColumn>();
                if (!idxNames.ContainsKey(typeof(T))) idxNames[typeof(T)] = new List<string>();
                idxNames[typeof(T)].Add(indexName);
                keys[typeof(T)].Add(col);
                if (!tables[typeof(T)].Columns.Contains(DATAFIELD))
                {
                    var datacol = new DataColumn(DATAFIELD, typeof(T));
                    tables[typeof(T)].Columns.Add(datacol);
                }
                if (!keyselector.ContainsKey(typeof(T))) keyselector[typeof(T)] = new Dictionary<string, Func<object, object>>();
                keyselector[typeof(T)][indexName] = new Func<object, object>(obj => keyFunction((T)obj));
                tables[typeof(T)].Columns.Add(col);
                tables[typeof(T)].PrimaryKey = keys[typeof(T)].ToArray();
            }
        }

        public void Delete<T>(T delObj)
        {
            lock (updatelock)
            {
                string expr = "";
                foreach (var idxsel in keyselector[typeof(T)])
                {
                    if (expr != "") expr += " AND ";
                    var name = idxsel.Key;
                    var func = idxsel.Value;
                    var val = func(delObj);

                    expr += " " + name + " = '" + val.ToString() + "' ";
                }

                var aen = tables[typeof(T)].Select(expr);
                foreach (var arow in aen)
                {
                    arow.Delete();
                }
            }
        }

        public void Delete<T>(IEnumerable<T> deletionSet)
        {
            foreach (var obj in deletionSet)
            {
                Delete(obj);
            }
        }

        public IEnumerable<T> GetEnumerable<T>()
        {
            lock (updatelock)
            {
                var aen = tables[typeof(T)].Select();
                var datas = aen.Select(x => x[DATAFIELD]);
                return (IEnumerable<T>)datas;
            }
        }

        public System.Collections.IEnumerable GetEnumerable(Type t)
        {
            var aen = tables[t].Select();
            var datas = aen.Select(x => x[DATAFIELD]);
            return datas;
        }

        public void Update<T>(T obj, UpdatePolicy policy)
        {
            Delete(obj);
            lock (updatelock)
            {
                var cols = keys[typeof(T)];
                var table = tables[typeof(T)];
                var row = table.NewRow();
                foreach (var name in idxNames[typeof(T)])
                {
                    var func = keyselector[typeof(T)][name];

                    row[name] = func(obj);
                }
                row[DATAFIELD] = obj;
                table.Rows.Add(row);
            }
        }

        public IEnumerable<T> GetEnumerable<T>(string ixName, object ixValue)
        {
            lock (updatelock)
            {
                List<T> retList = new List<T>();
                var exp = ixName + " = " + "'" + ixValue.ToString() + "'";
                var aen = tables[typeof(T)].Select(exp);

                foreach (var arow in aen)
                {
                    retList.Add((T)arow[DATAFIELD]);
                }
                return retList;
            }
        }

        public void Init<T>()
        {
            CheckForExist<T>();
        }

        private void CheckForExist<T>()
        {
            if (!tables.ContainsKey(typeof(T)))
            {
                tables[typeof(T)] = new DataTable();
                keys[typeof(T)] = new List<DataColumn>();
            }
        }

        public ArrayList GetEnumerable()
        {
            var ret = new ArrayList();
            foreach (var t in tables.Keys)
            {
                var inner = GetEnumerable(t);
                foreach (var value in inner)
                {
                    ret.Add(value);
                }
            }

            return ret;
        }
    }
}