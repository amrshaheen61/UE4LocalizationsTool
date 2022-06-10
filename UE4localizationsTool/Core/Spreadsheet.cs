using Helper.MemoryList;

namespace AssetParser
{
    public class Spreadsheet
    {
        public Spreadsheet(MemoryList memoryList, Uexp uexp, bool Modify = false)
        {
            memoryList.GetIntValue();//Null
            int TableCount = memoryList.GetIntValue();

            for (int TableIndex = 0; TableIndex < TableCount; TableIndex++)
            {
                _ = new StructProperty(memoryList, uexp, uexp.UassetData.UseFromStruct, false, Modify);
            }
        }
    }
}
