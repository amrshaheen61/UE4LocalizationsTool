using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace UE4localizationsTool.Controls
{
    public partial class SearchBox : UserControl
    {

        private string ColumnName = "Text value";

        [Browsable(true)]
        public NDataGridView DataGridView { get; set; }
        int CurrentRowIndex = -1;
        int CurrentColumnIndex = -1;

        public SearchBox()
        {
            InitializeComponent();
            Hide();
        }
        public SearchBox(NDataGridView dataGrid)
        {
            DataGridView = dataGrid;
            InitializeComponent();
            Hide();
        }

        private void SearchHide_Click(object sender, System.EventArgs e)
        {
            Hide();
            listView1.Visible = false;
            label2.Text = string.Empty;
        }

        private bool IsMatch(string value)
        {
            string searchText = InputSearch.Text;

            if (string.IsNullOrWhiteSpace(searchText))
                return false;

            return value.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) > -1;
        }

        private bool IsMatch(string value, string match)
        {
            if (string.IsNullOrWhiteSpace(match))
                return false;

            return value.IndexOf(match, StringComparison.OrdinalIgnoreCase) > -1;
        }


        private DataGridViewCell FindCell(int startRowIndex, int endRowIndex, string columnName, string value, bool endToTop = false, bool returnNullIfNotFound = false)
        {
            int step = endToTop ? -1 : 1;

            for (int rowIndex = startRowIndex; endToTop ? rowIndex > endRowIndex : rowIndex < endRowIndex; rowIndex += step)
            {
                if (DataGridView.Columns.Contains(columnName))
                {
                    DataGridViewCell cell = DataGridView.Rows[rowIndex].Cells[columnName];
                    if (cell.Value != null && IsMatch(cell.Value.ToString(), value))
                    {
                        return cell;
                    }
                }
            }

            if (returnNullIfNotFound)
            {
                return null;
            }

            return FindCell(endToTop ? DataGridView.Rows.Count - 1 : 0, endToTop ? startRowIndex : endRowIndex, columnName, value, endToTop, true);
        }


        private void FindNext_Click(object sender, EventArgs e)
        {
            if (DataGridView.Rows.Count == 0)
            {
                MessageBox.Show("No data found.", "Search Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectedCell = DataGridView.SelectedCells.Count > 0 ? DataGridView.SelectedCells[0] : DataGridView.Rows[0].Cells[ColumnName];

            var cell = FindCell(selectedCell.RowIndex + 1, DataGridView.Rows.Count, ColumnName, InputSearch.Text);

            if (cell != null)
            {

                if (object.ReferenceEquals(cell, DataGridView.SelectedCells[0]))
                {
                    MessageBox.Show("No more matches found.", "Search Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                SelectCell(cell.RowIndex, cell.ColumnIndex);
            }
            else
            {
                Failedmessage();
            }

        }


        private void FindPrevious_Click(object sender, EventArgs e)
        {

            if (DataGridView.Rows.Count == 0)
            {
                MessageBox.Show("No data found.", "Search Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectedCell = DataGridView.SelectedCells.Count > 0 ? DataGridView.SelectedCells[0] : DataGridView.Rows[0].Cells[ColumnName];

            var cell = FindCell(selectedCell.RowIndex - 1, -1, ColumnName, InputSearch.Text, true);

            if (cell != null)
            {

                if (object.ReferenceEquals(cell, DataGridView.SelectedCells[0]))
                {
                    MessageBox.Show("No more matches found.", "Search Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                SelectCell(cell.RowIndex, cell.ColumnIndex);
            }
            else
            {
                Failedmessage();
            }
        }


        private void Failedmessage()
        {
            MessageBox.Show(
                text: $"The searched value '{InputSearch.Text}' not found.",
                caption: "Search Result",
                buttons: MessageBoxButtons.OK,
                icon: MessageBoxIcon.Warning
            );
        }

        private void SelectCell(int rowIndex, int colIndex)
        {
            DataGridView.ClearSelection();
            DataGridView.FirstDisplayedScrollingRowIndex = rowIndex;
            DataGridView.Rows[rowIndex].Cells[colIndex].Selected = true;

            CurrentRowIndex = rowIndex;
            CurrentColumnIndex = colIndex;
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

        public new void Show()
        {
            if (DataGridView.CurrentCell != null)
            {
                InputSearch.Text = DataGridView.CurrentCell.Value.ToString();
            }
            InputSearch.Focus();

            label2.Text = string.Empty;
            base.Show();
        }

        public void ShowReplacePanel()
        {
            Replacepanel.Visible = true;
            Show();
            txtReplace.Focus();
        }

        public int CountTotalMatches()
        {
            int totalMatches = 0;

            for (int rowIndex = 0; rowIndex < DataGridView.Rows.Count; rowIndex++)
            {
                for (int colIndex = 0; colIndex < DataGridView.Columns.Count; colIndex++)
                {
                    if (rowIndex >= 0 && rowIndex < DataGridView.Rows.Count && colIndex >= 0 && colIndex < DataGridView.Columns.Count)
                    {
                        DataGridViewCell cell = DataGridView.Rows[rowIndex].Cells[colIndex];
                        if (cell.Value != null && IsMatch(cell.Value.ToString()))
                        {
                            totalMatches++;
                        }
                    }
                }
            }

            return totalMatches;
        }

        string ButtonText;
        bool IsFindAll = false;

        private void FindAll_Click(object sender, EventArgs e)
        {
            Replacepanel.Visible = false;

            if (DataGridView.Rows.Count == 0)
            {
                MessageBox.Show("No data found.", "Search Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            listView1.Items.Clear();

            foreach (DataGridViewRow row in DataGridView.Rows)
            {
                if (DataGridView.Columns.Contains(ColumnName))
                {
                    if (row.Cells[ColumnName].Value != null && IsMatch(row.Cells[ColumnName].Value.ToString()))
                    {
                        ListViewItem item = new ListViewItem();
                        item.Text = (row.Index + 1).ToString();
                        item.SubItems.Add(row.Cells[ColumnName].Value.ToString());
                        item.Tag = row;
                        listView1.Items.Add(item);
                    }
                }
            }
            listView1.Visible = true;
            label2.Text = $"Total matches: {listView1.Items.Count}";

        }


        private void Replace_Click(object sender, EventArgs e)
        {

            void ReplaceCell(DataGridViewCell Cell)
            {


                DataGridView.SetValue(Cell, Regex.Replace(Cell.Value.ToString(), InputSearch.Text, txtReplace.Text, RegexOptions.IgnoreCase));

            }

            var selectedCell = DataGridView.SelectedCells.Count > 0 ? DataGridView.SelectedCells[0] : DataGridView.Rows[0].Cells[ColumnName];



            if (IsMatch(selectedCell.Value.ToString()))
            {
                ReplaceCell(selectedCell);
                return;
            }

            var cell = FindCell(selectedCell.RowIndex + 1, DataGridView.Rows.Count, ColumnName, InputSearch.Text);

            if (cell != null)
            {
                SelectCell(cell.RowIndex, cell.ColumnIndex);
                ReplaceCell(cell);
            }
            else
            {
                Failedmessage();
            }
        }

        private void label4_Click(object sender, EventArgs e)
        {
            Replacepanel.Visible = false;
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listView1.SelectedItems.Count == 0)
                return;
            var cell = (listView1.SelectedItems[0].Tag as DataGridViewRow).Cells[ColumnName];

            SelectCell(cell.RowIndex, cell.ColumnIndex);
        }

        private void ReplaceAll_Click(object sender, EventArgs e)
        {
            if (DataGridView.Rows.Count == 0)
            {
                MessageBox.Show("No data found.", "Search Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }


            int totalMatches = 0;
            foreach (DataGridViewRow row in DataGridView.Rows)
            {
                if (DataGridView.Columns.Contains(ColumnName))
                {
                    if (row.Cells[ColumnName].Value != null && IsMatch(row.Cells[ColumnName].Value.ToString()))
                    {
                        DataGridView.SetValue(row.Cells[ColumnName], Regex.Replace(row.Cells[ColumnName].Value.ToString(), InputSearch.Text, txtReplace.Text, RegexOptions.IgnoreCase));
                        totalMatches++;
                    }
                }
            }


            MessageBox.Show($"Total matches replaced: {totalMatches}", "Search Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
