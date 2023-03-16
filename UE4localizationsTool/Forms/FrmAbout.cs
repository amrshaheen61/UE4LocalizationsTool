using System;
using System.Drawing;
using System.Windows.Forms;
using UE4localizationsTool.Controls;

namespace UE4localizationsTool
{
    public partial class FrmAbout : NForm
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
