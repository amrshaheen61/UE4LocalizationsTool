using Helper.MemoryList;
using System;
using System.Text;

namespace AssetParser
{
    public class UDataTable
    {


        public UDataTable(MemoryList memoryList, Uexp uexp, bool Modify = false)
        {

            int pos;
            int Name = 0;
            while (!memoryList.EndofFile())
            {
                int BlockSize = 0;
                int ID1 = memoryList.GetIntValue();
                int ID2 = memoryList.GetIntValue();
                memoryList.Skip(-8);
                pos = memoryList.GetPosition();
                int StringSize = memoryList.GetIntValue();
                BlockSize = StringSize;
                string Text;
                if (StringSize > 2000)
                {
                    memoryList.Seek(pos + 1);
                    continue;
                }
                if (StringSize < 0)
                {
                    StringSize = (StringSize * -1);
                    Text = memoryList.GetStringValueN(true, -1, Encoding.Unicode);
                    BlockSize *= -2;
                }
                else
                {
                    Text = memoryList.GetStringValueN();
                }

                if ((uexp.UassetData.NAMES_DIRECTORY.Count > ID1 && ID1 >= uexp.UassetData.PathCount) && (uexp.UassetData.NAMES_DIRECTORY.Count > ID2 && ID2 >= 0))
                {
                    if (uexp.UassetData.GetPropertyName(ID1) != "None")
                    {
                        Name = ID1;
                    }
                }

                if (((StringSize - 1) == Text.Length) && IsGoodText(Text))
                {

                    try
                    {
                        long num = memoryList.GetInt64Value(false, pos - 9);
                        if (num == BlockSize + 4)
                        {
                            ID1 = Name = memoryList.GetIntValue(false, pos - 25);
                        }
                        else
                        {
                            num = memoryList.GetInt64Value(false, pos - 8);
                            if (num == BlockSize + 4)
                            {
                                ID1 = Name = memoryList.GetIntValue(false, pos - 24);
                            }
                        }


                    }
                    catch
                    {

                    }

                    memoryList.Seek(pos);
                    new ReadStringProperty(memoryList, uexp, uexp.UassetData.GetPropertyName(Name), Modify);
                }
                else
                {
                    memoryList.Seek(pos + 1);
                }





            }
        }


        public static bool IsGoodText(string text)
        {
            if (String.IsNullOrEmpty(text))
            {
                return false;
            }

            //if (Regex.IsMatch(text, @"[\x00-\x08\x0B-\x0C\x0E-\x1F\x7F]"))
            //{
            //    return false;
            //}

            foreach (char c in text)
            {
                if ((c >= 0x00 && c <= 0x1F || c == 0x7f) && c != 0x09 && c != 0x0A && c != 0x0D)
                {

                    return false;
                }
            }

            return true;

        }

    }
}


