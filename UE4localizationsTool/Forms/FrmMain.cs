using AssetParser;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace UE4localizationsTool
{

    public partial class FrmMain : Form
    {
        struct DataRow
        {
            public int Index;
            public string StringValue;
        }


        Uexp Uexp;
        locres locres;
        String ToolName = Application.ProductName + " v" + Application.ProductVersion;
        string FilePath = "";
        FrmState state;
        Stack<DataRow> BackupDataUndo;
        Stack<DataRow> BackupDataRedo;
        List<List<string>> ListrefValues;
        ReadOnlyCollection<List<string>> ListBackupValues;
        bool Filter = false;
        bool SortApply = false;
        bool ClearTemp = true;
        List<int> FindArray;
        int FindIndex;
        string OldFind;
        public FrmMain()
        {
            InitializeComponent();
            dataGridView1.RowsAdded += (x, y) => this.UpdateCounter();
            dataGridView1.RowsRemoved += (x, y) => this.UpdateCounter();
            ResetControls();
        }



        private void AddToDataView()
        {

            if (ListrefValues == null) return;
            int Index = 0;
    
            foreach (var item in ListrefValues)
            {
                dataGridView1.Rows.Add(item[0], item[1], Index);
                //dataGridView1.Rows[Index].Cells[1].Style.WrapMode = DataGridViewTriState.True;
                //dataGridView1.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill; 

                if (item.Count >2)
                {
                    dataGridView1.Rows[Index].Cells[1].ToolTipText = item[2];

                    if (item.Count>3) 
                    {
                        dataGridView1.Rows[Index].Cells[0].Style.BackColor = System.Drawing.ColorTranslator.FromHtml(item[3]);
                    }
                    if (item.Count > 4)
                    {
                        dataGridView1.Rows[Index].Cells[0].Style.ForeColor = System.Drawing.ColorTranslator.FromHtml(item[4]);
                    }
                }


                Index++;
            }
            dataGridView1.AutoResizeRows();
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




        private async void LoadFile(string filePath)
        {
            ResetControls();
            ControlsMode(false);
            try
            {
                state = new FrmState(this, "loading File", "loading File please wait...");
                this.BeginInvoke(new Action(() => state.ShowDialog()));


                if (filePath.ToLower().EndsWith(".locres"))
                {
                    locres = await Task.Run(() => new locres(filePath));
                    ListrefValues = locres.Strings;
                    ListBackupValues = ListrefValues.AsReadOnly();
                    AddToDataView();
                }
                else if (filePath.ToLower().EndsWith(".uasset")|| filePath.ToLower().EndsWith(".umap"))
                {
                    Uasset Uasset = await Task.Run(() => new Uasset(filePath));
                    Uexp = await Task.Run(() => new Uexp(Uasset));
                    ListrefValues = Uexp.Strings;
                    ListBackupValues = ListrefValues.AsReadOnly();
                    AddToDataView();
                    if (!Uexp.IsGood)
                    {
                        StateLabel.Text = "Warning: This file is't fully parsed and may not contain some text.";
                    }

                }

                this.FilePath = filePath;
                this.Text = ToolName + " - " + Path.GetFileName(FilePath);
                ControlsMode(true);
                state.Close();
            }
            catch (Exception ex)
            {
                state.Close();
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void ResetControls()
        {
            dataGridView1.Rows.Clear();
            FilePath = "";
            StateLabel.Text = "";
            DataCount.Text = "";
            Text = ToolName;
            BackupDataUndo = new Stack<DataRow>();
            BackupDataRedo = new Stack<DataRow>();
            Filter = false;
            SortApply = false;
            FindArray = new List<int>();
            ClearTemp = true;
            FindIndex = 0;
            OldFind = "";
            searchcount.Text="";
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
        }
        enum ExportType
        {
            NoNames = 0,
            WithNames
        }

        private void ExportAll(ExportType exportType)
        {

            List<string> DataGridStrings = new List<string>();

            if (this.SortApply) SortDataGrid(2, true);
            if (exportType == ExportType.WithNames)
            {
                DataGridStrings.Add(@"[~NAMES-INCLUDED~]//Don't edit or remove this line.");
            }

            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                if (exportType == ExportType.WithNames)
                {
                    DataGridStrings.Add(ObjectToString(dataGridView1.Rows[i].Cells[0].Value) + "=" + ObjectToString(dataGridView1.Rows[i].Cells[1].Value));
                    continue;
                }
                DataGridStrings.Add(ObjectToString(dataGridView1.Rows[i].Cells[1].Value));
            }
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Text File|*.txt";
            sfd.Title = "Export All Text";
            sfd.FileName = Path.GetFileName(FilePath) + ".txt";


            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    System.IO.File.WriteAllLines(sfd.FileName, DataGridStrings);

                    if (Filter)
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
            ofd.Filter = "Text File|*.txt";
            ofd.Title = "Import All Text";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
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


                if (this.SortApply) SortDataGrid(2, true);
                for (int n = 0; n < dataGridView1.Rows.Count; n++)
                {
                    dataGridView1.Rows[n].Cells[1].Value = DataGridStrings[n];
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
                    state = new FrmState(this, "Saving File", "Saving File please wait...");
                    this.BeginInvoke(new Action(() => state.ShowDialog()));
                    if (FilePath.ToLower().EndsWith(".locres"))
                    {
                        await Task.Run(() => locres.SaveFile(sfd.FileName));

                    }
                    else if (FilePath.ToLower().EndsWith(".uasset") || FilePath.ToLower().EndsWith(".umap"))
                    {
                      
                       await Task.Run(() => Uexp.SaveFile(sfd.FileName));
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                state.Close();
            }
        }

        private void SearchHide_Click(object sender, EventArgs e)
        {
            SearchPanal.Visible = false;
            dataGridView1.Height += SearchPanal.Height;
        }

        private void search_Click(object sender, EventArgs e)
        {
            SearchPanal.Visible = !SearchPanal.Visible;
            if (SearchPanal.Visible)
            {

                dataGridView1.Height -= SearchPanal.Height;
                if (dataGridView1.SelectedCells.Count > 0)
                {
                    InputSearch.Text=ObjectToString( dataGridView1.SelectedCells[0].Value);
                }
                InputSearch.Focus();
                InputSearch.SelectAll();
            }
            else
            {
                dataGridView1.Height += SearchPanal.Height;
            }


        }


        private void Find_Click(object sender, EventArgs e)
        {
            FindArray.Clear();
            OldFind = InputSearch.Text;
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                if (ObjectToString(dataGridView1.Rows[i].Cells[1].Value).ToLower().Contains(InputSearch.Text.ToLower()))
                {
                    FindArray.Add(i);
                }
            }

            if (FindArray.Count == 0 || InputSearch.Text == "")
            {
                MessageBox.Show($"can't find '{InputSearch.Text}'", "No results", MessageBoxButtons.OK, MessageBoxIcon.Information);
                FindArray.Clear();
                searchcount.Text = "found: " + FindArray.Count;
                return;
            }
            dataGridView1.ClearSelection();
            dataGridView1.Rows[FindArray[0]].Selected = true;
            dataGridView1.FirstDisplayedScrollingRowIndex = FindArray[0];
            FindIndex = 0;
            searchcount.Text = "found: " + FindArray.Count;
        }

        private void FindNext_Click(object sender, EventArgs e)
        {
            if (FindArray.Count == 0 || OldFind != InputSearch.Text)
            {
                Find_Click(sender, e);
                return;
            }
            FindIndex++;
            if (FindIndex < FindArray.Count)
            {
                dataGridView1.ClearSelection();
                dataGridView1.Rows[FindArray[FindIndex]].Selected = true;
                dataGridView1.FirstDisplayedScrollingRowIndex = FindArray[FindIndex];
            }
            else
            {
                FindIndex = 0;
                dataGridView1.ClearSelection();
                dataGridView1.Rows[FindArray[FindIndex]].Selected = true;
                dataGridView1.FirstDisplayedScrollingRowIndex = FindArray[FindIndex];
            }
        }

        private void FindPrevious_Click(object sender, EventArgs e)
        {
            if (FindArray.Count == 0 || OldFind != InputSearch.Text)
            {
                Find_Click(sender, e);
                return;
            }
            FindIndex--;
            if (FindIndex > -1)
            {
                dataGridView1.ClearSelection();
                dataGridView1.Rows[FindArray[FindIndex]].Selected = true;
                dataGridView1.FirstDisplayedScrollingRowIndex = FindArray[FindIndex];
            }
            else
            {
                FindIndex = FindArray.Count - 1;
                dataGridView1.ClearSelection();
                dataGridView1.Rows[FindArray[FindIndex]].Selected = true;
                dataGridView1.FirstDisplayedScrollingRowIndex = FindArray[FindIndex];
            }
        }

        private void dataGridView1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                contextMenuStrip1.Show(this, new Point(e.X + ((Control)sender).Left, e.Y + ((Control)sender).Top));

            }
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedCells.Count > 0)
                Clipboard.SetText(dataGridView1.SelectedCells[0].Value.ToString());
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedCells.Count > 0)
            {
                if (BackupDataUndo.Count == 0)
                {
                    BackupDataRedo.Clear();
                }
                BackupDataUndo.Push(new DataRow() { Index = dataGridView1.SelectedCells[0].RowIndex, StringValue = ObjectToString(dataGridView1.Rows[dataGridView1.SelectedCells[0].RowIndex].Cells[1].Value) });
                dataGridView1.Rows[dataGridView1.SelectedCells[0].RowIndex].Cells[1].Value = Clipboard.GetText();
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void InputSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (!InputSearch.Focused)
            {
                InputSearch.Focus();
            }

            if (e.KeyCode == Keys.Enter)
            {
                FindNext_Click(sender, e);
            }

        }

        private void dataGridView1_Scroll(object sender, ScrollEventArgs e)
        {
            dataGridView1.ClearSelection();
            dataGridView1.Rows[dataGridView1.FirstDisplayedScrollingRowIndex].Selected = true;
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

        private void dataGridView1_KeyDown(object sender, KeyEventArgs e)
        {

            if (dataGridView1.CancelEdit() && dataGridView1.Focused)
            {
                if (e.KeyCode == Keys.V && e.Control)
                {
                    pasteToolStripMenuItem_Click(sender, e);
                }
                else if (e.KeyCode == Keys.Z && e.Control)
                {
                    undoToolStripMenuItem_Click(sender, e);

                }
                else if ((e.KeyCode == Keys.Y && e.Control) || (e.KeyCode == Keys.Z && e.Control && e.Shift))
                {
                    redoToolStripMenuItem_Click(sender, e);
                }


                else if (e.KeyCode == Keys.L && e.Control && e.Alt)
                {
                    dataGridView1.RightToLeft = RightToLeft.No;
                }
                else if (e.KeyCode == Keys.R && e.Control && e.Alt)
                {
                    dataGridView1.RightToLeft = RightToLeft.Yes;
                }


            }
        }

        private void rightToLeftToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dataGridView1.RightToLeft = dataGridView1.RightToLeft == RightToLeft.Yes ? RightToLeft.No : RightToLeft.Yes;
        }


        private void dataGridView1_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            BackupDataUndo.Push(new DataRow() { Index = e.RowIndex, StringValue = ObjectToString(dataGridView1.Rows[e.RowIndex].Cells[1].Value) });
        }

        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {

            if (BackupDataUndo.Peek().StringValue == ObjectToString(dataGridView1.Rows[e.RowIndex].Cells[1].Value))
            {
                BackupDataUndo.Pop();
                return;
            }

            if (BackupDataUndo.Count == 0)
            {
                BackupDataRedo.Clear();
            }
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView1.Created)
            {
                ListrefValues[int.Parse(dataGridView1.Rows[e.RowIndex].Cells[2].Value.ToString())][1] = ObjectToString(dataGridView1.Rows[e.RowIndex].Cells[1].Value);
                dataGridView1.Rows[e.RowIndex].Cells[1].Style.BackColor = System.Drawing.Color.FromArgb(255, 204, 153);
                this.Text = ToolName + " - " + Path.GetFileName(FilePath) + "*";
                if (ClearTemp)
                {
                    BackupDataRedo.Clear();
                    ClearTemp = false;
                }
            }
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {

            if (BackupDataUndo.Count > 0)
            {

                DataRow dataRow = BackupDataUndo.Pop();
                BackupDataRedo.Push(new DataRow() { Index = dataRow.Index, StringValue = ObjectToString(dataGridView1.Rows[dataRow.Index].Cells[1].Value) });
                ClearTemp = false;
                //MessageBox.Show(dataRow.StringValue);
                dataGridView1.Rows[dataRow.Index].Cells[1].Value = dataRow.StringValue;
                ClearTemp = true;
                if (dataRow.StringValue == ListBackupValues[dataRow.Index][1])
                    dataGridView1.Rows[dataRow.Index].Cells[1].Style.BackColor = System.Drawing.Color.FromArgb(255, 255, 255);
                else
                {
                    dataGridView1.Rows[dataRow.Index].Cells[1].Style.BackColor = System.Drawing.Color.FromArgb(255, 204, 153);
                }
                dataGridView1.ClearSelection();
                dataGridView1.Rows[dataRow.Index].Selected = true;
                if (BackupDataUndo.Count==0)
                {
                    this.Text = ToolName + " - " + Path.GetFileName(FilePath);
                }
            }

        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {

            if (BackupDataRedo.Count > 0)
            {
                //MessageBox.Show(BackupDataRedo.Peek().StringValue);

                DataRow dataRow = BackupDataRedo.Pop();
                
                BackupDataUndo.Push(new DataRow() { Index = dataRow.Index, StringValue = ObjectToString(dataGridView1.Rows[dataRow.Index].Cells[1].Value) });
                ClearTemp = false;
                dataGridView1.Rows[dataRow.Index].Cells[1].Value = dataRow.StringValue;
                ClearTemp = true;
                dataGridView1.Rows[dataRow.Index].Cells[1].Style.BackColor = System.Drawing.Color.FromArgb(255, 204, 153);
               

                dataGridView1.ClearSelection();
                dataGridView1.Rows[dataRow.Index].Selected = true;
            }
        }

        private void commandLinesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(Program.commandlines, "Command Lines", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void aboutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            new FrmAbout(this).ShowDialog();
        }




        private void FilterApply(int Cel)
        {


            if (ListrefValues == null)
            {
                return;
            }

            FrmFilter frmFilter = new FrmFilter(this);
            if (Cel == 0)
                frmFilter.Text = "Filter by name";
            else
                frmFilter.Text = "Filter by value";

            if (frmFilter.ShowDialog() == DialogResult.OK)
            {
                Filter = true;
                clearFilterToolStripMenuItem.Enabled = true;
                dataGridView1.Rows.Clear();
                for (int x = 0; x < ListrefValues.Count; x++)
                {

                    bool CanAdd = false;


                    frmFilter.ArrayValues.ForEach(Value =>
                    {

                        if (frmFilter.UseMatching)
                        {
                            if (frmFilter.RegularExpression)
                            {
                                try
                                {
                                    if (Regex.IsMatch(ListrefValues[x][Cel], Value))
                                    {
                                        CanAdd = true;
                                    }

                                }
                                catch { }
                            }
                            else
                            {
                                if (ListrefValues[x][Cel] == Value)
                                {
                                    CanAdd = true;
                                }
                            }
                        }
                        else
                        {
                            if (frmFilter.RegularExpression)
                            {
                                try
                                {
                                    if (Regex.IsMatch(ListrefValues[x][Cel], Value, RegexOptions.IgnoreCase))
                                    {
                                        CanAdd = true;
                                    }
                                }
                                catch { }
                            }
                            else
                            {
                                if (ListrefValues[x][Cel].IndexOf(Value, StringComparison.OrdinalIgnoreCase) >= 0)
                                {
                                    CanAdd = true;
                                }
                            }
                        }
                    });

                    if (CanAdd)
                    {
                        if (!frmFilter.ReverseMode)
                            dataGridView1.Rows.Add(ListrefValues[x][0], ListrefValues[x][1], x);
                    }
                    else if (frmFilter.ReverseMode)
                    {
                        dataGridView1.Rows.Add(ListrefValues[x][0], ListrefValues[x][1], x);
                    }



                }
            }
        }











        private void byNameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FilterApply(0);
        }
        private void byValueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FilterApply(1);
        }

        private void clearFilterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Filter)
            {
                dataGridView1.Rows.Clear();
                AddToDataView();
                Filter = false;
                clearFilterToolStripMenuItem.Enabled = false;
            }
        }

        private void UpdateCounter()
        {
            DataCount.Text = "Text count: " + dataGridView1.Rows.Count;
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private string ObjectToString(object Value)
        {
            if (!(Value is null))
            {
                return Value.ToString();
            }
            return "";
        }

        private async void FrmMain_Load(object sender, EventArgs e)
        {

            float ToolVer = 0;
            string ToolSite = "";

            using (WebClient client = new WebClient())
            {
                try
                {
                    string UpdateScript = await Task.Run(()=> client.DownloadString("https://raw.githubusercontent.com/amrshaheen61/UE4LocalizationsTool/master/UE4localizationsTool/UpdateInfo.txt"));

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
            LoadFile(array[0]);

        }

        private void DataCount_TextChanged(object sender, EventArgs e)
        {
            DataCount.Location =new Point( Width - DataCount.Width - 22 /*padding*/ , label1.Location.Y);
        }

        private void donateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://www.paypal.me/amrshaheen60");
        }
    }
}
