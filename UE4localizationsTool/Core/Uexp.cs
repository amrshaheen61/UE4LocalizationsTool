using Helper.MemoryList;
using System;
using System.Collections.Generic;

namespace AssetParser
{
    public class Uexp
    {

        public Uasset UassetData;
        public List<List<string>> Strings;  //[Text id,Text Value,...]
        private int _CurrentIndex;
        public bool IsGood = true;
        public int ExportIndex;
        public int CurrentIndex
        {
            get
            {
                // Console.WriteLine(Strings.Count+ " - "+ (_CurrentIndex+1));
                return _CurrentIndex;
            }
            set
            {
                _CurrentIndex = value;
            }

        }
        public Uexp(Uasset assets)
        {
            UassetData = assets;
            Strings = new List<List<string>>();
            CurrentIndex = 0;
            ReadOrEdit();
        }




        private void ReadOrEdit(bool Modify = false)
        {

            for (int n = 0; n < UassetData.Exports_Directory.Count; n++)
            {

                ExportIndex = n;

                using (MemoryList memoryList = new MemoryList(UassetData.Exports_Directory[n].ExportData))
                {
                    try
                    {
                        memoryList.Seek(0); //Seek to beginning of Block

                        ConsoleMode.Print($"-----------{n}------------", ConsoleColor.Red);
                        _ = new StructProperty(memoryList, this, UassetData.UseFromStruct, false, Modify);
                        ConsoleMode.Print($"-----------End------------", ConsoleColor.Red);


                        if (memoryList.EndofFile())
                        {
                            continue;
                        }
                        
                        ConsoleMode.Print($"-----------{n}------------", ConsoleColor.DarkRed);
                        switch (UassetData.GetExportPropertyName(UassetData.Exports_Directory[n].ExportClass))
                        {
                            case "StringTable":
                                new StringTable(memoryList, this, Modify);
                                break;
                            case "DataTable":

                                new DataTable(memoryList, this, Modify);
                                break;
                            case "Spreadsheet":

                                new Spreadsheet(memoryList, this, Modify);
                                break;
                            case "Function":
                             // new Function(memoryList, this, Modify);
                                break;
                            case "REDLocalizeTextData":
                                new REDLocalizeTextData(memoryList, this, Modify);
                                break;
                            case "REDAdvTextData":
                                new REDAdvTextData(memoryList, this, Modify);
                                break;
                            case "REDLibraryTextData":
                                new REDLibraryTextData(memoryList, this, Modify);
                                break;
                        }
                        ConsoleMode.Print($"-----------End------------", ConsoleColor.DarkRed);
                 
                    }
                    catch(Exception ex)
                    {
                         ConsoleMode.Print("Skip this export:\n"+ ex.ToString(),ConsoleColor.Red);
                        // Skip this export
                    }
                }

            }


        }


        private void ModifyStrings()
        {
            CurrentIndex = 0;
            ReadOrEdit(true);
        }

        public void SaveFile(string FilPath)
        {
            ModifyStrings();
            UassetData.ExportReadOrEdit(true);
            if (UassetData.IsNotUseUexp)
            {
                MakeBlocks();
                UassetData.UassetFile.WriteFile(System.IO.Path.ChangeExtension(FilPath, ".uasset"));
            }
            else
            {
                MemoryList UexpData = MakeBlocks();
                UassetData.UassetFile.WriteFile(System.IO.Path.ChangeExtension(FilPath, ".uasset"));
                UexpData.WriteFile(System.IO.Path.ChangeExtension(FilPath, ".uexp"));
            }
        }

        private MemoryList MakeBlocks()
        {

            if (UassetData.IsNotUseUexp)
            {
                UassetData.UassetFile.SetSize(UassetData.File_Directory_Offset);
                UassetData.Exports_Directory.ForEach(x =>
                {
                    UassetData.UassetFile.MemoryListData.AddRange(x.ExportData);
                });
                UassetData.UassetFile.Add((uint)2653586369);
                return UassetData.UassetFile;
            }
            else
            {

                MemoryList memoryList = new MemoryList();
                UassetData.Exports_Directory.ForEach(x =>
                {
                    memoryList.MemoryListData.AddRange(x.ExportData);
                });
                memoryList.Add((uint)2653586369);
                return memoryList;
            }
        }


    }
}
