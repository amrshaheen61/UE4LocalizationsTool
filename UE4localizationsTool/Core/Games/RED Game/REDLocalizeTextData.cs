using Helper.MemoryList;
using System;
using System.Collections.Generic;
using System.Text;

namespace AssetParser
{
    public class REDLocalizeTextData
    {
        public REDLocalizeTextData(MemoryList memoryList, Uexp uexp, bool Modify = false)
        {

            memoryList.GetIntValue();//null
            memoryList.Skip(4 + 8 + 4 /*uexp position*/ + 4 + 4);//Unkown
            int ValuesPosition = memoryList.GetPosition();
            int UncompressedSize = memoryList.GetIntValue();
            int CompressedSize = memoryList.GetIntValue();
            if (UncompressedSize != CompressedSize)
            {
                throw new Exception("Can't Parse this file.");
            }
            int StartPosition = memoryList.GetIntValue();// Uasset+Position 
            memoryList.Skip(4);
            StartPosition = memoryList.GetPosition();
            short Value = memoryList.GetShortValue();
            string[] String = memoryList.GetStringValue(UncompressedSize - 2, false, -1, Encoding.Unicode).Split(new string[] { "\r\n" }, StringSplitOptions.None);


            for (int n = 0; n < String.Length - 1; n = n + 2)
            {
                if (!Modify)
                {
                    uexp.Strings.Add(new List<string>() { String[n], String[n + 1] });
                }
                else
                {
                    String[n + 1] = uexp.Strings[uexp.CurrentIndex][1];
                    uexp.CurrentIndex++;
                }
            }

            if (Modify)
            {
                MemoryList memory = new MemoryList();

                memory.SetShortValue(Value);

                for (int n = 0; n < String.Length - 1; n++)
                {
                    memory.SetStringValue(String[n] + "\r\n", true, -1, Encoding.Unicode);
                }

                memoryList.SetSize(StartPosition);
                memoryList.Seek(StartPosition);
                memoryList.SetBytes(memory.ToArray());

                memoryList.Seek(ValuesPosition);
                memoryList.SetIntValue(memory.GetSize());
                memoryList.SetIntValue(memory.GetSize());
            }



        }






    }
}
