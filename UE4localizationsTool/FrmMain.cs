using AssetParser;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace UE4localizationsTool
{
    public partial class FrmMain : Form
    {
        Uasset Uasset;
        Uexp Uexp;
        locres locres;

        String ToolName = Application.ProductName + " v" + Application.ProductVersion;
        string FilePath = "";
        FrmState state;
        public FrmMain()
        {
            InitializeComponent();
            this.Text = ToolName;
            saveToolStripMenuItem.Enabled = false;
            exportAllTextToolStripMenuItem.Enabled = false;
            importAllTextToolStripMenuItem.Enabled = false;
        }

        private void AddToDataView(List<List<string>> strings)
        {
            int Index = 0;

            foreach (var item in strings)
            {
                dataGridView1.Rows.Add(item[0], item[1]);
                //dataGridView1.Rows[Index].Cells[1].Style.WrapMode = DataGridViewTriState.True;
                //dataGridView1.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill; 
                Index++;
            }
            dataGridView1.AutoResizeRows();
        }
        private void OpenFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "All localizations files|*.uasset;*.locres|Uasset File|*.uasset|Locres File|*.locres";
            ofd.Title = "Open localizations File";


            if (ofd.ShowDialog() == DialogResult.OK)
            {

                LoadFile(ofd.FileName);
            }
        }

        private async void LoadFile(string filePath)
        {

            dataGridView1.Rows.Clear();
            saveToolStripMenuItem.Enabled = false;
            exportAllTextToolStripMenuItem.Enabled = false;
            importAllTextToolStripMenuItem.Enabled = false;
            FilePath = "";
            this.Text = ToolName;
            try
            {
                state = new FrmState(this, "loading File", "loading File please wait...");
                this.BeginInvoke(new Action(() => state.ShowDialog()));


                if (filePath.ToLower().EndsWith(".locres"))
                {
                    locres = await Task.Run(() => new locres(filePath));

                    AddToDataView(locres.Strings);
                    saveToolStripMenuItem.Enabled = true;
                    exportAllTextToolStripMenuItem.Enabled = true;
                    importAllTextToolStripMenuItem.Enabled = true;
                    FilePath = filePath;
                    this.Text = ToolName + " - " + Path.GetFileName(FilePath);
                }
                else if (filePath.ToLower().EndsWith(".uasset"))
                {
                    Uasset = await Task.Run(() => new Uasset(filePath));
                    Uexp = await Task.Run(() => new Uexp(Uasset));

                    AddToDataView(Uexp.Strings);
                    saveToolStripMenuItem.Enabled = true;
                    exportAllTextToolStripMenuItem.Enabled = true;
                    importAllTextToolStripMenuItem.Enabled = true;
                    FilePath = filePath;
                    this.Text = ToolName + " - " + Path.GetFileName(FilePath);
                }
                state.Close();
            }
            catch (Exception ex)
            {
                state.Close();
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void exportAllTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string[] DataGridStrings = new string[dataGridView1.Rows.Count];
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                DataGridStrings[i] = dataGridView1.Rows[i].Cells[1].Value.ToString();
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
                    MessageBox.Show("Successful export!", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                    //for (int i = 0; i < dataGridView1.Rows.Count; i++)
                    //{
                    //    locres.Strings[i][1] = dataGridView1.Rows[i].Cells[1].Value != null ? dataGridView1.Rows[i].Cells[1].Value.ToString() : "";
                    //}
                    await Task.Run(() => locres.SaveFile(sfd.FileName));

                }
                else if (FilePath.ToLower().EndsWith(".uasset"))
                {

                    //for (int i = 0; i < dataGridView1.Rows.Count; i++)
                    //{
                    //    Uexp.Strings[i][1] = dataGridView1.Rows[i].Cells[1].Value != null ? dataGridView1.Rows[i].Cells[1].Value.ToString() : "";
                    //}
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
        }

        private void search_Click(object sender, EventArgs e)
        {
            SearchPanal.Visible = !SearchPanal.Visible;
            if (SearchPanal.Visible)
            {
                InputSearch.Focus();
            }
        }

        List<int> FindArray = new List<int>();
        int FindIndex = 0;
        string OldFind = "";
        private void Find_Click(object sender, EventArgs e)
        {
            FindArray.Clear();
            OldFind = InputSearch.Text;
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                if (dataGridView1.Rows[i].Cells[1].Value.ToString().ToLower().Contains(InputSearch.Text.ToLower()))
                {
                    FindArray.Add(i);
                }
            }

            if (FindArray.Count == 0)
            {
                MessageBox.Show($"can't find '{InputSearch.Text}'", "No results", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            dataGridView1.ClearSelection();
            dataGridView1.Rows[FindArray[0]].Selected = true;
            dataGridView1.FirstDisplayedScrollingRowIndex = FindArray[0];
            FindIndex = 0;
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
                dataGridView1.Rows[dataGridView1.SelectedCells[0].RowIndex].Cells[1].Value = Clipboard.GetText();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void InputSearch_KeyDown(object sender, KeyEventArgs e)
        {
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

        private void FrmMain_DragDrop(object sender, DragEventArgs e)
        {
            string[] array = (string[])e.Data.GetData(DataFormats.FileDrop);
            LoadFile(array[0]);
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView1.Created)
            {
                if (FilePath.ToLower().EndsWith(".locres"))
                {

                    locres.Strings[e.RowIndex][1] = dataGridView1.Rows[e.RowIndex].Cells[1].Value != null ? dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString() : "";
                    dataGridView1.Rows[e.RowIndex].Cells[1].Style.BackColor = System.Drawing.Color.FromArgb(255, 204, 153);
                }
                else if (FilePath.ToLower().EndsWith(".uasset"))
                {
                    Uexp.Strings[e.RowIndex][1] = dataGridView1.Rows[e.RowIndex].Cells[1].Value != null ? dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString() : "";
                    dataGridView1.Rows[e.RowIndex].Cells[1].Style.BackColor = System.Drawing.Color.FromArgb(255, 204, 153);
                }

            }
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
            if (dataGridView1.CancelEdit())
            {
                //if (e.KeyCode == Keys.V && e.Control)
                //{
                //    this.pasteToolStripMenuItem_Click(sender, e);
                //} else 
                if (e.KeyCode == Keys.L && e.Control && e.Alt)
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

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new FrmAbout(this).ShowDialog();
        }
    }
}
