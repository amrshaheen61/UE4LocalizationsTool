using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using UE4localizationsTool;

public class NDataGridView : DataGridView
{
    struct DataRow
    {
        public object StringValue;
        public DataGridViewCell cell;
        public DataGridViewCellStyle style;
    }

    private ContextMenuStrip contextMenuStrip;
    private Stack<DataRow> BackupDataUndo;
    private Stack<DataRow> BackupDataRedo;
    private DataTable TempDataSource;

    [Browsable(true)]
    public event EventHandler FilterApplied;
    [Browsable(true)]
    public event EventHandler FilterCleared;

    [Browsable(true)]
    public event EventHandler RowCountChanged;


    private bool _isFiltering = false;

    [Browsable(false)]
    public bool IsFilterApplied
    {
        get
        {
            return _isFiltering;
        }
        set
        {
            if (value)
            {
                OnFilterApplied();
            }
            else
            {
                OnFilterCleared();
            }
            _isFiltering = value;
        }
    }

    protected virtual void OnFilterApplied()
    {
        FilterApplied?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void OnFilterCleared()
    {
        FilterCleared?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void OnRowCountChanged()
    {
        RowCountChanged?.Invoke(this, EventArgs.Empty);
    }


    [Browsable(false)]
    public new int FirstDisplayedScrollingRowIndex
    {
        get
        {
            return base.FirstDisplayedScrollingRowIndex;
        }
        set
        {
            if (value < 0 || value >= RowCount)
            {
                return;
            }

            int firstVisibleRowIndex = FirstDisplayedScrollingRowIndex;

            if (value != firstVisibleRowIndex)
            {
                int newFirstDisplayedRowIndex = Math.Max(0, value - DisplayedRowCount(true) / 2);
                base.FirstDisplayedScrollingRowIndex = newFirstDisplayedRowIndex;
            }
        }
    }


    protected override void OnDataSourceChanged(EventArgs e)
    {
        base.OnDataSourceChanged(e);
        ClearUndoRedoStacks();
        OnRowCountChanged();
    }

    protected override void OnRowsRemoved(DataGridViewRowsRemovedEventArgs e)
    {
        base.OnRowsRemoved(e);
        if (Rows.Count == 0)
            ClearUndoRedoStacks();

        OnRowCountChanged();
    }

    protected override void OnRowsAdded(DataGridViewRowsAddedEventArgs e)
    {
        base.OnRowsAdded(e);

        OnRowCountChanged();
    }

    protected void ClearUndoRedoStacks()
    {
        BackupDataUndo.Clear();
        BackupDataRedo.Clear();
        IsFilterApplied = false;
    }




    protected override void OnCellValidating(DataGridViewCellValidatingEventArgs e)
    {
        base.OnCellValidating(e);

        if (!object.Equals(e.FormattedValue, Rows[e.RowIndex].Cells[e.ColumnIndex].Value))
        {
            BackupDataUndo.Push(new DataRow() { cell = Rows[e.RowIndex].Cells[e.ColumnIndex], StringValue = Rows[e.RowIndex].Cells[e.ColumnIndex].Value, style = Rows[e.RowIndex].Cells[e.ColumnIndex].Style.Clone() });
            BackupDataRedo.Clear();
            UpdateBackColor(Rows[e.RowIndex].Cells[e.ColumnIndex]);
        }
    }


    public void SetValue(DataGridViewCell cell, object Value)
    {
        if (!object.Equals(Value, cell.Value))
        {
            BackupDataUndo.Push(new DataRow() { cell = cell, StringValue = cell.Value, style = cell.Style.Clone() });
            BackupDataRedo.Clear();
            UpdateBackColor(cell);
            cell.Value = Value;
        }
    }

    public void UpdateBackColor(DataGridViewCell cell)
    {
        cell.Style.BackColor = System.Drawing.Color.FromArgb(255, 204, 153);
        cell.Style.ForeColor = Color.Black;

    }

    private void UpdateCellValueAndStyle(Stack<DataRow> PopStack, Stack<DataRow> PushStack)
    {
        var data = PopStack.Pop();
        if (data.cell.RowIndex == -1 || data.cell.ColumnIndex == -1)
        {
            return;
        }

        var temp = data.cell.Value;
        var tempstyle = data.cell.Style.Clone();
        data.cell.Value = data.StringValue;
        data.cell.Style = data.style;

        data.StringValue = temp;
        data.style = tempstyle;

        if (data.cell.ColumnIndex != -1 && data.cell.RowIndex != -1)
        {
            ClearSelection();
            data.cell.Selected = true;
            FirstDisplayedScrollingRowIndex = data.cell.RowIndex;
        }

        PushStack.Push(data);
    }

    public void Undo()
    {
        if (BackupDataUndo.Count > 0)
        {
            UpdateCellValueAndStyle(BackupDataUndo, BackupDataRedo);
        }
    }

    public void Redo()
    {
        if (BackupDataRedo.Count > 0)
        {
            UpdateCellValueAndStyle(BackupDataRedo, BackupDataUndo);
        }
    }




    public NDataGridView()
    {
        InitializeContextMenu();
        BackupDataUndo = new Stack<DataRow>();
        BackupDataRedo = new Stack<DataRow>();
    }

    private void InitializeContextMenu()
    {
        contextMenuStrip = new ContextMenuStrip();
        ToolStripMenuItem copyMenuItem = new ToolStripMenuItem("Copy");
        copyMenuItem.ShortcutKeyDisplayString = "Ctrl+C";
        copyMenuItem.Click += CopyMenuItem_Click;
        contextMenuStrip.Items.Add(copyMenuItem);


        ToolStripMenuItem pasteMenuItem = new ToolStripMenuItem("Paste");
        pasteMenuItem.ShortcutKeyDisplayString = "Ctrl+V";
        pasteMenuItem.Click += PasteMenuItem_Click;
        contextMenuStrip.Items.Add(pasteMenuItem);

        this.ContextMenuStrip = contextMenuStrip;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {

        if (e.Alt && e.KeyCode == Keys.E)
        {
            throw null;
        }

        else
        {
            base.OnKeyDown(e);
        }
    }


    protected override void OnKeyPress(KeyPressEventArgs e)
    {
        base.OnKeyPress(e);
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {

        if ((keyData & Keys.Control) == Keys.Control)
        {
            if (keyData == (Keys.Control | Keys.Z))
            {
                Undo();

            }
            else if (keyData == (Keys.Control | Keys.Y) || keyData == (Keys.Control | Keys.Shift | Keys.Z))
            {
                Redo();

            }
            else if (keyData == (Keys.Control | Keys.C))
            {
                Copy();
            }
            else if (keyData == (Keys.Control | Keys.V))
            {
                Paste();

            }
            else if (keyData == (Keys.Control | Keys.Alt | Keys.L))
            {
                RightToLeft = RightToLeft.No;

            }
            else if (keyData == (Keys.Control | Keys.Alt | Keys.R))
            {
                RightToLeft = RightToLeft.Yes;

            }

        }

        return base.ProcessCmdKey(ref msg, keyData);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Right)
        {
            var clickedCell = HitTest(e.X, e.Y);
            if (clickedCell.RowIndex >= 0 && clickedCell.ColumnIndex >= 0 && Rows.Count > 0)
            {
                CurrentCell = Rows[clickedCell.RowIndex].Cells[clickedCell.ColumnIndex];
                ContextMenuStrip.Items[1].Enabled = true;
                if (CurrentCell.ReadOnly)
                {
                    ContextMenuStrip.Items[1].Enabled = false;
                }
            }
        }

        base.OnMouseDown(e);
    }

    private void CopyMenuItem_Click(object sender, EventArgs e)
    {
        Copy();
    }


    private void PasteMenuItem_Click(object sender, EventArgs e)
    {
        Paste();
    }

    public void Copy()
    {
        if (SelectedCells.Count == 0 || CurrentCell == null)
        {
            MessageBox.Show("No cell is selected for copying.", "Copy Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (SelectedCells.Count > 0 && CurrentCell != null && !CurrentCell.IsInEditMode)
        {
            try
            {
                Clipboard.SetDataObject(GetClipboardContent());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Copy operation failed: {ex.Message}", "Copy Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    public void Paste()
    {
        var str = Clipboard.GetText();
        if (string.IsNullOrEmpty(str))
        {
            MessageBox.Show("The clipboard is empty. Nothing to paste.", "Paste Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }


        if (CurrentCell == null || CurrentCell.IsInEditMode)
        {
            return;
        }

        if (SelectedCells.Count == 0)
        {
            MessageBox.Show("Please select a single cell to paste into.", "Paste Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (SelectedCells.Count > 1)
        {
            MessageBox.Show("Cannot paste into a multi-select range. Please select a single cell.", "Paste Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (CurrentCell.ReadOnly)
        {
            MessageBox.Show("This cell is read-only. Paste failed.", "Paste Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        string[] lines = str.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);

        int rowIndex = CurrentCell.RowIndex;
        int cellIndex = CurrentCell.ColumnIndex;

        ClearSelection();
        int selectedCellIndex = 0;
        for (int i = rowIndex; (i < rowIndex + lines.Length) && i < Rows.Count; i++)
        {
            var cell = Rows[i].Cells[cellIndex];
            BackupDataUndo.Push(new DataRow()
            {
                cell = cell,
                StringValue = cell.Value,
                style = cell.Style.Clone()
            });

            cell.Value = lines[selectedCellIndex++];
            cell.Selected = true;
            UpdateBackColor(cell);
        }
    }

    public void Filter()
    {
        var filterForm = new FrmFilter(this);
        if (filterForm.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        DataTable datatable;
        datatable = (DataTable)DataSource;
        if (!IsFilterApplied)
        {
            TempDataSource = datatable.Copy();
        }

        datatable.Rows.Clear();

        foreach (System.Data.DataRow row in TempDataSource.Rows)
        {

            bool shouldShow = ShouldShowRow(row, filterForm);
            if (shouldShow)
            {
                if (!filterForm.ReverseMode)
                {
                    datatable.ImportRow(row);
                }
            }
            else if (filterForm.ReverseMode)
            {
                datatable.ImportRow(row);
            }
        }
        DataSource = datatable;
        IsFilterApplied = true;

    }

    private bool ShouldShowRow(System.Data.DataRow row, FrmFilter filterForm)
    {
        foreach (string filterValue in filterForm.ArrayValues)
        {
            string cellValue = row[filterForm.ColumnName]?.ToString() ?? string.Empty;

            if (filterForm.UseMatching)
            {
                if (filterForm.RegularExpression)
                {
                    try
                    {
                        if (Regex.IsMatch(cellValue, filterValue))
                        {
                            return true;
                        }
                    }
                    catch { }
                }
                else
                {
                    if (string.Equals(cellValue, filterValue, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            else
            {
                if (filterForm.RegularExpression)
                {
                    try
                    {
                        if (Regex.IsMatch(cellValue, filterValue, RegexOptions.IgnoreCase))
                        {
                            return true;
                        }
                    }
                    catch { }
                }
                else
                {
                    if (cellValue.IndexOf(filterValue, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }





    public void ClearFilter()
    {
        if (IsFilterApplied)
        {
            DataSource = TempDataSource.Copy();
            TempDataSource = null;
            IsFilterApplied = false;
        }
    }

}
