using Helper.MemoryList;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using UE4localizationsTool.Core.Games;

namespace AssetParser
{

    public class StringNode 
    {
        public string NameSpace { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }


        public string GetName()
        {
            return string.IsNullOrEmpty(NameSpace)?Key: NameSpace+ "::" + Key;
        }
    }
    
    public class Uexp : IAsset
    {

        public IUasset UassetData;

        public List<List<string>> Strings { get; set; }  //[Text id,Text Value,...]
        private int _CurrentIndex;
        public bool IsGood { get; set; } = true;
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

        
     public   List<StringNode> StringNodes { get; set; }
        public bool DumpNameSpaces { get; set; } = false;

        public Uexp(IUasset UassetObject,bool dumpnamespaces=false)
        {
            UassetData = UassetObject;
            Strings = new List<List<string>>();
            CurrentIndex = 0;

            if (dumpnamespaces) 
            {
                StringNodes = new List<StringNode>();
               this.DumpNameSpaces = dumpnamespaces;
            }
            
            ReadOrEdit();
        }


        public static IUasset GetUasset(string uassetpath)
        {
            var StreamFile = File.Open(uassetpath, FileMode.Open, FileAccess.Read);
            var array = new byte[4];
            StreamFile.Read(array, 0, array.Length);
            StreamFile.Close();

            //Todo
            if (array[0] == 0xC1 && array[1] == 0x83 && array[2] == 0x2A && array[3] == 0x9E)//pak -> uasset
            {
                return new Uasset(uassetpath);
            }
            else//utoc -> uasset
            {
                return new IoPackage(uassetpath);
            }

        }

