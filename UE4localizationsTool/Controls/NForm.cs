using System.Drawing;
using System.Windows.Forms;

namespace UE4localizationsTool.Controls
{
    public class NForm : Form
    {
        protected override void CreateHandle()
        {
            base.CreateHandle();
            if (Properties.Settings.Default.DarkMode)
                DarkMode(this);
        }

#if DEBUG

        protected override void OnLoad(System.EventArgs e)
        {
            GetAllControlNames(this, Name);
        }

        private void GetAllControlNames(Control control, string parentName)
        {
            string controlName = parentName;

            if (control is TextBox || control is Label || control is Button || control is ComboBox
                || control is ListBox || control is DataGridView || control is CheckBox)
            {
                controlName += $".{control.Name}";
                //control.GetType().GetProperty("Text", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).SetValue(column, "amr");
                Console.WriteLine(controlName);

                if (control is DataGridView)
                {
                    foreach (DataGridViewColumn column in ((DataGridView)control).Columns)
                    {
                        Console.WriteLine(controlName + "." + column.Name);
                        //column.HeaderText = "amr";
                        // column.GetType().GetProperty("HeaderText", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).SetValue(column, "amr");
                    }
                }

            }
            else if (control is ToolStripMenuItem)
            {
                controlName += $".{control.Name}";
                //control.GetType().GetProperty("Text", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).SetValue(column, "amr");
                Console.WriteLine(controlName);
            }

            foreach (Control childControl in control.Controls)
            {
                GetAllControlNames(childControl, controlName);
            }

            if (control.ContextMenuStrip != null)
            {
                foreach (ToolStripMenuItem toolStripItem in control.ContextMenuStrip.Items)
                {
                    GetAllContextMenuNames(toolStripItem, controlName);
                }
            }

            if (control is MenuStrip)
            {
                foreach (var toolStripItem in ((MenuStrip)control).Items)
                {
                    if (toolStripItem is ToolStripMenuItem)
                        GetAllContextMenuNames((ToolStripMenuItem)toolStripItem, controlName);
                }
            }
        }

        private void GetAllContextMenuNames(ToolStripMenuItem control, string parentName)
        {
            string controlName = parentName;

            if (control is ToolStripMenuItem)
            {
                controlName += $".{(control as ToolStripMenuItem).Name}";
                //control.GetType().GetProperty("Text", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).SetValue(column, "amr");
                Console.WriteLine(controlName);
            }

            // Recursively call the method for child controls
            foreach (ToolStripItem childControl in control.DropDownItems)
            {
                if (childControl is ToolStripMenuItem)
                {
                    GetAllContextMenuNames(childControl as ToolStripMenuItem, controlName);
                }
            }
        }

#endif

