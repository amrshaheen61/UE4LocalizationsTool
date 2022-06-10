using System;
using System.Drawing;
using System.Windows.Forms;

namespace UE4localizationsTool
{
    public partial class FrmAbout : Form
    {
        public FrmAbout(Form form)
        {
            InitializeComponent();
            this.Location = new Point(form.Location.X + (form.Width - this.Width) / 2, form.Location.Y + (form.Height - this.Height) / 2);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
