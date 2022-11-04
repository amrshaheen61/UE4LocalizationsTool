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
            public int Value { get; set; }
            public int ExportName { get; set; }
            public short ExportMemberType { get; set; }
            public int ExportLength { get; set; }
            public int ExportStart { get; set; }

            public List<byte> ExportData;
        }


        private int NewSize;
        public int LegacyFileVersion;
        public UE4Version EngineVersion;
        int numCustomVersions;
        public int File_Directory_Offset;
        private int FFile_Directory_Offset;
        public int Number_of_Names;
        public int Name_Directory_Offset;
        private int FName_Directory_Offset;
        public int Number_Of_Exports;
        public int GatherableTextDataCount;
        public int GatherableTextDataOffset;
        private int FGatherableTextDataOffset;
        public int Exports_Directory_Offset;
        private int FExports_Directory_Offset;
        public int Number_Of_Imports;
        public int Imports_Directory_Offset;
        private int FImports_Directory_Offset;
        public int DependsOffset;
        private int FDependsOffset;
        public int SoftPackageReferencesCount;
        public int SoftPackageReferencesOffset;
        private int FSoftPackageReferencesOffset;
        public int SearchableNamesOffset;
        private int FSearchableNamesOffset;
        public int ThumbnailTableOffset;
        private int FThumbnailTableOffset;
        public int AssetRegistryDataOffset;
        private int FAssetRegistryDataOffset;
        public int BulkDataStartOffset;
        private int FBulkDataStartOffset;
        public int WorldTileInfoDataOffset;
        private int FWorldTileInfoDataOffset;
        public int PreloadDependencyCount;
        public int PreloadDependencyOffset;
        private int FPreloadDependencyOffset;
        public List<string> NAMES_DIRECTORY;
        public List<ImportsDirectory> Imports_Directory;
        public List<ExportsDirectory> Exports_Directory;
        public MemoryList UassetFile;
        public MemoryList UexpFile;
        public bool IsNotUseUexp;
        public bool UseFromStruct = true;
        public bool AutoVersion=false;


        public int Header_Size;
        public int Name_Directory_Size;
        public int Hash_Directory_offset;
        public int Hash_Directory_Size;
        public int Bundles_Offset;
        public int GraphData_Offset;
        public int GraphData_Size;
        public bool IOFile = false;
        public int PathCount = 0;
        public bool PathModify = true;
        public bool UseMethod2 = false;



        public Uasset(string FilePath)
        {


            if (!File.Exists(FilePath))
            {
                throw new Exception("Uasset file is not exists!");
            }
            UassetFile = new MemoryList(FilePath);


            if (UassetFile.GetUIntValue(false) != 0x9E2A83C1u)
            {

                //Todo 
                EngineVersion = UE4Version.VER_UE4_16; //?!
                IsNotUseUexp =true;
                IOFile = true;
                UassetFile.Skip(16+4);
                Header_Size = UassetFile.GetIntValue(); 
                Name_Directory_Offset = UassetFile.GetIntValue();
                Name_Directory_Size  = UassetFile.GetIntValue();
                Hash_Directory_offset = UassetFile.GetIntValue();
                Hash_Directory_Size = UassetFile.GetIntValue();
                Imports_Directory_Offset=UassetFile.GetIntValue();
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
                    if (NAMES_DIRECTORY[n].Contains(@"/")&& PathModify)
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
                ExportReadOrEditIO();

                    return;
            }



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






            LegacyFileVersion = UassetFile.GetIntValue();
            if (LegacyFileVersion != -4)
            {
                UassetFile.GetIntValue(); //LegacyUE3Version
            }

            EngineVersion = (UE4Version)UassetFile.GetIntValue();

            if (LegacyFileVersion <= -8)
            {
                UassetFile.GetIntValue(); //FileVersionUE5
            }

            UassetFile.Skip(4);//FileVersionLicenseeUE 
            if (LegacyFileVersion <= -2)
            {
                numCustomVersions = UassetFile.GetIntValue();
                for (int i = 0; i < numCustomVersions; i++)
                {
                    UassetFile.Skip(16);//Guid
                    UassetFile.Skip(4);//Unkown 
                }
            }



            //File Start
            FFile_Directory_Offset = UassetFile.GetPosition();
            File_Directory_Offset = UassetFile.GetIntValue();

            // None
            UassetFile.GetStringUE();

            //Unknow
            UassetFile.Seek(4, SeekOrigin.Current);

            //Property names
            Number_of_Names = UassetFile.GetIntValue();
            FName_Directory_Offset = UassetFile.GetPosition();
            Name_Directory_Offset = UassetFile.GetIntValue();

            //TODO
            if (EngineVersion == UE4Version.UNKNOWN)
            {
                if (Name_Directory_Offset - (numCustomVersions * 20) == 189)
                {
                    EngineVersion = UE4Version.VER_UE4_15;
                    AutoVersion = true;
                }
                else if (Name_Directory_Offset - (numCustomVersions * 20) > 185)
                {
                    EngineVersion = UE4Version.VER_UE4_16;
                    AutoVersion = true;
                }
                else if (Name_Directory_Offset - (numCustomVersions * 20) == 185)
                {
                    EngineVersion = UE4Version.VER_UE4_6;
                    AutoVersion = true;
                }
            }
            if (EngineVersion >= UE4Version.VER_UE4_NAME_HASHES_SERIALIZED)
            {

                UseFromStruct = true;

            }
            else
            {
                UseFromStruct = false;
            }


            if (EngineVersion >= UE4Version.VER_UE4_SERIALIZE_TEXT_IN_PACKAGES)
            {
                GatherableTextDataCount = UassetFile.GetIntValue();
                FGatherableTextDataOffset = UassetFile.GetPosition();
                GatherableTextDataOffset = UassetFile.GetIntValue();
            }

            //Exports Blocks
            Number_Of_Exports = UassetFile.GetIntValue();
            FExports_Directory_Offset = UassetFile.GetPosition();
            Exports_Directory_Offset = UassetFile.GetIntValue();

            //Imports Blocks
            Number_Of_Imports = UassetFile.GetIntValue();
            FImports_Directory_Offset = UassetFile.GetPosition();
            Imports_Directory_Offset = UassetFile.GetIntValue();

            FDependsOffset = UassetFile.GetPosition();
            DependsOffset = UassetFile.GetIntValue();


            if (EngineVersion >= UE4Version.VER_UE4_ADD_STRING_ASSET_REFERENCES_MAP)
            {
                SoftPackageReferencesCount = UassetFile.GetIntValue();
                FSoftPackageReferencesOffset = UassetFile.GetPosition();
                SoftPackageReferencesOffset = UassetFile.GetIntValue();
            }
            if (EngineVersion >= UE4Version.VER_UE4_ADDED_SEARCHABLE_NAMES)
            {
                FSearchableNamesOffset = UassetFile.GetPosition();
                SearchableNamesOffset = UassetFile.GetIntValue();
            }
            FThumbnailTableOffset = UassetFile.GetPosition();
            ThumbnailTableOffset = UassetFile.GetIntValue();

            //PackageGuid
            UassetFile.Skip(16);

            int Num = UassetFile.GetIntValue();
            for (int i = 0; i < Num; i++)
            {
                UassetFile.Skip(8);
            }

            if (EngineVersion >= UE4Version.VER_UE4_ENGINE_VERSION_OBJECT)
            {
                UassetFile.Skip(2);
                UassetFile.Skip(2);
                UassetFile.Skip(2);
                UassetFile.Skip(4);
                UassetFile.GetStringUE();
            }
            else
            {
                UassetFile.Skip(4);
            }


            if (EngineVersion >= UE4Version.VER_UE4_PACKAGE_SUMMARY_HAS_COMPATIBLE_ENGINE_VERSION)
            {
                UassetFile.Skip(2);
                UassetFile.Skip(2);
                UassetFile.Skip(2);
                UassetFile.Skip(4);
                UassetFile.GetStringUE();
            }

            UassetFile.Skip(4);// CompressionFlags

            UassetFile.Skip(4);// numCompressedChunks

            UassetFile.Skip(4);// PackageSource

            UassetFile.Skip(4);// numAdditionalPackagesToCook

            if (LegacyFileVersion > -7)
            {
                UassetFile.Skip(4);// numTextureAllocations 
            }
            FAssetRegistryDataOffset = UassetFile.GetPosition();
            AssetRegistryDataOffset = UassetFile.GetIntValue();
            FBulkDataStartOffset = UassetFile.GetPosition();
            BulkDataStartOffset = UassetFile.GetIntValue();
            UassetFile.Skip(4);
            if (EngineVersion >= UE4Version.VER_UE4_WORLD_LEVEL_INFO)
            {
                FWorldTileInfoDataOffset = UassetFile.GetPosition();
                WorldTileInfoDataOffset = UassetFile.GetIntValue();
            }

            //ChunkIDs
            if (EngineVersion >= UE4Version.VER_UE4_CHANGED_CHUNKID_TO_BE_AN_ARRAY_OF_CHUNKIDS)
            {
                int numChunkIDs = UassetFile.GetIntValue();

                for (int i = 0; i < numChunkIDs; i++)
                {
                    UassetFile.Skip(4);
                }
            }
            else if (EngineVersion >= UE4Version.VER_UE4_ADDED_CHUNKID_TO_ASSETDATA_AND_UPACKAGE)
            {
                UassetFile.Skip(4);
            }

            if (EngineVersion >= UE4Version.VER_UE4_PRELOAD_DEPENDENCIES_IN_COOKED_EXPORTS)
            {
                PreloadDependencyCount = UassetFile.GetIntValue();
                FPreloadDependencyOffset = UassetFile.GetPosition();
                PreloadDependencyOffset = UassetFile.GetIntValue();
            }



            //seek to position
            UassetFile.Seek(Name_Directory_Offset, SeekOrigin.Begin);
            //Get Names
            NAMES_DIRECTORY = new List<string>();
            for (int n = 0; n < Number_of_Names; n++)
            {
                NAMES_DIRECTORY.Add(UassetFile.GetStringUE());

                if (NAMES_DIRECTORY[n].Contains(@"/") && PathModify)
                {
                    PathCount++;
                }
                else
                {
                    PathModify = false;
                }

                //Flags
                if (EngineVersion >= UE4Version.VER_UE4_NAME_HASHES_SERIALIZED)
                    UassetFile.Skip(4);
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



            //Get Exports
            Exports_Directory = new List<ExportsDirectory>();
            ExportReadOrEdit();
        }



        public void EditName(string NewStr, int Index)
        {

            if (IOFile) return;

            NewSize = 0;
            int OldSize = UassetFile.GetSize();
            UassetFile.Seek(Name_Directory_Offset, SeekOrigin.Begin);

            for (int n = 0; n < Number_of_Names; n++)
            {
                if (n == Index)
                {
                    UassetFile.ReplaceStringUE(NewStr);
                    break;
                }
                else
                {
                    UassetFile.GetStringUE();
                    if (EngineVersion >= UE4Version.VER_UE4_NAME_HASHES_SERIALIZED)
                        UassetFile.Skip(4);
                }
            }


            NewSize = UassetFile.GetSize() - OldSize;

            if (FFile_Directory_Offset != 0 && File_Directory_Offset != 0)
            {
                File_Directory_Offset = File_Directory_Offset + NewSize;
                UassetFile.SetIntValue(File_Directory_Offset, false, FFile_Directory_Offset);
            }

            //Name  

            if (FGatherableTextDataOffset != 0 && GatherableTextDataOffset != 0)
            {
                GatherableTextDataOffset = GatherableTextDataOffset + NewSize;
                UassetFile.SetIntValue(GatherableTextDataOffset, false, FGatherableTextDataOffset);
            }

            if (FExports_Directory_Offset != 0 && Exports_Directory_Offset != 0)
            {
                Exports_Directory_Offset = Exports_Directory_Offset + NewSize;
                UassetFile.SetIntValue(Exports_Directory_Offset, false, FExports_Directory_Offset);
            }

            if (FImports_Directory_Offset != 0 && Imports_Directory_Offset != 0)
            {
                Imports_Directory_Offset = Imports_Directory_Offset + NewSize;
                UassetFile.SetIntValue(Imports_Directory_Offset, false, FImports_Directory_Offset);
            }

            if (FDependsOffset != 0 && DependsOffset != 0)
            {
                DependsOffset = DependsOffset + NewSize;
                UassetFile.SetIntValue(DependsOffset, false, FDependsOffset);
            }

            if (FSoftPackageReferencesOffset != 0 && SoftPackageReferencesOffset != 0)
            {
                SoftPackageReferencesOffset = SoftPackageReferencesOffset + NewSize;
                UassetFile.SetIntValue(SoftPackageReferencesOffset, false, FSoftPackageReferencesOffset);
            }

            if (FSearchableNamesOffset != 0 && SearchableNamesOffset != 0)
            {
                SearchableNamesOffset = SearchableNamesOffset + NewSize;
                UassetFile.SetIntValue(SearchableNamesOffset, false, FSearchableNamesOffset);
            }

            if (FThumbnailTableOffset != 0 && ThumbnailTableOffset != 0)
            {
                ThumbnailTableOffset = ThumbnailTableOffset + NewSize;
                UassetFile.SetIntValue(ThumbnailTableOffset, false, FThumbnailTableOffset);
            }

            if (FAssetRegistryDataOffset != 0 && AssetRegistryDataOffset != 0)
            {
                AssetRegistryDataOffset = AssetRegistryDataOffset + NewSize;
                UassetFile.SetIntValue(AssetRegistryDataOffset, false, FAssetRegistryDataOffset);
            }

            if (FBulkDataStartOffset != 0 && BulkDataStartOffset != 0)
            {
                BulkDataStartOffset = BulkDataStartOffset + NewSize;
                UassetFile.SetIntValue(BulkDataStartOffset, false, FBulkDataStartOffset);
            }

            if (FWorldTileInfoDataOffset != 0 && WorldTileInfoDataOffset != 0)
            {
                WorldTileInfoDataOffset = WorldTileInfoDataOffset + NewSize;
                UassetFile.SetIntValue(WorldTileInfoDataOffset, false, FWorldTileInfoDataOffset);
            }

            if (FPreloadDependencyOffset != 0 && PreloadDependencyOffset != 0)
            {
                PreloadDependencyOffset = PreloadDependencyOffset + NewSize;
                UassetFile.SetIntValue(PreloadDependencyOffset, false, FPreloadDependencyOffset);
            }

        }



        public void ExportReadOrEdit(bool Modify = false)
        {
            if (IOFile)
            {
                ExportReadOrEditIO(Modify);
                return;
            }

            int NextExportPosition = File_Directory_Offset;
            //seek to position
            UassetFile.Seek(Exports_Directory_Offset, SeekOrigin.Begin);
            for (int n = 0; n < Number_Of_Exports; n++)
            {
                ExportsDirectory ExportsDirectory = new ExportsDirectory();
                ExportsDirectory.ExportClass = UassetFile.GetIntValue();
                ExportsDirectory.ExportParent_1 = UassetFile.GetIntValue();
                if (EngineVersion >= UE4Version.VER_UE4_TemplateIndex_IN_COOKED_EXPORTS)
                {
                    ExportsDirectory.ExportParent_2 = UassetFile.GetIntValue();
                }

                ExportsDirectory.Value = UassetFile.GetIntValue();
                ExportsDirectory.ExportName = UassetFile.GetIntValue();

                _ = UassetFile.GetIntValue();
                _ = UassetFile.GetIntValue();//ObjectFlags


                if (EngineVersion < UE4Version.VER_UE4_64BIT_EXPORTMAP_SERIALSIZES)
                {
                    if (!Modify)
                    {
                        ExportsDirectory.ExportLength = UassetFile.GetIntValue();
                    }
                    else
                    {
                        UassetFile.SetIntValue(Exports_Directory[n].ExportData.Count);
                    }
                    if (!Modify)
                    {
                        ExportsDirectory.ExportStart = UassetFile.GetIntValue();
                    }
                    else
                    {
                        UassetFile.SetIntValue(NextExportPosition);
                        NextExportPosition += Exports_Directory[n].ExportData.Count;
                    }
                }
                else
                {
                    if (!Modify)
                    {
                        ExportsDirectory.ExportLength = UassetFile.GetIntValue();
                    }
                    else
                    {
                        UassetFile.SetIntValue(Exports_Directory[n].ExportData.Count);
                    }
                    UassetFile.Skip(4);
                    if (!Modify)
                    {
                        ExportsDirectory.ExportStart = UassetFile.GetIntValue();
                    }
                    else
                    {
                        UassetFile.SetIntValue(NextExportPosition);
                        NextExportPosition += Exports_Directory[n].ExportData.Count;
                    }
                    UassetFile.Skip(4);
                }


                UassetFile.Skip(4 * 3);

                UassetFile.Skip(16);// PackageGuid
                UassetFile.Skip(4); // PackageFlags

                if (EngineVersion >= UE4Version.VER_UE4_LOAD_FOR_EDITOR_GAME)
                {
                    UassetFile.Skip(4);
                }

                if (EngineVersion >= UE4Version.VER_UE4_COOKED_ASSETS_IN_EDITOR_SUPPORT)
                {
                    UassetFile.Skip(4);
                }
                if (EngineVersion >= UE4Version.VER_UE4_PRELOAD_DEPENDENCIES_IN_COOKED_EXPORTS)
                {
                    UassetFile.Skip(4 * 5);
                }



                if (!Modify)
                {
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
        }


        public void ExportReadOrEditIO(bool Modify=false)
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
                    UassetFile.SetInt64Value((long)(Header_Size+(NextExportPosition-File_Directory_Offset)));
                    UassetFile.SetInt64Value((long)Exports_Directory[n].ExportData.Count);
                }
                ExportsDirectory.ExportName = UassetFile.GetIntValue();
                UassetFile.Skip(4);
                UassetFile.Skip(8);

                //Wrong way
                ulong Class = UassetFile.GetUInt64Value();


                if (Class == 0x71E24A29987BD1EDu)
                {
                    ExportsDirectory.ExportClass = NAMES_DIRECTORY.IndexOf("DataTable");
                }
                else if (Class == 0x70289FB93F770603u)
                {
                    ExportsDirectory.ExportClass = NAMES_DIRECTORY.IndexOf("StringTable");
                }
                else if (Class == 0x574F27AEC05072D0u)
                {
                    ExportsDirectory.ExportClass = NAMES_DIRECTORY.IndexOf("Function");
                }
                else
                {
                    ExportsDirectory.ExportClass = NAMES_DIRECTORY.IndexOf("StructProperty");
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

        int ExportSize()
        {
            int Totalsize = 0;
            foreach (ExportsDirectory Size in Exports_Directory)
            {
                Totalsize+= Size.ExportData.Count;
            }
            return Totalsize;
        }
       public void UpdateOffset()
        {
            //for textures 🤔
            if (FBulkDataStartOffset>0&& BulkDataStartOffset>0) 
            {
                UassetFile.SetIntValue(IsNotUseUexp ? UassetFile.GetSize() : UassetFile.GetSize() + ExportSize(), false, FBulkDataStartOffset);
            }
            }

    }
}