        public void DarkMode(Control control)
        {
            if (control == null) return;

            if (!(control.BackColor.R == control.BackColor.G && control.BackColor.G == control.BackColor.B) ||
                    !(control.ForeColor.R == control.ForeColor.G && control.ForeColor.G == control.ForeColor.B))
            {
                return;
            }

            control.BackColor = Color.FromArgb(30, 30, 30);
            control.ForeColor = Color.White;

            if (control is ListView listView)
            {
                listView.OwnerDraw = true;
                listView.DrawItem += (c, e) => e.DrawDefault = true;
                listView.DrawSubItem += (c, e) => e.DrawDefault = true;
                listView.DrawColumnHeader += (c, e) =>
                {
                    e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(50, 50, 50)), e.Bounds);
                    e.Graphics.DrawString(e.Header.Text, e.Font, new SolidBrush(Color.White), e.Bounds);
                };
            }
            else if (control is DataGridView dataGridView)
            {
                dataGridView.BackgroundColor = Color.FromArgb(30, 30, 30);
                dataGridView.DefaultCellStyle.BackColor = Color.FromArgb(40, 40, 40);
                dataGridView.DefaultCellStyle.ForeColor = Color.White;
                dataGridView.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(50, 50, 50);
                dataGridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                dataGridView.EnableHeadersVisualStyles = false;
                dataGridView.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
                dataGridView.RowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(70, 70, 70);
                dataGridView.RowsDefaultCellStyle.SelectionForeColor = Color.White;
                DarkMode(dataGridView.ContextMenuStrip);
            }
            else if (control is Panel panel)
            {
                panel.BackColor = Color.FromArgb(30, 30, 30);
                foreach (Control childControl in panel.Controls)
                {
                    DarkMode(childControl);
                }
            }
            else if (control is TextBox textBox)
            {
                textBox.BackColor = Color.FromArgb(40, 40, 40);
                textBox.ForeColor = Color.White;
                textBox.BorderStyle = BorderStyle.FixedSingle;
            }
            else if (control is ComboBox comboBox)
            {
                comboBox.BackColor = Color.FromArgb(40, 40, 40);
                comboBox.ForeColor = Color.White;
            }
            else if (control is CheckBox || control is RadioButton)
            {
                control.ForeColor = Color.White;
            }
            else if (control is Button button)
            {
                button.BackColor = Color.FromArgb(40, 40, 40);
                button.ForeColor = Color.White;
                button.FlatStyle = FlatStyle.Flat;
                button.FlatAppearance.BorderColor = Color.FromArgb(60, 60, 60);
                button.FlatAppearance.MouseOverBackColor = Color.FromArgb(60, 60, 60);
            }
            else if (control is MenuStrip || control is ContextMenuStrip)
            {
                control.BackColor = Color.FromArgb(30, 30, 30);
                control.ForeColor = Color.White;
                if (control is MenuStrip menuStrip)
                {
                    menuStrip.RenderMode = ToolStripRenderMode.Professional;
                    menuStrip.Renderer = new DarkModeMenuStripRenderer(new CustomColorTable());
                }
                else if (control is ContextMenuStrip contextMenuStrip)
                {
                    contextMenuStrip.RenderMode = ToolStripRenderMode.Professional;
                    contextMenuStrip.Renderer = new DarkModeMenuStripRenderer(new CustomColorTable());
                }
            }
            else if (control is SplitContainer splitContainer)
            {
                splitContainer.BackColor = Color.FromArgb(30, 30, 30);
                splitContainer.ForeColor = Color.White;
                DarkMode(splitContainer.Panel1);
                DarkMode(splitContainer.Panel2);
            }
            else if (control is PictureBox || control is Label)
            {
                control.BackColor = Color.FromArgb(30, 30, 30);
                control.ForeColor = Color.White;
            }

            foreach (Control childControl in control.Controls)
            {
                DarkMode(childControl);
            }
        }

        public class DarkModeMenuStripRenderer : ToolStripProfessionalRenderer
        {
            public DarkModeMenuStripRenderer(ProfessionalColorTable colorTable)
                : base(colorTable)
            {
            }

            protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
            {
                e.TextColor = Color.White;
                base.OnRenderItemText(e);
            }
        }

        public class CustomColorTable : ProfessionalColorTable
        {
            public override Color MenuStripGradientBegin => Color.FromArgb(30, 30, 30);
            public override Color MenuStripGradientEnd => Color.FromArgb(30, 30, 30);
            public override Color MenuItemSelected => Color.FromArgb(63, 63, 70);
            public override Color MenuItemBorder => Color.FromArgb(63, 63, 70);
            public override Color MenuItemSelectedGradientBegin => Color.FromArgb(63, 63, 70);
            public override Color MenuItemSelectedGradientEnd => Color.FromArgb(63, 63, 70);
            public override Color MenuItemPressedGradientBegin => Color.FromArgb(63, 63, 70);
            public override Color MenuItemPressedGradientEnd => Color.FromArgb(63, 63, 70);
            public override Color MenuBorder => Color.FromArgb(63, 63, 70);
            public override Color ToolStripDropDownBackground => Color.FromArgb(30, 30, 30);
            public override Color ImageMarginGradientBegin => Color.FromArgb(30, 30, 30);
            public override Color ImageMarginGradientMiddle => Color.FromArgb(30, 30, 30);
            public override Color ImageMarginGradientEnd => Color.FromArgb(30, 30, 30);
            public override Color ToolStripBorder => Color.FromArgb(63, 63, 70);
            public override Color ToolStripContentPanelGradientBegin => Color.FromArgb(30, 30, 30);
            public override Color ToolStripContentPanelGradientEnd => Color.FromArgb(30, 30, 30);
            public override Color ToolStripGradientBegin => Color.FromArgb(30, 30, 30);
            public override Color ToolStripGradientMiddle => Color.FromArgb(30, 30, 30);
            public override Color ToolStripGradientEnd => Color.FromArgb(30, 30, 30);
        }
    }
}
