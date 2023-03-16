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
                DarkMode();
        }


        public void DarkMode()
        {

            this.BackColor = Color.FromArgb(30, 30, 30);
            this.ForeColor = Color.White;

            foreach (Control control in this.Controls)
            {
                DarkMode(control);
            }
        }

        private void DarkMode(Control control)
        {

            if (!(control.BackColor.R == control.BackColor.G && control.BackColor.G == control.BackColor.B))
            {
                return;
            }

            control.BackColor = Color.FromArgb(30, 30, 30);
            control.ForeColor = Color.White;

            if (control is GroupBox)
            {
                ((GroupBox)control).ForeColor = Color.White;
            }
            if (control is ListView)
            {
                ((ListView)control).BackColor = Color.FromArgb(30, 30, 30);
                ((ListView)control).ForeColor = Color.White;
                ((ListView)control).OwnerDraw = true;
                ((ListView)control).DrawItem += (c, e) =>
                {
                    e.DrawDefault = true;
                };

                ((ListView)control).DrawSubItem += (c, e) =>
                {
                    e.DrawDefault = true;
                };

                ((ListView)control).DrawColumnHeader += (c, e) =>
                {
                    e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(50, 50, 50)), e.Bounds);
                    e.Graphics.DrawString(e.Header.Text, e.Font, new SolidBrush(Color.White), e.Bounds);
                };

            }

            if (control is TreeView)
            {
                ((TreeView)control).BackColor = Color.FromArgb(30, 30, 30);
                ((TreeView)control).ForeColor = Color.White;
                ((TreeView)control).LineColor = Color.White;


            }

            if (control is DataGridView)
            {
                ((DataGridView)control).BackgroundColor = Color.FromArgb(30, 30, 30);
                ((DataGridView)control).DefaultCellStyle.BackColor = Color.FromArgb(40, 40, 40);
                ((DataGridView)control).DefaultCellStyle.ForeColor = Color.White;
                ((DataGridView)control).ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(50, 50, 50);
                ((DataGridView)control).ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                ((DataGridView)control).EnableHeadersVisualStyles = false;

                ((DataGridView)control).ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
                ((DataGridView)control).RowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(70, 70, 70);
                ((DataGridView)control).RowsDefaultCellStyle.SelectionForeColor = Color.White;
            }
            if (control is Panel)
            {
                ((Panel)control).BackColor = Color.FromArgb(30, 30, 30);

                foreach (Control controls in ((Panel)control).Controls)
                {
                    DarkMode(controls);
                }
            }
            if (control is TextBox)
            {
                ((TextBox)control).BackColor = Color.FromArgb(40, 40, 40);
                ((TextBox)control).ForeColor = Color.White;
                ((TextBox)control).BorderStyle = BorderStyle.FixedSingle;
            }
            if (control is ComboBox)
            {
                ((ComboBox)control).BackColor = Color.FromArgb(40, 40, 40);
                ((ComboBox)control).ForeColor = Color.White;
            }
            if (control is CheckBox)
            {
                ((CheckBox)control).ForeColor = Color.White;
            }
            if (control is RadioButton)
            {
                ((RadioButton)control).ForeColor = Color.White;
            }
            if (control is Button)
            {
                ((Button)control).BackColor = Color.FromArgb(40, 40, 40);
                ((Button)control).ForeColor = Color.White;
                ((Button)control).FlatStyle = FlatStyle.Flat;
                ((Button)control).FlatAppearance.BorderSize = 1;
                ((Button)control).FlatAppearance.BorderColor = Color.FromArgb(60, 60, 60);
                ((Button)control).FlatAppearance.MouseOverBackColor = Color.FromArgb(60, 60, 60);

            }

            if (control is MenuStrip)
            {
                ((MenuStrip)control).BackColor = Color.FromArgb(30, 30, 30);
                ((MenuStrip)control).ForeColor = Color.White;

                ((MenuStrip)control).RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
                ((MenuStrip)control).Renderer = new DarkModeMenuStripRenderer(new CustomColorTable());
            }

            if (control is SplitContainer)
            {
                ((SplitContainer)control).BackColor = Color.FromArgb(30, 30, 30);
                ((SplitContainer)control).ForeColor = Color.White;


                DarkMode(((SplitContainer)control).Panel1);
                DarkMode(((SplitContainer)control).Panel2);
            }

            if (control is PictureBox)
            {
                ((PictureBox)control).BackColor = Color.FromArgb(30, 30, 30);
                ((PictureBox)control).ForeColor = Color.White;
            }


            if (control is Label)
            {
                ((Label)control).BackColor = Color.FromArgb(30, 30, 30);
                ((Label)control).ForeColor = Color.White;
            }



            foreach (Control c in control.Controls)
            {
                DarkMode(c);
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
            public override Color MenuStripGradientBegin
            {
                get { return Color.FromArgb(30, 30, 30); }
            }

            public override Color MenuStripGradientEnd
            {
                get { return Color.FromArgb(30, 30, 30); }
            }

            public override Color MenuItemSelected
            {
                get { return Color.FromArgb(63, 63, 70); }
            }

            public override Color MenuItemBorder
            {
                get { return Color.FromArgb(63, 63, 70); }
            }

            public override Color MenuItemSelectedGradientBegin
            {
                get { return Color.FromArgb(63, 63, 70); }
            }

            public override Color MenuItemSelectedGradientEnd
            {
                get { return Color.FromArgb(63, 63, 70); }
            }

            public override Color MenuItemPressedGradientBegin
            {
                get { return Color.FromArgb(63, 63, 70); }
            }

            public override Color MenuItemPressedGradientEnd
            {
                get { return Color.FromArgb(63, 63, 70); }
            }

            public override Color MenuBorder
            {
                get { return Color.FromArgb(63, 63, 70); }
            }

            public override Color ToolStripDropDownBackground
            {
                get { return Color.FromArgb(30, 30, 30); }
            }

            public override Color ImageMarginGradientBegin
            {
                get { return Color.FromArgb(30, 30, 30); }
            }

            public override Color ImageMarginGradientMiddle
            {
                get { return Color.FromArgb(30, 30, 30); }
            }


            public override Color ImageMarginGradientEnd
            {
                get { return Color.FromArgb(30, 30, 30); }
            }

            public override Color ToolStripBorder
            {
                get { return Color.FromArgb(63, 63, 70); }
            }

            public override Color ToolStripContentPanelGradientBegin
            {
                get { return Color.FromArgb(30, 30, 30); }
            }

            public override Color ToolStripContentPanelGradientEnd
            {
                get { return Color.FromArgb(30, 30, 30); }
            }

            public override Color ToolStripGradientBegin
            {
                get { return Color.FromArgb(30, 30, 30); }
            }

            public override Color ToolStripGradientMiddle
            {
                get { return Color.FromArgb(30, 30, 30); }
            }

            public override Color ToolStripGradientEnd
            {
                get { return Color.FromArgb(30, 30, 30); }
            }


        }


    }
}
