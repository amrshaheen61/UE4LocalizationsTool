using AssetParser;
using Helper.MemoryList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace UE4localizationsTool.Core.Games
{


    /*
     * Game:Jump Force
     * File:systemtext_EN.uasset
     */

    public class J5BinderAsset
    {
        //public MemoryList MemoryList { get; }
        public Uexp Uexp { get; }
        public bool Modify { get; }

        MemoryList StreamMemory;

        public J5BinderAsset(MemoryList memoryList, Uexp uexp, bool Modify = false)
        {
           // MemoryList = memoryList;
            Uexp = uexp;
            this.Modify = Modify;

            memoryList.GetIntValue();//Null
            var Baseposition = memoryList.GetPosition();
            var BufferSize = (int)memoryList.GetInt64Value();  
            StreamMemory=new MemoryList(memoryList.GetBytes(BufferSize));
            load();


            if (Modify)
            {
                Build();
                memoryList.SetSize(Baseposition);

                memoryList.SetInt64Value(StreamMemory.MemoryListSize);
                memoryList.SetBytes(StreamMemory.ToArray());
            }

        }


        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private class Header
        {
          public ulong Magic;
          public int Type;//??
          public int TablesCount;
          public int unk;//=48
          public int TableBlockSize;
          public long unk2;//=0
          public long Baseoffset;
          public long unk3;//=48
        }

        private class TableInfo
        {
            public int TableIndex;
            public int Type;//=16 ??
            public long TableSize;
            public long TableUSize;
            public ulong TableHash;
            public long TableNameoffset;
            public long NextTableOffset;
            public long TableOffset;//from +=Baseoffset 

            public string TableName;
            public STXTENLL Stxt;


            public static TableInfo Read(MemoryList memoryList)
            {
                var tableInfo = new TableInfo();
                tableInfo.TableIndex = memoryList.GetIntValue();
                tableInfo.Type = memoryList.GetIntValue();
                tableInfo.TableSize= memoryList.GetInt64Value();
                tableInfo.TableUSize = memoryList.GetInt64Value();
                tableInfo.TableHash = memoryList.GetUInt64Value();
                tableInfo.TableNameoffset = memoryList.GetInt64Value();
                tableInfo.NextTableOffset = memoryList.GetInt64Value();
                tableInfo.TableOffset = memoryList.GetInt64Value();

                tableInfo.TableName= memoryList.GetStringValueN(false,(int)tableInfo.TableNameoffset);

                return tableInfo;
            }

            public void Write(MemoryList memoryList)
            {
                memoryList.SetIntValue(TableIndex);
                memoryList.SetIntValue(Type);
                memoryList.SetInt64Value(TableSize);
                memoryList.SetInt64Value(TableUSize);
                memoryList.SetUInt64Value(TableHash);
                memoryList.SetInt64Value(TableNameoffset);
                memoryList.SetInt64Value(NextTableOffset);
                memoryList.SetInt64Value(TableOffset);
            }

        }


        public class STXTENLL
        {
            [StructLayout(LayoutKind.Sequential, Pack = 1)]
            public class Header
            {
               public ulong Magic;
               public int Type;//??
               public int HeaderSize;
               public int TableBufferSize;
               public int TablesCount;
               public ulong unko;//??=1200
            }

            public class TableInfo
            {
                public int Index;
                public int StringOffset;
                public string Value;
                public static TableInfo Read(MemoryList memoryList)
                {
                    var tableInfo = new TableInfo();
                    tableInfo.Index = memoryList.GetIntValue();
                    tableInfo.StringOffset = memoryList.GetIntValue();
                    tableInfo.Value = memoryList.GetStringUE(SavePosition: false,SeekAndRead: tableInfo.StringOffset, encoding: Encoding.Unicode);
                    return tableInfo;
                }
                public void Write(MemoryList memoryList)
                {
                    memoryList.SetIntValue(Index);
                    memoryList.SetIntValue(StringOffset);
                }
            }


            public Header header;
            public TableInfo[] tableInfos;

            public static STXTENLL Read(MemoryList memoryList)
            {
                var stxt = new STXTENLL();
                stxt.header = memoryList.GetStructureValues<Header>();

                if(stxt.header.Magic!= 0x4C4C4E4554585453)
                {
                    throw new Exception("stxt.header.Magic!=0x4C4C4E4554585453");
                }

                stxt.tableInfos = new TableInfo[stxt.header.TablesCount];
                for (int i = 0; i < stxt.header.TablesCount; i++)
                {
                    stxt.tableInfos[i] = TableInfo.Read(memoryList);
                }
                return stxt;
            }

            public byte[] Build()
            {
                var memoryList = new MemoryList();
                memoryList.SetStructureValus(header);

                var Position = memoryList.GetPosition();
                memoryList.SetBytes(new byte[header.TableBufferSize*header.TablesCount]);

                foreach (var tableInfo in tableInfos)
                {
                    tableInfo.StringOffset = memoryList.GetPosition();
                    memoryList.SetStringUE(tableInfo.Value,encoding:Encoding.Unicode);
                }
                memoryList.Seek(Position);


                foreach (var tableInfo in tableInfos)
                {
                    tableInfo.Write(memoryList);
                }

                return memoryList.ToArray();
            }   


        }



        Header header;
        TableInfo[] tableInfos;
        private void load()
        {
            header = StreamMemory.GetStructureValues<Header>();

            if(header.Magic != 0x4C4C46444E42)
            {
                throw new Exception("J5BinderAssetMagic!=0x4C4C46444E42");
            }

            tableInfos=new TableInfo[header.TablesCount];
            for (int i = 0; i < header.TablesCount; i++)
            {
                tableInfos[i] = TableInfo.Read(StreamMemory);
            }

            for (int i = 0; i < header.TablesCount; i++)
            {
                StreamMemory.Seek((int)(header.Baseoffset + tableInfos[i].TableOffset));
                tableInfos[i].Stxt = STXTENLL.Read(new MemoryList(StreamMemory.GetBytes((int)tableInfos[i].TableSize)));
            }


            foreach(var tableInfo in tableInfos)
            {
                foreach(var stxt in tableInfo.Stxt.tableInfos)
                {
                    Uexp.Strings.Add(new List<string>() { tableInfo.TableName, stxt.Value });
                }
            }   


        }


        private void Build()
        {
            StreamMemory.Seek((int)(header.Baseoffset));
            StreamMemory.SetSize((int)(header.Baseoffset));

            foreach (var tableInfo in tableInfos)
            {
                foreach (var stxt in tableInfo.Stxt.tableInfos)
                {
                    stxt.Value = Uexp.Strings[Uexp.CurrentIndex++][1];
                }
            }


            foreach (var tableInfo in tableInfos)
            {
              var buffer=  tableInfo.Stxt.Build();
                tableInfo.TableOffset = StreamMemory.GetPosition() - header.Baseoffset;
                tableInfo.TableSize= buffer.Length;
                tableInfo.TableUSize = buffer.Length;
                StreamMemory.SetBytes(buffer);
            }

            StreamMemory.Seek(0);

            StreamMemory.SetStructureValus(header);

            foreach (var tableInfo in tableInfos)
            {
                tableInfo.Write(StreamMemory);
            }

        } 




    }
}
