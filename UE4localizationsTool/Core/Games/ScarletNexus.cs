using Helper.MemoryList;
using System;
using System.Collections.Generic;
using System.Text;

namespace AssetParser
{
    public class ScarletNexus
    {

        public ScarletNexus(MemoryList memoryList, Uexp uexp, bool Modify = false)
        {
            memoryList.Skip(8);//-5
            int TextCount = memoryList.GetIntValue();

            for (int i=0;i< TextCount;i++) 
            {
                string Name = uexp.UassetData.GetPropertyName(memoryList.GetIntValue());
                memoryList.Skip(4);//null or something

                short num = memoryList.GetShortValue();

                if (num == 0x0B80)
                {
                    memoryList.Skip(1);
                    while (memoryList.GetIntValue(false) != 0)
                    {
                        int Position = memoryList.GetPosition();
                        int Lenght = memoryList.GetIntValue(true);
                        Encoding encoding = Encoding.ASCII;
                        if (Lenght < 0)
                        {
                            Lenght *= -1;
                            encoding = Encoding.Unicode;
                        }
                        //Console.WriteLine("Lenght: "+ Lenght);

                        string Value = "";

                        try
                        {
                            Value = memoryList.GetStringValueN(true, -1, encoding);
                        }
                        catch { }
                        memoryList.Seek(Position);
                        //Console.WriteLine("Value: " + Value.Length);


                        if (Lenght - 1 == Value.Length)
                        {
                            uexp.Strings.Add(new List<string>() { Name, memoryList.GetStringUE() });
                            ConsoleMode.Print(uexp.Strings[uexp.Strings.Count - 1][1], ConsoleColor.Magenta);
                        }
                        else
                        {
                            memoryList.Seek(Position);
                            memoryList.Skip(8);
                        }

                    }

                    memoryList.Skip(4);//null
                    Name = uexp.UassetData.GetPropertyName(memoryList.GetIntValue());
                    memoryList.Skip(4);//null or something
                    if (memoryList.GetShortValue(false) == 0x0B80)
                    {
                        memoryList.Skip(-8);
                    }
                }
                else if (num == 0x0300)
                {
                    uexp.Strings.Add(new List<string>() { Name, memoryList.GetStringUE() });
                    ConsoleMode.Print(uexp.Strings[uexp.Strings.Count - 1][1], ConsoleColor.Magenta);
                }



                /*
                if (uexp.UassetData.GetPropertyName(memoryList.GetIntValue(false))== "None")
                {
                    memoryList.Skip(12);//None
                    continue;
                }

                string Value = memoryList.GetStringUE();

                if (Value.StartsWith("DisplayName", StringComparison.OrdinalIgnoreCase)&& num!=10)
                {
                    Value = memoryList.GetStringUE();
                }

           
                */






            }

            return;

        }


     }

}
