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

                //for DRAGON.QUEST.XI.S.Echoes.of.an.Elusive.Age.Definitive.Edition

#if false 
                string Name = uexp.UassetData.GetPropertyName(memoryList.GetIntValue());
                memoryList.Skip(4);//null or something
                int ThisPosition = memoryList.GetPosition();
                memoryList.Skip(4);//Block size
                new ReadStringProperty(memoryList, uexp, Name, Modify);
                int ThisPositionAfterEdit = memoryList.GetPosition();
                memoryList.Skip(13);//Block size



                if (Modify)
                {
                    memoryList.SetIntValue((ThisPositionAfterEdit - ThisPosition - 4) + 13, false, ThisPosition);
                }

#else


                memoryList.Skip(8); //no neeed
                _ = new StructProperty(memoryList, uexp, uexp.UassetData.UseFromStruct, false, Modify);
#endif


            }
        }
    }
}
