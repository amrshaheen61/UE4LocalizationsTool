using Helper.MemoryList;
using System;
using System.Collections.Generic;
using System.Text;

namespace AssetParser
{
    public class REDAdvTextData
    {

        int StartOffset;
        int ATFBlockHeaderSize;
        int UncompressedSize;
        int CompressedSize;
        string MagicId;
        int StringCount;

        public REDAdvTextData(MemoryList memoryList, Uexp uexp, bool Modify = false)
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

            MemoryList ATFBlock = new MemoryList(memoryList.GetBytes(UncompressedSize));

            MagicId = ATFBlock.GetStringValueN();

            if (MagicId != "ATF")
            {
                throw new Exception("Can't Parse this file.\nSend this file to author.");
            }

            StringCount = ATFBlock.GetIntValue();
            ATFBlock.Skip(8); //Null

            int StringsIdPosition = ATFBlock.GetPosition();
            int StringsIdOffset = ATFBlock.GetIntValue();
            int StringsIdCount = ATFBlock.GetIntValue();
            int StringsIdSize = ATFBlock.GetIntValue();
            ATFBlock.Skip(4); //Null
            byte[] IdBlock = ATFBlock.GetBytes(StringsIdSize, false, StringsIdOffset);

            int StringsInfoPosition = ATFBlock.GetPosition();
            int StringsInfoOffset = ATFBlock.GetIntValue();
            int StringsInfoCount = ATFBlock.GetIntValue();
            int StringsInfoSize = ATFBlock.GetIntValue();
            ATFBlock.Skip(4); //Null            
            MemoryList StringsInfo = new MemoryList(ATFBlock.GetBytes(StringsInfoSize, false, StringsInfoOffset));

            int StringsTagsPosition = ATFBlock.GetPosition();
            int StringsTagsOffset = ATFBlock.GetIntValue();
            int StringsTagsSize = ATFBlock.GetIntValue();
            _ = ATFBlock.GetIntValue();//StringsTagsSize
            ATFBlock.Skip(4); //Null
            MemoryList StringsTags = new MemoryList(ATFBlock.GetBytes(StringsTagsSize, false, StringsTagsOffset));

            int StringsBlockPosition = ATFBlock.GetPosition();
            int StringsBlockOffset = ATFBlock.GetIntValue();
            int StringLenght = ATFBlock.GetIntValue();
            int StringsBlockSize = ATFBlock.GetIntValue();
            ATFBlock.Skip(4); //Null
            MemoryList StringsBlock = new MemoryList(ATFBlock.GetBytes(StringsBlockSize, false, StringsBlockOffset));

            ATFBlockHeaderSize = ATFBlock.GetPosition();

            if (!Modify)
            {
                for (int n = 0; n < StringCount; n++)
                {
                    int tagsOffsets = StringsInfo.GetIntValue();
                    int tagsLength = StringsInfo.GetIntValue();
                    StringsInfo.Skip(8);//Null
                    int stringOffsets = StringsInfo.GetIntValue();
                    int stringLength = StringsInfo.GetIntValue();
                    StringsInfo.Skip(8);//Null

                    //  Console.WriteLine(n+": " + tagsOffsets +" - "+ tagsLength + " - "+ stringOffsets + " - "+ stringLength);
                    uexp.Strings.Add(new List<string>() { StringsTags.GetStringUE(tagsLength, false, tagsOffsets), StringsBlock.GetStringUE(stringLength * 2, false, stringOffsets * 2, System.Text.Encoding.Unicode) });

                }
            }
            else
            {
                StringsBlock.Clear();
                for (int n = 0; n < StringCount; n++)
                {

                    byte[] StrBytes = Encoding.Unicode.GetBytes(AssetHelper.ReplaceBreaklines(uexp.Strings[uexp.CurrentIndex][1] + '\0', true));
                    uexp.CurrentIndex++;

                    StringsInfo.GetIntValue(); //tagsOffsets
                    StringsInfo.GetIntValue();//tagsLength
                    StringsInfo.Skip(8);//Null
                    StringsInfo.SetIntValue(StringsBlock.GetSize() / 2); //stringOffsets
                    StringsInfo.SetIntValue((StrBytes.Length - 2) / 2);
                    StringsInfo.Skip(8);//Null
                    StringsBlock.Add(StrBytes);
                }



                ATFBlock.SetSize(ATFBlockHeaderSize);


                ATFBlock.Seek(StringsIdPosition);
                ATFBlock.SetIntValue(ATFBlock.GetSize());
                ATFBlock.Skip(4);
                ATFBlock.SetIntValue(IdBlock.Length);
                ATFBlock.Add(IdBlock);


                ATFBlock.Seek(StringsInfoPosition);
                ATFBlock.SetIntValue(ATFBlock.GetSize());
                ATFBlock.Skip(4);
                ATFBlock.SetIntValue(StringsInfo.GetSize());
                ATFBlock.Add(StringsInfo.ToArray());


                ATFBlock.Seek(StringsTagsPosition);
                ATFBlock.SetIntValue(ATFBlock.GetSize());
                ATFBlock.SetIntValue(StringsTags.GetSize());
                ATFBlock.SetIntValue(StringsTags.GetSize());
                ATFBlock.Add(StringsTags.ToArray());

                ATFBlock.Seek(StringsBlockPosition);
                ATFBlock.SetIntValue(ATFBlock.GetSize());
                ATFBlock.SetIntValue(StringsBlock.GetSize() / 2);
                ATFBlock.SetIntValue(StringsBlock.GetSize());
                ATFBlock.Add(StringsBlock.ToArray());



                memoryList.SetSize(StartOffset);
                memoryList.Add(ATFBlock.ToArray());


                memoryList.Seek(ValuesPosition);
                memoryList.SetIntValue(ATFBlock.GetSize());
                memoryList.SetIntValue(ATFBlock.GetSize());


            }


        }




    }
}
