using Helper.MemoryList;
using System.Collections.Generic;
namespace AssetParser
{
    public class StringTable
    {

        public StringTable(MemoryList memoryList, Uexp uexp, bool Modify = false)
        {

            memoryList.GetIntValue();//Null
            int TableNameLenght = memoryList.GetIntValue();
            string TableName = memoryList.GetStringValue(TableNameLenght);
            int TableCount = memoryList.GetIntValue();

            for (int TableIndex = 0; TableIndex < TableCount; TableIndex++)
            {
                string TableId = memoryList.GetStringUE();
                if (!Modify)
                {
                    string TableValue = memoryList.GetStringUE();

                    uexp.Strings.Add(new List<string>() { TableId, TableValue });
                }
                else
                {
                    memoryList.ReplaceStringUE(uexp.Strings[uexp.CurrentIndex][1]);
                    uexp.CurrentIndex++;
                }
            }

        }
    }
}
