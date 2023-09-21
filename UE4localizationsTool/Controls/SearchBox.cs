using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace UE4localizationsTool.Controls
{
    public partial class SearchBox : UserControl
    {
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
        }

        private bool IsMatch(string value)
        {
            string searchText = InputSearch.Text;

            if (string.IsNullOrWhiteSpace(searchText))
                return false;

            return value.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) > -1;
        }

        private void FindNext_Click(object sender, EventArgs e)
        {
            bool found = false;
         
            for (int rowIndex = CurrentRowIndex; rowIndex < DataGridView.Rows.Count; rowIndex++)
            {
                for (int colIndex = CurrentColumnIndex + 1; colIndex < DataGridView.Columns.Count; colIndex++)
                {
                    if (rowIndex >= 0 && rowIndex < DataGridView.Rows.Count && colIndex >= 0 && colIndex < DataGridView.Columns.Count)
                    {
                        DataGridViewCell cell = DataGridView.Rows[rowIndex].Cells[colIndex];
                        if (cell.Value != null && IsMatch(cell.Value.ToString()))
                        {
                            SelectCell(rowIndex, colIndex);
                            found = true;
                            break;
                        }
                    }
                }

                if (found)
                    break;

                
                CurrentColumnIndex = -1;
            }

            if (!found)
            {
                
                for (int rowIndex = 0; rowIndex < DataGridView.Rows.Count; rowIndex++)
                {
                    for (int colIndex = 0; colIndex < DataGridView.Columns.Count; colIndex++)
                    {
                        if (rowIndex >= 0 && rowIndex < DataGridView.Rows.Count && colIndex >= 0 && colIndex < DataGridView.Columns.Count)
                        {
                            DataGridViewCell cell = DataGridView.Rows[rowIndex].Cells[colIndex];
                            if (cell.Value != null && IsMatch(cell.Value.ToString()))
                            {
                                SelectCell(rowIndex, colIndex);
                                found = true;
                                break;
                            }
                        }
                    }

                    if (found)
                        break;
                }
            }
            if (!found)
            {
                Failedmessage();
            }
        }


        private void FindPrevious_Click(object sender, EventArgs e)
        {
            bool found = false;

            for (int rowIndex = CurrentRowIndex; rowIndex >= 0; rowIndex--)
            {
                for (int colIndex = CurrentColumnIndex - 1; colIndex >= 0; colIndex--)
                {
                    if (rowIndex >= 0 && rowIndex < DataGridView.Rows.Count && colIndex >= 0 && colIndex < DataGridView.Columns.Count)
                    {
                        DataGridViewCell cell = DataGridView.Rows[rowIndex].Cells[colIndex];
                        if (cell.Value != null && IsMatch(cell.Value.ToString()))
                        {
                            SelectCell(rowIndex, colIndex);
                            found = true;
                            break;
                        }
                    }
                }

                if (found)
                    break;

                
                CurrentColumnIndex = DataGridView.Columns.Count;
            }

            if (!found)
            {
                for (int rowIndex = DataGridView.Rows.Count - 1; rowIndex >= 0; rowIndex--)
                {
                    for (int colIndex = DataGridView.Columns.Count - 1; colIndex >= 0; colIndex--)
                    {
                        if (rowIndex >= 0 && rowIndex < DataGridView.Rows.Count && colIndex >= 0 && colIndex < DataGridView.Columns.Count)
                        {
                            DataGridViewCell cell = DataGridView.Rows[rowIndex].Cells[colIndex];
                            if (cell.Value != null && IsMatch(cell.Value.ToString()))
                            {
                                SelectCell(rowIndex, colIndex);
                                found = true;
                                break;
                            }
                        }
                    }

                    if (found)
                        break;
                }
            }

            if (!found)
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
            if (DataGridView.CurrentCell!=null)
            {
                InputSearch.Text = DataGridView.CurrentCell.Value.ToString();
            }
            InputSearch.Focus();
            base.Show();
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

    }
}
