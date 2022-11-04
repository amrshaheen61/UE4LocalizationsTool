using Helper.MemoryList;

namespace AssetParser
{
    public class MuseStringTable
    {

        public MuseStringTable(MemoryList memoryList, Uexp uexp, bool Modify = false)
        {
            memoryList.GetIntValue();//Null

            memoryList.GetStringUE();
            int TablesCount = memoryList.GetIntValue();
            for (int i = 0; i < TablesCount; i++)
            {
                new ReadStringProperty(memoryList, uexp, memoryList.GetStringUE(), Modify);
            }

            TablesCount = memoryList.GetIntValue();

            for (int i = 0; i < TablesCount; i++)
            {
                string TableName = memoryList.GetStringUE();
                int TableCount = memoryList.GetIntValue();
                for (int n = 0; n < TableCount; n++)
                {
                    memoryList.Skip(8);
                    new ReadStringProperty(memoryList, uexp, TableName, Modify);
                }
            }
        }


    }
}
