using Helper.MemoryList;
using System;
using System.Collections.Generic;
using System.Text;

namespace AssetParser
{
    public class TheLastOricru
    {
        public TheLastOricru(MemoryList memoryList, string PropertyName, Uexp uexp, bool Modify = false)
        {


            try
            {

                while (true)
                {


                RemoveBytes:
                    while (memoryList.GetByteValue(false) != 1)
                    {
                        memoryList.Skip(1);
                    }

                    memoryList.Skip(1);

                    if (memoryList.GetByteValue(false) == 1)
                    {
                        goto RemoveBytes;
                    }

                    var Position = memoryList.GetPosition();
                    if (!UDataTable.IsGoodText(memoryList.GetStringValue(memoryList.GetByteValue(), true, -1, Encoding.UTF8)))
                    {
                        memoryList.SetPosition(Position);
                        goto RemoveBytes;
                    }
                    memoryList.SetPosition(Position);

                    if (!Modify)
                    {
                        uexp.Strings.Add(new List<string>() { PropertyName, AssetHelper.ReplaceBreaklines(memoryList.GetStringValue(memoryList.GetByteValue(), true, -1, Encoding.UTF8)) });
                        ConsoleMode.Print(uexp.Strings[uexp.Strings.Count - 1][1], ConsoleColor.Magenta);
                    }
                    else
                    {

                        memoryList.DeleteBytes(memoryList.GetByteValue(false) + 1);
                        var StrBytes = Encoding.UTF8.GetBytes(AssetHelper.ReplaceBreaklines(uexp.Strings[uexp.CurrentIndex][1], true));
                        memoryList.InsertByteValue((byte)StrBytes.Length);
                        memoryList.InsertBytes(StrBytes);
                        uexp.CurrentIndex++;
                    }
                }
            }
            catch
            {
                //nothing
            }




        }
    }
}