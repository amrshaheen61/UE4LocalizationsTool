using Helper.MemoryList;

namespace AssetParser
{
    public class DataTable
    {
        public DataTable(MemoryList memoryList, Uexp uexp, bool Modify = false)
        {
            memoryList.GetIntValue();//Null
            int TableCount = memoryList.GetIntValue();

            for (int TableIndex = 0; TableIndex < TableCount; TableIndex++)
            {
                memoryList.Skip(8); //no neeed
                _ = new StructProperty(memoryList, uexp, true, Modify);
            }
        }
    }
}
