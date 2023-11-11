using AssetParser;
using Helper.MemoryList;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using UE4localizationsTool.Core.Hash;

namespace UE4localizationsTool.Core.locres
{

    public class HashTable
    {
        public uint NameHash { get; set; }
        public uint KeyHash { get; set; }
        public uint ValueHash { get; set; }

        public HashTable()
        {

        }
        public HashTable(uint NameHash, uint KeyHash=0, uint ValueHash=0)
        {
            this.NameHash = NameHash;
            this.KeyHash = KeyHash;
            this.ValueHash = ValueHash;
        }

        public override string ToString()
        {
            return $"NameHash: {NameHash} KeyHash: {KeyHash} ValueHash: {ValueHash}";
        }
    }


    public class NameSpaceTable : List<StringTable>
    {
        public uint NameHash { get; set; } = 0;
        public string Name { get; set; }
        public NameSpaceTable(string TableName, uint NameHash = 0)
        {
            this.Name = TableName;
            this.NameHash = NameHash;
        }

        public StringTable this[string key]
        {
            get
            {
                int index = FindIndex(x => x.key == key);
                if (index >= 0)
                {
                    return this[index];
                }
                throw new KeyNotFoundException($"key '{key}' not found.");
            }
            set
            {
                int index = FindIndex(x => x.key == key);
                if (index >= 0)
                {
                    this[index] = value;
                }
                else
                {
                    throw new KeyNotFoundException($"key '{key}' not found.");
                }
            }
        }

        public bool ContainsKey(string key)
        {
            return FindIndex(x => x.key == key) >= 0;
        }

        public void RemoveKey(string key)
        {
            int index = FindIndex(x => x.key == key);
            if (index >= 0)
            {
                RemoveAt(index);
            }
        }

        //why?! i don't know but it's work :D
        public new void Add(StringTable table)
        {
            table.root = this;
            base.Add(table);
        }
    }

    public class StringTable
    {
        public uint keyHash { get; set; }
        public string key { get; set; }

        public uint ValueHash { get; set; }
        public string Value { get; set; }

        public NameSpaceTable root;
        public StringTable()
        {

        }
        public StringTable(string TableKey, string TableValue, uint keyHash = 0, uint ValueHash = 0)
        {
            this.key = TableKey;
            this.Value = TableValue;
            this.keyHash = keyHash;
            this.ValueHash = ValueHash;
        }
    }

    public enum LocresVersion : byte
    {
        Legacy = 0,
        Compact,
        Optimized,
        Optimized_CityHash64_UTF16,
    }

    public class LocresFile : List<NameSpaceTable>, IAsset
    {

        public NameSpaceTable this[string Name]
        {
            get
            {
                int index = FindIndex(x => x.Name == Name);
                if (index >= 0)
                {
                    return this[index];
                }
                throw new KeyNotFoundException($"key '{Name}' not found.");
            }
            set
            {
                int index = FindIndex(x => x.Name == Name);
                if (index >= 0)
                {
                    this[index] = value;
                }
                else
                {
                    throw new KeyNotFoundException($"key '{Name}' not found.");
                }
            }
        }

        bool ContainsKey(string key)
        {
            return FindIndex(x => x.Name == key) >= 0;
        }




        //{7574140E-4A67-FC03-4A15-909DC3377F1B}
        private readonly byte[] MagicGUID = { 0x0E, 0x14, 0x74, 0x75, 0x67, 0x4A, 0x03, 0xFC, 0x4A, 0x15, 0x90, 0x9D, 0xC3, 0x37, 0x7F, 0x1B };
        public bool IsGood { get; set; } = true;
        MemoryList locresData;
        public LocresVersion Version;
        public LocresFile(string FilePath)
        {
            locresData = new MemoryList(FilePath);
            Load();
        }


