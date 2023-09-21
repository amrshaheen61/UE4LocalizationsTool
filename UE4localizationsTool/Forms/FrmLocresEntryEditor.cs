using System;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using UE4localizationsTool.Controls;
using UE4localizationsTool.Core.Hash;
using UE4localizationsTool.Core.locres;

namespace UE4localizationsTool.Forms
{
    public partial class FrmLocresEntryEditor : NForm
    {
        public string NameSpace { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public LocresFile asset { get; set; }

        public FrmLocresEntryEditor()
        {
            InitializeComponent();
        }


        public FrmLocresEntryEditor(NDataGridView gridView, LocresFile asset)
        {
            InitializeComponent();
            Location = new Point(
                gridView.PointToScreen(Point.Empty).X + (gridView.Width - this.Width) / 2,
                gridView.PointToScreen(Point.Empty).Y + (gridView.Height - this.Height) / 2
            );

            this.asset = asset;
            var items = gridView.CurrentCell.OwningRow.Cells["Name"].Value.ToString().Split(new string[] { "::" }, StringSplitOptions.None);

            if (items.Length == 2)
            {
                NameSpace = items[0];
                Key = items[1];
                Value = gridView.CurrentCell.OwningRow.Cells["Text value"].Value.ToString();
            }
            else
            {
                Key = items[0];
                Value = gridView.CurrentCell.OwningRow.Cells["Text value"].Value.ToString();
            }
            Print();
        }

        public FrmLocresEntryEditor(Form form)
        {
            InitializeComponent();
            this.Location = new Point(form.Location.X + (form.Width - this.Width) / 2, form.Location.Y + (form.Height - this.Height) / 2);
            Apply.Text = "Add";
        }

        private void Print()
        {
            txtNameSpace.Text = NameSpace;
            txtKey.Text = Key;
            txtValue.Text = Value;
        }

        public void AddRow(NDataGridView gridView)
        {
            DataTable dt = (DataTable)gridView.DataSource;

            string RowName = GetName();

            //bool rowExists = false;
            //foreach (DataRow row in dt.Rows)
            //{
            //    if (string.Equals(row["Name"].ToString(), RowName, StringComparison.OrdinalIgnoreCase))
            //    {
            //        rowExists = true;
            //        break;
            //    }
            //}

            //if (rowExists)
            //{
            //    throw new Exception("this NameSpace and Key already exists in the table.");
            //}

            dt.Rows.Add(RowName, Value);

        }

        private string GetName()
        {
            string RowName;
            if (!string.IsNullOrEmpty(NameSpace))
                RowName = NameSpace + "::" + Key;
            else
                RowName = Key;
            return RowName;
        }

        public void EditRow(NDataGridView DGV)
        {
            DGV.SetValue(DGV.CurrentCell.OwningRow.Cells["Name"], GetName());
            DGV.SetValue(DGV.CurrentCell.OwningRow.Cells["Text value"], txtValue.Text);
        }

        private void txtNameSpace_TextChanged(object sender, EventArgs e)
        {
            NameSpace = txtNameSpace.Text;
        }

        private void txtKey_TextChanged(object sender, EventArgs e)
        {
            Key = txtKey.Text;
        }

        private void txtValue_TextChanged(object sender, EventArgs e)
        {
            Value = AssetParser.AssetHelper.ReplaceBreaklines(txtValue.Text);
        }

    }
}
