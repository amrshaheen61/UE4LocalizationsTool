using Helper.MemoryList;
using System;
using System.Collections.Generic;
using System.Text;

namespace AssetParser
{
    public class OctopathTraveler
    {
        //Octopath Traveler: Champions of the Continent
        readonly MemoryList MemoryData;
        public OctopathTraveler(MemoryList memoryList, string PropertyName, Uexp uexp, bool Modify = false)
        {
            MemoryData = memoryList;
            if (GetString() != "m_DataList")
            {
                throw new Exception("Not supported data type: !m_DataList");
            }
            MemoryData.Skip(1);//0xdc
            int Count = MemoryData.GetUShortValue(true, -1, Endian.Big);


            for (int i = 0; i < Count; i++)
            {
                while (GetString() != "m_gametext")
                {
                    SkipIds();
                }

                for (int n = 0; n < 12; n++)
                {
                    string value = GetString();
                    if (value == null) continue;

                    if (!Modify)
                    {
                        uexp.Strings.Add(new List<string>() { "m_gametext", AssetHelper.ReplaceBreaklines(value) });
                        ConsoleMode.Print(uexp.Strings[uexp.Strings.Count - 1][1], ConsoleColor.Magenta);
                    }
                    else
                    {
                        string replacevalue = AssetHelper.ReplaceBreaklines(uexp.Strings[uexp.CurrentIndex++][1], true);
                        if (value != replacevalue)
                            ReplaceString(replacevalue);
                    }
                }

            }


        }



        private int StringOffset;
        private string GetString(int m_size)
        {
            return MemoryData.GetStringValue(m_size, true, -1, Encoding.UTF8);
        }

        private void ReplaceString(string value)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            int StringLenght = MemoryData.GetPosition() - StringOffset;
            MemoryData.Seek(StringOffset);
            MemoryData.DeleteBytes(StringLenght);

            MemoryData.InsertByteValue(0xda);
            MemoryData.InsertShortValue((short)bytes.Length, true, -1, Endian.Big);
            MemoryData.InsertBytes(bytes);
        }


        private void SkipIds()
        {
            byte value = MemoryData.GetByteValue();//unsigned byte

            if (value >> 4 == 0xd)
            {
                byte casebyte = (byte)(value & 0xf);
                //this should be m_id value (byte - word - dword) -> big endian
                //0 -> dword
                if (casebyte != 0)
                    MemoryData.Skip(sizeof(short) * casebyte);
                else
                    MemoryData.Skip(sizeof(short) * 2);
            }


            if (value == 0x9c)
            {
                for (int n = 0; n < 12; n++)
                {
                    SkipIds();
                }
            }

        }



        private string GetString()
        {
            byte value = MemoryData.GetByteValue();//unsigned byte
            if (value == 0xa0)
            {
                return null;
            }

            if ((value >> 4) == 0xa)
            {
                StringOffset = MemoryData.GetPosition() - 1;
                return GetString(value & 0xf);
            }

            if (value >> 4 == 0xd)
            {
                byte casebyte = (byte)(value & 0xf);
                if (casebyte == 0xa)
                {
                    StringOffset = MemoryData.GetPosition() - 1;
                    return GetString(MemoryData.GetShortValue(true, -1, Endian.Big));
                }
            }


            if (value == 0x9c)
            {
                return GetString();
            }

            return GetString();
        }

    }
}
