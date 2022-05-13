using Helper.MemoryList;
using System;
using System.Collections.Generic;
using System.IO;

namespace AssetParser
{




    public class Uasset
    {

        public struct ImportsDirectory
        {

            public long ParentDirectoryNameID { get; set; }
            public long ClassID { get; set; }
            public int ParentImportObjectID { get; set; }
            public int NameID { get; set; }

        }
        public struct ExportsDirectory
        {
            public int ExportClass { get; set; }
            public int ExportParent_1 { get; set; }
            public int ExportParent_2 { get; set; }
            public int ExportName { get; set; }
            public short ExportMemberType { get; set; }
            public int ExportLength { get; set; }
            public int ExportStart { get; set; }

            public List<byte> ExportData;
        }



        public int FileVersion;
        public int File_Directory_Offset;
        public int Number_of_Names;
        public int Name_Directory_Offset;
        public int Number_Of_Exports;
        public int Exports_Directory_Offset;
        public int Number_Of_Imports;
        public int Imports_Directory_Offset;
        public List<string> NAMES_DIRECTORY;
        public List<ImportsDirectory> Imports_Directory;
        public List<ExportsDirectory> Exports_Directory;
        public MemoryList UassetFile;
        public MemoryList UexpFile;
        public bool IsNotUseUexp;
        public Uasset(string FilePath)
        {

            if (!File.Exists(FilePath))
            {
                throw new Exception("Uasset file is not exists!");
            }

            if (!File.Exists(FilePath))
            {
                throw new Exception("Uasset file is not exists!");
            }
            UassetFile = new MemoryList(FilePath);

            if (UassetFile.GetIntValue(false, UassetFile.GetSize() - 4) != -1641380927)
            {
                if (!File.Exists(Path.ChangeExtension(FilePath, ".uexp")))
                {
                    throw new Exception("Uexp file is not exists!");
                }
                UexpFile = new MemoryList(Path.ChangeExtension(FilePath, ".uexp"));
                IsNotUseUexp = false;
            }
            else
            {
                IsNotUseUexp = true;
            }



            if (UassetFile.GetIntValue() != -1641380927)
            {
                throw new Exception($"This file \'{FilePath}\' is not uasset file!");
            }

            FileVersion = UassetFile.GetIntValue();
            if (FileVersion != -7)
            {
                throw new Exception("Not supported version!");
            }


            //3 Null Bytes
            UassetFile.Seek(3 * 4, SeekOrigin.Current);

            //Unknow
            _ = UassetFile.GetIntValue();

            File_Directory_Offset = UassetFile.GetIntValue();
            // None
            UassetFile.GetBytes(UassetFile.GetIntValue());

            //Unknow
            UassetFile.Seek(4, SeekOrigin.Current);

            //Property names
            Number_of_Names = UassetFile.GetIntValue();
            Name_Directory_Offset = UassetFile.GetIntValue();

            //Unknow
            UassetFile.Seek(8, SeekOrigin.Current);

            //Exports Blocks
            Number_Of_Exports = UassetFile.GetIntValue();
            Exports_Directory_Offset = UassetFile.GetIntValue();

            //Imports Blocks
            Number_Of_Imports = UassetFile.GetIntValue();
            Imports_Directory_Offset = UassetFile.GetIntValue();

            //seek to position
            UassetFile.Seek(Name_Directory_Offset, SeekOrigin.Begin);
            //Get Names
            NAMES_DIRECTORY = new List<string>();
            for (int n = 0; n < Number_of_Names; n++)
            {
                NAMES_DIRECTORY.Add(UassetFile.GetStringUE());
                //Console.WriteLine(NAMES_DIRECTORY[n]);
                //Flags
                UassetFile.Seek(4, SeekOrigin.Current);
            }

            //seek to position
            UassetFile.Seek(Imports_Directory_Offset, SeekOrigin.Begin);
            //Get Imports
            Imports_Directory = new List<ImportsDirectory>();
            for (int n = 0; n < Number_Of_Imports; n++)
            {
                ImportsDirectory ImportsDirectory = new ImportsDirectory();
                ImportsDirectory.ParentDirectoryNameID = UassetFile.GetInt64Value();
                ImportsDirectory.ClassID = UassetFile.GetInt64Value();
                ImportsDirectory.ParentImportObjectID = UassetFile.GetIntValue();
                ImportsDirectory.NameID = UassetFile.GetIntValue();
                _ = UassetFile.GetIntValue(); //Unknown ID

                Imports_Directory.Add(ImportsDirectory);
            }






            //seek to position
            UassetFile.Seek(Exports_Directory_Offset, SeekOrigin.Begin);
            //Get Imports
            Exports_Directory = new List<ExportsDirectory>();
            for (int n = 0; n < Number_Of_Exports; n++)
            {
                ExportsDirectory ExportsDirectory = new ExportsDirectory();
                ExportsDirectory.ExportClass = UassetFile.GetIntValue();
                ExportsDirectory.ExportParent_1 = UassetFile.GetIntValue();
                ExportsDirectory.ExportParent_2 = UassetFile.GetIntValue();
                _ = UassetFile.GetIntValue();
                ExportsDirectory.ExportName = UassetFile.GetIntValue();
                _ = UassetFile.GetIntValue();
                ExportsDirectory.ExportMemberType = UassetFile.GetShortValue();
                _ = UassetFile.GetShortValue();
                ExportsDirectory.ExportLength = UassetFile.GetIntValue();
                int NullIntsize = 0;
                if (UassetFile.GetIntValue(false) == 0)
                {
                    _ = UassetFile.GetIntValue();
                }
                else
                {
                    NullIntsize = 1;
                }
                ExportsDirectory.ExportStart = UassetFile.GetIntValue();
                UassetFile.Seek(4 * 16 - NullIntsize, SeekOrigin.Current);


                if (IsNotUseUexp)
                {

                    ExportsDirectory.ExportData = new List<byte>();
                    ExportsDirectory.ExportData.AddRange(UassetFile.GetBytes(ExportsDirectory.ExportLength, false, ExportsDirectory.ExportStart));
                    Exports_Directory.Add(ExportsDirectory);
                }
                else
                {
                    UexpFile.Seek(ExportsDirectory.ExportStart - File_Directory_Offset, SeekOrigin.Begin);
                    ExportsDirectory.ExportData = new List<byte>();
                    ExportsDirectory.ExportData.AddRange(UexpFile.GetBytes(ExportsDirectory.ExportLength));
                    Exports_Directory.Add(ExportsDirectory);
                }
            }





        }



        public void UpdateExport()
        {
            int NextExportPosition = File_Directory_Offset;
            //seek to position
            UassetFile.Seek(Exports_Directory_Offset, SeekOrigin.Begin);
            for (int n = 0; n < Exports_Directory.Count; n++)
            {
                UassetFile.Skip(4 * 6);
                UassetFile.Skip(2 * 2);
                UassetFile.SetIntValue(Exports_Directory[n].ExportData.Count);
              // Console.WriteLine(Exports_Directory[n].ExportData.Count);
                int NullIntsize = 0;
                if (UassetFile.GetIntValue(false) == 0)
                {
                    _ = UassetFile.GetIntValue();
                }
                else
                {
                    NullIntsize = 1;
                }
                UassetFile.SetIntValue(NextExportPosition);
                NextExportPosition += Exports_Directory[n].ExportData.Count;
                UassetFile.Skip(4 * (16 - NullIntsize));
            }
        }

    }
}
