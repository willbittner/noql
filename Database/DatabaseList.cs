using NoQL.CEP;
using NoQL.CoreCEP.Datastructures;

namespace NoQL.CoreYoloQL
{
    public static class DatabaseList
    {
//        public static string ChildOrders = "ChildOrders";


        public static void InitDatabases(Processor p)
        {
//            p.CreateRamDB<QuickRisk>(QuickRisk);

            var keys = new DatabaseKeys(p);
            keys.Init();
        }
    }
}