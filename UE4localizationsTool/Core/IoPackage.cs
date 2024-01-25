using AssetParser.Object;
using Helper.MemoryList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AssetParser
{
    public class IoPackage : IUasset
    {
        public int LegacyFileVersion { get; set; }
        public UEVersions EngineVersion { get; set; }
        public EPackageFlags PackageFlags { get; set; }
        public int File_Directory_Offset { get; set; }
        public int Number_of_Names { get; set; }
        public int Name_Directory_Offset { get; set; }
        public int Number_Of_Exports { get; set; }
        public int Exports_Directory_Offset { get; set; }
        public int Number_Of_Imports { get; set; }
        public int Imports_Directory_Offset { get; set; }
        public int ExportBundleEntriesOffset { get; set; }
        public List<string> NAMES_DIRECTORY { get; set; }
        public List<ImportsDirectory> Imports_Directory { get; set; }
        public List<ExportsDirectory> Exports_Directory { get; set; }
        public MemoryList UassetFile { get; set; }
        public bool IOFile { get; set; } = true;
        public bool IsNotUseUexp { get; set; }
        public bool UseFromStruct { get; set; } = true;
        public bool AutoVersion { get; set; }
        public bool UseMethod2 { get; set; }


        public int Header_Size { get; set; }
        public int Name_Directory_Size { get; set; }
        public int Hash_Directory_offset { get; set; }
        public int Hash_Directory_Size { get; set; }
        public int Bundles_Offset { get; set; }
        public int GraphData_Offset { get; set; }
        public int GraphData_Size { get; set; }
        public int PathCount { get; set; } = 0;
        public bool PathModify { get; set; } = true;

        public IoPackage(string FilePath)
        {
            UassetFile = new MemoryList(FilePath);
            //Todo 

            IsNotUseUexp = true;
            UassetFile.MemoryListPosition = 0;
            ConsoleMode.Print("Reading Uasset Header...");
            Console.WriteLine(UassetFile.GetIntValue(false, 4));



            UassetFile.Seek(UassetFile.GetIntValue(false, 24), SeekOrigin.Begin);

            string path = UassetFile.GetStringUES();
            UassetFile.Seek(0, SeekOrigin.Begin);


            if (path.StartsWith("/"))
            {
                EngineVersion = UEVersions.VER_UE4_16; //?!
                UE4Header();
            }
            else
            {
                EngineVersion = UEVersions.VER_UE5_0; //?!
                UE5Header();
            }

            //ScarletNexus-> Game/L10N 
            if (NAMES_DIRECTORY.First().StartsWith("/Game/L10N/"))
            {
                UseMethod2 = true;
            }
        }

        private void UE4Header()
        {
            UassetFile.Skip(16 + 4);
            Header_Size = UassetFile.GetIntValue();
            Name_Directory_Offset = UassetFile.GetIntValue();
            Name_Directory_Size = UassetFile.GetIntValue();
            Hash_Directory_offset = UassetFile.GetIntValue();
            Hash_Directory_Size = UassetFile.GetIntValue();
            Imports_Directory_Offset = UassetFile.GetIntValue();
            Exports_Directory_Offset = UassetFile.GetIntValue();
            Bundles_Offset = UassetFile.GetIntValue();
            GraphData_Offset = UassetFile.GetIntValue();
            GraphData_Size = UassetFile.GetIntValue();


            File_Directory_Offset = GraphData_Offset + GraphData_Size;
            Number_of_Names = Hash_Directory_Size / 8;
            Number_Of_Exports = (Bundles_Offset - Exports_Directory_Offset) / 72 /*Block Size*/;
            Number_Of_Imports = (Exports_Directory_Offset - Imports_Directory_Offset) / 8 /*Block Size*/;


            //seek to position
            UassetFile.Seek(Name_Directory_Offset, SeekOrigin.Begin);
            //Get Names
            NAMES_DIRECTORY = new List<string>();
            for (int n = 0; n < Number_of_Names; n++)
            {
                NAMES_DIRECTORY.Add(UassetFile.GetStringUES());
                if (NAMES_DIRECTORY[n].Contains(@"/") && PathModify)
                {
                    PathCount++;
                }
                else
                {
                    PathModify = false;
                }
            }

            //UassetFile.Seek(Hash_Directory_offset, SeekOrigin.Begin);

            //seek to position
            UassetFile.Seek(Exports_Directory_Offset, SeekOrigin.Begin);
            //Get Exports
            Exports_Directory = new List<ExportsDirectory>();
            ExportReadOrEdit();
        }

        private void UE5Header()
        {
            //this for ue5_0 only
            bool bHasVersioningInfo = UassetFile.GetUIntValue() == 1;
            Header_Size = UassetFile.GetIntValue();
            UassetFile.Skip(8); //name
            UassetFile.Skip(4); //PackageFlags
            UassetFile.Skip(4); //CookedHeaderSize
            UassetFile.Skip(4); //ImportedPublicExportHashesOffset
            Imports_Directory_Offset = UassetFile.GetIntValue();
            Exports_Directory_Offset = UassetFile.GetIntValue();
            ExportBundleEntriesOffset = UassetFile.GetIntValue();
            UassetFile.Skip(4); //GraphDataOffset

            File_Directory_Offset = Header_Size;
            Number_Of_Exports = (ExportBundleEntriesOffset - Exports_Directory_Offset) / 72;
            Number_Of_Imports = (Exports_Directory_Offset - Imports_Directory_Offset) / sizeof(long);


            if (bHasVersioningInfo)
            {
                throw new Exception("Not supported uasset!");
            }

            //----------------------
            //Get Names
            NAMES_DIRECTORY = new List<string>();
            Number_of_Names = UassetFile.GetIntValue();
            int NamesBlockSize = UassetFile.GetIntValue();
            UassetFile.Skip(8); //hashVersion
            UassetFile.Skip(Number_of_Names * sizeof(long));//hashes
            var NamesHeader = UassetFile.GetShorts(Number_of_Names);

            foreach (var header in NamesHeader)
            {
                NAMES_DIRECTORY.Add(UassetFile.GetStringUES(header));
            }


            //Get Exports
            UassetFile.Seek(Exports_Directory_Offset, SeekOrigin.Begin);
            Exports_Directory = new List<ExportsDirectory>();
            ExportReadOrEdit();
        }

        public void EditName(string NewStr, int Index)
        {
            return;
        }

        public void ExportReadOrEdit(bool Modify = false)
        {
            //seek to position
            UassetFile.Seek(Exports_Directory_Offset, SeekOrigin.Begin);
            int NextExportPosition = File_Directory_Offset;

            for (int n = 0; n < Number_Of_Exports; n++)
            {
                int Start = UassetFile.GetPosition();
                ExportsDirectory ExportsDirectory = new ExportsDirectory();
                ExportsDirectory.ExportStart = File_Directory_Offset;
                if (!Modify)
                {
                    UassetFile.Skip(8);
                    ExportsDirectory.ExportLength = (int)UassetFile.GetInt64Value();
                }
                else
                {
                    UassetFile.SetInt64Value(Header_Size + (NextExportPosition - File_Directory_Offset));
                    UassetFile.SetInt64Value(Exports_Directory[n].ExportData.Count);
                }
                ExportsDirectory.ExportName = UassetFile.GetIntValue();
                UassetFile.Skip(4);
                UassetFile.Skip(8);

                //Wrong way
                ulong Class = UassetFile.GetUInt64Value();//CityHash64 ?!

                switch (Class)
                {
                    case 0x71E24A29987BD1EDu:
                        if (!NAMES_DIRECTORY.Contains("DataTable"))
                        {
                            NAMES_DIRECTORY.Add("DataTable");
                        }
                        ExportsDirectory.ExportClass = NAMES_DIRECTORY.IndexOf("DataTable");
                        break;
                    case 0x70289FB93F770603u:

                        if (!NAMES_DIRECTORY.Contains("StringTable"))
                        {
                            NAMES_DIRECTORY.Add("StringTable");
                        }
                        ExportsDirectory.ExportClass = NAMES_DIRECTORY.IndexOf("StringTable");

                        break;
                    case 0x574F27AEC05072D0u:
                        if (!NAMES_DIRECTORY.Contains("Function"))
                        {
                            NAMES_DIRECTORY.Add("Function");
                        }
                        ExportsDirectory.ExportClass = NAMES_DIRECTORY.IndexOf("Function");
                        break;
                    default:
                        {
                            if (!NAMES_DIRECTORY.Contains("StructProperty"))
                            {
                                NAMES_DIRECTORY.Add("StructProperty");
                            }
                            ExportsDirectory.ExportClass = NAMES_DIRECTORY.IndexOf("StructProperty");
                            break;
                        }
                }


                if (!Modify)
                {
                    ExportsDirectory.ExportData = new List<byte>();
                    ExportsDirectory.ExportData.AddRange(UassetFile.GetBytes(ExportsDirectory.ExportLength, false, NextExportPosition));
                    Exports_Directory.Add(ExportsDirectory);
                }

                NextExportPosition += ExportsDirectory.ExportLength;
                UassetFile.Seek(Start + 72);
            }


        }


        public void UpdateOffset()
        {

        }
    }
}
