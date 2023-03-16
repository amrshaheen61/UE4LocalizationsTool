using Helper.MemoryList;
using System;
using System.Collections.Generic;
using System.Text;

namespace AssetParser
{
    public class REDLibraryTextData
    {

        int StartOffset;
        int UncompressedSize;
        int CompressedSize;
        int StringCount;

        public REDLibraryTextData(MemoryList memoryList, Uexp uexp, bool Modify = false)
        {
            memoryList.GetIntValue();//Null

            memoryList.Skip(12); //Unknown
            memoryList.GetIntValue(); //Start Data offset
            memoryList.Skip(8); //Unknown2
            int ValuesPosition = memoryList.GetPosition();
            UncompressedSize = memoryList.GetIntValue();
            CompressedSize = memoryList.GetIntValue();
            if (UncompressedSize != CompressedSize)
            {
                throw new Exception("Can't Parse this file.");
            }
            memoryList.Skip(4); //Unknown3

            memoryList.GetIntValue();//Null

            StartOffset = memoryList.GetPosition();

            MemoryList Block = new MemoryList(memoryList.GetBytes(UncompressedSize));

            StringCount = Block.GetIntValue();


            string[] IdValues = new string[StringCount];


            for (int i = 0; i < StringCount; i++)
            {

                Block.GetIntValue(); //unknown maybe flag

                IdValues[i] = Block.GetStringValue(128, true, -1, Encoding.Unicode).Trim('\0');
            }

            Block.GetIntValue(); //unknown maybe flag also idk

            if (!Modify)
            {
                for (int i = 0; i < StringCount; i++)
                {
                    uexp.Strings.Add(new List<string>() { IdValues[i], Block.GetStringUE(Encoding.Unicode) });
                }
            }
            else
            {
                Block.SetSize(Block.GetPosition());

                for (int i = 0; i < StringCount; i++)
                {
                    Block.SetStringValueN(uexp.Strings[uexp.CurrentIndex][1]);
                    uexp.CurrentIndex++;
                }

                memoryList.Seek(ValuesPosition);
                memoryList.SetIntValue(Block.GetSize());
                memoryList.SetIntValue(Block.GetSize());

                memoryList.SetSize(StartOffset);

                memoryList.Seek(StartOffset);
                memoryList.Add(Block.ToArray());
            }


        }




    }
}
