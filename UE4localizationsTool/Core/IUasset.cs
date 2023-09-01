using AssetParser.Object;
using Helper.MemoryList;
using System.Collections.Generic;

namespace AssetParser
{
    public interface IUasset
    {
        int LegacyFileVersion { get; set; }
        UEVersions EngineVersion { get; set; }
        EPackageFlags PackageFlags { get; set; }
        int File_Directory_Offset { get; set; }
        int Number_of_Names { get; set; }
        int Name_Directory_Offset { get; set; }
        int Number_Of_Exports { get; set; }
        int Exports_Directory_Offset { get; set; }
        int Number_Of_Imports { get; set; }
        int Imports_Directory_Offset { get; set; }
        List<string> NAMES_DIRECTORY { get; set; }
        List<ImportsDirectory> Imports_Directory { get; set; }
        List<ExportsDirectory> Exports_Directory { get; set; }
        MemoryList UassetFile { get; set; }
        bool IOFile { get; set; }
        bool IsNotUseUexp { get; set; }
        bool UseFromStruct { get; set; }
        bool AutoVersion { get; set; }
        bool UseMethod2 { get; set; }
        int PathCount { get; set; }
        bool PathModify { get; set; }
        void EditName(string NewStr, int Index);
        void ExportReadOrEdit(bool Modify = false);
        void UpdateOffset();
    }
}