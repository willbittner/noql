using NoQL.CEP;
using NoQL.CoreYoloQL;
using System;

namespace NoQL.CoreCEP.Datastructures
{
    public class DatabaseKeys
    {
        private Processor proc;

        public DatabaseKeys(Processor p)
        {
            proc = p;
        }

        public void Init()
        {
            /**  NewRegisterKey<IParentOrder>(obj => obj.ID, "ID", DatabaseList.ParentOrders);
              //NewRegisterKey<ParentOrder>(obj => obj.ParentRefID,"ParentRefID",DatabaseList.ParentOrders);
              NewRegisterKey<IParentOrder>(obj => obj.Route.ID, "Route", DatabaseList.ParentOrders);
              NewRegisterKey<IChildOrder>(obj => obj.ID, "ID", DatabaseList.ChildOrders);
              NewRegisterKey<IChildOrder>(obj => obj.StreetRoute, "StreetRoute", DatabaseList.ChildOrders);
              NewRegisterKey<IChildOrder>(obj => obj.ParentRefID, "ParentRefID", DatabaseList.ChildOrders);
              NewRegisterKey<ParentRoute>(obj => obj.Name, "Name", DatabaseList.Routes);
              NewRegisterKey<ParentRoute>(obj => obj.ID, "ID", DatabaseList.Routes);
              NewRegisterKey<IVolumeCurve>(obj => obj.CurveID, "CurveID", DatabaseList.VolumeCurves);

              NewRegisterKey<QuickRisk>(obj => obj.ParentRefID, "ParentRefID", DatabaseList.QuickRisk);

              NewRegisterKey<ParentOrderContextWrapper>(obj => obj.Parent.ID, "ParentRefID", DatabaseList.ParentOrderContextWrappers);
              NewRegisterKey<RiskManagerRiskUpdate>(obj => obj.ParentRefID, "ParentRefID", DatabaseList.RiskUpdates);
              //RegisterKey<ParentRoute>("ID",DatabaseList.Routes);
              //RegisterKey<ParentRoute>("Name",DatabaseList.Routes);**/
        }

        public void NewRegisterKey<T>(Func<T, object> keySelectFunc, string name, string dbName)
        {
            if (proc == null) throw new Exception("Processor not defined, cannot register key");
            proc.GetRamDb(dbName).CreateIndex(name, keySelectFunc);
        }

        public void RegisterKey<T>(string KeyName, string dbName)
        {
            if (proc == null) throw new Exception("Processor not defined, cannot register key");
            if (typeof(T).GetProperty(KeyName) == null)
            {
                if (typeof(T).GetField(KeyName) == null)
                {
                    throw new Exception("Filed and Property not found in key register " + KeyName + " Type: " + typeof(T).Name);
                }
                proc.GetRamDb(dbName).CreateIndex<T>(KeyName, keyobj => typeof(T).GetField(KeyName).GetValue(keyobj));
            }
            else
            {
                proc.GetRamDb(dbName).CreateIndex<T>(KeyName, keyobj => typeof(T).GetProperty(KeyName).GetValue(keyobj));
            }
        }
    }
}