namespace NoQL.CEP
{
    public class RamDBFactory
    {
        public static IRamDB NewRamDb()
        {
            return new NoQL.CEP.RamDB();
        }
    }
}