        void Load()
        {
            locresData.Seek(0);
            byte[] FileGUID = locresData.GetBytes(16);
            if (FileGUID.SequenceEqual(MagicGUID))
            {
                Version = (LocresVersion)locresData.GetByteValue();
            }
            else
            {
                Version = LocresVersion.Legacy;
                locresData.Seek(0);
            }

            if (Version > LocresVersion.Optimized_CityHash64_UTF16)
            {
                throw new Exception("Unsupported locres version");
            }


            string[] Strings = new string[0];

            if (Version >= LocresVersion.Compact)
            {

                int localizedStringOffset = (int)locresData.GetInt64Value();
                int currentFileOffset = locresData.GetPosition();



                locresData.Seek(localizedStringOffset);

                int localizedStringCount = locresData.GetIntValue();

                Strings = new string[localizedStringCount];

                if (Version >= LocresVersion.Optimized)
                {
                    for (int i = 0; i < localizedStringCount; i++)
                    {
                        Strings[i] = locresData.GetStringUE();
                        locresData.Skip(4);//ref count
                    }
                }
                else
                {
                    for (int i = 0; i < localizedStringCount; i++)
                    {
                        Strings[i] = locresData.GetStringUE();
                    }
                }
                locresData.Seek(currentFileOffset);

            }
            else if (Version == LocresVersion.Legacy)
            {
                int HashTablesCount = locresData.GetIntValue();


                for (int i = 0; i < HashTablesCount; i++)
                {
                    var NameSpaceHash = locresData.GetStringUE(); //hash namespace

                    int localizedStringCount = locresData.GetIntValue();


                    for (int n = 0; n < localizedStringCount; n++)
                    {

                        string KeyHash = locresData.GetStringUE(); //string hash
                        uint SourceStringHash = locresData.GetUIntValue();
                        AddString(NameSpaceHash, KeyHash, locresData.GetStringUE(), ValueHash: SourceStringHash);
                    }

                }
                return;
            }


            if (Version >= LocresVersion.Optimized)
            {
                locresData.Skip(4);//keys Count
            }


            int namespaceCount = locresData.GetIntValue();

            for (int n = 0; n < namespaceCount; n++)
            {
                string nameSpaceStr;
                uint nameSpaceStrHash;
                ReadTextKey(locresData, Version, out nameSpaceStrHash, out nameSpaceStr);
                uint keyCount = locresData.GetUIntValue();
                for (int k = 0; k < keyCount; k++)
                {
                    string KeyStr;
                    uint KeyStrHash;
                    ReadTextKey(locresData, Version, out KeyStrHash, out KeyStr);
                    uint SourceStringHash = locresData.GetUIntValue();
                    if (Version >= LocresVersion.Compact)
                    {
                        int localizedStringIndex = locresData.GetIntValue();
                        AddString(nameSpaceStr, KeyStr, Strings[localizedStringIndex], nameSpaceStrHash, KeyStrHash, SourceStringHash);
                    }

                }

            }
        }
        void ReadTextKey(MemoryList memoryList, LocresVersion locresVersion, out uint StrHash, out string Str)
        {
            StrHash = 0;
            Str = "";
            if (locresVersion >= LocresVersion.Optimized)
            {
                StrHash = memoryList.GetUIntValue();//crc32
            }

            Str = memoryList.GetStringUE();
        }

        //build
        private void Save()
        {
            locresData.Clear();

            if (Version == LocresVersion.Legacy)
            {
                buildLegacyFile();
                return;
            }

            locresData.SetBytes(MagicGUID);
            locresData.SetByteValue((byte)Version);

            var localizedStringOffsetpos = locresData.GetPosition();
            locresData.SetInt64Value(0);//localizedStringOffset

            if (Version >= LocresVersion.Optimized)
            {
                var Keyscount = 0;
                foreach (var Table in this)
                {
                    Keyscount += Table.Count;
                }
                locresData.SetIntValue(Keyscount);
            }


            locresData.SetIntValue(Count);//namespaceCount
            var stringTable = new List<StringEntry>();

            foreach (var NameSpace in this)
            {
                if (Version >= LocresVersion.Optimized)
                {
                    if (NameSpace.NameHash == 0)
                    {
                        locresData.SetUIntValue(CalcHash(NameSpace.Name));
                    }
                    else
                    {
                        locresData.SetUIntValue(NameSpace.NameHash);
                    }
                }
                locresData.SetStringUE(NameSpace.Name);
                locresData.SetIntValue(NameSpace.Count);

                foreach (var Table in NameSpace)
                {
                    if (Version >= LocresVersion.Optimized)
                    {
                        if (Table.keyHash == 0)
                        {
                            locresData.SetUIntValue(CalcHash(Table.key));
                        }
                        else
                        {
                            locresData.SetUIntValue(Table.keyHash);
                        }
                    }
                    
                    locresData.SetStringUE(Table.key);

                    if(Table.ValueHash == 0)
                    {
                        locresData.SetUIntValue(Table.Value.StrCrc32());
                    }
                    else
                    {
                        locresData.SetUIntValue(Table.ValueHash);
                    }

                    int stringTableIndex = stringTable.FindIndex(x => x.Text == Table.Value);

                    if (stringTableIndex == -1)
                    {
                        stringTableIndex = stringTable.Count;
                        stringTable.Add(new StringEntry() { Text = Table.Value, refCount = 1 });
                    }
                    else
                    {
                        stringTable[stringTableIndex].refCount += 1;
                    }

                    locresData.SetIntValue(stringTableIndex);

                }



            }


            int localizedStringOffset = locresData.GetPosition();

            locresData.SetIntValue(stringTable.Count);

            if (Version >= LocresVersion.Optimized)
            {
                foreach (var entry in stringTable)
                {
                    locresData.SetStringUE(entry.Text);
                    locresData.SetIntValue(entry.refCount);
                }
            }
            else
            {
                foreach (var entry in stringTable)
                {
                    locresData.SetStringUE(entry.Text);
                }
            }

            locresData.Seek(localizedStringOffsetpos);
            locresData.SetInt64Value(localizedStringOffset);
        }

