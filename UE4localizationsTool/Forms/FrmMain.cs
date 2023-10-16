using AssetParser;
using System;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using UE4localizationsTool.Controls;
using UE4localizationsTool.Core.Hash;
using UE4localizationsTool.Core.locres;
using UE4localizationsTool.Forms;
using UE4localizationsTool.Helper;

namespace UE4localizationsTool
{
    public partial class FrmMain : NForm
    {
        IAsset Asset;
        String ToolName = Application.ProductName + " v" + Application.ProductVersion;
        string FilePath = "";
        bool SortApply = false;
        public FrmMain()
        {
            InitializeComponent();
            dataGridView1.RowCountChanged += (x, y) => this.UpdateCounter();
            ResetControls();
            pictureBox1.Height = menuStrip1.Height;
            darkModeToolStripMenuItem.Checked = Properties.Settings.Default.DarkMode;
            Checkforupdates.Checked = Properties.Settings.Default.CheckForUpdates;
        }

        private void OpenFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "All localizations files|*.uasset;*.locres;*.umap|Uasset File|*.uasset|Locres File|*.locres|Umap File|*.umap";
            ofd.Title = "Open localizations File";


            if (ofd.ShowDialog() == DialogResult.OK)
            {

                LoadFile(ofd.FileName);
            }
        }


