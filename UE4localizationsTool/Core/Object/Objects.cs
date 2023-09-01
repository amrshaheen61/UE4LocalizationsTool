using System.Collections.Generic;

namespace AssetParser.Object
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



}