        public void SaveFile(string FilPath)
        {
            Save();
            locresData.WriteFile(FilPath);
        }



        private void buildLegacyFile()
        {
            locresData.SetIntValue(Count);


            foreach (var names in this)
            {
                locresData.SetStringUE(names.Name, true);
                locresData.SetIntValue(names.Count);
                foreach (var table in names)
                {
                    locresData.SetStringUE(table.key, true);

                    if (table.ValueHash == 0)
                    {
                        locresData.SetUIntValue(table.Value.StrCrc32());
                    }
                    else
                    {
                        locresData.SetUIntValue(table.ValueHash);
                    }

                    locresData.SetStringUE(table.Value, true);
                }
            }


        }

        public void AddString(string NameSpace, string key, string value, uint NameSpaceHash = 0, uint keyHash = 0, uint ValueHash = 0)
        {
            if (!ContainsKey(NameSpace))
            {
                Add(new NameSpaceTable(NameSpace, NameSpaceHash));
            }

            if (!this[NameSpace].ContainsKey(key))
                this[NameSpace].Add(new StringTable(key, value, keyHash, ValueHash));
            else
                this[NameSpace][key].Value = value;
        }

        public void RemoveString(string NameSpace, string key)
        {
            this[NameSpace].RemoveKey(key);
        }

        public void RemoveNameSpace(string NameSpace)
        {
            int index = FindIndex(x => x.Name == NameSpace);
            if (index >= 0)
            {
                RemoveAt(index);
            }
        }
        public uint Optimized_CityHash64_UTF16Hash(string value)
        {
            var Hash = CityHash.Init.CityHash64(Encoding.Unicode.GetBytes(value));
            return (uint)Hash + ((uint)(Hash >> 32) * 23);
        }
        

        public void AddItemsToDataGridView(DataGridView dataGrid)
        {
            dataGrid.DataSource = null;
            dataGrid.Rows.Clear();
            dataGrid.Columns.Clear();

            var dataTable = new System.Data.DataTable();
            dataTable.Columns.Add("Name", typeof(string));
            dataTable.Columns.Add("Text value", typeof(string));
            dataTable.Columns.Add("Hash Table", typeof(HashTable));
            
            foreach (var names in this)
            {
                foreach (var table in names)
                {
                    string name = string.IsNullOrEmpty(names.Name) ? table.key : names.Name + "::" + table.key;
                    string textValue = table.Value;
                    dataTable.Rows.Add(name, textValue,new HashTable(names.NameHash,table.keyHash,table.ValueHash));
                }
            }

            dataGrid.DataSource = dataTable;
            dataGrid.Columns["Text value"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGrid.Columns["Hash Table"].Visible = false;
        }

        public void LoadFromDataGridView(DataGridView dataGrid)
        {
            Clear();
            foreach (DataGridViewRow row in dataGrid.Rows)
            {
                //0-> NameSpace
                //1-> Key
                var items = row.Cells["Name"].Value.ToString().Split(new string[] { "::" }, StringSplitOptions.None);

                string NameSpaceStr="";
                string KeyStr="";
                
                if (items.Length == 2)
                {
                    NameSpaceStr = items[0];
                    KeyStr = items[1];
                    
                }
                else
                {
                    KeyStr = items[0];
                }

                var HashTable= row.Cells["Hash Table"].Value as HashTable;



                AddString(NameSpaceStr, KeyStr, row.Cells["Text value"].Value.ToString(), HashTable.NameHash, HashTable.KeyHash, HashTable.ValueHash);
            }
        }


        public List<List<string>> ExtractTexts()
        {
            var strings = new List<List<string>>();

            foreach (var names in this)
            {
                foreach (var table in names)
                {
                    if (!string.IsNullOrEmpty(names.Name))
                        strings.Add(new List<string>() { names.Name + "::" + table.key, table.Value });
                    else
                        strings.Add(new List<string>() { names.Name, table.Value });

                }
            }
            return strings;
        }


        public void ImportTexts(List<List<string>> strings)
        {
            int i = 0;
            foreach (var names in this)
            {
                foreach (var table in names)
                {
                    table.Value = strings[i++][1];
                }
            }

        }

        public uint CalcHash(string Str)
        {
            if (string.IsNullOrEmpty(Str))
            {
                return 0;
            }


            if (Version == LocresVersion.Optimized_CityHash64_UTF16)
                return Optimized_CityHash64_UTF16Hash(Str);
            else if (Version >= LocresVersion.Optimized)
                return Str.StrCrc32();
            else
                return 0;
        }

        public class StringEntry
        {
            public string Text { get; set; }
            public int refCount { get; set; }
        }

    }

}