        public async void LoadFile(string filePath)
        {
            ResetControls();
            ControlsMode(false);

            try
            {
                StatusMessage("loading File...", "loading File, please wait.");

                if (filePath.ToLower().EndsWith(".locres"))
                {
                    Asset = await Task.Run(() => new LocresFile(filePath));
                    locresOprationsToolStripMenuItem.Visible = true;
                    CreateBackupList();
                }
                else if (filePath.ToLower().EndsWith(".uasset") || filePath.ToLower().EndsWith(".umap"))
                {
                    IUasset Uasset = await Task.Run(() => Uexp.GetUasset(filePath));
                    Uasset.UseMethod2 = Uasset.UseMethod2 ? Uasset.UseMethod2 : Method2.Checked;
                    Asset = await Task.Run(() => new Uexp(Uasset));
                    CreateBackupList();
                    if (!Asset.IsGood)
                    {
                        StateLabel.Text = "Warning: This file is't fully parsed and may not contain some text.";
                    }
                }

                this.FilePath = filePath;
                this.Text = ToolName + " - " + Path.GetFileName(FilePath);
                ControlsMode(true);
                CloseFromState();
            }
            catch (Exception ex)
            {
                CloseFromState();
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void CreateBackupList()
        {
            Asset.AddItemsToDataGridView(dataGridView1);
        }

        private void ResetControls()
        {
            FilePath = "";
            StateLabel.Text = "";
            DataCount.Text = "";
            Text = ToolName;
            SortApply = false;
            locresOprationsToolStripMenuItem.Visible = false;
        }

        private void ControlsMode(bool Enabled)
        {
            saveToolStripMenuItem.Enabled = Enabled;
            exportAllTextToolStripMenuItem.Enabled = Enabled;
            importAllTextToolStripMenuItem.Enabled = Enabled;
            undoToolStripMenuItem.Enabled = Enabled;
            redoToolStripMenuItem.Enabled = Enabled;
            filterToolStripMenuItem.Enabled = Enabled;
            noNamesToolStripMenuItem.Enabled = Enabled;
            withNamesToolStripMenuItem.Enabled = Enabled;
            clearFilterToolStripMenuItem.Enabled = Enabled;
            csvFileToolStripMenuItem.Enabled = Enabled;
        }
        enum ExportType
        {
            NoNames = 0,
            WithNames
        }

        private void ExportAll(ExportType exportType)
        {

            if (this.SortApply && !(Asset is LocresFile)) SortDataGrid(2, true);

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Text File|*.txt";
            sfd.Title = "Export All Text";
            sfd.FileName = Path.GetFileName(FilePath) + ".txt";


            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {

                    using (var stream = new StreamWriter(sfd.FileName))
                    {
                        if (exportType == ExportType.WithNames)
                        {
                            stream.WriteLine(@"[~NAMES-INCLUDED~]//Don't edit or remove this line.");
                        }

                        for (int i = 0; i < dataGridView1.Rows.Count; i++)
                        {
                            if (exportType == ExportType.WithNames)
                            {
                                stream.WriteLine(dataGridView1.Rows[i].Cells["Name"].Value.ToString() + "=" + dataGridView1.Rows[i].Cells["Text value"].Value.ToString());
                                continue;
                            }
                            stream.WriteLine(dataGridView1.Rows[i].Cells["Text value"].Value.ToString());
                        }

                    }
                    if (dataGridView1.IsFilterApplied)
                    {
                        MessageBox.Show("Successful export!\n Remember to apply the same filter you using right now before 'import'.", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Successful export!", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch
                {
                    MessageBox.Show("Can't write export file!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                }
            }
        }

        private void importAllTextToolStripMenuItem_Click(object sender, EventArgs e)
        {

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Text File|*.txt;*.csv";
            ofd.Title = "Import All Text";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                if (this.SortApply && !(Asset is LocresFile)) SortDataGrid(2, true);

                if (ofd.FileName.EndsWith(".csv", StringComparison.InvariantCulture))
                {
                    try
                    {
                        CSVFile.Instance.Load(this.dataGridView1, ofd.FileName);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, ToolName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    MessageBox.Show("Successful import!", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }



                string[] DataGridStrings;
                try
                {
                    DataGridStrings = System.IO.File.ReadAllLines(ofd.FileName);
                }
                catch
                {
                    MessageBox.Show("Can't read file or this file is using in Another process", "File is corrupted", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (DataGridStrings.Length < dataGridView1.Rows.Count)
                {
                    MessageBox.Show("This file does't contain enough strings for reimport", "Out of range", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (DataGridStrings[0].StartsWith("[~NAMES-INCLUDED~]", StringComparison.OrdinalIgnoreCase))
                {
                    DataGridStrings = DataGridStrings.Skip(1).ToArray();
                    for (int n = 0; n < DataGridStrings.Length; n++)
                    {
                        try
                        {
                            if (DataGridStrings[n].Contains("="))
                                DataGridStrings[n] = DataGridStrings[n].Split(new char[] { '=' }, 2)[1];
                        }
                        catch
                        {
                            MessageBox.Show($"Corrupted string format in line " + (n + 1), this.Text, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                            return;
                        }

                    }

                }

                for (int n = 0; n < dataGridView1.Rows.Count; n++)
                {
                    dataGridView1.SetValue(dataGridView1.Rows[n].Cells["Text value"], DataGridStrings[n]);
                }
                MessageBox.Show("Successful import!", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);


            }



        }

        private async void SaveFile(object sender, EventArgs e)
        {

            SaveFileDialog sfd = new SaveFileDialog();
            if (FilePath.ToLower().EndsWith(".locres"))
            {
                sfd.Filter = "locres File|*.locres";
            }
            else if (FilePath.ToLower().EndsWith(".uasset"))
            {
                sfd.Filter = "Uasset File|*.uasset";
            }
            else if (FilePath.ToLower().EndsWith(".umap"))
            {
                sfd.Filter = "Umap File|*.umap";
            }

            sfd.Title = "Save localizations file";
            sfd.FileName = Path.GetFileNameWithoutExtension(FilePath) + "_NEW";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    StatusMessage("Saving File...", "Saving File ,please wait.");
                    Asset.LoadFromDataGridView(dataGridView1);
                    await Task.Run(() => Asset.SaveFile(sfd.FileName));
                    MessageBox.Show("Saved Successful.", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {

                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                CloseFromState();
            }
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dataGridView1.Copy();
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dataGridView1.Paste();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }


        private void fontToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Dialog Font select
            FontDialog fd = new FontDialog();
            fd.Font = dataGridView1.Font;
            if (fd.ShowDialog() == DialogResult.OK)
            {
                dataGridView1.Font = fd.Font;
                dataGridView1.AutoResizeRows();
            }
        }

        private void rightToLeftToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dataGridView1.RightToLeft = dataGridView1.RightToLeft == RightToLeft.Yes ? RightToLeft.No : RightToLeft.Yes;
        }


        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dataGridView1.Undo();
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {

            dataGridView1.Redo();
        }

        private void commandLinesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(Program.commandlines, "Command Lines", MessageBoxButtons.OK);
        }

        private void aboutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            new FrmAbout(this).ShowDialog();
        }

        private void clearFilterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dataGridView1.ClearFilter();
        }

        private void UpdateCounter()
        {
            DataCount.Text = "Text count: " + dataGridView1.Rows.Count;
        }


        private void FrmMain_Load(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.CheckForUpdates)
            {
                this.Invoke(new Action(() =>
                   {
                       float ToolVer = 0;
                       string ToolSite = "";

                       using (WebClient client = new WebClient())
                       {
                           try
                           {
                               string UpdateScript = client.DownloadString("https://raw.githubusercontent.com/amrshaheen61/UE4LocalizationsTool/master/UE4localizationsTool/UpdateInfo.txt");

                               if (UpdateScript.StartsWith("UpdateFile", false, CultureInfo.InvariantCulture))
                               {
                                   var lines = Regex.Split(UpdateScript, "\r\n|\r|\n");
                                   foreach (string Line in lines)
                                   {

                                       if (Line.StartsWith("Tool_UpdateVer", false, CultureInfo.InvariantCulture))
                                       {
                                           ToolVer = float.Parse(Line.Split(new char[] { '=' }, 2)[1].Trim());
                                       }
                                       if (Line.StartsWith("Tool_UpdateSite", false, CultureInfo.InvariantCulture))
                                       {
                                           ToolSite = Line.Split(new char[] { '=' }, 2)[1].Trim();
                                       }
                                   }

                                   if (ToolVer > float.Parse(Application.ProductVersion))
                                   {

                                       DialogResult message = MessageBox.Show("There is an update available\nDo you want to download it?", "Update available", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                                       if (message == DialogResult.Yes)
                                       {
                                           Process.Start(new ProcessStartInfo { FileName = ToolSite, UseShellExecute = true });
                                           Application.Exit();
                                       }


                                   }
                               }

                           }
                           catch
                           {
                               //n
                           }
                       }
                   })

                       );
            }

        }

        private void noNamesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportAll(ExportType.NoNames);
        }

        private void withNamesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportAll(ExportType.WithNames);
        }

        private void valueToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        void SortDataGrid(int Cel, bool Ascending)
        {
            this.SortApply = true;
            if (Ascending)
            {
                dataGridView1.Sort(dataGridView1.Columns[Cel], System.ComponentModel.ListSortDirection.Ascending);
                return;
            }
            dataGridView1.Sort(dataGridView1.Columns[Cel], System.ComponentModel.ListSortDirection.Descending);
        }

        private void ascendingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SortDataGrid(0, true);
        }

        private void descendingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SortDataGrid(0, false);
        }

        private void ascendingToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            SortDataGrid(1, true);
        }

        private void descendingToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            SortDataGrid(1, false);
        }

        private void dataGridView1_Sorted(object sender, EventArgs e)
        {
            this.SortApply = true;
        }

        private void FrmMain_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.All;
            }
        }