        private void ReadOrEdit(bool Modify = false)
        {

            for (int n = 0; n < UassetData.Exports_Directory.Count; n++)
            {
                ExportIndex = n;
                MemoryList memoryList = new MemoryList(UassetData.Exports_Directory[n].ExportData);
                try
                {
                    ConsoleMode.Print("Block Start offset: " + UassetData.Exports_Directory[n].ExportStart.ToString(), ConsoleColor.DarkRed);
                    ConsoleMode.Print("Block Size: " + UassetData.Exports_Directory[n].ExportLength.ToString(), ConsoleColor.DarkRed);

                    memoryList.Seek(0); //Seek to beginning of Block

                    if (UassetData.UseMethod2)
                    {
                        new UDataTable(memoryList, this, Modify);
                        continue;
                    }

                    ConsoleMode.Print(UassetData.GetExportPropertyName(UassetData.Exports_Directory[n].ExportClass), ConsoleColor.DarkRed);


                    if (memoryList.GetByteValue(false) == 0 && UassetData.GetExportPropertyName(UassetData.Exports_Directory[n].ExportClass) != "MovieSceneCompiledData" && memoryList.GetIntValue(false) > UassetData.NAMES_DIRECTORY.Count)
                    {
                        memoryList.Skip(2);
                        goto Start;
                    }


                    ConsoleMode.Print($"-----------{n}------------", ConsoleColor.Red);
                    _ = new StructProperty(memoryList, this, UassetData.UseFromStruct, false, Modify);
                    ConsoleMode.Print($"-----------End------------", ConsoleColor.Red);

                    if (memoryList.EndofFile())
                    {
                        continue;
                    }
                Start:
                    ConsoleMode.Print($"-----------{n}------------", ConsoleColor.DarkRed);
                    switch (UassetData.GetExportPropertyName(UassetData.Exports_Directory[n].ExportClass))
                    {
                        case "StringTable":
                            new StringTable(memoryList, this, Modify);
                            break;
                        case "CompositeDataTable":
                        case "DataTable":
                            if (memoryList.GetIntValue(false) != -5)
                            {
                                new DataTable(memoryList, this, Modify);
                            }
                            else
                            {
                                //For not effect in original file structure
                                if (memoryList.GetIntValue(false) != -5)
                                {
                                    new DataTable(memoryList, this, Modify);
                                }
                                else
                                {
                                    new UDataTable(memoryList, this, Modify);
                                }
                            }
                            break;
                        case "Spreadsheet":

                            new Spreadsheet(memoryList, this, Modify);
                            break;
                        case "Function":
                            new Function(memoryList, this, Modify);
                            break;
                        case "REDLocalizeTextData":
                            new REDLocalizeTextData(memoryList, this, Modify);
                            break;
                        case "REDLibraryTextData":
                            new REDLibraryTextData(memoryList, this, Modify);
                            break;
                        case "REDAdvTextData":
                            new REDAdvTextData(memoryList, this, Modify);
                            break;
                        case "MuseStringTable":
                            new MuseStringTable(memoryList, this, Modify);
                            break;
                        case "J5BinderAsset":
                            new J5BinderAsset(memoryList, this, Modify);
                            break;
                    }
                    ConsoleMode.Print($"-----------End------------", ConsoleColor.DarkRed);
                }
                catch (Exception ex)
                {
                    ConsoleMode.Print("Skip this export:\n" + ex.ToString(), ConsoleColor.Red, ConsoleMode.ConsoleModeType.Error);
                    // Skip this export
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
            UassetData.UpdateOffset();
            if (UassetData.IsNotUseUexp)
            {
                MakeBlocks();
                UassetData.UassetFile.WriteFile(System.IO.Path.ChangeExtension(FilPath, FilPath.ToLower().EndsWith(".umap") ? ".umap" : ".uasset"));
            }
            else
            {
                MemoryList UexpData = MakeBlocks();
                UassetData.UassetFile.WriteFile(System.IO.Path.ChangeExtension(FilPath, FilPath.ToLower().EndsWith(".umap") ? ".umap" : ".uasset"));
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

                if (!UassetData.IOFile)
                {
                    UassetData.UassetFile.Add(2653586369);
                }
                return UassetData.UassetFile;
            }
            else
            {

                MemoryList memoryList = new MemoryList();
                UassetData.Exports_Directory.ForEach(x =>
                {
                    memoryList.MemoryListData.AddRange(x.ExportData);
                });
                memoryList.Add(2653586369);
                return memoryList;
            }
        }

        public void AddItemsToDataGridView(DataGridView dataGrid)
        {
            dataGrid.DataSource = null;
            dataGrid.Rows.Clear();
            dataGrid.Columns.Clear();

            var dataTable = new System.Data.DataTable();
            dataTable.Columns.Add("Name");
            dataTable.Columns["Name"].ReadOnly = true;
            dataTable.Columns.Add("Text value");
            dataTable.Columns.Add("index",typeof(int));

            int i = 0;
            foreach (var item in Strings)
            {
                dataTable.Rows.Add(item[0], item[1], i++);
            }

            dataGrid.DataSource = dataTable;

            dataGrid.Columns["Text value"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGrid.Columns["index"].Visible = false;


            // You can remove these event handlers if not needed.
            dataGrid.CellFormatting += DataGrid_CellFormatting;
            dataGrid.CellToolTipTextNeeded += DataGrid_CellToolTipTextNeeded;
        }

        private void DataGrid_CellToolTipTextNeeded(object sender, DataGridViewCellToolTipTextNeededEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var dataGridView = (DataGridView)sender;

                bool isFound = false;
                foreach (DataGridViewColumn column in dataGridView.Columns)
                {
                    if (column.Name == "index")
                    {
                        isFound = true;
                    }
                }
                if (!isFound) return;


                if (dataGridView.Columns[e.ColumnIndex].Name == "Name")
                {
                    var rowIndexCell = dataGridView.Rows[e.RowIndex].Cells["index"];

                    if (rowIndexCell != null && rowIndexCell.Value != null)
                    {
                        var rowIndex = Convert.ToInt32(rowIndexCell.Value);

                        if (rowIndex >= 0 && rowIndex < Strings.Count)
                        {
                            var item = Strings[rowIndex];

                            if (item.Count > 2)
                            {
                                e.ToolTipText = item[2];
                            }
                        }
                    }
                }
            }
        }

        private void DataGrid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var dataGridView = (DataGridView)sender;

                bool isFound = false;
                foreach (DataGridViewColumn column in dataGridView.Columns)
                {
                    if (column.Name == "index")
                    {
                        isFound = true;
                    }
                }
                if (!isFound) return;

                if (dataGridView.Columns[e.ColumnIndex].Name == "Name")
                {
                    var rowIndexCell = dataGridView.Rows[e.RowIndex].Cells["index"];
                    if (rowIndexCell != null && rowIndexCell.Value != null)
                    {
                        var rowIndex = Convert.ToInt32(rowIndexCell.Value);

                        if (rowIndex >= 0 && rowIndex < Strings.Count)
                        {
                            var item = Strings[rowIndex];

                            if (item.Count > 3)
                            {
                                e.CellStyle.BackColor = ColorTranslator.FromHtml(item[3]);
                            }
                            if (item.Count > 4)
                            {
                                e.CellStyle.ForeColor = ColorTranslator.FromHtml(item[4]);
                            }
                        }
                    }
                }
            }
        }


        public void LoadFromDataGridView(DataGridView dataGrid)
        {
            foreach (DataGridViewRow row in dataGrid.Rows)
            {
                if (row.Cells["index"].Value is int itemIndex &&
                    row.Cells["Text value"].Value != null)
                {
                    Strings[itemIndex][1] = row.Cells["Text value"].Value.ToString();
                }
            }
        }

        public List<List<string>> ExtractTexts()
        {
            return Strings;
        }

        public void ImportTexts(List<List<string>> strings)
        {
            Strings = strings;
        }
    }
}