        private void FrmMain_DragDrop(object sender, DragEventArgs e)
        {
            string[] array = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (array[0].Length >= 1 && (array[0].EndsWith(".uasset") || array[0].EndsWith(".umap") || array[0].EndsWith(".locres")))
            {
                LoadFile(array[0]);
            }
        }

        private void donateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://www.paypal.me/amrshaheen60");
        }

        private void Method2_CheckedChanged(object sender, EventArgs e)
        {

            if (Method2.Checked)
            {
                pictureBox1.Visible = true;
                fileToolStripMenuItem.Margin = new Padding(5, 0, 0, 0);
            }
            else
            {
                pictureBox1.Visible = false;
                fileToolStripMenuItem.Margin = new Padding(0, 0, 0, 0);
            }


        }

        private void darkModeToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            bool IsDark = Properties.Settings.Default.DarkMode;
            Properties.Settings.Default.DarkMode = darkModeToolStripMenuItem.Checked;
            Properties.Settings.Default.Save();

            if (IsDark != darkModeToolStripMenuItem.Checked)
                Application.Restart();
        }

        private void Checkforupdates_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.CheckForUpdates = Checkforupdates.Checked;
            Properties.Settings.Default.Save();
        }

        private void csvFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "CSV File|*.csv";
            sfd.Title = "Export All Text";
            sfd.FileName = Path.GetFileName(FilePath) + ".csv";


            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    CSVFile.Instance.Save(this.dataGridView1, sfd.FileName);

                    if (dataGridView1.IsFilterApplied)
                    {
                        MessageBox.Show("Successful export!\n Remember to apply the same filter you using right now before 'import'.", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Successful export!", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch
                {
                    MessageBox.Show("Can't write export file!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                }
            }
        }

        private void find_Click(object sender, EventArgs e)
        {
            searchBox.Show();

        }

        private void filterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dataGridView1.Filter();
        }

        private void dataGridView1_FilterApplied(object sender, EventArgs e)
        {
            filterToolStripMenuItem.Visible = false;
            clearFilterToolStripMenuItem.Visible = true;
        }

        private void dataGridView1_FilterCleared(object sender, EventArgs e)
        {
            filterToolStripMenuItem.Visible = true;
            clearFilterToolStripMenuItem.Visible = false;
        }

        private void removeSelectedRowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedCells.Count == 0)
            {
                MessageBox.Show("No row(s) selected to remove.", "Remove Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }


            DialogResult result = MessageBox.Show("Are you sure you want to remove the selected row(s)?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                dataGridView1.BeginEdit(false);
                foreach (DataGridViewCell cell in dataGridView1.SelectedCells)
                {
                    if (cell.RowIndex >= 0 && cell.RowIndex < dataGridView1.Rows.Count)
                    {
                        dataGridView1.Rows.Remove(cell.OwningRow);
                    }
                }
                dataGridView1.EndEdit();
            }
        }

        private void editSelectedRowToolStripMenuItem_Click(object sender, EventArgs e)
        {

            if (dataGridView1.SelectedCells.Count > 1 || dataGridView1.SelectedCells.Count == 0)
            {

                MessageBox.Show("Please select a single cell to edit.", "Edit Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;

            }

            var EntryEditor = new FrmLocresEntryEditor(dataGridView1, (LocresFile)Asset);
            if (EntryEditor.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    EntryEditor.EditRow(dataGridView1);
                    MessageBox.Show("The row edited successfully.", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while editing the row:\n " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void addNewRowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var EntryEditor = new FrmLocresEntryEditor(this);

            if (EntryEditor.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    dataGridView1.BeginEdit(false);
                    EntryEditor.AddRow(dataGridView1);
                    dataGridView1.EndEdit();
                    MessageBox.Show("New row added successfully.", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while adding the row:\n " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }
        }

        private void mergeLocresFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Locres File(s)|*.locres";
            ofd.Title = "Select localization file(s)";
            ofd.Multiselect = true;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                StatusMessage("Merging locres files...", "Merging locres files, please wait.");
                var dataTable = new System.Data.DataTable();

                if (dataGridView1.DataSource is System.Data.DataTable sourceDataTable)
                {
                    foreach (DataColumn col in sourceDataTable.Columns)
                    {
                        dataTable.Columns.Add(col.ColumnName, col.DataType);
                    }
                }


                try
                {
                    foreach (string fileName in ofd.FileNames)
                    {
                        foreach (var names in new LocresFile(fileName))
                        {
                            foreach (var table in names)
                            {
                                string name = string.IsNullOrEmpty(names.Name) ? table.key : names.Name + "::" + table.key;
                                string textValue = table.Value;
                                dataTable.Rows.Add(name, textValue, new HashTable(names.NameHash, table.keyHash, table.ValueHash));
                            }
                        }
                    }

                    ((System.Data.DataTable)dataGridView1.DataSource).Merge(dataTable);


                    MessageBox.Show("Locres file(s) merged successfully.", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while merging locres file(s):\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                CloseFromState();
            }
        }
        private void StatusMessage(string title, string message)
        {
            StatusTitle.Text = title;
            StatusText.Text = message;
            StatusBlock.Visible = true;
        }

        private void CloseFromState()
        {
            StatusBlock.Visible = false;
        }

        private void mergeUassetFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "All localizations files|*.uasset;*.umap|Uasset File|*.uasset|Umap File|*.umap";
            ofd.Title = "Open localizations File";
            ofd.Multiselect = true;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                StatusMessage("Merging uasset files...", "Merging uasset files, please wait.");

                var dataTable = new System.Data.DataTable();

                if (dataGridView1.DataSource is System.Data.DataTable sourceDataTable)
                {
                    foreach (DataColumn col in sourceDataTable.Columns)
                    {
                        dataTable.Columns.Add(col.ColumnName, col.DataType);
                    }
                }


                try
                {
                    foreach (string fileName in ofd.FileNames)
                    {
                        foreach (var Strings in new Uexp(Uexp.GetUasset(fileName), true).StringNodes)
                        {
                            var locresasset = Asset as LocresFile;
                            var HashTable = new HashTable(locresasset.CalcHash(Strings.NameSpace), locresasset.CalcHash(Strings.Key), Strings.Value.StrCrc32());

                            dataTable.Rows.Add(Strings.GetName(), Strings.Value, HashTable);
                        }
                    }

                      ((System.Data.DataTable)dataGridView1.DataSource).Merge(dataTable);


                    MessageBox.Show("Uasset file(s) merged successfully.", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while merging uasset file(s):\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                CloseFromState();

            }
        }

        private void dataGridView1_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {

        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dataGridView1_CellValidated(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dataGridView1_CellValuePushed(object sender, DataGridViewCellValueEventArgs e)
        {

        }

        private void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
        }
    }
}
